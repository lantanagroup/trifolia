using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.User
{
    public class ValidateUmlsCredentialsRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}