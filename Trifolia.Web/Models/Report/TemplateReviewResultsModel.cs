using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class TemplateReviewResultsModel
    {
        public TemplateReviewResultsModel()
        {
            this.Items = new List<TemplateReviewModel>();
        }

        public int Total { get; set; }
        public IEnumerable<TemplateReviewModel> Items { get; set; }
    }
}