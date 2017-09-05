<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .description {
            margin-top: 10px;
            white-space: pre;
            word-wrap: break-word;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ViewValueSet">
        <!-- ko if: Message() -->
        <div class="alert alert-warning" data-bind="html: Message"></div>
        <!-- /ko -->
        <div data-bind="if: ValueSet()">
            <div class="page-header">
                <h1><span data-bind="text: ValueSet().Name"></span> <small data-bind="text: ValueSet().Oid"></small></h1>
            </div>
            <p class="label label-default" data-bind="text: CompleteText, tooltip: CompleteHint"></p>
            <!-- ko if: ValueSet().Code() -->
            <p class="label label-default" data-bind="text: ValueSet().Code"></p>
            <!-- /ko -->
            <!-- ko if: ValueSet().ImportSource() -->
            <p class="label label-default" data-bind="text: ImportSourceDisplay"></p>
            <!-- /ko -->
            <!-- ko if: ValueSet().SourceUrl() -->
            <p>Source URL: <a target="_new" data-bind="attr: { href: ValueSet().SourceUrl }, text: ValueSet().SourceUrl"></a></p>
            <!-- /ko -->
            <!-- ko if: ValueSet().IsIntentional() -->
            <div class="well well-sm">
                <h4>Intentional definition</h4>
                <span data-bind="text: ValueSet().IntentionalDefinition"></span>
            </div>
            <!-- /ko -->
            <!-- ko if: ValueSet().Description() -->
            <p class="description" data-bind="text: ValueSet().Description"></p>
            <!-- /ko -->
        </div>

        <ul class="nav nav-tabs" role="tablist">
            <li role="presentation" class="active"><a href="#concepts" aria-controls="concepts" role="tab" data-toggle="tab">Concepts</a></li>
            <li role="presentation"><a href="#relationships" aria-controls="relationships" role="tab" data-toggle="tab">Relationships</a></li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
            <!-- Concepts -->
            <div role="tabpanel" class="tab-pane active" id="concepts">
                <form id="ConceptSearchForm">
                    <div class="input-group">
                        <div class="input-group-addon">Search</div>
                        <input type="text" class="form-control" data-bind="value: SearchQuery" />
                        <div class="input-group-btn">
                            <button type="button" class="btn btn-default" data-bind="click: function () { SearchQuery(''); Search(); }"><i class="glyphicon glyphicon-remove"></i></button>
                            <button type="submit" class="btn btn-default" data-bind="click: Search">Search</button>
                        </div>
                    </div>
                </form>

                <!-- ko if: LastPage() > 1 -->
                <p>
                    <div class="btn-group">
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: GoToFirstPage" title="First Page"><i class="glyphicon glyphicon-fast-backward"></i></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: PreviousPage" title="Previous Page"><i class="glyphicon glyphicon-backward"></i></button>
                        <button type="button" class="btn btn-default" disabled>Page <span data-bind="text: Page"></span> of <span data-bind="    text: LastPage"></span></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: NextPage" title="Next Page"><i class="glyphicon glyphicon-forward"></i></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: GoToLastPage" title="Last Page"><i class="glyphicon glyphicon-fast-forward"></i></button>
                    </div>
                </p>
                <!-- /ko -->

                <!-- ko if: !Loading() -->
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Code</th>
                            <th>Display Name</th>
                            <th style="width: 200px;">Code System</th>
                            <th style="width: 200px;">Status</th>
                            <th style="width: 150px;">Date</th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: Concepts">
                        <tr>
                            <td data-bind="text: Code"></td>
                            <td data-bind="text: DisplayName"></td>
                            <td data-bind="text: $parents[$parents.length - 1].GetCodeSystemName(CodeSystemId())"></td>
                            <td data-bind="text: Status"></td>
                            <td data-bind="text: $parents[$parents.length - 1].GetWellFormattedDate(StatusDate())"></td>
                        </tr>
                    </tbody>
                </table>
                <!-- /ko -->

                <!-- ko if: LastPage() > 1 -->
                <p>
                    <div class="btn-group">
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: GoToFirstPage" title="First Page"><i class="glyphicon glyphicon-fast-backward"></i></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: PreviousPage" title="Previous Page"><i class="glyphicon glyphicon-backward"></i></button>
                        <button type="button" class="btn btn-default" disabled>Page <span data-bind="text: Page"></span> of <span data-bind="    text: LastPage"></span></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: NextPage" title="Next Page"><i class="glyphicon glyphicon-forward"></i></button>
                        <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: GoToLastPage" title="Last Page"><i class="glyphicon glyphicon-fast-forward"></i></button>
                    </div>
                </p>
                <!-- /ko -->
            </div>

            <!-- Relationships -->
            <div role="tabpanel" class="tab-pane" id="relationships">
                <table class="table">
                    <thead>
                        <tr>
                            <th>Implementation Guide</th>
                            <th>Template/Profile</th>
                            <th>Published?</th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: Relationships">
                        <tr>
                            <td><a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }, text: ImplementationGuideName"></a></td>
                            <td>
                                <a data-bind="attr: { href: TemplateUrl() }, text: TemplateName"></a> (<span data-bind="    text: TemplateOid"></span>)
                                <div class="pull-right">
                                    <span class="glyphicon glyphicon-chevron-down clickable" onclick="toggleBindings(this)"></span>
                                </div>
                            </td>
                            <!-- ko if: IsImplementationGuidePublished() -->
                            <td>Yes</td>
                            <!-- /ko -->
                            <!-- ko if: !IsImplementationGuidePublished() -->
                            <td>No</td>
                            <!-- /ko -->
                        </tr>
                        <tr style="display: none" class="bindings">
                            <td colspan="2">
                                <table class="table">
                                    <thead>
                                        <tr>
                                            <th>Constraint Number</th>
                                            <th>Binding Date</th>
                                            <th>Binding Strength</th>
                                        </tr>
                                    </thead>
                                    <tbody data-bind="foreach: Bindings">
                                        <tr>
                                            <td data-bind="text: ConstraintNumber" title="Constraint Number"></td>
                                            <td data-bind="text: $parents[$parents.length - 1].GetWellFormattedDate(Date())" title="Binding Date"></td>
                                            <td data-bind="text: $parents[$parents.length - 1].GetBindingStrengthDisplay(Strength())" title="Binding Strength"></td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/TerminologyManagement/View.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = new ViewValueSetViewModel(<%= Model %>);
        ko.applyBindings(viewModel, $("#ViewValueSet")[0]);

        // not putting this in the view-model because it uses jquery directly
        function toggleBindings(span) {
            $(span).parent().parent().parent().next('.bindings').toggle();

            var removeClass = $(span).hasClass('glyphicon-chevron-up') ? 'glyphicon-chevron-up' : 'glyphicon-chevron-down';
            var addClass = $(span).hasClass('glyphicon-chevron-up') ? 'glyphicon-chevron-down' : 'glyphicon-chevron-up';
            $(span).removeClass(removeClass).addClass(addClass);
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>
