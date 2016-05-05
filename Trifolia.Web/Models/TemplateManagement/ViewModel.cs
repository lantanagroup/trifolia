using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Shared;
using Trifolia.Web.Models.IGManagement;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class ViewModel
    {
        public ViewModel()
        {
            this.Actions = new List<ActionItem>();
            this.Constraints = new List<Constraint>();
            this.ContainedByTemplates = new List<ReferenceTemplate>();
            this.ContainedTemplates = new List<ReferenceTemplate>();
            this.Samples = new List<XmlSample>();
        }

        public int Id { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string ImpliedTemplate { get; set; }
        public int? ImpliedTemplateId { get; set; }
        public string ImpliedTemplateOid { get; set; }
        public string ImpliedTemplateImplementationGuide { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public bool ShowNotes { get; set; }
        public bool IsOpen { get; set; }
        public string Organization { get; set; }
        public string ImplementationGuide { get; set; }
        public int? ImplementationGuideId { get; set; }
        public string Type { get; set; }
        public bool CanEdit { get; set; }
        public bool CanEditPublishSettings { get; set; }
        public bool CanCopy { get; set; }
        public bool CanVersion { get; set; }
        public bool CanDelete { get; set; }
        public bool CanEditGreen { get; set; }
        public bool CanMove { get; set; }
        public bool HasPreviousVersion { get; set; }
        public string PreviousVersionTemplateName { get; set; }
        public int? PreviousVersionTemplateId { get; set; }
        public string Status { get; set; }
        public bool HasGreenModel { get; set; }

        public List<ActionItem> Actions { get; set; }
        public List<ReferenceTemplate> ContainedByTemplates { get; set; }
        public List<ReferenceTemplate> ContainedTemplates { get; set; }
        public List<ReferenceTemplate> ImplyingTemplates { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<XmlSample> Samples { get; set; }

        public string Render(Constraint parent = null)
        {
            List<Constraint> theConstraints = (parent == null ? this.Constraints : parent.Children);
            string output = "<ol>\n";

            foreach (Constraint cConstraint in theConstraints)
            {
                output += cConstraint.Prose + "\n";

                output += Render(cConstraint);
            }

            output += "</ol>\n";

            return output;
        }

        public class XmlSample
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Sample { get; set; }
        }

        public class Constraint
        {
            public Constraint()
            {
                this.Children = new List<Constraint>();
            }

            public string Prose { get; set; }
            public bool IsHeading { get; set; }
            public string HeadingTitle { get; set; }
            public string HeadingDescription { get; set; }
            public string Description { get; set; }
            public string Label { get; set; }
            public List<Constraint> Children { get; set; }
        }

        public class ReferenceTemplate
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Oid { get; set; }
            public string ImplementationGuide { get; set; }
        }

        public class ActionItem
        {
            public string Url { get; set; }
            public string Text { get; set; }
            public bool Disabled { get; set; }
            public string ToolTip { get; set; }
        }
    }
}