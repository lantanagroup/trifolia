<%@ Import Namespace="Trifolia.Web.Models.TemplateManagement" %>
<%@ Import Namespace="Trifolia.Generation.Versioning" %>
<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.IGManagement.ViewChangesModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Scripts/IGManagement/ViewIgChanges.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Changes for <%:Model.IgName %></h2>

<%
    if (Model.TemplateDifferences == null || Model.TemplateDifferences.Count == 0)
    {
%>
<p style="font-weight: bold;">No templates have been modified in this implementation guide yet.</p>
<% 
    }
%>

<h3>Modified</h3>

<%
    Html.RenderPartial("ViewIgChangesPartial", Model.TemplateDifferences.Where(y => !y.IsAdded).OrderBy(y => y.TemplateName));
%>
    
<h3>Added</h3>
<%
    Html.RenderPartial("ViewIgChangesPartial", Model.TemplateDifferences.Where(y => y.IsAdded).OrderBy(y => y.TemplateName));
%>
</asp:Content>
