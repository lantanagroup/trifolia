using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models
{
    public class SupportRequestModel
    {
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Details { get; set; }
        public string Summary { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}