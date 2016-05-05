publishSettingsViewModel = function (templateId) {
    var self = this;

    self.Model = ko.observable();
    self.CurrentConstraint = ko.observable();
    self.OriginalCurrentConstraint = ko.observable();
    self.CurrentConstraintSample = ko.observable();
    self.OriginalConstraintSample = ko.observable();
    self.CurrentTemplateSample = ko.observable();
    self.OriginalCurrentTemplateSample = ko.observable();
    self.IsModified = ko.observable(false);
    self.PopupIsModified = ko.observable(false);

    self.OnModified = function () {
        self.PopupIsModified(true);
    };

    self.TidyTemplateSample = function () {
        var prettyXml = vkbeautify.xml(self.CurrentTemplateSample().SampleText());
        self.CurrentTemplateSample().SampleText(prettyXml);
    };

    self.GenerateTemplateSample = function () {
        $.ajax({
            url: '/api/Template/' + templateId + '/GenerateSample',
            success: function (results) {
                self.CurrentTemplateSample().SampleText(results);
            }
        });
    };

    self.EditTemplateSample = function (templateSample) {
        if (templateSample) {
            self.OriginalCurrentTemplateSample(templateSample);
            var tempTemplateSample = new XmlSample(ko.mapping.toJS(templateSample), self);
            self.CurrentTemplateSample(tempTemplateSample);
        } else {
            self.CurrentTemplateSample(new XmlSample(null, self));
        }

        self.PopupIsModified(false);
        $('#templateSampleEditorDialog').modal('show');
    };

    self.CancelEditTemplateSample = function () {
        self.CurrentTemplateSample(null);
        self.OriginalCurrentTemplateSample(null);

        $('#templateSampleEditorDialog').modal('hide');
    };

    self.SaveTemplateSample = function () {
        var templateSample = self.OriginalCurrentTemplateSample();
        var tempTemplateSample = self.CurrentTemplateSample();

        if (!templateSample) {
            self.Model().XmlSamples.push(tempTemplateSample);
        } else {
            templateSample.Name(tempTemplateSample.Name());
            templateSample.SampleText(tempTemplateSample.SampleText());
        }

        self.CancelEditTemplateSample();

        if (self.PopupIsModified()) {
            self.IsModified(true);
        }
    };

    self.RemoveTemplateSample = function (templateSample) {
        if (!confirm("Are you sure you want to remove this template/profile sample?")) {
            return;
        }

        if (templateSample.Id()) {
            templateSample.IsDeleted(true);
        } else {
            self.Model().XmlSamples.remove(templateSample);
        }
    };

    self.GetTemplateSamples = function () {
        return ko.utils.arrayFilter(self.Model().XmlSamples(), function (templateSample) {
            return !templateSample.IsDeleted();
        });
    };

    self.EditConstraint = function (constraint) {
        var tempConstraint = new Constraint(ko.mapping.toJS(constraint), self);
        self.CurrentConstraint(tempConstraint);
        self.OriginalCurrentConstraint(constraint);
        self.PopupIsModified(false);
        $('#editConstraintDialog').modal('show');
    };

    self.CancelEditConstraint = function () {
        self.CurrentConstraint(null);
        self.OriginalCurrentConstraint(null);
        $('#editConstraintDialog').modal('hide');
    };

    self.SaveConstraint = function () {
        var constraint = self.OriginalCurrentConstraint();
        var tempConstraint = self.CurrentConstraint();

        constraint.IsHeading(tempConstraint.IsHeading());
        constraint.HeadingDescription(tempConstraint.HeadingDescription());
        constraint.ConstraintDescription(tempConstraint.ConstraintDescription());
        constraint.ConstraintLabel(tempConstraint.ConstraintLabel());
        constraint.Samples(tempConstraint.Samples());

        self.CancelEditConstraint();

        if (self.PopupIsModified()) {
            self.IsModified(true);
        }
    };

    self.EditConstraintSample = function (constraintSample) {
        if (constraintSample) {
            self.OriginalConstraintSample(constraintSample);
            var tempConstraintSample = new ConstraintSample(ko.mapping.toJS(constraintSample), self);
            self.CurrentConstraintSample(tempConstraintSample);
        } else {
            self.CurrentConstraintSample(new ConstraintSample(null, self));
        }

        $('#editConstraintDialog').modal('hide');
        $('#editConstraintSampleDialog').modal('show');
    };

    self.CancelEditConstraintSample = function () {
        self.OriginalConstraintSample(null);
        self.CurrentConstraintSample(null);

        $('#editConstraintDialog').modal('show');
        $('#editConstraintSampleDialog').modal('hide');
    }

    self.SaveConstraintSample = function () {
        var constraintSample = self.OriginalConstraintSample();
        var tempConstraintSample = self.CurrentConstraintSample();

        if (!constraintSample) {
            self.CurrentConstraint().Samples.push(tempConstraintSample);
        } else {
            constraintSample.Name(tempConstraintSample.Name());
            constraintSample.SampleText(tempConstraintSample.SampleText());
        }

        self.CancelEditConstraintSample();
    };

    self.RemoveConstraintSample = function (constraintSample) {
        if (!confirm("Are you sure you want to delete this constraint sample?")) {
            return;
        }

        if (!constraintSample.Id()) {
            self.CurrentConstraint().Samples.remove(constraintSample);
        } else {
            constraintSample.IsDeleted(true);
        }
    };

    self.GetConstraintSamples = ko.computed(function () {
        if (!self.CurrentConstraint()) {
            return [];
        }

        return ko.utils.arrayFilter(self.CurrentConstraint().Samples(), function (sample) {
            return !sample.IsDeleted();
        });
    });

    self.Save = function () {
        var data = ko.mapping.toJS(self.Model());

        $.blockUI({ message: 'Saving...' });
        $.ajax({
            method: 'POST',
            url: '/api/Template/PublishSettings/Save',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function () {
                self.Initialize();
                self.IsModified(false);
                alert("Successfully saved publish settings");
            },
            error: function (ex) {
                console.log(ex.responseText);
                alert("An error occurred while saving the publish settings.");
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Cancel = function () {
        location.href = '/TemplateManagement/View/Id/' + templateId;
    };

    self.ConfirmLeave = function () {
        if (self.IsModified()) {
            return 'Changes have been made to the template. Are you sure you want to leave?';
        }
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Template/' + templateId + '/PublishSettings',
            success: function (results) {
                var model = new PublishModel(results, self);
                self.Model(model);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while loading");
            }
        });
    };

    self.Initialize();
};