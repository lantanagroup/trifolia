using System;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Test.Accounts
{
    [TestClass]
    public class HL7Accounts
    {
        [TestMethod]
        public void CheckPoint_CheckHL7Roles()
        {
            MockObjectRepository mockRepo = TestDataGenerator.GenerateMockDataset1();
            mockRepo.FindOrAddUser("testuser", mockRepo.FindOrAddOrganization(Helper.AUTH_EXTERNAL));
            mockRepo.AddRole("Users");
            mockRepo.AddRole("HL7 Members");
            mockRepo.AssociateUserWithRole("testuser", mockRepo.FindOrAddOrganization(Helper.AUTH_EXTERNAL).Id, "Users");
            
            //With Role Restriction added, try to add role, Assert role did not get added
            Role role = mockRepo.Roles.Single(y => y.Name == "HL7 Members");
            role.Restrictions.Add(new RoleRestriction()
                {
                    Role = role,
                    RoleId = role.Id,
                    OrganizationId = mockRepo.FindOrAddOrganization(Helper.AUTH_EXTERNAL).Id,
                    Organization = mockRepo.FindOrAddOrganization(Helper.AUTH_EXTERNAL)
                });

            DBContext.Instance = mockRepo;
            CheckPoint.Instance.CheckHL7Roles("testuser", "ismember");

            User user = mockRepo.Users.SingleOrDefault(y => y.UserName == "testuser" && y.Organization.Name == Helper.AUTH_EXTERNAL);

            Assert.IsTrue(user.UserName == "testuser");
            Assert.IsTrue(user.Roles.Count(y => y.Role.Name == "Users") > 0);
            Assert.IsFalse(user.Roles.Count(y => y.Role.Name == "HL7 Members") > 0);


            //Test for: Remove Role Restriction, try to modify role, Assert the role gets added

            role.Restrictions.Remove(role.Restrictions.SingleOrDefault(y => y.Organization.Name == Helper.AUTH_EXTERNAL));

            DBContext.Instance = mockRepo;
            CheckPoint.Instance.CheckHL7Roles("testuser", "ismember");

            Assert.IsTrue(user.UserName == "testuser");
            Assert.IsTrue(user.Roles.Count(y => y.Role.Name == "HL7 Members") > 0);
            
            //Should the old role be removed?
            //Assert.IsFalse(user.Roles.Count(y => y.Role.Name == "Users") > 0);

        }
    }
}
