<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.BetaViewModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="BetaRegistration">
        <h1>Welcome, Beta Testers!</h1>
        <h4>Thank you for submitting your application to join our closed beta for Trifolia's Read/Write version. We will contact successful applicants using your selected contact preference.</h4>
        <p><i>By submitting your application, you certify that you are over 18 years old and will be willing to sign a Beta User Agreement should you be selected for testing.</i></p>

        <% if (ViewData["Message"] != null && !string.IsNullOrEmpty((string)ViewData["Message"])) { %>
        <p style="font-style: italic; color: red;"><%= ViewData["Message"] %></p>
        <% } %>

        <form method="post" action="/Beta/SubmitBetaApplicant">
            <div class="form-group">
                <label>Name</label>
                <input type="text" class="form-control" name="NameText" autofocus="autofocus" data-bind="value: Name" />
            </div>
        
            <div class="form-group">
                <label>Company</label>
                <input type="text" class="form-control" name="CompanyText" data-bind="value: Company" />
            </div>
        
            <div class="form-group">
                <label>Email</label>
                <input type="text" class="form-control" name="EmailText" data-bind="value: Email" />
            </div>
        
            <div class="form-group">
                <label>Phone</label>
                <input type="text" class="form-control" name="PhoneText" data-bind="value: Phone" />
            </div>

            <div class="form-group">
                <label>Contact via</label>
                <select name="ContactPreference" class="form-control" data-bind="value: ContactPreference">
                    <option value="Phone">Phone</option>
                    <option value="Email">Email</option>
                </select>
            </div>

            <div class="form-group">
                <%= Html.GenerateCaptcha() %>
            </div>

            <div class="form-group">
                <button type="submit" class="btn btn-primary" data-bind="enable: IsValid">Submit</button>
            </div>
        </form>
    </div>

    <script type="text/javascript" src="/Scripts/Beta/Index.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new BetaRegistrationViewModel();
            viewModel.Name('<%= Model.NameText %>');
            viewModel.Company('<%= Model.CompanyText %>');
            viewModel.Email('<%= Model.EmailText %>');
            viewModel.Phone('<%= Model.PhoneText %>');
            viewModel.ContactPreference('<%= Model.ContactPreference %>');

            ko.applyBindings(viewModel, $('#BetaRegistration')[0]);
        });
    </script>
</asp:Content>