<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .constraint {
            font-weight: bold;
        }

        .errorNode {
            color: red;
            font-style: italic;
        }

        .node {
            border-top: 1px solid #CCCCCC;
            height: 35px;
            line-height: 32px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="MoveTemplate">
        <h2>Move: Step <span data-bind="text: MoveStep"></span></h2>
        <h3 data-bind="text: Template().TemplateName"></h3>

        <div data-bind="if: MoveStep() == 1, validationOptions: { insertMessages: false }" class="container form-horizontal">
            <!-- ko with: Template -->
            <div class="form-group">
                <label>Implementation Guide</label>
                <select class="form-control" data-bind="value: ImplementationGuideId, options: $parent.ImplementationGuides, optionsText: 'Name', optionsValue: 'Id'"></select>
                <span data-bind="validationMessage: ImplementationGuideId"></span>
            </div>

            <div class="form-group" data-bind="if: ImplementationGuideId()">
                <label>Template/Profile Type</label>
                <select class="form-control" data-bind="value: TemplateTypeId, options: $parent.TemplateTypes, optionsText: 'Name', optionsValue: 'Id'"></select>
                <span data-bind="validationMessage: TemplateTypeId"></span>
            </div>

            <div class="form-group" data-bind="if: TemplateTypeId()">
                <label>Applis To</label>
                <div class="input-group" style="width: 100%">
                    <input type="text" class="form-control" readonly="readonly" style="width: 50%" data-bind="value: PrimaryContext" />
                    <input type="text" class="form-control" readonly="readonly" style="width: 50%" data-bind="value: PrimaryContextType" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default" data-bind="click: function () { $('#appliesToDialog').modal('show'); }">Change</button>   <!-- TODO -->
                    </div>
                </div>
                <span data-bind="validationMessage: PrimaryContext"></span>
                <span data-bind="validationMessage: PrimaryContextType"></span>
            </div>

            <div class="form-group">
                <button type="button" class="btn btn-primary" data-bind="click: $parent.MoveStep2, disable: !IsValid()">Next</button>
                <button type="button" class="btn btn-default" data-bind="click: $parent.Cancel">Cancel</button>
            </div>

            <!-- /ko -->
        </div>

        <div data-bind="if: MoveStep() == 2" class="constraints">
            <div data-bind="template: { name: 'constraintTemplate', data: Nodes() }"></div>

            <div class="form-group">
                <button type="button" class="btn btn-primary" data-bind="click: Finish">Finish</button>
                <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
            </div>
        </div>
        
        <div id="appliesToDialog" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">Applies to...</div>
                    <div class="modal-body" style="max-height: 350px; overflow-y: auto">
                        <div data-bind="template: { name: 'appliesToNode', data: AppliesToNodes() }"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-bind="click: function () {
                            viewModel.Template().PrimaryContext(viewModel.CurrentAppliesToNode().Context());
                            viewModel.Template().PrimaryContextType(viewModel.CurrentAppliesToNode().DataType());
                            viewModel.CurrentAppliesToNode(null);
                            $('#appliesToDialog').modal('hide');
                        }">OK</button>
                        <button type="button" class="btn btn-primary" data-bind="click: function () {
                            viewModel.CurrentAppliesToNode(null);
                            $('#appliesToDialog').modal('hide');
                        }">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <script type="text/html" id="appliesToNode">
        <!-- ko foreach: $data -->
        <div class="row" style="cursor: pointer;" data-bind="css: { 'highlight': $parents[$parents.length - 1].IsCurrentAppliesToNode($data) }">
            <div class="col-md-1">
                <span data-bind="if: (HasChildren() && !IsExpanded())">
                    <i data-bind="click: function () { $parents[$parents.length - 1].ExpandAppliesToNode($data); }" class="glyphicon glyphicon-expand"></i>
                </span>
                <span data-bind="if: (HasChildren() && IsExpanded())">
                    <div data-bind="click: function () { $parents[$parents.length - 1].CollapseNode($data); }" class="glyphicon glyphicon-collapse-down"></div>
                </span>
            </div>
            <div class="col-md-11" data-bind="click: function () { $parents[$parents.length - 1].CurrentAppliesToNode($data); }">
                <!-- ko foreach: $parents -->
                <!-- ko if: $index() > 1 --><span style="white-space: pre">  </span><!-- /ko -->
                <!-- /ko -->
                <span data-bind="text: Context"></span>
            </div>
        </div>
        
        <!-- ko if: IsExpanded() -->
        <div data-bind="template: { name: 'appliesToNode', data: Children() }"></div>
        <!-- /ko -->
        <!-- /ko -->
    </script>

    <script type="text/html" id="constraintTemplate">
        <!-- ko foreach: $data -->
        
        <div class="row node" data-bind="css: { 'constraint': Constraint(), 'errorNode': IsError() }, attr: { title: (Constraint() ? Constraint().Narrative() : '') }">
            <div class="col-md-8">
                <!-- ko foreach: $parents -->
                <!-- ko if: $index() != 0 -->
                <span style="white-space: pre">    </span>
                <!-- /ko -->
                <!-- /ko -->
                <span data-bind="text: Context"></span>
            </div>
            <div class="col-md-4">
                <div class="pull-right" data-bind="if: Constraint()">
                    <div class="btn-group">
                        <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[$parents.length - 1].RemoveConstraintButton($data); }">Remove</button>
                    </div>
                </div>
            </div>
        </div>

        <div data-bind="template: { name: 'constraintTemplate', data: ChildNodes() }"></div>
        <!-- /ko -->
    </script>

    <script type="text/javascript" src="../../Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateManagement/Move.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new MoveTemplateViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#MoveTemplate')[0]);

            $('#appliesToDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
        });
    </script>

</asp:Content>