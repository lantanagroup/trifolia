using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.OrganizationManagement
{
    /// <summary>
    /// Models a standard organization as a view model
    /// </summary>
    public class OrganizationModel
    {
        #region Ctor

        public OrganizationModel()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Organization ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the organization name
        /// </summary>
        public string Name { get; set; }
        
        #endregion
    }
}