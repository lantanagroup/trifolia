using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.Web.Models.TemplateManagement;

namespace Trifolia.Web.Models.IG
{
    public class ViewDataModel
    {
        public ViewDataModel()
        {
            this.Templates = new List<Template>();
            this.ValueSets = new List<ValueSet>();
            this.CodeSystems = new List<CodeSystem>();
            this.TemplateTypes = new List<TemplateType>();
            this.Volume1Sections = new List<Section>();
        }

        public int ImplementationGuideId { get; set; }
        public string ImplementationGuideName { get; set; }
        public string ImplementationGuideDisplayName { get; set; }
        public string ImplementationGuideDescription { get; set; }
        public int? ImplementationGuideFileId { get; set; }
        public string RootContextType { get; set; }
        public string Status { get; set; }
        public string PublishDate { get; set; }
        public string Volume1Html { get; set; }

        public List<ValueSet> ValueSets { get; set; }
        public List<CodeSystem> CodeSystems { get; set; }
        public List<Template> Templates { get; set; }
        public List<TemplateType> TemplateTypes { get; set; }
        public List<Section> Volume1Sections { get; set; }

        public class Template
        {
            public Template()
            {
                this.Samples = new List<Sample>();
                this.Constraints = new List<Constraint>();
                this.ContainedTemplates = new List<TemplateReference>();
                this.ImplyingTemplates = new List<TemplateReference>();
                this.ContainedByTemplates = new List<TemplateReference>();
            }

            public string Identifier { get; set; }
            public string Bookmark { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ContextType { get; set; }
            public string Context { get; set; }
            public string Extensibility { get; set; }
            public int TemplateTypeId { get; set; }

            public DifferenceModel Changes { get; set; }
            public List<Sample> Samples { get; set; }
            public TemplateReference ImpliedTemplate { get; set; }
            public List<TemplateReference> ContainedTemplates { get; set; }
            public List<TemplateReference> ImplyingTemplates { get; set; }
            public List<TemplateReference> ContainedByTemplates { get; set; }
            public List<Constraint> Constraints { get; set; }
        }

        public class Constraint
        {
            public Constraint()
            {
                this.Constraints = new List<Constraint>();
            }

            public string Number { get; set; }
            public string Narrative { get; set; }
            public string Context { get; set; }
            public string Value { get; set; }
            public string Cardinality { get; set; }
            public string Conformance { get; set; }
            public string DataType { get; set; }
            public string ValueSetIdentifier { get; set; }
            public string ValueSetDate { get; set; }
            public TemplateReference ContainedTemplate { get; set; }

            public List<Constraint> Constraints { get; set; }
        }

        public class TemplateReference
        {
            public TemplateReference()
            {

            }

            public TemplateReference(Trifolia.DB.Template template)
            {
                this.Name = template.Name;
                this.Identifier = template.Oid;
                this.Bookmark = template.Bookmark;
                this.ImplementationGuide = template.OwningImplementationGuide.GetDisplayName();
                this.PublishDate = template.OwningImplementationGuide.PublishDate;
            }

            public string Name { get; set; }
            public string Identifier { get; set; }
            public string Bookmark { get; set; }
            public string ImplementationGuide { get; set; }
            public DateTime? PublishDate { get; set; }
        }

        public class Sample
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SampleText { get; set; }
        }

        public class ValueSet
        {
            public ValueSet()
            {
                this.Members = new List<ValueSetMember>();
            }

            public string Identifier { get; set; }
            public string Name { get; set; }
            public string Source { get; set; }
            public string Description { get; set; }
            public string BindingDate { get; set; }

            public List<ValueSetMember> Members { get; set; }
        }

        public class ValueSetMember
        {
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string CodeSystemIdentifier { get; set; }
            public string CodeSystemName { get; set; }
        }

        public class TemplateType
        {
            public int TemplateTypeId { get; set; }
            public string Name { get; set; }
            public string ContextType { get; set; }
            public string Description { get; set; }
        }

        public class CodeSystem : IEquatable<CodeSystem>
        {
            public string Identifier { get; set; }
            public string Name { get; set; }

            public bool Equals(CodeSystem other)
            {
                //Check whether the compared object is null.
                if (Object.ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                if (Object.ReferenceEquals(this, other)) return true;

                return Identifier.Equals(other.Identifier);
            }

            public override int GetHashCode()
            {
                return Identifier.GetHashCode();
            }
        }

        public class Section
        {
            public string Heading { get; set; }
            public string Content { get; set; }
            public int Level { get; set; }
        }
    }
}