var ViewImplementationGuideViewModel = function (implementationGuideId) {
    var self = this;

    self.Model = ko.observable(null);
    self.Files = ko.observableArray([]);
    self.AuditTrail = ko.observableArray([]);
    self.AuditTrailStartDate = ko.observable(formatDateObj(new Date().setDate(new Date().getDate() - 7)));
    self.AuditTrailEndDate = ko.observable(formatDateObj(Date.now()));
    self.Notes = ko.observableArray([]);
    self.Primitives = ko.observableArray([]);
    self.NewPublishDate = ko.observable();
    self.SearchTemplateQuery = ko.observable();

    var initializeAuditTrail = function () {
        $.ajax({
            url: '/api/ImplementationGuide/' + implementationGuideId + '/AuditTrail?startDate=' + encodeURIComponent(self.AuditTrailStartDate()) + '&endDate=' + encodeURIComponent(self.AuditTrailEndDate()),
            cache: false,
            success: function (auditTrail) {
                self.AuditTrail(auditTrail);
            }
        });
    };

    self.AuditTrailStartDate.subscribe(initializeAuditTrail);
    self.AuditTrailEndDate.subscribe(initializeAuditTrail);
    
    self.Initialize = function () {
        fireTrifoliaEvent('implementationGuideLoading');

        self.Model(null);
        self.Files([]);

        $.ajax({
            url: '/api/ImplementationGuide/' + implementationGuideId,
            cache: false,
            success: function (model) {
                self.Model(new ViewImplementationGuideModel(model));
                fireTrifoliaEvent('implementationGuideLoaded');

                if (model.ViewAuditTrail) {
                    initializeAuditTrail();
                }

                if (model.ViewNotes) {
                    $.ajax({
                        url: '/api/ImplementationGuide/' + implementationGuideId + '/Note',
                        cache: false,
                        success: function (notes) {
                            ko.mapping.fromJS({ Notes: notes }, {}, self);
                        }
                    });
                }

                if (model.ViewPrimitives) {
                    $.ajax({
                        url: '/api/ImplementationGuide/' + implementationGuideId + '/Primitive',
                        cache: false,
                        success: function (primitives) {
                            ko.mapping.fromJS({ Primitives: primitives }, {}, self);
                        }
                    });
                }

                if (model.ViewFiles) {
                    $.ajax({
                        url: '/api/ImplementationGuide/' + implementationGuideId + '/File',
                        cache: false,
                        success: function (files) {
                            ko.mapping.fromJS({ Files: files }, {}, self);
                        }
                    });
                }
            }
        });
    };

    self.ViewTemplate = function (id) {
        location.href = '/TemplateManagement/View/Id/' + id;
    };

    self.EditTemplate = function (id) {
        location.href = '/TemplateManagement/Edit/Id/' + id;
    };

    self.GetTemplateTypes = function () {
        var templateTypes = ko.utils.arrayFilter(self.Model().TemplateTypes(), function (templateType) {
            var templates = self.GetTemplates(templateType);

            return templates.length > 0;
        });

        return templateTypes;
    };

    self.GetTemplates = function (templateType) {
        var templates = ko.utils.arrayFilter(self.Model().Templates(), function (template) {
            return template.Type() == templateType.Name();
        });

        if (self.SearchTemplateQuery()) {
            templates = ko.utils.arrayFilter(templates, function (template) {
                var description = template.Description();
                var name = template.Name();
                var oid = template.Oid();
                var status = template.Status();

                var match = (name && name.indexOf(self.SearchTemplateQuery()) >= 0) ||
                    (oid && oid.indexOf(self.SearchTemplateQuery()) >= 0) ||
                    (status && status.indexOf(self.SearchTemplateQuery()) >= 0);

                return match;
            });
        }

        return templates;
    };

    self.ShowPublishDialog = function () {
        $('#publishDateSelector').modal('show');
    };

    self.Publish = function () {
        if (!self.NewPublishDate()) {
            alert("No publish date has been given. Cannot publish.");
            return;
        }

        $('#publishDateSelector').modal('hide');

        $.ajax({
            type: "POST",
            url: "/api/ImplementationGuide/" + implementationGuideId + "/Publish?publishDate=" + encodeURIComponent(self.NewPublishDate()),
            success: function (data) {
                window.location = "/IGManagement/View?implementationGuideId=" + implementationGuideId;
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            }
        });
    };

    self.Draft = function () {
        $.ajax({
            type: "POST",
            url: "/api/ImplementationGuide/" + implementationGuideId + "/Draft",
            success: function (data) {
                window.location = "/IGManagement/View?implementationGuideId=" + implementationGuideId;
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            }
        });
    };

    self.Ballot = function () {
        $.ajax({
            type: "POST",
            url: "/api/ImplementationGuide/" + implementationGuideId + "/Ballot",
            success: function () {
                window.location = "/IGManagement/View?implementationGuideId=" + implementationGuideId;
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            }
        });
    };

    self.Unpublish = function () {
        $.ajax({
            type: "POST",
            url: '/api/ImplementationGuide/' + implementationGuideId + '/Unpublish',
            success: function () {
                window.location = "/IGManagement/View?implementationGuideId=" + implementationGuideId;
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            }
        });
    };

    self.NewVersion = function () {
        var data = { implementationGuideId: implementationGuideId };

        if (confirm("Are you sure you want to create a new version of this implementation guide?")) {
            $.ajax({
                type: "POST",
                url: "/IGManagement/NewVersion",
                dataType: "json",
                data: data,
                complete: function (result, status) {
                    if (status == "success") {
                        var responseData = JSON.parse(result.responseText);

                        location.href = '/IGManagement/View/' + responseData["ImplementationGuideId"];
                    } else {
                        alert("An error occurred while creating a new version of the implementation guide.");
                    }
                }
            });
        }
    };

    self.Initialize();
};

var ViewImplementationGuideModel = function (data) {
    var self = this;
    var mapping = {
        'Templates': {
            create: function (options) {
                return new TemplateItemModel(options.data);
            }
        },
        'TemplateTypes': {
            create: function (options) {
                return new TemplateTypeItemModel(options.data);
            }
        }
    };

    self.Id = ko.observable();
    self.EnableNewVersion = ko.observable(false);
    self.HasPreviousVersion = ko.observable(false);
    self.Name = ko.observable();
    self.DisplayName = ko.observable();
    self.NextVersionImplementationGuideId = ko.observable();
    self.Organization = ko.observable();
    self.PreviousVersionIgName = ko.observable();
    self.PreviousVersionImplementationGuideId = ko.observable();
    self.PublishDate = ko.observable();
    self.ShowDelete = ko.observable();
    self.ShowEditBookmarks = ko.observable();
    self.ShowEditIG = ko.observable();
    self.ShowEditTemplate = ko.observable();
    self.ShowExportGreen = ko.observable();
    self.ShowExportMSWord = ko.observable();
    self.ShowExportSchematron = ko.observable();
    self.ShowExportVocabulary = ko.observable();
    self.ShowExportXML = ko.observable();
    self.ShowManageFiles = ko.observable();
    self.ShowPublish = ko.observable();
    self.Status = ko.observable();
    self.Type = ko.observable();
    self.UserPrompt = ko.observable();
    self.ViewAuditTrail = ko.observable();
    self.ViewFiles = ko.observable();
    self.ViewNotes = ko.observable();
    self.ViewPrimitives = ko.observable();

    self.TemplateTypes = ko.observableArray([]);
    self.Templates = ko.observableArray([]);
    self.WebPublications = ko.observableArray([]);

    self.ComputedName = ko.computed(function () {
        if (self.DisplayName()) {
            return self.DisplayName();
        }

        return self.Name();
    });

    self.NewVersionTooltip = ko.computed(function () {
        return !self.EnableNewVersion() ? 'Implementation guide must not already have a new version and must be published.' : null;
    });

    ko.mapping.fromJS(data, mapping, self);
};

var TemplateItemModel = function (data) {
    var self = this;
    var mapping = {

    };

    self.Id = ko.observable();
    self.HasGreenModel = ko.observable();
    self.Name = ko.observable();
    self.Oid = ko.observable();
    self.Status = ko.observable();
    self.Type = ko.observable();
    self.Description = ko.observable();
    self.CanEdit = ko.observable();
    self.IsExpanded = ko.observable(false);

    ko.mapping.fromJS(data, mapping, self);
};

var TemplateTypeItemModel = function (data) {
    var self = this;
    var mapping = {

    };

    self.Name = ko.observable();
    self.Description = ko.observable();
    self.Order = ko.observable();
    self.IsExpanded = ko.observable(false);

    ko.mapping.fromJS(data, mapping, self);
};