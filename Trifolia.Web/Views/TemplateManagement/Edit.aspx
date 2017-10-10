<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.EditModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        /* Tabs and container divs should fill screen */
        html,
        body,
        body > .container-fluid,
        .main,
        .editor,
        .editor-tabs > .tab-content > .tab-pane,
        .constraint-body,
        .constraint-container {
            height: 100%;
        }

        .main {
            position: absolute;
            left: 2px;
            right: 2px;
            bottom: 2px;
        }

        .editor {
            position: relative;
            padding-top: 50px;
        }

        .editor-tabs {
            position: absolute;
            right: 2px;
            left: 2px;
            bottom: 2px;
            top: 104px;
        }

        .editor-tabs > .tab-content {
            position: absolute;
            top: 42px;
            right: 0px;
            left: 0px;
            bottom: 0px;
        }

        .editor > fieldset > div:nth-child(2) {
            height: 100%;
            overflow-y: auto;
        }

        .editor > fieldset > div:nth-child(2) > .tab-content {
            height: 86%;
        }

        body > .container-fluid {
            padding: 5px;
        }

        body > .container-fluid > .main {
            padding: inherit;
        }

        fieldset {
            margin: auto;
            padding: inherit;
            border: inherit;
        }

        .left-nav {
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
            margin-top: -26px;
        }

        .editor-tabs {
            margin-left: 33px;
        }

        /* Title and labels */
        .editor .navbar-header .label {
            float: left;
            margin-top: 16px;
        }

        .glyphicon-plus,
        .glyphicon-remove,
        .editor .navbar-header .label > .glyphicon {
            cursor: pointer;
        }

        .editor-tabs > .tab-content > .tab-pane {
            padding: 5px;
        }

        /* Meta Data Tab */        
        .identifier-field select,
        .identifier-field input {
            width: 50% !important;
        }

        .metadata-fields textarea {
            height: 150px;
        }

        .metadata-fields .input-group {
            width: 100%;
        }

        .metadata-fields .input-group-addon {
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
            position: absolute;
            top: 0px;
            right: 0px;
            bottom: 0px;
        }
        
        .constraint-properties .panel {
            position: absolute;
            top: 1px;
            bottom: -26px;
            right: 0px;
            left: 0px;
        }

        .constraint-properties .panel > .panel-heading {
            padding: 8px;
            height: 38px;
        }

        .constraint-properties .panel > .panel-heading > span {
            float: left;
            padding-left: 10px;
        }

        .constraint-properties .panel > .panel-heading > .input-group {
            float: left;
            margin-top: -5px;
            width: 20%;
        }

        .constraint-properties .panel-heading .btn-group {
            margin-top: -5px;
        }
        
        .constraint-properties .panel > .panel-body {
            position: absolute;
            overflow-y: auto;
            top: 38px;
            bottom: 0px;
        }

        .constraint-properties .panel > .panel-body .form-group {
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

        .tree-grid .number-col-head {
            max-width: 50px;
        }

        .tree-grid .number-col-head input {
            padding: 6px;
            height: 25px;
            width: 70px;
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

        .constraint-preview > ol {
            padding-top: 5px;
        }

        .constraint-preview li {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="editor ng-cloak" ng-app="Trifolia" ng-controller="EditorController" ng-init="init(<%= Model.TemplateIdString %>, <%= Model.DefaultImplementationGuideIdString %>)" block-ui="constraintBlock">
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
                    <span class="label label-primary" ng-show="isLocked" ng-click="unlock()">Locked <i class="glyphicon glyphicon-question-sign"uib-tooltip="This template is locked for editing. The Implementation Guide it is associated with is published. Click this button to unlock it" tooltip-trigger="'click'" tooltip-placement="bottom"></i></span>            
                </div>

                <!-- Collect the nav links, forms, and other content for toggling -->
                <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                    <ul class="nav navbar-nav navbar-right">
                        <li ng-click="save()" ng-class="{ disabled: !isValid() }"><a href="#">Save</a></li>
                        <li ng-click="discard()"><a href="#">Discard</a></li>
                    </ul>
                </div><!-- /.navbar-collapse -->
            </div><!-- /.container-fluid -->
        </nav>

        <div class="alert alert-warning" ng-show="authTimeout">Your session has timed out. You will need to <a href="#" ng-click="reload()">reload the page</a> to re-authenticate and edit this template.</div>
        <!-- <div class="alert alert-info" ng-show="message">{{message}}</div> -->

        <!-- LEFT NAV (template search) -->
        <div class="left-nav">
            <button type="button" class="btn btn-default btn-sm" ng-disabled="false" uib-popover-template="'/Scripts/TemplateEdit/templateSearch.html'" popover-placement="right" popover-append-to-body="true" popover-class="template-search" popover-trigger="'outsideClick'" popover-is-open="leftNavOpened">Templates/Profiles</button>
        </div>

        <!-- MAIN EDITOR TABS -->
        <uib-tabset class="editor-tabs" active="activeTab">
            <uib-tab index="'metadata'" classes="metadata-tab" heading="Meta Data">
                <fieldset class="metadata-fields" ng-disabled="isLocked">
                    <div class="col-md-6">
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Name:</div>
                                <input type="text" class="form-control" ng-model="template.Name" ng-change="nameChanged()" />
                            </div>
                        </div>
                        <div class="long-id form-group">
                            <div class="input-group identifier-field">
                                <div class="input-group-addon">
                                    <span>Long Id:</span>
                                    <div title="" class="glyphicon glyphicon-question-sign clickable"></div>
                                </div>
                                <select class="form-control" ng-model="identifier.base" ng-change="identifierChanged()">
                                    <option>{{implementationGuide.Identifier + '.'}}</option>
                                    <option ng-if="!isFhir">urn:oid:</option>
                                    <option ng-if="!isFhir">urn:hl7ii:</option>
                                    <option>http://</option>
                                    <option>https://</option>
                                </select>
                                <input class="form-control" type="text" ng-model="identifier.ext" ng-change="identifierChanged()" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Short Id:</div>
                                <input type="text" class="form-control" ng-model="template.Bookmark" ng-change="templateChanged()" />
                            </div>
                        </div>
                        <template-select caption="Implied Template/Profile:" template-id="template.ImpliedTemplateId" restrict-type="!isFhir" restricted-type="template.PrimaryContextType" on-changed="templateChanged()" form-group="true"></template-select>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Extensibility:</div>
                                <select class="form-control" ng-model="template.IsOpen" ng-options="i.v as i.d for i in [{ v: true, d: 'Open' }, {v: false, d: 'Closed' }]" ng-change="templateChanged()">
                                </select>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Status:</div>
                                <select class="form-control" ng-model="template.StatusId" ng-options="s.Id as s.Status for s in statuses" ng-change="templateChanged()">
                                </select>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Description:</div>
                                <textarea class="form-control" ng-model="template.Description" ng-change="templateChanged()"></textarea>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Notes:</div>
                                <textarea class="form-control" ng-model="template.Notes" ng-change="templateChanged()"></textarea>
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
                                <input type="text" class="form-control" readonly="readonly" value="TODO" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Applies To:</div>
                                <input type="text" class="form-control" readonly="readonly" value="{{template.PrimaryContext}} {{template.PrimaryContextType}}" />
                            </div>
                            <span class="help-block"><a href="/TemplateManagement/Move/Id/{{template.Id}}">Move</a> the template/profile to change the Implementation Guide, Type, or Applies To fields.</span>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Authored By:</div>
                                <select class="form-control" ng-model="template.AuthorId" ng-options="p.Id as p.Name for p in permissions" ng-change="templateChanged()">
                                    <option>TODO</option>
                                </select>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-group">
                                <div class="input-group-addon">Organization:</div>
                                <input type="text" class="form-control" readonly="readonly" value="{{template.OrganizationName}}" />
                            </div>
                        </div>
                    </div>

                    <pre ng-if="isDebug">{{template | json}}</pre>
                </fieldset>
            </uib-tab>
            <uib-tab index="'constraints'" classes="constraints-tab" heading="Constraints">
                <div class="constraint-container">
                    <div class="constraint-body">
                        <tree-grid 
                            constraints="constraints" 
                            template="template" 
                            nodes="nodes" 
                            node-selected="nodeSelected(selectedNode)" 
                            node-expanded="nodeExpanded(selectedNode)"
                            search-constraint="selectConstraint(number)"
                            validate-node="isNodeValid(node)">
                        </tree-grid>
                    </div>
                    <div class="constraint-properties">
                        <fieldset ng-disabled="isLocked">
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
                                        <div ng-if="isDuplicateNode(selectedNode)" class="btn-group">
                                            <button type="button" class="btn btn-default btn-sm" title="Move the duplicated constraint up"  ng-show="showMoveUp(selectedNode)" ng-click="moveUp(selectedNode)">
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
                                    <div ng-if="selectedNode && selectedNode.Constraint" ng-include="'/Scripts/TemplateEdit/constraintsPanel.html'"></div>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </div>
            </uib-tab>
            <uib-tab index="'validation'" classes="validation-tab" heading="Validation {{template.ValidationResults.length > 0 ? '(' + template.ValidationResults.length + ')' : ''}}">
                <div class="alert alert-info">Validation is not updated in real-time during template/profile editing. Validation is only updated when loading and saving the template/profile.</div>
                <table class="table table-striped">
                    <thead>
                        <th>Level</th>
                        <th>Number</th>
                        <th>Message</th>
                    </thead>
                    <tbody>
                        <tr ng-repeat="vr in template.ValidationResults">
                            <td>{{vr.Level}}</td>
                            <td ng-if="vr.ConstraintNumber"><a href="#" ng-click="selectConstraint(vr.ConstraintNumber)">{{vr.ConstraintNumber}}</a></td>
                            <td ng-if="!vr.ConstraintNumber">N/A</td>
                            <td>{{vr.Message}}</td>
                        </tr>
                    </tbody>
                </table>
            </uib-tab>
            <uib-tab index="'relationships'" classes="relationships-tab" heading="Relationships">
                <div ng-if="template.ContainedByTemplates.length > 0">
                    <h3>Contained By</h3>
                    <ul>
                        <li ng-repeat="t in template.ContainedByTemplates">
                            {{t.Name}}<br />{{t.ImplementationGuide}}
                        </li>
                    </ul>
                </div>

                <div ng-if="template.ImpliedByTemplates.length > 0">
                    <h3>Implied By</h3>
                    <ul>
                        <li ng-repeat="t in template.ImpliedByTemplates">
                            {{t.Name}}<br />{{t.ImplementationGuide}}
                        </li>
                    </ul>
                </div>
            </uib-tab>
            <uib-tab index="'preview'" classes="preview-tab" heading="Preview">
                <div class="constraint-preview">
                    <ol>
                        <li ng-repeat="c in constraints" ng-include="'constraintPreview.html'"></li>
                    </ol>
                </div>
            </uib-tab>
        </uib-tabset>

        <script type="text/html" id="constraintPreview.html">
            <span ng-bind-html="c.NarrativeProseHtml"></span>
            <ol ng-if="c.Children.length > 0">
                <li ng-repeat="c in c.Children" ng-include="'constraintPreview.html'"></li>
            </ol>
        </script>

        <script type="text/html" id="constraintNumberPopover.html">
            <div class="constraint-number-popover">
                <div class="form-group">
                    <label>Unique Number</label>
                    <i class="glyphicon glyphicon-question-sign" tooltip-trigger="'click'" uib-tooltip="A unique number is always required. The unique number is used by default for conformance numbers in exports unless a display number is specified. This number must be unique across all constraints in the implementation guide that the template/profile is associated with."></i>
                    <input type="number" class="form-control" ng-model="selectedNode.Constraint.Number" />
                </div>
                
                <div class="form-group">
                    <label>Display Number</label>
                    <i class="glyphicon glyphicon-question-sign" tooltip-trigger="'click'" uib-tooltip="Optional. The display number is used to override the conformance number format used by default in exports (MS Word, Schematron, etc.). The display number can contain any character (including dashes, underlines, semi-colons, etc.)"></i>
                    <input type="text" class="form-control" value="TODO" />
                </div>
            </div>
        </script>

        <script type="text/html" id="addContainedTemplate.html">
            <div class="modal-header">
                <h3 class="modal-title" id="modal-title">Add Contained Template/Profile</h3>
            </div>
            <div class="modal-body" id="modal-body">
                <div class="input-group">
                    <input type="text" class="form-control" ng-model="query" placeholder="Search" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default" ng-click="search()">Search</button>
                    </div>
                </div>

                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Identifier</th>
                            <th>&nbsp;</th>
                        </tr>
                    </thead>
                    <tbody ng-if="results">
                        <tr ng-repeat="t in results.Items">
                            <td>
                                {{t.Name}}
                                <sub ng-show="t.Description">{{t.Description.substring(1, 100)}}</sub>
                            </td>
                            <td>{{t.Oid}}</td>
                            <td>
                                <div class="pull-right">
                                    <button type="button" class="btn btn-default btn-sm" ng-click="select(t)">Select</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="3">No results</td>
                        </tr>
                    </tfoot>
                </table>
            </div>
            <div class="modal-footer">
                <button class="btn btn-warning" type="button" ng-click="$ctrl.cancel()">Cancel</button>
            </div>
        </script>
    </div>
    
    <script type="text/javascript" src="/Scripts/TemplateEdit/editorController.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>""></script>
</asp:Content>