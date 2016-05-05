var XmlSample = function (data, publishModel) {
    var self = this;
    var mapping = {
        'include': ['Id', 'Name', 'SampleText', 'IsDeleted']
    };

    self.Id = ko.observable();
    self.Name = ko.observable().extend({ required: true, maxLength: 255 });
    self.SampleText = ko.observable().extend({ required: true });
    self.IsDeleted = ko.observable();
    self.InstanceIdentifier = ko.observable(GUID());
    self.IsNew = ko.observable(true);

    ko.mapping.fromJS(data, mapping, self);

    // Validation
    var validSample = ko.validatedObservable(self);

    self.IsValid = ko.computed(function () {
        return validSample().isValid();
    });

    // Detect changes
    self.Name.subscribe(publishModel.OnModified);
    self.SampleText.subscribe(publishModel.OnModified);
    self.IsDeleted.subscribe(publishModel.OnModified);
};

var ConstraintSample = function (data, publishModel) {
    var self = this;
    var mapping = {
        'include': ['Id', 'ConstraintId', 'Name', 'SampleText', 'IsDeleted']
    };

    self.Id = ko.observable();
    self.ConstraintId = ko.observable();
    self.Name = ko.observable().extend({ required: true, minLength: 5, maxLength: 255 });
    self.SampleText = ko.observable().extend({ required: true });
    self.IsDeleted = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.InstanceIdentifier = ko.observable(GUID());

    ko.mapping.fromJS(data, mapping, self);

    // Validation
    var validSample = ko.validatedObservable(self);

    self.IsValid = ko.computed(function () {
        return validSample().isValid();
    });

    // Detect changes
    self.Name.subscribe(publishModel.OnModified);
    self.SampleText.subscribe(publishModel.OnModified);
    self.IsDeleted.subscribe(publishModel.OnModified);
};

var Constraint = function (data, publishModel) {
    var self = this;
    var mapping = {
        'include': ['Id', 'TemplateId', 'DisplayText', 'HeadingDescription', 'ConstraintDescription', 'ConstraintLabel', 'PrimitiveText', 'IsPrimitive', 'IsHeading', 'Samples', 'ChildConstraints'],
        'Samples': {
            create: function (options) {
                return new ConstraintSample(options.data, publishModel);
            }
        },
        'ChildConstraints': {
            create: function (options) {
                return new Constraint(options.data, publishModel);
            }
        }
    };

    self.Id = ko.observable('');
    self.TemplateId = ko.observable('');
    self.DisplayText = ko.observable('');
    self.HeadingDescription = ko.observable('');
    self.ConstraintDescription = ko.observable('');
    self.ConstraintLabel = ko.observable('');
    self.PrimitiveText = ko.observable('');
    self.IsPrimitive = ko.observable('');
    self.IsHeading = ko.observable('');
    self.Samples = ko.observableArray([]);
    self.ChildConstraints = ko.observableArray([]);
    self.IsNew = ko.observable(false);
    self.InstanceIdentifier = ko.observable(GUID());

    ko.mapping.fromJS(data, mapping, self);

    // Validation
    self.IsValid = ko.computed(function () {
        var foundInvalid = ko.utils.arrayFirst(self.Samples(), function (sample) {
            return !sample.IsValid();
        });

        return !foundInvalid;
    });

    // Detect changes
    self.HeadingDescription.subscribe(publishModel.OnModified);
    self.ConstraintDescription.subscribe(publishModel.OnModified);
    self.ConstraintLabel.subscribe(publishModel.OnModified);
    self.PrimitiveText.subscribe(publishModel.OnModified);
    self.IsHeading.subscribe(publishModel.OnModified);
};

var PublishModel = function (data) {
    var self = this;
    var mapping = {
        'include': ['TemplateId', 'TemplateName', 'TemplateOid', 'XmlSamples', 'Constraints'],
        'XmlSamples': {
            create: function (options) {
                return new XmlSample(options.data, self);
            }
        },
        'Constraints': {
            create: function (options) {
                return new Constraint(options.data, self);
            }
        }
    };

    self.TemplateId = ko.observable(data.TemplateId);
    self.TemplateName = ko.observable(data.TemplateName);
    self.TemplateOid = ko.observable(data.TemplateOid);
    self.XmlSamples = ko.observableArray(data.XmlSamples || []);
    self.Constraints = ko.observableArray(data.Constraints || []);

    self.TemplateDisplay = ko.computed(function () {
        return self.TemplateName() + " (" + self.TemplateOid() + ")";
    });

    ko.mapping.fromJS(data, mapping, self);
};