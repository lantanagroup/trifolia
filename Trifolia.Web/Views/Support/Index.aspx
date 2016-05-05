<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Index</h2>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <table width="100%">
        <tr>
            <td width="200">
                Name:
            </td>
            <td>
                <table>
                    <tr>
                        <td><label for="NameText">Your Name:</label></td>
                        <td>
                            <dx:ASPxTextBox ID="NameText" runat="server" Width="300px" 
                                ClientInstanceName="nameText"
                                MaxLength="255">
                            </dx:ASPxTextBox>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
