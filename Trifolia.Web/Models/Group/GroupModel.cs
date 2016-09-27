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

        public string Description { get; set; }

        public string Disclaimer { get; set; }

        public bool IsManager { get; set; }

        public bool IsOpen { get; set; }

        #endregion

        public GroupModel() { }

        public GroupModel(Trifolia.DB.Group group)
        {
            this.Id = group.Id;
            this.Name = group.Name;
            this.Description = group.Description;
            this.Disclaimer = group.Disclaimer;
            this.IsOpen = group.IsOpen;
        }

        public GroupModel(Trifolia.DB.Group group, Trifolia.DB.User currentUser)
            : this(group)
        {
            this.IsManager = group.Managers.Count(y => y.UserId == currentUser.Id) > 0;
        }
    }
}