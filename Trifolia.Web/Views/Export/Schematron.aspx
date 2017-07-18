<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <form id="ExportSchematronForm" method="post">
        <div id="ExportSchematronDiv">
            <h2>Export Schematron</h2>
            <h3><a data-bind="attr: { href: ViewImplementationGuideLink }, text: Name"></a></h3>

            <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />
            <input type="hidden" name="CancelUrl" data-bind="value: CancelUrl" />
    
            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#options" data-toggle="tab">Options</a></li>
                <li><a href="#templates" data-toggle="tab">Templates/Profiles</a></li>
            </ul>
    
            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="options">
                    <!-- ko if: Categories().length > 0 -->
                    <div class="form-group">
                        <label>Category</label> (Hold CTRL and click to select multiple)
                        <select class="form-control" name="SelectedCategories" multiple="multiple" data-bind="options: Categories, selectedOptions: SelectedCategories"></select>
                    </div>
                    <!-- /ko -->

                    <div class="form-group">
                        <label>Value Set Format</label>
                        <select class="form-control" name="ValueSetOutputFormat">
                            <option value="1">Default</option>
                            <option value="2">SVS (Multiple ValueSet per file)</option>
                            <option value="3">SVS (Single ValueSet per file)</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Value Set File Name</label>
                        <input type="text" class="form-control" name="VocabularyFileName" value="voc.xml" />
                    </div>

                    <div class="form-group">
                        <label>Include Vocabulary</label>
                        <input type="radio" name="IncludeVocabulary" value="true" /> Yes <input type="radio" name="IncludeVocabulary" value="false" checked /> No
                    </div>

                    <div class="form-group">
                        <label>Include Custom Schematron</label>
                        <input type="radio" name="IncludeCustomSchematron" value="true" checked /> Yes <input type="radio" name="IncludeCustomSchematron" value="false" /> No
                    </div>

                    <div class="form-group">
                        <label>Default Schematron</label>
                        <input type="text" name="DefaultSchematron" value="not(.)" class="form-control" />
                    </div>
                </div>

                <div class="tab-pane" id="templates">
                    <div class="form-group">
                        <label>Include Inferred?</label>
                        <input type="radio" name="Inferred" value="true" checked="checked" data-bind="checked: IncludeInferred.ForBinding" /> Yes <input type="radio" name="Inferred" value="false" data-bind="checked: IncludeInferred.ForBinding" /> No
                    </div>

                    <table class="table">
                        <thead>
                            <tr>
                                <th><input type="checkbox" id="CheckAllTemplates" value="1" data-bind="checked: CheckAllTemplates" /></th>
                                <th style="cursor: pointer;" data-bind="click: function () { SortTemplates('Name'); }">
                                    Name
                                    <!-- ko if: SortProperty() == 'Name' -->
                                    <i class="glyphicon glyphicon-arrow-down"></i>
                                    <!-- /ko -->
                                </th>
                                <th style="cursor: pointer;" data-bind="click: function () { SortTemplates('OID'); }">
                                    Identifier
                                    <!-- ko if: SortProperty() == 'OID' -->
                                    <i class="glyphicon glyphicon-arrow-down"></i>
                                    <!-- /ko -->
                                </th>
                                <th style="cursor: pointer;" data-bind="click: function () { SortTemplates('ThisIG'); }">
                                    This IG?
                                    <!-- ko if: SortProperty() == 'ThisIG' -->
                                    <i class="glyphicon glyphicon-arrow-down"></i>
                                    <!-- /ko -->
                                </th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: Templates">
                            <tr>
                                <td><input type="checkbox" name="TemplateIds" class="templateCheck" data-bind="attr: { value: Id }, checked: IsSelected" /></td>
                                <td data-bind="text: Name"></td>
                                <td data-bind="text: Oid"></td>
                                <td data-bind="text: (ThisIG() ? 'Yes' : 'No')"></td>
                            </tr>
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan="4" data-bind="text: TotalTemplates"></td>
                            </tr>
                        </tfoot>
                        <tfoot>
                            <!-- ko if: Templates().length == 0 -->
                            <tr>
                                <td colspan="4">No Templates/Profiles</td>
                            </tr>
                            <!-- /ko -->
                            <!-- ko if: Templates().length != 0 -->
                            <tr>
                                <td colspan="4"><b>Total Templates/Profiles: <span data-bind="text: Templates().length" /></b></td>
                            </tr>
                            <!-- /ko -->
                        </tfoot>
                    </table>
                </div>
            </div>
    
            <div class="btn-group">
                <button class="btn btn-primary" type="button" id="ExportButton" data-bind="click: Export">Export</button>
                <button class="btn btn-default" type="button" id="CancelButton" data-bind="click: Cancel">Cancel</button>
            </div>
        </div>
</form>
    
    <script type="text/javascript" src="../../Scripts/Export/Schematron.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ExportSchematronViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $("#ExportSchematronDiv")[0]);
        });
    </script>
</asp:Content>
