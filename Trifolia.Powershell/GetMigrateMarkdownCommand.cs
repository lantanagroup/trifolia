using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

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
    /// <para type="synopsis">Migrates Wiki Markup and XHTML fields to Markdown</para>
    /// <para type="description">This PowerShell command should only be used once, after upgrading to Trifolia to 2.5.2+ to</para>
    /// <para type="description">migrate the WIKI Markup and XHTML fields to be Markdown fields. If run twice, it may screw up the markdown.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "TrifoliaMigrateMarkdown")]
    public class GetMigrateMarkdownCommand : BaseCommand
    {
        private List<MigrateMarkdownLog> logs;
        private const string CleanHtmlFormat = "<root xmlns:o=\"urn:msxml\" xmlns:v=\"urn:msxml\" xmlns:w=\"urn:msxml\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">{0}</root>";
        private const string TableRegexPattern = @"(<table[^>]*>(?:.|\n)*?<\/table>)";
        private const string SpanRegexPattern = @"<span[^>]*>((?:.|\n)+?)<\/span>";
        private const string DivRegexPattern = @"<div[^>]*>((?:.|\n)+?)<\/div>";
        private const string BadHeadingSpacingRegexPattern = @"(#+\s)\n\s+([A-z]+)";
        private Html2Markdown.Scheme.Markdown markdown = new Html2Markdown.Scheme.Markdown();

        public GetMigrateMarkdownCommand()
        {
            // Replace the stock ReplaceCode replacer with the customized one that uses
            // three back-ticks for multi-line code blocks instead of tabbing
            var replacers = this.markdown.Replacers(); //22
            replacers.RemoveAt(22);
            replacers.Insert(22, new H2MD.CustomReplacer()
            {
                CustomAction = H2MD.Replacers.ReplaceCode
            });
        }

        private string CleanHtml(string html)
        {
            XDocument doc = null;

            string fixedHtml = html;

            fixedHtml = fixedHtml
                .Replace("style=\"font-family: &quot;Courier New&quot;;\"", "style=\"font-family: 'Courier New';\"")
                .Replace("&quot;Bookman Old Style&quot;", "'Bookman Old Style'")
                .Replace(",serif;mso-fareast-font-family:SimSun;\r\n  mso-bidi-font-family:&quot;Times New Roman&quot;;mso-ansi-language:EN-US;mso-fareast-language:\r\n  EN-US;mso-bidi-language:AR-SA;mso-no-proof:yes", "")
                .Replace("”true”", "\"true\"")
                .Replace("”LOINC”", "\"LOINC\"")
                .Replace(",serif;mso-fareast-font-family:SimSun;\nmso-bidi-font-family:&quot;Times New Roman&quot;;mso-ansi-language:EN-US;mso-fareast-language:\nEN-US;mso-bidi-language:AR-SA;mso-no-proof:yes", "")
                .Replace("&quot;Helvetica Neue&quot;", "'Helvetica Neue'")
                .Replace("&quot;Times New Roman&quot;", "'Times New Roman'")
                .Replace("&quot;Arial&quot;", "'Arial'")
                .Replace("H&amp;P", "H&amp;amp;P");
                //.Replace("&lt;", "&amp;lt;")
                //.Replace("&gt;", "&amp;gt;")
                //.Replace("&quot;", "&amp;quot;");

            Regex msoRegex = new Regex(@"\<\!\-\- \[if gte mso 9\].*\!\[endif\] -->", RegexOptions.Singleline);
            fixedHtml = msoRegex.Replace(fixedHtml, "");

            try
            {
                doc = XDocument.Parse(string.Format(CleanHtmlFormat, fixedHtml));
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (XmlException xex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                try
                {
                    doc = XDocument.Parse(string.Format(CleanHtmlFormat, fixedHtml + "</div>"));
                }
                catch
                {
                    try
                    {
                        doc = XDocument.Parse(string.Format(CleanHtmlFormat, fixedHtml + "</div></div>"));
                    }
                    catch
                    {
                        return fixedHtml;
                    }
                }
            }
            catch (Exception)
            {
                return fixedHtml;
            }

            // Remove empty <ul> elements
            doc.Descendants("ul")
                .Where(y => y.Nodes().Count() == 0)
                .Remove();

            this.RemoveOuterElement(doc, "div");
            this.RemoveOuterElement(doc, "div");
            this.RemoveOuterElement(doc, "div");
            this.RemoveOuterElement(doc, "div");

            this.RemoveOuterElement(doc, "span");
            this.RemoveOuterElement(doc, "span");
            this.RemoveOuterElement(doc, "span");
            this.RemoveOuterElement(doc, "span");

            // Replace any paragraphs with ONLY a <code> block in it with just
            // the code block, so that the Html2Markdown conversion does not
            // turn the entire code-block into a single line
            var paras = doc.Descendants("p").ToArray();
            foreach (var para in paras)
            {
                if (para.Elements().Count() == 1 && para.Elements().First().Name == "code")
                {
                    para.ReplaceWith(para.Nodes().ToArray());
                }
            }

            var rootNodes = doc.XPathSelectElements("/root/*");
            return rootNodes.Aggregate("", (b, node) => b += node.ToString());
        }

        private void RemoveOuterElement(XDocument doc, string elementName)
        {
            var elements = doc.Descendants(elementName).ToArray();
            foreach (var element in elements)
            {
                element.ReplaceWith(element.Nodes().ToArray());
            }
        }

        private string CreateMarkdownForTable(string tableHtml)
        {
            XDocument tableDoc = XDocument.Parse(tableHtml);

            string tableMarkdown = "\n";

            var headerColumns = tableDoc.XPathSelectElements("thead/tr/td");
            int headerColumnCount = headerColumns.Count();
            IEnumerable<XElement> rows = null;

            if (headerColumnCount > 0)
            {
                rows = tableDoc.XPathSelectElements("tbody/tr");
                tableMarkdown += "|";

                foreach (var headerColumn in headerColumns)
                {
                    tableMarkdown += " " + headerColumn.Value + " |";
                }

                tableMarkdown += "\n";
            }
            else
            {
                rows = tableDoc.XPathSelectElements("//tr");
                headerColumnCount = tableDoc.XPathSelectElements("//tr[1]/td").Count();
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

            return tableMarkdown.ToString();
        }

        private string CleanMarkdown(string markdown)
        {
            var fixedMarkdown = markdown.Replace(" & ", " &amp; ");

            Regex spanRegex = new Regex(SpanRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Regex tableRegex = new Regex(TableRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Regex divRegex = new Regex(DivRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Regex badHeadingSpacingRegex = new Regex(BadHeadingSpacingRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            MatchCollection tableMatches = tableRegex.Matches(markdown);
            foreach (Match match in tableMatches)
            {
                string tableMarkdown = this.CreateMarkdownForTable(match.Groups[0].Value);
                markdown = markdown.Replace(match.Groups[0].Value, tableMarkdown);
            }

            MatchCollection spanMatches = spanRegex.Matches(markdown);
            while (spanMatches.Count > 0)
            {
                foreach (Match match in spanMatches)
                {
                    if (match.Groups[0].Value.StartsWith("<span class=\"tag\">"))
                    {
                        // Span with a tag of class is an indicator that it is supposed to be formatted as code
                        string codeChar = match.Groups[1].Value.Contains("\n") ? "```" : "`";
                        string replacement = codeChar + match.Groups[1].Value + codeChar;
                        markdown = markdown.Replace(match.Groups[0].Value, replacement);
                    }
                    else
                    {
                        // Other span elements should just be replaced with their inner content
                        markdown = markdown.Replace(match.Groups[0].Value, match.Groups[1].Value);
                    }
                }
                spanMatches = spanRegex.Matches(markdown);
            }

            MatchCollection divMatches = divRegex.Matches(markdown);
            while (divMatches.Count > 0)
            {
                foreach (Match match in divMatches)
                {
                    // Replace all divs with their content
                    markdown = markdown.Replace(match.Groups[0].Value, match.Groups[1].Value);
                }
                divMatches = divRegex.Matches(markdown);
            }

            MatchCollection badHeadingSpacingMatches = badHeadingSpacingRegex.Matches(markdown);
            foreach (Match match in badHeadingSpacingMatches)
            {
                markdown = markdown.Replace(match.Groups[0].Value, match.Groups[1].Value + match.Groups[2].Value);
            }

            return markdown;
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
            var converter = new Html2Markdown.Converter(this.markdown);
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

        public string ConvertHtmlToMarkdown(string html, int implementationGuideId, string property, int? sectionId = null)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            string cleanHtml = html
                .Replace("<div>", "")
                .Replace("</div>", "")
                .Replace("<br>", "")
                .Replace("<br />", "")
                .Replace("<br/>", "");
            cleanHtml = this.CleanHtml(cleanHtml);

            var converter = new Html2Markdown.Converter(this.markdown);
            string markdown;

            try
            {
                markdown = converter.Convert(cleanHtml);
            }
            catch (Exception ex)
            {
                this.WriteVerbose("Error converting to html: " + ex.Message);
                this.WriteVerbose(cleanHtml);
                return html;
            }

            var cleanMarkdown = this.CleanMarkdown(markdown);

            if (html != cleanMarkdown)
                this.logs.Add(new MigrateMarkdownLog()
                {
                    ImplementationGuideId = implementationGuideId,
                    SectionId = sectionId,
                    Property = property,
                    Original = html,
                    New = cleanMarkdown
                });

            return !string.IsNullOrEmpty(markdown) ? cleanMarkdown : html;
        }

        private void MigrateImplementationGuides()
        {
            foreach (var implementationGuide in this.tdb.ImplementationGuides)
            {
                implementationGuide.WebDescription = this.ConvertHtmlToMarkdown(implementationGuide.WebDescription, implementationGuide.Id, "WebDescription");

                foreach (var section in implementationGuide.Sections.Where(y => !string.IsNullOrEmpty(y.Content)))
                {
                    section.Content = this.ConvertHtmlToMarkdown(section.Content, implementationGuide.Id, "Content", section.Id);
                    this.WriteVerbose("Updated implementation guide " + implementationGuide.Id + "'s section " + section.Id + " to be markdown");
                }
            }
        }

        private void MigrateConstraints(Trifolia.DB.Template template)
        {
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

        private void MigrateTemplate(Trifolia.DB.Template template)
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

            this.MigrateConstraints(template);
        }

        private void MigrateTemplates()
        {
            foreach (var template in this.tdb.Templates)
            {
                this.MigrateTemplate(template);
            }
        }

        /// <summary>
        /// Converts fields to markdown
        /// </summary>
        protected override void ProcessRecord()
        {
            this.logs = new List<MigrateMarkdownLog>();

            this.MigrateImplementationGuides();
            this.MigrateTemplates();

            this.WriteObject(this.logs);
            this.tdb.SaveChanges();
        }
    }
}
