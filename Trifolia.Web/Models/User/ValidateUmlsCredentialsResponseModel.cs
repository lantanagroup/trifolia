using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.User
{
    public class ValidateUmlsCredentialsResponseModel
    {
        public bool CredentialsValid { get; set; }
        public bool LicenseValid { get; set; }
    }
}