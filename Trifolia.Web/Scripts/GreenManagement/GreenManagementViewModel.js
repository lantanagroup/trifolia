
function camelize(str) {
    return str.replace(/(?:^\w|[A-Z]|\b\w)/g, function(letter, index) {
        return index == 0 ? letter.toLowerCase() : letter.toUpperCase();
    }).replace(/\s+/g, '');
}

var greenManagementViewModel = function (dataLoadUrl, saveDataUrl, greenDataTypesUrl) {
    var self = this;

    self.loadUrl = dataLoadUrl;
    self.saveDataUrl = saveDataUrl;
    self.observableModel = ko.observable(new template());
    self.templateConstraints = [];
    self.businessNameEventName = 'blur.businessName';
    self.CurrentConstraint = ko.observable();
    self.CurrentRealConstraint = ko.observable();
    self.DataTypes = ko.observableArray([]);

    self.GetConstraintPadding = function (parents) {
        var count = (parents.length - 1) * 15;
        return count + 'px';
    };

    self.GetDataTypeName = function (dataTypeId) {
        if (!dataTypeId) {
            return '';
        }

        var found = ko.utils.arrayFirst(self.DataTypes(), function (item) {
            return item.datatypeId() == dataTypeId;
        });

        if (found) {
            return found.datatype;
        }

        return '';
    };

    self.Initialize = function () {
        $('body').block({ message: "<h1>Loading Green Template ...</h1><br/>" });
        $.getJSON(self.loadUrl, function (aModel) {
            $('body').unblock();

            self.observableModel(new template(aModel));
            self.observableModel().dirtyFlag.reset();

            if (aModel.IsNew === true) {
                alert('This is a temporary green template that has not been saved yet.  It was created because a green template did not yet exist.  Modify this template as necessary, then click Save to save this green template', true);
            }
        });

        $.getJSON(greenDataTypesUrl, function (aModel) {
            ko.mapping.fromJS({ DataTypes: aModel }, {}, self);
        });
    };

    self.DeleteConstraint = function (aConstraint) {
        aConstraint.isDeleted('true');
        aConstraint.hasGreenConstraint(false);
        aConstraint.businessName('');
        aConstraint.elementName('');
        aConstraint.datatype('');
        aConstraint.datatypeId(null);
    };

    self.EditConstraint = function (aConstraint) {
        var tempConstraint = new constraint(ko.mapping.toJS(aConstraint));
        self.CurrentRealConstraint(aConstraint);
        self.CurrentConstraint(tempConstraint);
        $('#constraintEditorDialog').modal('show');
    };

    self.SaveConstraint = function () {
        self.CurrentRealConstraint().hasGreenConstraint(self.CurrentConstraint().hasGreenConstraint());
        self.CurrentRealConstraint().businessName(self.CurrentConstraint().businessName());
        self.CurrentRealConstraint().elementName(self.CurrentConstraint().elementName());
        self.CurrentRealConstraint().datatypeId(self.CurrentConstraint().datatypeId());

        self.CurrentConstraint(null);
        self.CurrentRealConstraint(null);
        $('#constraintEditorDialog').modal('hide');
    };

    self.CancelEditConstraint = function () {
        self.CurrentConstraint(null);
        self.CurrentRealConstraint(null);
        $('#constraintEditorDialog').modal('hide');
    };

    self.Cancel = function () {
        if (self.observableModel().TemplateId()) {
            location.href = '/TemplateManagement/View/Id/' + self.observableModel().TemplateId();
        } else {
            location.href = '/TemplateManagement/List';
        }
    };

    self.Save = function () {
        var stringData = JSON.stringify(ko.mapping.toJS(self.observableModel));

        $('#mainBody').block({ message: "Saving To Server ..." });

        $.ajax({
            url: self.saveDataUrl,
            type: 'POST',
            dataType: 'json',
            data: stringData,
            contentType: 'application/json; charset=utf-8',
            complete: function (jqXHR, textStatus) {
                $('#mainBody').unblock();

                if (textStatus != 'success') {
                    alert('There was an error saving your changes; please submit a support ticket');
                }
            },
            success: function (saveResult) {
                if (saveResult.FailedValidation) {
                    alert(saveResult.ValidationMessage);
                    return;
                }

                self.observableModel(new template(saveResult.ViewModel));
                alert('Changes Saved Successfully');
            }
        });
    };

    self.Initialize();
};