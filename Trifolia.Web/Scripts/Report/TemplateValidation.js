var TemplateValidationViewModel = function () {
    var self = this;

    self.ImplementationGuides = ko.observable();
    self.ImplementationGuideId = ko.observable();
    self.ValidationResults = ko.observableArray();
    self.ShowLevel = ko.observable('all');

    self.Initialize = function () {
        $.ajax({
            url: '/api/ImplementationGuide',
            success: function (results) {
                ko.mapping.fromJS({ ImplementationGuides: results.Items }, {}, self);
            }
        });
    };

    self.ValidateTemplates = function () {
        self.ValidationResults(null);
        $.blockUI({ message: "Validating..." });
        $.ajax({
            url: '/api/Report/ImplementationGuide/' + self.ImplementationGuideId() + '/Validate',
            success: function (results) {
                ko.mapping.fromJS({ ValidationResults: results }, {}, self);
                $.unblockUI();
            }
        });
    };
    self.ImplementationGuideId.subscribe(self.ValidateTemplates);

    self.GetSeverityDisplay = function (severity) {
        switch (severity) {
            case 0:
                return 'Warning';
            case 1:
                return 'Error';
            default:
                return 'Unknown';
        }
    };

    self.GetItems = function (items) {
        var filtered = ko.utils.arrayFilter(items, function (item) {
            if (self.ShowLevel() == 'all') {
                return true;
            } else if (self.ShowLevel() == 'warnings') {
                return item.Level() == 0;
            } else if (self.ShowLevel() == 'errors') {
                return item.Level() == 1;
            }

            return true;
        });

        return filtered;
    };

    self.GetResults = function () {
        var filtered = ko.utils.arrayFilter(self.ValidationResults(), function (result) {
            return self.GetItems(result.Items()).length > 0;
        });

        return filtered;
    };

    self.Initialize();
};