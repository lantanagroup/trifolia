<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="DeleteImplementationGuide">
        <h2>Delete Implementation Guide</h2>

        <div data-bind="with: ImplementationGuide">
            <h3 data-bind="text: Name"></h3>

            <!-- ko if: NextVersionImplementationGuideId() -->
            <div class="alert alert-warning" role="alert">This implementation guide cannot be deleted because it has been versioned. In order to delete this implementation guide, you must delete the <a data-bind="attr: { href: '/IGManagement/View/' + NextVersionImplementationGuideId() }">new version</a> first.</div>
            <!-- /ko -->

            <p>This is an irreversable action. Deleting the implementation guide will remove it permanently. Any templates associated with the implementation guide will be removed as well, unless you specify a replacement implementation guide. Selecting a replacement implementation guide will move the templates to the replacement.</p>

            <p>
                <b>Templates: </b> <span data-bind="text: $parent.NumberOfTemplates"></span>
            </p>

            <div class="form-group">
                <select class="form-control" data-bind="value: $parent.ReplaceImplementationGuideId, options: $parent.ImplementationGuides, optionsText: 'Title', optionsValue: 'Id', optionsCaption: 'Select replacement...'"></select>
            </div>

            <div class="form-group">
                <button type="button" class="btn" data-bind="click: $parent.Delete, enable: $parent.CanDelete">Delete!</button>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/IGManagement/Delete.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new DeleteImplementationGuideViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#DeleteImplementationGuide')[0]);
        });
    </script>
</asp:Content>
