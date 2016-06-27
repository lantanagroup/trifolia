<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>View</title>
    
    <!-- Any styles added here must be added to IGController.Download/resourceMappings -->
    <link rel="stylesheet" type="text/css" href="/Styles/bootstrap.min.css" />
    <link rel="stylesheet" type="text/css" href="/Styles/highlight.css" />
    <link rel="stylesheet" type="text/css" href="/Styles/joint.min.css" />
    <link rel="stylesheet" type="text/css" href="/Styles/Site.css" />
    <link rel="stylesheet" type="text/css" href="/Styles/IGView.css" />
    
    <!-- Any scripts added here must be added to IGController.Download/resourceMappings -->
    <script type="text/javascript" src="/Scripts/jquery/jquery-1.10.2.min.js"></script>
    <script type="text/javascript" src="/Scripts/jquery/jquery-highlight-5.js"></script>
    <script type="text/javascript" src="/Scripts/bootstrap/bootstrap.js"></script>
    <script type="text/javascript" src="/Scripts/angular/angular.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/angular-route.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/ui-bootstrap-tpls-0.12.1.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/highlight.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/angular-highlight.min.js"></script>
    <script type="text/javascript" src="/Scripts/lodash.min.js"></script>
    <script type="text/javascript" src="/Scripts/backbone-min.js"></script>
    <script type="text/javascript" src="/Scripts/joint.min.js"></script>
    <script type="text/javascript" src="/Scripts/graphlib.core.min.js"></script>
    <script type="text/javascript" src="/Scripts/dagre.core.min.js"></script>
    <script type="text/javascript" src="/Scripts/joint.layout.DirectedGraph.min.js"></script>
    
    <script type="text/javascript" src="/Scripts/IG/View.js"></script>

    <script type="text/javascript">
        var viewModel;
        var dataModelLocation = "<%= Model != null ? (string)Model : "%DATA_FILENAME%" %>";
        var jsonData = "%DATA%";
        var dataModel;
    </script>

    <style type="text/css">
        .constraintAdded {
            font-style: italic;
        }

        .constraintRemoved {
            text-decoration: line-through;
        }

        .constraintModified {
            font-weight: bold;
        }
    </style>
</head>
<body ng-app="igViewApp">
    <div class="MainDiv" ng-controller="ViewCtrl">
        <nav class="navbar navbar-default navbar-static-top" id="navigation">
            <div class="container-fluid">
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar" aria-expanded="false" aria-controls="navbar">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand" href="#/overview">{{Model.ImplementationGuideName}} - {{Model.Status}} {{Model.PublishDate}}</a>
                </div>
                <div id="navbar" class="navbar-collapse collapse">
                    <ul class="nav navbar-nav navbar-right">
                        <li><a ng-click="EditOptions()">Options</a></li>
                        <li ng-if="!IsDownloaded"><a href="{{GetDownloadLocation()}}">Download</a></li>
                    </ul>
                    <form class="navbar-form navbar-right navbar-input-group">
                        <div class="form-group">
                            <input type="text" class="form-control" placeholder="Search text" ng-model="SearchText">
                        </div>
                        <button type="button" class="btn btn-primary" ng-click="Search()">Search</button>
                    </form>
                </div>
            </div>
        </nav>

        <div class="container-fluid">
            <span ng-repeat="bc in BreadCrumbs" style="line-break: strict">
                <span ng-if="$index > 0"> &gt; </span>
                <span><a href="{{bc.url}}" ng-click="SelectBreadCrumb($index)">{{bc.display}}</a></span>
            </span>

            <div ng-view autoscroll="true"></div>
        </div>

        <div class="loading-pane" ng-show="IsLoading">
            <div class="centered">Loading</div>
        </div>
    </div>

    <script type="text/html" id="template.html">
        <div ng-if="Template">
            <h1 class="page-header">
                <span>{{Template.Name}}</span>
            </h1>
            <h4>
                <span>[{{Template.Context}}: {{Template.Identifier}} ({{Template.Extensibility}})]</span>
                <i ng-if="IsDebug" class="glyphicon glyphicon-question-sign" tooltip="{{Template}}" tooltip-placement="bottom" tooltip-trigger="click"></i>
            </h4>

            <p ng-bind-html="GetHtml(Template.Description)"></p>

            <div ng-if="!Options.TemplateTabs">

                <div class="panel panel-default">
                    <div class="panel-heading">
                        <a ng-click="Template.DiagramExpanded = !Template.DiagramExpanded" ng-class="{ collapsed: !Template.DiagramExpanded }">
                        UML Relationship Diagram
                        </a>
                    </div>
                    <div class="panel-body" ng-if="Template.DiagramExpanded">
                        <ig-diagram template="Template" model="Model"></ig-diagram>
                    </div>
                </div>

                <!-- Relationships -->
                <div class="panel panel-default" ng-if="HasRelationships(Template)">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#{{GetPath()}}#relationships">
                            Relationships
                            </a>
                        </h4>
                    </div>
                    <div class="panel-body panel-collapse collapse" id="relationships">

                        <div ng-include="'template_relationships.html'"></div>
                    </div>
                </div>

                <!-- Constraints Table -->
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#{{GetPath()}}#constrainttable">
                            Constraints Table
                            </a>
                        </h4>
                    </div>
                    <div class="table-responsive collapse in" id="constrainttable">
                        <div ng-include="'template_constraint_table.html'"></div>
                    </div>
                </div>

                <!-- Narrative Constraints -->
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse" href="#{{GetPath()}}#narrativeconstraints">
                            Narrative Constraints
                            </a>
                        </h4>
                    </div>
                    <div class="panel-body panel-collapse collapse in" id="narrativeconstraints">
                        <div ng-include="'template_narrative.html'"></div>
                    </div>
                </div>

                <!-- Template Changes -->
                <div class="panel panel-default" ng-show="ShowTemplateChangesTab()">
                    <div class="panel-heading">
                        <h4 class="panel-title">
                            <a data-toggle="collapse"></a>
                            Changes in This Version

                        </h4>
                    </div>
                    <div class="panel-body panel-collapse collapse in" id="templatechanges">
                        <div ng-include="'template_changes.html'"></div>
                    </div>
                </div>


                <!-- Samples -->
                <h3 ng-if="Template.Samples.length > 0">Samples</h3>
                <div ng-include="'template_samples.html'"></div>

                <!-- Value Sets -->
                <h3 ng-if="CurrentValueSets.length > 0">Value Sets</h3>
                <div ng-include="'template_valuesets.html'"></div>
            </div>
            <tabset ng-if="Options.TemplateTabs">
                <tab heading="UML Relationship Diagram">
                    <ig-diagram template="Template" model="Model"></ig-diagram>
                </tab>
                <tab heading="Relationships">
                    <div ng-include="'template_relationships.html'"></div>
                </tab>
                <tab heading="Constraints Table">
                    <div ng-include="'template_constraint_table.html'"></div>
                </tab>
                <tab heading="Narrative Constraints">
                    <div ng-include="'template_narrative.html'"></div>
                </tab>
                <tab heading="Samples" ng-if="Template.Samples.length > 0">
                    <div ng-include="'template_samples.html'"></div>
                </tab>
                <tab heading="Value Sets" ng-if="CurrentValueSets.length > 0">
                    <div ng-include="'template_valuesets.html'"></div>
                </tab>
            </tabset>
        </div>
    </script>

    <script type="text/html" id="codesystems.html">
        <h1 class="page-header">Code systems in this guide</h1>

        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Identifier</th>
                    </tr>
                </thead>
                <tbody>
                    <tr ng-repeat="cs in Model.CodeSystems">
                        <td><a id="{{cs.Identifier}}"></a>{{cs.Name}}</td>
                        <td>{{cs.Identifier}}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </script>

    <script type="text/html" id="valuesets.html">
        <h1 class="page-header">Value sets in this guide</h1>

        <div ng-repeat="vs in Model.ValueSets | orderBy: 'Name'">
            <a id="{{GetValueSetAnchor(vs)}}"></a>
            <div class="panel panel-default">
                <div class="panel-heading">
                    <a ng-click="vs.show = !vs.show" ng-class="{ collapsed: !vs.show }">
                        <strong>{{vs.Name}}</strong> {{vs.Identifier}}
                    </a>
                    <div class="pull-right valueset-date">{{vs.BindingDate}}</div>
                </div>
                <div class="table-responsive" ng-if="vs.show">
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <td colspan="4">
                                    <p>{{vs.Description}}</p>
                                    <p ng-if="vs.Source">
                                        <a href="{{vs.Source}}" target="_blank">{{vs.Source}}</a>
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <th>Code</th>
                                <th>Code System</th>
                                <th>Code System Identifier</th>
                                <th>Display</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr ng-repeat="vsm in vs.Members">
                                <td>{{vsm.Code}}</td>
                                <td>{{vsm.CodeSystemName}}</td>
                                <td>{{vsm.CodeSystemIdentifier}}</td>
                                <td>{{vsm.DisplayName}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </script>

    <script type="text/html" id="volume2.html">
        <h1 class="page-header">Templates</h1>

        <!--
        <p>Description of templates chapter here</p>
        -->

        <accordion close-others="true">
            <accordion-group heading="Template Hierarchy" is-open="Volume2Modes.TemplateHierarchy">
                <div class="alert alert-info">Use the + and - to expand/collapse the hierarchy of the templates.</div>
                <div class="col-md-12 template-hierarchy">
                    <ul>
                        <li ng-repeat="template in TemplateHierarchy[0].Templates | orderBy: 'Name'" ng-include="'template_hierarchy.html'"></li>
                    </ul>
                </div>
            </accordion-group>
            <accordion-group heading="Template List" is-open="Volume2Modes.TemplateList">
                <div class="table-responsive template-type-filter">
                    <table class="table table-filter">
                        <tbody>
                            <tr>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('')">ALL</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('a')">a</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('b')">b</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('c')">c</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('d')">d</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('e')">e</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('f')">f</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('g')">g</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('h')">h</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('i')">i</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('j')">j</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('k')">k</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('l')">l</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('m')">m</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('n')">n</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('o')">o</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('p')">p</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('q')">q</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('r')">r</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('s')">s</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('t')">t</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('u')">u</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('v')">v</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('w')">w</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('x')">x</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('y')">y</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('z')">z</a></td>
                                <td><a href="#{{GetPath()}}" ng-click="SetVolume2Filter('0-9')">#</a></td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div class="container-fluid">
                    <div class="row">
                        <div class="col-md-4 template-type" ng-repeat="tt in Model.TemplateTypes">
                            <div class="panel panel-default">
                                <div class="panel-heading">{{tt.Name}}</div>
                                <div class="panel-body" style="max-height: 350px; overflow-y: auto;">
                                    <ul>
                                        <li ng-repeat="t in GetTemplatesByType(tt.TemplateTypeId)"><a href="#/volume2/{{t.Bookmark}}">{{t.Name}} <span class="label label-default">{{t.Identifier}}</span></a></li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </accordion-group>
        </accordion>
    </script>
    
    <script type="text/html" id="template_hierarchy.html">
        <span class="glyphicon" ng-class="{ 'glyphicon glyphicon-plus': !template.Expanded, 'glyphicon glyphicon-minus': template.Expanded }" ng-if="template.Children.length > 0" ng-click="template.Expanded = !template.Expanded"></span>
        <a href="#/volume2/{{template.Bookmark}}" ng-class="{ 'no-children': template.Children.length == 0 }">{{template.Name}} <span class="label label-default">{{template.Identifier}}</a></span>
        <ul ng-if="template.Children.length > 0 && template.Expanded">
            <li ng-repeat="template in template.Children | orderBy: 'Name'" ng-include="'template_hierarchy.html'"></li>
        </ul>
    </script>

    <script type="text/html" id="search.html">
        <h1 class="page-header">Search Results</h1>

        <h2>Templates</h2>
        <div class="search-template" ng-repeat="srt in SearchResults.Templates | orderBy: 'Priority' | orderBy: 'Name'">
            <p><strong><a href="#/volume2/{{srt.Template.Bookmark}}">{{srt.Name}} ({{srt.Template.Identifier}})</a></strong></p>
            <p class="search-description" ng-bind-html="GetHtml(srt.Template.Description)"></p>
        </div>

        <h2>Constraints</h2>
        <p ng-repeat="srt in SearchResults.Constraints">
            {{srt.Template.Name}} ({{srt.Template.Identifier}})<br />
            <a href="#/volume2/{{srt.Template.Bookmark}}/{{srt.Constraint.Number}}" ng-bind-html="GetHtml(srt.Constraint.Narrative)"></a>
        </p>

        <h2>Value Sets</h2>
        <p ng-repeat="srt in SearchResults.ValueSets | orderBy: 'Priority' | orderBy: 'Name'">
            <a href="#/valuesets#{{srt.ValueSet.Identifier}}">{{srt.Name}} ({{srt.ValueSet.Identifier}})</a>
        </p>

        <h2>Code Systems</h2>
        <p ng-repeat="srt in SearchResults.CodeSystems | orderBy: 'Priority' | orderBy: 'Name'">
            <a href="#/codesystems#{{srt.CodeSystem.Identifier}}">{{srt.Name}} ({{srt.CodeSystem.Identifier}})</a>
        </p>
    </script>

    <script type="text/html" id="overview.html">
        <!-- TODO: Separate field to capture display name -->
        <h2 class="page-header">Welcome to the Web-based Implementation Guide for {{Model.ImplementationGuideDisplayName}}</h2>

        <p ng-bind-html="GetHtml(Model.ImplementationGuideDescription)"></p>
        <p>The document types contained in this IG can be validated using the CDA Validator:</p>
        <div>
            <a href="https://www.lantanagroup.com/validator" target="_blank">https://www.lantanagroup.com/validator</a>
        </div>

        <h3>Table of Contents</h3>
        <h4><span class="glyphicon glyphicon-book"></span> <a href="#/volume1">Overview</a></h4>
        <h4><span class="glyphicon glyphicon-list-alt"></span> <a href="#/volume2">Templates</a></h4>
        <h4><span class="glyphicon glyphicon-list"></span> <a href="#/valuesets">Value sets in this guide</a></h4>
        <h4><span class="glyphicon glyphicon-list"></span> <a href="#/codesystems">Code systems in this guide</a></h4>
    </script>

    <script type="text/html" id="volume1.html">
        <h1 class="page-header">Overview</h1>
        <div ng-if="Model.Volume1Html">
            <div ng-bind-html="GetHtml(Model.Volume1Html)"></div>
        </div>
        <div class="volume1-content" ng-if="Model.Volume1Sections.length > 0" numbered-heading="Model.Volume1Sections" fix-html-content="Model.Volume1Sections">
            <h2 class="nocount">Table of Contents</h2>
            <ol class="table-of-contents" />

            <hr />

            <div ng-repeat="s in Model.Volume1Sections">
                <div class="animate-switch-container" ng-switch on="s.Level">
                    <h2 ng-switch-when="2" id="section{{$index+1}}">{{s.Heading}}</h2>
                    <h3 ng-switch-when="3" id="section{{$index+1}}">{{s.Heading}}</h3>
                    <h4 ng-switch-when="4" id="section{{$index+1}}">{{s.Heading}}</h4>
                    <h5 ng-switch-when="5" id="section{{$index+1}}">{{s.Heading}}</h5>
                    <h6 ng-switch-when="6" id="section{{$index+1}}">{{s.Heading}}</h6>
                    <h1 ng-switch-default id="section{{$index+1}}">{{s.Heading}}</h1>
                </div>
                <div ng-bind-html="GetHtml(s.Content)"></div>
            </div>
        </div>
    </script>

    <script type="text/html" id="template_relationships.html">
        <div ng-if="Template.ContainedTemplates.length > 0">
            <u>Contains</u>
            <ul>
                <li ng-repeat="tr in Template.ContainedTemplates | orderBy: 'Name'"><a href="#/volume2/{{tr.Bookmark}}">{{tr.Name}} ({{tr.Identifier}})</a></li>
            </ul>
        </div>
                            
        <div ng-if="Template.ContainedByTemplates.length > 0">
            <u>Contained By</u>
            <ul>
                <li ng-repeat="tr in Template.ContainedByTemplates | orderBy: 'Name'"><a href="#/volume2/{{tr.Bookmark}}">{{tr.Name}} ({{tr.Identifier}})</a></li>
            </ul>
        </div>    

        <div ng-if="Template.ImpliedTemplate">
            <u>Implies</u>
            <ul>
                <li><a href="#/volume2/{{Template.ImpliedTemplate.Bookmark}}">{{Template.ImpliedTemplate.Name}} ({{Template.ImpliedTemplate.Identifier}})</a></li>
            </ul>
        </div>
                                
        <div ng-if="Template.ImplyingTemplates.length > 0">
            <u>Implied By</u>
            <ul>
                <li ng-repeat="tr in Template.ImplyingTemplates | orderBy: 'Name'"><a href="#/volume2/{{tr.Bookmark}}">{{tr.Name}} ({{tr.Identifier}})</a></li>
            </ul>
        </div>
    </script>

    <script type="text/html" id="template_constraint_table.html">
        <table class="table table-striped">
            <thead>
                <th>XPath</th>
                <th>Card.</th>
                <th>Verb</th>
                <th>Data Type</th>
                <th>CONF #</th>
                <th>Value</th>
            </thead>
            <tbody>
                <tr ng-if="Template.ImpliedTemplate">
                    <td colspan="6">
                        Conforms to <a href="#/volume2/{{Template.ImpliedTemplate.Bookmark}}">{{Template.ImpliedTemplate.Name}} (identifier: {{Template.ImpliedTemplate.Identifier}})</a>
                    </td>
                </tr>
                <tr ng-repeat="tc in CurrentTableConstraints">
                    <td>
                        <span style="white-space: pre">{{tc.Level}}</span>
                        <span>{{tc.Constraint.Context}}</span>
                    </td>
                    <td>{{tc.Constraint.Cardinality}}</td>
                    <td>{{tc.Constraint.Conformance}}</td>
                    <td>{{tc.Constraint.DataType}}</td>
                    <td>{{tc.Constraint.Number}}</td>
                    <td ng-bind-html="GetConstraintTableValue(tc.Constraint)"></td>
                </tr>
            </tbody>
        </table>
    </script>

    <script type="text/html" id="template_narrative.html">
        <ol>
            <li ng-if="Template.ImpliedTemplate">Conforms to <a href="#/volume2/{{Template.ImpliedTemplate.Bookmark}}">{{Template.ImpliedTemplate.Name}} (identifier: {{Template.ImpliedTemplate.Identifier}})</a></li>
            <li ng-repeat="c in Template.Constraints" ng-include="'narr_constraint_render.html'"></li>
        </ol>
    </script>

    <script type="text/html" id="inline_constraint_change.html">
        <div class="row" ng-attr-title="{{ $parent.GetChangeTooltip(c.ChangeType) }}">
            <div class="col-md-12">
                <span>{{ $index + 1 }}</span>. <span ng-class="{ 'constraintAdded': c.ChangeType == 0, 'constraintModified': c.ChangeType == 1, 'constraintRemoved': c.ChangeType == 2 }">{{ c.Narrative }}</span>
            </div>
        </div>

        <div style="padding-left: 40px" ng-repeat="c in c.Constraints" ng-include="'inline_constraint_change.html'"></div>
    </script>

    <script type="text/html" id="template_changes.html">
        <p><b>From: </b> {{ Template.Changes.PreviousTemplateName }}</p>

        <div class="input-group">
            <input type="radio" name="ViewChangesInline" value="Inline" ng-model="ViewTemplateChangesMode" /> Inline
            <input type="radio" name="ViewChangesInline" value="List" ng-model="ViewTemplateChangesMode" /> List
        </div>

        <div class="well" ng-if="ViewTemplateChangesMode == 'Inline'">
            <div ng-repeat="c in Template.Changes.InlineConstraints" ng-include="'inline_constraint_change.html'"></div>
        </div>

        <div class="well" ng-if="ViewTemplateChangesMode == 'List'">
            <h4>Template Changes</h4>
            <div class="container-fluid">
                <div class="row">
                    <div class="col-md-2"><strong>Field</strong></div>
                    <div class="col-md-5"><strong>Old</strong></div>
                    <div class="col-md-5"><strong>New</strong></div>
                </div>
                <div class="row" ng-repeat="cf in Template.Changes.Difference.ChangedFields">
                    <div class="col-md-2">{{ cf.Name }}</div>
                    <div class="col-md-5">{{ cf.Old }}</div>
                    <div class="col-md-5">{{ cf.New }}</div>
                </div>
            </div>

            <h4>Constraint Changes</h4>
            <div class="container-fluid">
                <div class="row">
                    <div class="col-md-2"><strong>Change</strong></div>
                    <div class="col-md-2"><strong>Number</strong></div>
                    <div class="col-md-4"><strong>Old Narrative</strong></div>
                    <div class="col-md-4"><strong>New Narrative</strong></div>
                </div>
                <div class="row" ng-repeat="cc in Template.Changes.Difference.ChangedConstraints"  ng-show="cc.Type !== 3">
                    <div class="col-md-2">{{ $parent.GetChangeTooltip(cc.Type) }}</div>
                    <div class="col-md-2">{{ cc.Number }}</div>
                    <div class="col-md-4">{{ cc.OldNarrative }}</div>
                    <div class="col-md-4">{{ cc.NewNarrative }}</div>
                </div>
            </div>
        </div>
    </script>

    <script type="text/html" id="template_samples.html">
        <div class="panel panel-default" ng-repeat="s in Template.Samples">
            <div class="panel-heading">
                <h4 class="panel-title">
                    <a data-toggle="collapse" href="#{{GetPath()}}#sample{{s.Id}}">
                    {{s.Name}}
                    </a>
                </h4>
            </div>
            <div class="panel-body panel-collapse collapse" id="sample{{s.Id}}">
                <div hljs source="s.SampleText"></div>
            </div>
        </div>
    </script>

    <script type="text/html" id="template_valuesets.html">
        <div ng-repeat="vs in CurrentValueSets | orderBy: 'Name'">
            <a id="{{GetValueSetAnchor(vs)}}"></a>
            <div class="panel panel-default">
                <div class="panel-heading" ng-if="!Options.TemplateTabs">
                    <a data-toggle="collapse" href="#{{GetPath()}}#vs_{{GetElementIdentifier(vs.Identifier)}}">
                        {{vs.Name}} {{vs.Identifier}}
                    </a>
                </div>
                <div class="panel-heading" ng-if="Options.TemplateTabs">
                    {{vs.Name}} {{vs.Identifier}}
                </div>
                <div class="table-responsive" ng-class="{ collapse: !Options.TemplateTabs }" id="vs_{{GetElementIdentifier(vs.Identifier)}}">
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <td colspan="4">
                                    <p><strong>Binding Date:</strong> {{vs.BindingDate}}</p>
                                    <p>{{vs.Description}}</p>
                                    <p ng-if="vs.Source">
                                        <a href="{{vs.Source}}" target="_blank">{{vs.Source}}</a>
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <th>Code</th>
                                <th>Code System</th>
                                <th>Code System Identifier</th>
                                <th>Display</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr ng-repeat="vsm in vs.Members">
                                <td>{{vsm.Code}}</td>
                                <td>{{vsm.CodeSystemName}}</td>
                                <td>{{vsm.CodeSystemIdentifier}}</td>
                                <td>{{vsm.DisplayName}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </script>

    <script type="text/html" id="narr_constraint_render.html">
        <div class="constraint" ng-bind-html="GetHtml(c.Narrative)"></div>
        <ol>
            <li ng-repeat="c in c.Constraints" ng-include="'narr_constraint_render.html'"></li>
        </ol>
    </script>

    <script type="text/html" id="options_modal.html">
        <div class="modal-header">
            <h3 class="modal-title">Options</h3>
        </div>
        <div class="modal-body">
            <div class="form-group" ng-if="AllowDataChanges">
                <label>Include inferred?</label>
                <select class="form-control" ng-model="Options.Inferred" ng-options="o.v as o.n for o in [{ n: 'Yes', v: true }, { n: 'No', v: false }]">
                </select>
            </div>
            <div class="form-group" ng-if="AllowDataChanges">
                <label>Parent templates</label>
                <select class="form-control" ng-repeat="pt in Options.ParentTemplates" ng-options="t.Id as t.Name for t in Templates" ng-model="Options.ParentTemplates[$index]">
                    <option value="">SELECT</option>
                </select>
            </div>
            <div class="form-group">
                <label>Use tabs when viewing templates?</label>
                <select class="form-control" ng-model="Options.TemplateTabs" ng-options="o.v as o.n for o in [{ n: 'Yes', v: true }, { n: 'No', v: false }]">
                </select>
            </div>
        </div>
        <div class="modal-footer">
            <button class="btn btn-primary" ng-click="Ok()">OK</button>
            <button class="btn btn-warning" ng-click="Cancel()">Cancel</button>
        </div>
    </script>
</body>
</html>
