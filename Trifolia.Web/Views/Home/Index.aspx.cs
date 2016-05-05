using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Views.Home
{
    public partial class Index : System.Web.Mvc.ViewPage<Trifolia.Web.Models.LogInViewModel>
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }
    }
}