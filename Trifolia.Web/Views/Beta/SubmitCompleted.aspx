<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Application Submitted</h2>

    <p>
        Thank you, <%=ViewData["User"] %> for submitting your application to join our closed beta for Trifolia's Read/Write version. We will contact successful applicants via [email?]. By submitting your application, you certify that you are over 18 years old and will be willing to sign a Beta User Agreement should you be selected for testing.
    </p>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
