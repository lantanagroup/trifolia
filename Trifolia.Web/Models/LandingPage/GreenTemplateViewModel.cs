using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web.Models.LandingPage
{
    public class GreenTemplateViewModel
    {
        #region Public Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public string TemplateName { get; set; }

        public string Oid { get; set; }

        public string TemplateType { get; set; }

        #endregion

        public static GreenTemplateViewModel AdaptFromGreenTemplate(GreenTemplate g)
        {
            return new GreenTemplateViewModel()
            {
                Id = g.Id,
                Name = g.Name,
                Oid = g.Template.Oid,
                TemplateName = g.Template.Name,
                TemplateType = g.Template.TemplateType.Name
            };
        }
    }
}