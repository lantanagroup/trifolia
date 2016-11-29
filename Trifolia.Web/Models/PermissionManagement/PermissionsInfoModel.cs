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

    public class PermissionsInfoModel
    {
        public PermissionsInfoModel()
        {
            this.Groups = new List<MemberEntry>();
        }

        public List<MemberEntry> Groups { get; set; }
        
        public class MemberEntry
        {
            public PermissionTypes Type { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}