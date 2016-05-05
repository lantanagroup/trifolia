<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="ImportExcel">
        <h2>Import Value Sets from Excel</h2>
        
        <div class="well">
            <label>Excel File</label>
            <div class="input-group">
                <input type="file" class="form-control" data-bind="file: FileInfo, fileBinaryData: File" />
                <div class="input-group-btn">
                    <button type="button" class="btn btn-default" data-bind="click: Check">Check</button>
                </div>
            </div>
        </div>

        <!-- ko with: CheckResults -->
        <div class="well">
            <h3>Check Results</h3>
            <!-- ko if: Errors().length > 0 -->
            <div class="alert alert-warning" data-bind="foreach: Errors">
                <p data-bind="text: $data"></p>
            </div>
            <!-- /ko -->

            <!-- ko foreach: ValueSets -->
            <div class="panel panel-default">
                <div class="panel-heading">
                    <span data-bind="text: $parents[1].GetChangeType(ChangeType())"></span>: <span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)                    
                </div>
                <div class="panel-body">
                    <table class="table">
                        <thead>
                            <tr>
                                <th>Type</th>
                                <th>Code</th>
                                <th>Display</th>
                                <th>Code System</th>
                                <th>Status</th>
                                <th>Status Date</th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: Concepts">
                            <tr>
                                <td data-bind="text: $parents[2].GetChangeType(ChangeType())"></td>
                                <td data-bind="text: Code"></td>
                                <td data-bind="text: DisplayName"></td>
                                <td>
                                    <span data-bind="text: CodeSystemName"></span> (<span data-bind="text: CodeSystemOid"></span>)
                                </td>
                                <td data-bind="text: Status"></td>
                                <td data-bind="text: StatusDate"></td>
                            </tr>
                        </tbody>
                        <tfoot data-bind="if: Concepts().length == 0">
                            <tr>
                                <td colspan="7">No concepts</td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
            <!-- /ko -->
        </div>
        <!-- /ko -->

        <button type="button" class="btn btn-primary" data-bind="click: Import, disable: !CheckResults()">Import</button>
    </div>

    <script type="text/javascript" src="/Scripts/TerminologyManagement/ImportExcel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ImportExcelViewModel();
            ko.applyBindings(viewModel, $('#ImportExcel')[0]);
        });
    </script>

</asp:Content>