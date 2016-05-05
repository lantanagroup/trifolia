using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web.Models.LandingPage
{
    public class ImplementationGuideSummaryViewModel
    {
        #region Public Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Organization { get; set; }

        public string PublishStatus { get; set; }

        public string PublishDate { get; set; }

        public int NumberOfChildTemplates { get; set; }
        #endregion

        public static ImplementationGuideSummaryViewModel AdaptFromImplementationGuide(ImplementationGuide ig)
        {
            string status = "Draft";

            if (ig.PublishStatus != null && ig.PublishStatus.Status != "")
                status = ig.PublishStatus.Status;

            return new ImplementationGuideSummaryViewModel()
            {
                Id = ig.Id,
                Name = ig.Name,
                Organization = (ig.Organization != null ? ig.Organization.Name : ""),
                Type = ig.ImplementationGuideType.Name,
                PublishDate = (ig.PublishDate.HasValue ? ig.PublishDate.Value.ToString("MM/dd/yyyy") : null),
                PublishStatus = status,
                NumberOfChildTemplates = ig.ChildTemplates.Count
            };
        }
    }
}