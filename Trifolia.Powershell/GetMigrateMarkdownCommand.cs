using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Trifolia.DB;
using Trifolia.Shared.Plugins;
using Trifolia.Shared;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Text.RegularExpressions;

namespace Trifolia.Powershell
{
    public class MigrateMarkdownLog
    {
        public int ImplementationGuideId { get; set; }
        public int? SectionId { get; set; }
        public int? TemplateId { get; set; }
        public int? ConstraintId { get; set; }
        public string Property { get; set; }
        public string Original { get; set; }
        public string New { get; set; }
    }

    /// <summary>
    /// This PowerShell command should only be used once, after upgrading to Trifolia to 2.5.2+ to
    /// migrate the WIKI Markup and XHTML fields to be Markdown fields. If run twice, it may screw up
    /// the markdown.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "MigrateMarkdown")]
    public class GetMigrateMarkdownCommand : BaseCommand
    {
        private List<MigrateMarkdownLog> logs;
        private const string CleanHtmlFormat = "<root xmlns:o=\"urn:msxml\" xmlns:v=\"urn:msxml\" xmlns:w=\"urn:msxml\">{0}</root>";

        /*
        [Parameter(
            Mandatory = true,
            HelpMessage = "A filename that the results should be stored to (in CSV format)"
        )]
        public string Output { get; set; }
        */

        private string CleanHtml(string html)
        {
            XDocument doc = null;

            try
            {
                doc = XDocument.Parse(string.Format(CleanHtmlFormat, html));
            }
            catch (XmlException xex)
            {
                try
                {
                    doc = XDocument.Parse(string.Format(CleanHtmlFormat, html + "</div>"));
                }
                catch
                {
                    try
                    {
                        doc = XDocument.Parse(string.Format(CleanHtmlFormat, html + "</div></div>"));
                    }
                    catch
                    {
                        return html;
                    }
                }
            }
            catch (Exception)
            {
                return html;
            }

            // Remove empty <ul> elements
            doc.Descendants("ul")
                .Where(y => y.Nodes().Count() == 0)
                .Remove();

            using (var reader = doc.FirstNode.CreateReader())
            {
                reader.MoveToContent();
                string innerXml = reader.ReadInnerXml();
                return innerXml;
            }
        }

        private void ReplaceSpans(XDocument doc)
        {
            var spans = doc.Descendants("span").ToArray();
            foreach (var span in spans)
            {
                span.ReplaceWith(span.Nodes().ToArray());
            }
        }

        private string CleanMarkdown(string markdown)
        {
            try
            {
                var doc = XDocument.Parse(string.Format(CleanHtmlFormat, markdown));
                XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
                nsManager.AddNamespace("o", "urn:msxml");
                var tableElements = doc.XPathSelectElements("//table");

                foreach (var tableElement in tableElements)
                {
                    string tableMarkdown = "\n\n";

                    var headerColumns = tableElement.XPathSelectElements("thead/tr/td");
                    int headerColumnCount = headerColumns.Count();
                    IEnumerable<XElement> rows = null;

                    if (headerColumnCount > 0)
                    {
                        rows = tableElement.XPathSelectElements("tbody/tr");
                        tableMarkdown += "|";

                        foreach (var headerColumn in headerColumns)
                        {
                            tableMarkdown += " " + headerColumn.Value + " |";
                        }

                        tableMarkdown += "\n";
                    }
                    else
                    {
                        rows = tableElement.XPathSelectElements("//tr");
                        headerColumnCount = tableElement.XPathSelectElements("//tr[1]/td").Count();
                        tableMarkdown += "|";

                        for (int i = 0; i < headerColumnCount; i++)
                        {
                            tableMarkdown += " |";
                        }

                        tableMarkdown += "\n";
                    }

                    // Header separator
                    tableMarkdown += "|";

                    for (int i = 0; i < headerColumnCount; i++)
                    {
                        tableMarkdown += " ----- |";
                    }

                    tableMarkdown += "\n";

                    foreach (var row in rows)
                    {
                        tableMarkdown += "|";

                        foreach (var cell in row.XPathSelectElements("td"))
                        {
                            tableMarkdown += " " + cell.Value.Trim().Replace("\r", "").Replace("\n\n", "\n").Replace("\n", "<br/>") + " |";
                        }

                        tableMarkdown += "\n";
                    }

                    tableElement.ReplaceWith(tableMarkdown);
                }

                this.ReplaceSpans(doc);
                this.ReplaceSpans(doc);
                this.ReplaceSpans(doc);
                this.ReplaceSpans(doc);

                var msXmlParagraphs = doc.XPathSelectElements("//o:p", nsManager);

                foreach (var msXmlParagraph in msXmlParagraphs)
                {
                    msXmlParagraph.Remove();
                }

                using (var reader = doc.FirstNode.CreateReader())
                {
                    reader.MoveToContent();
                    string innerXml = reader.ReadInnerXml()
                        .Replace("\n\n\n\n", "\n\n")
                        .Replace("\n\n\n", "\n\n")
                        .Replace("\n\t", "\n")
                        .Replace("*   ", "* ")
                        .Replace(". **", ".** ")
                        .Replace(". **", ".** ");
                    return innerXml;
                }
            }
            catch (Exception ex)
            {
                this.WriteVerbose("Could not clean up markdown");
                return markdown;
            }
        }

        Regex noTitleUrlRegex = new Regex(@"\[URL:(.*?\|)?(ftp:\/\/.*?)\]");
        Regex longUrlDisplayRegex = new Regex(@"\[URL:(.{45,120})\|(http[s]?:\/\/.*?)\]");

        private string ConvertWikiToHtml(string wiki)
        {
            MatchCollection noTitleUrlMatches = noTitleUrlRegex.Matches(wiki);
            MatchCollection longUrlDisplayMatches = longUrlDisplayRegex.Matches(wiki);
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            
            // Excessivly long urls in wiki cannot be converted due to a bug in WikiNetParser
            // Replace the long display in the url with a short generated one, and then replace
            // it back in the html once the WikiNetParser is done converting to html
            foreach (Match longUrlDisplayMatch in longUrlDisplayMatches)
            {
                string replacement = Guid.NewGuid().ToString();
                replacements[replacement] = longUrlDisplayMatch.Groups[1].Value;
                wiki = wiki.Replace(
                    longUrlDisplayMatch.Captures[0].Value,
                    @"[URL:" + replacement + "|" + longUrlDisplayMatch.Groups[2].Value + "]");
            }

            // WikiNetParser expects URLs to be http|https, and errors if the URL is an ftp address
            // Change the url so that it represents an http address, and change it back to ftp 
            // after WikiNetParser has converted it to html.
            foreach (Match match in noTitleUrlMatches)
            {
                wiki = wiki.Replace(
                    match.Captures[0].Value,
                    @"[URL:" + match.Groups[2].Value + "|" + match.Groups[2].Value.Replace("ftp://", "http://") + "]");
            }

            string html = WikiNetParser.WikiProvider.ConvertToHtml(wiki);

            foreach (var replacement in replacements.Keys)
            {
                html = html.Replace(replacement, replacements[replacement]);
            }

            foreach (Match match in noTitleUrlMatches)
            {
                html = html.Replace(
                    "href=\"" + match.Groups[2].Value.Replace("ftp://", "http://") + "\"",
                    "href=\"" + match.Groups[2].Value + "\"");
                html = html.Replace(
                    "title=\"" + match.Groups[2].Value.Replace("ftp://", "http://") + "\"",
                    "title=\"" + match.Groups[2].Value + "\"");
            }

            return html;
        }

        private string ConvertWikiToMarkdown(string wiki, int implementationGuideId, int templateId, string property, int? constraintId = null)
        {
            var converter = new Html2Markdown.Converter();
            string markdown = wiki;
            string cleanWiki = wiki
                .Trim()
                .Replace(" [1..0]", " [[]1..0]")
                .Replace(" [0..1]", " [[]0..1]")
                .Replace(" [0..*]", " [[]0..*]")
                .Replace(" [1..1]", " [[]1..1]")
                .Replace(" [1..*]", " [[]1..*]")
                .Replace(" [urn:", " [[]urn:");

            if (string.IsNullOrEmpty(cleanWiki))
                return string.Empty;

            try
            {
                var html = this.ConvertWikiToHtml(cleanWiki);
                html = this.CleanHtml(html);

                if (string.IsNullOrEmpty(html))
                    throw new Exception("Error converting WIKI content to HTML");

                markdown = converter.Convert(html);
                markdown = this.CleanMarkdown(markdown);
            }
            catch
            {
                this.WriteVerbose("Could not convert from WIKI to HTML due to syntax error. Converting original content directly to MarkDown.");
                markdown = converter.Convert(wiki);
            }

            if (markdown != wiki)
                this.logs.Add(new MigrateMarkdownLog()
                {
                    TemplateId = templateId,
                    ConstraintId = constraintId,
                    Property = property,
                    Original = wiki,
                    New = markdown
                });

            return markdown;
        }

        public string ConvertHtmlToMarkdown(string html, int implementationGuideId, int sectionId, string property)
        {
            string cleanHtml = html
                .Replace("<div>", "")
                .Replace("</div>", "")
                .Replace("<br>", "")
                .Replace("<br />", "")
                .Replace("<br/>", "");
            cleanHtml = this.CleanHtml(cleanHtml);

            var converter = new Html2Markdown.Converter();
            string markdown = converter.Convert(cleanHtml);
            markdown = this.CleanMarkdown(markdown);

            if (html != markdown)
                this.logs.Add(new MigrateMarkdownLog()
                {
                    ImplementationGuideId = implementationGuideId,
                    SectionId = sectionId,
                    Property = property,
                    Original = html,
                    New = markdown
                });

            return !string.IsNullOrEmpty(markdown) ? markdown : html;
        }

        /// <summary>
        /// Converts the following fields to MarkDown format:
        /// - Template.Description
        /// - TemplateConstraint.PrimitiveText
        /// </summary>
        protected override void ProcessRecord()
        {
            this.logs = new List<MigrateMarkdownLog>();

            foreach (var implementationGuide in this.tdb.ImplementationGuides)
            {
                foreach (var section in implementationGuide.Sections.Where(y => !string.IsNullOrEmpty(y.Content)))
                {
                    section.Content = ConvertHtmlToMarkdown(section.Content, implementationGuide.Id, section.Id, "Content");
                    this.WriteVerbose("Updated implementation guide " + implementationGuide.Id + "'s section " + section.Id + " to be markdown");
                }
            }
            
            foreach (var template in this.tdb.Templates)
            {
                if (!string.IsNullOrEmpty(template.Description))
                {
                    template.Description = this.ConvertWikiToMarkdown(template.Description, template.OwningImplementationGuideId, template.Id, "Description");
                    this.WriteVerbose("Updated template " + template.Id + "'s description to be markdown");
                }

                if (!string.IsNullOrEmpty(template.Notes))
                {
                    template.Notes = this.ConvertWikiToMarkdown(template.Notes, template.OwningImplementationGuideId, template.Id, "Notes");
                    this.WriteVerbose("Updated template " + template.Id + "'s notes to be markdown");
                }

                foreach (var constraint in template.ChildConstraints.Where(y => !string.IsNullOrEmpty(y.PrimitiveText)))
                {
                    if (!string.IsNullOrEmpty(constraint.PrimitiveText))
                    {
                        constraint.PrimitiveText = this.ConvertWikiToMarkdown(constraint.PrimitiveText, template.OwningImplementationGuideId, template.Id, "PrimitiveText", constraint.Id);
                        this.WriteVerbose(string.Format("Update template {0}'s constraint {1}'s PrimitiveText to be markdown",
                            template.Id,
                            constraint.Id));
                    }
                }
            }

            this.WriteObject(this.logs);
            this.tdb.SaveChanges();
        }
    }
}
