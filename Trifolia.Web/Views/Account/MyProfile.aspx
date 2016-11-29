<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        pre {
            max-height: 200px;
            overflow-y: auto;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript" src="/Scripts/Account/MyProfile.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

    <h2>My Profile</h2>

    <div id="mainBody">
        <div class="row">
            <div class="col-md-6">
                <p style="font-style: italic">This information, including name, email address, and phone number, is collected from users that voluntarily enter the information in the process of authoring templates.  It is stored in a manner appropriate to the nature of the data and is used only for purposes related to the authoring and maintenance of the templates entered by the user. The information collected is never provided to any other company for that company's independent use.</p>

                <div class="form-group">
                    <label>User Name:</label>
                    <input type="text" class="form-control" data-bind="value: model.userName" readonly="readonly" />
                </div>

                <div class="form-group">
                    <label>First Name</label>
                    <input type="text" class="form-control" data-bind="value: model.firstName" />
                </div>

                <div class="form-group">
                    <label>Last Name</label>
                    <input type="text" class="form-control" data-bind="value: model.lastName" />
                </div>

                <div class="form-group">
                    <label>Phone</label>
                    <input type="text" class="form-control" data-bind="value: model.phone" />
                </div>

                <div class="form-group">
                    <label>Email</label>
                    <input type="text" class="form-control" data-bind="value: model.email" />
                </div>

                <div class="form-group">
                    <label>Organization</label>
                    <input type="text" class="form-control" data-bind="value: model.organization" />
                </div>

                <div class="form-group">
                    <label>Organization Type</label>
                    <select class="form-control" data-bind="value: model.organizationType, options: orgTypes, optionsCaption: 'Select'"></select>
                </div>

                <!-- ko if: enableReleaseAnnouncement() -->
                <div class="form-group">    
                    <label>Release Announcements</label>
                    <p>
                        <!-- ko if: releaseAnnouncementsSubscription() == true -->
                        <button type="button" class="btn btn-default" data-bind="click: toggleReleaseAnnouncementSubscription">Unsubscribe</button>
                        <!-- /ko -->
                        <!-- ko if: releaseAnnouncementsSubscription() != true -->
                        <button type="button" class="btn btn-default" data-bind="click: toggleReleaseAnnouncementSubscription">Subscribe</button>
                        <!-- /ko -->
                    </p>
                </div>
                <!-- /ko -->

                <p>
                    <button type="button" class="btn btn-primary" data-bind="click: saveChanges, enable: model.validation.isValid">Save</button>
                </p>
            </div>
        
            <!-- ko if: model.openIdConfigUrl() -->
            <div class="col-md-6">
                <div class="panel panel-default">
                    <div class="panel-heading">OpenID Configuration</div>
                    <div class="panel-body">
                        <p><strong>Url:</strong> <span data-bind="text: model.openIdConfigUrl"></span></p>

                        <!-- ko if: openIdConfig() -->
                        <p>
                            <pre data-bind="text: openIdConfig"></pre>
                        </p>
                        <!-- /ko -->
                    </div>
                </div>
            </div>
            <!-- /ko -->

            <!-- ko if: model.authToken() -->
            <div class="col-md-6">
                <div class="panel panel-default">
                    <div class="panel-heading">Current User OAuth Token</div>
                    <div class="panel-body">
                        <pre data-bind="text: model.authToken"></pre>
                    </div>
                </div>
            </div>
            <!-- /ko -->
        </div>
    </div>

    <script type="text/javascript">
        var vm = new myProfileViewModel(
            '<%: Url.Action("ProfileData") %>',
            '<%: Url.Action("SaveProfile") %>'
        );

        $(document).ready(function () {
            var mainBody = document.getElementById('mainBody');
            ko.applyBindings(vm, mainBody);
        });
    </script>

</asp:Content>