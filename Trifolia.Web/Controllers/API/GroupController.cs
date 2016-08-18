using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Web.Models.Group;

namespace Trifolia.Web.Controllers.API
{
    public class GroupController : ApiController
    {
        private IObjectRepository tdb;

        public GroupController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public GroupController()
            : this(DBContext.Create())
        {

        }

        [HttpGet, Route("api/Group"), SecurableAction()]
        public IEnumerable<GroupModel> GetGroups()
        {
            var groups = (from g in this.tdb.Groups
                          select new GroupModel()
                          {
                              Id = g.Id,
                              Name = g.Name
                          });

            return groups;
        }

        [HttpPost, Route("api/Group"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public int SaveGroup(int organizationId, GroupModel model)
        {
            Group group = this.tdb.Groups.SingleOrDefault(y => y.Id == model.Id);

            if (group == null)
            {
                group = new Group();
                this.tdb.Groups.AddObject(group);
            }

            group.Name = model.Name;

            this.tdb.SaveChanges();

            return group.Id;
        }

        [HttpDelete, Route("api/Group/{groupId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void DeleteGroup(int groupId)
        {
            var group = this.tdb.Groups.Single(y => y.Id == groupId);

            group.Users.ToList().ForEach(y =>
            {
                this.tdb.UserGroups.DeleteObject(y);
            });

            group.ImplementationGuidePermissions.ToList().ForEach(y =>
            {
                this.tdb.ImplementationGuidePermissions.DeleteObject(y);
            });

            this.tdb.Groups.DeleteObject(group);

            this.tdb.SaveChanges();
        }
    }
}