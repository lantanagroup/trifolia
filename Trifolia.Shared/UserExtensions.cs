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
        public static bool HasValidUMLSApiKey(this User user, string apiKey = null)
        {
            if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(user.UMLSApiKey))
                apiKey = user.UMLSApiKey.DecryptStringAES();

            if (string.IsNullOrEmpty(apiKey))
                return false;

            return UmlsHelper.ValidateLicense(apiKey);
        }
    }
}
