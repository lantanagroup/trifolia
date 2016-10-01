<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.IGManagement.FilesModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="/Scripts/IGManagement/Files.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="mainBody">
        <h2>Manage Implementation Guide Files</h2>
        <p data-bind="text: Name"></p>

        <table class="table">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Date</th>
                    <th>Description</th>
                    <th>
                        <div class="pull-right">
                            <button type="button" class="btn btn-default" data-bind="click: AddFileClicked">Add File</button>
                        </div>
                    </th>
                </tr>
            </thead>
            <tbody data-bind="foreach: FilesNotRemoved">
                <tr>
                    <td>
                        <a data-bind="attr: { href: '/IGManagement/Files/Download?versionId=' + VersionId() }"><span data-bind="text: Name"></span></a> 
                        [<a href="#" data-bind="click: function() { $parent.ViewHistory(FileId()); }">history</a>]
                    </td>
                    <td data-bind="text: $parent.GetTypeDisplay(Type())"></td>
                    <td data-bind="text: Date"></td>
                    <td data-bind="text: Description"></td>
                    <td>
                        <div class="pull-right">
                            <div class="btn-group">
                                <button type="button" class="btn btn-default" data-bind="click: function() { $parent.EditDescriptionClicked($data); }">Edit Description</button>
                                <button type="button" class="btn btn-default" data-bind="click: function() { $parent.RemoveFileClicked($data); }">Remove File</button>
                            </div>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>

        <div class="btn-group">
            <button type="button" class="btn btn-primary" data-bind="click: Save">Save</button>
            <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
        </div>

        <div class="modal fade" id="addFileDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Add File</h4>
                    </div>
                    <div class="modal-body" data-bind="with: NewFile">
                        <div class="form-group">
                            <label>File</label>
                            <input type="file" class="form-control" data-bind="file: File, fileBinaryData: Data, fileObjectURL: FileObjectURL" />
                        </div>

                        <div class="form-group">
                            <label>Type</label>
                            <select class="form-control" data-bind="value: Type, enable: TypeEnabled">
                                <option value="0">Implementation Guide Document</option>
                                <option value="1">Schematron</option>
                                <option value="2">Schematron Helper</option>
                                <option value="3">Vocabulary</option>
                                <option value="4">Sample (deliverable)</option>
                                <option value="5">Test Sample (good)</option>
                                <option value="6">Test Sample (bad)</option>
                                <option value="7">Data Snapshot (JSON)</option>
                                <option value="8">Image</option>
                                <option value="9">FHIR Resource Instance (XML or JSON)</option>
                            </select>
                        </div>

                        <!-- ko if: Type() == '7' -->
                        <div class="form-group" data-bind="validationOptions: { insertMessages: false }">
                            <label>Web Publication URL</label>
                            <div class="input-group">
                                <div class="input-group-addon">https://trifolia.lantanagroup.com/IG/Web/</div>
                                <input type="text" class="form-control" data-bind="value: Url" />
                            </div>
                            <span class="validationMessage" data-bind="validationMessage: Url"></span>
                        </div>
                        <!-- /ko -->

                        <div class="form-group">
                            <label>Description</label>
                            <textarea class="form-control" style="height: 50px;" data-bind="value: Description"></textarea>
                        </div>

                        <div class="form-group">
                            <label>Note</label>
                            <textarea class="form-control" style="height: 50px;" data-bind="value: Note"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: NewFile">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.AddFileOkClicked, enable: NewFileIsValid">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: $parent.AddFileCancelClicked">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="fileHistoryDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">File History</h4>
                    </div>
                    <div class="modal-body" data-bind="foreach: FileHistory">
                        <div class="row">
                            <div class="col-md-4" data-bind="text: Date"></div>
                            <div class="col-md-8" data-bind="text: Note"></div>
                        </div>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="editDescriptionDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Description</h4>
                    </div>
                    <div class="modal-body">
                        <textarea class="form-control" style="height: 100px" data-bind="value: TempDescription"></textarea>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: EditDescriptionOkClicked">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: EditDescriptionCancelClicked">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript">
        var filesModel = null;

        $(document).ready(function () {
            ko.validation.init({ messagesOnModified: false });

            var fileHistoryDialog = $("#fileHistoryDialog");
            fileHistoryDialog.modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });

            $("#addFileDialog").modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });

            $("#editDescriptionDialog").modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });

            filesModel = new FilesModel(<%= Model.Id %>, '<%= Model.Name %>');
            ko.applyBindings(filesModel, $('#mainBody')[0]);
        });

        function renderHistory(value, row) {
            return '<a href="/IGManagement/Files/Download?versionId=' + row.VersionId + '">' + value + '</a>';
        }
    </script>
</asp:Content>