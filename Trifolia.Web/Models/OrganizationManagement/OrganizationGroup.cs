using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.OrganizationManagement
{
    /// <summary>
    /// Models a standard Group within an organization
    /// </summary>
    public class OrganizationGroup
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}