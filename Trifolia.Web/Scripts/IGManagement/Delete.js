var DeleteImplementationGuideViewModel = function (implementationGuideId) {
    var self = this;

    self.ImplementationGuide = ko.observable();
    self.NumberOfTemplates = ko.observable();
    self.ImplementationGuides = ko.observableArray([]);
    self.ReplaceImplementationGuideId = ko.observable();

    self.CanDelete = ko.computed(function () {
        return self.ImplementationGuide() && !self.ImplementationGuide().NextVersionImplementationGuideId();
    });

    self.Initialize = function () {
        $.ajax({
            url: '/api/ImplementationGuide/' + implementationGuideId,
            success: function (results) {
                ko.mapping.fromJS({ ImplementationGuide: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Template?filterImplementationGuideId=' + implementationGuideId,
            success: function (result) {
                self.NumberOfTemplates(result.TotalItems);
            }
        });

        $.ajax({
            url: '/api/ImplementationGuide',
            success: function (results) {
                var filtered = ko.utils.arrayFilter(results.Items, function (item) {
                    return item.Id != implementationGuideId;
                });

                ko.mapping.fromJS({ ImplementationGuides: filtered }, {}, self);
            }
        });
    };

    self.Delete = function () {
        if (confirm("Are you absolutely sure you want to delete this implementation guide?")) {
            $.blockUI({ message: 'Deleting...' });

            var url = '/api/ImplementationGuide/' + implementationGuideId;

            if (self.ReplaceImplementationGuideId()) {
                url += '?replaceImplementationGuideId=' + self.ReplaceImplementationGuideId();
            }

            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (results) {
                    $.unblockUI();

                    if (self.ReplaceImplementationGuideId()) {
                        location.href = '/IGManagement/View/' + self.ReplaceImplementationGuideId();
                    } else {
                        location.href = '/IGManagement/List';
                    }
                }
            });
        }
    };

    self.Initialize();
};