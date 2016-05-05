<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .constraintRow {
            padding: 5px;
        }

        .spacer {
            white-space: pre;
        }

        .constraintHeader {
            font-weight: bold;
        }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript" src="../../Scripts/knockout/knockout.isdirty.js"></script>
    <script type="text/javascript" src="../../Scripts/utils.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="../../Scripts/GreenManagement/greenModels.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript" src="../../Scripts/GreenManagement/GreenManagementViewModel.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>

    <div id="mainBody">
        <h2>Edit Green Model</h2>
        <h3><span data-bind="text: observableModel().TemplateName" title="Template business name"></span>: <span data-bind="text: observableModel().Name" title="Template element name"></span></h3>
        
        <p>
            <span class="label label-info">Constraints with a single value binding are not displayed here</span>
        </p>

        <div data-bind="with: observableModel">
            <div class="row constraintRow">
                <div class="col-md-6 constraintHeader">Constraint</div>
                <div class="col-md-1 constraintHeader">Green?</div>
                <div class="col-md-1 constraintHeader">Data Type</div>
                <div class="col-md-1 constraintHeader">Business Name</div>
                <div class="col-md-1 constraintHeader">Element Name</div>
                <div class="col-md-2">
                </div>
            </div>
            <div data-bind="template: { name: 'constraintTemplate', foreach: childConstraints }">
            </div>
        </div>

        <div class="modal fade" id="constraintEditorDialog">
            <div class="modal-dialog">
                <div class="modal-content" data-bind="with: CurrentConstraint, validationOptions: { messagesOnModified: false }">
                    <div class="modal-header">
                        <h4 class="modal-title">Edit Constraint</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <input type="checkbox" id="isGreen" data-bind="checked: hasGreenConstraint" />
                            <label for="isGreen">Green?</label>
                        </div>

                        <div class="form-group">
                            <label for="businessName">Business Name</label>
                            <input type="text" class="form-control" id="businessName" data-bind="value: businessName" />
                        </div>
                        
                        <div class="form-group">
                            <label for="elementName">Element Name</label>
                            <input type="text" class="form-control" id="elementName" data-bind="value: elementName" />
                        </div>
                        
                        <div class="form-group">
                            <label for="dataType">Data Type</label>
                            <select class="form-control" id="dataType" data-bind="value: datatypeId, options: $parent.DataTypes, optionsText: 'datatype', optionsValue: 'datatypeId', optionsCaption: 'Select...'"></select>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bind="click: $parent.SaveConstraint, disable: !Validation().isValid()">OK</button>
                        <button type="button" class="btn btn-default" data-bind="click: $parent.CancelEditConstraint">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <!-- /.modal -->

        <button type="button" class="btn btn-primary" id="saveChanges" data-bind="click: Save">Save All Changes</button>
        <button type="button" class="btn btn-default" id="cancelChanges" data-bind="click: Cancel">Cancel Changes</button>
    </div>

    <script type="text/html" id="constraintTemplate">
        <div class="row constraintRow" data-bind="tooltip: constraintDescription">
            <div class="col-md-6" data-bind="style: { 'padding-left': $parents[$parents.length - 1].GetConstraintPadding($parents) }">
                <b><span data-bind="text: $index() + 1"></span>. </b>
                <span data-bind="text: text"></span>
            </div>
            <div class="col-md-1" data-bind="text: hasGreenConstraint() ? 'Yes' : 'No'"></div>
            <div class="col-md-1" data-bind="text: $parents[$parents.length-1].GetDataTypeName(datatypeId())"></div>
            <div class="col-md-1" data-bind="text: businessName"></div>
            <div class="col-md-1" data-bind="text: elementName"></div>
            <div class="col-md-2">
                <div class="button-group">
                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[$parents.length - 1].EditConstraint($data); }">Edit</button>
                    <button type="button" class="btn btn-primary" data-bind="click: function () { $parents[$parents.length - 1].DeleteConstraint($data); }">Delete</button>
                </div>
            </div>
        </div>

        <div data-bind="template: { name: 'constraintTemplate', foreach: children }"></div>
    </script>

    <script type="text/javascript">
        var viewModel = new greenManagementViewModel(
            '/api/Green/Template/<%= Model %>',
            '/api/Green/Template',
            '/api/Green/DataType');

        $(document).ready(function () {
            var mainBodyEl = document.getElementById('mainBody');
            ko.applyBindings(viewModel, mainBodyEl);

            $('.modal').modal({
                show: false,
                keyboard: false,
                backdrop: 'static'
            });
        });
                
    </script>

</asp:Content>