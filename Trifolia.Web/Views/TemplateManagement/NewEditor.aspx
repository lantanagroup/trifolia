<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.EditModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        /* Tabs and container divs should fill screen */
        html,
        body,
        body > .container-fluid,
        body > .container-fluid > .main,
        body > .container-fluid > .main > .editor,
        body > div.container-fluid > div > div > div > div > div.tab-pane:nth-child(2),
        .constraint-container {
            height: 100%;
        }

        body > .container-fluid > .main > .editor > div:nth-child(2) {
            height: 100%;
            overflow-y: auto;
        }

        body > .container-fluid > .main > .editor > div:nth-child(2) > .tab-content {
            height: 86%;
        }

        body > .container-fluid {
            padding: 5px;
        }

        body > .container-fluid > .main {
            padding: inherit;
        }

        .editor > h2:first-child {
            padding-top: 0px;
            margin-top: 0px;
        }

        .editor > .left-nav {
            border-bottom-right-radius: 4px;
            border-top-right-radius: 4px;
            width: 31px;
            position: fixed;
            height: 80%;
            z-index: 3;
        }

        .left-nav > .btn {
	        transform: rotate(90deg);
	        transform-origin: left top 0;
            margin-left: 30px;
        }

        .editor > .editor-tabs {
            margin-left: 33px;
        }


        /* Title and labels */
        .editor .navbar-header .label {
            float: left;
            margin-top: 16px;
        }

        .editor .navbar-header .label > .glyphicon {
            cursor: pointer;
        }


        /* Meta Data Tab */
        .editor > div:first-child > .tab-content > .tab-pane {
            padding: 5px;
        }

        .editor > .left-nav > .tab-content > .tab-pane:first-child > .col-md-6:first-child {
            padding-left: 5px;
        }

        .editor > .left-nav > .tab-content > .tab-pane:first-child > .col-md-6:last-child {
            padding-right: 5px;
        }
        
        .editor .identifier-field select,
        .editor .identifier-field input {
            width: 50%;
        }

        .editor > .editor-tabs > .tab-content > .tab-pane:first-child textarea {
            height: 150px;
        }

        .editor > .editor-tabs > .tab-content > .tab-pane:first-child .input-group {
            width: 100%;
        }

        .editor > .editor-tabs > .tab-content > .tab-pane:first-child .input-group-addon {
            min-width: 125px;
            text-align: left;
        }

        /* LEFT NAVIGATION */

        .template-search.popover {
            max-width: 60%;
            top: 62px !important;
        }

        .template-search.popover > .arrow {
            margin-top: -240px;
        }

        .template-search.popover .popover-inner {
            max-height: 30vw;
            overflow-y: auto;
        }

        /* CONSTRAINTS */
        
        .constraint-body {
            width: 65%;
        }

        .constraint-properties {
            margin-left: 5px;
            width: 35%;
        }
        
        .constraint-properties .panel {
            display: inline-block;
            width: 100%;
            height: 100%;
        }

        .constraint-properties > .panel > .panel-heading {
            padding: 8px;
            height: 38px;
        }

        .constraint-properties > .panel > .panel-heading > span {
            float: left;
            padding-left: 10px;
        }

        .constraint-properties > .panel > .panel-heading > .input-group {
            float: left;
            margin-top: -5px;
            width: 20%;
        }

        .constraint-properties .panel-heading .btn-group {
            margin-top: -5px;
        }
        
        .constraint-properties > .panel > .panel-body {
            padding: 8px;
            max-height: 88%;
            overflow-y: auto;
            width: 100%;
        }

        .constraint-properties > .panel > .panel-body .form-group {
            margin-bottom: 5px;
        }

        .constraint-properties .branching,
        .constraint-properties .branching .form-control {
            height: 55px;
        }

        .constraint-properties .input-group > .input-group-addon:first-child {
            min-width: 125px;
            text-align: left;
        }

        .constraint-container {
            display: inline-flex;
            width: 100%;
        }

        .ui-widget-header {
            border-top-left-radius: 4px;
            border-top-right-radius: 4px;
        }

        .constraint-properties hr {
            margin-top: 10px;
            margin-bottom: 10px;
            border-top: 1px solid gray;
        }

        .constraint-properties sub {
            bottom: 1.1em;
        }

        .constraint-properties sub:first-child {
            bottom: .7em;
        }

        .tree-grid {
            height: 100%;
            overflow-y: auto;
            min-width: 75%;
            overflow-x: auto;
        }

        .tree-grid tr.highlight {
            background-color: inherit;
            font-weight: bold;
        }

        .is-heading select {
            width: 50% !important;
        }

        /* Miscellaneous */
        .search-actions {
            width: 100px;
        }

        .constraint-number-popover {
            width: 300px;
            height: 125px;
        }

        .constraint-number-popover input {
            width: 240px !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="editor ng-cloak" ng-app="Trifolia" ng-controller="EditorController" ng-init="init(<%= Model.TemplateIdString %>, <%= Model.DefaultImplementationGuideIdString %>)">
        <nav class="navbar navbar-default">
            <div class="container-fluid">
                <!-- Brand and toggle get grouped for better mobile display -->
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand" href="#">{{template.Name}}</a>
                    <span class="label label-warning" ng-show="isModified">Modified <i class="glyphicon glyphicon-question-sign" uib-tooltip="This template has been modified since it was opened. Press the Save button to persist the changes." tooltip-trigger="'click'" tooltip-placement="bottom"></i></span>
                    <span class="label label-primary" ng-show="isLocked">Locked <i class="glyphicon glyphicon-question-sign"uib-tooltip="This template is locked for editing. The Implementation Guide it is associated with is published. Click this button to unlock it" tooltip-trigger="'click'" tooltip-placement="bottom"></i></span>            
                </div>

                <!-- Collect the nav links, forms, and other content for toggling -->
                <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                    <ul class="nav navbar-nav navbar-right">
                        <li ng-click="save()"><a href="#">Save</a></li>
                        <li ng-click="discard()"><a href="#">Discard</a></li>
                    </ul>
                </div><!-- /.navbar-collapse -->
            </div><!-- /.container-fluid -->
        </nav>

        <!-- LEFT NAV (template search) -->
        <div class="left-nav">
            <button type="button" class="btn btn-default btn-sm" uib-popover-template="'/Scripts/NewTemplateEditor/templateSearch.html'" popover-placement="right" popover-append-to-body="true" popover-class="template-search" popover-trigger="'outsideClick'">Templates/Profiles</button>
        </div>

        <!-- MAIN EDITOR TABS -->
        <uib-tabset class="editor-tabs">
            <uib-tab heading="Meta Data">
                <div class="col-md-6">
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Name:</div>
                            <input type="text" class="form-control" ng-model="template.Name" />
                        </div>
                    </div>
                    <div class="long-id form-group">
                        <div class="input-group identifier-field">
                            <div class="input-group-addon">
                                <span>Long Id:</span>
                                <div title="" class="glyphicon glyphicon-question-sign clickable"></div>
                            </div>
                            <select class="form-control" ng-model="identifier.base" ng-change="updateIdentifier()">
                                <option>{{implementationGuide.Identifier + '.'}}</option>
                                <option ng-if="!isFhir">urn:oid:</option>
                                <option ng-if="!isFhir">urn:hl7ii:</option>
                                <option>http://</option>
                                <option>https://</option>
                            </select>
                            <input class="form-control" type="text" ng-model="identifier.ext" ng-change="updateIdentifier()" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Short Id:</div>
                            <input type="text" class="form-control" ng-model="template.Bookmark" />
                        </div>
                    </div>
                    <template-select caption="Implied Template/Profile:" template-id="template.ImpliedTemplateId" restrict-type="!isFhir" restricted-type="template.PrimaryContextType" on-changed="templateChanged()" form-group="true"></template-select>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Extensibility:</div>
                            <select class="form-control" ng-model="template.IsOpen" ng-options="i.v as i.d for i in [{ v: true, d: 'Open' }, {v: false, d: 'Closed' }]">
                            </select>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Status:</div>
                            <select class="form-control" ng-model="template.StatusId" ng-options="s.Id as s.Status for s in statuses">
                            </select>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Description:</div>
                            <textarea class="form-control" ng-model="template.Description"></textarea>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Notes:</div>
                            <textarea class="form-control" ng-model="template.Notes"></textarea>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Implementation Guide</div>
                            <input type="text" class="form-control" readonly="readonly" value="{{template.OwningImplementationGuideId | implementationGuideName}}" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Type:</div>
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Applies To:</div>
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                        <span class="help-block"><a href="">Move</a> the template/profile to change the Implementation Guide, Type, or Applies To fields.</span>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Authored By:</div>
                            <select class="form-control"></select>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Organization:</div>
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                    </div>
                </div>

                <pre ng-if="isDebug">{{template | json}}</pre>
            </uib-tab>
            <uib-tab heading="Constraints">
                <div class="constraint-container">
                    <div class="constraint-body">
                        <tree-grid 
                            constraints="constraints" 
                            template="template" 
                            nodes="nodes" 
                            node-selected="nodeSelected(selectedNode)" 
                            node-expanded="nodeExpanded(selectedNode)">
                        </tree-grid>
                    </div>
                    <div class="constraint-properties">
                        <div class="panel panel-default panel-sm">
                            <div class="panel-heading">
                                <div class="input-group" ng-show="selectedNode.Constraint">
                                    <input type="text" class="form-control input-sm" readonly="readonly" ng-model="selectedNode.Constraint.Number" />
                                    <div class="input-group-btn" uib-popover-template="'constraintNumberPopover.html'" popover-title="Custom Number" popover-placement="bottom-left">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-pencil"></i>
                                        </button>
                                    </div>
                                </div>
                                <span><strong>Properties</strong></span>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm" title="Move the duplicated constraint up" ng-show="showMoveUp(selectedNode)" ng-click="moveUp(selectedNode)">
                                            <i class="glyphicon glyphicon-arrow-up"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm" title="Move the duplicated constraint down" ng-show="showMoveDown(selectedNode)" ng-click="moveDown(selectedNode)">
                                            <i class="glyphicon glyphicon-arrow-down"></i>
                                        </button>
                                    </div>
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm" title="Create a computable constraint for this node" ng-show="selectedNode" ng-click="createComputableConstraint(selectedNode)">
                                            <i class="glyphicon glyphicon-plus"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm" title="Add primitive/narrative constraint {{selectedNode.Constraint ? 'within the current constraint' : 'at the top level' }}" ng-click="createPrimitiveConstraint(selectedNode)">
                                            <i class="glyphicon glyphicon-text-height"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm" title="Duplicate the current constraint" ng-show="selectedNode && selectedNode.Constraint && !selectedNode.Constraint.IsPrimitive" ng-click="duplicateConstraint(selectedNode)">
                                            <i class="glyphicon glyphicon-repeat"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm" title="Remove constraint from the selected node" ng-show="selectedNode && selectedNode.Constraint" ng-click="deleteConstraint(selectedNode)">
                                            <i class="glyphicon glyphicon-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-body">
                                <div ng-if="selectedNode && selectedNode.Constraint" ng-include="'/Scripts/NewTemplateEditor/constraintsPanel.html'"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </uib-tab>
            <uib-tab heading="Validation">

            </uib-tab>
            <uib-tab heading="Preview">

            </uib-tab>
        </uib-tabset>

        <script type="text/html" id="constraintNumberPopover.html">
            <div class="constraint-number-popover">
                <div class="form-group">
                    <label>Unique Number</label>
                    <i class="glyphicon glyphicon-question-sign" tooltip-trigger="'click'" uib-tooltip="A unique number is always required. The unique number is used by default for conformance numbers in exports unless a display number is specified. This number must be unique across all constraints in the implementation guide that the template/profile is associated with."></i>
                    <input type="number" class="form-control" />
                </div>
                
                <div class="form-group">
                    <label>Display Number</label>
                    <i class="glyphicon glyphicon-question-sign" tooltip-trigger="'click'" uib-tooltip="Optional. The display number is used to override the conformance number format used by default in exports (MS Word, Schematron, etc.). The display number can contain any character (including dashes, underlines, semi-colons, etc.)"></i>
                    <input type="text" class="form-control" />
                </div>
            </div>
        </script>

        <script type="text/html" id="templateSelect.html">
        </script>
    </div>
    
    <script type="text/javascript" src="/Scripts/NewTemplateEditor/editorController.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>""></script>
</asp:Content>