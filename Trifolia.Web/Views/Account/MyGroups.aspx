<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript" src="/Scripts/Account/MyGroups.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    
    <div id="MyGroups">
        <h2>My Groups</h2>
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>
                            <div class="pull-right">
                                <a class="btn btn-default" data-bind="attr: { href: '/Account/Group/' }">Add</a>
                            </div>
                        </th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: Groups">
                    <tr>
                        <td data-bind="text: Name"></td>
                        <td>
                            <div class="pull-right">
                                <div class="btn-group">
                                    <!-- ko if: IsManager() -->
                                    <a class="btn btn-default" data-bind="attr: { href: '/Account/Group/' + Id() }">Edit</a>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $root.RemoveGroup($data); }">Remove</button>
                                    <!-- /ko -->
                                    <!-- ko if: !IsManager() -->
                                    <button type="button" class="btn btn-default" data-bind="disable: ViewDisabled, click: function () { $root.ViewGroup($data); }">View</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function () { $root.LeaveGroup(Id()); }">Leave</button>
                                    <!-- /ko -->
                                </div>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        
        <h3>Available Groups</h3>
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: AllGroups">
                    <tr>
                        <td data-bind="text: Name"></td>
                        <td>
                            <div class="pull-right">
                                <a class="btn btn-default" data-bind="click: function () { $root.JoinGroup($data); }">Join</a>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="modal fade" tabindex="-1" role="dialog" id="GroupDisclaimerDialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Group Disclaimer</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentGroup()">
                        <p data-bind="html: Disclaimer"></p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: function () { DoJoinGroup(CurrentGroup().Id()); }">Join</button>
                        <button type="button" class="btn btn-default" data-bind="click: CancelJoinGroup">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" tabindex="-1" role="dialog" id="ViewGroupDialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">View Group</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentGroup()">
                        <!-- ko if: Description() -->
                        <div class="panel panel-default">
                            <div class="panel-heading">Description</div>
                            <div class="panel-body" data-bind="text: Description"></div>
                        </div>
                        <!-- /ko -->
                        <!-- ko if: Disclaimer() -->
                        <div class="panel panel-default">
                            <div class="panel-heading">Disclaimer</div>
                            <div class="panel-body" data-bind="html: Disclaimer"></div>
                        </div>
                        <!-- /ko -->
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-dismiss="modal">Close</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/javascript">
        var vm = new MyGroupsViewModel();

        $(document).ready(function () {
            var mainBody = document.getElementById('MyGroups');
            ko.applyBindings(vm, mainBody);
        });

        $('#GroupDisclaimerDialog').modal({
            show: false,
            backdrop: 'static'
        });

        $('#ViewGroupDialog').modal({
            show: false,
            backdrop: 'static'
        });
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
