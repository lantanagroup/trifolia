using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Shared;
using Trifolia.Authentication;
using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Shared
{
    [Serializable]
    public class LookupImplementationGuide
    {
        #region Properties

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        private string organization;

        public string Organization
        {
            get { return organization; }
            set { organization = value; }
        }
        private DateTime? publishDate;

        public DateTime? PublishDate
        {
            get { return publishDate; }
            set { publishDate = value; }
        }

        #endregion

        public static List<LookupImplementationGuide> GetImplementationGuides(bool aIncludePublishedGuides = true)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetImplementationGuides(tdb, aIncludePublishedGuides);
            }
        }

        public static List<LookupImplementationGuide> GetImplementationGuides(IObjectRepository tdb, bool aIncludePublishedGuides = true)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return new List<LookupImplementationGuide>();

            List<ImplementationGuide> implementationGuides = tdb.ImplementationGuides
                 .OrderBy(y => y.Name)
                 .ToList();

            if (!aIncludePublishedGuides)
            {
                implementationGuides.RemoveAll(ig => ig.IsPublished());
            }

            return ConvertAndFilter(tdb, implementationGuides);
        }

        public static List<LookupImplementationGuide> GetImplementationGuidesExcluding(int? excludedImplementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                if (!CheckPoint.Instance.IsAuthenticated)
                    return new List<LookupImplementationGuide>();

                List<ImplementationGuide> implementationGuides = tdb.ImplementationGuides
                    .Where(y => excludedImplementationGuideId == null || y.Id != excludedImplementationGuideId.Value)
                    .ToList();

                return ConvertAndFilter(tdb, implementationGuides);
            }
        }

        private static List<LookupImplementationGuide> ConvertAndFilter(IObjectRepository tdb, List<ImplementationGuide> implementationGuides)
        {
            List<ImplementationGuide> filteredImplementationGuides = implementationGuides;

            // If the user is not an internal user, then only return a list of implementation guides where the user has A permission on it
            if (!CheckPoint.Instance.IsDataAdmin)
            {
                filteredImplementationGuides = (from ig in implementationGuides
                                                join p in tdb.ViewImplementationGuidePermissions on ig.Id equals p.ImplementationGuideId
                                                where p.UserId == CheckPoint.Instance.User.Id
                                                select ig)
                                                .Distinct()
                                                .ToList();
            }

            return (from ig in filteredImplementationGuides
                    select new LookupImplementationGuide()
                    {
                        Id = ig.Id,
                        Title = string.Format("{0} {1}", ig.Name, ig.Version > 1 ? "V" + ig.Version.ToString() : string.Empty),
                        Type = ig.ImplementationGuideType.Name,
                        Organization = ig.Organization != null ? ig.Organization.Name : null,
                        PublishDate = ig.PublishDate
                    }).ToList();
        }
    }
}
