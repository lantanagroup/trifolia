using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.LandingPage
{
    public class ConstraintViewModel
    {
        #region Public Properties

        public int? ID { get; set; }

        public string Name { get; set; }

        public bool IsConstrained { get; set; }

        #endregion
    }
}