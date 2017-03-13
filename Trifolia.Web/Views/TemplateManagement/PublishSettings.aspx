<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #mainBody .row {
            padding-top: 5px;
        }

        .constraintRow .btn-sm {
            margin-bottom: 5px;
        }

        .constraintRow {
            border-bottom: 1px solid #CCCCCC;
            line-height: 30px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="mainBody">
        <h2><span data-bind="if: IsModified">* </span>Edit Publish Settings</h2>

        <div data-bind="with: Model">
            <div class="dropdown" style="padding-bottom: 5px;">
                <btn type="button" class="btn btn-default" data-toggle="dropdown" href="#" style="font-size: 24px;"><span data-bind="text: TemplateName"></span> <span class="caret"></span></btn>
                <ul class="dropdown-menu" role="menu" aria-labelledby="dLabel">
                    <li><a data-bind="attr: { href: '/TemplateManagement/View/Id/<%= Model %>' }">View</a></li>
                    <li><a data-bind="attr: { href: '/TemplateManagement/Edit/Id/<%= Model %>' }">Edit</a></li>
                </ul>
            </div>

            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#templateSamples" data-toggle="tab">Template/Profile Samples</a></li>
                <li><a href="#constraints" data-toggle="tab">Constraints</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="templateSamples">
                    <div class="row">
                        <div class="col-md-10"><strong>Sample Name</strong></div>
                        <div class="col-md-2">
                            <div class="pull-right">
                                <button type="button" class="btn btn-primary" data-bind="click: function() { viewModel.EditTemplateSample(); }">Add</button>
                            </div>
                        </div>
                    </div>

                    <!-- ko foreach: viewModel.GetTemplateSamples() -->
                    <div class="row">
                        <div class="col-md-10" data-bind="text: Name"></div>
                        <div class="col-md-2">
                            <div class="pull-right">
                                <div class="btn-group">
                                    <button type="button" class="btn btn-primary" data-bind="click: function() { viewModel.EditTemplateSample($data); }">Edit</button>
                                    <button type="button" class="btn btn-default" data-bind="click: function() { viewModel.RemoveTemplateSample($data); }">Remove</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- /ko -->
                </div>

                <div class="tab-pane" id="constraints">
                    <div data-bind="template: { name: 'constraintTemplate', data: Constraints() }"></div>
                </div>
            </div>
        </div>

        <div class="form-group">
            <button type="button" class="btn btn-primary" data-bind="click: Save">Save</button>
            <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
        </div>

        <!-- Constraint Editor -->
        <div class="modal fade" id="editConstraintDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Constraint</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentConstraint, validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <input type="checkbox" data-bind="checked: IsHeading" />
                            <label>Heading</label>
                        </div>

                        <div class="form-group" data-bind="visible: IsHeading()">
                            <label>Heading Description</label>
                            <textarea class="form-control" style="height: 50px;" data-bind="value: HeadingDescription"></textarea>
                        </div>
                        

                        <div class="form-group">
                            <label>Description</label>
                            <textarea class="form-control" style="height: 50px;" data-bind="value: ConstraintDescription"></textarea>
                        </div>
                        
                        <div class="form-group">
                            <label>Label</label>
                            <input type="text" class="form-control" data-bind="value: ConstraintLabel" />
                        </div>
                        
                        <div class="container-fluid" data-bind="visible: IsHeading()">
                            <div class="row">
                                <div class="col-md-10"><strong>Sample Name</strong></div>
                                <div class="col-md-2">
                                    <div class="pull-right">
                                        <button type="button" class="btn btn-default btn-sm" data-bind="click: function() { viewModel.EditConstraintSample(); }">Add</button>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- ko foreach: $parent.GetConstraintSamples() -->
                            <div class="row">
                                <div class="col-md-9" data-bind="text: Name"></div>
                                <div class="col-md-3">
                                    <div class="pull-right">
                                        <div class="btn-group btn-group-sm">
                                            <button type="button" class="btn btn-primary btn-sm" data-bind="click: function() { viewModel.EditConstraintSample($data); }">Edit</button>
                                            <button type="button" class="btn btn-default btn-sm" data-bind="click: function() { viewModel.RemoveConstraintSample($data); }">Remove</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- /ko -->
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: CurrentConstraint">
                        <button type="button" class="btn btn-primary" data-bind="click: viewModel.SaveConstraint, enable: IsValid">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: viewModel.CancelEditConstraint">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Constraint Sample Editor -->
        <div class="modal fade" id="editConstraintSampleDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Constraint Sample</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentConstraintSample, validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <label>Name</label>
                            <input type="text" class="form-control" data-bind="value: Name" />
                        </div>
                        <div class="form-group">
                            <label>Sample Contents</label>
                            <textarea type="text" class="form-control" style="height: 150px;" data-bind="value: SampleText"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: CurrentConstraintSample">
                        <button type="button" class="btn btn-primary" data-bind="click: viewModel.SaveConstraintSample, enable: IsValid">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: viewModel.CancelEditConstraintSample">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="templateSampleEditorDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Template/Profile Sample</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentTemplateSample, validationOptions: { messagesOnModified: false }">
                        <div class="form-group">
                            <label>Name</label>
                            <input type="text" class="form-control" data-bind="value: Name" />
                        </div>

                        <div class="form-group">
                            <label>Sample Contents</label>
                            <textarea class="form-control" style="height: 150px;" data-bind="value: SampleText"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: CurrentTemplateSample">
                        <div class="pull-left">
                            <button type="button" class="btn btn-default" data-bind="click: viewModel.GenerateTemplateSample" title="Automatically generate the sample based on the template/profile definition">Generate</button>
                            <button type="button" class="btn btn-default" data-bind="click: viewModel.TidyTemplateSample" title="Format and indent the XML sample">Format and Indent</button>
                        </div>
                        <button type="button" class="btn btn-primary" data-bind="click: viewModel.SaveTemplateSample, enable: IsValid">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: viewModel.CancelEditTemplateSample">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <script type="text/html" id="constraintTemplate">
            <!-- ko foreach: $data -->
            <div class="row constraintRow">
                <div class="col-md-10">
                    <!-- ko foreach: $parents -->
                    <!-- ko if: $index() >= 1 -->
                    <span style="white-space: pre">    </span>
                    <!-- /ko -->
                    <!-- /ko -->
                    <span data-bind="text: $index() + 1"></span>. <span data-bind="text: DisplayText"></span>
                    <!-- ko if: IsHeading() -->
                    <span class="badge" data-bind="tooltip: HeadingDescription">Heading</span>
                    <!-- /ko -->
                </div>
                <div class="col-md-2">
                    <div class="pull-right">
                        <button type="button" class="btn btn-primary btn-sm" data-bind="click: function() { viewModel.EditConstraint($data); }">Edit</button>
                    </div>
                </div>
            </div>

            <div data-bind="template: { name: 'constraintTemplate', data: ChildConstraints() }"></div>
            <!-- /ko -->
        </script>
    </div>
        
    <script type="text/javascript" src="/Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateManagement/publishModels.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateManagement/constraintViewModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateManagement/PublishSettingsViewModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/lib/vkbeautify.0.99.00.beta.js"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new publishSettingsViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $("#mainBody")[0]);

            $("#editConstraintDialog").modal({
                show: false,
                backdrop: 'static',
                keyboard: false
            });

            $("#editConstraintSampleDialog").modal({
                show: false,
                backdrop: 'static',
                keyboard: false
            });

            $("#templateSampleEditorDialog").modal({
                show: false,
                backdrop: 'static',
                keyboard: false
            });

            $(window).bind('beforeunload', viewModel.ConfirmLeave);
        });
    </script>

</asp:Content>