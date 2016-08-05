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

namespace Trifolia.Export.FHIR.STU3
{
    public class StructureDefinitionExporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private ImplementationGuideType implementationGuideType;
        private Bundle profileBundle;

        public StructureDefinitionExporter(IObjectRepository tdb, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.implementationGuideType = STU3Helper.GetImplementationGuideType(this.tdb, true);
            this.profileBundle = ProfileHelper.GetProfileBundle();
        }

        public Extension Convert(TemplateExtension extension)
        {
            var fhirExtension = new Extension()
            {
                Url = extension.Identifier
            };

            switch (extension.Type)
            {
                case "String":
                    fhirExtension.Value = new FhirString(extension.Value);
                    break;
                case "Integer":
                    try
                    {
                        fhirExtension.Value = new Integer(Int32.Parse(extension.Value));
                    }
                    catch { }
                    break;
                case "Boolean":
                    try
                    {
                        fhirExtension.Value = new FhirBoolean(Boolean.Parse(extension.Value));
                    }
                    catch { }
                    break;
                case "Date":
                    fhirExtension.Value = new Date(extension.Value);
                    break;
                case "DateTime":
                    fhirExtension.Value = new FhirDateTime(extension.Value);
                    break;
                case "Code":
                    fhirExtension.Value = new Code(extension.Value);
                    break;
                case "Coding":
                case "CodeableConcept":
                    string[] valueSplit = extension.Value.Split('|');

                    if (valueSplit.Length == 3)
                    {
                        var coding = new Coding();
                        coding.Code = valueSplit[0];
                        coding.Display = valueSplit[1];
                        coding.System = valueSplit[2];

                        if (extension.Type == "Coding")
                        {
                            fhirExtension.Value = coding;
                        }
                        else
                        {
                            var codeableConcept = new CodeableConcept();
                            codeableConcept.Coding.Add(coding);
                            fhirExtension.Value = codeableConcept;
                        }
                    }
                    break;
            }

            if (fhirExtension.Value != null)
                return fhirExtension;

            return null;
        }

        public void CreateElementDefinition(
            StructureDefinition strucDef,
            TemplateConstraint constraint,
            SimpleSchema.SchemaObject schemaObject,
            string sliceName = null)
        {
            if (constraint.IsPrimitive)     // Skip primitives (for now, at least)
                return;

            string newSliceName = null;
            if (constraint.IsBranch)
                newSliceName = string.Format("{0}_slice_pos{1}", constraint.Context, constraint.Order);

            var igSettings = GetIGSettings(constraint);
            var constraintFormatter = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, constraint);

            ElementDefinition newElementDef = new ElementDefinition()
            {
                ElementId = constraint.Id.ToString(),
                Short = !string.IsNullOrEmpty(constraint.Label) ? constraint.Label : constraint.Context,
                Label = !string.IsNullOrEmpty(constraint.Label) ? constraint.Label : null,
                Comments = !string.IsNullOrEmpty(constraint.Notes) ? constraint.Notes : null,
                Path = constraint.GetElementPath(strucDef.Type != null ? strucDef.Type.ToString() : null),
                Name = constraint.IsBranch ? newSliceName : sliceName,
                Definition = constraintFormatter.GetPlainText(false, false, false)
            };

            // Cardinality
            if (!string.IsNullOrEmpty(constraint.Cardinality))
            {
                newElementDef.Min = constraint.CardinalityType.Left;
                newElementDef.Max = constraint.CardinalityType.Right == Cardinality.MANY ? "*" : constraint.CardinalityType.Right.ToString();
            }

            // Binding
            string valueConformance = string.IsNullOrEmpty(constraint.ValueConformance) ? constraint.Conformance : constraint.ValueConformance;
            bool hasBinding = constraint.ContainedTemplate != null;

            if (constraint.ValueSet != null && valueConformance.IndexOf("NOT") < 0)
            {
                hasBinding = true;
                newElementDef.Binding = new ElementDefinition.BindingComponent()
                {
                    ValueSet = new ResourceReference()
                    {
                        Reference = string.Format("ValueSet/{0}", constraint.ValueSet.Id),
                        Display = constraint.ValueSet.Name
                    }
                };

                if (valueConformance == "SHALL")
                    newElementDef.Binding.Strength = BindingStrength.Required;
                else if (valueConformance == "SHOULD")
                    newElementDef.Binding.Strength = BindingStrength.Preferred;
                else if (valueConformance == "MAY")
                    newElementDef.Binding.Strength = BindingStrength.Example;
            }

            // Single-Value Binding
            if (schemaObject != null && (!string.IsNullOrEmpty(constraint.Value) || !string.IsNullOrEmpty(constraint.DisplayName)))
            {
                hasBinding = true;
                switch (schemaObject.DataType)
                {
                    case "CodeableConcept":
                        var fixedCodeableConcept = new CodeableConcept();
                        var coding = new Coding();
                        fixedCodeableConcept.Coding.Add(coding);

                        if (!string.IsNullOrEmpty(constraint.Value))
                            coding.Code = constraint.Value;

                        if (constraint.CodeSystem != null)
                            coding.System = constraint.CodeSystem.Oid;

                        if (!string.IsNullOrEmpty(constraint.DisplayName))
                            coding.Display = constraint.DisplayName;

                        newElementDef.Fixed = fixedCodeableConcept;
                        break;
                    case "Coding":
                        var fixedCoding = new Coding();

                        if (!string.IsNullOrEmpty(constraint.Value))
                            fixedCoding.Code = constraint.Value;

                        if (constraint.CodeSystem != null)
                            fixedCoding.System = constraint.CodeSystem.Oid;

                        if (!string.IsNullOrEmpty(constraint.DisplayName))
                            fixedCoding.Display = constraint.DisplayName;

                        newElementDef.Fixed = fixedCoding;
                        break;
                    case "code":
                        var fixedCode = new Code();
                        fixedCode.Value = !string.IsNullOrEmpty(constraint.Value) ? constraint.Value : constraint.DisplayName;
                        newElementDef.Fixed = fixedCode;
                        break;
                    default:
                        var fixedString = new FhirString();
                        fixedString.Value = !string.IsNullOrEmpty(constraint.Value) ? constraint.Value : constraint.DisplayName;
                        newElementDef.Fixed = fixedString;
                        break;
                }
            }

            // Add the type of the element when bound to a value set
            if (hasBinding && schemaObject != null && !string.IsNullOrEmpty(schemaObject.DataType))
            {
                StructureDefinition profile = GetBaseProfile(constraint.Template);
                newElementDef.Type = GetProfileDataTypes(profile, constraint);

                // If there is a contained template/profile, make sure it supports a "Reference" type, and then output the profile identifier in the type
                if (constraint.ContainedTemplate != null && newElementDef.Type.Exists(y => y.Code == "Reference" || y.Code == "Extension"))
                {
                    bool isExtension = constraint.ContainedTemplate.PrimaryContextType == "Extension" && newElementDef.Type.Exists(y => y.Code == "Extension");

                    var containedTypes = new List<ElementDefinition.TypeRefComponent>();
                    containedTypes.Add(new ElementDefinition.TypeRefComponent()
                    {
                        Code = isExtension ? "Extension" : "Reference",
                        Profile = constraint.ContainedTemplate.Oid
                    });

                    newElementDef.Type = containedTypes;
                }
            }

            // Add the element to the list
            strucDef.Differential.Element.Add(newElementDef);

            // Children
            foreach (var childConstraint in constraint.ChildConstraints.OrderBy(y => y.Order))
            {
                var childSchemaObject = schemaObject != null ? schemaObject.Children.SingleOrDefault(y => y.Name == childConstraint.Context) : null;
                CreateElementDefinition(strucDef, childConstraint, childSchemaObject, newSliceName);
            }
        }

        public StructureDefinition Convert(Template template, SimpleSchema schema, SummaryType? summaryType = null)
        {
            string id = template.Id.ToString();

            if (template.Oid.StartsWith("http://") || template.Oid.StartsWith("https://"))
                id = template.Oid.Substring(template.Oid.LastIndexOf("/") + 1);

            var fhirStructureDef = new fhir_stu3.Hl7.Fhir.Model.StructureDefinition()
            {
                Id = id,
                Name = template.Name,
                Description = !string.IsNullOrEmpty(template.Description) ? new Markdown(template.Description) : null,
                Kind = StructureDefinition.StructureDefinitionKind.Resource,
                Url = template.Oid,
                Type = template.PrimaryContextType,
                Abstract = false
            };

            // Extensions
            foreach (var extension in template.Extensions)
            {
                var fhirExtension = Convert(extension);

                if (fhirExtension != null)
                    fhirStructureDef.Extension.Add(fhirExtension);
            }

            // Status
            if (template.Status == null || template.Status.IsDraft || template.Status.IsBallot)
                fhirStructureDef.Status = ConformanceResourceStatus.Draft;
            else if (template.Status.IsPublished)
                fhirStructureDef.Status = ConformanceResourceStatus.Active;
            else if (template.Status.IsDraft)
                fhirStructureDef.Status = ConformanceResourceStatus.Retired;

            // Publisher and Contact
            if (template.Author != null)
            {
                if (!string.IsNullOrEmpty(template.Author.ExternalOrganizationName))
                    fhirStructureDef.Publisher = template.Author.ExternalOrganizationName;

                var newContact = new StructureDefinition.ContactComponent();
                newContact.Name = string.Format("{0} {1}", template.Author.FirstName, template.Author.LastName);
                newContact.Telecom.Add(new ContactPoint()
                {
                    Value = template.Author.Phone,
                    Use = ContactPoint.ContactPointUse.Work,
                    System = ContactPoint.ContactPointSystem.Phone
                });
                newContact.Telecom.Add(new ContactPoint()
                {
                    Value = template.Author.Email,
                    Use = ContactPoint.ContactPointUse.Work,
                    System = ContactPoint.ContactPointSystem.Email
                });

                fhirStructureDef.Contact.Add(newContact);
            }

            // Base profile
            if (template.ImpliedTemplate != null)
                fhirStructureDef.BaseDefinitionElement = new FhirUri(string.Format("StructureDefinition/{0}", template.ImpliedTemplate.Id));
            else
                fhirStructureDef.BaseDefinitionElement = new FhirUri(string.Format("http://hl7.org/fhir/StructureDefinition/{0}", template.TemplateType.RootContextType));

            // Constraints
            if (summaryType == null || summaryType == SummaryType.Data)
            {
                var differential = new StructureDefinition.DifferentialComponent();
                fhirStructureDef.Differential = differential;

                // Add base element for resource
                differential.Element.Add(new ElementDefinition()
                {
                    ElementId = string.Format("{0}-00001", template.Id.ToString()),
                    Path = template.PrimaryContextType
                });

                foreach (var constraint in template.ChildConstraints.Where(y => y.ParentConstraint == null).OrderBy(y => y.Order))
                {
                    var schemaObject = schema.Children.SingleOrDefault(y => y.Name == constraint.Context);
                    CreateElementDefinition(fhirStructureDef, constraint, schemaObject);
                }

                // Slices
                var slices = template.ChildConstraints.Where(y => y.IsBranch);
                var sliceGroups = slices.GroupBy(y => y.GetElementPath(template.TemplateType.RootContextType));
                int currentSliceGroupCount = 2;

                foreach (var sliceGroup in sliceGroups)
                {
                    ElementDefinition newElementDef = new ElementDefinition();
                    newElementDef.ElementId = string.Format("{0}-{1}", template.Id, currentSliceGroupCount.ToString("00000"));
                    newElementDef.Path = sliceGroup.Key;

                    foreach (var branchConstraint in sliceGroup)
                    {
                        var igSettings = GetIGSettings(branchConstraint);
                        var constraintFormatter = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, branchConstraint);
                        var branchIdentifiers = branchConstraint.ChildConstraints.Where(y => y.IsBranchIdentifier);

                        newElementDef.Definition = constraintFormatter.GetPlainText(false, false, false);
                        newElementDef.Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Discriminator = (from bi in branchIdentifiers
                                             select bi.GetElementPath(template.TemplateType.RootContextType)),
                            Rules = template.IsOpen ? ElementDefinition.SlicingRules.Open : ElementDefinition.SlicingRules.Closed
                        };

                        // If no discriminators are specified, assume the child SHALL constraints are discriminators
                        if (newElementDef.Slicing.Discriminator.Count() == 0)
                        {
                            var discriminatorConstraints = branchConstraint.ChildConstraints.Where(y => y.Conformance == "SHALL");

                            // If the slice referencing a contained template, use the constraints of the contained template instead of the 
                            // direct constraints of the branch
                            if (branchConstraint.ContainedTemplate != null)
                                discriminatorConstraints = branchConstraint.ContainedTemplate.ChildConstraints.Where(y => y.ParentConstraint == null & y.Conformance == "SHALL");

                            var singleValueDiscriminators = discriminatorConstraints.Where(y => !string.IsNullOrEmpty(y.Value));

                            // If there are constraints that have specific single-value bindings, prefer those
                            if (singleValueDiscriminators.Count() > 0 && singleValueDiscriminators.Count() != discriminatorConstraints.Count())
                                discriminatorConstraints = singleValueDiscriminators;

                            newElementDef.Slicing.Discriminator = discriminatorConstraints.Select(y => y.GetElementPath(template.PrimaryContextType));
                        }
                    }

                    // Find where to insert the slice in the element list
                    var firstElement = fhirStructureDef.Differential.Element.First(y => y.Path == sliceGroup.Key);
                    var firstElementIndex = fhirStructureDef.Differential.Element.IndexOf(firstElement);
                    differential.Element.Insert(firstElementIndex, newElementDef);
                }
            }

            return fhirStructureDef;
        }

        private IGSettingsManager GetIGSettings(TemplateConstraint constraint)
        {
            if (this.allIgSettings.ContainsKey(constraint.Template.OwningImplementationGuide))
                return this.allIgSettings[constraint.Template.OwningImplementationGuide];

            if (constraint.Template.OwningImplementationGuideId == 0)
                return null;

            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, constraint.Template.OwningImplementationGuide.Id);
            this.allIgSettings.Add(constraint.Template.OwningImplementationGuide, igSettings);

            return igSettings;
        }

        private StructureDefinition GetBaseProfile(Template template)
        {
            if (this.baseProfiles.ContainsKey(template.TemplateType.RootContextType))
                return this.baseProfiles[template.TemplateType.RootContextType];

            var resourceType = template.TemplateType.RootContextType.ToLower();
            var resourceTypeUrl = "http://hl7.org/fhir/StructureDefinition/" + resourceType;

            foreach (var entry in this.profileBundle.Entry)
            {
                if (entry.FullUrl.ToLower() == resourceTypeUrl.ToLower())
                    return (StructureDefinition)entry.Resource;
            }

            return null;
        }

        private List<ElementDefinition.TypeRefComponent> GetProfileDataTypes(StructureDefinition structure, TemplateConstraint constraint)
        {
            if (structure == null || structure.Snapshot == null)
                return null;

            string path = constraint.GetFhirPath();
            var element = structure.Snapshot.Element.FirstOrDefault(y => y.Path == path);

            if (element == null)
                return null;

            return element.Type;
        }
    }
}
