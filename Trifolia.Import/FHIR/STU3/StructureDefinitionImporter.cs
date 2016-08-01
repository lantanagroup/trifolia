extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Serialization;
using fhir_stu3.Hl7.Fhir.Specification.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using Trifolia.Shared.FHIR.Profiles.STU3;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using StructureDefinition = fhir_stu3.Hl7.Fhir.Model.StructureDefinition;
using SummaryType = fhir_stu3.Hl7.Fhir.Rest.SummaryType;

namespace Trifolia.Import.FHIR.STU3
{
    public class StructureDefinitionImporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private ImplementationGuideType implementationGuideType;
        private Bundle profileBundle;

        public StructureDefinitionImporter(IObjectRepository tdb, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.implementationGuideType = DSTU2Helper.GetImplementationGuideType(this.tdb, true);
            this.profileBundle = ProfileHelper.GetProfileBundle();
        }

        public Template Convert(StructureDefinition strucDef, Template template = null)
        {
            if (string.IsNullOrEmpty(strucDef.Type))
                throw new Exception("StructureDefinition.type is required");

            string strucDefDescription = strucDef.Description != null ? strucDef.Description.Value : null;

            if (template == null)
            {
                ImplementationGuide unassignedImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y =>
                    y.Name == DSTU2Helper.DEFAULT_IG_NAME &&
                    y.ImplementationGuideTypeId == this.implementationGuideType.Id);

                if (unassignedImplementationGuide == null)
                {
                    unassignedImplementationGuide = new ImplementationGuide()
                    {
                        Name = DSTU2Helper.DEFAULT_IG_NAME,
                        ImplementationGuideType = this.implementationGuideType
                    };
                    this.tdb.ImplementationGuides.AddObject(unassignedImplementationGuide);
                }

                template = new Template()
                {
                    OwningImplementationGuide = unassignedImplementationGuide,
                    ImplementationGuideType = this.implementationGuideType,
                    Author = this.tdb.Users.Single(y => y.UserName == DSTU2Helper.DEFAULT_USER_NAME),
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
                identifier = string.Format(STU3Helper.STRUCDEF_NEW_IDENTIFIER_FORMAT, Guid.NewGuid());

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
            template.Bookmark = Template.GenerateBookmark(template.Name, template.TemplateType.Name.ToUpper());

            if (strucDef.Snapshot != null && strucDef.Differential == null)
                throw new Exception("Trifolia does not support snapshots for DSTU2, yet");

            // Differential.Element -> Constraint
            if (strucDef.Differential != null)
            {
                // Remove all current constraints from the template so that we re-create
                foreach (var cc in template.ChildConstraints.ToList())
                    this.tdb.TemplateConstraints.DeleteObject(cc);

                ElementNavigator navigator = new ElementNavigator(strucDef.Differential.Element);
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
