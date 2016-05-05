ko.validation.rules['elementNameValid'] = {
    validator: function (val, otherVal) {
        var validElementNameRegex = /^(?!\d)[A-Za-z0-9_]\w*$/;
        var isValid = validElementNameRegex.test(val);
        return isValid;
    },
    message: 'Element name is not a validate XML name. It must not contain special characters.'
};
ko.validation.registerExtenders();

constraint = function (data) {
    var self = this;

    self.id = ko.observable('');
    self.templateId = ko.observable('');
    self.text = ko.observable('');
    self.headingDescription = ko.observable('');
    self.constraintDescription = ko.observable('');
    self.constraintLabel = ko.observable('');
    self.primitiveText = ko.observable('');
    self.datatype = ko.observable('');
    self.datatypeId = ko.observable('');
    self.isPrimitive = ko.observable('');
    self.isHeading = ko.observable('');
    self.parentConstraintId = ko.observable('');
    self.order = ko.observable('');
    self.number = ko.observable('');
    self.elementName = ko.observable('');
    self.businessName = ko.observable('');
    self.greenConstraintId = ko.observable('');
    self.parentGreenConstraintId = ko.observable('');
    self.hasGreenConstraint = ko.observable('');
    self.isDeleted = ko.observable('');
    self.xPath = ko.observable('');
    self.children = ko.observableArray([]);

    self.IsNew = ko.observable(false);
    self.IsEdited = ko.observable(false);
    self.InstanceIdentifier = ko.observable(GUID());

    ko.mapping.fromJS(data, constraintMap, self);

    self.Validation = ko.validatedObservable({
        elementName: self.elementName.extend({ required: true, maxLength: 255, elementNameValid: true }),
        businessName: self.businessName.extend({ required: true, maxLength: 125 }),
        datatypeId: self.datatypeId.extend({ required: true })
    });
};

constraintMap = {
    'include': ['id', 'templateId', 'text', 'headingDescription', 'constraintDescription',
        'constraintLabel', 'primitiveText', 'datatype', 'datatypeId', 'isPrimitive', 'isHeading', 'parentConstraintId',
        'order', 'number', 'elementName', 'businessName', 'greenConstraintId', 'parentGreenConstraintId',
        'hasGreenConstraint', 'isDeleted', 'xPath', 'IsNew'],
    'children': {
        create: function (options) {
            return new constraint(options.data);
        }
    }
};

template = function (data) {
    var self = this;

    self.Id = ko.observable('');
    self.TemplateId = ko.observable('');
    self.Name = ko.observable('');
    self.TemplateName = ko.observable('');
    self.TemplateOid = ko.observable('');
    self.childConstraints = ko.observableArray([]);
    self.IsNew = ko.observable(false);

    ko.mapping.fromJS(data, templateMap, self);
    self.dirtyFlag = ko.dirtyFlag(self);
};

templateMap = {
    'include': ['Id', 'TemplateId', 'Name', 'TemplateName', 'TemplateOid', 'IsNew'],
    'childConstraints': {
        create: function (options) {
            return new constraint(options.data);
        }
    }
}