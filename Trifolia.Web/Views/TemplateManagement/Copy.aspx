<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.CopyRequestModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <input type="hidden" name="TemplateId" data-bind="value: TemplateId" />
    <input type="hidden" name="TemplateName" data-bind="value: Name" />
    <input type="hidden" name="Type" data-bind="value: Type" />
    
    <!-- This template is used to build the hierarchy of constraints to display to the user so that the user can choose
        what to do with the conformance number conflicts (if any exist) -->
    <script type="text/html" id="constraint-template">
        <div class="row">
            <div class="col-md-2">
                <select name="NumberReplacement" class="form-control input-sm" data-bind="value: NumberReplacementType, attr: { constraintNumber: Number }">
                    <!-- ko if: NumberReplacementType() == 1 -->
                    <option value="1" selected>Regenerate This</option>
                    <option value="2">Regenerate Other</option>
                    <!-- /ko -->
                    <!-- ko if: NumberReplacementType() == 0 -->
                    <option value="0" selected>Use Same</option>
                    <option value="1">Regenerate This</option>
                    <!-- /ko -->
                </select>
            </div>
            <div class="col-md-10">
                <!-- ko foreach: $parents -->
                <!-- ko if: $index() != 0 -->
                <span style="white-space: pre">    </span>
                <!-- /ko -->
                <!-- /ko -->
                <span data-bind="text: Narrative"></span>
            </div>
        </div>

        <div data-bind="template: { name: 'constraint-template', foreach: $parents[$parents.length - 1].GetConstraints(Number()) }"></div>
    </script>

    <div id="CopyTemplate" data-bind="validationOptions: { messagesOnModified: false }">
        <div id="Step1" data-bind="visible: IsStep1">
            <h2><span data-bind="text: CopyModeDisplay"></span> Step 1: <span data-bind="text: OriginalName"></span></h2>
            <p data-bind="text: Type" style="margin-top: 0px;"></p>
            <p data-bind="text: Message, enabled: Message != ''" style="color: red; font-weight: bold;"></p>

            <div class="form-group">
                <label for="Name">Name</label>
                <input type="text" class="form-control" name="Name" id="Name" data-bind="value: Name" />
            </div>

            <div class="form-group">
                <label for="Oid">Identifier</label>
                <input type="text" class="form-control" name="Oid" id="Oid" data-bind="value: Oid" />
            </div>

            <div class="form-group">
                <label for="Bookmark">Bookmark</label>
                <input type="text" class="form-control" name="Bookmark" id="Bookmark" data-bind="value: Bookmark" />
            </div>

            <div class="form-group">
                <label>Implied Template/Profile</label>
                <div data-bind="templateSelect: ImpliedTemplateId"></div>
            </div>

            <div class="form-group">
                <label for="ImplementationGuide">Implementation Guide</label>
                <!-- ko if: IsNewVersion() -->
                <input type="hidden" name="ImplementationGuideId" data-bind="value: ImplementationGuideId" />
                <span data-bind="text: NewVersionImplementationGuideName"></span>
                <!-- /ko -->

                <!-- ko if: !IsNewVersion() -->
                <select class="form-control" name="ImplementationGuideId" id="ImplementationGuideId" data-bind="value: ImplementationGuideId, options: ImplementationGuides, optionsText: 'Name', optionsValue: 'Id', optionsCaption: 'Select...'"></select>
                <!-- TODO -->
                <!-- /ko -->
            </div>

            <div class="btn-group" style="padding-top: 5px;">
                <button type="button" class="btn btn-default" data-bind="enable: (SubmitEnabled() && IsValid()), click: CompleteStep1">Next</button>
                <button type="button" class="btn btn-primary" data-bind="click: CancelButtonClicked">Cancel</button>
            </div>
        </div>
        <div id="Step2" data-bind="visible: IsStep2">
            <h2><span data-bind="text: CopyModeDisplay"></span> Step 2: <span data-bind="    text: OriginalName"></span></h2>
            <span data-bind="text: Type"></span>

            <p>This step allows you to resolve and conformance number conflicts that may exist when copying a template/profile from one implementation guide to another.</p>

            <p style="font-weight: bold;" data-bind="visible: !HasConfConflicts()">There are no conformance number conflicts to resolve. Press "Finish" at the bottom of the screen to complete the copy process.</p>

            <h3>Constraints [<a href="#" data-bind="click: RegenerateAllNew">Regenerate all new constraints</a>]</h3>            
        
            <div data-bind="template: { name: 'constraint-template', foreach: GetConstraints(null) }"></div>
        
            <div class="btn-group" style="padding-top: 5px;">
                <button type="button" class="btn btn-default" id="FinishButton" data-bind="click: FinishButtonClicked">Finish</button>
                <button type="button" class="btn btn-primary" id="CancelButton" data-bind="click: CancelButtonClicked">Cancel</button>
            </div>
        </div>
    </div>
    
    <script type="text/javascript" src="/Scripts/TemplateManagement/templateCopy.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>"></script>
    <script type="text/javascript">
        var viewModel = null;

        $(document).ready(function () {
            viewModel = new CopyModel(<%= Model.TemplateId %>, <%= Model.NewVersion ? "true" : "false" %>, <%= Model.NewVersionImplementationGuideId != null ? Model.NewVersionImplementationGuideId.ToString() : "null" %>);
            ko.applyBindings(viewModel, $("#CopyTemplate")[0]);
        });
    </script>
</asp:Content>