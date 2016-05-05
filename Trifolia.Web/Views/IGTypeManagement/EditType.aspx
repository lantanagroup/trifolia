<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int?>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="EditType" data-bind="validationOptions: { messagesOnModified: false }">
        <h2>Edit Implementation Guide Type</h2>

        <!-- ko if: Model().Id() -->
        <div class="alert alert-info alert-dismissable">
            <button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>
            Incorrectly changing the information on the implementation guide type that is used by existing templates may cause the system to become unstable for those templates.
        </div>
        <!-- /ko -->

        <!-- ko with: Model -->

        <div class="form-group">
            <label>Name</label>
            <input type="text" class="form-control" data-bind="value: Name" />
        </div>

        <!-- Nav tabs -->
        <ul class="nav nav-tabs">
            <li class="active"><a href="#schema" data-toggle="tab">Schema</a></li>
            <li><a href="#templateTypes" data-toggle="tab">Template/Profile Types</a></li>
            <li><a href="#dataTypes" data-toggle="tab">Data Types</a></li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
            <!-- Schema -->
            <div class="tab-pane active" id="schema">
                <div class="form-group">
                    <label>Schema</label>
                    <input type="file" class="form-control" data-bind="file: SchemaFileInfo, fileBinaryData: SchemaFile" />
                </div>

                <div class="form-group">
                    <label>Schema Location</label>
                    <select class="form-control" data-bind="value: SchemaLocation, options: $parent.SchemaLocations, optionsCaption: 'Select...'"></select>
                </div>

                <div class="form-group">
                    <label>Schema Prefix</label>
                    <input type="text" class="form-control" data-bind="value: SchemaPrefix" />
                </div>

                <div class="form-group">
                    <label>Schema URI</label>
                    <input type="text" class="form-control" data-bind="value: SchemaUri" />
                </div>
            </div>

            <!-- Template Types -->
            <div class="tab-pane" id="templateTypes">
                <table class="table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Order</th>
                            <th>Context</th>
                            <th>Context Type</th>
                            <th>
                                <div class="pull-right">
                                    <button type="button" class="btn btn-primary" data-bind="click: $parent.AddTemplateType">Add</button>
                                </div>
                            </th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: TemplateTypes">
                        <tr>
                            <td data-bind="text: Name"></td>
                            <td data-bind="text: OutputOrder"></td>
                            <td data-bind="text: Context"></td>
                            <td data-bind="text: ContextType"></td>
                            <td>
                                <div class="btn-group pull-right">
                                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditTemplateType($data); }">Edit</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].MoveTemplateTypeUp($data); }, disable: (OutputOrder() == 1)">Move Up</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].MoveTemplateTypeDown($data); }, disable: (OutputOrder() == $parent.TemplateTypes().length)">Move Down</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].DeleteTemplateType($data); }">Delete</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Data Types -->
            <div class="tab-pane" id="dataTypes">
                <div class="row">
                    <!-- Available -->
                    <div class="col-md-5">
                        <select multiple class="form-control" data-bind="value: $parent.SelectedComplexType, options: ComplexTypes" style="height: 150px;">
                        </select>
                    </div>

                    <div class="col-md-2" style="text-align: center">
                        <p>
                            <button type="button" class="btn btn-primary" data-bind="click: $parent.AssignComplexType, enable: $parent.SelectedComplexType()"><i class="glyphicon glyphicon-chevron-right"></i></button>
                        </p>
                        <p>
                            <button type="button" class="btn btn-primary" data-bind="click: $parent.RemoveDataType, enable: $parent.SelectedDataType()"><i class="glyphicon glyphicon-chevron-left"></i></button>
                        </p>
                    </div>

                    <!-- Assigned -->
                    <div class="col-md-5">
                        <select multiple class="form-control" data-bind="value: $parent.SelectedDataType, options: DataTypes" style="height: 150px;">
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <!-- /ko -->

        <div class="btn-group">
            <button type="button" class="btn btn-primary" data-bind="click: Save, enable: (Model() && Model().IsValid())">Save</button>
            <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
        </div>

        <!-- Edit Template Type Modal -->
        <div class="modal fade" id="editTemplateTypeModal">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentTemplateType, validationOptions: { messagesOnModified: false }">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Template/Profile Type</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Name</label>
                            <input type="text" class="form-control" data-bind="value: Name" />
                        </div>
                        
                        <div class="form-group">
                            <label>Context</label>
                            <input type="text" class="form-control" data-bind="value: Context" />
                        </div>
                        
                        <div class="form-group">
                            <label>Context Type</label>
                            <input type="text" class="form-control" data-bind="value: ContextType" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveTemplateType, enable: IsValid">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditTemplateType">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript" src="/Scripts/Type/EditType.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new EditTypeViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#EditType')[0]);

            $('#editTemplateTypeModal').modal({
                show: false,
                backdrop: 'static',
                keyboard: false
            });
        });
    </script>

</asp:Content>