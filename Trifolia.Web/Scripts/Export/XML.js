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

    var templatesLoaded = false;

    self.RefreshTemplates = function () {
        templatesLoaded = false;

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
                templatesLoaded = true;
            }
        });
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

        if (templatesLoaded) {
            self.RefreshTemplates();
        }
    };

    self.ParentTemplateIds()[0].Id.subscribe(parentTemplateIdChanged);
    self.IncludeInferred.subscribe(self.RefreshTemplates);
    self.SelectedCategories.subscribe(self.RefreshTemplates);

    self.Initialize = function () {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/XML',
            success: function (data) {
                ko.mapping.fromJS(data, {}, self);
            }
        });

        self.RefreshTemplates();
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

    self.Initialize();
};