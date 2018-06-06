<%@ Page Title="Trifolia Workbench - Home" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.HomeModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Welcome to Trifolia Workbench!</h2>

    <% if (!string.IsNullOrEmpty(Model.Message)) { %>
    <div class="alert alert-info"><%= Model.Message %></div>
    <% } %>

    <div id="intro"></div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="/Scripts/Home/LoggedIn.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var versionNumber = "<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>";

        $(document).ready(function () {
            loadHelpTopics(versionNumber);
        });
    </script>
</asp:Content>