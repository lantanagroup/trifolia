<%@ Page Title="Trifolia Workbench - Home" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <img src="/Images/frown_face.png" align="left" style="padding-right: 50px;" />
    <h2>Ooops!</h2>

    <p><%= ViewData["Message"] %></p>
</asp:Content>