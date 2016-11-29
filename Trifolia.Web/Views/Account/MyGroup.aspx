<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int?>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        textarea.disclaimer {
            height: 200px;
        }

        textarea.description {
            height: 100px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript" src="/Scripts/Account/MyGroup.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

    <h2>Edit Group</h2>

    <div id="MyGroup">
        <div class="col-md-4" data-bind="validationOptions: { messagesOnModified: false }">
            <div class="form-group">
                <label>Name</label>
                <input type="text" class="form-control" data-bind="value: Group().Name" />
            </div>

            <div class="form-group">
                <label>Description</label>
                <textarea class="form-control description" data-bind="value: Group().Description"></textarea>
            </div>

            <div class="form-group">
                <label>Join Method</label>
                <br />
                <input type="checkbox" data-bind="checked: Group().IsOpen" /> Anyone can join
                <!-- ko if: !Group().IsOpen() -->
                <span class="help-block">An email will be sent to managers for approval</span>
                <!-- /ko -->
                <!-- ko if: Group().IsOpen() -->
                <span class="help-block">When a user requests to join a group, they will be automatically added</span>
                <!-- /ko -->
            </div>

            <div class="form-group">
                <label>Disclaimer</label>
                <textarea class="form-control disclaimer" data-bind="value: Group().Disclaimer" placeholder="HTML"></textarea>
            </div>

            <!-- ko if: !GroupId() -->
            <button type="button" class="btn btn-primary" data-bind="click: SaveChanges">Save</button>
            <!-- /ko -->
        </div>
        <div class="col-md-4" data-bind="if: GroupId()">
            <h3>Managers (<span data-bind="text: Managers().length"></span>)<button type="button" class="btn btn-primary btn-sm" data-bind="click: AddManager">Add</button></h3>

            <p class="alert alert-warning">Being a manager of a group does not imply you are a member of the group. Add yourself as a member if you intend to be included in implementation guide permissions when this group is assigned.</p>

            <ul data-bind="foreach: Managers">
                <li>
                    <span data-bind="text: Name"></span>
                    <!-- ko if: $root.CurrentUser() && Id != $root.CurrentUser().Id -->
                    [<a href="#" data-bind="click: function () { $root.RemoveUser(Id, true); }">remove</a>]
                    <!-- /ko -->
                </li>
            </ul>
        </div>
        <div class="col-md-4" data-bind="if: GroupId()">
            <h3>Members (<span data-bind="text: Members().length"></span>)<button type="button" class="btn btn-primary btn-sm" data-bind="click: AddMember">Add</button></h3>

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

    <script type="text/javascript">
        var vm = new MyGroupViewModel(<%= Model %>);

        $(document).ready(function () {
            var mainBody = document.getElementById('MyGroup');
            ko.applyBindings(vm, mainBody);
        });

        $('#AddUserDialog').modal({
            backdrop: 'static',
            show: false
        });
    </script>

</asp:Content>