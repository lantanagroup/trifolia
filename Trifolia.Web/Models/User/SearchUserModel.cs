using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.User
{
    public class SearchUserModel
    {
        public SearchUserModel()
        {

        }

        public SearchUserModel(Trifolia.DB.User user)
        {
            this.Id = user.Id;
            this.Name = string.Format("{0} {1}", user.FirstName, user.LastName);
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}