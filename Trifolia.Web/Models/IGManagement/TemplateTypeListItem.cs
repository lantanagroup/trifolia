using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class TemplateTypeListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RootContext { get; set; }
        public string RootContextType { get; set; }
        public string Abbreviation { get; set; }
        public string ImplementationGuideType { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0}: {1}", ImplementationGuideType, Name);
            }
        }
    }
}