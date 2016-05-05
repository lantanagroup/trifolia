using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Trifolia.Web
{
    public static class PageExtensions
    {
        #region Public Methods

        /// <summary>
        /// Shows the passed in message to the user as a client-side message
        /// </summary>
        /// <param name="aPage"></param>
        /// <param name="aMessage"></param>
        public static void ShowClientMessage(this Page aPage, string aMessage)
        {
            string lMessage = "<script type='text/javascript'>showMessage(1, '" + aMessage + "');</script>";

            aPage.ClientScript.RegisterStartupScript(
                aPage.GetType(), "ChangesSaved", lMessage);
        }

        public static void ShowClientWarning(this Page aPage, string aMessage, string key)
        {
            string lMessage = "<script type='text/javascript'>showMessage(2, '" + aMessage + "');</script>";

            aPage.ClientScript.RegisterStartupScript(
                aPage.GetType(), key, lMessage);
        }

        public static void ShowClientWarning(this Page aPage, string aMessage)
        {
            string lMessage = "<script type='text/javascript'>showMessage(2, '" + aMessage + "');</script>";

            aPage.ClientScript.RegisterStartupScript(
                aPage.GetType(), "ChangesSaved", lMessage);
        }

        public static void ShowClientError(this Page aPage, string aMessage)
        {
            string lMessage = "<script type='text/javascript'>showMessage(3, '" + aMessage + "');</script>";

            aPage.ClientScript.RegisterStartupScript(
                aPage.GetType(), "ChangesSaved", lMessage);
        }

        #endregion
    }
}