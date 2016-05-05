<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="../../Scripts/Export/Green.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <form id="ExportGreenForm" method="post">
        <div id="ExportGreen">
            <h2>Export Green</h2>
            <h3><a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }, text: Name"></a></h3>
    
            <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />

            <p data-bind="if: Templates().length == 0">No document templates exist within this implementation guide that have green models. Cannot generate green artifacts for this implementation guide.</p>

            <div class="form-group">
                <label>Document Level Template</label>
                <select class="form-control" name="RootTemplateId" data-bind="value: RootTemplateId, options: Templates, optionsText: 'Title', optionsValue: 'Id', optionsCaption: 'Select...'"></select>
            </div>

            <div class="form-group">
                <label>Data Types</label>
                <input type="checkbox" name="SeparateDataTypes" value="true" /> Separate data types from the main green schema?
            </div>
    
            <button type="button" class="btn btn-primary" data-bind="click: Export">Export</button>
        </div>
    </form>

    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ExportGreenViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#ExportGreen')[0]);
        });
    </script>

</asp:Content>
