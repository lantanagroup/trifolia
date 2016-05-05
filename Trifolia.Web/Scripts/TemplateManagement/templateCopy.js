var CopyModel = function (templateId, isNewVersion) {
    var self = this;
    var mapping = {
        include: [ 'TemplateId', 'Bookmark', 'Constraints', 'ImplementationGuideId', 'IsNewVersion', 'Message', 'Name', 'NewVersionImplementationGuideName', 'Oid', 'OriginalName', 'SubmitEnabled', 'Type' ],
        'ignore': ["CurrentStep", "IsStep1", "IsStep2", "NameIsValid", "NameInvalidMessage"]
    }

    self.TemplateId = ko.observable();
    self.OriginalName = ko.observable('');
    self.Name = ko.observable('');
    self.Oid = ko.observable('');
    self.Bookmark = ko.observable('');
    self.ImpliedTemplateId = ko.observable();
    self.ImplementationGuideId = ko.observable('');
    self.NewVersionImplementationGuideName = ko.observable();
    self.Type = ko.observable();
    self.Message = ko.observable();
    self.IsNewVersion = ko.observable(isNewVersion);
    self.SubmitEnabled = ko.observable(false);
    self.Constraints = ko.observableArray([]);

    self.ImplementationGuides = ko.observableArray([]);
    self.CopyModeDisplay = ko.computed(function () {
        if (self.IsNewVersion()) {
            return 'Version';
        }

        return 'Copy';
    });

    /* CUSTOM PROPERTIES */
    self.CurrentStep = ko.observable(1);

    self.IsStep1 = ko.computed(function () {
        return self.CurrentStep() == 1;
    });

    self.IsStep2 = ko.computed(function () {
        return self.CurrentStep() == 2;
    });

    /* VALIDATION */
    var selfValid = ko.validatedObservable({
        Name: self.Name.extend({
            required: true,
            maxLength: 255
        }),
        Oid: self.Oid.extend({
            required: true,
            maxLength: 255,
            templateIdentifierUnique: false,
            hl7iiValidation: false,
            templateOidFormat: true
        }),
        Bookmark: self.Bookmark.extend({
            required: true
        }),
        ImplementationGuideId: self.ImplementationGuideId.extend({
            required: true
        })
    });

    self.IsValid = ko.computed(function () {
        return selfValid.isValid();
    });
    
    self.Name.subscribe(function (newValue) {
        $.ajax({
            url: '/api/Template/' + self.TemplateId() + '/Bookmark?newTemplateName=' + encodeURIComponent(newValue),
            success: function(data) {
                self.Bookmark(data);
            }
        });
    });

    self.HasConfConflicts = function () {
        var ret = false;
        
        for (var i = 0; i < self.Constraints().length; i++) {
            var cConstraint = self.Constraints()[i];

            if (cConstraint.NumberReplacementType() != 0) {
                ret = true;
                break;
            }
        }

        return ret;
    };

    self.GetConstraints = function(parentNumber) {
        var ret = [];

        for (var i = 0; i < self.Constraints().length; i++) {
            var cConstraint = self.Constraints()[i];
            if (cConstraint.ParentNumber() == parentNumber) {
                ret.push(cConstraint);
            }
        }

        return ret;
    }

    self.GetConstraint = function (number) {
        for (var i = 0; i < self.Constraints().length; i++) {
            var cConstraint = self.Constraints()[i];
            if (cConstraint.Number() == number) {
                return cConstraint;
            }
        }
        return null;
    }

    self.CompleteStep1 = function () {
        var jsonData = ko.mapping.toJSON(self, mapping);
        $.ajax({
            url: "/api/Template/Constraint/Conflict",
            type: 'POST',
            dataType: 'json',
            data: jsonData,
            contentType: 'application/json; charset=utf-8',
            success: function(jsonConstraints) {
                ko.mapping.fromJS(jsonConstraints, null, self.Constraints);
                self.CurrentStep(2);
            }
        });
    }

    self.RegenerateAllNew = function () {
        for (var i = 0; i < self.Constraints().length; i++) {
            var currentConstraint = self.Constraints()[i];

            currentConstraint.NumberReplacementType(1);     // Set to "Replace This"
        }
    };

    self.RegenerateAllOriginal = function () {
        for (var i = 0; i < self.Constraints().length; i++) {
            var currentConstraint = self.Constraints()[i];

            currentConstraint.NumberReplacementType(2);     // Set to "Replace Other"
        }
    };

    self.FinishButtonClicked = function () {
        var jsonData = ko.mapping.toJSON(self, mapping);
        $.ajax({
            url: "/api/Template/Copy",
            type: 'POST',
            dataType: 'json',
            data: jsonData,
            contentType: 'application/json; charset=utf-8',
            success: function(data) {
                if (data.Status == "Success") {
                    location.href = '/TemplateManagement/Edit/Id/' + data.TemplateId;
                } else {
                    alert(data.Message);
                }
            }
        });
    };

    self.CancelButtonClicked = function () {
        location.href = '/TemplateManagement/View/Id/' + self.TemplateId();
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/ImplementationGuide/Editable',
            async: false,
            success: function (data) {
                ko.mapping.fromJS({ ImplementationGuides: data }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Template/' + templateId + '/Copy?newVersion=' + isNewVersion,
            success: function (data) {
                ko.mapping.fromJS(data, mapping, self);
            }
        });
    };

    self.Initialize();
};