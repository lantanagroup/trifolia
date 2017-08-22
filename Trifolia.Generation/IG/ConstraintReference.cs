using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class ConstraintReference
    {
        public int TemplateConstraintId { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string Bookmark { get; set; }
        public bool IncludedInIG { get; set; }

        public string GetLink(bool linkIsBookmark, string linkBase)
        {
            if (linkIsBookmark)
                return string.Format("{0}{1}", linkBase, this.Bookmark);

            // TODO: A bit of a hack... Should move GetViewUrl() to static method that is reused by extension methods
            return new Template()
            {
                Oid = this.Identifier,
                Bookmark = this.Bookmark
            }.GetViewUrl(linkBase);
        }
    }
}
