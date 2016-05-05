using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Trifolia.Web
{
    public interface IRestrictedForm
    {
        Control MainPanel { get; }
        Control AuthorizationErrorPanel { get; }
        string RequiredRole { get; }
    }
}
