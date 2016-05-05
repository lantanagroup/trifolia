<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="/Scripts/knockout-2.1.0.js"></script>
    <script src="/Scripts/jquery-1.8.0.min.js"></script>
    <script src="/Scripts/jquery-ui-1.9.2.custom.min.js"></script>
    <script src="/Scripts/jquery.blockUI.js"></script>

    <script src="/Scripts/TemplateSearch.ko.extensions.js"></script>
    <script src="/Scripts/TeamplateLandingPageViewModel.js"></script>

    <link href="/Styles/easyui-themes/default/easyui.css" rel="stylesheet" type="text/css">
    <link href="/Styles/ui-lightness/jquery-ui-1.9.2.custom.min.css" rel="stylesheet" />
    <link href="/Styles/TemplateList.css" rel="stylesheet" />

    <div id="recentTemplatesDiv">
        <h2>Templates you recently looked at</h2>

        <table id="recentTemplatesTable" class="recentTemplates" data-img-url="<%= Url.Content("/Images/spinner.gif") %>">
            <thead>
                <tr>
                    <th>Template Name</th>
                    <th>Oid</th>
                </tr>
            </thead>

            <tbody data-bind="foreach: templateModel.recentTemplates">
                <tr class="datagrid-row">
                    <td class="datagrid-cell">
                        <a data-bind="attr: {href: '/Account/Browse/EditTemplate.aspx?id=' + Id, title: 'View ' + TemplateName}, text: TemplateName" target="_blank"></a>
                    </td>
                    <td data-bind="text: Oid"></td>
                </tr>
            </tbody>

        </table>
    </div>

    <p>
        <h2>Find a template by searching</h2>
    </p>

    <div id="searchDiv">
        <div>
            <table cellpadding="1px" cellspacing="0px" width="60%">
                <tr>
                    <td class="searchRow">
                        <input type="text" id="searchInput" 
                            placeholder = "Find a Template" class="searchBox" 
                            data-bind="value: aQuery, ko_autocomplete: { source: '<%=Url.Action("GetTemplateNameMatches")%>', select: valueSelected, minLength: 3 }"
                            data-autocomplete-url="<%=Url.Action("GetTemplateNameMatches")%>" />
                    </td>
                    <td>
                        <input type="submit" value="Find Templates" id="btnSearch" data-bind="click: search" data-img-url="<%= Url.Content("/Images/spinner.gif") %>" />
                    </td>
                </tr>
            </table>
        </div>
        <div id="Div1" data-bind="slideVisible: displayResults">
            <table class="searchResults">
                <thead class="searchResultsHeader">
                    <tr>
                        <th>
                            Template Name
                        </th>
                        <th>
                            Template OID
                        </th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: templateModel.templates">
                    <tr>
                        <td class="oid">
                            <a data-bind="attr: {href: '/Account/Browse/EditTemplate.aspx?id=' + Id, title: 'View ' + TemplateName}, text: TemplateName" target="_blank"></a>
                        </td>
                        <td data-bind="text: Oid"></td>
                        <td data-bind="text: Description"></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <div>
        <table></table>
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
