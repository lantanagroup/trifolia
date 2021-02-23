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
    <h2>My Profile</h2>

    <div class="ng-cloak" ng-app="Trifolia" ng-controller="MyProfileCtrl" ng-init="init()">
        <form name="userProfileForm">
            <p class="alert" ng-class="{'alert-info': !messageIsWarning, 'alert-warning': messageIsWarning }" ng-if="message">{{message}}</p>
            <p style="font-style: italic">This information, including name, email address, and phone number, is collected from users that voluntarily enter the information in the process of authoring templates.  It is stored in a manner appropriate to the nature of the data and is used only for purposes related to the authoring and maintenance of the templates entered by the user. The information collected is never provided to any other company for that company's independent use.</p>

            <div class="row">
                <div class="col-md-6">
                    <div class="panel panel-default">
                        <div class="panel-heading">General Information</div>
                        <div class="panel-body">
                            <div class="form-group">
                                <label>Username:</label>
                                <input type="text" class="form-control" ng-model="userModel.UserName" readonly="readonly" />
                            </div>

                            <div class="form-group has-feedback" ng-class="{'has-error': !userModel.FirstName}">
                                <label>First Name</label>
                                <input type="text" class="form-control" ng-model="userModel.FirstName" ng-required="true" />
                            </div>

                            <div class="form-group has-feedback" ng-class="{'has-error': !userModel.LastName}">
                                <label>Last Name</label>
                                <input type="text" class="form-control" ng-model="userModel.LastName" ng-required="true" />
                            </div>

                            <div class="form-group has-feedback" ng-class="{'has-error': !userModel.Email}">
                                <label>Email</label>
                                <input type="email" class="form-control" ng-model="userModel.Email" ng-required="true" />
                            </div>

                            <div class="form-group has-feedback" ng-class="{'has-error': !userModel.Phone}">
                                <label>Phone</label>
                                <input type="text" class="form-control" ng-model="userModel.Phone" ng-required="true" />
                            </div>

                            <div class="form-group">
                                <label>Organization</label>
                                <input type="text" class="form-control" ng-model="userModel.Organization" />
                            </div>

                            <div class="form-group">
                                <label>Organization Type</label>
                                <select class="form-control" ng-model="userModel.OrganizationType" ng-options="ot for ot in orgTypes">
                                    <option value="">SELECT</option>
                                </select>
                            </div>
                        </div>
                    </div>
                    
                    <div class="panel panel-default">
                        <div class="panel-heading">OAuth Information</div>
                        <div class="panel-body">
                            <p><strong>Auth Token</strong></p>
                            <p><pre>{{userModel.AuthToken}}</pre></p>

                            <div ng-if="userModel.OpenIdConfigUrl">
                                <p><strong>OpenId Config Url</strong> <a href="{{userModel.OpenIdConfigUrl}}" target="_new">{{userModel.OpenIdConfigUrl}}</a></p>
                                <p ng-if="openIdConfig"><pre>{{openIdConfig | json}}</pre></p>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="panel panel-default">
                        <div class="panel-heading">UMLS/VSAC Licensing Credentials</div>
                        <div class="panel-body">
                            <p>To import value sets from VSAC and view/export implementation guides that include VSAC content, please specify your UMLS/VSAC API KEY so that we can verify you have an active UMLS license. This API KEY is encrypted and stored in the Trifolia database to limit the number of times you have to authenticate against UMLS/VSAC. These credentials are <strong>not</strong> used for any purpose other than to validate your UMLS/VSAC license.</p>
                            <p>To find your API KEY, login to <a href="https://uts.nlm.nih.gov/uts/" target="_new">UMLS Terminology Services</a>, select "Visit Your Profile" under "UTF Profile" (on the right), and copy/paste the value from the "API KEY" field.</p>
                        
                            <div class="form-group">
                                <label>API KEY</label>
                                <input type="password" class="form-control" ng-model="userModel.UMLSApiKey" ng-change="umlsCredentialsChanged()" />
                            </div>

                            <p>
                                <button type="button" class="btn btn-default" ng-click="validateUMLSApiKey()" ng-disabled="!userModel.UMLSApiKey || userModel.UMLSApiKey == '******'">Test</button>
                                <span>This test will confirm if the credentials you entered are valid. You should test your credentials prior to saving your profile changes.</span>
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="panel panel-default">
                        <div class="panel-heading">Release Announcements</div>
                        <div class="panel-body">
                            <p>When Trifolia (<a href="https://trifolia.lantanagroup.com">https://trifolia.lantanagroup.com</a>) has released a new version, a release announcement is sent to everyone that is subscribed.</p>
                            
                            <p><strong>Currently {{subscribed ? 'subscribed to' : 'unsubscribed from'}} announcements</strong></p>
                            <p><button type="button" class="btn btn-default" ng-click="toggleReleaseAnnouncementSubscription()">{{subscribed ? 'Unsubscribe' : 'Subscribe'}}</button></p>
                        </div>
                    </div>
                </div>
            </div>

            <p class="alert alert-info" ng-if="!umlsCredentialsConfirmed && userModel.UMLSApiKey">Confirm/test your UMLS API Key before saving.</p>
            <p><button type="submit" class="btn btn-primary" ng-click="save()" ng-disabled="!umlsCredentialsConfirmed || !userProfileForm.$valid">Save</button></p>
        </form>
    </div>
    
    <script type="text/javascript" src="/Scripts/Account/controllers.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>