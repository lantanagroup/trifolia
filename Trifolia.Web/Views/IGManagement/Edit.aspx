<%@ Page Title="Edit Implementation Guide" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int?>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        #EditImplementationGuide .input-group, 
        #EditImplementationGuide .row {
            padding-top: 5px;
        }
    </style>

    <link rel="Stylesheet" type="text/css" href="/Styles/default.min.css" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="EditImplementationGuide">
        <h2><span data-bind="text: EditType">Add/Edit</span> Implementation Guide</h2>
    
        <div data-bind="with: Model, validationOptions: { messagesOnModified: false, insertMessages: false }">
            <!-- Nav tabs -->
            <ul class="nav nav-tabs" style="padding-top: 10px;">
                <li class="active"><a href="#general" data-toggle="tab">General</a></li>
                <li><a href="#templateTypes" data-toggle="tab">Template/Profile Types</a></li>
                <li><a href="#cardinality" data-toggle="tab">Cardinality</a></li>
                <li><a href="#customSchematron" data-toggle="tab">Custom Schematron</a></li>
                <li><a href="#permissions" data-toggle="tab">Permissions</a></li>
                <li><a href="#volume1" data-toggle="tab">Volume 1</a></li>
                <li><a href="#categories" data-toggle="tab">Categories</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="general">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group has-feedback" data-bind="css: { 'has-error': !Validation().Name.isValid() }">
                                <label>Name</label>
                                <input type="text" class="form-control" data-bind="value: Name, disable: PreviousVersionId" />
                                <span class="help-block" data-bind="validationMessage: Name"></span>
                            </div>

                            <div class="form-group has-feedback" data-bind="css: { 'has-error': !Validation().TypeId.isValid() }">
                                <label>Type</label>
                                <select class="form-control" data-bind="value: TypeId, options: $parent.ImplementationGuideTypes, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select', disable: Id"></select>
                                <span class="help-block" data-bind="validationMessage: TypeId"></span>
                            </div>

                            <div class="form-group">
                                <label>Organization</label>
                                <select class="form-control" data-bind="value: OrganizationId, options: $parent.Organizations, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select...'"></select>
                                <span class="help-block">The organization specified here identifies (for example) a standards body that the implementation guide will be developed under.</span>
                            </div>

                            <div class="form-group has-feedback" data-bind="css: { 'has-error': !Validation().Identifier.isValid() }">
                                <label>Identifier/Base URL</label>
                                <input type="text" class="form-control" data-bind="value: Identifier, disable: $parent.DisableIdentifier" />
                            </div>
                            <span data-bind="validationMessage: Identifier"></span>

                            <div class="form-group">
                                <label>Display Name</label>
                                <input type="text" class="form-control" data-bind="value: DisplayName" />
                                <span class="help-block">Display name is used on many of the management screens, and in the DOCX export</span>
                            </div>
                            <span data-bind="validationMessage: DisplayName"></span>

                            <div class="form-group">
                                <label>Web Display Name</label>
                                <input type="text" class="form-control" data-bind="value: WebDisplayName" />
                                <span class="help-block">Web Display Name is used only by the web-based implementation guide, in the home screen's "Welcome to the web-based implementation guide for XXX" heading</span>
                            </div>
                            <span data-bind="validationMessage: WebDisplayName"></span>
            
                            <div class="form-group">
                                <label>Consolidated Format</label>
                                <select class="form-control" data-bind="value: ConsolidatedFormatString">
                                    <option value="true">Yes</option>
                                    <option value="false">No</option>
                                </select>
                            </div>
                            <span data-bind="validationMessage: ConsolidatedFormat"></span>

                            <div class="form-group">
                                <input type="checkbox" data-bind="checked: TestIg" value="false" /> Check if this is a Test IG (can be upgraded to a Draft)
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <label>Access Manager</label>
                                <select class="form-control" data-bind="value: AccessManagerId, options: GetEditUsers(), optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select...'""></select>
                                <span class="help-block">One of the individual users that have edit permissions to the IG. Is the user that receives emails with access requests.</span>
                            </div>

                            <div class="form-group">
                                <label>Allow access requests?</label>
                                <select class="form-control" data-bind="value: AllowAccessRequestsString">
                                    <option value="true">Yes</option>
                                    <option value="false" selected>No</option>
                                </select>
                                <span class="help-block">Indicates whether the implementation guide should be displayed in the list of implementation guides that users can request access to.</span>
                            </div>

                            <div class="form-group">
                                <label>Web Implementation Guide Description</label>
                                <textarea style="width: 100%; height: 300px;" data-bind="sceditor: WebDescription, value: WebDescription, imageOpts: imageOpts"></textarea>
                                <span class="help-block">Web Implementation Guide Description is used only by the web-based implementation guide, in the home screen's "Welcome to the web-based implementation guide for XXX" heading</span>
                            </div>
                            <span data-bind="validationMessage: WebDescription"></span>

                            <div class="form-group">
                                <label>Web Readme Overview</label>
                                <textarea class="form-control" style="height: 100px;" data-bind="value: WebReadmeOverview"></textarea>
                                <span class="help-block">Used in the .txt README file when the web-based implementation guide is downloaded as a ZIP package.</span>
                            </div>
                            <span data-bind="validationMessage: WebReadmeOverview"></span>
                        </div>
                    </div>
                </div>
                <div class="tab-pane" id="templateTypes">
                    <div class="row">
                        <div class="col-md-2"><strong>Name</strong></div>
                        <div class="col-md-3"><strong>Custom Name</strong></div>
                        <div class="col-md-7"><strong>Description</strong></div>
                    </div>
                    <div data-bind="foreach: TemplateTypes">
                        <div class="row">
                            <div class="col-md-2" data-bind="text: DefaultName"></div>
                            <div class="col-md-3">
                                <input type="text" class="form-control" data-bind="value: Name" placeholder="Name (required)" />
                            </div>
                            <div class="col-md-7">
                                <textarea class="form-control" data-bind="value: Description" placeholder="Description" style="height: 75px;"></textarea>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="tab-pane" id="cardinality">
                    <div class="input-group">
                        <div class="input-group-addon">[0..1]</div>
                        <input type="text" class="form-control" data-bind="value: CardinalityZeroOrOne" />
                    </div>
                    <span data-bind="validationMessage: CardinalityZeroOrOne"></span>

                    <div class="input-group">
                        <div class="input-group-addon">[1..1]</div>
                        <input type="text" class="form-control" data-bind="value: CardinalityExactlyOne" />
                    </div>
                    <span data-bind="validationMessage: CardinalityExactlyOne"></span>

                    <div class="input-group">
                        <div class="input-group-addon">[1..*]</div>
                        <input type="text" class="form-control" data-bind="value: CardinalityAtLeastOne" />
                    </div>
                    <span data-bind="validationMessage: CardinalityAtLeastOne"></span>

                    <div class="input-group">
                        <div class="input-group-addon">[0..*]</div>
                        <input type="text" class="form-control" data-bind="value: CardinalityZeroOrMore" />
                    </div>
                    <span data-bind="validationMessage: CardinalityZeroOrMore"></span>

                    <div class="input-group">
                        <div class="input-group-addon">[0..0]</div>
                        <input type="text" class="form-control" data-bind="value: CardinalityZero" />
                    </div>
                    <span data-bind="validationMessage: CardinalityZero"></span>
                </div>

                <div class="tab-pane" id="customSchematron">
                    <div class="row">
                        <div class="col-md-2"><strong>Phase</strong></div>
                        <div class="col-md-2"><strong>Pattern ID</strong></div>
                        <div class="col-md-6"><strong>Rule(s) Definition</strong></div>
                        <div class="col-md-2">
                            <div class="pull-right">
                                <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditCustomSchematron(); }">Add</button>
                            </div>
                        </div>
                    </div>
                    
                    <div data-bind="foreach: CustomSchematrons">
                        <div class="row">
                            <div class="col-md-2" data-bind="text: Phase"></div>
                            <div class="col-md-2" data-bind="text: PatternId"></div>
                            <div class="col-md-6" data-bind="text: PatternContent"></div>
                            <div class="col-md-2">
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditCustomSchematron($data); }">Edit</button>
                                        <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].RemoveCustomSchematron($data); }">Remove</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="tab-pane" id="permissions">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="panel panel-default">
                                <div class="panel-heading">View Permissions</div>
                                <table class="table table-striped">
                                    <tbody data-bind="foreach: ViewPermissions">
                                        <tr>
                                            <td>
                                                <span data-bind="text: Name"></span><span data-bind="text: $parents[1].GetPermissionTypeName(Type())"></span>
                                            </td>
                                            <td>
                                                <div class="pull-right">
                                                    <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[1].RemovePermission('View', $data); }, disable: IsDefault">Remove</button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                            
                            <div class="panel panel-default">
                                <div class="panel-heading">Edit Permissions</div>
                                <table class="table table-striped">
                                    <tbody data-bind="foreach: EditPermissions">
                                        <tr>
                                            <td>
                                                <span data-bind="text: Name"></span><span data-bind="text: $parents[1].GetPermissionTypeName(Type())"></span>
                                            </td>
                                            <td>
                                                <div class="pull-right">
                                                    <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $parents[1].RemovePermission('Edit', $data); }, disable: IsDefault">Remove</button>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="panel panel-default">
                                <div class="panel-heading">Add Permissions</div>
                                <div class="panel-body">
                                    <form id="searchPermissionsForm" data-bind="submit: $parent.SearchPermissions">
                                        <div class="row">
                                            <div class="col-md-12">
                                                <div class="input-group">
                                                    <input type="text" class="form-control" placeholder="User Name" data-bind="value: $parent.PermissionSearchText" />
                                                    <div class="input-group-btn">
                                                        <button type="button" class="btn btn-default" data-bind="click: function () { $parent.PermissionSearchText(''); $parent.SearchPermissions(); }">Clear</button>
                                                        <button type="submit" class="btn btn-default">Search</button>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </form>
                                </div>
                                <!-- ko if: $parent.PermissionSearchResults().length > 0 -->
                                <table class="table table-striped">
                                    <tbody data-bind="foreach: $parent.PermissionSearchResults">
                                        <tr>
                                            <td data-bind="text: Name"></td>
                                            <td>
                                                <div class="pull-right">
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $root.AddPermission('View', $data); }">Add View</button>
                                                        <button type="button" class="btn btn-default btn-sm" data-bind="click: function () { $root.AddPermission('Edit', $data); }">Add View & Edit</button>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                                <!-- /ko -->
                            </div>
                        </div>
                    </div>
                </div>

                <div class="tab-pane" id="volume1">
                    <div class="form-group">
                        <label>Type</label>
                        <select class="form-control" data-bind="value: Volume1Type">
                            <option value="html">HTML Content</option>
                            <option value="defined">Defined Sections</option>
                        </select>
                    </div>

                    <!-- ko if: Volume1Type() == 'html' -->
                    <div class="form-group">
                        <label>Volume 1 HTML</label>
                        <textarea class="form-control" rows="10" data-bind="value: Html"></textarea>
                    </div>
                    <!-- /ko -->
                    
                    <!-- ko if: Volume1Type() == 'defined' -->
                    <div class="row">
                        <div class="col-md-10"><strong>Heading</strong></div>
                        <div class="col-md-2">
                            <div class="pull-right">
                                <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditSection(); }">Add</button>
                            </div>
                        </div>
                    </div>
                    
                    <div data-bind="foreach: Sections" class="row-striped">
                        <div class="row">
                            <div class="col-md-8">
                                <span style="white-space: pre;" data-bind="text: $parents[1].GetSectionPadding($data.Level())"></span>
                                H<span data-bind="text: Level"></span>. 
                                <span data-bind="text: Heading"></span>
                            </div>
                            <div class="col-md-4">
                                <div class="pull-right">
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[1].EditSection($data); }" title="Edit"><i class="glyphicon glyphicon-pencil"></i></button>
                                        <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].RemoveSection($data); }" title="Remove"><i class="glyphicon glyphicon-remove"></i></button>
                                    </div>
                                    <div class="btn-group">
                                        <button type="button" class="btn btn-default" title="Move up" data-bind="enable: $parents[1].CanMoveUp($index()), click: function() { $parents[1].MoveUp($index()); }">
                                            <i class="glyphicon glyphicon-chevron-up"></i>
                                        </button>
                                        <button type="button" class="btn btn-default" title="Move down" data-bind="enable: $parents[1].CanMoveDown($index()), click: function() { $parents[1].MoveDown($index()); }">
                                            <i class="glyphicon glyphicon-chevron-down"></i>
                                        </button>
                                        <button type="button" class="btn btn-default" title="Decrease heading level" data-bind="enable: $parents[1].CanMoveLeft($index()), click: function() { $parents[1].MoveLeft($index()); }">
                                            <i class="glyphicon glyphicon-chevron-left"></i>
                                        </button>
                                        <button type="button" class="btn btn-default" title="Increase heading level" data-bind="enable: $parents[1].CanMoveRight($index()), click: function() { $parents[1].MoveRight($index()); }">
                                            <i class="glyphicon glyphicon-chevron-right"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- /ko -->
                </div>

                <div class="tab-pane" id="categories">
                    <div data-bind="foreach: CategoryModels">
                        <div class="input-group">
                            <input type="text" class="form-control" data-bind="value: Value, valueUpdate: 'keypress'" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default" data-bind="click: function () { $parents[1].RemoveCategory($index()); }">Remove</button>
                            </div>
                        </div>
                        <span class="help-block" data-bind="validationMessage: Value"></span>
                    </div>
                    <div class="input-group">
                        <input type="text" class="form-control" data-bind="value: $parent.NewCategory, valueUpdate: 'keypress'" placeholder="NEW CATEGORY" />
                        <div class="input-group-btn">
                            <button type="button" class="btn btn-default" data-bind="click: $parent.AddCategory, disable: !$parent.NewCategory() || !$parent.NewCategoryValid().isValid()">Add</button>
                        </div>
                    </div>
                    <span class="help-block" data-bind="validationMessage: $parent.NewCategory"></span>
                </div>
            </div>
        </div>

        <div class="form-group">
            <input type="checkbox" data-bind="checked: Model().NotifyNewPermissions" value="true" /> Notify new users and groups that they have been granted permissions
        </div>

        <div class="btn-group" data-bind="with: Model">
            <button type="button" class="btn btn-primary" data-bind="click: $parent.Save, disable: !IsValid()">Save</button>
            <button type="button" class="btn btn-default" data-bind="click: $parent.Cancel">Cancel</button>
        </div>

        <div class="modal fade" id="customSchematronDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Custom Schematron</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentCustomSchematron">
                        <div class="form-group">
                            <label>Phase</label>
                            <select class="form-control" data-bind="value: Phase">
                                <option value="errors">Error</option>
                                <option value="warnings">Warning</option>
                            </select>
                        </div>
                        <div class="form-group">
                            <label>Pattern ID</label>
                            <input type="text" class="form-control" data-bind="value: PatternId" />
                        </div>
                        <div class="form-group">
                            <label>Rule(s) Definition</label>
                            <textarea class="form-control" style="height: 100px;" data-bind="value: PatternContent"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: SaveCustomSchematron">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: CancelEditCustomSchematron">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <div class="modal fade" id="sectionDialog">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Section</h4>
                    </div>
                    <div class="modal-body" data-bind="with: CurrentSection">
                        <div class="form-group">
                            <label>Heading</label>
                            <input type="text" class="form-control" data-bind="value: Heading" />
                        </div>

                        <div class="form-group">
                            <label>Content</label>
                            <textarea style="width: 100%; height: 300px;" data-bind="sceditor: Content, value: Content"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: SaveSection">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: CancelEditSection">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

    </div>
    <!-- /#EditImplementationGuide -->

    <script type="text/javascript" src="/Scripts/IGManagement/Edit.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="/Scripts/lib/sceditor/jquery.sceditor.xhtml.min.js"></script>
    <script type="text/javascript" src="/Scripts/lib/sceditor/sceditor.custom.js"></script>

    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new EditImplementationGuideViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#EditImplementationGuide')[0]);

            $('#customSchematronDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });

            $('#sectionDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
        });
    </script>

</asp:Content>
