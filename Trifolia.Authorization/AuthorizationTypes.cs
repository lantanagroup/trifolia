using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Authorization
{
    public enum AuthorizationTypes
    {
        AuthorizationSuccessful,
        UserNotAuthorized,
        UserNonExistant
    }
}