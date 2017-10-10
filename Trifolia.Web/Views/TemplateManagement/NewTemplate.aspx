<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ng-cloak" ng-app="Trifolia" ng-controller="NewTemplateController" ng-init="init()" block-ui="templateBlock">
        <div><h2>Add New Template</h2></div>
        <div class="col-md-6">
            <div class="form-group">
                <div class="input-group" ng-class="{'has-error': !template.OwningImplementationGuideId}">
                    <div class="input-group-addon">Implementation Guide:</div>
                    <select class="form-control" ng-options="implementationGuide.Id as implementationGuide.Name for implementationGuide in implementationGuides" ng-model="template.OwningImplementationGuideId" ng-change="initializeTemplateTypes(template.OwningImplementationGuideId); createBookmark()"></select>
                </div>
            </div>
            {{implementationGuide | json}}
            <div class="form-group" ng-if="template.OwningImplementationGuideId">
                <div class="input-group" ng-class="{'has-error': !template.TemplateTypeId}">
                    <div class="input-group-addon">Template Type:</div>
                    <select class="form-control" ng-options="templateType.Id as templateType.Name for templateType in templateTypes" ng-model="template.TemplateTypeId" ng-change="setPrimaryContext()"></select>
                </div>
            </div>
            <div class="form-group" ng-if="template.TemplateTypeId">
                <div class="input-group">
                    <input type="text" class="form-control" ng-model="template.PrimaryContext" style="width: 50%" readonly="readonly" />
                    <input type="text" class="form-control" ng-model="template.PrimaryContextType" style="width: 50%" readonly="readonly" /> 
                    <!-- TODO: Make the user able to change the primary context type based on owning IG -->
                    <div class="input-group-btn">
                        <button type="button" class="btn btn-primary" ng-click="selectContext()">...</button>
                    </div>
                </div>
            </div>
            <div class="form-group" ng-if="template.TemplateTypeId">
                <div class="input-group" ng-class="{'has-error': !template.Name}">
                    <div class="input-group-addon">Name:</div>
                    <input type="text" class="form-control" ng-model="template.Name" ng-change="createBookmark()" ngMaxlength="255"/>
                </div>
                <span class="help-block" ng-if="!template.Name">Name is required.</span>
            </div>
            <div class="form-group" ng-if="template.TemplateTypeId">
                <div class="input-group">
                    <div class="input-group-addon">Bookmark:</div>
                    <input type="text" class="form-control" ng-model="template.Bookmark" ngMaxlength="40"/>
                </div>
            </div>
            <div class="long-id form-group" ng-if="template.TemplateTypeId">
                <div class="input-group identifier-field" ng-class="{'has-error': !isIdentifierUnique || !identifier.base || !identifier.ext || !isIdentifierValid || !isIdentifierRightSize}">
                    <div class="input-group-addon">
                        <span>Long Id:</span>
                        <div title="" class="glyphicon glyphicon-question-sign clickable"></div>
                    </div>
                    <select class="form-control" ng-model="identifier.base" ng-change="updateIdentifier()" style="width: 50%;">
                        <option ng-if="implementationGuide.Identifier">{{implementationGuide.Identifier + '.'}}</option>
                        <option ng-if="!isFhir">urn:oid:</option>
                        <option ng-if="!isFhir">urn:hl7ii:</option>
                        <option>http://</option>
                        <option>https://</option>
                    </select>
                    <input class="form-control" type="text" ng-model="identifier.ext" ng-change="updateIdentifier()" ng-model-options="{ debounce: 500 }" style="width: 50%;" />
                </div>
                <span class="help-block" ng-if="!isIdentifierUnique">Identifier should be unique</span>
                <span class="help-block" ng-if="!isIdentifierValid">Identifier is in an invalid format</span>
                <span class="help-block" ng-if="!isIdentifierRightSize">Identifier should be less than or equal to 255 characters</span>
            </div>
            <div class="form-group" ng-if="template.TemplateTypeId">
                <div class="input-group">
                    <div class="input-group-addon">Description:</div>
                    <textarea class="form-control" ng-model="template.Description"></textarea>
                </div>
            </div>
        </div>
        <div id="footer">
            <div class="row">
                <div class="col-md-2">
                    <div class="btn-group">
                        <button type="button" class="btn btn-default dropdown-toggle" ng-click="save()" ng-disabled="invalid">Save and Continue</button>
                    </div>
                    <div class="btn-group">
                        <button type="button" class="btn btn-default dropdown-toggle" ng-click="cancel()">Cancel</button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript" src="/Scripts/TemplateEdit/newTemplateController.js?<%= ViewContext.Controller.GetType().Assembly.GetName().Version %>""></script>
</asp:Content>
