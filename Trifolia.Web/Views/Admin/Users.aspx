<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.RoleManagement.RolesModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #Roles .row {
            padding-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="Users">
        <h2>Users</h2>

        <div class="form-group">
            <input type="text" class="form-control" placeholder="Search..." data-bind="value: SearchQuery, valueUpdate: 'keypress'" />
        </div>

        <table class="table table-striped">
            <thead>
                <th>First Name</th>
                <th>Last Name</th>
                <th>Email</th>
                <th>&nbsp;</th>
            </thead>
            <tbody data-bind="foreach: GetFilteredUsers">
                <tr>
                    <td data-bind="text: FirstName"></td>
                    <td data-bind="text: LastName"></td>
                    <td data-bind="text: Email"></td>
                    <td>
                        <div class="btn-group">
                            <button type="button" class="btn btn-default" data-bind="click: function () { $parent.EditUser($data); }">Edit</button>
                            <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditUserGroups($data); }">Groups</button>
                            <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditUserRoles($data); }">Roles</button>
                            <button type="button" class="btn btn-default" data-bind="click: function () { $parent.RemoveUser($data); }">Remove</button>
                        </div>
                    </td>
                </tr>
            </tbody>
            <tfoot>
                <th colspan="4">
                    <span>Total: </span>
                    <span data-bind="text: Users().length"></span>
                </th>
            </tfoot>
        </table>

        <div class="modal fade" id="EditUserModal" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title" id="H2">User</h4>
                    </div>
                    <div class="modal-body">
                        ...
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>

        <div class="modal fade" id="EditUserGroupsModal" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title" id="H1">User Groups</h4>
                    </div>
                    <div class="modal-body">
                        <div class="row" data-bind="with: CurrentUser">
                            <div class="col-md-6">
                                <table class="table table-striped">
                                    <caption>Unassigned</caption>
                                    <tbody data-bind="foreach: $parent.GetUnassignedGroups()">
                                        <tr>
                                            <td data-bind="text: Name">test</td>
                                            <td>
                                                <div class="pull-right">
                                                    <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[$parents.length-1].AssignGroup($parent, Id); }">Assign</button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <table class="table table-striped">
                                    <caption>Assigned</caption>
                                    <tbody data-bind="foreach: Groups">
                                        <tr>
                                            <td>
                                                <button type="button" class="btn btn-default btn-sm" data-bind="click: function() { $parents[$parents.length-1].UnassignGroup($parent, $data); }">Unassign</button>
                                            </td>
                                            <td>
                                                <div class="pull-right" data-bind="text: $parents[$parents.length - 1].GetGroupName($data)">test</div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>

        <div class="modal fade" id="EditUserRolesModal" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title" id="myModalLabel">User Roles</h4>
                    </div>
                    <div class="modal-body">
                        <div class="row" data-bind="with: CurrentUser">
                            <div class="col-md-6">
                                <table class="table table-striped">
                                    <caption>Unassigned</caption>
                                    <tbody data-bind="foreach: $parent.GetUnassignedRoles()">
                                        <tr>
                                            <td data-bind="text: Name"></td>
                                            <td>
                                                <div class="pull-right">
                                                    <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[$parents.length - 1].AssignRole($parent, Id); }">Assign</button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <table class="table table-striped">
                                    <caption>Assigned</caption>
                                    <tbody data-bind="foreach: Roles">
                                        <tr>
                                            <td>
                                                <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[$parents.length - 1].UnassignRole($parent, $data); }">Unassign</button>
                                            </td>
                                            <td>
                                                <div class="pull-right" data-bind="text: $parents[$parents.length - 1].GetRoleName($data)"></div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/Admin/Users.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/lodash.min.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new UsersViewModel();
            ko.applyBindings(viewModel, $('#Users')[0]);

            $('#EditUserModal').modal({ show: false }).on('hidden.bs.modal', viewModel.ClearCurrentUser);
            $('#EditUserGroupsModal').modal({ show: false }).on('hidden.bs.modal', viewModel.ClearCurrentUser);
            $('#EditUserRolesModal').modal({ show: false }).on('hidden.bs.modal', viewModel.ClearCurrentUser);
        });
    </script>

</asp:Content>
