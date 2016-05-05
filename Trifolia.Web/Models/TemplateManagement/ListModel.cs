using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web.Models.TemplateManagement
{
    /// <summary>
    /// A model returned by the REST API to represent a list of templates
    /// </summary>
    public class ListModel
    {
        public ListModel()
        {
            this.Items = new List<BigListItem>();
        }

        #region Properties

        /// <summary>
        /// Indicates if the organization should be hidden from the screen
        /// </summary>
        public bool HideOrganization { get; set; }

        /// <summary>
        /// Indicates if the user can (role-based) view templates
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// Indicates if the user can (role-based) edit templates
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// A list of templates
        /// </summary>
        public List<BigListItem> Items { get; set; }

        /// <summary>
        /// The number of items included in the list
        /// </summary>
        public int TotalItems { get; set; }

        #endregion
    }
}