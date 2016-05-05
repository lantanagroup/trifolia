<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2><asp:Label ID="TestTitle" runat="server" Text="<%$ Resources:TrifoliaLang, PageTitle %>"></asp:Label></h2>

    <p>
        <asp:Label ID="TestLabel" runat="server" Text="<%$ Resources:TrifoliaLang, PageIntro %>"></asp:Label>
    </p>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>