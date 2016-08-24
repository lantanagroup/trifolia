using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Group
{
    public class GroupDisclaimer
    {
        public GroupDisclaimer()
        {

        }

        public GroupDisclaimer(Trifolia.DB.Group group)
        {
            this.GroupName = group.Name;
            this.GroupDescription = group.Description;
            this.Disclaimer = group.Disclaimer;
        }

        public string GroupName { get; set; }

        public string GroupDescription { get; set; }

        public string Disclaimer { get; set; }
    }
}