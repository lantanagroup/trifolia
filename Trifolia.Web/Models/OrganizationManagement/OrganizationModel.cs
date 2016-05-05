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
            this.Groups = new List<OrganizationGroup>();
            this.Users = new List<OrganizationUser>();
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

        /// <summary>
        /// Gets all OrganizationGroups in the current collection, or adds the passed in groups to the collection
        /// </summary>
        public List<OrganizationGroup> Groups { get; set; }
        public List<OrganizationUser> Users { get; set; }
        
        #endregion
    }
}