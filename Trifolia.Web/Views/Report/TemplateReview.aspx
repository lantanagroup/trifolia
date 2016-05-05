<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .groupBorder {
            border-right: 2px solid #ddd;
        }
        .groupHeading {
            text-align: center;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="TemplateReview">
        <h2>Template/Profile Review</h2>
        
        <div data-bind="template: { name: 'pageNavigation' }"></div>

        <table class="table" id="TemplateReviewResults">
            <thead>
                <tr>
                    <th colspan="2" class="groupBorder groupHeading">Template/Profile</th>
                    <th colspan="7"></th>
                </tr>
                <tr>
                    <th>Name</th>
                    <th class="groupBorder">Identifier</th>
                    <th>Implementation Guide</th>
                    <th>Applies To</th>
                    <th>Constraint</th>
                    <th>Is Primitive</th>
                    <th>Has SCH</th>
                    <th>Value Set</th>
                    <th>Code System</th>
                </tr>
                <tr>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().TemplateName" />
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().TemplateOid" />
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().ImplementationGuideName" />
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().AppliesTo" />
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().ConstraintNumber" />
                    </td>
                    <td>
                        <select class="form-control" data-bind="value: Filter().IsPrimitive" style="width: 75px;">
                            <option value="">Any</option>
                            <option>Yes</option>
                            <option>No</option>
                        </select>
                    </td>
                    <td>
                        <select class="form-control" data-bind="value: Filter().HasSchematron" style="width: 75px;">
                            <option value="">Any</option>
                            <option>Yes</option>
                            <option>No</option>
                        </select>
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().ValueSetName" />
                    </td>
                    <td>
                        <input type="text" class="form-control" data-bind="value: Filter().CodeSystemName" />
                    </td>
                </tr>
            </thead>
            <!-- ko with: Model -->
            <tbody data-bind="foreach: Items">
                <tr>
                    <td>
                        <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + TemplateId() }"><span data-bind="text: TemplateName"></span></a>
                    </td>
                    <td>
                        <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + TemplateId() }"><span data-bind="text: TemplateOid"></span></a>
                    </td>
                    <td>
                        <a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }"><span data-bind="text: ImplementationGuideName"></span></a>
                    </td>
                    <td data-bind="text: AppliesTo"></td>
                    <td data-bind="text: ConstraintNumber"></td>
                    <td data-bind="text: IsPrimitive"></td>
                    <td data-bind="text: HasSchematron"></td>
                    <td data-bind="text: ValueSetName"></td>
                    <td data-bind="text: CodeSystemName"></td>
                </tr>
            </tbody>
            <!-- /ko -->
        </table>

        <div data-bind="template: { name: 'pageNavigation' }"></div>
        
        <script type="text/html" id="pageNavigation">
            Page <span data-bind="text: Filter().PageCount"></span> of <span data-bind="text: TotalPages"></span>, <span data-bind="text: Model().Total"></span> items in total
            <div class="btn-group btn-group-sm">
                <button type="button" class="btn btn-default btn-sm" data-bind="click: FirstPage, disable: Filter().PageCount() == 1" title="First Page">
                    <i class="glyphicon glyphicon-fast-backward"></i>
                </button>
                <button type="button" class="btn btn-default btn-sm" data-bind="click: PreviousPage, disable: Filter().PageCount() == 1" title="Previous Page">
                    <i class="glyphicon glyphicon-backward"></i>
                </button>
                <button type="button" class="btn btn-default btn-sm" data-bind="click: NextPage, disable: Filter().PageCount() == TotalPages()" title="Next Page">
                    <i class="glyphicon glyphicon-forward"></i>
                </button>
                <button type="button" class="btn btn-default btn-sm" data-bind="click: LastPage, disable: Filter().PageCount() == TotalPages()" title="Last Page">
                    <i class="glyphicon glyphicon-fast-forward"></i>
                </button>
            </div>
        </script>
    </div>

    <script type="text/javascript" src="/Scripts/Report/TemplateReview.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new TemplateReviewViewModel();
            ko.applyBindings(viewModel, $('#TemplateReview')[0]);
        });
    </script>

</asp:Content>