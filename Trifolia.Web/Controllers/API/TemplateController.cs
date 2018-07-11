using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Export.Versioning;
using Trifolia.Logging;
using Trifolia.Plugins;
using Trifolia.Shared;
using Trifolia.Web.Extensions;
using Trifolia.Web.Models.TemplateManagement;
using Trifolia.Web.Models.User;

namespace Trifolia.Web.Controllers.API
{
    public class TemplateController : ApiController
    {
        private const string DUPLICATE_OID = "The template OID is already in use by another template.";

        private IObjectRepository tdb;

        #region Construct/Dispose

        public TemplateController()
            : this(DBContext.Create())
        {

        }

        public TemplateController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        private Constraint Convert(TemplateConstraint constraint)
        {
            Constraint newConstraint = new Constraint(constraint);

            foreach (var childConstraint in constraint.ChildConstraints)
            {
                newConstraint.ChildConstraints.Add(new Constraint(childConstraint));
            }

            return newConstraint;
        }

        /// <summary>
        /// Gets a hierarchy of all constraints for the specified template, including basic details for the constraint (such as id, context, conformance, cardinality, etc.)
        /// </summary>
        /// <param name="templateId">The id of the template</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Trifolia.Web.Models.TemplateManagement.Constraint&gt;</returns>
        [HttpGet, Route("api/Template/{templateId}/Constraint"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IEnumerable<Constraint> GetTemplateConstraints(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");

            var template = this.tdb.Templates.Single(y => y.Id == templateId);
            var dbConstraints = template.ChildConstraints.AsEnumerable();
            var constraints = (from tc in dbConstraints
                               select Convert(tc));
            return constraints;
        }

        /// <summary>
        /// Finds the internal identifier of a template/profile by the identifier
        /// </summary>
        /// <param name="identifier">The identifier of the template/profile to search for</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;: Returns null/nothing if no template/profile is found. Otherwise, returns the internal Trifolia id (integer) of the template/profile.</returns>
        [HttpGet, Route("api/Template/Find/{identifier}"), SecurableAction]
        public int? FindId(string identifier)
        {
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
                return null;

            if (!CheckPoint.Instance.GrantViewTemplate(template.Id))
                throw new AuthorizationException("You do not have permission to view this template");

            return template.Id;
        }

        /// <summary>
        /// Gets the template by the template's identifier
        /// </summary>
        /// <param name="identifier">The OID, HL7II, or HTTP identifier of the template to get</param>
        [HttpGet, Route("api/Template/Identifier"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public ViewModel GetTemplate(string identifier)
        {
            int templateId = this.tdb.Templates.Where(y => y.Oid == identifier).Select(y => y.Id).FirstOrDefault();

            if (templateId <= 0)
                throw new ArgumentException("Could not find template with id/reference of " + identifier);

            return GetTemplate(templateId);
        }

        /// <summary>
        /// Gets the template by the template's internal id within Trifolia
        /// </summary>
        /// <param name="templateId">The internal id of the template to get</param>
        [HttpGet, Route("api/Template/{templateId}"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public ViewModel GetTemplate(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");
            
            Template template = this.tdb.Templates
                .Include("ChildConstraints")
                .Include("ChildConstraints.ChildConstraints")
                .Include("ChildConstraints.ChildConstraints.ChildConstraints")
                .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints")
                .Include("ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints.ChildConstraints")
                .Single(y => y.Id == templateId);
            IGSettingsManager igManager = new IGSettingsManager(this.tdb, template.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = template.OwningImplementationGuide.ImplementationGuideType.GetPlugin();
            string baseLink = this.Request.RequestUri.GetLeftPart(UriPartial.Authority);
            bool canEditTemplate = CheckPoint.Instance.GrantEditTemplate(template.Id)
                && CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT);
            bool canEditPublishSettings = CheckPoint.Instance.GrantEditTemplate(template.Id) &&
                CheckPoint.Instance.HasSecurables(SecurableNames.PUBLISH_SETTINGS);
            bool canCopy = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_COPY);
            bool canVersion = this.tdb.ImplementationGuides.Count(y => y.PreviousVersionImplementationGuideId == template.OwningImplementationGuideId) > 0;
            bool canDelete = CheckPoint.Instance.GrantEditTemplate(template.Id) &&
                CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_DELETE);
            bool canMove = CheckPoint.Instance.GrantEditTemplate(template.Id) &&
                CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_MOVE);

            ViewModel model = new ViewModel()
            {
                Id = template.Id,
                Author = string.Format("{0} {1} ({2})", template.Author.FirstName, template.Author.LastName, template.Author.Email),
                ImplementationGuideType = template.ImplementationGuideType.Name,
                ImplementationGuideTypeId = template.ImplementationGuideType.Id,
                Description = template.Description.MarkdownToHtml(),
                Notes = template.Notes.MarkdownToHtml(),
                ImplementationGuideId = template.OwningImplementationGuideId,
                ImplementationGuide = template.OwningImplementationGuide.GetDisplayName(),
                ImpliedTemplate = template.ImpliedTemplate != null ? template.ImpliedTemplate.Name : null,
                ImpliedTemplateId = template.ImpliedTemplate != null && CheckPoint.Instance.GrantViewTemplate(template.ImpliedTemplateId.Value) ? template.ImpliedTemplateId : null,
                ImpliedTemplateOid = template.ImpliedTemplate != null ? template.ImpliedTemplate.Oid : null,
                ImpliedTemplateImplementationGuide = template.ImpliedTemplate != null ? template.ImpliedTemplate.OwningImplementationGuide.GetDisplayName() : null,
                IsOpen = template.IsOpen,
                Name = template.Name,
                Oid = template.Oid,
                Bookmark = template.Bookmark,
                Organization = template.OwningImplementationGuide != null && template.OwningImplementationGuide.Organization != null ? template.OwningImplementationGuide.Organization.Name : string.Empty,
                ShowNotes = CheckPoint.Instance.GrantEditTemplate(template.Id),
                CanEdit = canEditTemplate,
                CanEditPublishSettings = canEditPublishSettings,
                CanCopy = canCopy,
                CanVersion = canVersion,
                CanDelete = canDelete,
                CanMove = canMove,
                CanEditGreen = CheckPoint.Instance.HasSecurables(SecurableNames.GREEN_MODEL),
                HasPreviousVersion = template.PreviousVersionTemplateId.HasValue,
                HasGreenModel = template.GreenTemplates.Any()
            };

            PublishStatus lStatus = template.Status;

            if (lStatus == null)
                model.Status = Shared.PublishStatuses.Draft.ToString();
            else
                model.Status = lStatus.Status;

            if (model.HasPreviousVersion)
            {
                Template lPreviousVersion = this.tdb.Templates.Single(t => t.Id == template.PreviousVersionTemplateId.Value);
                model.PreviousVersionTemplateName = string.Format("{0} ({1})", lPreviousVersion.Name, lPreviousVersion.Oid);
                model.PreviousVersionTemplateId = lPreviousVersion.Id;
            }

            if (!string.IsNullOrEmpty(template.PrimaryContext) && template.PrimaryContext.ToLower() != template.TemplateType.RootContext.ToLower())
                model.Type = string.Format("{0} ({1})", template.PrimaryContext, template.TemplateType.Name);
            else
                model.Type = template.TemplateType.Name;

            int constraintCount = 0;
            foreach (TemplateConstraint cDbConstraint in template.ChildConstraints.Where(y => y.Parent == null).OrderBy(y => y.Order))
            {
                ViewModel.Constraint newConstraint = BuildConstraint(baseLink, igManager, igTypePlugin, cDbConstraint, ++constraintCount);
                model.Constraints.Add(newConstraint);
            }

            // Xml samples
            model.Samples = (from s in template.TemplateSamples
                             select new ViewModel.XmlSample()
                             {
                                 Id = s.Id,
                                 Name = s.Name,
                                 Sample = s.XmlSample
                             }).ToList();

            // Build referenced templates
            var containedByTemplates = (from tc in this.tdb.TemplateConstraints
                                        join tcr in this.tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                                        join te in this.tdb.Templates on tc.TemplateId equals te.Id
                                        where tcr.ReferenceIdentifier == template.Oid && tc.TemplateId != template.Id && tcr.ReferenceType == ConstraintReferenceTypes.Template
                                        orderby tc.Conformance, te.Name
                                        select te).Distinct().ToList();
            var containedTemplates = (from ac in template.ChildConstraints
                                      join tcr in this.tdb.TemplateConstraintReferences on ac.Id equals tcr.TemplateConstraintId
                                      join ct in this.tdb.Templates on tcr.ReferenceIdentifier equals ct.Oid
                                      where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                      orderby ct.Name
                                      select ct).Distinct().ToList();
            var implyingTemplates = (from t in this.tdb.Templates
                                     where t.ImpliedTemplateId == template.Id
                                     orderby t.Name
                                     select t).Distinct().ToList();

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                var user = CheckPoint.Instance.GetUser(this.tdb);
                var viewableTemplates = this.tdb.ViewTemplatePermissions.Where(y => y.UserId == user.Id).ToList();

                containedByTemplates = (from t in containedByTemplates
                                        join p in viewableTemplates on t.Id equals p.TemplateId
                                        select t).Distinct().ToList();
                containedTemplates = (from t in containedTemplates
                                      join p in viewableTemplates on t.Id equals p.TemplateId
                                      select t).Distinct().ToList();
                implyingTemplates = (from t in implyingTemplates
                                     join p in viewableTemplates on t.Id equals p.TemplateId
                                     select t).Distinct().ToList();
            }

            model.ContainedByTemplates = (from t in containedByTemplates
                                          select new ViewModel.ReferenceTemplate()
                                          {
                                              Id = t.Id,
                                              ImplementationGuide = t.OwningImplementationGuide.NameWithVersion,
                                              Name = t.Name,
                                              Oid = t.Oid
                                          }).ToList();
            model.ContainedTemplates = (from t in containedTemplates
                                        select new ViewModel.ReferenceTemplate()
                                        {
                                            Id = t.Id,
                                            ImplementationGuide = t.OwningImplementationGuide.NameWithVersion,
                                            Name = t.Name,
                                            Oid = t.Oid
                                        }).ToList();
            model.ImplyingTemplates = (from t in implyingTemplates
                                       select new ViewModel.ReferenceTemplate()
                                       {
                                           Id = t.Id,
                                           ImplementationGuide = t.OwningImplementationGuide.NameWithVersion,
                                           Name = t.Name,
                                           Oid = t.Oid
                                       }).ToList();

            // Build actions
            model.Actions.Add(new ViewModel.ActionItem()
            {
                Url = "/TemplateManagement/List",
                Text = "Back to List"
            });

            if (model.CanEdit)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = template.GetEditUrl(),
                    Text = "Edit"
                });
            }

            if (model.CanEditPublishSettings)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/TemplateManagement/PublishSettings?id=" + template.Id,
                    Text = "Publish Settings"
                });
            }

            if (model.CanEditGreen)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/Green/Index?id=" + template.Id,
                    Text = model.HasGreenModel ? "Edit Green Model" : "Create Green Model"
                });
            }

            if (model.CanCopy)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/TemplateManagement/Copy?templateId=" + template.Id,
                    Text = "Copy"
                });

                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/TemplateManagement/BulkCopy?templateId=" + template.Id,
                    Text = "Bulk Copy"
                });
            }

            if (model.CanMove)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = template.GetMoveUrl(),
                    Text = "Move",
                });
            }

            if (model.CanCopy)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/TemplateManagement/Copy?templateId= " + template.Id + "&newVersion=true",
                    Text = "New Version",
                    Disabled = !model.CanVersion,
                    ToolTip = !model.CanVersion ? "You cannot version this template because the associated implementation guide is not versioned yet." : string.Empty
                });
            }

            if (model.CanDelete)
            {
                model.Actions.Add(new ViewModel.ActionItem()
                {
                    Url = "/TemplateManagement/Delete/" + template.Id,
                    Text = "Delete"
                });
            }

            return model;
        }

        [HttpGet, Route("api/Template/{templateId}/Permissions")]
        public List<SearchUserModel> GetTemplatePermissionsName(int templateId)
        {
            Template template = this.tdb.Templates.Single(y => y.Id == templateId);
            var users = (from tp in this.tdb.ViewTemplatePermissions
                         join u in this.tdb.Users on tp.UserId equals u.Id
                         where tp.TemplateId == templateId && tp.Permission == "Edit"
                         select u).ToList();

            var usersList = users.Select(y => new SearchUserModel(y)).ToList();

            // Add the current user to the list if they are a data admin.
            // Data admins don't require explicit permissions.
            if (CheckPoint.Instance.IsDataAdmin)
            {
                User currentUser = CheckPoint.Instance.GetUser(this.tdb);
                usersList.Add(new SearchUserModel(currentUser));
            }

            // Add the current author of the template to the list if they are not already there
            if (!users.Any(y => y.Id == template.AuthorId))
                usersList.Add(new SearchUserModel(template.Author));

            return usersList.OrderBy(y => y.Name).ToList();
        }

        /// <summary>
        /// Searches for templates and returns basic meta-data about the templates
        /// </summary>
        /// <param name="count">The number of templates to return per page</param>
        /// <param name="page">The page number</param>
        /// <param name="sortProperty">The property to sort the results by. Possible values are: "Name", "Oid", "Organization", "TemplateType", "ImplementationGuide"</param>
        /// <param name="sortDescending">When true, sorts the results descending (z-a)</param>
        /// <param name="queryText">String to search the entire template for (including constraint numbers). Searches template's description, name, bookmark, etc.</param>
        /// <param name="filterName">Matches the string specified by filterName specifically to the name of the template. Only templates with a name containing this value will be returned.</param>
        /// <param name="filterOid">Matches the string specified by filterOid specifically to the oid (identifier) of the template. Only templates with an oid (identifier) containing this value will be returned.</param>
        /// <param name="filterImplementationGuideId">Matches the id of the implementation guide specifically to the implementation guide of the template. Only templates that are used by the specified implementation guide will be returned.</param>
        /// <param name="filterTemplateTypeId">Matches the id of the template type specified specifically to the template type of the template. Only templates with a matching type will be returned.</param>
        /// <param name="filterOrganizationId">Matches the id of the organization specified specifically to the organization of the template. Only templates with a matching organization will be returned.</param>
        /// <param name="selfOid">If exists, makes sure not to return the Oid of the template currently in use</param>
        /// <param name="filterContextType">Matches the context specified specifically to the context of the template. Only templates whose context type contain the specified value will be returned.</param>
        /// <param name="fhirPath">The path to a FHIR element that should be filtered based on the types within the base element</param>
        /// <param name="implementationGuideTypeId">The type id of the Implementation Guide associated with the template</param>
        /// <returns>Trifolia.Web.Models.TemplateManagement.ListModel</returns>
        [HttpGet, Route("api/Template"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public ListModel GetTemplates(
            int? count = null,
            int page = 1,
            string sortProperty = "Name",
            bool sortDescending = false,
            string queryText = null,
            string filterName = null,
            string filterOid = null,
            int? filterImplementationGuideId = null,
            int? filterTemplateTypeId = null,
            int? filterOrganizationId = null,
            string selfOid = null,
            string filterContextType = null,
            string fhirPath = null,
            int? implementationGuideTypeId = null)
        {
            Log.For(this).Trace("BEGIN: Getting list model for List and ListPartial");

            ListModel model = new ListModel()
            {
                CanEdit = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT),
                HideOrganization = !CheckPoint.Instance.IsDataAdmin
            };

            bool canEditTemplates = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT);
            var templateIds =
                tdb.SearchTemplates(
                    CheckPoint.Instance.GetUser(tdb).Id,
                    filterImplementationGuideId,
                    filterName,
                    filterOid,
                    filterTemplateTypeId,
                    filterOrganizationId,
                    filterContextType,
                    queryText)
                .Select(y => y.Value)
                .ToList();

            var query = (from tid in templateIds
                         join vtl in tdb.ViewTemplateLists on tid equals vtl.Id
                         select vtl);

            if (selfOid != null)
            {
                query = (from q in query
                         where q.Oid != selfOid
                         select q);
            }

            if (implementationGuideTypeId != null)
            {
                // TODO: Always filter based on the ig type
                query = (from q in query
                         where q.ImplementationGuideTypeId == implementationGuideTypeId
                         select q);
            }
            

            // filter based on fhirPath's base element types
            if (implementationGuideTypeId != null && !string.IsNullOrEmpty(fhirPath))
            {
                try
                {
                    var igType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);
                    var plugin = igType.GetPlugin();
                    List<String> types = plugin.GetFhirTypes(fhirPath);

                    //If types isn't set to resource (all possible templates), filter the query to match
                    if(!types.Contains("Resource"))
                    {
                        // Filter based on the types (only return templates with a Template.PrimaryContextType that matches one of the types[])
                        query = (from q in query
                                 join t in types on q.PrimaryContextType equals t
                                 select q);
                    }

                    
                }
                catch (NotSupportedException nse)
                {
                    if(nse.Message == "Not a reference")
                    {
                        return new ListModel(); //Return an empty model if user tries to attach a container template to a non-reference
                    }
                    else
                    {
                        // Do nothing... Can't filter like this for DSTU2 (as example)
                    }
                }
            }

            int currentUserId = CheckPoint.Instance.GetUser(tdb).Id;
            var editableTemplates = (from tp in this.tdb.ViewTemplatePermissions
                                     where tp.UserId == currentUserId && tp.Permission == "Edit"
                                     select tp.TemplateId).ToList();

            model.TotalItems = query.Count();

            switch (sortProperty)
            {
                case "Name":
                    if (!sortDescending)
                        query = query.OrderBy(y => y.Name);
                    else
                        query = query.OrderByDescending(y => y.Name);
                    break;
                case "Oid":
                    if (!sortDescending)
                        query = query.OrderBy(y => y.Oid);
                    else
                        query = query.OrderByDescending(y => y.Oid);
                    break;
                case "Organization":
                    if (!sortDescending)
                        query = query.OrderBy(y => y.Organization);
                    else
                        query = query.OrderByDescending(y => y.Organization);
                    break;
                case "TemplateType":
                    if (!sortDescending)
                        query = query.OrderBy(y => y.TemplateType);
                    else
                        query = query.OrderByDescending(y => y.TemplateType);
                    break;
                case "ImplementationGuide":
                    if (!sortDescending)
                        query = query.OrderBy(y => y.ImplementationGuide);
                    else
                        query = query.OrderByDescending(y => y.ImplementationGuide);
                    break;
                default:
                    throw new ArgumentException("Ordering results by the property specified is not supported");
            }

            if (count != null)
            {
                if (page > 1)
                    query = query.Skip((page - 1) * count.Value);

                query = query.Take(count.Value);
            }

            query = query.ToList();

            model.Items = (from queryItem in query
                           join et in editableTemplates on queryItem.Id equals et into qet
                           from jqet in qet.DefaultIfEmpty()
                           select new BigListItem()
                    {
                        Id = queryItem.Id,
                        Name = queryItem.Name,
                        Oid = queryItem.Oid,
                        OrganizationId = queryItem.OrganizationId,
                        Organization = queryItem.Organization,
                        ImplementationGuideId = queryItem.ImplementationGuideId.Value,
                        ImplementationGuide = queryItem.ImplementationGuide,
                        ImpliedTemplateName = queryItem.ImpliedTemplateName,
                        ImpliedTemplateOid = queryItem.ImpliedTemplateOid,
                        Open = queryItem.Open,
                        TemplateTypeId = queryItem.TemplateTypeId,
                        TemplateType = queryItem.TemplateType,
                        PublishDate = queryItem.PublishDate,
                        CanEdit = canEditTemplates && jqet != null,
                        //HasGreenModel = t.GreenTemplates.Count() > 0,
                        Description = queryItem.Description
                    }).ToList();

            Log.For(this).Trace("END: Getting list model for List and ListPartial");

            return model;
        }

        [HttpGet, Route("api/Template/Simple"), SecurableAction]
        public IEnumerable<SmallListItem> GetTemplates(bool editable = false)
        {
            var templates = (from t in this.tdb.Templates
                             join tp in this.tdb.ViewTemplatePermissions on t.Id equals tp.TemplateId
                             where tp.UserId == CheckPoint.Instance.User.Id && 
                             ((!editable && tp.Permission == "View") || (editable && tp.Permission == "Edit"))
                             select t);

            return (from t in templates
                    select new SmallListItem()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Oid = t.Oid
                    })
                    .OrderBy(y => y.Name);
        }

        [HttpGet, Route("api/Template/{templateId}/Usage"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IEnumerable<TemplateUsageModel> GetTemplateUsage(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");

            var template = this.tdb.Templates.Single(y => y.Id == templateId);

            var usageTemplates = (from t in this.tdb.Templates
                                  where t.ImpliedTemplateId == templateId
                                  select new
                                  {
                                      ImplementationGuideId = t.OwningImplementationGuideId,
                                      TemplateUsage = new TemplateUsageModel.Template()
                                      {
                                          Id = t.Id,
                                          Name = t.Name,
                                          Oid = t.Oid,
                                          Type = "Inheritance"
                                      }
                                  })
                                  .Union(from tc in this.tdb.TemplateConstraints
                                         join tcr in this.tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                                         join t in this.tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                                         where t.Id == templateId && tcr.ReferenceType == ConstraintReferenceTypes.Template
                                         select new
                                         {
                                             ImplementationGuideId = tc.Template.OwningImplementationGuideId,
                                             TemplateUsage = new TemplateUsageModel.Template()
                                                 {
                                                     Id = tc.TemplateId,
                                                     Name = tc.Template.Name,
                                                     Oid = tc.Template.Oid,
                                                     Type = "Containment"
                                                 }
                                         });

            var groupedTemplates = (from ut in usageTemplates
                                    join ig in this.tdb.ImplementationGuides on ut.ImplementationGuideId equals ig.Id
                                    group ut by ig into utg
                                    select new TemplateUsageModel()
                                    {
                                        ImplementationGuideId = utg.Key.Id,
                                        ImplementationGuideName = utg.Key.Name,
                                        IsSameImplementationGuide = template.OwningImplementationGuideId == utg.Key.Id,
                                        Templates = utg.Select(y => y.TemplateUsage)
                                    });

            return groupedTemplates;
        }

        [HttpDelete, Route("api/Template/{templateId}"), SecurableAction(SecurableNames.TEMPLATE_DELETE)]
        public void DeleteTemplate(int templateId, int? replaceTemplateId = null)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException("You do not have permission to delete this template");

            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                var template = auditedTdb.Templates.Single(y => y.Id == templateId);

                template.Delete(auditedTdb, replaceTemplateId);

                auditedTdb.SaveChanges();
            }
        }

        [HttpGet, Route("api/Template/{templateId}/Copy"), SecurableAction(SecurableNames.TEMPLATE_COPY)]
        public CopyModel Copy(int templateId, bool newVersion = false)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template.");

            Template template = this.tdb.Templates.Single(y => y.Id == templateId);
            ImplementationGuide previousVersionIg = tdb.ImplementationGuides.SingleOrDefault(y => y.PreviousVersionImplementationGuideId == template.OwningImplementationGuideId);

            string newOid = template.Oid;
            string oid, root, extension;

            if (template.GetIdentifierOID(out oid))
                newOid = string.Format("urn:hl7ii:{0}:{1}", oid, DateTime.Now.ToString("yyyy-MM-dd"));
            else if (template.GetIdentifierII(out root, out extension))
                newOid = string.Format("urn:hl7ii:{0}:{1}", root, DateTime.Now.ToString("yyyy-MM-dd"));

            CopyModel model = new CopyModel()
            {
                OriginalName = template.Name,
                TemplateId = template.Id,
                Name = template.Name,
                Oid = newOid,
                OriginalOid = template.Oid,
                Bookmark = template.Bookmark,
                ImpliedTemplateId = template.ImpliedTemplateId,
                Type = template.TypeDisplay,
                IsNewVersion = newVersion
            };

            if (newVersion && previousVersionIg != null)
            {
                model.ImplementationGuideId = previousVersionIg.Id;
                model.NewVersionImplementationGuideName = previousVersionIg.NameWithVersion;
            }
            else if (newVersion && previousVersionIg == null)
            {
                model.Message = "The implementation guide associated with the template has not been versioned yet. Cannot continue.";
                model.SubmitEnabled = false;
            }

            return model;
        }

        [HttpGet, Route("api/Template/Validate/Name"), SecurableAction]
        public bool ValidateName(string name)
        {
            if (this.tdb.Templates.Count(y => y.Name.ToLower() == name.ToLower()) > 0)
                return false;

            return true;
        }

        [HttpGet, Route("api/Template/Validate/Oid"), SecurableAction]
        public bool ValidateOid(string identifier, int? ignoreTemplateId = null)
        {
            if (ignoreTemplateId != null)
                return this.tdb.Templates.Count(y => y.Id != ignoreTemplateId && y.Oid.ToLower() == identifier.ToLower()) == 0;

            return this.tdb.Templates.Count(y => y.Oid.ToLower() == identifier.ToLower()) == 0;
        }

        [HttpGet, Route("api/Template/Validate/Bookmark"), SecurableAction]
        public bool ValidateBookmark(string bookmark, int? ignoreTemplateId = null)
        {
            if (ignoreTemplateId != null)
                return !this.tdb.Templates.Any(y => y.Id != ignoreTemplateId && y.Bookmark.Trim().ToLower() == bookmark.Trim().ToLower());

            return !this.tdb.Templates.Any(y => y.Bookmark.Trim().ToLower() == bookmark.Trim().ToLower());
        }

        [HttpGet, Route("api/Template/{sourceTemplateId}/Bookmark"), SecurableAction]
        public string GenerateBookmark(int sourceTemplateId, string newTemplateName)
        {
            Template sourceTemplate = this.tdb.Templates.Single(y => y.Id == sourceTemplateId);
            string abbreviation = sourceTemplate.TemplateType.GetAbbreviation();
            string bookmark = Template.GenerateBookmark(newTemplateName, abbreviation);

            return bookmark;
        }

        [HttpPost, Route("api/Template/Constraint/Conflict"), SecurableAction(SecurableNames.TEMPLATE_COPY)]
        public List<CopyModel.Constraint> GetConstraintConflicts(CopyModel model)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(model.TemplateId))
                throw new AuthorizationException("You do not have permission to view this template.");

            Template template = this.tdb.Templates.Single(y => y.Id == model.TemplateId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, template.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = template.OwningImplementationGuide.ImplementationGuideType.GetPlugin();

            // Check for duplicate conformance numbers
            List<int> destinationIgConfNumbers = (from t in this.tdb.Templates
                                                    join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                                    where t.OwningImplementationGuideId == model.ImplementationGuideId
                                                    select tc.Number.Value).ToList();

            
            List<int> duplicateConformanceNumbers = destinationIgConfNumbers
                .Where(y => template.ChildConstraints.Count(x => x.Number.Value == y) > 0).ToList();
            var constraintReferences = (from c in template.ChildConstraints
                                        join tcr in this.tdb.TemplateConstraintReferences on c.Id equals tcr.TemplateConstraintId
                                        join t in this.tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                                        where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                        select new ConstraintReference()
                                        {
                                            Bookmark = t.Bookmark,
                                            Identifier = t.Oid,
                                            Name = t.Name,
                                            TemplateConstraintId = tcr.TemplateConstraintId
                                        }).ToList();

            template.Constraints.ForEach(c =>
            {
                IFormattedConstraint fc = new FormattedConstraint(this.tdb, igSettings, igTypePlugin, (TemplateConstraint) c, constraintReferences);

                model.Constraints.Add(new CopyModel.Constraint()
                {
                    Number = c.Number.Value,
                    ParentNumber = c.Parent != null ? c.Parent.Number : null,
                    DataType = !string.IsNullOrEmpty(c.DataType) ? c.DataType : "N/A",
                    Context = c.Context,
                    Conformance = c.Conformance,
                    Cardinality = c.Cardinality,
                    Order = c.Order,
                    Narrative = fc.GetPlainText(),
                    NumberReplacementType = duplicateConformanceNumbers.Contains(c.Number.Value) ?
                        NumberReplacementTypes.RegenerateThis :
                        NumberReplacementTypes.UseSame
                });
            });

            // Only return the Constraints from the model
            return model.Constraints;
        }

        [HttpPost, Route("api/Template/Copy"), SecurableAction(SecurableNames.TEMPLATE_COPY)]
        public dynamic Copy(CopyModel model)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                if (!CheckPoint.Instance.GrantViewTemplate(model.TemplateId))
                    throw new AuthorizationException("You do not have permission to view this template.");

                if (!CheckPoint.Instance.GrantEditImplementationGuide(model.ImplementationGuideId.Value))
                    throw new AuthorizationException("You do not have permision to edit the destination implementation guide.");

                if (auditedTdb.Templates.Count(y => y.Oid.ToLower() == model.Oid.ToLower()) > 0)
                    return new { Status = "Failure", Message = DUPLICATE_OID };

                try
                {
                    Template sourceTemplate = auditedTdb.Templates.Single(y => y.Id == model.TemplateId);
                    ImplementationGuide newImplementationGuide = auditedTdb.ImplementationGuides.Single(y => y.Id == model.ImplementationGuideId);
                    IGSettingsManager newIgSettings = new IGSettingsManager(auditedTdb, model.ImplementationGuideId.Value);
                    string igCategoriesSetting = newIgSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
                    List<string> igCategories = new List<string>();

                    if (!string.IsNullOrEmpty(igCategoriesSetting))
                         igCategories = new List<string>(igCategoriesSetting.Split(','));

                    var currentUser = CheckPoint.Instance.GetUser(auditedTdb);
                    Template copyTemplate = sourceTemplate.CloneTemplate(auditedTdb, currentUser.Id);
                    int? statusId = newImplementationGuide.PublishStatusId != null ? newImplementationGuide.PublishStatusId : PublishStatus.GetDraftStatus(auditedTdb).Id;

                    copyTemplate.Name = model.Name;
                    copyTemplate.Oid = model.Oid;
                    copyTemplate.Bookmark = Template.GenerateBookmark(model.Name, sourceTemplate.TemplateType.GetAbbreviation());
                    copyTemplate.OwningImplementationGuideId = newImplementationGuide.Id;
                    copyTemplate.OwningImplementationGuide = newImplementationGuide;
                    copyTemplate.StatusId = statusId;
                    copyTemplate.ImpliedTemplateId = model.ImpliedTemplateId;

                    // For new versions, the UI automatically sets the IG, but the template needs to be linked to its previous version
                    if (model.IsNewVersion)
                        copyTemplate.PreviousVersionTemplateId = sourceTemplate.Id;

                    // Update the template type of the template
                    if (sourceTemplate.ImplementationGuideType != newImplementationGuide.ImplementationGuideType)
                    {
                        var foundTemplateType = newImplementationGuide.ImplementationGuideType.TemplateTypes.SingleOrDefault(y => y.RootContextType == sourceTemplate.TemplateType.RootContextType);

                        if (foundTemplateType == null)
                            return new { Status = "Failure", Message = "The destination implementation guide does not have a matching template/profile type \"" + sourceTemplate.TemplateType.RootContextType + "\"." };

                        copyTemplate.TemplateTypeId = foundTemplateType.Id;
                        copyTemplate.TemplateType = foundTemplateType;
                    }

                    // Update the constraints conformance numbers
                    foreach (var cConstraint in model.Constraints)
                    {
                        if (cConstraint.NumberReplacementType == NumberReplacementTypes.UseSame)
                            continue;

                        if (cConstraint.NumberReplacementType == NumberReplacementTypes.RegenerateThis)
                        {
                            var newConstraint = copyTemplate.ChildConstraints.Single(y => y.Number == cConstraint.Number);
                            newConstraint.Number = null;
                        }
                        else if (cConstraint.NumberReplacementType == NumberReplacementTypes.RegenerateOther)
                        {
                            var sourceConstraint = auditedTdb.TemplateConstraints.Single(y => y.TemplateId == sourceTemplate.Id && y.Number == cConstraint.Number);
                            sourceConstraint.Number = null;
                        }
                    }

                    // Remove any categories from the constraint that aren't in the new implementation guide
                    foreach (var cConstraint in copyTemplate.ChildConstraints)
                    {
                        if (string.IsNullOrEmpty(cConstraint.Category))
                            continue;

                        List<string> cConstraintCategories = new List<string>(cConstraint.Category.Split(','));
                        bool updateCategories = false;

                        for (int i = cConstraintCategories.Count - 1; i >= 0; i--)
                        {
                            var cConstraintCategory = cConstraintCategories[i];

                            if (!igCategories.Contains(cConstraintCategory))
                            {
                                cConstraintCategories.RemoveAt(i);
                                updateCategories = true;
                            }
                        }

                        if (updateCategories)
                            cConstraint.Category = string.Join(",", cConstraintCategories);
                    }

                    auditedTdb.Templates.Add(copyTemplate);
                    auditedTdb.SaveChanges();

                    return new { Status = "Success", TemplateId = copyTemplate.Id };
                }
                catch (Exception ex)
                {
                    Log.For(this).Error("Error occurred while copying template", ex);
                    return new { Status = "Failure", Message = "An error occurred while copying the template: " + ex.Message };
                }
            }
        }

        [HttpGet, Route("api/Template/{templateId}/Changes"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public DifferenceModel Changes(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");

            Template currentVersion = this.tdb.Templates.Single(t => t.Id == templateId);
            Template previousVersion = this.tdb.Templates.SingleOrDefault(t => t.Id == currentVersion.PreviousVersionTemplateId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, currentVersion.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = currentVersion.ImplementationGuideType.GetPlugin();

            if (previousVersion == null)
                return null;

            VersionComparer comparer = VersionComparer.CreateComparer(this.tdb, igTypePlugin, igSettings);
            ComparisonResult result = comparer.Compare(previousVersion, currentVersion);

            DifferenceModel model = new DifferenceModel(result)
            {
                Id = currentVersion.Id,
                TemplateName = currentVersion.Name,
                PreviousTemplateName = string.Format("{0} ({1})", previousVersion.Name, previousVersion.Oid),
                PreviousTemplateId = previousVersion.Id
            };

            return model;
        }

        [HttpGet, Route("api/Template/{templateId}/Move"), SecurableAction(SecurableNames.TEMPLATE_MOVE)]
        public MoveModel GetMoveModel(int templateId)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException("You do not have permission to move this template.");

            Template template = this.tdb.Templates.Single(y => y.Id == templateId);

            MoveModel model = new MoveModel()
            {
                TemplateId = template.Id,
                TemplateName = string.Format("{0} ({1})", template.Name, template.Oid),
                ImplementationGuideId = template.OwningImplementationGuideId,
                TemplateTypeId = template.TemplateTypeId,
                PrimaryContext = template.PrimaryContext,
                PrimaryContextType = template.PrimaryContextType,
                IsPublished = template.OwningImplementationGuide.IsPublished()
            };

            return model;
        }

        [HttpPost, Route("api/Template/Move/Constraint"), SecurableAction(SecurableNames.TEMPLATE_MOVE)]
        public List<MoveConstraint> MoveConstraints(MoveModel model)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(model.TemplateId))
                throw new AuthorizationException("You do not have permission to move this template.");

            Template template = this.tdb.Templates.Single(y => y.Id == model.TemplateId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, template.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = template.OwningImplementationGuide.ImplementationGuideType.GetPlugin();
            List<int> duplicateConformanceNumbers = new List<int>();
            List<MoveConstraint> moveConstraints = new List<MoveConstraint>();

            // Only check for duplicate conformance numbers if we are moving to a new IG
            if (model.ImplementationGuideId != template.OwningImplementationGuideId)
            {
                List<int> destinationIgConfNumbers = (from t in this.tdb.Templates
                                                      join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                                      where t.OwningImplementationGuideId == model.ImplementationGuideId
                                                      select tc.Number.Value).ToList();

                duplicateConformanceNumbers =
                    destinationIgConfNumbers.Where(y => template.ChildConstraints.Count(x => x.Number.Value == y) > 0).ToList();
            }

            template.ChildConstraints.ToList().ForEach(c =>
            {
                IFormattedConstraint fc = new FormattedConstraint(this.tdb, igSettings, igTypePlugin, c);

                moveConstraints.Add(new MoveConstraint()
                {
                    Id = c.Id,
                    ParentId = c.ParentConstraintId,
                    Number = c.Number.Value,
                    DataType = !string.IsNullOrEmpty(c.DataType) ? c.DataType : "N/A",
                    Context = c.Context,
                    Conformance = c.Conformance,
                    Cardinality = c.Cardinality,
                    Order = c.Order,
                    Narrative = fc.GetPlainText(),
                    NumberReplacementType = duplicateConformanceNumbers.Contains(c.Number.Value) ?
                        NumberReplacementTypes.RegenerateThis :
                        NumberReplacementTypes.UseSame
                });
            });

            return moveConstraints;
        }

        [HttpPost, Route("api/Template/Move"), SecurableAction(SecurableNames.TEMPLATE_MOVE)]
        public void Move(MoveCompleteModel model)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                if (!CheckPoint.Instance.GrantEditTemplate(model.Template.TemplateId))
                    throw new AuthorizationException("You do not have permission to move this template");

                Template template = auditedTdb.Templates.Single(y => y.Id == model.Template.TemplateId);
                ImplementationGuide newIg = auditedTdb.ImplementationGuides.Single(y => y.Id == model.Template.ImplementationGuideId);

                if (template.OwningImplementationGuideId != model.Template.ImplementationGuideId)
                    template.OwningImplementationGuideId = model.Template.ImplementationGuideId;

                if (template.TemplateTypeId != model.Template.TemplateTypeId)
                    template.TemplateTypeId = model.Template.TemplateTypeId;

                if (template.PrimaryContext != model.Template.PrimaryContext)
                    template.PrimaryContext = model.Template.PrimaryContext;

                if (template.PrimaryContextType != model.Template.PrimaryContextType)
                    template.PrimaryContextType = model.Template.PrimaryContextType;

                // If the template's status is deprecated or retired, leave it. Otherwise, the status of the template 
                // should match the IG it's being moved to
                if (template.StatusId != PublishStatus.GetDeprecatedStatus(auditedTdb).Id &&
                    template.StatusId != PublishStatus.GetRetiredStatus(auditedTdb).Id &&
                    template.StatusId != newIg.PublishStatusId &&
                    newIg.PublishStatusId != null)
                    template.StatusId = newIg.PublishStatusId;

                if (model.Constraints != null)
                {
                    foreach (MoveConstraint cMoveConstraint in model.Constraints.Where(y => !y.IsDeleted))
                    {
                        TemplateConstraint cConstraint = template.ChildConstraints.Single(y => y.Number == cMoveConstraint.Number);

                        TemplateConstraint cParentConstraint = cMoveConstraint.ParentId != null ?
                            template.ChildConstraints.Single(y => y.Id == cMoveConstraint.ParentId) :
                            null;

                        if (cConstraint.ParentConstraint != cParentConstraint)
                            cConstraint.ParentConstraint = cParentConstraint;

                        if (cConstraint.Context != cMoveConstraint.Context)
                            cConstraint.Context = cMoveConstraint.Context;
                    }

                    foreach (MoveConstraint cMoveConstraint in model.Constraints.Where(y => y.IsDeleted))
                    {
                        TemplateConstraint cConstraint = template.ChildConstraints.Single(y => y.Number == cMoveConstraint.Number);
                        auditedTdb.TemplateConstraints.Remove(cConstraint);
                    }

                    // Conf number regeneration
                    foreach (MoveConstraint cMoveConstraint in model.Constraints.Where(y => y.NumberReplacementType != NumberReplacementTypes.UseSame))
                    {
                        TemplateConstraint cConstraint = template.ChildConstraints.Single(y => y.Number == cMoveConstraint.Number);

                        if (cMoveConstraint.NumberReplacementType == NumberReplacementTypes.RegenerateThis)
                        {
                            cConstraint.Number = null;
                        }
                        else if (cMoveConstraint.NumberReplacementType == NumberReplacementTypes.RegenerateOther)
                        {
                            var otherConstraints = (from t in auditedTdb.Templates
                                                    join tc in auditedTdb.TemplateConstraints on t.Id equals tc.TemplateId
                                                    where t.Id != model.Template.TemplateId && t.OwningImplementationGuideId == model.Template.ImplementationGuideId
                                                    select tc);

                            otherConstraints.ToList().ForEach(y =>
                            {
                                y.Number = null;
                            });
                        }
                    }
                }

                auditedTdb.SaveChanges();
            }
        }

        #region Publish Settings

        [HttpGet, Route("api/Template/{templateId}/GenerateSample"), SecurableAction(SecurableNames.PUBLISH_SETTINGS)]
        public string GenerateXmlSample(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");

            Template lTemplate = tdb.Templates.Single(t => t.Id == templateId);

            var igTypePlugin = IGTypePluginFactory.GetPlugin(lTemplate.ImplementationGuideType);
            return igTypePlugin.GenerateSample(this.tdb, lTemplate);
        }

        [HttpGet, Route("api/Template/{templateId}/PublishSettings"), SecurableAction(SecurableNames.PUBLISH_SETTINGS)]
        public PublishModel PublishSettingsData(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException("You do not have permission to view this template");

            Template lTemplate = tdb.Templates.Single(t => t.Id == templateId);
            PublishModelMapper lMapper = new PublishModelMapper(this.tdb);
            PublishModel lModel = lMapper.MapEntityToViewModel(lTemplate);

            return lModel;
        }

        [HttpPost, Route("api/Template/PublishSettings/Save"), SecurableAction(SecurableNames.PUBLISH_SETTINGS)]
        public void SavePublishSettings(PublishModel aModel)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(aModel.TemplateId))
                throw new AuthorizationException("You do not have permission to edit this template");

            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                PublishModelMapper lMapper = new PublishModelMapper(auditedTdb);
                lMapper.MapViewModelToEntity(aModel);

                auditedTdb.SaveChanges();
            }
        }

        #endregion

        private ViewModel.Constraint BuildConstraint(string baseLink, IGSettingsManager igSettings, IIGTypePlugin igTypePlugin, TemplateConstraint dbConstraint, int constraintCount)
        {
            IFormattedConstraint fc = new FormattedConstraint(this.tdb, igSettings, igTypePlugin, dbConstraint, linkContainedTemplate: true, linkIsBookmark: false, createLinksForValueSets: false);

            ViewModel.Constraint newConstraint = new ViewModel.Constraint()
            {
                Prose = fc.GetHtml(baseLink, constraintCount, false),
                IsHeading = dbConstraint.IsHeading,
                HeadingTitle = dbConstraint.Context,
                HeadingDescription = dbConstraint.HeadingDescription,
                Description = dbConstraint.Description.MarkdownToHtml(),
                Label = dbConstraint.Label
            };

            int nextConstraintCount = 0;
            foreach (TemplateConstraint cDbConstraint in dbConstraint.ChildConstraints.OrderBy(y => y.Order))
            {
                ViewModel.Constraint nextNewConstraint = BuildConstraint(baseLink, igSettings, igTypePlugin, cDbConstraint, ++nextConstraintCount);
                newConstraint.Children.Add(nextNewConstraint);
            }

            return newConstraint;
        }
    }
}
