var TemplateReviewViewModel = function () {
    var self = this;

    self.Filter = ko.observable(new TemplateReviewFilterModel());
    self.Model = ko.observable(new TemplateReviewResultsModel());

    self.TotalPages = ko.computed(function () {
        if (self.Model().Total() == 0) {
            return 0;
        }

        if (self.Model().Total() < 20) {
            return 1;
        }

        return Math.ceil(self.Model().Total() / 20);
    });

    self.FirstPage = function () {
        self.Filter().PageCount(1);
        self.Initialize();
    };

    self.LastPage = function () {
        if (self.TotalPages() > 0) {
            self.Filter().PageCount(self.TotalPages());
            self.Initialize();
        }
    };

    self.PreviousPage = function () {
        self.Filter().PageCount(self.Filter().PageCount() - 1);
        self.Initialize();
    };

    self.NextPage = function () {
        self.Filter().PageCount(self.Filter().PageCount() + 1);
        self.Initialize();
    };

    self.Initialize = function () {
        var data = ko.mapping.toJS(self.Filter());

        $.blockUI();
        $.ajax({
            method: 'POST',
            url: '/api/Report/Template/Review',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (results) {
                var templateReviewResults = new TemplateReviewResultsModel(results);
                self.Model(templateReviewResults);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.FilterChanged = function () {
        self.Filter().PageCount(1);
        self.Initialize();
    };

    self.Filter().TemplateName.subscribe(self.FilterChanged);
    self.Filter().TemplateOid.subscribe(self.FilterChanged);
    self.Filter().ImplementationGuideName.subscribe(self.FilterChanged);
    self.Filter().AppliesTo.subscribe(self.FilterChanged);
    self.Filter().ImpliedTemplateName.subscribe(self.FilterChanged);
    self.Filter().ImpliedTemplateOid.subscribe(self.FilterChanged);
    self.Filter().ConstraintNumber.subscribe(self.FilterChanged);
    self.Filter().IsPrimitive.subscribe(self.FilterChanged);
    self.Filter().HasSchematron.subscribe(self.FilterChanged);
    self.Filter().ValueSetName.subscribe(self.FilterChanged);
    self.Filter().CodeSystemName.subscribe(self.FilterChanged);

    self.Initialize();
};

var TemplateReviewFilterModel = function () {
    var self = this;
    var mapping = {
        include: ['PageCount', 'Count', 'TemplateName', 'TemplateOid', 'ImplementationGuideName', 'AppliesTo', 'ImpliedTemplateName', 'ImpliedTemplateOid', 'ConstraintNumber', 'IsPrimitive', 'HasSchematron', 'ValueSetName', 'CodeSystemName']
    };

    self.PageCount = ko.observable(1);
    self.Count = ko.observable(20);

    self.TemplateName = ko.observable('');
    self.TemplateOid = ko.observable('');
    self.ImplementationGuideName = ko.observable('');
    self.AppliesTo = ko.observable('');
    self.ImpliedTemplateName = ko.observable('');
    self.ImpliedTemplateOid = ko.observable('');
    self.ConstraintNumber = ko.observable('');
    self.IsPrimitive = ko.observable('');
    self.HasSchematron = ko.observable('');
    self.ValueSetName = ko.observable('');
    self.CodeSystemName = ko.observable('');
};

var TemplateReviewResultsModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Total', 'Items'],
        'Items': {
            create: function (options) {
                return new TemplateReviewModel(options.data);
            }
        }
    };

    self.Total = ko.observable(0);
    self.Items = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var TemplateReviewModel = function (data) {
    var self = this;
    var mapping = {
        include: ['TemplateId', 'TemplateName', 'TemplateOid', 'ImplementationGuideId', 'ImplementationGuideName', 'ImpliedTemplateId', 'AppliesTo', 'ImpliedTemplateName', 'ImpliedTemplateOid', 'ConstraintNumber', 'IsPrimitive', 'HasSchematron', 'ValueSetName', 'CodeSystemName']
    };

    self.TemplateId = ko.observable();
    self.TemplateName = ko.observable();
    self.TemplateOid = ko.observable();
    self.ImplementationGuideId = ko.observable();
    self.ImplementationGuideName = ko.observable();
    self.ImpliedTemplateId = ko.observable();
    self.AppliesTo = ko.observable();
    self.ImpliedTemplateName = ko.observable();
    self.ImpliedTemplateOid = ko.observable();
    self.ConstraintNumber = ko.observable();
    self.IsPrimitive = ko.observable();
    self.HasSchematron = ko.observable();
    self.ValueSetName = ko.observable();
    self.CodeSystemName = ko.observable();

    ko.mapping.fromJS(data, mapping, self);
};