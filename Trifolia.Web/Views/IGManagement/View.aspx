<%@ Import Namespace="System.Linq" %>
<%@ Page Title="View Implementation Guide" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #templates .panel-body {
            padding-left: 25px;
            padding-top: 5px;
            padding-bottom: 15px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ViewImplementationGuide">
        <div class="navbar navbar-default" role="navigation" data-bind="with: Model">
            <div class="container-fluid">
                <!-- Brand and toggle get grouped for better mobile display -->
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
                    <a class="navbar-brand" href="#" data-bind="text: Name">Brand</a>
                </div>

                <!-- Collect the nav links, forms, and other content for toggling -->
                <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                    <ul class="nav navbar-nav">
                        <li data-bind="if: ViewWebIG()" title="View dynamically generated Web IG (based on current data)">
                            <a data-bind="attr: { href: '/IG/View/' + Id() }" target="_blank">Web IG</a>
                        </li>
                        <li class="dropdown" data-bind="if: ShowEditIG() || ShowEditBookmarks() || ShowManageFiles()">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">Edit <b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li data-bind="if: ShowEditIG()"><a data-bind="attr: { href: '/IGManagement/Edit/' + Id() }">Implementation Guide</a></li>
                                <li data-bind="if: ShowEditBookmarks()"><a data-bind="attr: { href: '/IGManagement/EditBookmarks?implementationGuideId=' + Id() }">Bookmarks</a></li>
                                <li data-bind="if: ShowManageFiles()"><a data-bind="attr: { href: '/IGManagement/Files?implementationGuideId=' + Id() }">Files</a></li>
                            </ul>
                        </li>
                        <li class="dropdown" data-bind="if: ShowExportMSWord() || ShowExportSchematron() || ShowExportXML() || ShowExportVocabulary() || ShowExportGreen()">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">Export <b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li data-bind="if: ShowExportMSWord()"><a data-bind="attr: { href: '/Export/MSWord?implementationGuideId=' + Id() }">MS Word</a></li>
                                <li data-bind="if: ShowExportSchematron()"><a data-bind="attr: { href: '/Export/Schematron?implementationGuideId=' + Id() }">Schematron</a></li>
                                <li data-bind="if: ShowExportXML()"><a data-bind="attr: { href: '/Export/XML?implementationGuideId=' + Id() }">XML/JSON</a></li>
                                <li data-bind="if: ShowExportVocabulary()"><a data-bind="attr: { href: '/Export/Vocabulary?implementationGuideId=' + Id() }">Vocabulary</a></li>
                                <li data-bind="if: ShowExportGreen()"><a data-bind="attr: { href: '/Export/Green?implementationGuideId=' + Id() }">Green Artifacts</a></li>
                            </ul>
                        </li>
                        <li class="dropdown" data-bind="if: ShowPublish() && Status()">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown">Workflow <b class="caret"></b></a>
                            <ul class="dropdown-menu">
                                <li data-bind="if: Status() == 'Draft'"><a href="#" data-bind="click: $parent.Ballot">Ballot</a></li>
                                <li data-bind="if: Status() == 'Draft' || Status() == 'Ballot'"><a href="#" data-bind="click: $parent.ShowPublishDialog">Publish</a></li>
                                <li data-bind="if: Status() == 'Ballot'"><a href="#" data-bind="click: $parent.Draft">Draft</a></li>
                                <li data-bind="if: Status() == 'Published'"><a href="#" data-bind="click: $parent.Unpublish">Unpublish</a></li>
                            </ul>
                        </li>
                        <li data-bind="if: ShowNewVersion(), css: { disabled: !EnableNewVersion() }"><a href="#" data-bind="click: function() { if (EnableNewVersion()) { $parent.NewVersion(); } }, attr: { title: NewVersionTooltip }">New Version</a></li>
                        <li data-bind="if: ShowDelete()"><a data-bind="attr: { href: '/IGManagement/Delete/' + Id() }">Delete</a></li>
                    </ul>
                </div>
                <!-- /.navbar-collapse -->
            </div>
            <!-- /.container-fluid -->
        </div>

        <div class="panel panel-default" data-bind="with: Model">
            <div class="panel-heading">
                <h2 data-bind="text: ComputedName"></h2>
                
                <!-- ko if: PreviousVersionImplementationGuideId -->
                <span class="label label-success" title="Previous Version"><i class="glyphicon glyphicon-share"></i> <a data-bind="attr: { href: '/IGManagement/View/' + PreviousVersionImplementationGuideId() }">Previous Version</a></span>&nbsp;
                <!-- /ko -->
                
                <!-- ko if: NextVersionImplementationGuideId -->
                <span class="label label-success" title="Next Version"><i class="glyphicon glyphicon-share"></i> <a data-bind="attr: { href: '/IGManagement/View/' + NextVersionImplementationGuideId() }">Next Version</a></span>
                <!-- /ko -->

                <span class="label label-primary" data-bind="text: Type" title="Implementation Guide Type"></span> 

                <span class="label label-info" data-bind="text: Organization" title="Organization"></span> 

                <!-- ko if: Status() -->
                <span class="label label-primary" title="Status">
                    <span data-bind="text: Status"></span>
                    <!-- ko if: PublishDate -->
                    : <span data-bind="text: PublishDate"></span>
                    <!-- /ko -->
                </span>
                <!-- /ko -->
            </div>
        </div>

        <ul class="nav nav-tabs" data-bind="with: Model">
            <li class="active"><a href="#templates" data-toggle="tab">Templates/Profiles <span data-bind="helpTooltip: {}" title="Templates/profiles that appear in this implementation guide are based on an algorithm that includes both implied templates/profiles and contained templates/profiles regardless of whether or not that are directly contained within this implementation guide."></span></a></li>
            <!-- ko if: ViewNotes() -->
            <li><a href="#notes" data-toggle="tab">Notes</a></li>
            <!-- /ko -->
            <!-- ko if: ViewPrimitives() -->
            <li><a href="#primitives" data-toggle="tab">Primitives</a></li>
            <!-- /ko -->
            <!-- ko if: ViewAuditTrail() -->
            <li><a href="#auditTrail" data-toggle="tab">Audit Trail</a></li>
            <!-- /ko -->
            <!-- ko if: ViewFiles() -->
            <li><a href="#files" data-toggle="tab">Files</a></li>
            <!-- /ko -->
            <!-- ko if: ViewWebIG() && WebPublications().length > 0 -->
            <li><a href="#webPublications" data-toggle="tab">Web Publications</a></li>
            <!-- /ko -->
            <!-- ko if: $root.FhirXml() || $root.FhirJson() -->
            <li><a href="#fhirXml" data-toggle="tab">FHIR XML</a></li>
            <li><a href="#fhirJson" data-toggle="tab">FHIR JSON</a></li>
            <!-- /ko -->
        </ul>

        <div class="tab-content">
            <!-- Templates -->
            <div class="tab-pane active" id="templates" data-bind="with: Model">
                <div class="input-group" style="padding-bottom: 10px;">
                    <div class="input-group-addon">Search</div>
                    <input id="searchTemplateQuery" type="text" class="form-control" data-bind="value: $parent.SearchTemplateQuery, valueUpdate: 'keyup'" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default" data-bind="click: function () { $parent.SearchTemplateQuery(''); }"><i class="glyphicon glyphicon-remove"></i></button>
                    </div>
                </div>

                <!-- ko foreach: $parent.GetTemplateTypes() -->
                <div class="panel panel-default">
                    <div class="panel-heading">
                        <div class="row" style="line-height: 28px;">
                            <div class="col-md-10">
                                <span data-bind="text: Name"></span>
                            </div>
                            <div class="col-md-2">
                                <div class="pull-right">
                                    <!-- ko if: !IsExpanded() -->
                                    <button type="button" class="btn btn-default btn-sm" title="Expand" data-bind="click: function () { IsExpanded(true); }">
                                        <i class="glyphicon glyphicon-chevron-down"></i>
                                    </button>
                                    <!-- /ko -->
                                    <!-- ko if: IsExpanded() -->
                                    <button type="button" class="btn btn-default btn-sm" title="Collapse" data-bind="click: function () { IsExpanded(false); }">
                                        <i class="glyphicon glyphicon-chevron-up"></i>
                                    </button>
                                    <!-- /ko -->
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- ko if: IsExpanded() -->
                    <div class="panel-body">
                        <p data-bind="text: Description"></p>

                        <!-- ko foreach: $parents[1].GetTemplates($data) -->
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <div class="row" style="line-height: 28px;">
                                    <div class="col-md-10">
                                        <span data-bind="text: Name"></span>
                                        <span class="label label-primary" data-bind="text: Oid"></span>
                                        <span class="label label-primary" data-bind="text: Status"></span>
                                    </div>
                                    <div class="col-md-2">
                                        <div class="pull-right">
                                            <a class="btn btn-default btn-sm" data-bind="attr: { href: getTemplateViewUrl(Id(), Oid()) }" title="View Template/Profile"><i class="glyphicon glyphicon-search"></i></a>

                                            <!-- ko if: CanEdit() -->
                                            <a class="btn btn-default btn-sm" data-bind="attr: { href: getTemplateEditUrl(Id(), Oid()) }" title="Edit Template/Profile"><i class="glyphicon glyphicon-edit"></i></a>
                                            <!-- /ko -->

                                            <!-- ko if: !IsExpanded() -->
                                            <button type="button" class="btn btn-default btn-sm" title="Expand" data-bind="click: function () { IsExpanded(true); }">
                                                <i class="glyphicon glyphicon-chevron-down"></i>
                                            </button>
                                            <!-- /ko -->
                                            <!-- ko if: IsExpanded() -->
                                            <button type="button" class="btn btn-default btn-sm" title="Collapse" data-bind="click: function () { IsExpanded(false); }">
                                                <i class="glyphicon glyphicon-chevron-up"></i>
                                            </button>
                                            <!-- /ko -->
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-body" data-bind="visible: IsExpanded">
                                <p data-bind="text: Description"></p>
                            </div>
                        </div>
                        <!-- /ko -->
                    </div>
                    <!-- /ko -->
                </div>
                <!-- /ko -->
            </div>

            <!-- Notes -->
            <!-- ko if: Model() && Model().ViewNotes() -->
            <div class="tab-pane" id="notes">
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Type</th>
                            <th>Item</th>
                            <th>Note</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- ko foreach: Notes -->
                        <tr>
                            <td data-bind="text: Type"></td>
                            <td>
                                <!-- ko if: Type() == 'Template' -->
                                <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + TemplateId() }, text: TemplateName"></a>
                                <!-- /ko -->
                                <!-- ko if: Type() == 'Constraint' -->
                                <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + TemplateId() }, text: Number"></a>
                                <!-- /ko -->
                            </td>
                            <td data-bind="text: Note">
                            </td>
                        </tr>
                        <!-- /ko -->
                        <!-- ko if: Notes().length == 0 -->
                        <tr>
                            <td colspan="3">No notes</td>
                        </tr>
                        <!-- /ko -->
                    </tbody>
                </table>
            </div>
            <!-- /ko -->

            <!-- Primitives -->
            <!-- ko if: Model() && Model().ViewPrimitives -->
            <div class="tab-pane" id="primitives">
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Template/Profile</th>
                            <th>Constraint</th>
                            <th>Primitive</th>
                            <th>Schematron</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- ko foreach: Primitives() -->
                        <tr>
                            <td>
                                <a data-bind="attr: { href: '/TemplateManagement/View/Id/' + TemplateId() }, text: TemplateName"></a>
                            </td>
                            <td data-bind="text: Number"></td>
                            <td data-bind="text: PrimitiveText"></td>
                            <td>
                                <span data-bind="text: Schematron"></span>
                            </td>
                        </tr>
                        <!-- /ko -->
                        <!-- ko if: Primitives().length == 0 -->
                        <tr>
                            <td colspan="4">No primitives</td>
                        </tr>
                        <!-- /ko -->
                    </tbody>
                </table>
            </div>
            <!-- /ko -->

            <!-- Audit Trail -->
            <!-- ko if: Model() && Model().ViewAuditTrail -->
            <div class="tab-pane" id="auditTrail">
                <div class="input-group date" data-bind="date: AuditTrailStartDate">
                    <div class="input-group-addon">Start Date</div>
                    <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                    </div>
                </div>

                <div class="input-group date" data-bind="date: AuditTrailEndDate" style="padding-top: 5px;servi">
                    <div class="input-group-addon">End Date</div>
                    <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                    </div>
                </div>

                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Who</th>
                            <th>When</th>
                            <th>What</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- ko foreach: AuditTrail() -->
                        <tr>
                            <td data-bind="text: Username, attr: { title: IP }"></td>
                            <td data-bind="text: AuditDate"></td>
                            <!-- ko if: Type == 'ImplementationGuide' -->
                            <td data-bind="text: ImplementationGuideId"></td>
                            <!-- /ko -->
                            <!-- ko if: Type == 'Template' -->
                            <td data-bind="text: TemplateName"></td>
                            <!-- /ko -->
                            <!-- ko if: Type == 'TemplateConstraint' -->
                            <td data-bind="text: ConformanceNumber"></td>
                            <!-- /ko -->
                        </tr>
                        <tr>
                            <td colspan="3">
                                <small data-bind="text: Note"></small>
                            </td>
                        </tr>
                        <!-- /ko -->
                        <!-- ko if: AuditTrail().length == 0 -->
                        <tr>
                            <td colspan="3">No audit entries</td>
                        </tr>
                        <!-- /ko -->
                    </tbody>
                </table>
            </div>
            <!-- /ko -->

            <!-- Files -->
            <!-- ko if: Model() && Model().ViewFiles() -->
            <div class="tab-pane" id="files">
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Type</th>
                            <th>Date</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- ko foreach: Files() -->
                        <tr>
                            <td>
                                <a data-bind="attr: { href: '/IGManagement/Files/Download?versionId=' + VersionId() }, text: Name"></a>
                            </td>
                            <td data-bind="text: Type"></td>
                            <td data-bind="text: Date"></td>
                            <td>
                                <span data-bind="text: Description"></span>
                            </td>
                        </tr>
                        <!-- /ko -->
                        <!-- ko if: Files().length == 0 -->
                        <tr>
                            <td colspan="4">No files</td>
                        </tr>
                        <!-- /ko -->
                    </tbody>
                </table>
            </div>
            <!-- /ko -->

            <!-- Web Publications -->
            <!-- ko if: Model() && Model().WebPublications().length > 0 -->
            <div class="tab-pane" id="webPublications">
                <ul data-bind="foreach: Model().WebPublications">
                    <li><a target="_blank" data-bind="attr: { href: $data }, text: $data"></a></li>
                </ul>
            </div>
            <!-- /ko -->

            <!-- FHIR XML -->
            <!-- ko if: FhirXml() -->
            <div class="tab-pane" id="fhirXml">
                <pre data-bind="text: FhirXml"></pre>
            </div>
            <!-- /ko -->

            <!-- FHIR JSON -->
            <!-- ko if: FhirJson() -->
            <div class="tab-pane" id="fhirJson">
                <pre data-bind="text: FhirJson"></pre>
            </div>
            <!-- /ko -->
        </div>

        <div id="publishDateSelector" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Select publish date</h4>
                    </div>
                    <div class="modal-body">
                        <div class="input-group date" data-bind="date: NewPublishDate">
                            <div class="input-group-addon">Publish Date</div>
                            <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default btn-clear" data-bind="click: function () { NewPublishDate(''); }"><i class="glyphicon glyphicon-remove"></i></button>
                                <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: Publish">OK</button>
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>
    
    <script src="/Scripts/lib/vkbeautify.0.99.00.beta.js"></script>
    <script type="text/javascript" src="../../Scripts/IGManagement/View.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function() {
            viewModel = new ViewImplementationGuideViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $("#ViewImplementationGuide")[0]);

            $("#publishDateSelector").modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
        });
    </script>

</asp:Content>