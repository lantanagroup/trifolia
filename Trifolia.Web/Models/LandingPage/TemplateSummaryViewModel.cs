using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web.Models.LandingPage
{
    public class TemplateSummaryViewModel
    {
        #region Private Ctor

        /// <summary>
        /// Hide the ctor so we can enforce usage of the factory method, so we can "fix up" the model as necessary
        /// </summary>
        private TemplateSummaryViewModel()
        {
        }

        #endregion

        #region Public Properties

        public int Id { get; set; }
        public string Oid { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string IgName { get; set; }
        public int NumberOfConstraints { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new instance of <see cref="TemplateSummaryViewModel"/> from the passed in Template
        /// </summary>
        /// <param name="aTemplate"></param>
        /// <returns></returns>
        public static TemplateSummaryViewModel AdaptFromTemplate(Template aTemplate)
        {
            return new TemplateSummaryViewModel() 
                { 
                    TemplateName = aTemplate.Name,
                    Description = !string.IsNullOrEmpty(aTemplate.Description)? aTemplate.Description.Substring(0, Math.Min(aTemplate.Description.Length, 400)) + "...": string.Empty,
                    Id = aTemplate.Id,
                    IgName = aTemplate.OwningImplementationGuide.Name,
                    Oid = aTemplate.Oid,
                    NumberOfConstraints = aTemplate.ChildConstraints.Count
                };
        }

        #endregion
    }
}