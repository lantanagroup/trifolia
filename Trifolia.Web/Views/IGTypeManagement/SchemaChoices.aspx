<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="SchemaChoices">
        <h2>Schema Choices</h2>

        <table class="table">
            <thead>
                <tr>
                    <th>Complex Type</th>
                    <th>Calculated Name</th>
                    <th>Defined Name</th>
                    <th>Documentation</th>
                </tr>
            </thead>
            <tbody data-bind="foreach: Choices">
                <tr>
                    <td>
                        <span data-bind="text: ComplexTypeName"></span>
                        <span class="glyphicon glyphicon-question-sign" data-bind="tooltip: GetChoiceTooltip, tooltipOptions: { html: true, trigger: 'click' }"></span>
                    </td>
                    <td data-bind="text: CalculatedName"></td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: DefinedName" />
                    </td>
                    <td data-bind="text: Documentation"></td>
                </tr>
            </tbody>
        </table>

        <div class="btn-group">
            <button type="button" class="btn btn-primary" data-bind="click: Save">Save</button>
            <a class="btn btn-default" href="/IGTypeManagement/List">Cancel</a>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/Type/SchemaChoices.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new SchemaChoicesViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#SchemaChoices')[0]);
        });
    </script>

</asp:Content>