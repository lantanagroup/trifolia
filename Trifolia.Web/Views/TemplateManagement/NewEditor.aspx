<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.MVC.Master" Inherits="System.Web.Mvc.ViewPage<Trifolia.Web.Models.TemplateManagement.EditModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="KeyboardShortcuts" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div ng-app="NewEditor" ng-controller="EditorController" ng-init="init(<%= Model.TemplateIdString %>, <%= Model.DefaultImplementationGuideIdString %>)">
        <h2>New Template Editor</h2>
    </div>

    <tabset>
        <tab heading="Meta Data">
            <div class="col-md-6">
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Name:</div>
                        <input type="text" class="form-control" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Long Id:</div>
                        <input type="text" class="form-control" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Short Id:</div>
                        <input type="text" class="form-control" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Implied Template/Profile:</div>
                        <input type="text" class="form-control" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Extensibility:</div>
                        <select class="form-control"></select>
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Status:</div>
                        <select class="form-control"></select>
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Description:</div>
                        <textarea class="form-control" height="100"></textarea>
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Notes:</div>
                        <textarea class="form-control" height="100"></textarea>
                    </div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Implementation Guide</div>
                        <input type="text" class="form-control" readonly="readonly" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Type:</div>
                        <input type="text" class="form-control" readonly="readonly" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Applies To:</div>
                        <input type="text" class="form-control" readonly="readonly" />
                    </div>
                    <span class="help-block"><a href="">Move</a> the template/profile to change the Implementation Guide, Type, or Applies To fields.</span>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Authored By:</div>
                        <input type="text" class="form-control" readonly="readonly" />
                    </div>
                </div>
                <div class="form-group">
                    <div class="input-group">
                        <div class="input-group-addon">Organization:</div>
                        <input type="text" class="form-control" readonly="readonly" />
                    </div>
                </div>
            </div>
        </tab>
        <tab heading="Constraints">
        </tab>
        <tab heading="Validation">

        </tab>
        <tab heading="Preview">

        </tab>
    </tabset>

    <script type="text/javascript" src="/Scripts/angular/angular.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular/ui-bootstrap-tpls-0.12.1.min.js"></script>
    <script type="text/javascript" src="/Scripts/lodash.min.js"></script>
    <script type="text/javascript" src="/Scripts/TemplateEdit/newTemplateEditor.js"></script>
</asp:Content>