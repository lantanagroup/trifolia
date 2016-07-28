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
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using StructureDefinition = fhir_stu3.Hl7.Fhir.Model.StructureDefinition;

namespace Trifolia.Export.FHIR.STU3
{
    public class StructureDefinitionExporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private string baseProfilePath;
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private ImplementationGuideType implementationGuideType;

        public StructureDefinitionExporter(IObjectRepository tdb, string baseProfilePath, string scheme, string authority)
        {
            this.tdb = tdb;
            this.baseProfilePath = baseProfilePath;
            this.scheme = scheme;
            this.authority = authority;
            this.implementationGuideType = Shared.GetImplementationGuideType(this.tdb, true);
        }

        public void CreateTemplateConstraint(ElementDefinition elementDef, Template template, TemplateConstraint parentConstraint = null)
        {
            string context = elementDef.Path.Split('.').Last();
            TemplateConstraint constraint = null;

            if (parentConstraint != null)
                constraint = parentConstraint.ChildConstraints.SingleOrDefault(y => y.Context == context);
            else
                constraint = template.ChildConstraints.SingleOrDefault(y => y.ParentConstraint == null && y.Context == context);

            if (constraint == null)
            {
                constraint = new TemplateConstraint();
                constraint.Context = context;

                if (parentConstraint != null)
                    constraint.ParentConstraint = parentConstraint;

                template.ChildConstraints.Add(constraint);
            }

            // Cardinality
            string cardinality = string.Format("{0}..{1}",
                elementDef.Min == null ? 0 : elementDef.Min.Value,
                string.IsNullOrEmpty(elementDef.Max) ? "*" : elementDef.Max);

            if (constraint.Cardinality != cardinality)
                constraint.Cardinality = cardinality;

            // Conformance
            string conformance = elementDef.Min != null && elementDef.Min.Value > 0 ? "SHALL" : "SHOULD";

            if (constraint.Conformance != conformance)
                constraint.Conformance = conformance;
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

        public Template Convert(StructureDefinition strucDef, Template template = null)
        {
            if (string.IsNullOrEmpty(strucDef.Type))
                throw new Exception("StructureDefinition.type is required");

            string strucDefDescription = strucDef.Description != null ? strucDef.Description.Value : null;

            if (template == null)
            {
                ImplementationGuide unassignedImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y =>
                    y.Name == Shared.DEFAULT_IG_NAME &&
                    y.ImplementationGuideTypeId == this.implementationGuideType.Id);

                if (unassignedImplementationGuide == null)
                {
                    unassignedImplementationGuide = new ImplementationGuide()
                    {
                        Name = Shared.DEFAULT_IG_NAME,
                        ImplementationGuideType = this.implementationGuideType,
                        Organization = this.tdb.Organizations.Single(y => y.Name == Shared.DEFAULT_ORG_NAME)
                    };
                    this.tdb.ImplementationGuides.AddObject(unassignedImplementationGuide);
                }

                template = new Template()
                {
                    OwningImplementationGuide = unassignedImplementationGuide,
                    ImplementationGuideType = this.implementationGuideType,
                    Author = this.tdb.Users.Single(y => y.UserName == Shared.DEFAULT_USER_NAME && y.Organization.Name == Shared.DEFAULT_ORG_NAME),
                    Organization = this.tdb.Organizations.Single(y => y.Name == Shared.DEFAULT_ORG_NAME),
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
                identifier = string.Format(Shared.STRUCDEF_NEW_IDENTIFIER_FORMAT, Guid.NewGuid());

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
            var fhirStructureDef = new fhir_stu3.Hl7.Fhir.Model.StructureDefinition()
            {
                Name = template.Name,
                Description = new Markdown(template.Description),
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
                    Path = template.TemplateType.RootContextType
                });

                foreach (var constraint in template.ChildConstraints.Where(y => y.ParentConstraint == null).OrderBy(y => y.Order))
                {
                    var schemaObject = schema.Children.SingleOrDefault(y => y.Name == constraint.Context);
                    CreateElementDefinition(fhirStructureDef, constraint, schemaObject);
                }

                // Slices
                var slices = template.ChildConstraints.Where(y => y.IsBranch);
                var sliceGroups = slices.GroupBy(y => y.GetElementPath(template.TemplateType.RootContextType));

                foreach (var sliceGroup in sliceGroups)
                {
                    ElementDefinition newElementDef = new ElementDefinition();
                    newElementDef.Path = sliceGroup.Key;

                    foreach (var branchConstraint in sliceGroup)
                    {
                        var branchIdentifiers = branchConstraint.ChildConstraints.Where(y => y.IsBranchIdentifier);
                        newElementDef.Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Discriminator = (from bi in branchIdentifiers
                                             select bi.GetElementPath(template.TemplateType.RootContextType)),
                            Rules = template.IsOpen ? ElementDefinition.SlicingRules.Open : ElementDefinition.SlicingRules.Closed
                        };

                        // If no discriminators are specified, assume the child SHALL constraints are discriminators
                        if (newElementDef.Slicing.Discriminator.Count() == 0)
                        {
                            newElementDef.Slicing.Discriminator = (from cc in branchConstraint.ChildConstraints
                                                                   where cc.Conformance == "SHALL"
                                                                   select cc.GetElementPath(template.TemplateType.RootContextType));
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
            string profileLocation = System.IO.Path.Combine(this.baseProfilePath, "profiles-resources.xml");

            Bundle profileBundle = (Bundle)FhirParser.ParseFromXml(File.ReadAllText(profileLocation));

            foreach (var entry in profileBundle.Entry)
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
