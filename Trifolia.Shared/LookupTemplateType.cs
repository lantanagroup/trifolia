using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    [Serializable]
    public class LookupTemplateType
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImplementationGuideType { get; set; }
        public string RootContext { get; set; }
        public string RootContextType { get; set; }

        public string NameDisplay
        {
            get
            {
                return string.Format("{0} ({1})", this.Name, this.ImplementationGuideType);
            }
            set
            {

            }
        }

        #endregion

        public static List<LookupTemplateType> GetTemplateTypes(IObjectRepository tdb)
        {
            return (from tt in tdb.TemplateTypes
                    select new LookupTemplateType()
                    {
                        Id = tt.Id,
                        Name = tt.Name,
                        ImplementationGuideType = tt.ImplementationGuideType.Name,
                        RootContext = tt.RootContext,
                        RootContextType = tt.RootContextType
                    }).ToList();
        }

        public static List<LookupTemplateType> GetTemplateTypes()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplateTypes(tdb);
            }
        }

        public static List<LookupTemplateType> GetTemplateTypes(IObjectRepository tdb, int implementationGuideId)
        {
            var results = (from ig in tdb.ImplementationGuides
                           join tt in tdb.TemplateTypes on ig.ImplementationGuideTypeId equals tt.ImplementationGuideTypeId
                           where ig.Id == implementationGuideId
                           select new LookupTemplateType()
                           {
                               Id = tt.Id,
                               Name = tt.Name,
                               ImplementationGuideType = tt.ImplementationGuideType.Name,
                               RootContext = tt.RootContext,
                               RootContextType = tt.RootContextType
                           }).ToList();
            return results;
        }

        public static List<LookupTemplateType> GetTemplateTypes(int implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplateTypes(tdb, implementationGuideId);
            }
        }
    }
}
