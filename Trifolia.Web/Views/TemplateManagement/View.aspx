<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
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

        #samples .panel-heading a:after {
            font-family: 'Glyphicons Halflings';
            content: "\e114";    
            float: right; 
            color: grey; 
        }

        #samples .panel-heading a.collapsed:after {
           content: "\e080";
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="ViewTemplate" style="position: relative; width: 100%" data-bind="if: Initialized()">
        
        <!-- Navbar -->
        <div class="navbar navbar-default" role="navigation">
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
                        <!-- ko foreach: Actions -->
                        <li data-bind="css: { 'disabled': Disabled }"><a data-bind="    text: Text, attr: { href: (Disabled() ? '#' : Url()) }, tooltip: ToolTip"></a></li>
                        <!-- /ko -->
                    </ul>
                </div>
                <!-- /.navbar-collapse -->
            </div>
            <!-- /.container-fluid -->
        </div>

        <div class="panel panel-default">
            <div class="panel-heading">
                <!-- OID -->
                <h4 data-bind="text: Oid"></h4>
                
                <!-- ko if: HasPreviousVersion() -->
                <span class="label label-success" title="Previous Version"><i class="glyphicon glyphicon-share"></i> <a data-bind="attr: { href: PreviousVersionUrl }, text: PreviousVersionTemplateName"></a></span>
                <!-- /ko -->
        
                <!-- Implied Template -->
                <!-- ko if: ImpliedTemplateId() -->
                <span class="label label-success" title="Implied Template/Profile"><i class="glyphicon glyphicon-share"></i> 
                    <a data-bind="attr: { href: ImpliedTemplateUrl }">
                        <span data-bind="text: ImpliedTemplate"></span> - <span data-bind="text: ImpliedTemplateOid"></span>
                    </a>
                </span>
                <span style="white-space: pre"> </span>
                <!-- /ko -->

                <!-- Implementation Guide -->
                <span class="label label-primary" title="Implementation Guide"><i class="glyphicon glyphicon-share"></i> <a data-bind="attr: { href: ImplementationGuideUrl }, text: ImplementationGuide"></a></span>

                <!-- Status -->
                <!-- ko if: Status() -->
                <span class="label label-primary" title="Status" data-bind="text: Status"></span> 
                <!-- /ko -->

                <!-- Type -->
                <span class="label label-primary" title="Template/Profile Type" data-bind="text: Type"></span>

                <!-- Extensibility -->
                <span class="label label-primary" title="Extensibility" data-bind="text: Extensibility"></span>

                <!-- Organization -->
                <span class="label label-info" title="Organization" data-bind="text: Organization"></span>

                <!-- Author -->
                <span class="label label-info" title="Author" data-bind="text: Author"></span>
        
                <!-- ko if: HasGreenModel() -->
                <span class="label label-info">Green Model</span>
                <!-- /ko -->

                <div title="Template/Profile Description" class="wiki-markup" data-bind="html: Description"></div>

                <!-- ko if: ShowNotes() && Notes() -->
                <h4>Notes</h4>
                <p data-bind="html: Notes"></p>
                <!-- /ko -->
            </div>
        </div>

        <!-- Nav tabs -->
        <ul class="nav nav-tabs">
            <li class="active"><a href="#constraints" data-toggle="tab">Constraints</a></li>
            <!-- ko if: Samples().length > 0 -->
            <li><a href="#samples" data-toggle="tab">Samples</a></li>
            <!-- /ko -->
            <li><a href="#references" data-toggle="tab">Relationships</a></li>
            <!-- ko if: Changes() -->
            <li><a href="#changes" data-toggle="tab">Changes</a></li>
            <!-- /ko -->
            <!-- ko if: StructureDefinitionJSON() -->
            <li><a href="#strucdefjson" data-toggle="tab">StructureDefinition JSON</a></li>
            <!-- /ko -->
            <!-- ko if: StructureDefinitionXML() -->
            <li><a href="#strucdefxml" data-toggle="tab">StructureDefinition XML</a></li>
            <!-- /ko -->
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
            <div class="tab-pane active" id="constraints">
                <div data-bind="template: { name: 'templateConstraint', data: $data.Constraints() }"></div>
            </div>

            <!-- ko if: Samples().length > 0 -->
            <div class="tab-pane" id="samples" data-bind="foreach: Samples">
                <div class="panel-group">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <span data-bind="text: Name"></span>
                                <a data-toggle="collapse" data-bind="attr: { 'data-target': '#' + Id(), href: '#' + Id() }"></a>
                            </h4>
                        </div>
                        <div class="panel-collapse collapse in" data-bind="attr: { id: Id }">
                            <div class="panel-body" style="white-space: pre;" data-bind="text: Sample"></div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- /ko -->

            <div class="tab-pane" id="references">
                <div data-bind="if: ImpliedTemplateId">
                    <h4>Implied Template/Profile</h4>
                    <ul>
                        <li>
                            <a data-bind="attr: { href: getTemplateViewUrl(ImpliedTemplateId(), ImpliedTemplateOid()) }"><span data-bind="text: ImpliedTemplate"></span> (<span data-bind="text: ImpliedTemplateOid"></span>)</a>
                            <br /><span data-bind="text: ImpliedTemplateImplementationGuide"></span>
                        </li>
                    </ul>
                </div>

                <div data-bind="if: ImplyingTemplates().length > 0">
                    <h4>Implying Templates <span title="Templates/Profiles that imply the template/profile you are viewing now." data-bind="helpTooltip: {}"></span></h4>
                    <ul data-bind="foreach: ImplyingTemplates">
                        <li>
                            <a data-bind="attr: { href: getTemplateViewUrl(Id(), Oid()) }"><span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)</a>
                            <br /><span data-bind="text: ImplementationGuide"></span>
                        </li>
                    </ul>
                </div>

                <div data-bind="if: ContainedTemplates().length > 0">
                    <h4>Contained Templates/Profiles <span title="Templates/Profiles that contain the template/profile you are viewing via constraints" data-bind="helpTooltip: {}"></span></h4>
                    <ul data-bind="foreach: ContainedTemplates">
                        <li>
                            <a data-bind="attr: { href: getTemplateViewUrl(Id(), Oid()) }"><span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)</a>
                            <br /><span data-bind="text: ImplementationGuide"></span>
                        </li>
                    </ul>
                </div>

                <div data-bind="if: ContainedByTemplates().length > 0">
                    <h4>Contained By <span title="Templates/Profiles that contain the template/profile you are viewing via constraints" data-bind="helpTooltip: {}"></span></h4>
                    <ul data-bind="foreach: ContainedByTemplates">
                        <li>
                            <a data-bind="attr: { href: getTemplateViewUrl(Id(), Oid()) }"><span data-bind="text: Name"></span> (<span data-bind="text: Oid"></span>)</a>
                            <br /><span data-bind="text: ImplementationGuide"></span>
                        </li>
                    </ul>
                </div>
            </div>

            <!-- ko with: Changes -->
            <div class="tab-pane" id="changes">
                <p><b>From: </b> <span data-bind="text: PreviousTemplateName"></span></p>

                <div class="input-group">
                    <input type="radio" name="ViewChangesInline" value="Inline" data-bind="checked: $parent.ViewChangesMode" /> Inline
                    <input type="radio" name="ViewChangesInline" value="List" data-bind="checked: $parent.ViewChangesMode" /> List
                </div>

                <!-- ko if: $parent.ViewChangesMode() == 'Inline' -->
                <div class="well">
                    <div data-bind="template: { name: 'inlineConstraintChange', data: InlineConstraints() }"></div>
                </div>
                <!-- /ko -->

                <!-- ko if: $parent.ViewChangesMode() == 'List' -->
                <div class="well">
                    <h4>Template/Profile Changes</h4>
                    <div class="container-fluid">
                        <div class="row">
                            <div class="col-md-2"><strong>Field</strong></div>
                            <div class="col-md-5"><strong>Old</strong></div>
                            <div class="col-md-5"><strong>New</strong></div>
                        </div>
                        <!-- ko foreach: Difference.ChangedFields -->
                        <div class="row">
                            <div class="col-md-2" data-bind="text: Name"></div>
                            <div class="col-md-5" data-bind="text: Old"></div>
                            <div class="col-md-5" data-bind="text: New"></div>
                        </div>
                        <!-- /ko -->
                    </div>

                    <h4>Constraint Changes</h4>
                    <div class="container-fluid">
                        <div class="row">
                            <div class="col-md-2"><strong>Change</strong></div>
                            <div class="col-md-2"><strong>Number</strong></div>
                            <div class="col-md-4"><strong>Old Narrative</strong></div>
                            <div class="col-md-4"><strong>New Narrative</strong></div>
                        </div>
                        <!-- ko foreach: Difference.ChangedConstraints -->
                        <div class="row" data-bind="visible: Type() !== 3">
                            <div class="col-md-2" data-bind="text: $parents[$parents.length - 1].GetChangeTooltip(Type())"></div>
                            <div class="col-md-2" data-bind="text: Number"></div>
                            <div class="col-md-4" data-bind="text: OldNarrative"></div>
                            <div class="col-md-4" data-bind="text: NewNarrative"></div>
                        </div>
                        <!-- /ko -->
                    </div>
                </div>
                <!-- /ko -->
            </div>
            <!-- /ko -->

            <!-- ko if: StructureDefinitionJSON() -->
            <div class="tab-pane" id="strucdefjson">
                <pre data-bind="text: StructureDefinitionJSON()"></pre>
            </div>
            <!-- /ko -->

            <!-- ko if: StructureDefinitionJSON() -->
            <div class="tab-pane" id="strucdefxml">
                <pre data-bind="text: StructureDefinitionXML()"></pre>
            </div>
            <!-- /ko -->
        </div>
    </div>

    <script type="text/html" id="inlineConstraintChange">
        <!-- ko foreach: $data -->
        <div class="row" data-bind="attr: { title: $parents[$parents.length - 1].GetChangeTooltip(ChangeType()) }">
            <div class="col-md-12">
                <!-- ko foreach: $parents -->
                <!-- ko if: $index() != 0 -->
                <span style="white-space: pre">    </span>
                <!-- /ko -->
                <!-- /ko -->
                <span data-bind="text: ($index() + 1)"></span>. <span data-bind="text: Narrative, css: { 'constraintAdded': ChangeType() == 0, 'constraintModified': ChangeType() == 1, 'constraintRemoved': ChangeType() == 2 }"></span>
            </div>
        </div>

        <div data-bind="template: { name: 'inlineConstraintChange', data: Constraints() }"></div>
        <!-- /ko -->
    </script>

    <script type="text/html" id="templateConstraint">
        <!-- ko foreach: $data -->
        
        <!-- ko if: IsHeading() -->
        <h3 data-bind="text: HeadingTitle"></h3>
        <p data-bind="text: HeadingDescription"></p>
        <!-- /ko -->
        
        <!-- ko if: Description() -->
        <p class="wiki-markup" data-bind="html: Description"></p>
        <!-- /ko -->

        <p>
            <!-- ko foreach: $parents -->
            <span data-bind="if: $index() != 0" style="white-space: pre">  </span>
            <!-- /ko -->
            <span data-bind="text: ($index() + 1)"></span>.
            <span class="wiki-markup" data-bind="html: ProseDisplay"></span>

            <!-- ko if: Label() -->
            <br />
            <!-- ko foreach: $parents -->
            <span data-bind="if: $index() != 0" style="white-space: pre">  </span>
            <!-- /ko -->
            <!-- ko foreach: $index().toString() -->
            <span style="white-space: pre"> </span>
            <!-- /ko -->
            <span style="white-space: pre"> </span>
        
            Note: <span data-bind="html: Label"></span>
            <!-- /ko -->
        </p>

        <div data-bind="template: { name: 'templateConstraint', data: $data.Children() }"></div>
        <!-- /ko -->
    </script>

    <script src="/Scripts/TemplateManagement/templateView.js"></script>
    <script src="/Scripts/lib/vkbeautify.0.99.00.beta.js"></script>
    <script type="text/javascript">
        var viewModel;

        $(document).ready(function () {
            $.blockUI({ message: "Loading..." });
            
            viewModel = new TemplateViewModel(<%= Model %>);

            viewModel.Initialized.subscribe(function (initialized) {
                if (initialized) {
                    $.unblockUI();
                }
            });

            ko.applyBindings(viewModel, $("#ViewTemplate")[0]);

            $('.sampleHelpContext').helpContext('view_template_sample');
        });
    </script>
</asp:Content>
