using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using System.Text.RegularExpressions;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.Logging;
using Trifolia.Web.Models.TemplateManagement;
using Trifolia.Web.Models;
using Trifolia.Web.Extensions;
using Trifolia.DB;

using Trifolia.Authorization;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Generation.IG;
using Trifolia.Generation.Versioning;

namespace Trifolia.Web.Controllers
{
    public class TemplateManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public TemplateManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public TemplateManagementController()
        {
            this.tdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Publish Settings

        [Securable(SecurableNames.PUBLISH_SETTINGS)]
        public ActionResult PublishSettings(int id)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(id))
                throw new AuthorizationException("You do not have authorization to edit this template.");

            return View("PublishSettings", id);
        }

        #endregion

        #region List Actions

        [Securable(SecurableNames.TEMPLATE_LIST)]
        public ActionResult List()
        {
            return View("List");
        }

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult AddNewTemplate()
        {
            Log.For(this).Trace("Add New Template Clicked");
            return Redirect("/TemplateManagement/Edit");
        }

        #endregion

        #region Copy Actions

        [Securable(SecurableNames.TEMPLATE_COPY)]
        public ActionResult Copy(int templateId, bool newVersion = false)
        {
            CopyRequestModel model = new CopyRequestModel()
            {
                TemplateId = templateId,
                NewVersion = newVersion
            };

            if (newVersion)
            {
                var template = this.tdb.Templates.Single(y => y.Id == templateId);
                var newVersionImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y => y.PreviousVersionImplementationGuideId == template.OwningImplementationGuideId);

                if (newVersionImplementationGuide == null)
                    throw new Exception("A new version of the template's implementation guide does not exist yet.");
                else if (!CheckPoint.Instance.GrantEditImplementationGuide(newVersionImplementationGuide.Id))
                    throw new Exception("You are not authorized to edit the new version of the implementation guide!");
            }

            return View("Copy", model);
        }

        #endregion

        #region View

        [Securable(SecurableNames.TEMPLATE_LIST)]
        public ActionResult ViewOid(string oid)
        {
            string identifier = string.Format("urn:oid:{0}", oid);
            Template foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (foundTemplate == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return View(foundTemplate.Id);
        }

        [Securable(SecurableNames.TEMPLATE_LIST)]
        public ActionResult ViewInstanceIdentifier(string root, string extension)
        {
            string identifier = string.Format("urn:hl7ii:{0}:{1}", root, extension);
            Template foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (foundTemplate == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return View(foundTemplate.Id);
        }
        
        [Securable(SecurableNames.TEMPLATE_LIST)]
        public ActionResult ViewUri(string uri)
        {
            Template foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == uri);

            if (foundTemplate == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, uri);
                throw new ArgumentException(message);
            }

            return View(foundTemplate.Id);
        }

        /// <summary>
        /// View action for templates. Builds a model with all of the information necessary to display a template, including constraints and xml sample
        /// </summary>
        /// <param name="templateId">Required. The id of the template to view</param>
        /// <returns>The "View" view from the TemplateManagement view set, and the prepared ViewModel model</returns>
        [Securable(SecurableNames.TEMPLATE_LIST)]
        public ActionResult ViewId(int templateId)
        {
            return View(templateId);
        }

        private ActionResult View(int templateId)
        {
            if (!CheckPoint.Instance.GrantViewTemplate(templateId))
                throw new AuthorizationException(App_GlobalResources.TrifoliaLang.TemplateViewPermissionMessage);

            return View("View", templateId);
        }

        #endregion

        #region Move Actions

        [Securable(SecurableNames.TEMPLATE_MOVE)]
        public ActionResult MoveOid(string oid)
        {
            string identifier = string.Format("urn:oid:{0}", oid);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return MoveId(template.Id);
        }

        [Securable(SecurableNames.TEMPLATE_MOVE)]
        public ActionResult MoveInstanceIdentifier(string root, string extension)
        {
            string identifier = string.Format("urn:hl7ii:{0}:{1}", root, extension);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return MoveId(template.Id);
        }

        [Securable(SecurableNames.TEMPLATE_MOVE)]
        public ActionResult MoveUri(string uri)
        {
            string identifier = string.Format("uri:{0}", uri);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return MoveId(template.Id);
        }

        [Securable(SecurableNames.TEMPLATE_MOVE)]
        public ActionResult MoveId(int templateId)
        {
            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException(App_GlobalResources.TrifoliaLang.TemplateEditPermissionMessage);

            return View("Move", templateId);
        }

        #endregion

        #region Delete Actions

        [Securable(SecurableNames.TEMPLATE_DELETE)]
        public ActionResult Delete(int templateId)
        {
            if (!this.tdb.Templates.Any(y => y.Id == templateId))
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, templateId);
                throw new ArgumentException(message);
            }

            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new AuthorizationException(App_GlobalResources.TrifoliaLang.TemplateEditPermissionMessage);

            return View("Delete", templateId);
        }

        #endregion

        #region Bulk Copy Actions

        [Securable(SecurableNames.TEMPLATE_COPY)]
        public ActionResult BulkCopy(int templateId)
        {
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Id == templateId);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, templateId);
                throw new ArgumentException(message);
            }

            if (!CheckPoint.Instance.GrantEditImplementationGuide(template.OwningImplementationGuideId))
                throw new ArgumentException(App_GlobalResources.TrifoliaLang.TemplateEditPermissionMessage);

            return View("BulkCopy", templateId);
        }

        #endregion

        #region Edit Template

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult EditId(int templateId, int? defaultImplementationGuideId = null, bool newEditor = false)
        {
            Template template = this.tdb.Templates.Single(y => y.Id == templateId);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, templateId);
                throw new ArgumentException(message);
            }

            return Edit(template, defaultImplementationGuideId, newEditor);
        }

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult EditOid(string oid, int? defaultImplementationGuideId = null, bool newEditor = false)
        {
            string identifier = string.Format("urn:oid:{0}", oid);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return Edit(template, defaultImplementationGuideId, newEditor);
        }

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult EditInstanceIdentifier(string root, string extension, int? defaultImplementationGuideId = null, bool newEditor = false)
        {
            string identifier = string.Format("urn:hl7ii:{0}:{1}", root, extension);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return Edit(template, defaultImplementationGuideId, newEditor);
        }

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult EditUri(string uri, int? defaultImplementationGuideId = null, bool newEditor = false)
        {
            string identifier = string.Format("uri:{0}", uri);
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == identifier);

            if (template == null)
            {
                var message = string.Format(App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat, identifier);
                throw new ArgumentException(message);
            }

            return Edit(template, defaultImplementationGuideId, newEditor);
        }

        [Securable(SecurableNames.TEMPLATE_EDIT)]
        public ActionResult EditNew(int? defaultImplementationGuide = null, bool newEditor = false)
        {
            return Edit(null, defaultImplementationGuide, newEditor);
        }

        private ActionResult Edit(Template template, int? defaultImplementationGuideId = null, bool newEditor = false)
        {
            EditModel model = new EditModel()
            {
                DefaultImplementationGuideId = defaultImplementationGuideId
            };

            if (template != null)
            {
                if (!CheckPoint.Instance.GrantEditTemplate(template.Id))
                    throw new AuthorizationException("You do not have authorization to edit this template!");

                model.TemplateId = template.Id;
            }

            return View(newEditor ? "NewEditor" : "Edit", model);
        }

        #endregion
    }
}
