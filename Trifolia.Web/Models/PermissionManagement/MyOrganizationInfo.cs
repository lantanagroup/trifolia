using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.PermissionManagement
{
    public enum PermissionTypes
    {
        Everyone = 0,
        Group = 1,
        User = 2
    }

    public class MyOrganizationInfo
    {
        public MyOrganizationInfo()
        {
            this.MyGroups = new List<MemberEntry>();
            this.OtherOrganizations = new List<OrganizationEntry>();
        }

        public int MyOrganizationId { get; set; }
        public List<MemberEntry> MyGroups { get; set; }
        public List<OrganizationEntry> OtherOrganizations { get; set; }
        
        public class MemberEntry
        {
            public PermissionTypes Type { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class OrganizationEntry
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}