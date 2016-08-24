<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #Roles .row {
            padding-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="Group">
        <h2>Edit Group</h2>
        
        <div class="col-md-4" data-bind="with: Group, validationOptions: { messagesOnModified: false }">
            <div class="form-group">
                <label>Name</label>
                <input type="text" class="form-control" data-bind="value: Name" />
            </div>

            <div class="form-group">
                <label>Description</label>
                <textarea class="form-control description" data-bind="value: Description"></textarea>
            </div>

            <div class="form-group">
                <label>Join Method</label>
                <br />
                <input type="checkbox" data-bind="checked: IsOpen" /> Anyone can join
                <!-- ko if: !IsOpen() -->
                <span class="help-block">An email will be sent to managers for approval</span>
                <!-- /ko -->
                <!-- ko if: IsOpen() -->
                <span class="help-block">When a user requests to join a group, they will be automatically added</span>
                <!-- /ko -->
            </div>

            <div class="form-group">
                <label>Disclaimer</label>
                <textarea class="form-control disclaimer" data-bind="value: Disclaimer" placeholder="HTML"></textarea>
            </div>
        </div>
        <div class="col-md-4">
            <h3>Managers <button type="button" class="btn btn-primary btn-sm" data-bind="click: AddManager">Add</button></h3>

            <ul data-bind="foreach: Managers">
                <li>
                    <span data-bind="text: Name"></span> [<a href="#" data-bind="click: function () { $root.RemoveUser(Id, true); }">remove</a>]
                </li>
            </ul>
        </div>
        <div class="col-md-4">
            <h3>Members <button type="button" class="btn btn-primary btn-sm" data-bind="click: AddMember">Add</button></h3>

            <ul data-bind="foreach: Members">
                <li><span data-bind="text: Name"></span> [<a href="#" data-bind="click: function () { $root.RemoveUser(Id, false); }">remove</a>]</li>
            </ul>
        </div>

        <div class="modal fade" tabindex="-1" role="dialog" id="AddUserDialog">
            <div class="modal-dialog modal-sm" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Add User</h4>
                    </div>
                    <div class="modal-body">
                        <div class="input-group">
                            <input type="text" class="form-control" data-bind="value: SearchQuery" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default" data-bind="click: SearchUsers, disabled: !SearchQuery()">Search</button>
                            </div>
                        </div>

                        <div data-bind="foreach: SearchResults">
                            <p>
                                <input type="checkbox" data-bind="value: Id, checked: $root.SelectedUserIds" />
                                <span data-bind="text: Name"></span>
                            </p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: AddUserOk">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: AddUserCancel">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript" src="/Scripts/Admin/Group.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new GroupViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#Group')[0]);
        });

        $('#AddUserDialog').modal({
            show: false,
            backdrop: 'static'
        });
    </script>

</asp:Content>