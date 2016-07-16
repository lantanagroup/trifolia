using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.Entity.Core.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Web.Controllers.API;
using Trifolia.Web.Models.RoleManagement;
using Trifolia.Shared;
using Trifolia.Authorization;

namespace Trifolia.Test.Controllers.RoleManagement
{
    [TestClass]
    public class RoleManagementControllerTest
    {
        MockObjectRepository mockRepo = new MockObjectRepository();
        RoleController controller = null;

        private int roleUserId = 0;
        private int roleAdminId = 0;

        [TestInitialize]
        public void Setup()
        {
            this.controller = new RoleController(mockRepo);

            // Setup test data
            roleUserId = mockRepo.FindOrAddRole("user").Id;
            roleAdminId = mockRepo.FindOrAddRole("admin").Id;
        }

        [TestMethod]
        public void TestRoles()
        {
            RolesModel model = this.controller.GetRoles();

            Assert.IsNull(model.DefaultRoleId, "No default role should have been returned.");
            Assert.AreEqual(2, model.Roles.Count, "Expected to find two roles.");
            Assert.AreEqual(32, model.AllSecurables.Count, "Expected to find 32 securables.");

            RolesModel.RoleItem roleModel = model.Roles.Single(y => y.Id == this.roleUserId);

            // Make sure the controller returns the role WITHOUT any securables assigned to it in the model
            Assert.IsNotNull(roleModel, "Expected the JSON response to contain the RoleItem model.");

            Assert.AreEqual(this.roleUserId, roleModel.Id, "Returned model does not match the role id requested.");
            Assert.AreEqual(0, roleModel.AssignedSecurables.Count, "Role shouldn't have any securables assigned to it yet.");

            this.mockRepo.AssociateSecurableToRole("user", SecurableNames.TEMPLATE_EDIT);
            this.mockRepo.AssociateSecurableToRole("user", SecurableNames.TEMPLATE_LIST);
            
            model = this.controller.GetRoles();
            roleModel = model.Roles.Single(y => y.Id == this.roleUserId);

            Assert.AreEqual(2, roleModel.AssignedSecurables.Count, "Expected two securables to be returned for the role.");

            // Remove a securable from the role and make sure the controller only returns one securable for the role
            this.mockRepo.RemoveSecurableFromRole("user", SecurableNames.TEMPLATE_EDIT);

            model = this.controller.GetRoles();
            roleModel = model.Roles.Single(y => y.Id == this.roleUserId);

            Assert.AreEqual(1, roleModel.AssignedSecurables.Count, "Expected one securable to be returned for the role.");
        }
    }
}
