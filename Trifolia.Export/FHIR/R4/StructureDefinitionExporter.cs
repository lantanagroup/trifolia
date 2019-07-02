extern alias fhir_r4;
using fhir_r4.Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.DB;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Plugins;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using Trifolia.Shared.FHIR.Profiles.R4;
using static fhir_r4::Hl7.Fhir.Model.ElementDefinition;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using StructureDefinition = fhir_r4.Hl7.Fhir.Model.StructureDefinition;

namespace Trifolia.Export.FHIR.R4
{
    public class StructureDefinitionExporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private ImplementationGuideType implementationGuideType;
        private IIGTypePlugin igTypePlugin;

        public StructureDefinitionExporter(IObjectRepository tdb, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.implementationGuideType = STU3Helper.GetImplementationGuideType(this.tdb, true);
            this.igTypePlugin = this.implementationGuideType.GetPlugin();
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
            SimpleSchema.SchemaObject schemaObject)
        {
            if (constraint.IsPrimitive)     // Skip primitives (for now, at least)
                return;

            string sliceName = constraint.GetSliceName();
            string elementPath = constraint.GetElementPath(strucDef.Type != null ? strucDef.Type.ToString() : null);
            var igSettings = GetIGSettings(constraint);
            var constraintFormatter = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, this.igTypePlugin, constraint);
            string definition = constraintFormatter.GetPlainText(false, false, false);

            if (definition == null)
                definition = string.Empty;

            ElementDefinition newElementDef = new ElementDefinition()
            {
                Short = !string.IsNullOrEmpty(constraint.Label) ? constraint.Label : constraint.Context.Replace("@", ""),
                Label = !string.IsNullOrEmpty(constraint.Label) ? constraint.Label : null,
                Comment = !string.IsNullOrEmpty(constraint.Notes) ? new Markdown(constraint.Notes) : null,
                Path = elementPath,
                SliceName = sliceName,
                Definition = new Markdown(definition),
                ElementId = constraint.GetElementId()
            };

            /*
            if (constraint.IsBranch && schemaObject != null && schemaObject.UpperBound <= 1)
            {
                newElementDef.Extension.Add(new Extension()
                {
                    Url = "https://trifolia.lantanagroup.com/fhir/StructureDefinition/extension-not-slicable",
                    Value = new FhirBoolean(true)
                });
            }
            */

            if (constraint.IsChoice)
            {
                newElementDef.Slicing = new ElementDefinition.SlicingComponent();
                newElementDef.Slicing.Discriminator.Add(new ElementDefinition.DiscriminatorComponent()
                {
                    Type = ElementDefinition.DiscriminatorType.Type,
                    Path = "$this"
                });
                newElementDef.Slicing.Rules = ElementDefinition.SlicingRules.Open;
            }
            else if (constraint.Parent != null && constraint.Parent.IsChoice)
            {
                newElementDef.SliceName = constraint.Context.Replace("@", "");
            }

            // Cardinality
            if (!string.IsNullOrEmpty(constraint.Cardinality))
            {
                newElementDef.Min = constraint.CardinalityType.Left;
                newElementDef.Max = constraint.CardinalityType.Right == Cardinality.MANY ? "*" : constraint.CardinalityType.Right.ToString();
            }

            // Binding
            string valueConformance = string.IsNullOrEmpty(constraint.ValueConformance) ? constraint.Conformance : constraint.ValueConformance;
            bool hasBinding = constraint.References.Any(y => y.ReferenceType == ConstraintReferenceTypes.Template);

            if (constraint.ValueSet != null && valueConformance.IndexOf("NOT") < 0)
            {
                hasBinding = true;
                newElementDef.Binding = new ElementDefinition.ElementDefinitionBindingComponent()
                {
                    ValueSetElement = constraint.ValueSet.GetIdentifier(ValueSetIdentifierTypes.HTTP)
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
                Element elementBinding = null;

                hasBinding = true;
                switch (schemaObject.DataType)
                {
                    case "CodeableConcept":
                        var codableConceptBinding = new CodeableConcept();
                        var coding = new Coding();
                        codableConceptBinding.Coding.Add(coding);

                        if (!string.IsNullOrEmpty(constraint.Value))
                            coding.Code = constraint.Value;

                        if (constraint.CodeSystem != null)
                            coding.System = constraint.CodeSystem.Oid;

                        if (!string.IsNullOrEmpty(constraint.DisplayName))
                            coding.Display = constraint.DisplayName;

                        elementBinding = codableConceptBinding;
                        break;
                    case "Coding":
                        var codingBinding = new Coding();

                        if (!string.IsNullOrEmpty(constraint.Value))
                            codingBinding.Code = constraint.Value;

                        if (constraint.CodeSystem != null)
                            codingBinding.System = constraint.CodeSystem.Oid;

                        if (!string.IsNullOrEmpty(constraint.DisplayName))
                            codingBinding.Display = constraint.DisplayName;

                        elementBinding = codingBinding;
                        break;
                    case "code":
                        var codeBinding = new Code();
                        codeBinding.Value = !string.IsNullOrEmpty(constraint.Value) ? constraint.Value : constraint.DisplayName;
                        elementBinding = codeBinding;
                        break;
                    default:
                        var stringBinding = new FhirString();
                        stringBinding.Value = !string.IsNullOrEmpty(constraint.Value) ? constraint.Value : constraint.DisplayName;
                        elementBinding = stringBinding;
                        break;
                }

                newElementDef.Fixed = elementBinding;
            }

            // Add the type of the element when bound to a value set
            if (hasBinding && schemaObject != null && !string.IsNullOrEmpty(schemaObject.DataType))
            {
                StructureDefinition profile = GetBaseProfile(constraint.Template);
                newElementDef.Type = GetProfileDataTypes(profile, constraint);

                var containedTemplates = (from tcr in constraint.References
                                          join t in this.tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                                          where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                          select new { Identifier = t.Oid, t.PrimaryContextType });
                
                // If there is a contained template/profile, make sure it supports a "Reference" type, and then output the profile identifier in the type
                if (containedTemplates.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(newElementDef.SliceName))
                    {
                        var foundMatchingElement = strucDef.Differential.Element.SingleOrDefault(y => y.Path == newElementDef.Path && y.SliceName == newElementDef.SliceName);

                        if (foundMatchingElement != null)
                        {
                            foundMatchingElement.Definition.Value += " " + definition;
                            newElementDef = foundMatchingElement;
                        }
                    }

                    var containedTypes = new List<ElementDefinition.TypeRefComponent>();
                    foreach (var containedTemplate in containedTemplates)
                    {
                        containedTypes.Add(new ElementDefinition.TypeRefComponent()
                        {
                            Code = containedTemplate.PrimaryContextType,
                            Profile = new string[] { containedTemplate.Identifier }
                        });
                    }

                    newElementDef.Type = containedTypes;
                }
            }

            // Add the element to the list if it's new
            if (!strucDef.Differential.Element.Contains(newElementDef))
                strucDef.Differential.Element.Add(newElementDef);

            // Children
            foreach (var childConstraint in constraint.ChildConstraints.Where(y => !y.IsPrimitive).OrderBy(y => y.Order))
            {
                SimpleSchema.SchemaObject childSchemaObject = null;

                if (schemaObject != null)
                {
                    if (childConstraint.Context.StartsWith("@"))
                        childSchemaObject = schemaObject.Children.SingleOrDefault(y => y.Name == childConstraint.Context.Substring(1) && y.IsAttribute);
                    else
                        childSchemaObject = schemaObject.Children.SingleOrDefault(y => y.Name == childConstraint.Context);
                }

                CreateElementDefinition(strucDef, childConstraint, childSchemaObject);
            }
        }

        private IEnumerable<TemplateConstraint> GetTemplateConstraints(Template template)
        {
            List<TemplateConstraint> constraints = template.ChildConstraints.Where(y => !y.IsPrimitive).ToList();
             
            // Remove all primitives from the child constraints of all the constraints in the template
            // We use .ChildConstraints in a lot of places in this class, so we need to make sure primitives are not part of it
            for (var x = constraints.Count - 1; x >= 0; x--)
            {
                var constraint = constraints.ElementAt(x);

                for (var i = constraint.ChildConstraints.Count - 1; i >= 0; i--)
                {
                    var childConstraint = constraint.ChildConstraints.ElementAt(i);

                    if (childConstraint.IsPrimitive)
                        constraint.ChildConstraints.Remove(childConstraint);
                }

                // If it's an empty constraint
                if (!constraint.HasBinding() && !constraint.HasContainedTemplates() && constraint.ChildConstraints.Count == 0)
                {
                    // and there are other constraints for the same context
                    if (constraints.Exists(y => y != constraint && y.ParentConstraintId == constraint.ParentConstraintId && y.Context == constraint.Context))
                    {
                        // remove the constraint because it doesn't do anything...
                        constraints.Remove(constraint);
                    }
                }
            }

            var groupedBranches = constraints.Where(y => y.IsBranch && (y.ChildConstraints.Count > 0 || y.References.Count > 0)).GroupBy(y => y.Context).Where(y => y.Count() > 1);

            foreach (var groupedBranch in groupedBranches)
            {
                var allHasContainedTemplate = true;

                foreach (var branch in groupedBranch)
                {
                    if (branch.ChildConstraints.Count == 1 && branch.ChildConstraints.First().References.Count != 1)
                        allHasContainedTemplate = false;
                    else if (branch.ChildConstraints.Count > 1)
                        allHasContainedTemplate = false;
                    else if (branch.ChildConstraints.Count == 0 && branch.References.Count != 1)
                        allHasContainedTemplate = false;
                }

                if (allHasContainedTemplate)
                {
                    var firstBranch = groupedBranch.ElementAt(0);

                    for (var i = groupedBranch.Count() - 1; i > 0; i--)
                    {
                        var branch = groupedBranch.ElementAt(i);

                        if (branch.References.Count == 1)
                        {
                            // The reference is directly on the branch root
                            firstBranch.References.Add(branch.References.First());
                            constraints.Remove(branch);
                        }
                        else if (branch.ChildConstraints.Count == 1 && firstBranch.ChildConstraints.Count == 1)
                        {
                            // The reference is on the only child in the branch root
                            firstBranch.ChildConstraints.First().References.Add(branch.ChildConstraints.First().References.First());
                            constraints.Remove(branch.ChildConstraints.First());
                            constraints.Remove(branch);
                        }
                    }

                    firstBranch.IsBranch = false;
                }
            }

            return constraints;
        }

        public StructureDefinition Convert(Template template, SimpleSchema schema, SummaryType? summaryType = null)
        {
            string id = template.FhirId();
            string version = "";

            var isCDA = schema.Namespaces.ToList().Exists(y => y.Key == "urn:hl7-org:v3");

            if (isCDA)
            {
                if (template.IsIdentifierOID())
                    template.GetIdentifierOID(out id);
                else if (template.IsIdentifierII())
                    template.GetIdentifierII(out id, out version);
            }

            var fhirStructureDef = new fhir_r4.Hl7.Fhir.Model.StructureDefinition()
            {
                Id = id,
                Version = version,
                Name = template.Name,
                Description = template.Description != null ? new Markdown(template.Description.RemoveInvalidUtf8Characters()) : null,
                Kind = template.PrimaryContextType == "Extension" ? StructureDefinition.StructureDefinitionKind.ComplexType : StructureDefinition.StructureDefinitionKind.Resource,
                Url = template.FhirUrl(),
                Type = isCDA ? template.PrimaryContextType : template.TemplateType.RootContextType,
                Abstract = false,
                Derivation = StructureDefinition.TypeDerivationRule.Constraint
            };

            fhirStructureDef.Identifier.Add(new Identifier()
            {
                Value = template.Oid
            });

            // If this is an extension, determine what uses the extension and list them in the
            // "context" field so that the extension knows where it can be used.
            foreach (var extension in template.Extensions)
            {
                var fhirExtension = Convert(extension);

                if (fhirExtension != null)
                    fhirStructureDef.Extension.Add(fhirExtension);
            }

            // Status
            if (template.Status == null || template.Status.IsDraft || template.Status.IsBallot)
                fhirStructureDef.Status = PublicationStatus.Draft;
            else if (template.Status.IsPublished)
                fhirStructureDef.Status = PublicationStatus.Active;
            else if (template.Status.IsDraft)
                fhirStructureDef.Status = PublicationStatus.Retired;

            // Publisher and Contact
            if (template.Author != null)
            {
                if (!string.IsNullOrEmpty(template.Author.ExternalOrganizationName))
                    fhirStructureDef.Publisher = template.Author.ExternalOrganizationName;

                var newContact = new ContactDetail();
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
                fhirStructureDef.BaseDefinitionElement = new Canonical(template.ImpliedTemplate.FhirUrl());
            else
                fhirStructureDef.BaseDefinitionElement = new Canonical(string.Format("http://hl7.org/fhir/StructureDefinition/{0}", template.TemplateType.RootContextType));

            // Constraints
            if (summaryType == null || summaryType == SummaryType.Data)
            {
                var constraints = this.GetTemplateConstraints(template);

                var differential = new StructureDefinition.DifferentialComponent();
                fhirStructureDef.Differential = differential;

                // Add base element for resource
                var rootElement = new ElementDefinition();
                rootElement.Path = template.PrimaryContextType;
                rootElement.ElementId = template.PrimaryContextType;
                differential.Element.Add(rootElement);

                var rootConstraints = constraints.Where(y => y.ParentConstraint == null && !string.IsNullOrEmpty(y.Context)).OrderBy(y => y.Order);
                foreach (var constraint in rootConstraints)
                {
                    SimpleSchema.SchemaObject schemaObject = null;

                    if (schema != null)
                    {
                        if (constraint.Context.StartsWith("@"))
                            schemaObject = schema.Children.SingleOrDefault(y => y.Name == constraint.Context.Substring(1) && y.IsAttribute);
                        else
                            schemaObject = schema.Children.SingleOrDefault(y => y.Name == constraint.Context);
                    }

                    CreateElementDefinition(fhirStructureDef, constraint, schemaObject);
                }

                // Slices
                var slices = constraints.Where(y => y.IsBranch);
                var sliceGroups = slices.GroupBy(y => y.GetElementPath(isCDA ? template.PrimaryContextType : template.TemplateType.RootContextType));
                int currentSliceGroupCount = 2;

                // Adds an element that contains "slicing" information for the branch(es)
                foreach (var sliceGroup in sliceGroups)
                {
                    ElementDefinition newElementDef = new ElementDefinition();
                    //newElementDef.ElementId = string.Format("{0}-{1}", template.Id, currentSliceGroupCount.ToString("00"));
                    newElementDef.Path = sliceGroup.Key;
                    newElementDef.ElementId = sliceGroup.First().GetElementPath(template.PrimaryContextType);

                    foreach (var branchConstraint in sliceGroup)
                    {
                        var igSettings = GetIGSettings(branchConstraint);
                        var constraintFormatter = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, this.igTypePlugin, branchConstraint);
                        var branchIdentifiers = branchConstraint.ChildConstraints.Where(y => y.IsBranchIdentifier);

                        newElementDef.Definition = new Markdown(constraintFormatter.GetPlainText(false, false, false));
                        newElementDef.Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Rules = template.IsOpen ? ElementDefinition.SlicingRules.Open : ElementDefinition.SlicingRules.Closed
                        };

                        if (branchIdentifiers.Count() > 0)
                        {
                            newElementDef.Slicing.Discriminator = (from bi in branchIdentifiers
                                                                   select new ElementDefinition.DiscriminatorComponent()
                                                                   {
                                                                       Type = ElementDefinition.DiscriminatorType.Value,
                                                                       Path = bi.GetElementPath(null, branchConstraint)
                                                                   }).ToList();
                        }
                        else if (branchConstraint.Context == "extension")
                        {
                            newElementDef.Slicing.Discriminator.Add(new ElementDefinition.DiscriminatorComponent()
                            {
                                Type = ElementDefinition.DiscriminatorType.Value,
                                Path = "url"
                            });
                        }
                        else        // If no discriminators are specified, assume the child SHALL constraints are discriminators
                        {
                            var discriminatorConstraints = branchConstraint.ChildConstraints.Where(y => y.Conformance == "SHALL");
                            var singleValueDiscriminators = discriminatorConstraints.Where(y => !string.IsNullOrEmpty(y.Value));

                            // If there are constraints that have specific single-value bindings, prefer those
                            if (singleValueDiscriminators.Count() > 0 && singleValueDiscriminators.Count() != discriminatorConstraints.Count())
                                discriminatorConstraints = singleValueDiscriminators;

                            newElementDef.Slicing.Discriminator = (from d in discriminatorConstraints
                                                                   where !d.IsPrimitive
                                                                   select new ElementDefinition.DiscriminatorComponent()
                                                                   {
                                                                       Type = ElementDefinition.DiscriminatorType.Value,
                                                                       Path = d.GetElementPath(template.PrimaryContextType, branchConstraint)
                                                                   }).ToList();
                        }
                    }

                    // Find where to insert the slice in the element list
                    var firstElement = fhirStructureDef.Differential.Element.First(y => y.Path == sliceGroup.Key);
                    var firstElementIndex = fhirStructureDef.Differential.Element.IndexOf(firstElement);
                    differential.Element.Insert(firstElementIndex, newElementDef);
                    currentSliceGroupCount++;
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
            var resourceType = template.TemplateType.RootContextType;

            if (this.baseProfiles.ContainsKey(resourceType))
                return this.baseProfiles[resourceType];

            var baseProfile = ProfileHelper.GetProfile(resourceType);
            this.baseProfiles[resourceType] = baseProfile;
            return baseProfile;
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

    public static class TemplateConstraintExtensions
    {
        public static bool HasBinding(this TemplateConstraint constraint)
        {
            return !string.IsNullOrEmpty(constraint.Value) ||
                !string.IsNullOrEmpty(constraint.DisplayName) ||
                constraint.ValueSetId != null ||
                constraint.CodeSystemId != null;
        }

        public static bool HasContainedTemplates(this TemplateConstraint constraint)
        {
            return constraint.References.Count > 0;
        }
    }
}
