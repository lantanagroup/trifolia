using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.OrganizationManagement
{
    /// <summary>
    /// Models a staqndard organization and user relationship as a view model
    /// </summary>
    /// <remarks>Although the underlying model allows us to link a user to many organizations, this view model represents 
    /// a single organization, and therefore in this context, a user either belongs to the current organization
    /// or not.</remarks>
    public class OrganizationUser
    {
        #region Ctor

        public OrganizationUser()
        {
            this.Roles = new List<int>();
            this.Groups = new List<int>();
        }

        #endregion

        #region Properties

        public IEnumerable<int> Roles { get; set; }
        public IEnumerable<int> Groups { get; set; }
       
        public int? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }

        #endregion
    }
}