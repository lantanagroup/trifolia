using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.RoleManagement
{
    /// <summary>
    /// Model used by the REST API to return all roles and securables to the client
    /// </summary>
    public class RolesModel
    {
        public RolesModel()
        {
            this.Roles = new List<RoleItem>();
            this.AllSecurables = new List<SecurableItem>();
        }

        /// <summary>
        /// List of all roles in Trifolia
        /// </summary>
        public List<RoleItem> Roles { get; set; }

        /// <summary>
        /// List of all securables in Trifolia
        /// </summary>
        public List<SecurableItem> AllSecurables { get; set; }

        /// <summary>
        /// The default role to assign to new users
        /// </summary>
        public int? DefaultRoleId { get; set; }

        /// <summary>
        /// An individual role, including securables the role is assigned to and organizations
        /// </summary>
        public class RoleItem
        {
            public RoleItem()
            {
                this.AssignedSecurables = new List<SecurableItem>();
                this.Organizations = new List<Organization>();
            }

            /// <summary>
            /// The id of the role
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the role
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// All securables currently assigned to the role
            /// </summary>
            public List<SecurableItem> AssignedSecurables { get; set; }

            /// <summary>
            /// All organizations associated with the role
            /// </summary>
            public List<Organization> Organizations { get; set; }
        }

        /// <summary>
        /// A model representing an organization and how it is used within a role
        /// </summary>
        public class Organization
        {
            /// <summary>
            /// The id of the organization
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the organization
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Whether the organization is restricted from usign the role it is associated with
            /// </summary>
            public bool Restricted { get; set; }
        }

        /// <summary>
        /// A model representing an individual securable within Trifolia
        /// </summary>
        public class SecurableItem
        {
            /// <summary>
            /// The id of the securable
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The key of the securable (must be unique)
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// The name of the securable
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The description of the securable
            /// </summary>
            public string Description { get; set; }
        }
    }
}