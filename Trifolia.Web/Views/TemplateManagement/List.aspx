<%@ Page Title="Trifolia: Templates" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.ListModel>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="BrowseTemplates">
        <h3>Browse Templates/Profiles</h3>

        <form id="TemplateSearch">
            <div class="input-group" style="padding-bottom: 10px;">
                <input id="SearchQuery" type="text" class="form-control" data-bind="value: SearchQuery, valueUpdate: 'keyup'" autofocus="autofocus" />
                <span class="input-group-btn">
                    <button class="btn btn-default" type="button" data-bind="click: function () { SearchQuery(''); Search(); }">
                        <i class="glyphicon glyphicon-remove"></i>
                    </button>
                    <button class="btn btn-default" type="submit" data-bind="click: Search">Search</button>
                </span>
            </div>
        </form>

        <div data-bind="template: { name: 'pageNavigation' }"></div>
        
        <table class="table table-striped">
            <thead>
                <tr>
                    <th style="cursor: pointer;" data-bind="click: function () { ToggleSort('Name'); }">
                        Name
                        <!-- ko if: SortProperty() == 'Name' && !SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-up"></i>
                        <!-- /ko -->
                        <!-- ko if: SortProperty() == 'Name' && SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-down"></i>
                        <!-- /ko -->
                    </th>
                    <th style="cursor: pointer;" data-bind="click: function () { ToggleSort('Oid'); }">
                        Identifier
                        <!-- ko if: SortProperty() == 'Oid' && !SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-up"></i>
                        <!-- /ko -->
                        <!-- ko if: SortProperty() == 'Oid' && SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-down"></i>
                        <!-- /ko -->
                    </th>
                    <th style="cursor: pointer;" data-bind="click: function () { ToggleSort('ImplementationGuide'); }">
                        Implementation Guide
                        <!-- ko if: SortProperty() == 'ImplementationGuide' && !SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-up"></i>
                        <!-- /ko -->
                        <!-- ko if: SortProperty() == 'ImplementationGuide' && SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-down"></i>
                        <!-- /ko -->
                    </th>
                    <th style="cursor: pointer;" data-bind="click: function () { ToggleSort('TemplateType'); }">
                        Type
                        <!-- ko if: SortProperty() == 'TemplateType' && !SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-up"></i>
                        <!-- /ko -->
                        <!-- ko if: SortProperty() == 'TemplateType' && SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-down"></i>
                        <!-- /ko -->
                    </th>
                    <!-- ko if: !Model() || !Model().HideOrganization() -->
                    <th style="cursor: pointer;" data-bind="click: function () { ToggleSort('Organization'); }">
                        Organization
                        <!-- ko if: SortProperty() == 'Organization' && !SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-up"></i>
                        <!-- /ko -->
                        <!-- ko if: SortProperty() == 'Organization' && SortDescending() -->
                        <i class="glyphicon glyphicon-chevron-down"></i>
                        <!-- /ko -->
                    </th>
                    <!-- /ko -->
                    <th style="width: 130px;">
                        <div class="pull-right" data-bind="if: containerViewModel.HasSecurable(['TemplateEdit'])">
                            <a class="btn btn-primary" href="/TemplateManagement/Edit/New" title="Add new template/profile">Add</a>
                        </div>
                    </th>
                </tr>
                <tr>
                    <td><input type="text" class="form-control" data-bind="value: FilterName, valueUpdate: 'keyup'" /></td>
                    <td><input type="text" class="form-control" data-bind="value: FilterOid, valueUpdate: 'keyup'" /></td>
                    <td><select class="form-control" data-bind="value: FilterImplementationGuideId, options: ImplementationGuides, optionsText: 'Title', optionsValue: 'Id', optionsCaption: 'Filter...'"></select></td>
                    <td><select class="form-control" data-bind="value: FilterTemplateTypeId, options: TemplateTypes, optionsText: 'FullName', optionsValue: 'Id', optionsCaption: 'Filter...'"></select></td>
                    <!-- ko if: !Model() || !Model().HideOrganization() -->
                    <td><select class="form-control" data-bind="value: FilterOrganizationId, options: Organizations, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Filter...', enable: containerViewModel.HasSecurable(['OrganizationList'])"></select></td>
                    <!-- /ko -->
                    <td>
                        <button type="button" class="btn btn-default" style="width: 100%;" data-bind="click: ClearFilter">Clear Filter</button>
                    </td>
                </tr>
            </thead>
            <!-- ko with: Model -->
            <tbody data-bind="foreach: Items">
                <tr data-bind="highlight: $parents[1].SearchQuery">
                    <td>
                        <span data-bind="text: Name"></span>
                        <!-- ko if: Description() -->
                        <i class="glyphicon glyphicon-comment" data-bind="tooltip: Description"></i>
                        <!-- /ko -->
                    </td>
                    <td data-bind="text: Oid"></td>
                    <td data-bind="text: ImplementationGuide"></td>
                    <td data-bind="text: TemplateType"></td>
                    <!-- ko if: !$parent.HideOrganization() -->
                    <td data-bind="text: Organization"></td>
                    <!-- /ko -->
                    <td>
                        <div class="btn-group btn-group-sm pull-right" style="width: 100%;">
                            <a class="btn btn-default btn-sm" data-bind="attr: { href: $parents[1].ViewTemplateUrl($data) }" style="width: 50%;">View</a>
                            <!-- ko if: containerViewModel.HasSecurable(['TemplateEdit']) -->
                            <a class="btn btn-default btn-sm" data-bind="attr: { href: $parents[1].EditTemplateUrl($data) }, css: { disabled: !CanEdit() }" style="width: 50%;">Edit</a>
                            <!-- /ko -->
                        </div>
                    </td>
                </tr>
            </tbody>
            <!-- /ko -->
        </table>
        <div data-bind="template: { name: 'pageNavigation' }"></div>
    </div>

    <script type="text/html" id="pageNavigation">
        Page <span data-bind="text: PageCount"></span> of <span data-bind="    text: TotalPages"></span>, <span data-bind="    text: TotalItems"></span> templates/profiles
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-default btn-sm" data-bind="click: FirstPage, disable: PageCount() == 1" title="First Page">
                <i class="glyphicon glyphicon-fast-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: PreviousPage, disable: PageCount() == 1" title="Previous Page">
                <i class="glyphicon glyphicon-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: NextPage, disable: PageCount() == TotalPages()" title="Next Page">
                <i class="glyphicon glyphicon-forward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: LastPage, disable: PageCount() == TotalPages()" title="Last Page">
                <i class="glyphicon glyphicon-fast-forward"></i>
            </button>
        </div>
    </script>

    <script type="text/javascript" src="/Scripts/TemplateManagement/List.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new BrowseTemplatesViewModel();
            ko.applyBindings(viewModel, $("#BrowseTemplates")[0]);
            viewModel.WatchFilters();

            $("#SearchQuery").keypress(function (e) {
                if (e.keyCode == 13) {
                    viewModel.Search();
                } else if (e.keyCode == 27) {
                    viewModel.SearchQuery('');
                    viewModel.Search();
                }
            });
        });
    </script>
</asp:Content>
