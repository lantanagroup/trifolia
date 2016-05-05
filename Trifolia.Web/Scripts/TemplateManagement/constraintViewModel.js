constraintViewModel = function () {
    var self = this;

    self.constraintModel = ko.observable(new constraint({}));
    self.currentConstraint = ko.observable(new constraint({}));
    self.currentConstraintSamples = ko.observableArray([]);
    self.currentConstraintSample = ko.observable(new constraintSample({}));
    
    var bindConstraintEditor = function (constraint) {
        $("#constraintDescription").databind({
            value: constraint.ConstraintDescription,
            valueUpdate: 'afterkeydown'
        });

        $("#constraintLabel").databind({
            value: constraint.ConstraintLabel,
            valueUpdate: 'afterkeydown'
        });

        $("#primitiveTextLabel").databind({
            visible: constraint.IsPrimitive
        });

        $("#primitiveText").databind({
            value: constraint.PrimitiveText,
            valueUpdate: 'afterkeydown',
            visible: constraint.IsPrimitive
        });

        $("#headingDescriptionLabel").databind({
            visible: constraint.IsHeading
        });

        $("#headingDescription").databind({
            value: constraint.HeadingDescription,
            valueUpdate: 'afterkeydown',
            visible: constraint.IsHeading
        });

        $("#constraintSamplesDiv").databind({
            visible: constraint.IsHeading
        });
    };

    var unbindConstraintEditor = function () {
        $("#constraintDescription").undatabind();
        $("#constraintLabel").undatabind();
        $("#primitiveTextLabel").undatabind();
        $("#primitiveText").undatabind();

        $("#headingDescriptionLabel").undatabind();
        $("#headingDescription").undatabind();
        $("#constraintSamplesDiv").undatabind();
    };

    self.editConstraint = function (editedConstraint) {
        self.constraintModel(editedConstraint);
        var constraintJS = ko.mapping.toJS(editedConstraint);
        var constraintClone = new constraint({});
        ko.mapping.fromJS(constraintJS, constraintMap, constraintClone);
        self.currentConstraint(constraintClone);

        self.currentConstraint().IsNew(false);
        self.currentConstraint().IsEdited(true);
        self.currentConstraint().InstanceIdentifier(editedConstraint.InstanceIdentifier());

        var samples = $.grep(self.currentConstraint().Samples(), function (existingSample) {
            return !existingSample.IsDeleted();
        });

        self.currentConstraintSamples(samples);

        bindConstraintEditor(self.currentConstraint());
        $("#constraintEditor").modal("show");
    };

    self.acceptConstraintChanges = function () {
        var updatedJS = ko.mapping.toJS(self.currentConstraint());
        ko.mapping.fromJS(updatedJS, constraintMap, self.constraintModel());
        unbindConstraintEditor();
    };

    self.cancelConstraintEdit = function () {
        unbindConstraintEditor();
        self.constraintModel(new constraint({}));
    };

    self.saveConstraintSample = function () {
        if (self.currentConstraintSample().IsNew()) {
            var newSample = new constraintSample(ko.mapping.toJS(self.currentConstraintSample()));
            newSample.IsNew(false);
            self.currentConstraintSamples.push(newSample);
            self.currentConstraint().Samples.push(newSample);
        } else if (self.currentConstraint().IsEdited()) {
            var updatedJS = ko.mapping.toJS(self.currentConstraintSample());

            var match = ko.utils.arrayFirst(self.currentConstraint().Samples(), function (existingSample) {
                return self.currentConstraintSample().InstanceIdentifier() == existingSample.InstanceIdentifier();
            });

            if (match != null) {
                ko.mapping.fromJS(updatedJS, constraintSampleMap, match);
            }
        }
    };

    self.addConstraintSample = function () {
        self.currentConstraintSample(new constraintSample({}));
        self.currentConstraintSample().IsNew(true);
        self.currentConstraintSample().IsEdited(false);

        $("#constraintSampleEditor").modal("show");
        $("#constraintEditor").modal("hide");
        self.currentConstraintSample.valueHasMutated();
    };

    self.cancelConstraintSampleEdit = function () {
    };

    self.editConstraintSample = function (sample) {
        var sampleJS = ko.mapping.toJS(sample);
        self.currentConstraintSample(new constraintSample(sampleJS));
        self.currentConstraintSample().IsEdited(true);
        self.currentConstraintSample().IsNew(false);
        self.currentConstraintSample().InstanceIdentifier(sample.InstanceIdentifier());

        $("#constraintSampleEditor").modal("show");
        $("#constraintEditor").modal("hide");
    };

    self.removeConstraintSample = function (sample) {
        var match = ko.utils.arrayFirst(self.currentConstraint().Samples(), function (existingSample) {
            return sample.InstanceIdentifier() == existingSample.InstanceIdentifier();
        });

        if (match == null) {
            alert('Could not find the removed sample in the existing collection of samples');
            return;
        }

        match.IsDeleted(true);
        self.currentConstraintSamples.remove(sample);
    };
};