<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.EditModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
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
            height: 94%;
        }

        body > .container-fluid {
            padding: 5px;
        }

        body > .container-fluid > .main {
            padding: inherit;
        }

        .editor > div:first-child > .tab-content > .tab-pane {
            padding: 5px;
        }

        /* Meta Data Tab */
        .editor > div:first-child > .tab-content > .tab-pane:first-child > .col-md-6:first-child {
            padding-left: 5px;
        }

        .editor > div:first-child > .tab-content > .tab-pane:first-child > .col-md-6:last-child {
            padding-right: 5px;
        }

        .editor > div:first-child > .tab-content > .tab-pane:first-child textarea {
            height: 150px;
        }

        .editor > div:first-child > .tab-content > .tab-pane:first-child .input-group {
            width: 100%;
        }

        .editor > div:first-child > .tab-content > .tab-pane:first-child .input-group-addon {
            min-width: 125px;
            text-align: left;
        }

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

        /* LEFT NAVIGATION */

        .editor > div:nth-child(2) {
            margin-left: 20px;
        }

        .left-nav {
            border-bottom-right-radius: 4px;
            border-top-right-radius: 4px;
            width: 25px;
            position: fixed;
            height: 80%;
            z-index: 3;
        }

        .left-nav.expanded {
            width: 50%;
            border: 1px solid transparent;
            border-color: #ddd;
            background-color: white;
        }

        .left-nav > .btn {
	        transform: rotate(90deg);
	        transform-origin: left top 0;
            margin-left: 17px;
            margin-top: 3px;
        }

        .left-nav.expanded > .btn {
            position: absolute;
            right: 0;
            margin-right: -157px;
            margin-top: -1px;
        }

        .left-nav-content {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="editor" ng-app="NewEditor" ng-controller="EditorController" ng-init="init(<%= Model.TemplateIdString %>, <%= Model.DefaultImplementationGuideIdString %>)">
        <div class="left-nav" ng-class="{ 'expanded': leftNavExpanded }">
            <button type="button" class="btn btn-default btn-sm" ng-click="toggleLeftNav()">Templates/Profiles</button>
            <div class="left-nav-content" ng-show="leftNavExpanded">
                <div class="input-group">
                    <input type="text" placeholder="Search Text" class="form-control" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-primary">Search</button>
                    </div>
                </div>

                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Identifier</th>
                            <th>Description</th>
                            <th>&nbsp;</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Test 1</td>
                            <td>urn:oid:1.1.1.1</td>
                            <td>Test Description 1</td>
                            <td>
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-open"></i>
                                        </button>
                                        <button type="button" class="btn btn-default btn-sm">
                                            <i class="glyphicon glyphicon-new-window"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <tabset>
            <tab heading="Meta Data">
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
                            <input class="form-control" type="text" />
                            <div class="input-group-btn">
                                <button class="dropdown-toggle btn btn-default" type="button" href="#" data-toggle="dropdown">
                                    <span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu pull-right">
                                    <li><a href="#">urn:oid:1.3.6.1.4.1.19376.1.4.1.7</a></li>
                                    <li><a href="#">http://localhost:49366/api/FHIR2/StructureDefinition</a></li>
                                    <!-- ko if: !$parent.IsFhir() -->
                                    <li><a href="#">urn:oid:</a></li>
                                    <li><a href="#">urn:hl7ii:</a></li>
                                    <!-- /ko -->
                                </ul>
                            </div>
                            <input class="form-control" type="text" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Short Id:</div>
                            <input type="text" class="form-control" ng-model="template.Bookmark" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Implied Template/Profile:</div>
                            <input type="text" class="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Extensibility:</div>
                            <select class="form-control"></select>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Status:</div>
                            <select class="form-control"></select>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Description:</div>
                            <textarea class="form-control"></textarea>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Notes:</div>
                            <textarea class="form-control"></textarea>
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
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="input-group">
                            <div class="input-group-addon">Organization:</div>
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                    </div>
                </div>
            </tab>
            <tab heading="Constraints">
                <div class="constraint-container">
                    <div class="constraint-body">
                        <tree-grid 
                            constraints="constraints" 
                            template="template" 
                            nodes="nodes" 
                            node-selected="nodeSelected(selectedNode)" 
                            node-expanded="nodeExpanded(node)">
                        </tree-grid>
                    </div>
                    <div class="constraint-properties">
                        <div class="panel panel-default panel-sm">
                            <div class="panel-heading">
                                <span>Properties</span>
                                <div class="pull-right">
                                    <button type="button" ng-show="!selectedNode || selectedNode.Constraint" class="btn btn-default btn-sm">Add Primitive Constraint</button>
                                </div>
                            </div>
                            <div class="panel-body">
                                <div ng-if="selectedNode && selectedNode.Constraint" ng-include="'constraintPanel.html'"></div>
                                <button type="button" ng-show="selectedNode && !selectedNode.Constraint" class="btn btn-default">Create Computable Constraint</button>
                            </div>
                        </div>
                    </div>
                </div>
            </tab>
            <tab heading="Validation">

            </tab>
            <tab heading="Preview">

            </tab>
        </tabset>

        <script type="text/html" id="treeGrid.html">
            <div class="tree-grid">
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Context</th>
                            <th>Number</th>
                            <th>Conf.</th>
                            <th>Card.</th>
                            <th>DataType</th>
                            <th>Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="clickable" ng-repeat="node in flattenedNodes" ng-click="toggleSelect(node)" ng-class="{ 'highlight': node.Constraint, 'danger': selectedNode == node }">
                            <td>
                                <span style="white-space: pre">{{getNodeTabs(node)}}</span>
                                <i ng-if="node.HasChildren" class="glyphicon clickable" ng-class="{ 'glyphicon-plus': !node.$expanded, 'glyphicon-minus': node.$expanded }" ng-click="toggleExpand(node)"></i>
                                {{node.Context}} ({{node.Constraint != undefined}})
                            </td>
                            <td>{{getCellDisplay(node, 'Number')}}</td>
                            <td>{{getCellDisplay(node, 'Conformance')}}</td>
                            <td>
                                {{getCellDisplay(node, 'Cardinality')}}
                                <i class="glyphicon glyphicon-exclamation-sign" ng-if="isInvalidCardinality(node)" title="{{isInvalidCardinality(node)}}"></i>
                            </td>
                            <td>{{getCellDisplay(node, 'DataType')}}</td>
                            <td>{{getCellDisplay(node, 'Value')}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </script>

        <script type="text/html" id="constraintPanel.html">
            <sub>General</sub>

            <!-- Conf/Card -->
            <div class="form-group">
                <div class="input-group input-group-sm" style="width: 100%;">
                    <div class="input-group-addon">Conf/Card:</div>
                    <select class="form-control input-sm" style="width: 50%;" ng-model="selectedNode.Constraint.Conformance">
                        <option>SHALL</option>
                        <option>SHOULD</option>
                        <option>MAY</option>
                        <option>SHALL NOT</option>
                        <option>SHOULD NOT</option>
                        <option>MAY NOT</option>
                    </select>
                    <div class="input-group input-group-sm cardinality" style="width:50%; padding-top: 0px">
                        <input class="span2 form-control" id="appendedInputButton" size="16" type="text" ng-model="selectedNode.Constraint.Cardinality">
                        <div class="input-group-btn">
                            <a class="dropdown-toggle btn btn-primary btn-sm" data-toggle="dropdown" href="#">
                                <span class="caret"></span>
                            </a>
                            <ul class="dropdown-menu pull-right">
                                <li><a href="#" ng-click="selectedNode.Constraint.Cardinality = '0..0'">0..0</a></li>
                                <li><a href="#" ng-click="selectedNode.Constraint.Cardinality = '0..1'">0..1</a></li>
                                <li><a href="#" ng-click="selectedNode.Constraint.Cardinality = '0..*'">0..*</a></li>
                                <li><a href="#" ng-click="selectedNode.Constraint.Cardinality = '1..1'">1..1</a></li>
                                <li><a href="#" ng-click="selectedNode.Constraint.Cardinality = '1..*'">1..*</a></li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Data Type -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        <span>Data Type:</span>
                        <span class="glyphicon glyphicon-question-sign clickable"></span>
                    </div>
                    <select class="form-control input-sm">
                        <option value="">DEFAULT</option>
                    </select>
                </div>
            </div>

            <!-- Branching -->
            <div class="form-group">
                <div class="input-group input-group-sm branching">
                    <div class="input-group-addon">Branch/Slice:</div>
                    <div class="form-control">
                        <input type="checkbox" name="Branching">&nbsp;<span>Root</span><br>
                        <input type="checkbox" name="Branching">&nbsp;<span>Identifier/Discriminator</span>
                    </div>
                </div>
            </div>

            <!-- Contained Template/Profile -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">Template/Profile:</div>
                    <input type="text" class="form-control" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default btn-sm">
                            <i class="glyphicon glyphicon-remove"></i>
                        </button>
                        <button type="button" class="btn btn-default btn-sm">
                            ...
                        </button>
                    </div>
                </div>
            </div>

            <hr />
            <sub>Bindings</sub>

            <!-- Binding Type -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Binding Type:
                        <i class="glyphicon glyphicon-question-sign clickable"></i>
                    </div>
                    <select class="form-control input-sm">
                        <option value="None">None</option>
                        <option value="SingleValue">Single Value</option>
                        <option value="ValueSet">Value Set</option>
                        <option value="CodeSystem">Code System</option>
                        <option value="Other">Other</option>
                    </select>
                </div>
            </div>

            <hr />
            <sub>Publishing</sub>

            <!-- Description -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon" title="Exported before constraint">Description:</div>
                    <textarea class="form-control input-sm" style="height: 50px;"></textarea>
                </div>
            </div>

            <!-- Label -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon" title="Exported at end of constraint">Label:</div>
                    <input type="text" class="form-control input-sm" />
                </div>
            </div>

            <!-- Heading -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Heading:&nbsp;
                        <input type="checkbox" />
                    </div>
                    <textarea class="form-control input-sm" style="height: 50px;" placeholder="Heading Description"></textarea>
                </div>
            </div>

            <hr />
            <sub>Schematron</sub>

            <!-- Auto Generate -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Auto Generate:
                    </div>
                    <select class="form-control">
                        <option value="true">Yes</option>
                        <option value="false">No</option>
                    </select>
                </div>
            </div>

            <!-- Inheritable -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Inheritable:
                    </div>
                    <select class="form-control">
                        <option value="true">Yes</option>
                        <option value="false">No</option>
                    </select>
                </div>
            </div>

            <!-- Rooted -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Rooted:
                    </div>
                    <select class="form-control">
                        <option value="true">Yes</option>
                        <option value="false">No</option>
                    </select>
                </div>
            </div>

            <!-- Custom Schematron -->
            <div class="form-group">
                <div class="input-group input-group-sm">
                    <div class="input-group-addon">
                        Schematron:
                    </div>
                    <textarea class="form-control input-sm" style="height: 50px"></textarea>
                </div>
            </div>
        </script>
    </div>
    
    <script src="http://igniteui.com/js/modernizr.min.js"></script>
    <script src="http://igniteui.com/js/angular.min.js"></script>
    <script src="http://code.jquery.com/ui/1.10.3/jquery-ui.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/ui-bootstrap-tpls-0.12.1.min.js"></script>
    <script type="text/javascript" src="/Scripts/lodash.min.js"></script>
    <script type="text/javascript" src="/Scripts/TemplateEdit/newTemplateEditor.js"></script>
    
    <script src="http://cdn-na.infragistics.com/igniteui/latest/js/infragistics.core.js"></script>
    <script src="http://cdn-na.infragistics.com/igniteui/latest/js/infragistics.lob.js"></script>
    <script src="http://cdn-na.infragistics.com/igniteui/latest/js/extensions/igniteui-angular.js"></script>
    <link href="http://cdn-na.infragistics.com/igniteui/latest/css/themes/infragistics/infragistics.theme.css" rel="stylesheet"></link>
    <link href="http://cdn-na.infragistics.com/igniteui/latest/css/structure/infragistics.css" rel="stylesheet"></link>
</asp:Content>