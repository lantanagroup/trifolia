using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Authorization;

namespace Trifolia.Web.Models.Group
{
    public class MyGroupModel
    {
        public MyGroupModel()
        {
            this.Managers = new List<UserModel>();
            this.Members = new List<UserModel>();
        }

        public MyGroupModel(Trifolia.DB.Group group)
        {
            this.Id = group.Id;
            this.Name = group.Name;
            this.Description = group.Description;
            this.Disclaimer = group.Disclaimer;
            this.IsOpen = group.IsOpen;

            this.Members = group.Users.OrderBy(y => y.User.FirstName).ThenBy(y => y.User.LastName).Select(y => new UserModel(y.User)).ToList();
            this.Managers = group.Managers.OrderBy(y => y.User.FirstName).ThenBy(y => y.User.LastName).Select(y => new UserModel(y.User)).ToList();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Disclaimer { get; set; }

        public bool IsOpen { get; set; }

        public List<UserModel> Managers { get; set; }

        public List<UserModel> Members { get; set; }

        public class UserModel
        {
            public UserModel()
            {

            }

            public UserModel(Trifolia.DB.User user)
            {
                this.Id = user.Id;
                this.Name = string.Format("{0} {1}", user.FirstName, user.LastName);
            }

            public int Id { get; set; }
            
            public string Name { get; set; }
        }
    }
}