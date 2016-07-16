using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Group
{
    /// <summary>
    /// Models a group as a view model
    /// </summary>
    public class GroupModel
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}