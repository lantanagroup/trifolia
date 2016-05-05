<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <div id="TemplateUsage">
        <h2>Template/Profile Usage</h2>

        <div data-bind="templateSelect: TemplateId"></div>

        <div data-bind="foreach: TemplateUsage">
            <h3><a data-bind="attr: { href: '/IGManagement/View/Id/' + ImplementationGuideId() }, text: ImplementationGuideName"></a></h3>

            <ul data-bind="foreach: Templates">
                <li>
                    <div class="dropdown">
                        <a href="#" data-toggle="dropdown"><span data-bind="text: FullName"></span> <span class="caret"></span></a>
                        <ul class="dropdown-menu">
                            <li><a href="#" data-bind="click: function () { $parents[1].ViewUsage($data); }">View Usage</a></li>
                            <li><a href="#" data-bind="click: function () { $parents[1].ViewTemplate($data); }">View Template</a></li>
                        </ul>
                        <span class="badge" data-bind="text: Type"></span>
                    </div>
                </li>
            </ul>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/Report/TemplateUsage.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new TemplateUsageViewModel();
            ko.applyBindings(viewModel, $('#TemplateUsage')[0]);
        });
    </script>
</asp:Content>
