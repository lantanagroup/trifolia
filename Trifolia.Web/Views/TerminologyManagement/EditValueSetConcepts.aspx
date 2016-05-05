<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="EditValueSetConcepts">
        <h2>Edit <span data-bind="text: ValueSetName"></span> Concepts</h2>
        
        <div class="input-group">
            <div class="input-group-addon">Search</div>
            <input type="text" class="form-control" data-bind="value: SearchQuery" />
            <div class="input-group-btn">
                <button type="button" class="btn btn-default" data-bind="click: function() { SearchQuery(''); Search(); }"><i class="glyphicon glyphicon-remove"></i></button>
                <button type="button" class="btn btn-default" data-bind="click: Search">Search</button>
            </div>
        </div>

        <!-- ko if: LastPage() > 1 -->
        <p>
            <div class="btn-group">
                <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: GoToFirstPage" title="First Page"><i class="glyphicon glyphicon-fast-backward"></i></button>
                <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: PreviousPage" title="Previous Page"><i class="glyphicon glyphicon-backward"></i></button>
                <button type="button" class="btn btn-default" disabled>Page <span data-bind="text: Page"></span> of <span data-bind="text: LastPage"></span></button>
                <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: NextPage" title="Next Page"><i class="glyphicon glyphicon-forward"></i></button>
                <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: GoToLastPage" title="Last Page"><i class="glyphicon glyphicon-fast-forward"></i></button>
            </div>
        </p>
        <!-- /ko -->

        <!-- ko if: !Loading() -->
        <table class="table">
            <thead>
                <tr>
                    <th>Code</th>
                    <th>Display Name</th>
                    <th style="width: 200px;">Code System</th>
                    <th style="width: 200px;">Status</th>
                    <th style="width: 300px;">Date</th>
                    <th></th>
                </tr>
            </thead>
            <tbody data-bind="foreach: Concepts">
                <!-- ko if: !IsEditing() -->
                <tr>
                    <td data-bind="text: Code"></td>
                    <td data-bind="text: DisplayName"></td>
                    <td data-bind="text: $parents[$parents.length-1].GetCodeSystemName(CodeSystemId())"></td>
                    <td data-bind="text: Status"></td>
                    <td data-bind="text: StatusDate"></td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-primary" data-bind="click: Edit">Edit</button>
                            <button type="button" class="btn btn-default" data-bind="click: function() { $parent.Remove($data); }">Remove</button>
                        </div>
                    </td>
                </tr>
                <!-- /ko -->
                <!-- ko if: IsEditing() -->
                <tr>
                    <td><input type="text" class="form-control" data-bind="value: Code" /></td>
                    <td><input type="text" class="form-control" data-bind="value: DisplayName" /></td>
                    <td>
                        <select class="form-control" data-bind="options: $parent.CodeSystems, optionsText: 'Name', optionsValue: 'Id', value: CodeSystemId, optionsCaption: 'Choose...'">
                        </select>
                    </td>
                    <td>
                        <select class="form-control" data-bind="value: Status">
                            <option value="">Choose...</option>
                            <option>active</option>
                            <option>inactive</option>
                        </select>
                    </td>
                    <td>
                        <div class="input-group date" data-bind="date: StatusDate">
                            <div class="input-group-addon">Status Date</div>
                            <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                            </div>
                        </div>
                    </td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-primary" data-bind="enable: IsValid(), click: function() { $parent.SaveConcept($data); }">OK</button>
                            <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
                        </div>
                    </td>
                </tr>
                <!-- /ko -->
            </tbody>
            <tfoot data-bind="with: NewConcept">
                <tr>
                    <td><input type="text" class="form-control" data-bind="value: Code" /></td>
                    <td><input type="text" class="form-control" data-bind="value: DisplayName" /></td>
                    <td>
                        <select class="form-control" data-bind="options: $parent.CodeSystems, optionsText: 'Name', optionsValue: 'Id', value: CodeSystemId, optionsCaption: 'Choose...'">
                        </select>
                    </td>
                    <td>
                        <select class="form-control" data-bind="value: Status">
                            <option value="">Choose...</option>
                            <option>active</option>
                            <option>inactive</option>
                        </select>
                    </td>
                    <td>
                        <div class="input-group date" data-bind="date: StatusDate">
                            <div class="input-group-addon">Status Date</div>
                            <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                            </div>
                        </div>
                    </td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-primary" data-bind="enable: IsValid, click: $parent.Add">Add</button>
                        </div>
                    </td>
                </tr>
            </tfoot>
        </table>
        <!-- /ko -->
        
        <!-- ko if: ModifiedConcepts().length > 0 -->
        <h3>Pending Changes</h3>
        <table class="table">
            <thead>
                <tr>
                    <th>Code</th>
                    <th>Display Name</th>
                    <th style="width: 200px;">Code System</th>
                    <th style="width: 200px;">Status</th>
                    <th style="width: 300px;">Date</th>
                    <th></th>
                </tr>
            </thead>
            <tbody data-bind="foreach: ModifiedConcepts">
                <!-- ko if: !IsEditing() -->
                <tr>
                    <td>
                        <span data-bind="text: Code"></span>
                        <span class="badge" data-bind="if: !Id()">NEW</span>
                        <span class="badge" data-bind="if: Id()">CHANGED</span>
                    </td>
                    <td data-bind="text: DisplayName"></td>
                    <td data-bind="text: $parents[$parents.length-1].GetCodeSystemName(CodeSystemId())"></td>
                    <td data-bind="text: Status"></td>
                    <td data-bind="text: StatusDate"></td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-primary" data-bind="click: Edit">Edit</button>
                            <button type="button" class="btn btn-default" data-bind="click: function() { $parent.Remove($data, true); }">Remove</button>
                        </div>
                    </td>
                </tr>
                <!-- /ko -->
                <!-- ko if: IsEditing() -->
                <tr>
                    <td><input type="text" class="form-control" data-bind="value: Code" /></td>
                    <td><input type="text" class="form-control" data-bind="value: DisplayName" /></td>
                    <td>
                        <select class="form-control" data-bind="options: $parent.CodeSystems, optionsText: 'Name', optionsValue: 'Id', value: CodeSystemId, optionsCaption: 'Choose...'">
                        </select>
                    </td>
                    <td>
                        <select class="form-control" data-bind="value: Status">
                            <option value="">Choose...</option>
                            <option>active</option>
                            <option>inactive</option>
                        </select>
                    </td>
                    <td>
                        <div class="input-group date" data-bind="date: StatusDate">
                            <div class="input-group-addon">Status Date</div>
                            <input type="text" class="form-control" name="publishDate" placeholder="MM/DD/YYYY" />
                            <div class="input-group-btn">
                                <button type="button" class="btn btn-default"><i class="glyphicon glyphicon-calendar"></i></button>
                            </div>
                        </div>
                    </td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-primary" data-bind="enable: IsValid(), click: function() { $parent.SaveConcept($data); }">OK</button>
                            <button type="button" class="btn btn-default" data-bind="click: Cancel">Cancel</button>
                        </div>
                    </td>
                </tr>
                <!-- /ko -->
            </tbody>
        </table>
        <!-- /ko -->
        
        <!-- ko if: RemovedConcepts().length > 0 -->
        <h3>Pending Removal</h3>
        <table class="table">
            <thead>
                <tr>
                    <th>Code</th>
                    <th>Display Name</th>
                    <th>Code System</th>
                    <th>Status</th>
                    <th style="width: 300px;">Date</th>
                    <th></th>
                </tr>
            </thead>
            <tbody data-bind="foreach: RemovedConcepts">
                <tr>
                    <td data-bind="text: Code"></td>
                    <td data-bind="text: DisplayName"></td>
                    <td data-bind="text: $parents[$parents.length-1].GetCodeSystemName(CodeSystemId())"></td>
                    <td data-bind="text: Status"></td>
                    <td data-bind="text: StatusDate"></td>
                    <td style="width: 150px;">
                        <div class="btn-group pull-right">
                            <button type="button" class="btn btn-default" data-bind="click: function() { $parent.UndoRemove($index()); }">Undo</button>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        <!-- /ko -->
        
        <!-- ko if: LastPage() > 1 -->
        <div class="btn-group">
            <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: GoToFirstPage" title="First Page"><i class="glyphicon glyphicon-fast-backward"></i></button>
            <button type="button" class="btn btn-default" data-bind="enable: Page() != 0, click: PreviousPage" title="Previous Page"><i class="glyphicon glyphicon-backward"></i></button>
            <button type="button" class="btn btn-default" disabled>Page <span data-bind="text: Page"></span> of <span data-bind="text: LastPage"></span></button>
            <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: NextPage" title="Next Page"><i class="glyphicon glyphicon-forward"></i></button>
            <button type="button" class="btn btn-default" data-bind="enable: Page() != LastPage(), click: GoToLastPage" title="Last Page"><i class="glyphicon glyphicon-fast-forward"></i></button>
        </div>
        <!-- /ko -->

        <p>
            <div class="btn-group">
                <button type="button" class="btn btn-primary" data-bind="click: Save">Save</button>
                <a href="/TerminologyManagement/Browse" class="btn btn-default">Cancel</a>
            </div>
        </p>
    </div>

    <script type="text/javascript" src="/Scripts/TerminologyManagement/EditValueSetConcepts.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = new EditValueSetConceptsViewModel(<%= Model %>);
        ko.applyBindings(viewModel, $("#EditValueSetConcepts")[0]);
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>
