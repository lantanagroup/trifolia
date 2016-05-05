<%@ Page Title="Edit Template Names and Bookmarks" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master"  Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #EditBookmarks .row {
            padding-top: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="EditBookmarks">
        <h2>Edit Bookmarks</h2>

        <div class="form-group">
            <button type="button" class="btn btn-primary" data-bind="click: RegenerateAll">Regenerate All Bookmarks</button>
        </div>
        
        <div data-bind="with: Model">
            <h3 data-bind="text: Name"></h3>

            <div class="row">
                <div class="col-md-4">
                    <strong>Name</strong>
                </div>
                <div class="col-md-4">
                    <strong>Bookmark</strong>
                </div>
            </div>
            <!-- ko foreach: TemplateItems -->
            <div class="row">
                <div class="col-md-4">
                    <input type="text" class="form-control" data-bind="value: Name" placeholder="Name" />
                </div>
                <div class="col-md-4">
                    <input type="text" class="form-control" data-bind="value: Bookmark" placeholder="Bookmark" />
                </div>
            </div>
            <!-- /ko -->
        </div>

        <div class="row">
            <div class="col-md-12">
                <div class="btn-group">
                    <button type="button" class="btn btn-primary" data-bind="click: Save, enable: Model().IsValid">Save</button>
                    <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/IGManagement/Bookmarks.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new EditBookmarksViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#EditBookmarks')[0]);
        });
    </script>
</asp:Content>