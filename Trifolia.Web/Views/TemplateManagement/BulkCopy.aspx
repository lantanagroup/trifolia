<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="BulkCopyContainer">
        <h2>Bulk Copy: Step <span data-bind="text: CurrentStep"></span></h2>
    
        <!-- Step 1 -->
        <div data-bind="if: CurrentStep() == 1">
            <div class="form-group">
                <label>Excel File</label>
                <input type="file" class="form-control" data-bind="file: UploadData().ExcelFileInfo, fileBinaryData: UploadData().ExcelFile, fileObjectURL: UploadData().ExcelFileUrl" />
                <p class="help-block">This is the excel file that contains the template names, oids, etc. in the first sheet, and the constraint changes in the second sheet.</p>
            </div>

            <div class="form-group">
                <label>Configuration File</label>
                <input type="file" class="form-control" data-bind="file: UploadData().ConfigFileInfo, fileBinaryData: UploadData().ConfigFile, fileObjectURL: UploadData().ConfigFileUrl" />
                <p class="help-block">If you have already executed this bulk copy of templates before, this is the configuration file made available for you to download from the last step of the bulk copy. Providing this file will automatically map the excel columns and cells to Trifolia fields to match the same as the last time you ran the bulk copy.</p>
            </div>
            
            <div class="form-group">
                <input type="checkbox" data-bind="checked: UploadData().FirstRowIsHeader" /> First row is header
            </div>
        </div>

        <!-- Step 2 -->
        <div data-bind="if: CurrentStep() == 2">
            <p>The following fields are required to proceed:</p>
            <ul>
                <li>Template Name</li>
                <li>Template OID</li>
                <li>Template Bookmark</li>
            </ul>

            <div class="form-group">
                <label>Template meta-data</label>
                <select class="form-control" data-bind="options: TemplateExcelSheets, value: TemplateMetaDataSheet"></select>
            </div>

            <div class="form-group">
                <label>Constraint changes</label>
                <select class="form-control" data-bind="options: ConstraintExcelSheets, value: ConstraintChangesSheet"></select>
            </div>

            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#templateFields" data-toggle="tab">Template Fields</a></li>
                <li><a href="#constraintFields" data-toggle="tab">Constraint Fields</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="templateFields">
                    <!-- ko if: TemplateMetaDataSheet() != '' -->
                    <!-- ko foreach: ExcelFieldsForTemplateMetaData() -->
                    <div class="row">
                        <div class="col-md-3">
                            <div class="pull-right">Column "<span data-bind="text: Name"></span>"</div>
                        </div>
                        <div class="col-md-3">
                            <select class="form-control" data-bind="options: $parent.AvailableMappingFields($parent.TemplateMetaDataSheet(), $parent.TemplateFields(), MappedField()), optionsText: 'Name', optionsValue: 'Value', value: MappedField"></select>
                        </div>
                    </div>
                    <!-- /ko -->
                    <!-- /ko -->
                </div>
                <div class="tab-pane" id="constraintFields">
                    <!-- ko if: ConstraintChangesSheet -->
                    <!-- ko foreach: ExcelFieldsForConstraintChanges() -->
                    <div class="row">
                        <div class="col-md-3">
                            <div class="pull-right">Column "<span data-bind="text: Name"></span>"</div>
                        </div>
                        <div class="col-md-3">
                            <select class="form-control" data-bind="options: $parent.AvailableMappingFields($parent.ConstraintChangesSheet(), $parent.ConstraintFields(), MappedField()), optionsText: 'Name', optionsValue: 'Value', value: MappedField" style="width: 150px;"></select>
                        </div>
                    </div>
                    <!-- /ko -->
                    <!-- /ko -->
                </div>
            </div>
        </div>

        <!-- Step 3 -->
        <div data-bind="if: CurrentStep() == 3">
            <!-- ko if: ExistingTemplates().length > 0 -->
            <p style="text-decoration: underline">Already-existing template OIDs:</p>
            <ul data-bind="foreach: ExistingTemplates()">
                <li><span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)</li>
            </ul>
            <p>
                <input type="checkbox" data-bind="checked: ConfirmOverwrite" /> I am aware that the templates listed above will be deleted and re-created as a result of this bulk copy. This will <em>likely</em> result in different conformance numbers on the re-created templates.
            </p> 
            <!-- /ko -->

            <!-- ko if: ExternalTemplates().length > 0 -->
            <p><span style="text-decoration: underline">External Templates:</span><br /><small>Templates that would be re-created and are not owned by the same IG as the source template.</small></p>
            <ul data-bind="foreach: ExternalTemplates()">
                <li><span data-bind="text: Name"></span> (<span data-bind="    text: Oid"></span>)</li>
            </ul>

            <p style="color: red">You cannot continue this bulk copy with these external templates. You can only re-create templates that are owned by the same implementation guide as the source template.</p>
            <!-- /ko -->

            <p data-bind="if: ExistingTemplates().length == 0">
                <em>All templates listed in the excel file will be created new and do not already exist.</em>
            </p>

            <p style="text-decoration: underline">Mapping Configuration:</p>
            <p>Download <a href="#" data-bind="click: DownloadConfigClicked">this config</a> to repeat this bulk copy easily in the future.</p>
        </div>

        <!-- Step 4 -->
        <div data-bind="if: CurrentStep() == 4">
            <!-- ko if: IsCopying -->
                <h3>Please wait while the template is copied</h3>
            <!-- /ko -->

            <ul style="color: red;" data-bind="foreach: CopyResults().Errors()">
                <li data-bind="text: $data"></li>
            </ul>

            <!-- ko if: CopyResults().Errors.length == 0 -->
            <ul data-bind="foreach: CopyResults().NewTemplates()">
                <li>Created template <a href="#" data-bind="attr: { href: Url }"><span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)</a></li>
            </ul>
            <!-- /ko -->
        </div>

        <div class="btn-group">
            <button type="button" class="btn btn-primary" data-bind="click: NextButtonClicked, text: NextButtonText, enable: NextButtonEnabled"></button>
            <button type="button" class="btn btn-default" data-bind="click: CancelClicked">Cancel</button>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/TemplateManagement/BulkCopy.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var bulkCopyModel;

        $(document).ready(function () {
            var templateId = <%= Model %>;
            bulkCopyModel = new BulkCopyModel(templateId);
            ko.applyBindings(bulkCopyModel, $("#BulkCopyContainer")[0]);
        });
    </script>
</asp:Content>
