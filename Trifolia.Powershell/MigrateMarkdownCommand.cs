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

namespace Trifolia.Powershell
{
    public class MigrateMarkdownLog
    {
        public int TemplateId { get; set; }
        public int? ConstraintId { get; set; }
        public string Property { get; set; }
        public string Original { get; set; }
        public string New { get; set; }
    }

    [Cmdlet(VerbsCommon.Get, "MigrateMarkdown")]
    public class MigrateMarkdownCommand : BaseCommand
    {
        private List<MigrateMarkdownLog> logs;

        private string ConvertWikiToMarkdown(string wiki, int templateId, string property, int? constraintId = null)
        {
            var converter = new Html2Markdown.Converter();
            string markdown = wiki;
            string cleanWiki = wiki
                .Replace(" [1..0]", " [[]1..0]")
                .Replace(" [0..1]", " [[]0..1]")
                .Replace(" [0..*]", " [[]0..*]")
                .Replace(" [1..1]", " [[]1..1]")
                .Replace(" [1..*]", " [[]1..*]")
                .Replace(" [urn:", " [[]urn:");

            try
            {
                string html = WikiNetParser.WikiProvider.ConvertToHtml(cleanWiki);

                if (string.IsNullOrEmpty(html))
                    throw new Exception("Error converting WIKI content to HTML");

                markdown = converter.Convert(html);
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

        /// <summary>
        /// Converts the following fields to MarkDown format:
        /// - Template.Description
        /// - TemplateConstraint.PrimitiveText
        /// </summary>
        protected override void ProcessRecord()
        {
            this.logs = new List<MigrateMarkdownLog>();

            foreach (var template in this.tdb.Templates)
            {
                if (!string.IsNullOrEmpty(template.Description))
                {
                    template.Description = this.ConvertWikiToMarkdown(template.Description, template.Id, "Description");
                    this.WriteVerbose("Updated template " + template.Id.ToString() + "'s description to be markdown");
                }

                if (!string.IsNullOrEmpty(template.Notes))
                {
                    template.Notes = this.ConvertWikiToMarkdown(template.Notes, template.Id, "Notes");
                    this.WriteVerbose("Updated template " + template.Id.ToString() + "'s notes to be markdown");
                }

                foreach (var constraint in template.ChildConstraints.Where(y => !string.IsNullOrEmpty(y.PrimitiveText)))
                {
                    if (!string.IsNullOrEmpty(constraint.PrimitiveText))
                    {
                        constraint.PrimitiveText = this.ConvertWikiToMarkdown(constraint.PrimitiveText, template.Id, "PrimitiveText", constraint.Id);
                        this.WriteVerbose(string.Format("Update template {0}'s constraint {1}'s PrimitiveText to be markdown",
                            template.Id,
                            constraint.Id));
                    }
                }
            }

            this.WriteObject(this.logs);
        }
    }
}
