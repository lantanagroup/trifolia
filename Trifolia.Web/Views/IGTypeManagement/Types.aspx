<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="ViewTypes">
        <h2>Implementation Guide Types</h2>

        <table class="table">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Schema</th>
                    <th>&nbsp;</th>
                </tr>
            </thead>
            <tbody data-bind="foreach: Types">
                <tr>
                    <td data-bind="text: Name"></td>
                    <td data-bind="text: Schema"></td>
                    <td>
                        <div class="pull-right">
                            <a class="btn btn-primary" data-bind="attr: { href: '/IGTypeManagement/SchemaChoice/' + Id() }">Schema Choices</a>
                        </div>
                    </td>
                </tr>
            </tbody>
            <tfoot data-bind="if: Types().length == 0">
                <tr>
                    <td colspan="3">No implementation guide types</td>
                </tr>
            </tfoot>
        </table>
    </div>

    <script type="text/javascript" src="/Scripts/Type/Types.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new ViewTypesViewModel();
            ko.applyBindings(viewModel, $('#ViewTypes')[0]);
        });
    </script>

</asp:Content>
