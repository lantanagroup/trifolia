using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Web.Extensions;

namespace Trifolia.Web.Controllers
{
    [Securable()]
    public class LookupController : Controller
    {
        public enum IGPublishTypes
        {
            Published,
            Unpublished,
            All
        }

        public enum PermissionTypes
        {
            View,
            Edit
        }

        #region Constructors

        private IObjectRepository tdb;

        public LookupController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        public LookupController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        public ActionResult ImplementationGuides(PermissionTypes permissionType, IGPublishTypes publishType, string q)
        {
            var implementationGuides = this.tdb.ImplementationGuides
                .AsEnumerable()
                .Select(y => new {
                    Id = y.Id,
                    Name = y.NameWithVersion,
                    Organization = y.Organization != null ? y.Organization.Name : string.Empty,
                    Status = y.PublishStatus != null ? y.PublishStatus.Status : string.Empty,
                    PublishDate = y.PublishDate != null ? y.PublishDate.Value.ToString("MM/dd/yyyy") : string.Empty
                });

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                int userId = CheckPoint.Instance.User.Id;
                implementationGuides = (from ig in implementationGuides
                                        join igp in this.tdb.ImplementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                                        where igp.UserId == userId &&
                                          ((permissionType == PermissionTypes.Edit && igp.Permission == "Edit") ||
                                          permissionType == PermissionTypes.View)
                                        select ig);
            }

            if (publishType != IGPublishTypes.All)
            {
                implementationGuides = (from ig in implementationGuides
                                        where (publishType == IGPublishTypes.Published && ig.Status == "Published") || 
                                          (publishType == IGPublishTypes.Unpublished && ig.Status != "Published")
                                        select ig);
            }

            if (!string.IsNullOrEmpty(q))
            {
                implementationGuides = (from ig in implementationGuides
                                        where ig.Name.ToLower().Contains(q.ToLower())
                                        select ig);
            }

            var ret = implementationGuides
                .Distinct()
                .OrderBy(y => y.Name);

            return Json(ret);
        }
    }
}
