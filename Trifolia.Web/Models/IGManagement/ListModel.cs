using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class ListModel
    {
        public ListModel()
        {
            this.Organizations = new List<FilterItem>();
            this.Items = new List<ImplementationGuideItem>();
            this.Statuses = new List<Status>();
        }

        #region Properties

        public int? FocusImplementationGuideId { get; set; }
        public bool HideOrganization { get; set; }
        public List<FilterItem> Organizations { get; set; }
        public List<Status> Statuses { get; set; }
        public List<ImplementationGuideItem> Items { get; set; }

        #endregion

        public class ImplementationGuideItem
        {
            #region Properties

            public int Id { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public string Organization { get; set; }
            public DateTime? PublishDate { get; set; }
            public string Status { get; set; }
            public string Url { get; set; }
            public bool CanEdit { get; set; }
            public string DisplayName { get; set; }

            #endregion
        }

        public class Status
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}