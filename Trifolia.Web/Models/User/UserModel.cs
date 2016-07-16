using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.User
{
    /// <summary>
    /// Models a user as a view model
    /// </summary>
    /// <remarks>Although the underlying model allows us to link a user to many organizations, this view model represents 
    /// a single organization, and therefore in this context, a user either belongs to the current organization
    /// or not.</remarks>
    public class UserModel
    {
        #region Ctor

        public UserModel()
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