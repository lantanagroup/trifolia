<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.Account.LoginModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= BotDetect.Web.CaptchaUrls.Absolute.LayoutStyleSheetUrl %>" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        function InitUsernameText(s, e) {
            s.Focus();
        }
    </script>
    <script src='https://www.google.com/recaptcha/api.js'></script>
    <style type="text/css">
        .btn-login {
            display: block;
            margin-top: 10px;
        }

        #TrifoliaCaptcha {
            margin-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <form action="/Account/DoLogin<%= Request.Params.ToString().Split('&').Contains("debug") ? "?debug" : "" %>" method="post">
        <h2>Login</h2>
    
        <h3>HL7 Members can login and non-members can register to use Trifolia Workbench <a href="<%= Model.HL7LoginLink %>">here.</a></h3>

        <% if (!string.IsNullOrEmpty(Model.ReturnUrl)) { %>
        <div class="alert alert-warning">You do not have access to the resource requested. Please login with an account which <i>does</i> have the access desired.</div>
        <% } %>

        <% if (!string.IsNullOrEmpty(Model.Message)) { %>
        <div class="alert alert-info"><%= Model.Message %></div>
        <% } %>

        <%= Html.HiddenFor(y => y.ReturnUrl) %>

        <div class="form-group">
            <label>Organization</label>
            <select class="form-control" name="OrganizationId">
                <%
                    foreach (var cOrganizationId in Model.Organizations.Keys)
                    {
                        var organizationDisplay = Model.Organizations[cOrganizationId];
                        var organizationId = cOrganizationId;
                %>
                <option value="<%= organizationId %>"><%= organizationDisplay %></option>
                <%
                    }
                %>
            </select>
        </div>
    
        <div class="form-group">
            <label>Username</label>
            <input type="text" name="Username" class="form-control" placeholder="Username" value="<%= Model.Username %>" autofocus />
        </div>
    
        <div class="form-group">
            <label>Password</label>
            <input type="password" name="Password" class="form-control" placeholder="Password" />
        </div>

        <div class="form-group">
            <input type="checkbox" name="RememberMe" value="true" checked="<%= Model.RememberMe %>" /> Remember Me
        </div>

        <% 
            if (!Model.RecaptchaAllowBypass || !Request.Params.ToString().Split('&').Contains("debug")) { 
            MvcCaptcha captcha = new MvcCaptcha("TrifoliaCaptcha");
        %>
        <%= Html.Captcha(captcha) %>
        <%= Html.TextBox("CaptchaCode") %>
        <% } %>

        <button type="submit" class="btn btn-default btn-login">Login</button>
    </form>

</asp:Content>