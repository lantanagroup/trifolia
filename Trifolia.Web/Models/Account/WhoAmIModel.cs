using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    public class WhoAmIModel
    {
        public WhoAmIModel()
        {
            this.Securables = new List<string>();
        }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Securables { get; set; }
    }
}