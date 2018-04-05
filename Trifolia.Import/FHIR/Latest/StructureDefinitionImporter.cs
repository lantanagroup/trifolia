extern alias fhir_latest;
using fhir_latest.Hl7.Fhir.Model;
using fhir_latest.Hl7.Fhir.Specification.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.DB;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using StructureDefinition = fhir_latest.Hl7.Fhir.Model.StructureDefinition;

namespace Trifolia.Import.FHIR.Latest
{
    public class StructureDefinitionImporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private ImplementationGuideType implementationGuideType;

        public StructureDefinitionImporter(IObjectRepository tdb, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.implementationGuideType = LatestHelper.GetImplementationGuideType(this.tdb, true);
        }

        private void PopulateBinding(TemplateConstraint constraint, CodeableConcept codeableConcept)
        {
            if (codeableConcept != null)
            {
                var coding = codeableConcept.Coding != null ? codeableConcept.Coding.FirstOrDefault() : null;
                this.PopulateBinding(constraint, coding);
            }
        }

        private void PopulateBinding(TemplateConstraint constraint, Coding coding)
        {
            if (coding != null)
            {
                constraint.Value = coding.Code;
                constraint.DisplayName = coding.Display;

                // TODO: Handle coding.System
            }
        }

        private void PopulateBinding(TemplateConstraint constraint, Code code)
        {
            if (code != null)
                constraint.Value = code.Value;
        }

        private void PopulateBinding(TemplateConstraint constraint, FhirString fhirString)
        {
            if (fhirString != null)
                constraint.Value = fhirString.Value;
        }

        public Template Convert(StructureDefinition strucDef, Template template = null, User author = null)
        {
            if (string.IsNullOrEmpty(strucDef.Type))
                throw new Exception("StructureDefinition.type is required");

            string strucDefDescription = strucDef.Description != null ? strucDef.Description.Value : null;

            if (author == null)
            {
                author = this.tdb.Users.SingleOrDefault(y => y.UserName == LatestHelper.DEFAULT_USER_NAME);

                if (author == null)
                {
                    Log.For(this).Error("Default user {0} could not be found to set author for template from StructureDefinition.", LatestHelper.DEFAULT_USER_NAME);
                    throw new Exception("Default user could not be found. Trifolia is configured incorrectly.");
                }
            }

            if (template == null)
            {
                ImplementationGuide unassignedImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y =>
                    y.Name == LatestHelper.DEFAULT_IG_NAME &&
                    y.ImplementationGuideTypeId == this.implementationGuideType.Id);

                if (unassignedImplementationGuide == null)
                {
                    unassignedImplementationGuide = new ImplementationGuide()
                    {
                        Name = LatestHelper.DEFAULT_IG_NAME,
                        ImplementationGuideType = this.implementationGuideType,
                        Organization = this.tdb.Organizations.Single(y => y.Name == LatestHelper.DEFAULT_ORG_NAME)
                    };
                    this.tdb.ImplementationGuides.Add(unassignedImplementationGuide);
                }

                template = new Template()
                {
                    OwningImplementationGuide = unassignedImplementationGuide,
                    ImplementationGuideType = this.implementationGuideType,
                    Author = author,
                    IsOpen = true
                };
            }

            // Name
            if (template.Name != strucDef.Name)
                template.Name = strucDef.Name;

            // Descrition
            if (template.Description != strucDefDescription)
                template.Description = strucDefDescription;

            // Identifier -> Oid
            string identifier = strucDef.Url;

            if (string.IsNullOrEmpty(identifier))
                identifier = string.Format(LatestHelper.STRUCDEF_NEW_IDENTIFIER_FORMAT, Guid.NewGuid());

            if (template.Oid != identifier)
                template.Oid = identifier;

            string fhirBaseType = strucDef.Type != null ? strucDef.Type.ToString() : string.Empty;

            // ConstrainedType -> Template Type
            TemplateType templateType = this.tdb.TemplateTypes.SingleOrDefault(y =>
                y.ImplementationGuideTypeId == this.implementationGuideType.Id &&
                y.RootContextType == fhirBaseType);

            if (templateType == null)
                throw new Exception("Could not find Template Type for " + strucDef.Type);

            if (template.TemplateType != templateType)
                template.TemplateType = templateType;

            if (template.PrimaryContext != template.TemplateType.RootContext)
                template.PrimaryContext = template.TemplateType.RootContext;

            if (template.PrimaryContextType != template.TemplateType.RootContextType)
                template.PrimaryContextType = template.TemplateType.RootContextType;

            // Bookmark
            if (string.IsNullOrEmpty(template.Bookmark))
            {
                template.Bookmark = Template.GenerateBookmark(template.Name, template.TemplateType.Name.ToUpper());

                // Check if the bookmark already exists. If so, append a number onto the end
                if (this.tdb.Templates.Any(y => y.Bookmark.ToLower().Trim() == template.Bookmark.ToLower().Trim()))
                {
                    int bookmarkCount = this.tdb.Templates.Count(y => y.Bookmark.ToLower().Trim().StartsWith(template.Bookmark.ToLower().Trim()));
                    template.Bookmark += (bookmarkCount + 1);
                }
            }

            if (strucDef.Snapshot != null && strucDef.Differential == null)
                throw new Exception("Trifolia does not support snapshots for DSTU2, yet");

            // Differential.Element -> Constraint
            if (strucDef.Differential != null)
            {
                // Remove all current constraints from the template so that we re-create
                foreach (var cc in template.ChildConstraints.ToList())
                    this.tdb.TemplateConstraints.Remove(cc);

                ElementDefinitionNavigator navigator = new ElementDefinitionNavigator(strucDef.Differential.Element);
                TemplateConstraint current = null;

                if (navigator.MoveToFirstChild() && navigator.MoveToFirstChild())
                {
                    while (true)
                    {
                        if (navigator.Current.Slicing != null)
                        {
                            if (!navigator.MoveToNext())
                            {
                                if (current != null && current.ParentConstraint != null && navigator.MoveToParent())
                                    current = current.ParentConstraint;
                                else
                                    break;
                            }
                            continue;
                        }

                        TemplateConstraint next = new TemplateConstraint();
                        next.Context = navigator.PathName;
                        next.ParentConstraint = current;
                        next.Order = current != null ? current.ChildConstraints.Count() : template.ChildConstraints.Count(y => y.ParentConstraint == null);

                        if (navigator.Elements.Any(y => y.Path == navigator.Path && y.Slicing != null))
                            next.IsBranch = true;

                        template.ChildConstraints.Add(next);

                        string cardinality = string.Format("{0}..{1}",
                            navigator.Current.Min == null ? 0 : navigator.Current.Min,
                            string.IsNullOrEmpty(navigator.Current.Max) ? "*" : navigator.Current.Max);

                        if (next.Cardinality != cardinality)
                            next.Cardinality = cardinality;

                        string conformance = cardinality.StartsWith("1") ? "SHALL" : "SHOULD";

                        if (next.Conformance != conformance)
                            next.Conformance = conformance;

                        // Binding
                        if (navigator.Current.Fixed != null)
                        {
                            this.PopulateBinding(next, navigator.Current.Fixed as CodeableConcept);
                            this.PopulateBinding(next, navigator.Current.Fixed as Coding);
                            this.PopulateBinding(next, navigator.Current.Fixed as Code);
                            this.PopulateBinding(next, navigator.Current.Fixed as FhirString);

                            next.IsFixed = true;
                        }
                        else
                        {
                            this.PopulateBinding(next, navigator.Current.Pattern as CodeableConcept);
                            this.PopulateBinding(next, navigator.Current.Pattern as Coding);
                            this.PopulateBinding(next, navigator.Current.Pattern as Code);
                            this.PopulateBinding(next, navigator.Current.Pattern as FhirString);

                            next.IsFixed = false;
                        }

                        if (navigator.MoveToFirstChild())
                        {
                            current = next;
                            continue;
                        }
                        else if (navigator.MoveToNext())
                            continue;
                        else if (navigator.MoveToParent() && navigator.MoveToNext())
                        {
                            current = current.ParentConstraint;
                            continue;
                        }
                        else
                            break;
                    }
                }
            }

            return template;
        }
    }
}
