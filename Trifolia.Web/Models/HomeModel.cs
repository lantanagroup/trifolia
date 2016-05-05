using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Trifolia.Web.Models
{
    public class HomeModel
    {
        #region Ctor

        public HomeModel()
        {
            this.WhatsNewMessages = new List<string>();
        }

        #endregion

        public List<string> WhatsNewMessages { get; private set; }

        public string DidYouKnowTip { get; set; }

        public bool DisplayInternalTechSupportPanel { get; set; }
    }
}