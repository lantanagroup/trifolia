<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="DeleteTemplate">
        <h2>Delete Template/Profile</h2>

        <div data-bind="with: Template">
            <h3><a data-bind="attr: { href: '/TemplateManagement/View/Id/' + Id() }, text: Name"></a></h3>
            <h4 data-bind="text: Oid"></h4>

            <p>Deleting the template/profile is a permanent action that cannot be reversed. If you delete the template/profile, relationships to the template/profile will be removed as well. If you delete a template/profile that is being implied by another template/profile, the other template/profile will no longer imply it. If you delete a template/profile that is contained by another template/profile, the template/profile associated on the constraint will be removed, but not the constraint itself. Alternatively, you can select a replacement template/profile that will replace these implied and contained relationships.</p>

            <p><b>Samples:</b> <span data-bind="text: Samples().length"></span></p>
            <p><b>Contained By Templates/Profiles:</b> <span data-bind="text: ContainedByTemplates().length"></span></p>

            <div class="form-group">
                <div data-bind="templateSelect: $parent.ReplaceTemplateId, label: 'Replacement Template/Profile'"></div>
            </div>

            <div class="form-group">
                <div class="btn-group">
                    <button type="button" class="btn btn-primary" data-bind="click: $parent.Delete">Delete</button>
                    <button type="button" class="btn btn-default" data-bind="click: $parent.Cancel">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/TemplateManagement/Delete.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new DeleteTemplateViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#DeleteTemplate')[0]);
        });
    </script>

</asp:Content>
