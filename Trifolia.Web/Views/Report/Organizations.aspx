<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Trifolia Organizations</h2>

    <div id="MainDiv">
        <!-- Nav tabs -->
        <ul class="nav nav-tabs" data-bind="foreach: Organizations" id="orgTabs">
            <li><a data-toggle="tab" data-bind="attr: { href: '#org' + Id() }, text: Name"></a></li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
        <!-- ko foreach: Organizations -->
            <div class="tab-pane" data-bind="attr: { id: 'org' + Id() }">
                <div style="padding: 5px">
                    <p>Total Editing Users: <span data-bind="text: TotalEditingUsers"></span></p>
                    <p>Total Non-Editing Users: <span data-bind="text: TotalNonEditingUsers"></span></p>
                    <p>Total Authored Templates/Profiles: <span data-bind="text: TotalAuthoredTemplates"></span></p>
                    <p>Total Users: <span data-bind="text: TotalUsers"></span></p>

                    <div class="panel panel-default">
                        <div class="panel-heading">External Organizations</div>
                        <table class="table">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th>Type</th>
                                    <th>Can Edit?</th>
                                    <th>Can Contact?</th>
                                    <th>Contact Name</th>
                                    <th>Contact Email</th>
                                    <th>Contact Phone</th>
                                </tr>
                            </thead>
                            <tbody data-bind="foreach: ExternalOrganizations">
                                <tr>
                                    <td data-bind="text: Name"></td>
                                    <td data-bind="text: Type"></td>
                                    <td data-bind="text: CanUserEdit() ? 'Yes' : 'No'"></td>
                                    <td data-bind="text: CanContact() ? 'Yes' : 'No'"></td>
                                    <td data-bind="text: ContactName"></td>
                                    <td><a data-bind="text: ContactEmail, attr: { href: 'mailto:' + ContactEmail() }"></a></td>
                                    <td data-bind="text: ContactPhone"></td>
                                </tr>
                            </tbody>
                            <tfoot>
                                <!-- ko if: ExternalOrganizations().length == 0 -->
                                <tr>
                                    <th colspan="7">No external organization information available.</th>
                                </tr>
                                <!-- /ko -->
                            </tfoot>
                        </table>
                    </div>

                    <div class="panel panel-default">
                        <div class="panel-heading">Users</div>
                        <div class="panel-body">
                            Okay to Contact: <select data-bind="value: $parent.FilterOkayToContact">
                                <option value="">Don't filter</option>
                                <option value="true">Only "Yes"</option>
                                <option value="false">Only "No"</option>
                            </select>
                        </div>
                        <table class="table">
                            <thead>
                                <tr>
                                    <th>First Name</th>
                                    <th>Last Name</th>
                                    <th>Email</th>
                                    <th>Phone</th>
                                    <th>Okay to Contact?</th>
                                    <th>Ext. Org. Name</th>
                                    <th>Ext. Org. Type</th>
                                </tr>
                            </thead>
                            <tbody data-bind="foreach: $parent.FilterUsers(Users())">
                                <tr>
                                    <td data-bind="text: FirstName"></td>
                                    <td data-bind="text: LastName"></td>
                                    <td>
                                        <a data-bind="attr: { href: 'mailto:' + Email() }, text: Email"></a>
                                    </td>
                                    <td data-bind="text: Phone"></td>
                                    <td>
                                        <span data-bind="if: OkayToContact()">Yes</span>
                                        <span data-bind="if: !OkayToContact()">No</span>
                                    </td>
                                    <td data-bind="text: ExternalOrganizationName"></td>
                                    <td data-bind="text: ExternalOrganizationType"></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        <!-- /ko -->
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/Report/Organizations.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var viewModel = new OrganizationsReportModel();
            ko.applyBindings(viewModel, $("#MainDiv")[0]);
        });
    </script>

</asp:Content>
