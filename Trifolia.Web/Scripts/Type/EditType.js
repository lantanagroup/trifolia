ko.validation.rules['uniqueTemplateTypeName'] = {
    validator: function (val, otherVal) {
        var foundSameName = null;

        var templateTypes = otherVal.TemplateTypes;
        var thisTemplateType = otherVal.ThisTemplateType;

        var ret = ko.utils.arrayFirst(templateTypes(), function (item) {
            return JSON.stringify(ko.mapping.toJS(item)) != JSON.stringify(ko.mapping.toJS(thisTemplateType)) && item.Name() == val;
        });

        return !ret;
    },
    message: 'Name must be unique'
};
ko.validation.registerExtenders();

var EditTypeViewModel = function (implementationGuideTypeId) {
    var self = this;
    
    self.SchemaLocations = ko.observableArray([]);

    self.Model = ko.observable(new ImplementationGuideTypeModel());
    self.CurrentTemplateType = ko.observable();
    self.CurrentTemplateTypeIndex = ko.observable();
    self.SelectedComplexType = ko.observable();
    self.SelectedDataType = ko.observable();

    self.Model().SchemaFile.subscribe(function (newSchemaFile) {
        if (!newSchemaFile) {
            return;
        }

        var fileName = self.Model().SchemaFileInfo().name;

        if (fileName.lastIndexOf('.xsd') == fileName.length - 4) {
            self.SchemaLocations([ fileName ]);
            self.Model().SchemaLocation(fileName);
            return;
        } else if (fileName.lastIndexOf('.zip') == fileName.length - 4) {
            $.blockUI({ message: 'Inspecting ZIP package...' });
            $.ajax({
                method: 'POST',
                url: '/api/Type/Zip',
                data: JSON.stringify(newSchemaFile),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (results) {
                    self.SchemaLocations(results);
                },
                error: function (ex) {
                    var exception = JSON.parse(ex.responseText);
                    alert(exception.ExceptionMessage);
                },
                complete: function () {
                    $.unblockUI();
                }
            });
        } else {
            alert('Invalid schema file selected! Must be either a ZIP package or a single .xsd file.');
        }
    });

    var sortTypes = function () {
        self.Model().ComplexTypes.sort(function (left, right) {
            return left == right ? 0 : (left < right ? -1 : 1);
        });
    };

    self.AssignComplexType = function () {
        var complexType = self.SelectedComplexType();
        self.Model().DataTypes.push(complexType);
        self.Model().ComplexTypes.remove(complexType);
        self.SelectedDataType(complexType);
    };

    self.RemoveDataType = function () {
        var dataType = self.SelectedDataType();
        self.Model().ComplexTypes.push(dataType);
        self.Model().DataTypes.remove(dataType);
        self.SelectedComplexType(dataType);
    };

    self.Save = function () {
        var data = ko.mapping.toJS(self.Model());

        if (self.Model().SchemaFileInfo()) {
            data.SchemaFile = self.Model().SchemaFile();
            data.SchemaFileContentType = self.Model().SchemaFileInfo().type;
        }

        $.blockUI({ message: 'Saving...' });
        $.ajax({
            method: 'POST',
            url: '/api/Type/Save',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (results) {
                self.SchemaLocations([]);
                self.SchemaLocations.push(results.SchemaLocation);
                ko.mapping.fromJS({ Model: results }, {}, self);
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Cancel = function () {
        location.href = '/IGTypeManagement/List';
    };

    self.AddTemplateType = function () {
        var tempTemplateType = new TemplateTypeModel(null, self.Model());
        tempTemplateType.OutputOrder(0);
        
        ko.utils.arrayForEach(self.Model().TemplateTypes(), function (templateType) {
            if (tempTemplateType.OutputOrder() < templateType.OutputOrder()) {
                tempTemplateType.OutputOrder(templateType.OutputOrder());
            }
        });

        tempTemplateType.OutputOrder(tempTemplateType.OutputOrder() + 1);

        self.CurrentTemplateType(tempTemplateType);
        $('#editTemplateTypeModal').modal('show');
    };

    self.EditTemplateType = function (templateType) {
        var tempTemplateType = new TemplateTypeModel(ko.mapping.toJS(templateType), self.Model());
        self.CurrentTemplateType(tempTemplateType);
        self.CurrentTemplateTypeIndex(self.Model().TemplateTypes.indexOf(templateType));
        $('#editTemplateTypeModal').modal('show');
    };

    var sortTemplateTypes = function () {
        self.Model().TemplateTypes.sort(function (left, right) {
            return left.OutputOrder() == right.OutputOrder() ? 0 : (left.OutputOrder() < right.OutputOrder() ? -1 : 1);
        });
    };

    self.DeleteTemplateType = function (templateType) {
        if (templateType.TemplateCount() > 0) {
            alert("You cannot delete this template/profile type because it is associated with templates. Move all templates to a different template/profile type to delete this template type.");
            return;
        }

        ko.utils.arrayForEach(self.Model().TemplateTypes(), function (item) {
            if (item.OutputOrder() > templateType.OutputOrder()) {
                item.OutputOrder(item.OutputOrder() - 1);
            }
        });

        var index = self.Model().TemplateTypes.indexOf(templateType);
        self.Model().TemplateTypes.splice(index, 1);

        // Don't delete new template types that haven't been saved yet
        if (templateType.Id()) {
            self.Model().DeletedTemplateTypes.push(templateType);
        }
    };

    self.MoveTemplateTypeUp = function (templateType) {
        var previousOrder = templateType.OutputOrder() - 1;
        var thisOrder = templateType.OutputOrder();
        var previousTemplateType = ko.utils.arrayFirst(self.Model().TemplateTypes(), function (item) {
            return item.OutputOrder() == previousOrder;
        });

        templateType.OutputOrder(previousOrder);
        previousTemplateType.OutputOrder(thisOrder);
        sortTemplateTypes();
    };

    self.MoveTemplateTypeDown = function (templateType) {
        var nextOrder = templateType.OutputOrder() + 1;
        var thisOrder = templateType.OutputOrder();
        var nextTemplateType = ko.utils.arrayFirst(self.Model().TemplateTypes(), function (item) {
            return item.OutputOrder() == nextOrder;
        });

        templateType.OutputOrder(nextOrder);
        nextTemplateType.OutputOrder(thisOrder);
        sortTemplateTypes();
    };
    
    self.SaveTemplateType = function () {
        if (self.CurrentTemplateTypeIndex()) {
            self.Model().TemplateTypes.splice(self.CurrentTemplateTypeIndex(), 1, self.CurrentTemplateType());
        } else {
            self.Model().TemplateTypes.push(self.CurrentTemplateType());
        }

        self.CurrentTemplateType(null);
        self.CurrentTemplateTypeIndex(null);
        $('#editTemplateTypeModal').modal('hide');
    };

    self.CancelEditTemplateType = function () {
        self.CurrentTemplateType(null);
        self.CurrentTemplateTypeIndex(null);
        $('#editTemplateTypeModal').modal('hide');
    };

    self.Initialize = function () {
        self.SchemaLocations([]);

        if (!implementationGuideTypeId) {
            return;
        }

        $.ajax({
            url: '/api/Type/' + implementationGuideTypeId,
            success: function (results) {
                self.SchemaLocations.push(results.SchemaLocation);
                ko.mapping.fromJS({ Model: results }, {}, self);
            }
        });
    };

    self.Initialize();
};

var ImplementationGuideTypeModel = function (data) {
    var self = this;
    var mapping = {
        'TemplateTypes': {
            create: function (options) {
                return new TemplateTypeModel(options.data, self);
            }
        },
        include: [ 'Id', 'Name', 'SchemaLocation', 'SchemaUri', 'SchemaPrefix', 'TemplateTypes', 'DeletedTemplateTypes', 'SchemaFile', 'DataTypes', 'ComplexTypes' ]
    };

    // Server
    self.Id = ko.observable();
    self.Name = ko.observable();
    self.SchemaLocation = ko.observable();
    self.SchemaUri = ko.observable();
    self.SchemaPrefix = ko.observable();
    self.TemplateTypes = ko.observableArray([]);
    self.DeletedTemplateTypes = ko.observableArray([]);
    self.SchemaFile = ko.observable();
    self.SchemaFileContentType = ko.observable();
    self.ComplexTypes = ko.observableArray([]);
    self.DataTypes = ko.observableArray([]);

    // Client
    self.SchemaFileInfo = ko.observable();

    ko.mapping.fromJS(data, mapping, self);
    
    self.Validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true }),
        SchemaLocation: self.SchemaLocation.extend({ required: true }),
        SchemaUri: self.SchemaUri.extend({ required: true }),
        SchemaPrefix: self.SchemaPrefix.extend({ required: true })
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });
};

var TemplateTypeModel = function (data, igTypeModel) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'OutputOrder', 'Context', 'ContextType']
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.OutputOrder = ko.observable();
    self.Context = ko.observable();
    self.ContextType = ko.observable();
    self.TemplateCount = ko.observable(0);

    ko.mapping.fromJS(data, mapping, self);

    self.Validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, uniqueTemplateTypeName: { TemplateTypes: igTypeModel.TemplateTypes, ThisTemplateType: self } }),
        Context: self.Context.extend({ required: true }),
        ContextType: self.ContextType.extend({ required: true }),
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });
};