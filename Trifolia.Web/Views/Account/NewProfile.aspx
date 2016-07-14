<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.Account.NewProfileModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="mainBody">
        <h2>New Profile</h2>

        <p style="font-style: italic">This information, including name, email address, and phone number, is collected from users that voluntarily enter the information in the process of authoring templates.  It is stored in a manner appropriate to the nature of the data and is used only for purposes related to the authoring and maintenance of the templates entered by the user. The information collected is never provided to any other company for that company's independent use.</p>
        
        <div data-bind="with: Model, validationOptions: { messagesOnModified: false }">
            <div class="form-group">
                <label>First Name</label>
                <input type="text" class="form-control" data-bind="value: FirstName" />
            </div>
        
            <div class="form-group">
                <label>Last Name</label>
                <input type="text" class="form-control" data-bind="value: LastName" />
            </div>
        
            <div class="form-group">
                <label>Phone</label>
                <input type="text" class="form-control" data-bind="value: Phone" />
            </div>
        
            <div class="form-group">
                <label>Email</label>
                <input type="text" class="form-control" data-bind="value: Email" />
            </div>
        
            <div class="form-group">
                <label>Organization</label>
                <input type="text" class="form-control" data-bind="value: Organization" />
            </div>

            <div class="form-group">
                <select class="form-control" data-bind="value: OrganizationType, options: $parent.OrgTypes" />
            </div>

            <div class="form-group">
                <input type="checkbox" data-bind="checked: OkayToContact" /> It is OK to contact me.
            </div>

            <div class="form-group">
                <button type="button" class="btn btn-default" data-bind="click: $parent.SaveChanges, enable: validation.isValid">Save</button>
            </div>
        </div>
    </div>
    
    <script type="text/javascript" src="/Scripts/Account/NewProfile.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var vm = new newProfileViewModel('<%= Model.RedirectUrl %>');
            vm.Model.FirstName('<%= Model.FirstName %>');
            vm.Model.LastName('<%= Model.LastName %>');
            vm.Model.Email('<%= Model.Email %>');
            vm.Model.Phone('<%= Model.Phone %>');
            ko.applyBindings(vm, $("#mainBody")[0]);
        });
    </script>
</asp:Content>
