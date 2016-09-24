<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .dataTableFooter
        {
            background-color: #E4ECD4;
            padding: 7px;
        }
    </style>
    <script type="text/javascript" src="../../Scripts/Export/XML.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <form id="ExportXMLForm" method="post">
        <div id="ExportXML">
            <h2>Export XML/JSON</h2>
            <h3><a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }, text: Name"></a></h3>

            <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />

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
                        <label>Type</label>
                        <select class="form-control" name="XmlType" data-bind="value: XmlType">
                            <option value="Proprietary">Trifolia XML</option>
                            <!-- ko if: IsFhir() -->
                            <option value="FHIR">FHIR XML</option>
                            <option value="FHIRBuild">FHIR Build Package</option>
                            <!-- /ko -->
                            <option value="JSON">Data Snapshot (JSON)</option>
                            <!-- ko if: ImplementationGuideType() == 'CDA' -->
                            <option value="DSTU">Templates DSTU</option>
                            <!-- /ko -->
                        </select>
                    </div>

                    <!-- ko if: XmlType() == 'FHIR' -->
                    <div class="form-group">
                        <label>Include Vocabulary?</label>
                        <select class="form-control" name="IncludeVocabulary">
                            <option value="false" selected="selected">No</option>
                            <option value="true">Yes</option>
                        </select>
                    </div>
                    <!-- /ko -->
                </div>

                <div class="tab-pane" id="templates">
                    <div class="form-group">
                        <label for="ParentTemplateId">Parent Templates/Profiles</label>
                        <!-- ko foreach: ParentTemplateIds -->
                        <div data-bind="templateSelect: Id, filterImplementationGuideId: $parent.ImplementationGuideId"></div>
                        <!-- /ko -->
                    </div>

                    <div class="form-group">
                        <input type="checkbox" data-bind="checked: IncludeInferred" /> Include Inferred
                    </div>

                    <table class="table">
                        <thead>
                            <tr>
                                <td>
                                    <button type="button" class="btn btn-default btn-sm" data-bind="click: SelectAllTemplates, css: { 'active': AllTemplatesSelected() }"><i class="glyphicon glyphicon-ok"></i></button>
                                </td>
                                <th>Name</th>
                                <th>Identifier</th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: Templates()">
                            <tr>
                                <td><input type="checkbox" name="TemplateIds" data-bind="value: Id" checked="checked" /></td>
                                <td data-bind="text: Name"></td>
                                <td data-bind="text: Oid"></td>
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
    
            <div class="btn-group">
                <button class="btn btn-primary" type="button" id="ExportButton" data-bind="click: Export">Export</button>
                <button class="btn btn-default" type="button" id="CancelButton" data-bind="click: Cancel">Cancel</button>
            </div>
        </div>
    </form>

    <script type="text/javascript">
        var viewModel = null;
        $(document).ready(function () {
            viewModel = new ExportXMLViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#ExportXML')[0]);
        });
    </script>
</asp:Content>
