<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Template Validation</h3>
    <div id="TemplateValidation">
        <div class="form-group">
            <div class="input-group">
                <select class="form-control" data-bind="value: ImplementationGuideId, options: ImplementationGuides, optionsText: 'Title', optionsValue: 'Id', optionsCaption: 'Select Implementation Guide'"></select>
                <div class="input-group-btn">
                    <button type="button" class="btn btn-default" data-bind="disable: !ImplementationGuideId(), click: ValidateTemplates"><i class="glyphicon glyphicon-refresh"></i></button>
                </div>
            </div>
        </div>
        <div class="form-group">
            <div class="btn-group">
                <button type="button" class="btn btn-primary" data-bind="css: { 'active': ShowLevel() == 'all' }, click: function () { ShowLevel('all'); }">All</button>
                <button type="button" class="btn btn-default" data-bind="css: { 'active': ShowLevel() == 'warnings' }, click: function () { ShowLevel('warnings'); }">Warnings</button>
                <button type="button" class="btn btn-default" data-bind="css: { 'active': ShowLevel() == 'errors' }, click: function () { ShowLevel('errors'); }">Errors</button>
            </div>
        </div>
        
        <!-- ko foreach: GetResults() -->
        <div class="panel panel-default">
            <div class="panel-heading">
                <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + Id() }"><span data-bind="    text: Name"></span> (<span data-bind="    text: Oid"></span>)</a>
            </div>
            <div class="panel-body">
                <table class="table">
                    <tbody data-bind="foreach: $parent.GetItems(Items())">
                        <tr>
                            <td data-bind="text: ConstraintNumber"></td>
                            <td data-bind="text: $parents[1].GetSeverityDisplay(Level())"></td>
                            <td data-bind="text: Message"></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <!-- /ko -->

        <!-- ko if: GetResults().length == 0 -->
        No validation messages!
        <!-- /ko -->
    </div>

    <script type="text/javascript" src="/Scripts/Report/TemplateValidation.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new TemplateValidationViewModel();
            ko.applyBindings(viewModel, $('#TemplateValidation')[0]);
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
