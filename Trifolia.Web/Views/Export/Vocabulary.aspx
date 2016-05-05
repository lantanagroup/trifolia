<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <form id="ExportVocabularyForm" method="post">
        <div id="ExportVocabulary">
            <h2>Export Vocabulary</h2>
            <h3><a data-bind="attr: { href: '/IGManagement/View/' + ImplementationGuideId() }, text: Name"></a></h3>

            <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />
            <input type="hidden" name="CancelUrl" data-bind="value: CancelUrl" />
        
            <!-- Nav tabs -->
            <ul class="nav nav-tabs">
                <li class="active"><a href="#options" data-toggle="tab">Options</a></li>
                <li><a href="#valuesets" data-toggle="tab">Value Sets</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div class="tab-pane active" id="options">
                    <div class="form-group">
                        <label>Export Format</label>
                        <select class="form-control" name="ExportFormat">
                            <option value="1">Lantana Standard (XML)</option>
                            <option value="2">SVS (XML)</option>
                            <option value="3">Excel (XLSX)</option>
                            <option value="4">FHIR (XML)</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Maximum Members (0 = export all members)</label>
                        <div class="input-group">
                            <input type="text" name="MaximumMembers" class="form-control" value="0" data-bind="spinedit: MaximumMembers" />
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Encoding</label>
                        <select class="form-control" name="Encoding">
                            <option value="UTF-8">UTF-8</option>
                            <option value="Unicode">Unicode</option>
                        </select>
                    </div>
                </div>

                <div class="tab-pane" id="valuesets">
                    <table class="table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Identifier</th>
                                <th>Binding Date</th>
                            </tr>
                        </thead>
                        <tbody data-bind="foreach: ValueSets">
                            <tr>
                                <td data-bind="text: Name"></td>
                                <td data-bind="text: Oid"></td>
                                <td data-bind="text: BindingDate"></td>
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
            </div>
    
            <div class="btn-group">
                <button class="btn btn-primary" type="button" id="ExportButton" data-bind="click: Export">Export</button>
                <button class="btn btn-default" type="button" id="CancelButton" data-bind="click: Cancel">Cancel</button>
            </div>
        </div>
    </form>
    
    <script type="text/javascript" src="../../Scripts/Export/Vocabulary.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new ExportVocabularyViewModel(<%= Model %>);
            ko.applyBindings(viewModel, $('#ExportVocabulary')[0]);
        });
    </script>
</asp:Content>
