using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class UserExtensions
    {
        public static bool HasValidUmlsLicense(this User user, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(user.UMLSUsername))
                username = user.UMLSUsername.DecryptStringAES();

            if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(user.UMLSPassword))
                password = user.UMLSPassword.DecryptStringAES();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            return UmlsHelper.ValidateLicense(username, password);
        }

        public static bool HasValidUmlsCredentials(this User user, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(user.UMLSUsername))
                username = user.UMLSUsername.DecryptStringAES();

            if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(user.UMLSPassword))
                password = user.UMLSPassword.DecryptStringAES();

            if (string.IsNullOrEmpty(user.UMLSUsername) || string.IsNullOrEmpty(user.UMLSPassword))
                return false;

            return UmlsHelper.ValidateCredentials(username, password);
        }
    }
}
