var ExportXMLViewModel = function (implementationGuideId) {
    var self = this;

    self.Name = ko.observable();
    self.ImplementationGuideId = ko.observable(implementationGuideId);
    self.ParentTemplateIds = ko.observableArray([{ Id: ko.observable() }]);
    self.ImplementationGuideType = ko.observable('');
    self.IncludeInferred = ko.observable(true);
    self.Templates = ko.observableArray([]);
    self.CancelUrl = ko.observable();
    self.AllTemplatesSelected = ko.observable(true);
    self.Categories = ko.observableArray([]);
    self.SelectedCategories = ko.observableArray([]);
    self.XmlType = ko.observable('Proprietary');
    self.IncludeVocabulary = ko.observable(false);
    self.Messages = ko.observableArray();
    self.IsValidated = ko.observable(false);
    self.IsLoaded = ko.observable(false);

    self.IsFhir = ko.computed(function () {
        return _.some(trifoliaConfig.FhirIgTypes, function (fhirIgType) {
            return fhirIgType.Name == self.ImplementationGuideType();
        });
    });

    self.EnableExportButton = ko.computed(function () {
        return self.IsLoaded() && self.IsValidated();
    });

    var validate = function () {
        var exportSettingsModel = {
            TemplateIds: [],
            IncludeVocabulary: self.IncludeVocabulary(),
            XmlType: self.XmlType(),
            ImplementationGuideId: self.ImplementationGuideId()
        };

        exportSettingsModel.TemplateIds = _.map(self.Templates(), function (template) {
            return template.Id();
        });

        self.IsValidated(false);

        $.ajax({
            method: 'POST',
            url: '/api/Export/XML/Validate',
            data: exportSettingsModel,
            success: function (messages) {
                self.Messages(messages);
                self.IsValidated(true);
            },
            error: function (err, err2, err3) {
                self.Messages([err3]);
            }
        });
    };

    var loadTemplates = function () {
        var deferred = Q.defer();
        var url = '/api/ImplementationGuide/' + implementationGuideId + '/Template?inferred=' + self.IncludeInferred();

        for (var i in self.ParentTemplateIds()) {
            if (self.ParentTemplateIds()[i].Id()) {
                url += '&parentTemplateIds=' + self.ParentTemplateIds()[i].Id();
            }
        }

        for (var i in self.SelectedCategories()) {
            var category = self.SelectedCategories()[i];

            if (category) {
                url += '&categories=' + encodeURIComponent(category);
            }
        }

        $.ajax({
            url: url,
            success: function (results) {
                ko.mapping.fromJS({ Templates: results }, {}, self);
                deferred.resolve();
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    var loadImplementationGuide = function () {
        var deferred = Q.defer();

        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/XML',
            success: function (data) {
                ko.mapping.fromJS(data, {}, self);
                deferred.resolve();
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    var parentTemplateIdChanged = function () {
        var parentTemplateIds = self.ParentTemplateIds();
        var lastParentTemplateId = parentTemplateIds[parentTemplateIds.length - 1].Id();

        if (lastParentTemplateId) {
            var newParentTemplateId = {
                Id: ko.observable()
            };

            newParentTemplateId.Id.subscribe(parentTemplateIdChanged);
            self.ParentTemplateIds.push(newParentTemplateId);
        }

        loadTemplates();
    };

    self.SelectAllTemplates = function () {
        self.AllTemplatesSelected(!self.AllTemplatesSelected());
        $('input[name=TemplateIds]').prop('checked', self.AllTemplatesSelected());
    };

    self.Export = function () {
        $("#ExportXMLForm").attr("action", "/api/Export/XML");
        $("#ExportXMLForm").submit();
    }

    self.Cancel = function () {
        location.href = self.CancelUrl();
    };

    // Initialize the page
    loadImplementationGuide()
        .then(loadTemplates)
        .then(validate)
        .then(function () {
            // Subscriptions
            self.ParentTemplateIds()[0].Id.subscribe(parentTemplateIdChanged);
            self.IncludeInferred.subscribe(loadTemplates);
            self.SelectedCategories.subscribe(loadTemplates);
            self.XmlType.subscribe(validate);

            // Indicate the page is loaded
            self.IsLoaded(true);
        })
        .catch(function (err) {
            self.Messages([err]);
        });
};