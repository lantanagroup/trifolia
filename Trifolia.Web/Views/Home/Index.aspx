<%@ Page Title="Trifolia Workbench - Home" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" CodeBehind="~/Views/Home/Index.aspx.cs" Inherits="Trifolia.Web.Views.Home.Index" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Welcome to Trifolia Workbench!
    </h2>

    <% if (Model.DisplayInternalTechSupportPanel) { %>
    <div class="panel">
        <div class="panel-body">Technical issues can be reported <a href="https://jira.lantanagroup.com/browse/TDBMGMT" target="_support"></a></div>
    </div>
    <% } %>

    <div class="panel">
        <div class="panel-heading">Did you know?</div>
        <div class="panel-body"><%# Model.DidYouKnowTip %></div>
    </div>
</asp:Content>