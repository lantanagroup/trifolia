var TemplateUsageViewModel = function () {
    var self = this;

    self.TemplateId = ko.observable(609);
    self.TemplateUsage = ko.observableArray([]);

    var loadTemplateUsage = function () {
        if (!self.TemplateId()) {
            return;
        }

        $.ajax({
            url: '/api/Template/' + self.TemplateId() + '/Usage',
            success: function (results) {
                ko.mapping.fromJS({ TemplateUsage: results }, {}, self);
            }
        });
    };

    self.TemplateId.subscribe(loadTemplateUsage);

    self.Initialize = function () {
        loadTemplateUsage();
    };

    self.ViewUsage = function (template) {
        self.TemplateId(template.Id());
    };

    self.ViewTemplate = function (template) {
        location.href = '/TemplateManagement/View/Id/' + template.Id();
    };

    self.Initialize();
};