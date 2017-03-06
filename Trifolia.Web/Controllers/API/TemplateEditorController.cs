using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using Trifolia.Web.Models.TemplateEditing;
using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Shared;
using Trifolia.Generation.IG;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Web.Extensions;

namespace Trifolia.Web.Controllers.API
{
    [SecurableAction(SecurableNames.TEMPLATE_EDIT)]
    public class TemplateEditorController : ApiController
    {
        #region Constructors

        private IObjectRepository tdb;

        public TemplateEditorController()
            : this(DBContext.Create())
        {

        }

        public TemplateEditorController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        /// <summary>
        /// Gets all extensions that the specified implementation guide can use
        /// If the specified implementation guide is not a FHIR IG Type, an empty list is returned
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Trifolia.Web.Models.TemplateEditing.TemplateItem&gt;</returns>
        [HttpGet, Route("api/Template/Edit/{implementationGuideId}/Extension"), SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public IEnumerable<TemplateItem> GetAvailableExtensions(int implementationGuideId)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            if (implementationGuide.ImplementationGuideType.SchemaURI != ImplementationGuideType.FHIR_NS)
                return new List<TemplateItem>();

            User user = CheckPoint.Instance.GetUser(this.tdb);
            var templates = (from t in this.tdb.Templates
                         join vtp in this.tdb.ViewTemplatePermissions on t.Id equals vtp.TemplateId
                         where t.OwningImplementationGuide.ImplementationGuideTypeId == implementationGuide.ImplementationGuideTypeId
                           && t.PrimaryContextType == "Extension"
                           && vtp.UserId == user.Id
                         select t);
            var items = (from t in templates.Distinct().AsEnumerable()
                         select new TemplateItem(t));

            return items;
        }

        [HttpGet, Route("api/Template/Edit/{implementationGuideId}/Category"), SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public List<string> GetImplementationGuideCategories(int implementationGuideId)
        {
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            List<string> returnCategories = new List<string>();

            var categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);

            if (!string.IsNullOrEmpty(categories))
            {
                var categoriesSplit = categories.Split(',');

                foreach (string category in categoriesSplit)
                {
                    returnCategories.Add(category.Replace("###", ","));
                }
            }

            return returnCategories;
        }

        [HttpGet, Route("api/Template/Edit/{templateId}/MetaData")]
        public TemplateMetaDataModel GetMetaData(int templateId)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException("You do not have permission to edit this template");
            
            DB.Template lTemplate = tdb.Templates.Single(t => t.Id == templateId);

            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, lTemplate.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(lTemplate.PrimaryContextType);

            TemplateMetaDataModel lViewModel = new TemplateMetaDataModel()
            {
                Bookmark = lTemplate.Bookmark,
                PrimaryContext = lTemplate.PrimaryContext,
                PrimaryContextType = lTemplate.PrimaryContextType,
                Description = lTemplate.Description,
                Notes = lTemplate.Notes,
                Id = lTemplate.Id,
                OwningImplementationGuideId = lTemplate.OwningImplementationGuideId,
                ImpliedTemplateId = lTemplate.ImpliedTemplateId,
                IsOpen = lTemplate.IsOpen,
                Name = lTemplate.Name,
                Oid = lTemplate.Oid,
                StatusId = lTemplate.StatusId,
                TemplateTypeId = lTemplate.TemplateTypeId,
                AuthorId = lTemplate.AuthorId,
                Author = string.Format("{0} {1}", lTemplate.Author.FirstName, lTemplate.Author.LastName),
                OrganizationName = lTemplate.OrganizationName,
                MoveUrl = lTemplate.GetMoveUrl(),
                TemplateTypeAbbreviation = lTemplate.TemplateType.GetAbbreviation(),
                Locked = lTemplate.OwningImplementationGuide.IsPublished()
            };

            // Parse the validation results for the template
            lViewModel.ValidationResults = (from vr in lTemplate.ValidateTemplate(schema)
                                            select new
                                            {
                                                ConstraintNumber = vr.ConstraintNumber,
                                                Level = vr.Level.ToString(),
                                                Message = vr.Message
                                            });

            lViewModel.Extensions = (from te in lTemplate.Extensions
                                     select new TemplateMetaDataModel.TemplateExtension()
                                     {
                                         Identifier = te.Identifier,
                                         Type = te.Type,
                                         Value = te.Value
                                     });

            if (lTemplate.PreviousVersion != null)
            {
                lViewModel.PreviousVersionLink = "/TemplateManagement/View/" + lTemplate.PreviousVersion.Oid;
                lViewModel.PreviousVersionName = lTemplate.PreviousVersion.Name;
                lViewModel.PreviousVersionOid = lTemplate.PreviousVersion.Oid;
            }

            // Contained By Templates
            List<TemplateMetaDataModel.TemplateReference> containedByTemplates = new List<TemplateMetaDataModel.TemplateReference>();
            lViewModel.ContainedByTemplates = containedByTemplates;

            foreach (var containingConstraint in lTemplate.ContainingConstraints)
            {
                TemplateMetaDataModel.TemplateReference newReference = new TemplateMetaDataModel.TemplateReference()
                {
                    EditUrl = containingConstraint.Template.GetEditUrl(),
                    ViewUrl = containingConstraint.Template.GetViewUrl(),
                    Name = containingConstraint.Template.Name,
                    ImplementationGuide = containingConstraint.Template.OwningImplementationGuide.GetDisplayName()
                };

                containedByTemplates.Add(newReference);
            }

            // Implied By Templates
            List<TemplateMetaDataModel.TemplateReference> impliedByTemplates = new List<TemplateMetaDataModel.TemplateReference>();
            lViewModel.ImpliedByTemplates = impliedByTemplates;

            foreach (Template implyingTemplate in this.tdb.Templates.Where(y => y.ImpliedTemplateId == templateId))
            {
                TemplateMetaDataModel.TemplateReference newReference = new TemplateMetaDataModel.TemplateReference()
                {
                    EditUrl = implyingTemplate.GetEditUrl(),
                    ViewUrl = implyingTemplate.GetViewUrl(),
                    Name = implyingTemplate.Name,
                    ImplementationGuide = implyingTemplate.OwningImplementationGuide.GetDisplayName()
                };

                impliedByTemplates.Add(newReference);
            }

            return lViewModel;
        }

        [HttpGet, Route("api/Template/Edit/{templateId}/Constraint")]
        public IEnumerable<ConstraintModel> GetConstraints(int templateId)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException("You do not have permission to edit this template");

            Template template = this.tdb.Templates.Single(y => y.Id == templateId);
            return GetConstraints(this.tdb, template);
        }

        private IEnumerable<ConstraintModel> GetConstraints(IObjectRepository tdb, Template template)
        {
            IGSettingsManager igSettings = new IGSettingsManager(tdb, template.OwningImplementationGuideId);

            var constraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null);
            List<ConstraintModel> constraintModels = new List<ConstraintModel>();

            foreach (TemplateConstraint rootConstraint in constraints.OrderBy(y => y.Order))
            {
                var newConstraintModel = CreateConstraintModel(rootConstraint, igSettings);
                constraintModels.Add(newConstraintModel);
            }

            return constraintModels;
        }

        private ConstraintModel CreateConstraintModel(TemplateConstraint constraint, IGSettingsManager igSettings)
        {
            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, constraint);

            var newConstraintModel = new ConstraintModel()
            {
                Id = constraint.Id,
                Number = constraint.Number.Value,
                DisplayNumber = constraint.DisplayNumber,
                IsNew = false,
                Context = constraint.Context,
                Conformance = constraint.Conformance,
                Cardinality = constraint.Cardinality,
                DataType = constraint.DataType == null ? string.Empty : constraint.DataType,
                IsBranch = constraint.IsBranch,
                IsBranchIdentifier = constraint.IsBranchIdentifier,
                Description = constraint.Description,
                Notes = constraint.Notes,
                Label = constraint.Label,
                PrimitiveText = constraint.PrimitiveText,
                Value = constraint.Value,
                ValueDisplayName = constraint.ValueDisplayName,
                ValueSetId = constraint.ValueSetId,
                ValueSetDate = constraint.ValueSetDate,
                ValueCodeSystemId = constraint.CodeSystemId,
                IsPrimitive = constraint.IsPrimitive,
                ContainedTemplateId = constraint.ContainedTemplateId,
                IsHeading = constraint.IsHeading,
                HeadingDescription = constraint.HeadingDescription,
                IsInheritable = constraint.IsInheritable,
                IsSchRooted = constraint.IsSchRooted,
                ValueConformance = constraint.ValueConformance,
                Schematron = constraint.Schematron,
                Category = constraint.Category,
                IsModifier = constraint.IsModifier,
                MustSupport = constraint.MustSupport,
                IsChoice = constraint.IsChoice,

                NarrativeProseHtml = fc.GetPlainText(false, false, false)
            };

            if (constraint.IsStatic == true)
                newConstraintModel.Binding = "STATIC";
            else if (constraint.IsStatic == false)
                newConstraintModel.Binding = "DYNAMIC";
            else
                newConstraintModel.Binding = "DEFAULT";

            List<ConstraintModel> children = newConstraintModel.Children as List<ConstraintModel>;
            foreach (TemplateConstraint childConstraint in constraint.ChildConstraints.OrderBy(y => y.Order))
            {
                var newChildConstraintModel = CreateConstraintModel(childConstraint, igSettings);
                children.Add(newChildConstraintModel);
            }

            return newConstraintModel;
        }

        [HttpGet, Route("api/Template/Edit/Schema/{implementationGuideId}")]
        public IEnumerable<SchemaNode> GetSchemaNodesByType(int implementationGuideId, string parentType = null, string path = null, bool includeAttributes = true)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to view this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);
            List<SimpleSchema.SchemaObject> children = null;

            if (schema == null)
            {
                Logging.Log.For(this).Error("Request for schema nodes by type did not find a schema for implementation guide id " + implementationGuideId);
                return new List<SchemaNode>();
            }

            if (!string.IsNullOrEmpty(parentType))
            {
                schema = schema.GetSchemaFromContext(parentType);

                if (schema != null)
                    children = schema.Children;
                else
                    Logging.Log.For(this).Warn("Schema for implementation guide type " + ig.ImplementationGuideType.Name + " did not find a parent type of " + parentType);
            }
            else
            {
                children = schema.Children[0].Children;
            }

            if (children == null)
                return new List<SchemaNode>();

            if (!string.IsNullOrEmpty(path))
            {
                string[] pathSplit = path.Split('/');

                foreach (string nextPath in pathSplit)
                {
                    var child = children.SingleOrDefault(y => y.Name == nextPath);

                    if (child == null)
                        throw new ArgumentException("Path specified could not be found within type");

                    children = child.Children;
                }
            }

            var ret = (from s in children
                       where includeAttributes || !s.IsAttribute
                       select new SchemaNode()
                       {
                           Context = s.IsAttribute ? "@" + s.Name : s.Name,
                           Conformance = s.Conformance,
                           Cardinality = s.Cardinality,
                           DataType = s.DataType,
                           IsChoice = s.IsChoice,
                           HasChildren = s.Children.Where(y => includeAttributes || !y.IsAttribute).Count() > 0
                       });

            return ret;
        }

        [HttpGet, Route("api/Template/Edit/PublishStatus/{implementationGuideId}")]
        public IEnumerable<LookupPublishStatus> GetPublishStatuses(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to view this implementation guide");

            LookupPublishStatus status = new LookupPublishStatus();

            return (from ps in status.GetPublishStatusesForTemplate(implementationGuideId)
                    select new LookupPublishStatus()
                    {
                        Id = ps.Id,
                        Name = ps.Status
                    });
        }

        [HttpGet, Route("api/Template/Edit/DerivedType/{implementationGuideId}/{dataType}")]
        public IEnumerable<string> GetDerivedTypes(int implementationGuideId, string dataType)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to view this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);
            var derivedTypes = schema.GetDerivedTypes(dataType);

            if (derivedTypes != null)
                return derivedTypes.Where(y => y.Name != dataType).Select(y => y.Name);

            return null;
        }

        [HttpPost, Route("api/Template/Edit/Prose")]
        public string GetNarrative(ConstraintModel constraint)
        {
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb);
            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igSettings, constraint);
            fc.HasChildren = true;

            return fc.GetPlainText(false, false, true);
        }

        [HttpGet, Route("api/Template/Edit/List")]
        public IEnumerable<TemplateItem> GetTemplates()
        {
            IEnumerable<Template> templates = this.tdb.Templates;

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);
                templates = (from vtp in this.tdb.ViewTemplatePermissions
                             join t in this.tdb.Templates on vtp.TemplateId equals t.Id
                             where vtp.Permission == "View" && vtp.UserId == currentUser.Id
                             select t);
            }

            return (from t in templates
                    select new TemplateItem()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Oid = t.Oid,
                        PrimaryContextType = t.PrimaryContextType
                    })
                    .OrderBy(y => y.Name)
                    .ThenBy(y => y.Oid);
        }

        [HttpPost, Route("api/Template/Edit/Save")]
        public SaveResponse Save(SaveModel model)
        {
            SaveResponse response = new SaveResponse();

            using (IObjectRepository tdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                if (!CheckPoint.Instance.GrantEditImplementationGuide(model.Template.OwningImplementationGuideId))
                    throw new AuthorizationException("You do not have authorization to edit templates associated with the selected implementation guide");

                if (model.Template.Id != null && !CheckPoint.Instance.GrantEditTemplate(model.Template.Id.Value))
                    throw new AuthorizationException("You do not have permission to edit this template");

                Template template = SaveTemplate(tdb, model.Template);

                // Remove the specified constraints before creating/updating the other constraints
                this.RemoveConstraints(tdb, model.RemovedConstraints);

                // Create/update constraints
                this.SaveConstraints(tdb, template, model.Constraints);

                var duplicateNumbers = (from tcc in template.ChildConstraints
                                        join tc in tdb.TemplateConstraints on tcc.Number equals tc.Number
                                        join t in tdb.Templates on tc.TemplateId equals t.Id
                                        where tcc.Id != tc.Id && t.OwningImplementationGuideId == template.OwningImplementationGuideId
                                        select tcc.Number).ToList();

                if (duplicateNumbers.Count > 0)
                {
                    response.Error = string.Format("The following constraints have duplicate numbers: {0}", string.Join(", ", duplicateNumbers));
                }
                else
                {
                    tdb.SaveChanges();

                    // TODO: Check that ids have been updated template and constraints

                    /*
                    tdb.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, template);

                    foreach (var constraint in template.ChildConstraints)
                    {
                        tdb.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, constraint);
                    }
                    */

                    response.TemplateId = template.Id;
                    response.Constraints = GetConstraints(tdb, template);

                    SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, template.ImplementationGuideType);
                    schema = schema.GetSchemaFromContext(template.PrimaryContextType);

                    response.ValidationResults = (from vr in template.ValidateTemplate(schema)
                                                    select new
                                                    {
                                                        ConstraintNumber = vr.ConstraintNumber,
                                                        Level = vr.Level.ToString(),
                                                        Message = vr.Message
                                                    });
                }
            }

            return response;
        }

        /// <summary>
        /// Converts the TemplateMetaDataModel into the Template (EF) model.
        /// </summary>
        /// <remarks>
        /// Each property is checked to determine if it is different before updating the property, otherwise
        /// it registers as a change event when auditing, regardless if the value is different.
        /// </remarks>
        private Template SaveTemplate(IObjectRepository tdb, TemplateMetaDataModel model)
        {
            Template template = null;

            // Create the initial template object and add it to the appropriate list (if it is new)
            if (model.Id != null)
            {
                template = tdb.Templates.Single(y => y.Id == model.Id);
            }
            else
            {
                template = new Template();
                tdb.Templates.Add(template);
            }

            // Set the properties
            if (template.Author == null)
            {
                User currentUser = CheckPoint.Instance.GetUser(tdb);
                template.AuthorId = currentUser.Id;
            }
            else if (template.AuthorId != model.AuthorId)
            {
                template.AuthorId = model.AuthorId;
            }

            ImplementationGuide ig = tdb.ImplementationGuides.Single(y => y.Id == model.OwningImplementationGuideId);

            if (template.ImplementationGuideTypeId != ig.ImplementationGuideTypeId)
                template.ImplementationGuideTypeId = ig.ImplementationGuideTypeId;

            

            if (template.Name != model.Name)
                template.Name = model.Name;

            if (template.Oid != model.Oid)
                template.Oid = model.Oid;

            if (template.Bookmark != model.Bookmark)
                template.Bookmark = model.Bookmark;

            if (template.OwningImplementationGuideId != model.OwningImplementationGuideId)
                template.OwningImplementationGuideId = model.OwningImplementationGuideId;

            if (template.TemplateTypeId != model.TemplateTypeId)
                template.TemplateTypeId = model.TemplateTypeId;

            if (template.PrimaryContext != model.PrimaryContext)
                template.PrimaryContext = model.PrimaryContext;

            if (template.PrimaryContextType != model.PrimaryContextType)
                template.PrimaryContextType = model.PrimaryContextType;

            if (template.ImpliedTemplateId != model.ImpliedTemplateId)
                template.ImpliedTemplateId = model.ImpliedTemplateId;

            if (template.Description != model.Description)
                template.Description = model.Description;

            if (template.Notes != model.Notes)
                template.Notes = model.Notes;

            if (template.StatusId != model.StatusId)
                template.StatusId = model.StatusId;

            if (template.IsOpen != model.IsOpen)
                template.IsOpen = model.IsOpen;
            
            // Remove Extensions
            var extensions = template.Extensions.ToList();
            foreach (var extension in extensions)
            {
                var foundExtension = model.Extensions.SingleOrDefault(y => y.Identifier == extension.Identifier);

                if (foundExtension == null)
                    tdb.TemplateExtensions.Remove(extension);
            }

            // Add/Update Extensions
            foreach (var extensionModel in model.Extensions)
            {
                var extension = template.Extensions.SingleOrDefault(y => y.Identifier == extensionModel.Identifier);

                if (extension == null)
                {
                    extension = new TemplateExtension();
                    extension.Identifier = extensionModel.Identifier;
                    template.Extensions.Add(extension);
                }

                if (extension.Type != extensionModel.Type)
                    extension.Type = extensionModel.Type;

                if (extension.Value != extensionModel.Value)
                    extension.Value = extensionModel.Value;
            }

            return template;
        }

        /// <summary>
        /// Removes constraints from the database context
        /// </summary>
        private void RemoveConstraints(IObjectRepository tdb, List<ConstraintModel> constraintModels)
        {
            foreach (var constraintModel in constraintModels)
            {
                if (constraintModel.IsNew)
                    continue;

                TemplateConstraint constraint = tdb.TemplateConstraints.Single(y => y.Id == constraintModel.Id);
                tdb.TemplateConstraints.Remove(constraint);

                // Recursively remove child constraints
                this.RemoveConstraints(tdb, constraintModel.Children);
            }
        }

        /// <summary>
        /// Converts ConstraintModel into TemplateConstraint (EF) models.
        /// </summary>
        /// <remarks>
        /// Each property is checked to determine if it is different before updating the property, otherwise
        /// it registers as a change event when auditing, regardless if the value is different.
        /// </remarks>
        private void SaveConstraints(IObjectRepository tdb, Template template, List<ConstraintModel> constraintModels, TemplateConstraint parentConstraint = null)
        {
            TemplateConstraint constraint = null;

            foreach (ConstraintModel constraintModel in constraintModels)
            {
                // Create the constraint object and add it to the appropriate lists (if it is new)
                if (constraintModel.IsNew)
                {
                    constraint = new TemplateConstraint();
                    constraint.Template = template;
                    tdb.TemplateConstraints.Add(constraint);

                    if (parentConstraint != null)
                        constraint.ParentConstraint = parentConstraint;
                }
                else
                {
                    constraint = template.ChildConstraints.Single(y => y.Id == constraintModel.Id);
                }

                // Set the properties
                int order = constraintModels.IndexOf(constraintModel) + 1;
                var dataType = string.IsNullOrEmpty(constraintModel.DataType) || constraintModel.DataType == "DEFAULT" ? null : constraintModel.DataType;

                if (constraint.Order != order)
                    constraint.Order = order;

                if (constraint.Number != constraintModel.Number)
                    constraint.Number = constraintModel.Number;

                if (constraint.DisplayNumber != constraintModel.DisplayNumber)
                    constraint.DisplayNumber = constraintModel.DisplayNumber;

                if (constraint.Context != constraintModel.Context)
                    constraint.Context = constraintModel.Context;

                if (constraint.Conformance != constraintModel.Conformance)
                    constraint.Conformance = constraintModel.Conformance;

                if (constraint.Cardinality != constraintModel.Cardinality)
                    constraint.Cardinality = constraintModel.Cardinality;

                if (constraint.DataType != dataType)
                    constraint.DataType = dataType;

                if (constraint.IsBranch != constraintModel.IsBranch)
                    constraint.IsBranch = constraintModel.IsBranch;

                if (constraint.IsBranchIdentifier != constraintModel.IsBranchIdentifier)
                    constraint.IsBranchIdentifier = constraintModel.IsBranchIdentifier;

                if (constraint.PrimitiveText != constraintModel.PrimitiveText)
                    constraint.PrimitiveText = constraintModel.PrimitiveText;

                if (constraint.ContainedTemplateId != constraintModel.ContainedTemplateId)
                    constraint.ContainedTemplateId = constraintModel.ContainedTemplateId;

                if (constraint.ValueConformance != constraintModel.ValueConformance)
                    constraint.ValueConformance = constraintModel.ValueConformance;

                if (constraint.Value != constraintModel.Value)
                    constraint.Value = constraintModel.Value;

                if (constraint.ValueConformance != constraintModel.ValueConformance)
                    constraint.ValueConformance = constraintModel.ValueConformance;

                if (constraint.DisplayName != constraintModel.ValueDisplayName)
                    constraint.DisplayName = constraintModel.ValueDisplayName;

                if (constraint.ValueSetId != constraintModel.ValueSetId)
                    constraint.ValueSetId = constraintModel.ValueSetId;

                if (constraint.ValueSetDate != constraintModel.ValueSetDate)
                    constraint.ValueSetDate = constraintModel.ValueSetDate;

                if (constraint.CodeSystemId != constraintModel.ValueCodeSystemId)
                    constraint.CodeSystemId = constraintModel.ValueCodeSystemId;

                if (constraint.Description != constraintModel.Description)
                    constraint.Description = constraintModel.Description;

                if (constraint.Notes != constraintModel.Notes)
                    constraint.Notes = constraintModel.Notes;

                if (constraint.Label != constraintModel.Label)
                    constraint.Label = constraintModel.Label;

                if (constraint.IsPrimitive != constraintModel.IsPrimitive)
                    constraint.IsPrimitive = constraintModel.IsPrimitive;

                if (constraint.IsHeading != constraintModel.IsHeading)
                    constraint.IsHeading = constraintModel.IsHeading;

                if (constraint.HeadingDescription != constraintModel.HeadingDescription)
                    constraint.HeadingDescription = constraintModel.HeadingDescription;

                if (constraint.IsSchRooted != constraintModel.IsSchRooted)
                    constraint.IsSchRooted = constraintModel.IsSchRooted;

                if (constraint.IsInheritable != constraintModel.IsInheritable)
                    constraint.IsInheritable = constraintModel.IsInheritable;

                if (constraint.Schematron != constraintModel.Schematron)
                    constraint.Schematron = constraintModel.Schematron;

                bool? isStatic = null;

                if (string.IsNullOrEmpty(constraintModel.Binding))
                    isStatic = null;
                else if (constraintModel.Binding == "STATIC")
                    isStatic = true;
                else if (constraintModel.Binding == "DYNAMIC")
                    isStatic = false;

                if (constraint.IsStatic != isStatic)
                    constraint.IsStatic = isStatic;

                if (constraint.Category != constraintModel.Category)
                    constraint.Category = constraintModel.Category;

                if (constraint.IsModifier != constraintModel.IsModifier)
                    constraint.IsModifier = constraintModel.IsModifier;

                if (constraint.MustSupport != constraintModel.MustSupport)
                    constraint.MustSupport = constraintModel.MustSupport;

                if (constraint.IsChoice != constraintModel.IsChoice)
                    constraint.IsChoice = constraintModel.IsChoice;

                // Recurse through child constraints
                SaveConstraints(tdb, template, constraintModel.Children, constraint);
            }
        }
    }
}
