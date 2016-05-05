extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using ValueSet = Trifolia.DB.ValueSet;
using Trifolia.Web.Formatters.FHIR.DSTU2;
using Trifolia.Generation.XML.FHIR.DSTU2;
using Trifolia.Generation.IG.ConstraintGeneration;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using FhirValueSet = fhir_dstu2.Hl7.Fhir.Model.ValueSet;
using FhirConformance = fhir_dstu2.Hl7.Fhir.Model.Conformance;
using fhir_dstu2.Hl7.Fhir.Serialization;
using fhir_dstu2.Hl7.Fhir.Specification.Navigation;
using System.Web;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    public class FHIR2StructureDefinitionController : TrifoliaApiController
    {
        private IObjectRepository tdb;
        private const string DEFAULT_IG_NAME = "Unowned FHIR DSTU2 Profiles";
        private Dictionary<string, StructureDefinition> baseProfiles = new Dictionary<string, StructureDefinition>();
        private Dictionary<ImplementationGuide, IGSettingsManager> allIgSettings = new Dictionary<ImplementationGuide, IGSettingsManager>();

        #region Constructors

        public FHIR2StructureDefinitionController(IObjectRepository tdb, HttpRequestMessage request = null)
        {
            this.tdb = tdb;

            // NOTE: This is for unit testing only
            if (request != null)
                this.Request = request;
        }

        public FHIR2StructureDefinitionController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        private void CreateTemplateConstraint(ElementDefinition elementDef, Template template, TemplateConstraint parentConstraint = null)
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

        private Extension Convert(TemplateExtension extension)
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

        private Template Convert(StructureDefinition strucDef, Template template = null)
        {
            if (string.IsNullOrEmpty(strucDef.ConstrainedType))
                throw new Exception("StructureDefinition.constrainedType is required");

            if (template == null)
            {
                ImplementationGuide unassignedImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y =>
                    y.Name == DEFAULT_IG_NAME &&
                    y.ImplementationGuideType.Name == ImplementationGuideType.FHIR_DSTU2_NAME);

                if (unassignedImplementationGuide == null)
                {
                    unassignedImplementationGuide = new ImplementationGuide()
                    {
                        Name = DEFAULT_IG_NAME,
                        ImplementationGuideType = this.tdb.ImplementationGuideTypes.Single(y => y.Name == ImplementationGuideType.FHIR_DSTU2_NAME),
                        Organization = this.tdb.Organizations.Single(y => y.Name == Shared.DEFAULT_ORG_NAME)
                    };
                    this.tdb.ImplementationGuides.AddObject(unassignedImplementationGuide);
                }

                template = new Template()
                {
                    OwningImplementationGuide = unassignedImplementationGuide,
                    ImplementationGuideType = this.tdb.ImplementationGuideTypes.Single(y => y.Name == ImplementationGuideType.FHIR_DSTU2_NAME),
                    Author = this.tdb.Users.Single(y => y.UserName == Shared.DEFAULT_USER_NAME && y.Organization.Name == Shared.DEFAULT_ORG_NAME),
                    Organization = this.tdb.Organizations.Single(y => y.Name == Shared.DEFAULT_ORG_NAME),
                    IsOpen = true
                };
            }

            // Name
            if (template.Name != strucDef.Name)
                template.Name = strucDef.Name;

            // Descrition
            if (template.Description != strucDef.Description)
                template.Description = strucDef.Description;

            // Identifier -> Oid
            string identifier = strucDef.Url;

            if (string.IsNullOrEmpty(identifier))
                identifier = string.Format("https://trifolia.lantanagroup.com/Generated/{0}", Guid.NewGuid());

            if (template.Oid != identifier)
                template.Oid = identifier;

            // ConstrainedType -> Template Type
            TemplateType templateType = this.tdb.TemplateTypes.SingleOrDefault(y =>
                y.ImplementationGuideType.Name == ImplementationGuideType.FHIR_DSTU2_NAME &&
                y.RootContextType == strucDef.ConstrainedType);

            if (templateType == null)
                throw new Exception("Could not find Template Type for " + strucDef.ConstrainedType);

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

        private void CreateElementDefinition(
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
                Path = constraint.GetElementPath(strucDef.ConstrainedType),
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
                newElementDef.Binding = new ElementDefinition.ElementDefinitionBindingComponent()
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
                        Profile = new List<string>(new string[] { constraint.ContainedTemplate.Oid })
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

        public StructureDefinition Convert(Template template, SummaryType? summaryType = null)
        {
            StructureDefinition fhirStructureDef = new StructureDefinition()
            {
                Name = template.Name,
                Description = template.Description,
                Kind = StructureDefinition.StructureDefinitionKind.Resource,
                Url = template.Oid,
                ConstrainedType = template.TemplateType.RootContextType,
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

                var newContact = new StructureDefinition.StructureDefinitionContactComponent();
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
                fhirStructureDef.Base = string.Format("StructureDefinition/{0}", template.ImpliedTemplate.Id);
            else
                fhirStructureDef.Base = string.Format("http://hl7.org/fhir/StructureDefinition/{0}", template.TemplateType.RootContextType);

            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current, template.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(template.PrimaryContextType);

            // Constraints
            if (summaryType == null || summaryType == SummaryType.Data)
            {
                fhirStructureDef.Differential = new StructureDefinition.StructureDefinitionDifferentialComponent();

                // Add base element for resource
                fhirStructureDef.Differential.Element.Add(new ElementDefinition()
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
                        newElementDef.Slicing = new ElementDefinition.ElementDefinitionSlicingComponent()
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
                    fhirStructureDef.Differential.Element.Insert(firstElementIndex, newElementDef);
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

            string profileVirtualLocation = VirtualPathUtility.Combine("~/App_Data/FHIR/DSTU2/", template.TemplateType.RootContextType.ToLower() + ".profile.xml");
            string profileLocation = HttpContext.Current.Server.MapPath(profileVirtualLocation);

            if (System.IO.File.Exists(profileLocation))
            {
                var profile = (StructureDefinition)FhirParser.ParseResourceFromXml(System.IO.File.ReadAllText(profileLocation));
                this.baseProfiles.Add(template.TemplateType.RootContextType, profile);
                return profile;
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

        /// <summary>
        /// Gets a specific profile and converts it to a StructureDefinition resource.
        /// </summary>
        /// <param name="templateId">The id of the profile/template to get</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_LIST">Only users with the ability to list templates/profiles can execute this operation</permission>
        [HttpGet]
        [Route("api/FHIR2/StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IHttpActionResult GetTemplate(
            [FromUri] string templateId,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == this.Request.RequestUri.AbsoluteUri || y.Id.ToString() == templateId);

            // Return an operation outcome indicating that the profile was not found
            if (template == null)
            {
                OperationOutcome oo = new OperationOutcome();
                oo.Issue.Add(new OperationOutcome.OperationOutcomeIssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Fatal,
                    Diagnostics = "Profile was not found"
                });

                return Content<OperationOutcome>(HttpStatusCode.NotFound, oo);
            }

            // Redirect the user to the Trifolia web interface if an acceptable format is text/html, and no format or summary was specified
            if (Request.Headers.Accept.Any(y => y.MediaType == "text/html") && summary == null)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Location", Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/TemplateManagement/View/Id/" + template.Id);

                OperationOutcome redirectOutcome = new OperationOutcome();
                redirectOutcome.Issue.Add(new OperationOutcome.OperationOutcomeIssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Diagnostics = "Detecting browser-based request, redirecting to Trifolia web interface."
                });

                return Content<OperationOutcome>(HttpStatusCode.Redirect, redirectOutcome, headers);
            }

            if (template.TemplateType.ImplementationGuideType.Name != ImplementationGuideType.FHIR_DSTU2_NAME)
                throw new FormatException(App_GlobalResources.TrifoliaLang.TemplateNotFHIRDSTU2);

            if (!CheckPoint.Instance.GrantViewTemplate(template.Id))
                throw new UnauthorizedAccessException();

            StructureDefinition strucDef = Convert(template, summary);
            return Content<StructureDefinition>(HttpStatusCode.OK, strucDef);
        }

        /// <summary>
        /// Searches for profiles within Trifolia and returns them as StructureDefinition resources within a Bundle.
        /// </summary>
        /// <param name="templateId">The id of a template/profile to search for and return in the bundle</param>
        /// <param name="name">The name of a template/profile to search for. Only templates/profiles that contain this specified name will be returned.</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.Bundle</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_LIST">Only users with the ability to list templates/profiles can execute this operation</permission>
        [HttpGet]
        [Route("api/FHIR2/StructureDefinition")]
        [Route("api/FHIR2/StructureDefinition/_search")]
        [SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IHttpActionResult GetTemplates(
            [FromUri(Name = "_id")] int? templateId = null,
            [FromUri(Name = "name")] string name = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            var templates = this.tdb.Templates.Where(y => y.TemplateType.ImplementationGuideType.Name == ImplementationGuideType.FHIR_DSTU2_NAME);

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                User currentUser = CheckPoint.Instance.GetUser(this.tdb);
                templates = (from t in templates
                             join vtp in this.tdb.ViewTemplatePermissions on t.Id equals vtp.TemplateId
                             where vtp.UserId == currentUser.Id
                             select t);
            }

            if (templateId != null)
                templates = templates.Where(y => y.Id == templateId);

            if (!string.IsNullOrEmpty(name))
                templates = templates.Where(y => y.Name.ToLower().Contains(name.ToLower()));

            Bundle bundle = new Bundle()
            {
                Type = Bundle.BundleType.BatchResponse
            };

            foreach (var template in templates)
            {
                bool isMatch = true;
                StructureDefinition strucDef = Convert(template, summary);
                var fullUrl = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    template.Id);

                // Skip adding the structure definition to the response if a criteria rules it out
                foreach (var queryParam in this.Request.GetQueryNameValuePairs())
                {
                    if (queryParam.Key == "_id" || queryParam.Key == "name" || queryParam.Key == "_format" || queryParam.Key == "_summary")
                        continue;

                    if (queryParam.Key.Contains("."))
                        throw new NotSupportedException(App_GlobalResources.TrifoliaLang.FHIRSearchCriteriaNotSupported);

                    var propertyDef = strucDef.GetType().GetProperty(queryParam.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (propertyDef == null)
                        continue;

                    var value = propertyDef.GetValue(strucDef);
                    var valueString = value != null ? value.ToString() : string.Empty;

                    if (valueString != queryParam.Value)
                        isMatch = false;
                }

                if (isMatch)
                    bundle.AddResourceEntry(strucDef, fullUrl);
            }

            return Content<Bundle>(HttpStatusCode.OK, bundle);
        }

        public Template CreateTemplate(StructureDefinition strucDef)
        {
            Template template = Convert(strucDef);
            this.tdb.Templates.AddObject(template);
            return template;
        }

        /// <summary>
        /// Creates a template/profile from the specified StructureDefinition
        /// </summary>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_EDIT">Only users with the ability to edit templates/profiles can execute this operation</permission>
        [HttpPost]
        [Route("api/FHIR2/StructureDefinition")]
        [SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public IHttpActionResult CreateStructureDefinition(
            [FromBody] StructureDefinition strucDef)
        {
            if (!string.IsNullOrEmpty(strucDef.Id))
            {
                OperationOutcome error = new OperationOutcome();
                error.Issue.Add(new OperationOutcome.OperationOutcomeIssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Diagnostics = App_GlobalResources.TrifoliaLang.CreateFHIR2TemplateFailedDuplicateId
                });
                return Content<OperationOutcome>(HttpStatusCode.BadRequest, error);
            }

            var template = CreateTemplate(strucDef);
            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    template.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            StructureDefinition newStrucDef = Convert(template);
            return Content<StructureDefinition>(HttpStatusCode.Created, newStrucDef, headers);
        }

        /// <summary>
        /// Updates an existing template/profile based on the specified StructureDefinition
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_EDIT">Only users with the ability to edit templates/profiles can execute this operation</permission>
        [HttpPut]
        [Route("api/FHIR2/StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public IHttpActionResult UpdateStructureDefinition(
            [FromBody] StructureDefinition strucDef,
            [FromUri] int templateId)
        {
            Template existingTemplate = this.tdb.Templates.SingleOrDefault(y => y.Id == templateId);

            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new UnauthorizedAccessException();

            Template updatedTemplate = Convert(strucDef, existingTemplate);

            if (existingTemplate == null)
                this.tdb.Templates.AddObject(updatedTemplate);

            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    updatedTemplate.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            StructureDefinition updatedStrucDef = Convert(updatedTemplate);
            return Content<Resource>((existingTemplate == null ? HttpStatusCode.Created : HttpStatusCode.OK), updatedStrucDef, headers);
        }

        /// <summary>
        /// Deletes the specified profile/template. This is a permanent deletion, and cannot be restored via _history.
        /// </summary>
        /// <param name="templateId">The id of the profile/template to delete</param>
        /// <returns>Hl7.Fhir.Model.OperationOutcome</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_DELETE">Only users with the ability to delete templates/profiles can execute this operation</permission>
        [HttpDelete]
        [Route("api/FHIR2/StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_DELETE)]
        public IHttpActionResult DeleteStructureDefinition(
            [FromUri] int templateId)
        {
            Template template = null;
            OperationOutcome outcome = new OperationOutcome();
            outcome.Id = "ok";

            try
            {
                template = this.tdb.Templates.Single(y => y.Id == templateId);
            }
            catch
            {
                OperationOutcome error = new OperationOutcome();
                error.Issue.Add(new OperationOutcome.OperationOutcomeIssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Diagnostics = App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat
                });
                return Content<OperationOutcome>(HttpStatusCode.BadRequest, error);
            }

            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new UnauthorizedAccessException();

            template.Delete(this.tdb, null);

            return Content(HttpStatusCode.NoContent, outcome);
        }
    }
}
