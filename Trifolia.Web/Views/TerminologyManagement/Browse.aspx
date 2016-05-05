<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="BrowseTerminology">
        <h3>Browse Terminology</h3>
        <!-- Nav tabs -->
        <ul class="nav nav-tabs" id="tabs">
            <!-- ko if: containerViewModel.HasSecurable(['ValueSetList']) -->
            <li>
                <a href="#valuesets" data-toggle="tab">Value Sets</a>
            </li>
            <!-- /ko -->
            <!-- ko if: containerViewModel.HasSecurable(['CodeSystemList']) -->
            <li>
                <a href="#codesystems" data-toggle="tab">Code Systems</a>
            </li>
            <!-- /ko -->
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
            <!-- ko if: containerViewModel.HasSecurable(['ValueSetList']) -->
            <div class="tab-pane" id="valuesets" data-bind="with: ValueSetResults">
                <form id="ValueSetSearchForm">
                    <div class="input-group" style="padding-bottom: 10px;">
                        <input type="text" class="form-control" data-bind="value: $parent.ValueSetQuery, valueUpdate: 'keyup', event: { 'keypress': $parent.HandleValueSetSearchKeyPress }" />
                        <span class="input-group-btn">
                            <button class="btn btn-default" type="button" data-bind="click: function () { $parent.ValueSetQuery(''); $parent.SearchValueSets(); }">
                                <i class="glyphicon glyphicon-remove"></i>
                            </button>
                            <button class="btn btn-default" type="submit" data-bind="click: $parent.SearchValueSets">Search</button>
                        </span>
                    </div>
                </form>

                <div class="row">
                    <div class="col-md-8">
                        <div data-bind="template: { name: 'valueSetPageNavigation' }"></div>
                    </div>
                    <div class="col-md-4">
                        <div class="pull-right">
                            <!-- ko if: containerViewModel.HasSecurable(['ValueSetEdit']) -->
                            <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditValueSet(); }">Add Value Set</button>
                            <!-- /ko -->
                        </div>
                    </div>
                </div>

                <table class="table table-striped" data-bind="highlight: $parent.ValueSetQuery">
                    <thead>
                        <tr>
                            <th data-bind="click: function () { $parent.ToggleValueSetSort('Name'); }" style="cursor: pointer;">
                                Name
                                <!-- ko if: $parent.ValueSetSort() == 'Name' && $parent.ValueSetOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.ValueSetSort() == 'Name' && $parent.ValueSetOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                            <th data-bind="click: function () { $parent.ToggleValueSetSort('Oid'); }" style="cursor: pointer;">
                                Identifier
                                <!-- ko if: $parent.ValueSetSort() == 'Oid' && $parent.ValueSetOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.ValueSetSort() == 'Oid' && $parent.ValueSetOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                            <th data-bind="click: function () { $parent.ToggleValueSetSort('IsComplete'); }" style="cursor: pointer;">
                                Complete?
                                <!-- ko if: $parent.ValueSetSort() == 'IsComplete' && $parent.ValueSetOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.ValueSetSort() == 'IsComplete' && $parent.ValueSetOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: Items">
                        <tr>
                            <td>
                                <div class="dropdown">
                                    <a href="#" class="dropdown-toggle" role="menu" data-toggle="dropdown"><span data-bind="text: Name"></span> <span class="caret"></span></a>
                                    <!-- ko if: IsPublished() -->
                                    <i class="glyphicon glyphicon-exclamation-sign" title="This value set is used by a published implementation guide!"></i>
                                    <!-- /ko -->
                                    <ul class="dropdown-menu">
                                        <li><a href="#" data-bind="attr: { href: '/TerminologyManagement/ValueSet/View/' + Id() }">View</a></li>
                                        <li data-bind="css: { disabled: !PermitModify() || !(CanModify() || CanOverride()) }"><a href="#" data-bind="click: function () { $parents[$parents.length - 1].EditValueSet($data); }">Edit Value Set</a></li>
                                        <li data-bind="css: { disabled: !PermitModify() || !(CanModify() || CanOverride()) }"><a href="#" data-bind="click: function () { $parents[$parents.length - 1].EditValueSetConcepts($data); }">Edit Concepts</a></li>
                                        <li data-bind="css: { disabled: !PermitModify() || !(CanModify() || CanOverride()) }"><a href="#" data-bind="click: function () { $parents[$parents.length - 1].RemoveValueSet($data); }">Remove</a></li>
                                    </ul>
                                </div>
                            </td>
                            <td data-bind="text: Oid"></td>
                            <td data-bind="text: (IsComplete() ? 'Yes' : 'No')"></td>
                        </tr>
                    </tbody>
                </table>

                <div data-bind="template: { name: 'valueSetPageNavigation' }"></div>
            </div>
            <!-- /ko -->
            
            <!-- ko if: containerViewModel.HasSecurable(['CodeSystemList']) -->
            <div class="tab-pane" id="codesystems" data-bind="with: CodeSystemResults">
                <form id="CodeSystemSearchForm">
                    <div class="input-group" style="padding-bottom: 10px;">
                        <input type="text" class="form-control" data-bind="value: $parent.CodeSystemQuery, valueUpdate: 'keyup', event: { 'keypress': $parent.HandleCodeSystemSearchKeyPress }">
                        <span class="input-group-btn">
                            <button class="btn btn-default" type="button" data-bind="click: function () { $parent.CodeSystemQuery(''); $parent.SearchCodeSystems(); }">
                                <i class="glyphicon glyphicon-remove"></i>
                            </button>
                            <button class="btn btn-default" type="submit" data-bind="click: $parent.SearchCodeSystems">Search</button>
                        </span>
                    </div>
                </form>
                
                <div class="row">
                    <div class="col-md-8">
                        <div data-bind="template: { name: 'codeSystemPageNavigation' }"></div>
                    </div>
                    <div class="col-md-4">
                        <div class="pull-right">
                            <!-- ko if: containerViewModel.HasSecurable(['CodeSystemEdit']) -->
                            <button type="button" class="btn btn-primary" data-bind="click: function () { $parent.EditCodeSystem(); }">Add Code System</button>
                            <!-- /ko -->
                        </div>
                    </div>
                </div>

                <table class="table table-striped" data-bind="highlight: $parent.CodeSystemQuery">
                    <thead>
                        <tr>
                            <th data-bind="click: function () { $parent.ToggleCodeSystemSort('Name'); }" style="cursor: pointer;">
                                Name
                                <!-- ko if: $parent.CodeSystemSort() == 'Name' && $parent.CodeSystemOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.CodeSystemSort() == 'Name' && $parent.CodeSystemOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                            <th data-bind="click: function () { $parent.ToggleCodeSystemSort('Oid'); }" style="cursor: pointer;">
                                Identifier
                                <!-- ko if: $parent.CodeSystemSort() == 'Oid' && $parent.CodeSystemOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.CodeSystemSort() == 'Oid' && $parent.CodeSystemOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                            <th data-bind="click: function () { $parent.ToggleCodeSystemSort('MemberCount'); }" style="cursor: pointer;">
                                #/Members
                                <!-- ko if: $parent.CodeSystemSort() == 'MemberCount' && $parent.CodeSystemOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.CodeSystemSort() == 'MemberCount' && $parent.CodeSystemOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                            <th data-bind="click: function () { $parent.ToggleCodeSystemSort('ConstraintCount'); }" style="cursor: pointer;">
                                #/Constraints
                                <!-- ko if: $parent.CodeSystemSort() == 'ConstraintCount' && $parent.CodeSystemOrder() == 'desc' -->
                                <i class="glyphicon glyphicon-chevron-down"></i>
                                <!-- /ko -->
                                <!-- ko if: $parent.CodeSystemSort() == 'ConstraintCount' && $parent.CodeSystemOrder() != 'desc' -->
                                <i class="glyphicon glyphicon-chevron-up"></i>
                                <!-- /ko -->
                            </th>
                        </tr>
                    </thead>
                    <tbody data-bind="foreach: rows">
                        <tr>
                            <td>
                                <div class="dropdown">
                                    <a href="#" class="dropdown-toggle" role="menu" data-toggle="dropdown"><span data-bind="text: Name"></span> <span class="caret"></span></a>
                                    <!-- ko if: IsPublished() -->
                                    <i class="glyphicon glyphicon-exclamation-sign" title="This code system is used by a published implementation guide!"></i>
                                    <!-- /ko -->
                                    <ul class="dropdown-menu">
                                        <li data-bind="css: { disabled: !PermitModify() || !(CanModify() || CanOverride()) }"><a href="#" data-bind="click: function () { $parents[$parents.length - 1].EditCodeSystem($data); }">Edit</a></li>
                                        <li data-bind="css: { disabled: !PermitModify() || !(CanModify() || CanOverride()) }"><a href="#" data-bind="click: function () { $parents[$parents.length - 1].RemoveCodeSystem($data); }">Remove</a></li>
                                    </ul>
                                </div>
                            </td>
                            <td data-bind="text: Oid"></td>
                            <td data-bind="text: MemberCount"></td>
                            <td data-bind="text: ConstraintCount"></td>
                        </tr>
                    </tbody>
                </table>

                <div data-bind="template: { name: 'codeSystemPageNavigation' }"></div>
            </div>
            <!-- /ko -->
        </div>

        <!-- Edit Value Set Dialog -->
        <div class="modal fade" id="editValueSetDialog">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Value Set</h4>
                    </div>
                    <div class="modal-body" style="max-height: 350px; overflow-y: scroll;" data-bind="with: CurrentValueSet, validationOptions: { insertMessages: false, messagesOnModified: false }">
                        <div class="form-group">
                            <label for="valueSetName">Name</label>
                            <input type="text" class="form-control" id="valueSetName" data-bind="value: Name" maxlength="255" />
                            <span data-bind="validationMessage: Name"></span>
                        </div>
                        
                        <div class="form-group">
                            <label for="valueSetOid">Identifier</label>
                            <input type="text" class="form-control" id="valueSetOid" data-bind="value: Oid" maxlength="255" />
                            <span data-bind="validationMessage: Oid"></span>
                        </div>
                        
                        <div class="form-group">
                            <label for="valueSetCode">Code</label>
                            <input type="text" class="form-control" id="valueSetCode" data-bind="value: Code" maxlength="255" />
                            <span data-bind="validationMessage: Code"></span>
                        </div>
                        
                        <div class="form-group">
                            <label for="valueSetDescription">Description</label>
                            <textarea class="form-control" id="valueSetDescription" data-bind="value: Description" style="height: 50px;"></textarea>
                            <span data-bind="validationMessage: Description"></span>
                        </div>
                        
                        <div class="form-group">
                            <input type="checkbox" id="valueSetIntentional" data-bind="checked: IsIntentional" />
                            <label for="valueSetIntentional">Intentional</label>
                            <span data-bind="validationMessage: IsIntentional"></span>
                        </div>
                        
                        <div class="form-group" data-bind="visible: IsIntentional">
                            <label for="valueSetIntentionalDefinition">Intentional Definition</label>
                            <textarea class="form-control" id="valueSetIntentionalDefinition" data-bind="value: IntentionalDefinition"></textarea>
                            <span data-bind="validationMessage: IntentionalDefinition"></span>
                        </div>
                        
                        <div class="form-group">
                            <input type="checkbox" id="valueSetIncomplete" data-bind="checked: IsComplete" />
                            <label for="valueSetIncomplete">Complete?</label>
                            <span data-bind="validationMessage: IsComplete"></span>
                        </div>
                        
                        <div class="form-group">
                            <label for="valueSetSourceUrl">Source URL</label>
                            <input type="text" class="form-control" id="valueSetSourceUrl" data-bind="value: SourceUrl" maxlength="255" />
                            <span data-bind="validationMessage: SourceUrl"></span>
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: CurrentValueSet">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveValueSet, enable: IsValid">Save</button>
                        <button type="button" class="btn btn-default" data-dismiss="modal" data-bind="click: $parent.CancelEditValueSet">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <!-- Edit Code System Dialog -->
        <div class="modal fade" id="editCodeSystemDialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Code System</h4>
                    </div>
                    <div class="modal-body" style="max-height: 350px; overflow-y: auto;" data-bind="with: CurrentCodeSystem, validationOptions: { insertMessages: false, messagesOnModified: false }">
                        <div class="form-group">
                            <label for="codeSystemName">Name</label>
                            <input type="text" class="form-control" id="codeSystemName" data-bind="value: Name" maxlength="255" />
                            <span data-bind="validationMessage: Name"></span>
                        </div>
                        
                        <div class="form-group">
                            <label for="codeSystemOid">Identifier</label>
                            <input type="text" class="form-control" id="codeSystemOid" data-bind="value: Oid" maxlength="255" />
                            <span data-bind="validationMessage: Oid"></span>
                        </div>
                        
                        <div class="form-group">
                            <label>Description</label>
                            <textarea class="form-control" style="height: 100px;" data-bind="value: Description"></textarea>
                            <span data-bind="validationMessage: Description"></span>
                        </div>
                    </div>
                    <div class="modal-footer" data-bind="with: CurrentCodeSystem">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveCodeSystem, enable: IsValid">OK</button>
                        <button type="button" class="btn btn-default" data-dismiss="modal" data-bind="click: $parent.CancelEditCodeSystem">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->
    </div>

    <script type="text/html" id="valueSetPageNavigation">
        Page <span data-bind="text: $parent.ValueSetPage"></span> of <span data-bind="text: $parent.ValueSetTotalPages"></span>, <span data-bind="text: TotalItems()"></span> value sets
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.ValueSetFirstPage, disable: $parent.ValueSetPage() == 1" title="First Page">
                <i class="glyphicon glyphicon-fast-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.ValueSetPreviousPage, disable: $parent.ValueSetPage() == 1" title="Previous Page">
                <i class="glyphicon glyphicon-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.ValueSetNextPage, disable: $parent.ValueSetPage() == $parent.ValueSetTotalPages()" title="Next Page">
                <i class="glyphicon glyphicon-forward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.ValueSetLastPage, disable: $parent.ValueSetPage() == $parent.ValueSetTotalPages()" title="Last Page">
                <i class="glyphicon glyphicon-fast-forward"></i>
            </button>
        </div>
    </script>

    <script type="text/html" id="codeSystemPageNavigation">
        Page <span data-bind="text: $parent.CodeSystemPage"></span> of <span data-bind="text: $parent.CodeSystemTotalPages"></span>, <span data-bind="text: total()"></span> code systems
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.CodeSystemFirstPage, disable: $parent.CodeSystemPage() == 1" title="First Page">
                <i class="glyphicon glyphicon-fast-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.CodeSystemPreviousPage, disable: $parent.CodeSystemPage() == 1" title="Previous Page">
                <i class="glyphicon glyphicon-backward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.CodeSystemNextPage, disable: $parent.CodeSystemPage() == $parent.CodeSystemTotalPages()" title="Next Page">
                <i class="glyphicon glyphicon-forward"></i>
            </button>
            <button type="button" class="btn btn-default btn-sm" data-bind="click: $parent.CodeSystemLastPage, disable: $parent.CodeSystemPage() == $parent.CodeSystemTotalPages()" title="Last Page">
                <i class="glyphicon glyphicon-fast-forward"></i>
            </button>
        </div>
    </script>

    <script type="text/javascript" src="/Scripts/TerminologyManagement/Browse.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new BrowseTerminologyViewModel();
            ko.applyBindings(viewModel, $("#BrowseTerminology")[0]);

            $('#tabs a:first').tab('show');

            $('#editValueSetDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            }).
                on('keydown', function (e) {
                    if (e.keyCode == 27) {
                        viewModel.CurrentValueSet(null);
                        $('#editValueSetDialog').modal('hide');
                    }
                });
            $('#editCodeSystemDialog').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            }).
                on('keydown', function (e) {
                    if (e.keyCode == 27) {
                        viewModel.CurrentCodeSystem(null);
                        $('#editCodeSystemDialog').modal('hide');
                    }
                });
        });
    </script>
</asp:Content>
