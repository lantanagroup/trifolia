<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">

    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ImportContainer" style="display: none;">
        <div class="form-group">
            <label>Import File (XML)</label>
            <input type="file" class="form-control" data-bind="file: ImportFileInfo, fileBinaryData: ImportContent" />
        </div>

        <p>
            <button type="button" class="btn btn-primary" data-bind="click: Import, disable: DisableImportButton">Import Now</button>
        </p>
        
        <!-- ko if: IsImporting() -->
        <div class="alert alert-info">
            Importing...
        </div>
        <!-- /ko -->

        <!-- ko if: ImportResults() -->
        <!-- ko if: ImportResults().Success -->
        <p class="label label-success">Successfully imported</p>
        <!-- /ko -->
        <!-- ko if: !ImportResults().Success -->
        <p class="label label-warning">Import failed</p>
        <!-- /ko -->

        <div class="panel panel-default" data-bind="if: ImportResults().Messages.length > 0">
            <div class="panel-heading">Messages</div>
            <table class="table">
                <tbody data-bind="foreach: ImportResults().Messages">
                    <tr>
                        <td data-bind="text: $data"></td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="panel panel-default">
            <div class="panel-heading">Import Results</div>
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>Name/Identifier</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    <!-- ko foreach: ImportResults().ImplementationGuides -->
                    <tr>
                        <td>Implementation Guide</td>
                        <td>
                            <span data-bind="text: Name"></span> (<span data-bind="text: Version"></span>)
                        </td>
                        <td data-bind="text: Status"></td>
                    </tr>
                    <!-- /ko -->

                    <!-- ko foreach: ImportResults().Templates -->
                    <tr>
                        <td>Template</td>
                        <td>
                            <span data-bind="text: Name"></span> (<span data-bind="text: Identifier"></span>)
                            <!-- ko if: !Expanded() --><i class="glyphicon glyphicon-chevron-up clickable" data-bind="click: function () { Expanded(true); }"></i><!-- /ko -->
                            <!-- ko if: Expanded() --><i class="glyphicon glyphicon-chevron-down clickable" data-bind="click: function () { Expanded(false); }"></i><!-- /ko -->
                        </td>
                        <td data-bind="text: $parents[$parents.length - 1].GetTemplateStatus($data)"></td>
                    </tr>

                    <!-- ko if: Expanded() -->
                    <tr>
                        <td>&nbsp;</td>
                        <td colspan="2">
                            <strong>Constraints and Constraint Samples</strong>
                            <table class="table">
                                <tbody>
                                    <!-- ko foreach: Constraints -->
                                    <tr>
                                        <td>Constraint</td>
                                        <td data-bind="text: Number"></td>
                                        <td data-bind="text: Status"></td>
                                    </tr>

                                    <!-- ko foreach: Samples -->
                                    <tr>
                                        <td>Constraint Sample</td>
                                        <td data-bind="text: Name"></td>
                                        <td data-bind="text: Status"></td>
                                    </tr>
                                    <!-- /ko -->
                                    <!-- /ko -->
                                </tbody>
                            </table>
                            
                            <strong>Template Samples</strong>
                            <table class="table">
                                <tbody>
                                    <!-- ko foreach: Samples -->
                                    <tr>
                                        <td data-bind="text: Name"></td>
                                        <td data-bind="text: Status"></td>
                                    </tr>
                                    <!-- /ko -->
                                </tbody>
                            </table>
                        </td>
                    </tr>
                    <!-- /ko -->

                    <!-- /ko -->

                    <!-- ko foreach: ImportResults().ValueSets -->
                    <tr>
                        <td>Value Set</td>
                        <td><span data-bind="text: Name"></span> (<span data-bind="text: Identifier"></span>)</td>
                        <td>Added</td>
                    </tr>
                    <!-- /ko -->
                    
                    <!-- ko foreach: ImportResults().CodeSystems -->
                    <tr>
                        <td>Code System</td>
                        <td><span data-bind="text: Name"></span> (<span data-bind="text: Identifier"></span>)</td>
                        <td>Added</td>
                    </tr>
                    <!-- /ko -->
                </tbody>
            </table>
        </div>
        <!-- /ko -->
    </div>
    
    <script type="text/javascript" src="/Scripts/Import/Import.js"></script>
    <script type="text/javascript">
        var viewModel = new ImportViewModel();
        ko.applyBindings(viewModel, $("#ImportContainer")[0]);
        setTimeout(function () {
            $("#ImportContainer").show();
        });
    </script>
</asp:Content>