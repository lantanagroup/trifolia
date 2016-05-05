var ExportGreenViewModel = function (implementationGuideId) {
    var self = this;

    self.ImplementationGuideId = ko.observable();
    self.Name = ko.observable();
    self.RootTemplateId = ko.observable();
    self.Templates = ko.observableArray([]);

    self.Export = function () {
        if (!self.RootTemplateId()) {
            alert("You must select a document level template to export.");
            return;
        }

        $("#ExportGreenForm").attr("action", "/api/Export/Green");
        $("#ExportGreenForm").submit();
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/Green',
            success: function (results) {
                ko.mapping.fromJS(results, {}, self);
            }
        });
    };

    self.Initialize();
};