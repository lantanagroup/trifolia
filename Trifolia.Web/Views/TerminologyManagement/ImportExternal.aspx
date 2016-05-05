<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="ImportExternal">
        <h2>Import Terminology from External Sources</h2>
        
        <label>Source</label>
        <div class="input-group">
            <input type="radio" value="roseTree" data-bind="checked: SearchSource" /> Rose Tree
            <input type="radio" value="phinVads" data-bind="checked: SearchSource" /> PHIN VADS
        </div>
        
        <label>Value Set OID</label>
        <div class="input-group">
            <input type="text" class="form-control" data-bind="value: SearchOid" autofocus="autofocus" />
            <div class="input-group-btn">
                <button type="button" class="btn btn-default" data-bind="click: Search">Search</button>
            </div>
        </div>

        <!-- ko with: ImportValueSet -->
        <div class="panel panel-default">
            <div class="panel-heading">
                <span data-bind="text: ImportStatus"></span>: <span data-bind="    text: Name"></span> (<span data-bind="    text: Oid"></span>)
            </div>
            <div class="panel-body">
                <!-- ko if: Description() -->
                <p><b>Description:</b> <span data-bind="text: Description"></span></p>
                <!-- /ko -->
                <!-- ko if: Code() -->
                <p><b>Code:</b> <span data-bind="text: Code"></span></p>
                <!-- /ko -->

                <table class="table">
                    <thead>
                        <tr>
                            <th>Import</th>
                            <th>Code</th>
                            <th>Display Name</th>
                            <th>Code System</th>
                            <th>Status</th>
                            <th>Status Date</th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: Members">
                        <tr>
                            <td data-bind="text: ImportStatus"></td>
                            <td data-bind="text: Code"></td>
                            <td data-bind="text: DisplayName"></td>
                            <td>
                                <span data-bind="text: CodeSystemName"></span> (<span data-bind="    text: CodeSystemOid"></span>)
                            </td>
                            <td data-bind="text: Status"></td>
                            <td data-bind="text: StatusDate"></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <button type="button" class="btn btn-primary" data-bind="click: $parent.Import">Import</button>
        <!-- /ko -->
    </div>

    <script type="text/javascript" src="/Scripts/TerminologyManagement/ImportExternal.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ImportExternalViewModel();
            ko.applyBindings(viewModel, $('#ImportExternal')[0]);
        });
    </script>

</asp:Content>
