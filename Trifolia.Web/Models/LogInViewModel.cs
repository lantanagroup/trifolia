using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models
{
    /// <summary>
    /// Provides data for the view that's when the user should log in
    /// </summary>
    public class LogInViewModel : HomeModel
    {
        #region Properties

        public string HL7LoginLink
        {
            get;
            set;
        }

        #endregion
    }
}