<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript" src="/Scripts/knockout/knockout.isdirty.js"></script>
    <script type="text/javascript" src="/Scripts/Account/MyProfile.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

    <h2>My Profile</h2>

    <div id="mainBody">
        <p style="font-style: italic">This information, including name, email address, and phone number, is collected from users that voluntarily enter the information in the process of authoring templates.  It is stored in a manner appropriate to the nature of the data and is used only for purposes related to the authoring and maintenance of the templates entered by the user. The information collected is never provided to any other company for that company's independent use.</p>

        <div class="form-group">
            <label>User Name:</label>
            <input type="text" class="form-control" data-bind="value: model.userName" readonly="readonly" />
        </div>

        <div class="form-group">
            <label>Account Organization:</label>
            <input type="text" class="form-control" data-bind="value: model.accountOrganization" readonly="readonly" />
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

        <div class="form-group">
            <label>API Key</label>
            <div class="input-group">
                <input type="text" class="form-control" data-bind="value: model.apiKey" />
                <div class="input-group-btn">
                    <button type="button" class="btn btn-default" data-bind="click: generateApiKey">Generate</button>
                </div>
            </div>
        </div>

        <!-- ko if: model.apiKey() -->
        <div class="panel panel-default">
            <div class="panel-heading">Using the API Key</div>
            <div class="panel-body">
                <p>The API Key is used to authorize requests to the Trifolia REST API. Trifolia requires that REST calls be authenticated via forms authentication (when accessed via the web application) or via an Authentication Bearer header in direct REST calls.</p>
                <p>To generate the Authorization header for calls made directly to the Trifolia REST API, follow these steps:</p>
                <ol>
                    <li>
                        Create a string in the following format: "UserName|OrganizationName|Timestamp|Salt|ApiKey
                        <ul>
                            <li>UserName: The username associated with your account</li>
                            <li>OrganizationName: The name of the organization in Trifolia associated with your account (ex: "LCG" or "HL7")</li>
                            <li>Timestamp: Time, in milliseconds, from 1/1/1970</li>
                            <li>Salt: Any random number for salting the hash in the next step</li>
                            <li>ApiKey: <span data-bind="text: model.apiKey"></span></li>
                        </ul>
                    </li>
                    <li>Encrypt the string using a SHA1 hash</li>
                    <li>
                        Create a new string in the following format: "UserName|OrganizationName|Timestamp|Salt|Hash"
                        <ul>
                            <li>Hash: The hash generated from the previous step</li>
                        </ul>
                    </li>
                    <li>Base64 encode the string and send it in the Authorization header in the following format: "Bearer BASE64String"</li>
                </ol>

                <div>
                    <!-- Nav tabs -->
                    <ul class="nav nav-tabs" role="tablist">
                        <li role="presentation" class="active"><a href="#node" aria-controls="node" role="tab" data-toggle="tab">Node.JS</a></li>
                        <li role="presentation"><a href="#js" aria-controls="js" role="tab" data-toggle="tab">JavaScript</a></li>
                    </ul>

                    <!-- Tab panes -->
                    <div class="tab-content">
                        <div role="tabpanel" class="tab-pane active" id="node">
                            <pre>var crypto = require('crypto');
                                
function generateAuthorizationHeader(userName, accountOrganization, apiKey) {
    var timestamp = Date.now();
    var salt = Math.random();
    var properties = userName + '|' + accountOrganization + '|' + timestamp + '|' + salt + '|';
    var hashContent = properties + apiKey;
    var authBasicValue = properties + crypto.createHash('sha1').update(hashContent).digest('base64');
    var b64AuthBasicValue = new Buffer(authBasicValue).toString('base64');
    var authorizationHeader = 'Bearer ' + b64AuthBasicValue;
    return authorizationHeader;
}

var authorizationHeader = generateAuthorizationHeader('<span data-bind="text: model.userName"></span>', '<span data-bind="text: model.accountOrganization"></span>', '<span data-bind="text: model.apiKey"></span>');</pre>
                        </div>
                        <div role="tabpanel" class="tab-pane" id="js">
                            <pre>
        var timestamp = Date.now();
        var salt = Math.random();
        var properties = self.model.userName() + '|' + self.model.accountOrganization() + '|' + timestamp + '|' + salt + '|';
        var hashContent = properties + self.model.apiKey();
        var authBasicValue = properties + CryptoJS.SHA1(hashContent).toString(CryptoJS.enc.Base64);
        var b64AuthBasicValue = btoa(authBasicValue.toString(CryptoJS.enc.Base64));</pre>

                            <p>See <a href="https://code.google.com/p/crypto-js/" target="_new">https://code.google.com/p/crypto-js/</a> for CryptoJS scripts.</p>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label>Generate authorization header based on API key (this generated header is valid for 5 minutes)</label>
                    <div class="input-group">
                        <div class="input-group-addon">Authorization</div>
                        <input type="text" class="form-control" data-bind="value: authorizationHeader" />
                        <div class="input-group-btn">
                            <button type="button" class="btn btn-default" data-bind="click: generateAuthorizationHeader">Generate</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- /ko -->

        <div class="form-group">
            <input type="checkbox" data-bind="checked: model.okayToContact" /> It is OK to contact me.
        </div>

        <button type="button" class="btn btn-primary" data-bind="click: saveChanges, enable: model.validation.isValid">Save</button>
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

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
