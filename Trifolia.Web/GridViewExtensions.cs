using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Trifolia.Web
{
    public static class GridViewExtensions
    {
        public static void SetSelected(this GridView gridView, int id)
        {
            int selectedIndex = 0;
            int pageIndex = 0;
            int gridViewPages = gridView.PageCount;

            for (int cPage = 0; cPage < gridViewPages; cPage++)
            {
                gridView.PageIndex = cPage;
                gridView.DataBind();

                for (int i = 0; i < gridView.DataKeys.Count; i++)
                {
                    if ((int)gridView.DataKeys[i].Value == id)
                    {
                        selectedIndex = i;
                        pageIndex = cPage;
                        break;
                    }
                }
            }

            gridView.PageIndex = pageIndex;
            gridView.SelectedIndex = selectedIndex;
            gridView.DataBind();
        }
    }
}