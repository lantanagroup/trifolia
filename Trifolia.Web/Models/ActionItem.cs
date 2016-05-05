using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models
{
    public class ActionItem
    {
        #region Properties

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        #endregion

        public ActionItem(string text, string name)
        {
            this.Text = text;
            this.Name = name;
        }

        public ActionItem(string text)
        {
            this.Text = text;
            this.Name = text;
        }
    }
}