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
            height: 86%;
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
    <div class="editor" ng-app="Trifolia" ng-controller="EditorController" ng-init="init(<%= Model.TemplateIdString %>, <%= Model.DefaultImplementationGuideIdString %>)">
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
                        <template-select caption="Implied Template/Profile"></template-select>
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
                            node-expanded="nodeExpanded(selectedNode)">
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
                                <div ng-if="selectedNode && selectedNode.Constraint" ng-include="'/Scripts/NewTemplateEditor/constraintsPanel.html'"></div>
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

        <script type="text/html" id="templateSelect.html">
            <div class="input-group" ng-class="{ 'input-group-sm': smallFields }">
                <div class="input-group-addon">{{caption}}</div>
                <input type="text" class="form-control" />
                <div class="input-group-btn">
                    <button type="button" class="btn btn-default" ng-class="{ 'btn-sm': smallFields }" ng-click="openModal()">
                        <i class="glyphicon glyphicon-edit"></i>
                    </button>
                    <button type="button" class="btn btn-default" ng-class="{ 'btn-sm': smallFields }" ng-click="clearSelection()">
                        <i class="glyphicon glyphicon-remove"></i>
                    </button>
                </div>
            </div>
        </script>
        <script type="text/html" id="templateSelectModal.html">
            <div class="modal-header">
                <h3 class="modal-title" id="modal-title">{{caption}}</h3>
            </div>
            <div class="modal-body" id="modal-body">
                <form>
                    <div class="input-group">
                        <input type="text" class="form-control" placeholder="Search Text..." ng-model="searchText" />
                        <div class="input-group-btn">
                            <button type="submit" class="btn btn-default" ng-click="search()">Search</button>
                        </div>
                    </div>
                </form>

                <table class="table table-striped" ng-if="searchResults">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Identifier</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr ng-repeat="r in searchResults.Items">
                            <td>{{r.Name}}</td>
                            <td>{{r.Oid}}</td>
                            <td>
                                <div class="pull-right">
                                    <button type="button" class="btn btn-default btn-sm" ng-click="select(r)">Select</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="modal-footer">
                <button class="btn btn-warning" type="button" ng-click="close()">Close</button>
            </div>
        </script>
    </div>
    
    <script type="text/javascript" src="/Scripts/NewTemplateEditor/editorController.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>""></script>
</asp:Content>