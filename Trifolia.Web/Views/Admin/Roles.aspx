<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.RoleManagement.RolesModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #Roles .row {
            padding-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="Roles">
        <h2>Roles</h2>

        <p class="alert alert-info">Note: Changes made on this page are saved/persisted immediately.</p>

        <div class="form-group" data-bind="with: Model">
            <label>Default Role</label>
            <select class="form-control" data-bind="value: DefaultRoleId, options: Roles, optionsText: 'Name', optionsValue: 'Id'"></select>
        </div>

        <div class="container-fluid" data-bind="with: Model">
            <div class="row">
                <div class="col-md-10"><strong>Role Name</strong></div>
                <div class="col-md-2">
                    <div class="pull-right">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.ShowAddRole">Add</button>
                    </div>
                </div>
            </div>
            
            <!-- ko foreach: Roles() -->
            <div class="row">
                <div class="col-md-8" data-bind="text: Name"></div>
                <div class="col-md-4">
                    <div class="pull-right">
                        <div class="btn-group">
                            <button type="button" class="btn btn-primary" data-bind="click: function () { viewModel.EditRoleSecurables($data); }">Securables</button>
                            <button type="button" class="btn btn-primary" data-bind="click: function () { viewModel.EditRoleRestrictions($data); }">Restrictions</button>
                            <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.RemoveRole($data); }">Remove</button>
                        </div>
                    </div>
                </div>
            </div>
            <!-- /ko -->
        </div>

        <div class="modal fade" id="editRoleSecurablesDialog">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header" data-bind="with: CurrentRole">
                        <h4 class="modal-title">Edit <span data-bind="text: Name"></span></h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentRole">
                        <div class="row">
                            <div class="col-md-5"><strong>Available Securables</strong></div>
                            <div class="col-md-2"></div>
                            <div class="col-md-5"><strong>Assigned Securables</strong></div>
                        </div>
                        <div class="row">
                            <div class="col-md-6" style="max-height: 350px; overflow-y: auto;">
                                <!-- ko foreach: viewModel.GetAvailableSecurables() -->
                                <div class="input-group" data-bind="tooltip: Description">
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.AssignSecurable($data); }">
                                            <i class="glyphicon glyphicon-chevron-right"></i>
                                        </button>
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>
                            <div class="col-md-6" style="max-height: 350px; overflow-y: auto;">
                                <!-- ko foreach: viewModel.GetAssignedSecurables() -->
                                <div class="input-group" data-bind="tooltip: Description">
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.UnassignSecurable($data); }">
                                            <i class="glyphicon glyphicon-chevron-left"></i>
                                        </button>
                                    </div>
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                </div>
                                <!-- /ko -->
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: CloseEditRoleSecurables">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
        
        <div class="modal fade" id="editRoleRestrictionsDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" data-bind="with: CurrentRole">
                        <h4 class="modal-title">Edit <span data-bind="text: Name"></span> Restrictions</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentRole">
                        <div class="row">
                            <div class="col-md-5"><strong>Unrestricted Organizations</strong></div>
                            <div class="col-md-2"></div>
                            <div class="col-md-5"><strong>Restricted Organizations</strong></div>
                        </div>
                        <div class="row">
                            <div class="col-md-6" style="max-height: 350px; overflow-y: auto;">
                                <!-- ko foreach: viewModel.GetUnrestrictedOrganizations() -->
                                <div class="input-group">
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.RestrictOrganization($data); }">
                                            <i class="glyphicon glyphicon-chevron-right"></i>
                                        </button>
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>
                            <div class="col-md-6" style="max-height: 350px; overflow-y: auto;">
                                <!-- ko foreach: viewModel.GetRestrictedOrganizations() -->
                                <div class="input-group">
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.UnrestrictOrganization($data); }">
                                            <i class="glyphicon glyphicon-chevron-left"></i>
                                        </button>
                                    </div>
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                </div>
                                <!-- /ko -->
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: CloseEditRoleRestrictions">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="addRoleDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Add Role</h4>
                    </div>
                    <div class="modal-body" data-bind="validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <label>Role Name</label>
                            <input type="text" class="form-control" data-bind="value: AddRoleName" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: AddRole, enable: AddRoleValidation().isValid()">OK</button>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript" src="/Scripts/Admin/Roles.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new RolesViewModel();
            ko.applyBindings(viewModel, $('#Roles')[0]);

            $('#editRoleSecurablesDialog').modal({
                keyboard: false,
                backdrop: 'static',
                show: false
            });

            $('#addRoleDialog').modal({
                keyboard: false,
                backdrop: 'static',
                show: false
            });
        });
    </script>

</asp:Content>