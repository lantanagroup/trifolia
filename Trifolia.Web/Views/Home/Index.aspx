<%@ Page Title="Trifolia Workbench - Home" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.HomeModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Welcome to Trifolia Workbench!</h2>

    <p>
        <strong>Did you know?</strong>
        <br />
        <label><%= Model.DidYouKnowTip %></label>
    </p>

    <div id="whatsnew"></div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="/Scripts/Home/LoggedIn.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var versionNumber = "<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>";

        $(document).ready(function () {
            LoadWhatsNew(versionNumber);
        });
    </script>
</asp:Content>