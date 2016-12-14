<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.EditModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Styles/templateEditor.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
    <div class="row">
        <div class="col-md-2">Ctrl + S</div>
        <div class="col-md-10">Save and Continue</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + M</div>
        <div class="col-md-10">Template/Profile Tab</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + O</div>
        <div class="col-md-10">Constraints Tab</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + P</div>
        <div class="col-md-10">Preview Tab</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + L</div>
        <div class="col-md-10">Validation Tab</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + A</div>
        <div class="col-md-10">Analyst View Mode</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + I</div>
        <div class="col-md-10">Editor View Mode</div>
    </div>
    <div class="row">
        <div class="col-md-2">Alt + G</div>
        <div class="col-md-10">Engineer View Mode</div>
    </div>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="/Scripts/jquery/jquery.cookie.js"></script>
    <script type="text/javascript" src="/Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateEdit/templateEditGlobals.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateEdit/templateEditViewModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/TemplateEdit/templateEditModels.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

    <div id="TemplateEditor">
        <h3><span data-bind="if: IsModified()">*</span> <span data-bind="text: Template().Name"></span></h3>
        <h4><span data-bind="text: Template().Oid"></span></h4>

        <!-- ko if: Template().Locked -->
        <div class="alert alert-warning" style="margin-top: 5px;">This template/profile is locked. <a href="#" data-bind="click: function () { Template().Locked(false); }">Unlock the template/profile</a> to edit.</div>
        <!-- /ko -->

        <div style="float: left; width: 100%; padding-bottom: 60px; padding-top: 10px;">
            <ul class="nav nav-tabs" id="templateEditorTabs">
                <li class="active"><a href="#template" data-toggle="tab">Template/Profile</a></li>
                <!-- ko if: DisableConstraints() -->
                <li class="disabled"><a>Constraints</a></li>
                <!-- /ko -->
                <!-- ko if: !DisableConstraints() -->
                <li data-bind="click: function () { setTimeout(ApplyWidths, 200); }"><a id="constraintsTabButton" href="#constraints" data-toggle="tab">Constraints</a></li>
                <!-- /ko -->
                <li><a href="#preview" data-toggle="tab">Preview</a></li>
                <li>
                    <a href="#validation" data-toggle="tab">
                        Validation 
                        <!-- ko if: Template().ValidationErrors().length > 0 -->
                        (<span data-bind="text: Template().ValidationErrors().length"></span>)
                        <!-- /ko -->
                    </a>
                </li>
            </ul>

            <div class="tab-content">
                <!-- Template/Profile Meta Data-->
                <div id="template" class="tab-pane active" data-bind="with: Template, validationOptions: { messagesOnModified: false, insertMessages: false }">
                    <div class="row">
                        <div class="col-md-8">

                            <div data-bind="if: !Id()">
                                <div class="input-group implementation-guide-field">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataImplementationGuideField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorImplementationGuideTooltip, placement: 'right' }"></div>
                                    </div>
                                    <select id="igCombo" class="form-control" data-bind="options: $parent.ImplementationGuides, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select', value: OwningImplementationGuideId, disable: DisableTypeFields">
                                    </select>
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: OwningImplementationGuideId"></span>

                                <!-- ko if: OwningImplementationGuideId() -->
                                <div class="input-group template-type-field">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataTemplateTypeField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorTemplateTypeTooltip, placement: 'right' }"></div>
                                    </div>
                                    <select id="templateTypeId" class="form-control" data-bind="options: $parent.TemplateTypes, optionsText: 'FullName', optionsValue: 'Id', optionsCaption: 'Select', value: TemplateTypeId, disable: DisableTypeFields"></select>
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: TemplateTypeId"></span>
                                <!-- /ko -->

                                <!-- ko if: TemplateTypeId() -->
                                <div class="input-group applies-to-field" style="width: 100%">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataAppliesToField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorAppliesToTooltip, placement: 'right' }"></div>
                                    </div>
                                    <input type="text" class="form-control" data-bind="value: PrimaryContext" style="width: 50%;" readonly="readonly" /> 
                                    <input type="text" class="form-control" data-bind="value: PrimaryContextType" style="width: 50%;" readonly="readonly" /> 
                                    <div class="input-group-btn">
                                        <button type="button" class="btn btn-primary" data-bind="disable: DisableAppliesToButton, click: function () { $('#appliesToDialog').modal('show'); }">...</button>
                                    </div>
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: PrimaryContext"></span>
                                <span class="templateMetaDataError" data-bind="validationMessage: PrimaryContextType"></span>
                                <!-- /ko -->
                            </div>

                            <div data-bind="visible: TemplateTypeId()">
                                <div class="input-group name-field">
                                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataNameField"></div>
                                    <input type="text" class="form-control" data-bind="value: Name, disable: DisableFields" autofocus />
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: Name"></span>

                                <div class="input-group identifier-field">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataOidField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorOidTooltip, placement: 'right' }"></div>
                                    </div>
                                    <input type="text" class="form-control" data-bind="value: IdentifierPrefix, disable: DisableOidField" />
                                    <div class="input-group-btn">
                                        <button type="button" class="dropdown-toggle btn btn-default" data-toggle="dropdown" href="#" data-bind="disable: DisableOidField">
                                            <span class="caret"></span>
                                        </button>
                                        <ul class="dropdown-menu pull-right">
                                            <li><a href="#" data-bind="click: function () { IdentifierPrefix($root.ImplementationGuideBaseIdentifier()) }, text: $root.ImplementationGuideBaseIdentifier"></a></li>
                                            <li><a href="#" data-bind="click: function () { IdentifierPrefix(location.origin + '/api/FHIR2/StructureDefinition/') }, text: location.origin + '/api/FHIR2/StructureDefinition'"></a></li>
                                            <li><a href="#" data-bind="click: function () { IdentifierPrefix('http://hl7.org/fhir/StructureDefinition/') }">http://hl7.org/fhir/StructureDefinition/</a></li>
                                            <!-- ko if: !$parent.IsFhir() -->
                                            <li><a href="#" data-bind="click: function () { IdentifierPrefix('urn:oid:') }">urn:oid:</a></li>
                                            <li><a href="#" data-bind="click: function () { IdentifierPrefix('urn:hl7ii:') }">urn:hl7ii:</a></li>
                                            <!-- /ko -->
                                        </ul>
                                    </div>
                                    <input type="text" class="form-control" data-bind="value: IdentifierAfix, disable: DisableOidField" />
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: IdentifierPrefix"></span>
                                <span class="templateMetaDataError" data-bind="validationMessage: IdentifierAfix"></span>

                                <div class="input-group bookmark-field">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataBookmarkField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorBookmarkTooltip, placement: 'right' }"></div>
                                    </div>
                                    <input type="text" class="form-control" data-bind="value: Bookmark, disable: DisableEngineerFields" />
                                </div>
                                <span class="templateMetaDataError" data-bind="validationMessage: Bookmark"></span>

                                <!-- ko if: TemplateTypeId() -->
                                <div class="implied-template-field" data-bind="templateSelect: ImpliedTemplateId, disable: $parent.DisableImpliedTemplate, filterContextType: PrimaryContextType, label: 'Implied Template/Profile:', canTypeAhead: true"></div>
                                <!-- /ko -->

                                <div class="input-group is-open-field">
                                    <div class="input-group-addon">
                                        <span data-bind="html: Trifolia.Web.TemplateEditorMetaDataExtensibilityField"></span>
                                        <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorExtensibilityTooltip, placement: 'right' }"></div>
                                    </div>
                                    <select class="form-control" data-bind="value: IsOpenString, disable: DisableFields">
                                        <option value="true">Open</option>
                                        <option value="false">Closed</option>
                                    </select>
                                </div>
                                <span data-bind="validationMessage: IsOpen"></span>

                                <!-- ko if: OwningImplementationGuideId() -->
                                <div class="input-group publish-status-field">
                                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataStatusField"></div>
                                    <select class="form-control" data-bind="options: $parent.PublishStatuses, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select', value: StatusId, disable: DisableFields"></select>
                                </div>
                                <!-- /ko -->

                                <div class="input-group description-field">
                                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataDescriptionField"></div>
                                    <textarea class="form-control" style="height: 100px;" data-bind="value: Description, disable: DisableEngineerFields"></textarea>
                                </div>

                                <div class="input-group notes-field">
                                    <div class="input-group-addon" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorNotesTooltip }, html: Trifolia.Web.TemplateEditorMetaDataNotesField"></div>
                                    <textarea class="form-control" style="height: 100px;" data-bind="value: Notes, disable: DisableEngineerFields"></textarea>
                                </div>

                                <!-- ko if: $parent.IsFhir() -->
                                <div class="panel panel-default extensions-panel" style="margin-top: 10px;">
                                    <div class="panel-heading">Extensions</div>
                                    <div class="panel-body">
                                        <div class="table-responsive">
                                            <table class="table">
                                                <thead>
                                                    <tr>
                                                        <th>Identifier</th>
                                                        <th>Type</th>
                                                        <th>Value</th>
                                                        <th>&nbsp;</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <!-- ko foreach: Extensions -->
                                                    <tr>
                                                        <td>
                                                            <div class="input-group extension-identifier-field">
                                                                <input type="text" class="form-control" data-bind="value: Identifier" placeholder="URI (ex: http[s]://XXX/yy/zz)" />
                                                                <span class="help-block" data-bind="validationMessage: Identifier"></span>
                                                            </div>
                                                        </td>
                                                        <td data-bind="text: Type"></td>
                                                        <td>
                                                            <div class="input-group extension-value-field">
                                                                <!-- ko if: Type() != 'Boolean' && Type() != 'Coding' && Type() != 'CodeableConcept' -->
                                                                <input type="text" class="form-control" data-bind="value: Value" />
                                                                <!-- /ko -->
                                                        
                                                                <!-- ko if: Type() == 'Boolean' -->
                                                                <select class="form-control" data-bind="value: Value">
                                                                    <option value="true">True</option>
                                                                    <option value="false">False</option>
                                                                </select>
                                                                <!-- /ko -->
                                                            
                                                                <!-- ko if: (Type() == 'Coding' || Type() == 'CodeableConcept') -->
                                                                <input type="text" class="form-control" style="width: 33%" placeholder="code" data-bind="value: ValueCodingCode" />
                                                                <input type="text" class="form-control" style="width: 33%" placeholder="display" data-bind="value: ValueCodingDisplay" />
                                                                <input type="text" class="form-control" style="width: 34%" placeholder="system" data-bind="value: ValueCodingSystem" />
                                                                <!-- /ko -->
                                                        
                                                                <span class="help-block" data-bind="validationMessage: Value"></span>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <button type="button" class="btn btn-default" data-bind="click: function() { $parent.RemoveExtension($index()); }">Remove</button>
                                                        </td>
                                                    </tr>
                                                    <!-- /ko -->
                                                </tbody>
                                                <tfoot>
                                                    <tr data-bind="with: NewExtension">
                                                        <td>
                                                            <div class="input-group new-extension-identifier-field">
                                                                <input type="text" class="form-control" data-bind="value: Identifier" placeholder="URI (ex: http[s]://XXX/yy/zz)" />
                                                                <span class="help-block" data-bind="validationMessage: Identifier"></span>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <div class="input-group new-extension-type-field">
                                                                <select class="form-control" data-bind="value: Type">
                                                                    <option>String</option>
                                                                    <option>Integer</option>
                                                                    <option>Boolean</option>
                                                                    <option>Date</option>
                                                                    <option>DateTime</option>
                                                                    <option>Code</option>
                                                                    <option>Coding</option>
                                                                    <option>CodeableConcept</option>
                                                                </select>
                                                                <span class="help-block" data-bind="validationMessage: Type"></span>
                                                            </div>
                                                        </td>
                                                        <td>
                                                            <div class="input-group new-extension-value-field">
                                                                <!-- ko if: Type() != 'Boolean' && Type() != 'Coding' && Type() != 'CodeableConcept' -->
                                                                <input type="text" class="form-control" data-bind="value: Value" />
                                                                <!-- /ko -->
                                                        
                                                                <!-- ko if: Type() == 'Boolean' -->
                                                                <select class="form-control" data-bind="value: Value">
                                                                    <option value="true">True</option>
                                                                    <option value="false">False</option>
                                                                </select>
                                                                <!-- /ko -->

                                                                <!-- ko if: (Type() == 'Coding' || Type() == 'CodeableConcept') -->
                                                                <input type="text" class="form-control" style="width: 33%" placeholder="code" data-bind="value: ValueCodingCode" />
                                                                <input type="text" class="form-control" style="width: 33%" placeholder="display" data-bind="value: ValueCodingDisplay" />
                                                                <input type="text" class="form-control" style="width: 34%" placeholder="system" data-bind="value: ValueCodingSystem" />
                                                                <!-- /ko -->
                                                        
                                                                <span class="help-block" data-bind="validationMessage: Value"></span>
                                                            </div>
                                                        </td>
                                                        <td><button type="button" class="btn btn-default new-extension-add-button" data-bind="click: $parent.AddExtension, enable: IsValid">Add</button></td>
                                                    </tr>
                                                </tfoot>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div data-bind="if: Id()">
                                <div class="input-group">
                                    <span class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataImplementationGuideField"></span>
                                    <input type="text" class="form-control" readonly="readonly" data-bind="value: $parent.GetImplementationGuideDisplay()" />
                                </div>

                                <div class="input-group">
                                    <span class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataTemplateTypeField"></span>
                                    <input type="text" class="form-control" readonly="readonly" data-bind="value: $parent.GetTemplateTypeDisplay()" />
                                </div>

                                <div class="input-group">
                                    <span class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataAppliesToField"></span>
                                    <input type="text" class="form-control" style="width: 50%" readonly="readonly" data-bind="value: PrimaryContext" />
                                    <input type="text" class="form-control" style="width: 50%" readonly="readonly" data-bind="value: PrimaryContextType" />
                                </div>

                                <!-- ko if: Id() && containerViewModel.HasSecurable(['TemplateMove']) -->
                                <div class="row">
                                    <div class="col-md-12">
                                        <a data-bind="attr: { href: MoveUrl }">Move</a> the template/profile to change the Implementation Guide, Type, or Applies To fields.
                                    </div>
                                </div>
                                <!-- /ko -->
                            </div>

                            <!-- ko if: Author() -->
                            <div class="input-group">
                                <span class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataAuthoredByField"></span>
                                <input type="text" class="form-control" readonly="readonly" data-bind="value: Author" />
                            </div>
                            <!-- /ko -->

                            <!-- ko if: OrganizationName() -->
                            <div class="input-group">
                                <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataOrganizationField"></div>
                                <input type="text" class="form-control" readonly="readonly" data-bind="value: OrganizationName" />
                            </div>
                            <!-- /ko -->

                            <!-- ko if: PreviousVersionLink() -->
                            <div class="input-group">
                                <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorMetaDataNewVersionOfField"></div>
                                <input type="text" class="form-control" readonly="readonly" data-bind="value: PreviousVersionName" />
                                <div class="input-group-btn">
                                    <button type="button" class="btn btn-default" data-bind="click: function () { location.href = PreviousVersionLink(); }">View</button>
                                </div>
                            </div>
                            <!-- /ko -->
                        </div>
                    </div>
                </div>

                <!-- Constraints -->
                <div id="constraints" class="tab-pane" style="overflow: auto; padding: 20px; float: left; ">

                    <div id="constraintsTree" class="constraintTree">
                        <div class="constraintRow constraintHeader">
                            <div class="constraintColumn" style="width: 15px;">&nbsp;</div>
                            <div class="constraintColumn constraintContext" data-bind="html: Trifolia.Web.TemplateEditorContextHeading"></div>
                            <div class="constraintColumn constraintNumber" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorConstraintNumberHeadingTooltip }">
                                <input type="text" class="searchConstraintText" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorSearchConstraintTooltip }, event: { keypress: SearchConstraintKeyPress }, value: SearchConstraintNumber, valueUpdate: 'afterkeydown'" placeholder="CONF#"/>
                                <div class="glyphicon glyphicon-search" data-bind="click: function () { FindConstraintNode(SearchConstraintNumber()); }"></div>
                            </div>
                            <div class="constraintColumn constraintBranch" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorBranchIdentifierHeadingTooltip }, html: Trifolia.Web.TemplateEditorBranchIdentifierHeading"></div>
                            <div class="constraintColumn constraintConformance" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorConformanceHeadingTooltip }, html: Trifolia.Web.TemplateEditorConformanceHeadingTooltip"></div>
                            <div class="constraintColumn constraintCardinality" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorCardinalityHeadingTooltip }, html: Trifolia.Web.TemplateEditorCardinalityHeading"></div>
                            <div class="constraintColumn constraintDataType" data-bind="html: Trifolia.Web.TemplateEditorDataTypeHeading"></div>
                            <div class="constraintColumn constraintValue">
                                Value
                                <!-- ko if: (IsAnalyst() && !Template().Locked()) -->
                                <a href="#" style="text-align: right" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorAddTopLevelPrimitiveTooltip }, click: function () { AddPrimitive(); }"><span class="glyphicon glyphicon-text-width" style="padding-right: 5px; float: right; margin-top: 9px;"></span></a>
                                <!-- /ko -->
                            </div>
                        </div>

                        <div data-bind="template: { name: 'constraintTreeTemplate', data: GetNodes(null) }"></div>
                    </div>
                
                    <div class="constraintEditor panel panel-default" data-bind="visible: CurrentNode(), with: CurrentNode">
                        <!-- Constraint Editor Header -->
                        <div class="panel-heading" style="padding: 2px;">
                            <table class="table" style="margin-bottom: 0px;">
                                <tr>
                                    <td>
                                        <span class="glyphicon glyphicon-resize-full" style="cursor: pointer;" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorExpandCollapseConstraintEditorTooltip }, click: function () { $parent.IsEditorMaximized(!$parent.IsEditorMaximized()); }"></span>
                                    </td>
                                    <td>
                                        <span data-bind="text: DisplayContext"></span>
                                    </td>
                                    <td>
                                        <!-- ko if: (Constraint() && $parent.IsAnalyst() && !$parent.Template().Locked() && $parent.IsEditorMaximized()) -->
                                        <span data-bind="text: ComputedNumber"></span>
                                        <a class="clickable" data-bind="click: function () { $('#constraintNumberEdit').slideToggle(); }" title="Edit Constraint Number"><i class="glyphicon glyphicon-pencil"></i></a>
                                        <div id="constraintNumberEdit" class="popover fade bottom in">
                                            <div class="arrow"></div>
                                            
                                            <div class="form-group">
                                                <label>Unique Number</label> <span class="glyphicon glyphicon-question-sign" data-bind="tooltip: 'A unique number is always required. The unique number is used by default for conformance numbers in exports unless a display number is specified. This number must be unique across all constraints in the implementation guide that the template/profile is associated with.'"></span>
                                                <div class="input-group">
                                                    <input type="text" class="form-control input-sm" data-bind="spinedit: Constraint().Number, treatZeroAsNull: true" />
                                                </div>
                                            </div>

                                            <div class="form-group">
                                                <label>Display Number</label> <span class="glyphicon glyphicon-question-sign" data-bind="tooltip: 'Optional. The display number is used to override the conformance number format used by default in exports (MS Word, Schematron, etc.). The display number can contain any character (including dashes, underlines, semi-colons, etc.)'"></span>
                                                <input type="text" class="form-control" data-bind="value: Constraint().DisplayNumber" />
                                            </div>

                                            <button type="button" class="btn btn-primary" style="width: 100%;" data-bind="click: $parent.ConformanceNumberChanged">OK</button>
                                        </div>
                                        <!-- /ko -->
                                        <!-- ko if: (Constraint() && (!$parent.IsAnalyst() || $parent.Template().Locked() || !$parent.IsEditorMaximized())) -->
                                        <span data-bind="text: Constraint().Number"></span>
                                        <!-- /ko -->
                                    </td>
                                    <td>
                                        <div class="pull-right">
                                            <div class="btn-group btn-group-sm">
                                                <!-- ko if: ($parent.IsAnalyst() && !$parent.Template().Locked() && $parent.IsEditorMaximized()) -->

                                                <!-- ko if: !Constraint() -->
                                                <button type="button" class="btn btn-default" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorCreateComputableTooltip }, click: CreateComputable"><i class="glyphicon glyphicon-plus"></i></button>
                                                <!-- /ko -->
                                
                                                <button type="button" class="btn btn-default" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorAddChildPrimitiveTooltip }, click: function () { $parent.AddPrimitive($data); }, disable: $parent.DisableAddChildPrimitive()"><i class="glyphicon glyphicon-text-width"></i></button>

                                                <!-- ko if: Constraint() -->
                                                <button type="button" class="btn btn-default" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorDuplicateTooltip }, click: function () { $parent.DuplicateConstraint($data); }, disable: $parent.DisableDuplicateConstraint()"><i class="glyphicon glyphicon-repeat"></i></button>
                                                <button type="button" class="btn btn-default" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorEditNoteTooltip }, click: $parent.EditNote"><i class="glyphicon glyphicon-comment"></i></button>
                                                <button type="button" class="btn btn-default" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorRemoveTooltip }, click: function () { $parent.RemoveConstraint($data); }, disable: $parent.DisableConstraintRemove()"><i class="glyphicon glyphicon-trash"></i></button>
                                                <!-- /ko -->

                                                <!-- /ko -->
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div class="panel-body" style="overflow-y: scroll; max-height: 325px;">
                            <!-- ko with: Constraint -->
                            <div data-bind="template: { name: 'constraintEditorBody' }, validationOptions: { messagesOnModified: false, insertMessages: false }"></div>
                            <!-- /ko -->

                            <!-- ko if: !Constraint() && $parent.IsFhir() && Context() == 'extension' -->
                            <div data-bind="template: { name: 'constraintExtension' }"></div>
                            <!-- /ko -->
                        </div>
                    
                        <div class="panel-footer" data-bind="with: Constraint, visible: $parent.IsEditorMaximized">
                            <div data-bind="html: NarrativeProseHtml"></div>
                        </div>
                    </div>
                </div>

                <!-- Preview -->
                <div id="preview" class="tab-pane">
                    <div style="position: relative">
                        <div data-bind="template: { name: 'constraintPreviewTemplate', data: Constraints() }"></div>
                    </div>
                </div>

                <!-- Validation -->
                <div id="validation" class="tab-pane">
                    <div class="row">
                        <!-- ko if: Template().ValidationErrors().length > 0 -->
                        <div class="col-md-6">
                            <h3 data-bind="html: Trifolia.Web.TemplateEditorValidationErrors"></h3>
                            <table class="table">
                                <thead>
                                    <tr>
                                        <th>CONF</th>
                                        <th>Message</th>
                                    </tr>
                                </thead>
                                <tbody data-bind="foreach: Template().ValidationErrors()">
                                    <tr>
                                        <td data-bind="text: ConstraintNumber"></td>
                                        <td data-bind="text: Message"></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <!-- /ko -->
                        <!-- ko if: Template().ValidationWarnings().length > 0 -->
                        <div class="col-md-6">
                            <h3 data-bind="html: Trifolia.Web.TemplateEditorValidationWarnings"></h3>
                            <table class="table">
                                <thead>
                                    <tr>
                                        <th>CONF</th>
                                        <th>Message</th>
                                    </tr>
                                </thead>
                                <tbody data-bind="foreach: Template().ValidationWarnings()">
                                    <tr>
                                        <td data-bind="text: ConstraintNumber"></td>
                                        <td data-bind="text: Message"></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <!-- /ko -->
                        
                        <!-- ko if: Template().ValidationResults().length == 0 -->
                        <div class="col-md-12">
                            <span style="font-style: italic" data-bind="html: Trifolia.Web.TemplateEditorValidationNoMessages"></span>
                        </div>
                        <!-- /ko -->
                    </div>
                    
                    <!-- ko if: Template().ImpliedByTemplates().length > 0 || Template().ContainedByTemplates().length > 0 -->
                    <div class="alert alert-warning" style="margin-top: 20px; margin-bottom: -5px;">Modifying this template/profile may impact one or more of the following templates which contain/imply this template/profile.</div>
                    <!-- /ko -->

                    <div class="row">
                        <!-- ko if: Template().ImpliedByTemplates().length > 0 -->
                        <div class="col-md-6">
                            <h3>Implied By</h3>
                            <ul data-bind="foreach: Template().ImpliedByTemplates">
                                <li><span data-bind="text: Name"></span> [<a data-bind="attr: { href: ViewUrl }">View</a> | <a data-bind="attr: { href: EditUrl }">Edit</a>]<br />
                                    <span data-bind="text: ImplementationGuide"></span></li>
                            </ul>
                        </div>
                        <!-- /ko -->
                        <!-- ko if: Template().ContainedByTemplates().length > 0 -->
                        <div class="col-md-6">
                            <h3>Contained By</h3>
                            <ul data-bind="foreach: Template().ContainedByTemplates">
                                <li><span data-bind="text: Name"></span> [<a data-bind="attr: { href: ViewUrl }">View</a> | <a data-bind="attr: { href: EditUrl }">Edit</a>]<br />
                                    <span data-bind="text: ImplementationGuide"></span></li>
                            </ul>
                        </div>
                        <!-- /ko -->
                    </div>
                </div>
            </div>
        </div>

        <div id="footer">
            <div class="row">
                <div class="col-md-2">
                    <!-- ko with: Template -->
                    <div class="btn-group dropup">
                        <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown" data-bind="css: { 'disabled': !IsValid() }">Save <b class="caret"></b></button>
                        <ul class="dropdown-menu" role="menu">
                            <li><a href="#" data-bind="click: function () { $parent.Save(); }">and Continue</a></li>
                            <li><a href="#" data-bind="click: function () { $parent.Save('publishSettings'); }">and Publish Settings</a></li>
                            <li><a href="#" data-bind="click: function () { $parent.Save('list'); }">and List</a></li>
                            <li><a href="#" data-bind="click: function () { $parent.Save('view'); }">and View</a></li>
                        </ul>
                    </div>
                    <!-- /ko -->
                    <div class="btn-group dropup">
                        <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">Cancel <b class="caret"></b></button>
                        <ul class="dropdown-menu" role="menu">
                            <li><a href="#" data-bind="click: function () { Cancel('list'); }">and List</a></li>
                            <li data-bind="css: { 'disabled': !TemplateId() }"><a href="#" data-bind="    click: function () { Cancel('view'); }">and View</a></li>
                        </ul>
                    </div>
                </div>
                <div class="col-md-2">
                    <!-- ko if: Status -->
                    <span data-bind="text: Status"></span>
                    <!-- /ko -->
                </div>
                <div class="col-md-5">
                    <div data-bind="templateSelect: QuickEditTemplateId, buttons: [{ Text: 'Go', Handler: QuickEditTemplate }], label: 'Quick Edit', small: true, canTypeAhead: true"></div>            
                </div>
                <div class="col-md-3">
                    <div class="input-group input-group-sm">
                        <span class="input-group-addon">View Mode <i data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorViewModeTooltip, placement: 'top' }"></i></span>
                        <select class="form-control" data-bind="value: ViewMode">
                            <option>Analyst</option>
                            <option>Editor</option>
                            <option>Engineer</option>
                        </select>
                    </div>
                </div>
            </div>
        </div>
        
        <div id="editNoteDialog" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">Edit Note</div>
                    <div class="modal-body">
                        <textarea class="form-control" data-bind="value: ConstraintNote"></textarea>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-bind="click: EditNoteSave">OK</button>
                        <button type="button" class="btn btn-primary" data-bind="click: EditNoteCancel">Cancel</button>
                    </div>
                </div>
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
                        <span class="help-block" data-bind="if: viewModel.CurrentAppliesToNode() && !viewModel.CurrentAppliesToNode().DataType()">The selection is not valid.</span>
                        <button type="button" class="btn btn-default" data-bind="click: function () {
                            viewModel.Template().PrimaryContext(viewModel.CurrentAppliesToNode().Context());
                            viewModel.Template().PrimaryContextType(viewModel.CurrentAppliesToNode().DataType());
                            viewModel.CurrentAppliesToNode(null);
                            $('#appliesToDialog').modal('hide');
                        }, disable: !viewModel.CurrentAppliesToNode() || !viewModel.CurrentAppliesToNode().DataType()">OK</button>
                        <button type="button" class="btn btn-primary" data-bind="click: function () {
                            viewModel.CurrentAppliesToNode(null);
                            $('#appliesToDialog').modal('hide');
                        }">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/html" id="constraintExtension">
        <div class="form-group">
            <label>Pre-defined Extension</label>
            <div class="input-group input-group-sm">
                <select class="form-control input-sm" data-bind="options: $parent.AvailableExtensions, optionsText: 'Name', optionsValue: 'Id', value: $parent.SelectedAvailableExtensionId, optionsCaption: 'Choose...'"></select>
                <div class="input-group-btn">
                    <button type="button" class="btn btn-default btn-sm" data-bind="disable: !$parent.SelectedAvailableExtensionId(), click: $parent.AddAvailableExtension"><i class="glyphicon glyphicon-plus"></i></button>
                </div>
            </div>
        </div>
    </script>

    <script type="text/html" id="constraintEditorBody">
        <!-- Analyst: Computable -->
        <div data-bind="if: IsAnalystComputable(), disableAll: $parents[1].DisableConstraintFields()" class="constraintEditorSet">
            <!-- Main Constraint Details -->

            <!-- ko if: !$root.CurrentNode().IsChildOfChoice() -->
            <div class="input-group input-group-sm" style="width: 100%;">
                <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorConformanceCardinality"></div>
                <select class="form-control input-sm" style="width: 50%;" data-bind="value: Conformance, disable: $parents[1].Template().Locked">
                    <option>SHALL</option>
                    <option>SHOULD</option>
                    <option>MAY</option>
                    <option>SHALL NOT</option>
                    <option>SHOULD NOT</option>
                    <option>MAY NOT</option>
                </select>
                <div class="input-group input-group-sm cardinality" style="width:50%; padding-top: 0px">
                    <input class="span2 form-control" id="appendedInputButton" size="16" type="text" data-bind="value: Cardinality, disable: $parents[1].Template().Locked">
                    <div class="input-group-btn">
                        <a class="dropdown-toggle btn btn-primary btn-sm" data-toggle="dropdown" href="#">
                            <span class="caret"></span>
                        </a>
                        <ul class="dropdown-menu pull-right">
                            <li><a href="#" data-bind="click: function () { Cardinality('0..0') }">0..0</a></li>
                            <li><a href="#" data-bind="click: function () { Cardinality('0..1') }">0..1</a></li>
                            <li><a href="#" data-bind="click: function () { Cardinality('0..*') }">0..*</a></li>
                            <li><a href="#" data-bind="click: function () { Cardinality('1..1') }">1..1</a></li>
                            <li><a href="#" data-bind="click: function () { Cardinality('1..*') }">1..*</a></li>
                        </ul>
                    </div>
                </div>
            </div>
            <span class="help-block" data-bind="validationMessage: Cardinality"></span>
            
            <div class="input-group input-group-sm">
                <div class="input-group-addon">
                    <span data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorDataType"></span>
                    <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorConstraintEditorDataTypeTooltip, placement: 'right' }"></div>
                </div>
                <select class="form-control input-sm" data-bind="
options: $parents[1].GetDataTypes($parent.DataType()),
optionsText: 'Text', optionsValue: 'Value',
value: DataType,
event: { change: $parents[1].ConstraintDataTypeChanged },
disable: $parents[1].Template().Locked">
                </select>
            </div>
            <!-- /ko -->

            <!-- ko if: !$root.CurrentNode().IsChoice() && !$root.CurrentNode().IsChildOfChoice() -->
            <div class="input-group input-group-sm branching">
                <div class="input-group-addon" data-bind="text: Trifolia.Web.TemplateEditorBranchingTitle"></div>
                <div class="form-control">
                    <input type="checkbox" name="Branching" data-bind="checked: IsBranch, disable: (IsBranchDisabled() || $parents[1].Template().Locked())" />&nbsp;<span data-bind="text: Trifolia.Web.TemplateEditorBranchRootOption"></span><br />
                    <input type="checkbox" name="Branching" data-bind="checked: IsBranchIdentifier, disable: (IsBranchIdentifierDisabled() || $parents[1].Template().Locked())" />&nbsp;<span data-bind="text: Trifolia.Web.TemplateEditorBranchIdentifierOption"></span>
                </div>
            </div>
            <!-- /ko -->

            <!-- ko if: !$root.CurrentNode().IsChoice() -->
            <!-- ko if: $parents[1].Categories().length > 0 -->
            <div class="input-group input-group-sm">
                <div class="input-group-addon">Category</div>
                <select class="form-control input-sm" multiple="multiple" data-bind="disable: $parents[1].Template().Locked, options: $parents[1].Categories, selectedOptions: Categories"></select>
            </div>
            <!-- /ko -->

            <div data-bind="templateSelect: ContainedTemplateId, filterContextType: $parent.DisplayDataType, disable: $parents[1].Template().Locked, label: 'Template/Profile:', small: true, canTypeAhead: true"></div>

            <div class="input-group input-group-sm">
                <div class="input-group-addon">
                    Binding Type:
                    <div data-bind="helpTooltip: { title: Trifolia.Web.TemplateEditorConstraintEditorBindingTypeTooltip, placement: 'right' }"></div>
                </div>
                <select class="form-control input-sm" data-bind="value: BindingType, disable: $parents[1].Template().Locked">
                    <option value="None">None</option>
                    <option value="SingleValue">Single Value</option>
                    <option value="ValueSet">Value Set</option>
                    <option value="CodeSystem">Code System</option>
                    <option value="Other">Other</option>
                </select>
            </div>

            <div data-bind="if: BindingType() == 'SingleValue'">
                <div data-bind="template: { name: 'singleValueBinding' }"></div>
            </div>
                                        
            <div data-bind="if: BindingType() == 'ValueSet'">
                <div data-bind="template: { name: 'valueSetBinding' }"></div>
            </div>
                                        
            <div data-bind="if: BindingType() == 'CodeSystem'">
                <div data-bind="template: { name: 'codeSystemBinding' }"></div>
            </div>
                                        
            <div data-bind="if: BindingType() == 'Other'">
                <div data-bind="template: { name: 'otherBinding' }"></div>
            </div>

            <!-- ko if: $parents[$parents.length-1].IsFhir() -->
            <div class="input-group input-group-sm">
                <div class="input-group-addon">Is Modifier</div>
                <div class="form-control">
                    <input type="checkbox" name="Modifier" data-bind="checked: IsModifier"/>
                </div>
            </div>

            <div class="input-group input-group-sm">
                <div class="input-group-addon">Must Support</div>
                <div class="form-control">
                    <input type="checkbox" name="Support" data-bind="checked: MustSupport"/>
                </div>
            </div>
            <!-- /ko -->
            <!-- /ko -->
        </div>

        <!-- Analyst: Primitive Constraint -->
        <div data-bind="if: IsAnalystPrimitive()" class="constraintEditorSet">
            <div style="min-width: 400px">
                <textarea class="form-control input-sm" style="height: 50px;" data-bind="value: PrimitiveText, disable: $parents[1].Template().Locked" placeholder="Constraint Prose"></textarea>
                <div class="input-group input-group-sm">
                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorConformance"></div>
                    <select class="form-control input-sm" data-bind="value: Conformance, disable: $parents[1].Template().Locked">
                        <option>SHALL</option>
                        <option>SHOULD</option>
                        <option>MAY</option>
                        <option>SHALL NOT</option>
                        <option>SHOULD NOT</option>
                        <option>MAY NOT</option>
                    </select>
                </div>
                                    
                <div data-bind="valuesetSelect: ValueSetId, label: 'Value Set:', disable: $parents[1].Template().Locked, small: true, canTypeAhead: true"></div>

                <div class="input-group input-group-sm date" data-bind="date: ValueSetDate">
                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBindingDate"></div>
                    <input type="text" class="form-control input-sm" placeholder="MM/DD/YYYY" />
                    <span class="input-group-btn" style="width: 20px;">
                        <button type="button" class="btn btn-default btn-sm btn-clear" data-bind="click: function () { ValueSetDate(''); }"><i class="glyphicon glyphicon-remove"></i></button>
                        <button type="button" class="btn btn-default btn-sm"><i class="glyphicon glyphicon-calendar"></i></button>
                    </span>
                </div>
                <div class="input-group input-group-sm">
                    <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBinding"></div>
                    <select class="form-control input-sm" data-bind="value: Binding, disable: $parents[1].Template().Locked">
                        <option value="DEFAULT"></option>
                        <option>STATIC</option>
                        <option>DYNAMIC</option>
                    </select>
                </div>
            </div>
        </div>
                            
        <!-- Tech Editor: Primitive -->
        <div style="width: 100%;" data-bind="if: IsTechEditorPrimitive()" class="constraintEditorSet">
            <textarea class="form-control input-sm" style="height: 50px;" data-bind="value: PrimitiveText, disable: $parents[1].Template().Locked" placeholder="Constraint Prose"></textarea>

            <div class="input-group input-group-sm">
                <div class="input-group-addon" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorConstraintEditorDescriptionTooltip }, html: Trifolia.Web.TemplateEditorConstraintEditorDescription"></div>
                <textarea class="form-control input-sm" style="height: 50px;" data-bind="value: Description, disable: $parents[1].Template().Locked" placeholder="Description"></textarea>
            </div>
            <div class="input-group input-group-sm">
                <div class="input-group-addon" data-bind="attr: { 'data-original-title': Trifolia.Web.TemplateEditorConstraintEditorLabelTooltip }, html: Trifolia.Web.TemplateEditorConstraintEditorLabel"></div>
                <input type="text" class="form-control input-sm" data-bind="value: Label, disable: $parents[1].Template().Locked" />
            </div>
        </div>

        <!-- Engineer -->
        <div style="width: 100%;" data-bind="if: IsEngineer()">
            <div class="input-group input-group-sm">
                <span data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorAutoGenerate"></span>
                <span style="white-space: pre;"> </span>
                <input type="checkbox" data-bind="checked: IsAutomaticSchematron, disable: $parents[1].Template().Locked" />
            </div>
            <div class="input-group input-group-sm">
                <span data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorInheritable"></span>
                <span style="white-space: pre;"> </span>
                <input type="checkbox" data-bind="checked: IsInheritable, disable: $parents[1].Template().Locked" />
            </div>
            <div class="input-group input-group-sm">
                <span data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorRooted"></span>
                <span style="white-space: pre;"> </span>
                <input type="checkbox" data-bind="checked: IsSchRooted, disable: $parents[1].Template().Locked" />
            </div>

            <textarea class="form-control input-sm" style="height: 50px;" data-bind="disable: (IsAutomaticSchematron() || $parents[1].Template().Locked()), value: Schematron" placeholder="Custom Schematron"></textarea>
        </div>

        <!-- Tech Editor: Computable -->
        <div data-bind="if: IsTechEditorComputable()" class="constraintEditorSet">
            <div class="input-group input-group-sm">
                <div class="input-group-addon" data-bind="attr: { title: Trifolia.Web.TemplateEditorConstraintEditorDescriptionTooltip }, html: Trifolia.Web.TemplateEditorConstraintEditorDescription"></div>
                <textarea class="form-control input-sm" style="height: 50px;" data-bind="value: Description, disable: $parents[1].Template().Locked" placeholder="Description"></textarea>
            </div>
            <div class="input-group input-group-sm">
                <div class="input-group-addon" data-bind="attr: { title: Trifolia.Web.TemplateEditorConstraintEditorLabelTooltip }, html: Trifolia.Web.TemplateEditorConstraintEditorLabel"></div>
                <input type="text" class="form-control input-sm" data-bind="value: Label, disable: $parents[1].Template().Locked" />
            </div>
            <div class="input-group input-group-sm">
                <div class="input-group-addon">
                    Heading <input type="checkbox" data-bind="checked: IsHeading, disable: $parents[1].Template().Locked" />
                </div>
                <textarea class="form-control input-sm" style="height: 50px;" data-bind="value: HeadingDescription, disable: ($parents[1].Template().Locked() || !IsHeading())" placeholder="Heading Description"></textarea>
            </div>
        </div>
    </script>

    <script type="text/html" id="singleValueBinding">
        <div class="input-group input-group-sm" style="width: 100%;">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorCode"></div>
            <input type="text" style="width: 50%;" class="form-control input-sm" data-bind="value: Value, disable: $parents[1].Template().Locked" placeholder="Code XXXX" />
            <input type="text" style="width: 50%;" class="form-control input-sm" data-bind="value: ValueDisplayName, disable: $parents[1].Template().Locked" placeholder="Display XXXX" />
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorCodeSystem"></div>
            <select class="form-control input-sm" data-bind="value: ValueCodeSystemId, options: $parents[1].CodeSystems, optionsText: 'Display', optionsValue: 'Id', optionsCaption: 'Select', disable: $parents[1].Template().Locked">
            </select>
        </div>
    </script>

    <script type="text/html" id="valueSetBinding">
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorValueConformance"></div>
            <select class="form-control input-sm" data-bind="value: ValueConformance, disable: $parents[1].Template().Locked">
                <option value="">NONE</option>
                <option>SHALL</option>
                <option>SHOULD</option>
                <option>MAY</option>
                <option>SHALL NOT</option>
                <option>SHOULD NOT</option>
                <option>MAY NOT</option>
            </select>
        </div>
                                
        <div data-bind="valuesetSelect: ValueSetId, label: 'Value Set:', disable: $parents[1].Template().Locked, small: true, canTypeAhead: true"></div>

        <div class="input-group input-group-sm date" data-bind="date: ValueSetDate">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBindingDate"></div>
            <input type="text" class="form-control input-sm" placeholder="MM/DD/YYYY" data-bind="disable: $parents[1].Template().Locked" />
            <span class="input-group-btn" style="width: 20px;">
                <button type="button" class="btn btn-default btn-sm btn-clear" data-bind="click: function () { ValueSetDate(''); }, disable: $parents[1].Template().Locked"><i class="glyphicon glyphicon-remove"></i></button>
                <button type="button" class="btn btn-default btn-sm" data-bind="disable: $parents[1].Template().Locked"><i class="glyphicon glyphicon-calendar"></i></button>
            </span>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBinding"></div>
            <select class="form-control input-sm" data-bind="value: Binding, disable: $parents[1].Template().Locked">
                <option value="DEFAULT"></option>
                <option>STATIC</option>
                <option>DYNAMIC</option>
            </select>
        </div>
    </script>

    <script type="text/html" id="codeSystemBinding">
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorValueConformance"></div>
            <select class="form-control input-sm" data-bind="value: ValueConformance, disable: $parents[1].Template().Locked">
                <option value="">NONE</option>
                <option>SHALL</option>
                <option>SHOULD</option>
                <option>MAY</option>
                <option>SHALL NOT</option>
                <option>SHOULD NOT</option>
                <option>MAY NOT</option>
            </select>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorCodeSystem"></div>
            <select class="form-control input-sm" data-bind="value: ValueCodeSystemId, options: $parents[1].CodeSystems, optionsText: 'Display', optionsValue: 'Id', optionsCaption: 'Select', disable: $parents[1].Template().Locked">
            </select>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBinding"></div>
            <select class="form-control input-sm" data-bind="value: Binding, disable: $parents[1].Template().Locked">
                <option value="DEFAULT"></option>
                <option>STATIC</option>
                <option>DYNAMIC</option>
            </select>
        </div>
    </script>

    <script type="text/html" id="otherBinding">
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorValueConformance"></div>
            <select class="form-control input-sm" data-bind="value: ValueConformance, disable: $parents[1].Template().Locked">
                <option value="">NONE</option>
                <option>SHALL</option>
                <option>SHOULD</option>
                <option>MAY</option>
                <option>SHALL NOT</option>
                <option>SHOULD NOT</option>
                <option>MAY NOT</option>
            </select>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorCode"></div>
            <input type="text" class="form-control input-sm" style="width: 50%;" data-bind="value: Value, disable: $parents[1].Template().Locked" placeholder="Code XXXX" />
            <input type="text" class="form-control input-sm" style="width: 50%;" data-bind="value: ValueDisplayName, disable: $parents[1].Template().Locked" placeholder="Display XXXX" />
        </div>
                                
        <div data-bind="valuesetSelect: ValueSetId, label: 'Value Set:', disable: $parents[1].Template().Locked, small: true, canTypeAhead: true"></div>
                                
        <div class="input-group input-group-sm date" data-bind="date: ValueSetDate">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBindingDate"></div>
            <input type="text" class="form-control input-sm" placeholder="MM/DD/YYYY" data-bind="disable: $parents[1].Template().Locked" />
            <span class="input-group-btn" style="width: 20px;">
                <button type="button" class="btn btn-default btn-sm btn-clear" data-bind="click: function () { ValueSetDate(''); }, disable: $parents[1].Template().Locked"><i class="glyphicon glyphicon-remove"></i></button>
                <button type="button" class="btn btn-default btn-sm" data-bind="disable: $parents[1].Template().Locked"><i class="glyphicon glyphicon-calendar"></i></button>
            </span>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorCodeSystem"></div>
            <select class="form-control input-sm" data-bind="value: ValueCodeSystemId, options: $parents[1].CodeSystems, optionsText: 'Display', optionsValue: 'Id', optionsCaption: 'Select', disable: $parents[1].Template().Locked">
            </select>
        </div>
        <div class="input-group input-group-sm">
            <div class="input-group-addon" data-bind="html: Trifolia.Web.TemplateEditorConstraintEditorBinding"></div>
            <select class="form-control input-sm" data-bind="value: Binding, disable: $parents[1].Template().Locked">
                <option value="DEFAULT"></option>
                <option>STATIC</option>
                <option>DYNAMIC</option>
            </select>
        </div>
    </script>
    
    <script type="text/html" id="constraintPreviewTemplate">
        <!-- ko foreach: $data -->

        <!-- ko if: !IsChoice() || Children().length != 1 -->
        <!-- ko if: IsHeading() -->
        <p data-bind="text: Context" class="previewConstraintHeading"></p>
        <p data-bind="text: HeadingDescription" class="previewConstraintHeadingDescription"></p>
        <!-- /ko -->
        
        <!-- ko if: Description() -->
        <p data-bind="text: Description" class="previewConstraintDescription"></p>
        <!-- /ko -->

        <p>
            <!-- ko foreach: $parents -->
            <!-- ko if: $index() > 1 && $parents[$index()].IsChoice && (!$parents[$index()].IsChoice() || $parents[$index()].Children().length != 1) --><span style="white-space: pre">  </span><!-- /ko -->
            <!-- /ko -->
            <span data-bind="text: ($index() + 1)"></span>) 
            <span data-bind="html: NarrativeProseHtml"></span>
        </p>
        <!-- /ko -->

        <div data-bind="template: { name: 'constraintPreviewTemplate', data: $data.Children() }"></div>

        <!-- /ko -->
    </script>
    
    <script type="text/html" id="constraintTreeTemplate">
        <!-- ko foreach: $data -->
        <div class="constraintRow" data-bind="
    attr: {
        'nodeId': Id,
        title: DisplayTitle
    },
    css: {
        'constrained': Constraint(),
        'primitive': Constraint() && Constraint().IsPrimitive(),
        'selected': $parents[$parents.length - 1].IsCurrentNode($data)
    },
    style: {
        'color': HasWarning() ? 'red' : 'black',
        'font-style': HasWarning() ? 'italic' : 'normal'
    }">
            <div class="constraintColumn" style="width: 15px;">
                <span data-bind="if: (HasChildren() && !IsExpanded())">
                    <div data-bind="click: function () { $parents[$parents.length - 1].ExpandNode($data, true); }" class="glyphicon glyphicon-expand"></div>
                </span>
                <span data-bind="if: (HasChildren() && IsExpanded())">
                    <div data-bind="click: function () { $parents[$parents.length - 1].CollapseNode($data); }" class="glyphicon glyphicon-collapse-down"></div>
                </span>
            </div>
            <div class="constraintColumn constraintContext" data-bind="click: function () { $parents[$parents.length - 1].SelectNode($data); }">
                <!-- ko foreach: $parents -->
                <!-- ko if: $index() > 1 --><span style="white-space: pre">  </span><!-- /ko -->
                <!-- /ko -->
                <span data-bind="text: DisplayContext"></span>

                <!-- ko if: !IsValid() -->
                <span class="glyphicon glyphicon-exclamation-sign" style="color: red;" data-bind="tooltip: Constraint().ValidationMessages"></span>
                <!-- /ko -->
                
                <!-- ko if: DisplayNotes() -->
                <span class="glyphicon glyphicon-comment" data-bind="tooltip: DisplayNotes"></span>
                <!-- /ko -->
            </div>
            <div class="constraintColumn constraintNumber" data-bind="text: ComputedNumber, attr: { 'data-original-title': ComputedNumberTooltip }, click: function () { $parents[$parents.length - 1].SelectNode($data); }"></div>
            <div class="constraintColumn constraintBranch" data-bind="click: function () { $parents[$parents.length - 1].SelectNode($data); }">
                <!-- ko if: !IsChildOfChoice() -->
                <span data-bind="text: DisplayBranchRoot"></span>
                <!-- /ko -->
            </div>
            <div class="constraintColumn constraintBranch" data-bind="click: function () { $parents[$parents.length - 1].SelectNode($data); }">
                <!-- ko if: !IsChildOfChoice() -->
                <span data-bind="text: DisplayBranchIdentifier"></span>
                <!-- /ko -->
            </div>
            <div class="constraintColumn constraintConformance" data-bind="text: DisplayConformance, click: function () { $parents[$parents.length - 1].SelectNode($data); }"></div>
            <div class="constraintColumn constraintCardinality" data-bind="text: DisplayCardinality, click: function () { $parents[$parents.length - 1].SelectNode($data); }"></div>
            <div class="constraintColumn constraintDataType" data-bind="text: DisplayDataType, click: function () { $parents[$parents.length - 1].SelectNode($data); }"></div>
            <div class="constraintColumn constraintValue" data-bind="text: DisplayValue, click: function () { $parents[$parents.length - 1].SelectNode($data); }"></div>
        </div>
        
        <!-- ko if: IsExpanded() -->
        <div data-bind="template: { name: 'constraintTreeTemplate', data: $parents[$parents.length-1].GetNodes($data) }"></div>
        <!-- /ko -->
        <!-- /ko -->
    </script>
      
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
                <span data-bind="text: DisplayContext"></span>
            </div>
        </div>
        
        <!-- ko if: IsExpanded() -->
        <div data-bind="template: { name: 'appliesToNode', data: Children() }"></div>
        <!-- /ko -->
        <!-- /ko -->
    </script>

    <script type="text/javascript">
        var defaults = {
            ImplementationGuideId: parseInt('<%= Model.DefaultImplementationGuideId %>')
        };
        var viewModel = new templateEditViewModel(parseInt('<%= Model.TemplateId %>'), defaults);

        $(document).ready(function () {
            $(window).bind('beforeunload', viewModel.ConfirmLeave);

            $("#appliesToDialog").modal({
                show: false
            });

            $("#editNoteDialog").modal({
                show: false
            });

            $("#constraintsTabButton").on("shown.bs.tab", function (e) {
                viewModel.ApplyWidths();
            });

            var dialogIsOpen = function () {
                return $("body").hasClass('modal-open');
            };

            $(document).on('keydown', function (e) {
                if (e.ctrlKey) {            // Ctrl
                    if (e.keyCode == 83) {      // S: Save
                        if (!dialogIsOpen())
                            viewModel.Save();
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    }
                }
                
                if (e.altKey) {
                    if (e.keyCode == 77) {       // M: Template/Profile Tab
                        if (!dialogIsOpen())
                            $('#templateEditorTabs a:nth(0)').tab('show');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 79) {       // O: Constraint Tab
                        if (!dialogIsOpen())
                            $('#templateEditorTabs a:nth(1)').tab('show');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 80) {       // P: Preview Tab
                        if (!dialogIsOpen())
                            $('#templateEditorTabs a:nth(2)').tab('show');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 76) {       // L: Validation Tab
                        if (!dialogIsOpen())
                            $('#templateEditorTabs a:nth(3)').tab('show');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 65) {       // A: View Mode - Analyst
                        if (!dialogIsOpen())
                            viewModel.ViewMode('Analyst');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 73) {       // I: View Mode - Editor
                        if (!dialogIsOpen())
                            viewModel.ViewMode('Editor');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    } else if (e.keyCode == 71) {       // G: View Mode - Engineer
                        if (!dialogIsOpen())
                            viewModel.ViewMode('Engineer');
                        e.bubbles = false;
                        e.preventDefault();
                        return false;
                    }
                }

                return true;
            });

            ko.applyBindings(viewModel, $("#TemplateEditor")[0]);
        });
    </script>

</asp:Content>