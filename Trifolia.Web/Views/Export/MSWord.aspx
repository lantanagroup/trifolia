<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<int>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .dataTableFooter
        {
            background-color: #E4ECD4;
            padding: 7px;
        }
    </style>

    <script type="text/javascript" src="../../Scripts/Export/MSWord.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <form id="ExportMSWordForm" method="post">
        <div id="ExportMSWord">
            <h2>Export Templates/Profiles to MS Word</h2>
            <h3><a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }, text: Name"></a></h3>

            <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />

            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#content" data-toggle="tab">Content</a></li>
                <li><a href="#valuesets" data-toggle="tab">Value Sets</a></li>
                <li><a href="#templates" data-toggle="tab">Templates/Profiles</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="content">
                    <!-- ko if: Categories().length > 0 -->
                    <div class="form-group">
                        <label>Category</label> (Hold CTRL and click to select multiple)
                        <select class="form-control" name="SelectedCategories" multiple="multiple" data-bind="options: Categories, selectedOptions: SelectedCategories"></select>
                    </div>
                    <!-- /ko -->

                    <div class="form-group">
                        <label>Sort Order</label>
                        <select class="form-control" name="TemplateSortOrder">
                            <option value="0" selected="selected">Alphabetically</option>
                            <option value="1">Alpha-Hierarchical</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Document Tables</label>
                        <select class="form-control" name="DocumentTables" data-bind="value: DocumentTables">
                            <option value="0">None</option>
                            <option value="1">Both</option>
                            <option value="2">List</option>
                            <option value="3">Containment</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Template/Profile Tables</label>
                        <select class="form-control" name="TemplateTables" data-bind="value: TemplateTables">
                            <option value="0">None</option>
                            <option value="1">Both</option>
                            <option value="2">Context</option>
                            <option value="3">Constraint Overview</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>XML Samples</label>
                        <input type="checkbox" id="IncludeXmlSample" name="IncludeXmlSample" value="true" data-bind="checked: IncludeXmlSample" /> Include
                    </div>

                    <div class="form-group">
                        <label>Change List</label>
                        <input type="checkbox" id="IncludeChangeList" name="IncludeChangeList" value="true" data-bind="checked: IncludeChangeList" /> Include
                    </div>

                    <div class="form-group">
                        <label>Publish Settings</label>
                        <input type="checkbox" id="IncludeTemplateStatus" name="IncludeTemplateStatus" value="true" data-bind="checked: IncludeTemplateStatus" /> Include
                    </div>

                    <div class="form-group" data-bind="if: CanEdit()">
                        <label>Notes</label>
                        <input type="checkbox" id="IncludeNotes" name="IncludeNotes" value="true" data-bind="checked: IncludeNotes" /> Include
                    </div>
                </div>
                <div class="tab-pane" id="valuesets">
                    <div class="form-group">
                        <label>Tables?</label>
                        <input type="radio" name="GenerateValuesets" value="true" data-bind="checked: GenerateValuesetsString" />Yes <input type="radio" name="GenerateValuesets" value="false" data-bind="checked: GenerateValuesetsString" /> No
                    </div>
                
                    <div class="form-group">
                        <label>Maximum Members <i title="Indicates the maximum number of members that should be exported for a given value set. 0 indicates that no members should be exported, and will omit the valueset tables from the export entirely." data-bind="helpTooltip: {}"></i></label>
                        <div class="input-group">
                            <input type="text" class="form-control" id="MaximumValuesetMembers" name="MaximumValuesetMembers" value="10" data-bind="spinedit: MaximumValuesetMembers, enable: GenerateValuesets">
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Create as appendix</label>
                        <input type="radio" name="ValuesetAppendix" value="true" data-bind="checked: ValuesetAppendixString" /> Yes <input type="radio" name="ValuesetAppendix" value="false" data-bind="    checked: ValuesetAppendixString" /> No
                    </div>

                    <table class="table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Identifier</th>
                                <th>Binding Date</th>
                                <th style="width: 100px;">Max Members</th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: ValueSets">
                            <tr>
                                <td data-bind="text: Name"></td>
                                <td data-bind="text: Oid"></td>
                                <td data-bind="text: BindingDate"></td>
                                <td>
                                    <div class="input-group">
                                        <input type="hidden" id="ValueSetOid" name="ValueSetOid" data-bind="value: Oid" />
                                        <input type="text" id="ValueSetMaxMembers" name="ValueSetMaxMembers" class="form-control" data-bind="spinedit: MaxMembers" />
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                        <tfoot>
                            <!-- ko if: ValueSets().length == 0 -->
                            <tr>
                                <td colspan="4">No Value Sets</td>
                            </tr>
                            <!-- /ko -->
                            <!-- ko if: ValueSets().length != 0 -->
                            <tr>
                                <td colspan="4"><b>Total Value Sets: <span data-bind="text: ValueSets().length" /></b></td>
                            </tr>
                            <!-- /ko -->
                        </tfoot>
                    </table>
                </div>
                <div class="tab-pane" id="templates">
                    <div class="form-group">
                        <label>Parent Templates/Profiles</label>
                        <!-- ko foreach: ParentTemplateIds -->

                        <!-- ko if: Id() -->
                        <input type="hidden" name="ParentTemplateIds" data-bind="value: Id" />
                        <!-- /ko -->

                        <div data-bind="templateSelect: Id, filterImplementationGuideId: $parent.ImplementationGuideId"></div>
                        <!-- /ko -->
                    </div>

                    <div class="form-group">
                        <label>Include Inferred?</label>
                        <input type="radio" name="Inferred" value="true" checked="checked" data-bind="checked: IncludeInferred.ForBinding" /> Yes <input type="radio" name="Inferred" value="false" data-bind="    checked: IncludeInferred.ForBinding" /> No
                    </div>

                    <table class="table">
                        <thead>
                            <tr>
                                <th><input type="checkbox" id="CheckAllTemplates" data-bind="checked: AllTemplatesChecked" /></th>
                                <th>Name</th>
                                <th>Identifier</th>
                                <th>This IG?</th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: Templates">
                            <tr>
                                <td><input type="checkbox" name="TemplateIds" class="templateIdCheckboxes" data-bind="value: Id, checked: $parent.TemplateChecked(Id)" /></td>
                                <td data-bind="text: Name"></td>
                                <td data-bind="text: Oid"></td>
                                <td data-bind="text: (ThisIG() ? 'Yes' : 'No')"></td>
                            </tr>
                        </tbody>
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

            <div class="form-group" data-bind="if: CanEdit()">
                <input type="checkbox" id="SaveAsDefaultSettings" name="SaveAsDefaultSettings" value="true" data-bind="checked: SaveSettings" /> Save as default settings
            </div>

            <button type="button" class="btn btn-primary" data-bind="click: Export">Export</button>
        </div>
    </form>

    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ExportMSWordViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#ExportMSWord')[0]);
        });
    </script>
</asp:Content>
