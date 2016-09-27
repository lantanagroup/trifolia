using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public class LookupPublishStatus
    {
        #region Ctor

        public LookupPublishStatus()
        {
            this.Name = string.Empty;
        }

        #endregion

        #region Public Properties

        public int Id { get; set; }

        public string Name { get; set; }

        #endregion

        #region Public Methods

        public List<PublishStatus> GetPublishStatusAll()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.PublishStatuses.ToList();
            }
        }

        public List<PublishStatus> GetFilteredPublishStatuses(string aExcludeStatuses)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                List<PublishStatus> lStatuses = tdb.PublishStatuses.ToList();

                if (!string.IsNullOrEmpty(aExcludeStatuses))
                {
                    foreach (string lRemovedStatus in aExcludeStatuses.Split(','))
                    {
                        PublishStatus lStatus = lStatuses.DefaultIfEmpty(null).SingleOrDefault(s => s.Status == lRemovedStatus);
                        if (lStatus != null) lStatuses.Remove(lStatus);
                    }
                }

                return lStatuses;
            }
        }

        public List<PublishStatus> GetPublishStatusesForTemplate(int? implementationGuideId)
        {
            if (implementationGuideId == null)
                return GetFilteredPublishStatuses(null);

            List<string> excludedStatuses = new List<string>();

            using (IObjectRepository tdb = DBContext.Create())
            {
                ImplementationGuide ig = tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
                var excludedStatusObjects = tdb.PublishStatuses.Where(y => 
                    y.Id != ig.PublishStatusId && 
                    y.Status != PublishStatus.DEPRECATED_STATUS && 
                    y.Status != PublishStatus.RETIRED_STATUS);

                foreach (PublishStatus current in excludedStatusObjects)
                {
                    excludedStatuses.Add(current.Status);
                }
            }

            return GetFilteredPublishStatuses(String.Join(",", excludedStatuses));
        }

        #endregion
    }
}