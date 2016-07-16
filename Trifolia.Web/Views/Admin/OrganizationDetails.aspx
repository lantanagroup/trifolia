<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int?>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #OrganizationEdit .row {
            padding-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="OrganizationEdit">
        <div data-bind="with: Model">
            <h2><span data-bind="text: Name"></span> Members and Groups</h2>
        </div>

        <p class="alert alert-info">Changes made to this screen are persisted immediately.</p>

        <div data-bind="with: Model">
            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#users" data-toggle="tab">Users</a></li>
                <li><a href="#groups" data-toggle="tab">Groups</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="users">
                    <div class="row">
                        <div class="col-md-3">
                            <strong>Full Name</strong>
                        </div>
                        <div class="col-md-3">
                            <strong>UserName</strong>
                        </div>
                        <div class="col-md-3">
                            <strong>Email</strong>
                        </div>
                        <div class="col-md-3">
                            <div class="pull-right">
                                <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditUser(); }">Add</button>
                            </div>
                        </div>
                    </div>
                    
                    <!-- ko foreach: $parent.GetUsers() -->
                    <div class="row">
                        <div class="col-md-3" data-bind="text: FullName"></div>
                        <div class="col-md-3" data-bind="text: UserName"></div>
                        <div class="col-md-3">
                            <a data-bind="attr: { href: 'mailto:' + Email() }"><span data-bind="text: Email"></span></a>
                        </div>
                        <div class="col-md-3">
                            <div class="pull-right">
                                <div class="btn-group">
                                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditUser($data); }">Edit</button>
                                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditUserGroups($data); }">Groups</button>
                                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditUserRoles($data); }">Roles</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].DeleteUser($data); }">Delete</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- /ko -->
                </div>
                <div class="tab-pane" id="groups">
                    <div class="row">
                        <div class="col-md-9">
                            <strong>Name</strong>
                        </div>
                        <div class="col-md-3">
                            <div class="pull-right">
                                <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditGroup(); }">Add</button>
                            </div>
                        </div>
                    </div>
                    
                    <!-- ko foreach: $parent.GetGroups() -->
                    <div class="row">
                        <div class="col-md-9" data-bind="text: Name"></div>
                        <div class="col-md-3">
                            <div class="pull-right">
                                <div class="btn-group">
                                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditGroup($data); }">Edit</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].DeleteGroup($data); }">Delete</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- /ko -->
                </div>
            </div>
        </div>

        <div class="modal fade" id="editUserRolesDialog">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentUser">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit User <span data-bind="text: FirstName"></span> <span data-bind="text: LastName"></span> Roles</h4>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <strong>Available Roles</strong>
                            </div>
                            <div class="col-md-6">
                                <strong>Assigned Roles</strong>
                            </div>
                        </div>

                        <div class="row">
                            <!-- Available Roles -->
                            <div class="col-md-6">
                                <!-- ko foreach: $parent.GetUserAvailableRoles() -->
                                <div class="input-group">
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.AssignRole(Id()); }">
                                            <i class="glyphicon glyphicon-chevron-right"></i>
                                        </button>
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>
                            <!-- Assigned Roles -->
                            <div class="col-md-6">
                                <!-- ko foreach: Roles -->
                                <div class="input-group">
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.UnassignRole($data); }">
                                            <i class="glyphicon glyphicon-chevron-left"></i>
                                        </button>
                                    </div>
                                    <input type="text" class="form-control" readonly data-bind="value: $parents[1].GetRoleName($data)" />
                                </div>
                                <!-- /ko -->
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditUserRoles">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="editUserGroupsDialog">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentUser">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit User <span data-bind="text: FirstName"></span> <span data-bind="text: LastName"></span> Groups</h4>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <strong>Available Groups</strong>
                            </div>
                            <div class="col-md-6">
                                <strong>Assigned Groups</strong>
                            </div>
                        </div>

                        <div class="row">
                            <!-- Available Groups -->
                            <div class="col-md-6">
                                <!-- ko foreach: $parent.GetUserAvailableGroups() -->
                                <div class="input-group">
                                    <input type="text" class="form-control" readonly data-bind="value: Name" />
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.AssignGroup(Id()); }">
                                            <i class="glyphicon glyphicon-chevron-right"></i>
                                        </button>
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>
                            <!-- Assigned Groups -->
                            <div class="col-md-6">
                                <!-- ko foreach: Groups -->
                                <div class="input-group">
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-default" data-bind="click: function () { viewModel.UnassignGroup($data); }">
                                            <i class="glyphicon glyphicon-chevron-left"></i>
                                        </button>
                                    </div>
                                    <input type="text" class="form-control" readonly data-bind="value: $parents[1].GetGroupName($data)" />
                                </div>
                                <!-- /ko -->
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditUserGroups">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="editUserDialog">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentUser">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit User</h4>
                    </div>
                    <div class="modal-body" data-bind="validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <label>User Name</label>
                            <input type="text" class="form-control" data-bind="value: UserName" autofocus="autofocus" />
                        </div>
                        
                        <div class="form-group">
                            <label>First Name</label>
                            <input type="text" class="form-control" data-bind="value: FirstName" />
                        </div>

                        <div class="form-group">
                            <label>Last Name</label>
                            <input type="text" class="form-control" data-bind="value: LastName" />
                        </div>
                        
                        <div class="form-group">
                            <label>Email</label>
                            <input type="text" class="form-control" data-bind="value: Email" />
                        </div>

                        <div class="form-group">
                            <label>Phone</label>
                            <input type="text" class="form-control" data-bind="value: Phone" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveUser, enable: IsValid">Save</button>
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditUser">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="editGroupDialog">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentGroup">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Group</h4>
                    </div>
                    <div class="modal-body" data-bind="validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <label>Name</label>
                            <input type="text" class="form-control" data-bind="value: Name" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveGroup, enable: IsValid">Save</button>
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditGroup">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript" src="/Scripts/Organization/Details.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new OrganizationEditModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#OrganizationEdit')[0]);

            $('#editUserDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            }).on('shown.bs.modal', function () {
                $(this).find('[autofocus]').focus();
            });
            $('#editGroupDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
            $('#editUserGroupsDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
            $('#editUserRolesDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
        });
    </script>
</asp:Content>
