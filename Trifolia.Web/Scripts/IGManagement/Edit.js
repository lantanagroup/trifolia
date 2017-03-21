ko.validation.rules['uniqueImplementationGuideName'] = {
    validator: function (val, otherVal) {
        var implementationGuideId = otherVal();
        var ret = false;

        $.ajax({
            url: '/api/ImplementationGuide/Validate/Name?implementationGuideId=' + implementationGuideId + '&igName=' + encodeURIComponent(val),
            async: false,
            cache: false,
            success: function (isValid) {
                ret = isValid;
            }
        });

        return ret;
    },
    message: 'The name specified is already in use'
};

ko.validation.rules['uniqueImplementationGuideIdentifier'] = {
    validator: function (val, otherVal) {
        var implementationGuideId = otherVal();
        var ret = false;

        $.ajax({
            url: '/api/ImplementationGuide/Validate/Identifier?implementationGuideId=' + implementationGuideId + '&identifier=' + encodeURIComponent(val),
            async: false,
            cache: false,
            success: function (isValid) {
                ret = isValid;
            }
        });

        return ret;
    },
    message: 'The identifier specified is already in use'
};

ko.validation.rules['categoryText'] = {
    validator: function (val, otherVal) {
        var regex = new RegExp(',', 'g');

        if (regex.test(val)) {
            return false;
        }

        return true;
    },
    message: 'The category cannot contain commas (,)'
};

ko.validation.rules['categoriesValid'] = {
    getValue: function (o) {
        return (typeof o === 'function' ? o() : o);
    },
    validator: function (val, fields) {
        for (var i in val) {
            var current = val[i];
            
            if (!current.Validation().isValid()) {
                return false;
            }
        }

        return true;
    },
    message: 'One of the categories is invalid'
};

ko.validation.registerExtenders();

var CategoryModel = function (value) {
    var self = this;

    self.Value = ko.observable(value);

    self.Validation = ko.validatedObservable({
        Value: self.Value.extend({ categoryText: true })
    });
};

var EditImplementationGuideViewModel = function (implementationGuideId) {
    var self = this;

    self.ImplementationGuideId = ko.observable(implementationGuideId);
    self.Model = ko.observable(new ImplementationGuideModel());
    self.ImplementationGuideTypes = ko.observableArray([]);
    self.CurrentCustomSchematron = ko.observable();
    self.CurrentCustomSchematronIndex = ko.observable();
    self.CurrentSection = ko.observable();
    self.CurrentSectionIndex = ko.observable();
    self.PermissionSearchResults = ko.observableArray([]);
    self.PermissionSearchText = ko.observable();
    self.NewCategory = ko.observable();
    self.Organizations = ko.observableArray([]);
    self.DisableIdentifier = ko.observable(false);
    self.EditType = ko.computed(function () {
        return self.Model().Id() ? 'Edit' : 'Add';
    });

    self.NewCategoryValid = ko.validatedObservable({
        NewCategory: self.NewCategory.extend({ categoryText: true })
    });

    self.EditCustomSchematron = function (customSchematron) {
        if (!customSchematron) {
            self.CurrentCustomSchematron(new CustomSchematronModel());
            self.CurrentCustomSchematronIndex(null);
        } else {
            var index = self.Model().CustomSchematrons.indexOf(customSchematron);
            var tempCustomSchematron = new CustomSchematronModel(ko.mapping.toJS(customSchematron));
            self.CurrentCustomSchematron(tempCustomSchematron);
            self.CurrentCustomSchematronIndex(index);
        }

        $('#customSchematronDialog').modal('show');
    };

    self.RemoveCustomSchematron = function (customSchematron) {
        if (!confirm("Are you sure you want to remove this custom schematron?")) {
            return;
        }

        if (customSchematron.Id()) {
            self.Model().DeletedCustomSchematrons.push(customSchematron.Id());
        }

        self.Model().CustomSchematrons.remove(customSchematron);
    };

    self.CancelEditCustomSchematron = function () {
        self.CurrentCustomSchematron(null);
        self.CurrentCustomSchematronIndex(null);
        $('#customSchematronDialog').modal('hide');
    };

    self.SaveCustomSchematron = function () {
        if (self.CurrentCustomSchematronIndex() >= 0) {
            self.Model().CustomSchematrons.splice(self.CurrentCustomSchematronIndex(), 1, self.CurrentCustomSchematron());
        } else {
            self.Model().CustomSchematrons.push(self.CurrentCustomSchematron());
        }

        self.CurrentCustomSchematron(null);
        self.CurrentCustomSchematronIndex(null);
        $('#customSchematronDialog').modal('hide');
    };

    self.GetPermissionTypeName = function (permissionType) {
        switch (permissionType) {
            case 0:
                return "";
            case 1:
                return ", Group";
            case 2:
                return ", User";
        }

        return "Unknown";
    };

    self.SearchPermissions = function () {
        var params = '?includeGroups=true&searchText=' + encodeURIComponent(self.PermissionSearchText());

        $.ajax({
            url: '/api/ImplementationGuide/Edit/Permission/Search' + params,
            success: function (results) {
                self.PermissionSearchResults([]);
                ko.utils.arrayForEach(results, function (result) {
                    var newUserModel = new UserModel(result);
                    self.PermissionSearchResults.push(newUserModel);
                });
            }
        });
    };

    self.AddPermission = function (permissionType, permission) {
        var list = null;

        if (permissionType == 'View') {
            list = self.Model().ViewPermissions;
        } else if (permissionType == 'Edit') {
            list = self.Model().EditPermissions;
        }

        var foundPermission = ko.utils.arrayFirst(list(), function (currentPermission) {
            return currentPermission.Type() == permission.Type() && currentPermission.Id() == permission.Id();
        });

        if (!foundPermission) {
            list.push(permission);
        }
        
        // Doesn't make sense to have an Edit permission without a View permission. Make sure there is a View permission as well.
        if (permissionType == 'Edit') {
            self.AddPermission('View', permission);
        }
    };

    self.RemovePermission = function (type, permission) {
        var list = null;

        if (type == 'View') {
            list = self.Model().ViewPermissions;
        } else if (type == 'Edit') {
            list = self.Model().EditPermissions;
        }

        list.remove(permission);

        // Doesn't make sense to have an Edit permission without a View permission. Remove the Edit permission as well.
        if (type == 'View') {
            var foundPermission = _.find(self.Model().EditPermissions(), function (editPermission) {
                return editPermission.Id() == permission.Id() && editPermission.Type() == permission.Type();
            });

            if (foundPermission) {
                self.RemovePermission('Edit', foundPermission);
            }
        }
    };

    self.ImplementationGuideTypeChanged = function (newImplementationGuideTypeId) {
        $.ajax({
            url: '/api/ImplementationGuide/Edit/TemplateType?implementationGuideTypeId=' + newImplementationGuideTypeId,
            success: function (results) {
                self.Model().TemplateTypes([]);
                ko.utils.arrayForEach(results, function (templateType) {
                    var newTemplateTypeModel = new TemplateTypeModel(templateType);
                    self.Model().TemplateTypes.push(newTemplateTypeModel);
                });
            }
        });
    };

    /* Section functionality */
    
    self.GetSectionPadding = function (level) {
        var display = '';

        for (var i = 1; i < level; i++) {
            display += '    ';
        }

        return display;
    };

    self.CanMoveUp = function (index) {
        if (index == 0) {
            return false;
        }

        return true;
    };

    self.CanMoveDown = function (index) {
        var sections = self.Model().Sections();

        if (index == sections.length - 1) {
            return false;
        }
        
        return true;
    };

    self.CanMoveLeft = function (index) {
        var sections = self.Model().Sections();
        var current = sections[index];

        if (current.Level() == 1) {
            return false;
        }

        return true;
    };

    self.CanMoveRight = function (index) {
        var sections = self.Model().Sections();
        var current = sections[index];

        if (current.Level() == 6) {
            return false;
        }
        return true;
    };

    self.MoveLeft = function (index) {
        var current = self.Model().Sections()[index];
        current.Level(current.Level() - 1);
    };

    self.MoveRight = function (index) {
        var current = self.Model().Sections()[index];
        current.Level(current.Level() + 1);
    };

    self.MoveUp = function (index) {
        var sections = self.Model().Sections;
        var current = sections()[index];
        sections.splice(index, 1);
        sections.splice(index - 1, 0, current);
        current.Order(index - 1);
    };

    self.MoveDown = function (index) {
        var sections = self.Model().Sections;
        var current = sections()[index];
        sections.splice(index, 1);
        sections.splice(index + 1, 0, current);
        current.Order(index + 1);
    };

    self.EditSection = function (section) {
        if (!section) {
            self.CurrentSection(new IGSectionModel());
            self.CurrentSectionIndex(null);
        } else {
            var index = self.Model().Sections.indexOf(section);
            var tempCustomSchematron = new IGSectionModel(ko.mapping.toJS(section));
            self.CurrentSection(tempCustomSchematron);
            self.CurrentSectionIndex(index);
        }

        $('#sectionDialog').modal('show');
    };

    self.CancelEditSection = function () {
        self.CurrentSection(null);
        $('#sectionDialog').modal('hide');
    };

    self.SaveSection = function () {
        if (self.CurrentSectionIndex() != null) {
            self.Model().Sections.splice(self.CurrentSectionIndex(), 1, self.CurrentSection());
        } else {
            self.Model().Sections.push(self.CurrentSection());
        }

        self.CurrentSection(null);
        self.CurrentSectionIndex(null);
        $('#sectionDialog').modal('hide');
    };

    self.RemoveSection = function (section) {
        if (!confirm("Are you sure you want to remove the selected section?")) {
            return;
        }

        self.Model().Sections.remove(section);
    };

    self.AddCategory = function () {
        var categoryModel = new CategoryModel(self.NewCategory());
        self.Model().CategoryModels.push(categoryModel);
        self.NewCategory('');
    };

    self.RemoveCategory = function (index) {
        self.Model().CategoryModels.splice(index, 1);
    };

    self.Initialize = function (afterSave) {
        $.blockUI({ message: 'Loading...' });
        $.ajax({
            url: '/api/Type',
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ ImplementationGuideTypes: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Organization',
            success: function (results) {
                ko.mapping.fromJS({ Organizations: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/ImplementationGuide/Edit/' + (self.ImplementationGuideId() ? self.ImplementationGuideId() : ''),
            success: function (results) {
                var model = new ImplementationGuideModel(results);
                self.Model(model);
                
                // If this a new version of an IG and an identifier already exists on the IG, don't let them change it
                if (results.PreviousVersionId) {
                    self.DisableIdentifier(results.Identifier && results.Identifier.length > 0);
                }

                if (!self.ImplementationGuideId()) {
                    self.Model().TypeId.subscribe(self.ImplementationGuideTypeChanged);

                    // Setup default permissions
                    ko.utils.arrayForEach(results.DefaultViewPermissions, function (defaultPermission) {
                        var newUserModel = new UserModel(defaultPermission);
                        newUserModel.IsDefault(true);
                        self.Model().ViewPermissions.push(newUserModel);
                    });
                    ko.utils.arrayForEach(results.DefaultEditPermissions, function (defaultPermission) {
                        var newUserModel = new UserModel(defaultPermission);
                        newUserModel.IsDefault(true);
                        self.Model().EditPermissions.push(newUserModel);
                    });
                } else {
                    // Disable default permissions for removal
                    ko.utils.arrayForEach(results.DefaultViewPermissions, function (defaultPermission) {
                        var foundPermission = ko.utils.arrayFirst(self.Model().ViewPermissions(), function (permission) {
                            return permission.Type() == defaultPermission.Type && permission.Id() == defaultPermission.Id;
                        });

                        if (foundPermission) {
                            foundPermission.IsDefault(true);
                        }
                    });
                    ko.utils.arrayForEach(results.DefaultEditPermissions, function (defaultPermission) {
                        var foundPermission = ko.utils.arrayFirst(self.Model().EditPermissions(), function (permission) {
                            return permission.Type() == defaultPermission.Type && permission.Id() == defaultPermission.Id;
                        });

                        if (foundPermission) {
                            foundPermission.IsDefault(true);
                        }
                    });
                }

                $.unblockUI();
            }
        });
    };

    self.Save = function () {
        // Assign the order property to each of the sections based on the index
        for (var i in self.Model().Sections()) {
            self.Model().Sections()[i].Order(i);
        }

        var data = ko.mapping.toJS(self.Model());

        $.blockUI({ message: 'Saving...' });
        $.ajax({
            method: 'POST',
            url: '/api/ImplementationGuide/Save',
            data: data,
            success: function (newImplementationGuideId) {
                self.ImplementationGuideId(newImplementationGuideId);
                self.Initialize();
                alert("Successfully saved implementation guide!");
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.Message);
                $.unblockUI();
            }
        });
    };

    self.Cancel = function () {
        if (self.ImplementationGuideId()) {
            location.href = '/IGManagement/View/' + self.ImplementationGuideId();
        } else {
            location.href = '/IGManagement/List';
        }
    };

    self.Initialize();
};

var ImplementationGuideModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'DisplayName', 'WebDisplayName', 'WebDescription', 'WebReadmeOverview', 'TypeId', 'ConsolidatedFormat', 'PreviousVersionName', 'PreviousVersionId', 'CurrentVersion', 'DisableVersionFields', 'OrganizationId', 'CardinalityZeroOrOne', 'CardinalityExactlyOne', 'CardinalityAtLeastOne', 'CardinalityZeroOrMore', 'CardinalityZero', 'TemplateTypes', 'CustomSchematrons', 'DeletedCustomSchematrons', 'PreviousIgs', 'Statuses', 'ViewPermissions', 'EditPermissions', 'DefaultViewPermissions', 'DefaultEditPermissions', 'Html', 'Sections'],
        'CustomSchematrons': {
            create: function (options) {
                return new CustomSchematronModel(options.data);
            }
        },
        'ViewPermissions': { 
            create: function(options) {
                return new UserModel(options.data);
            }
        },
        'EditPermissions': {
            create: function(options) {
                return new UserModel(options.data);
            }
        },
        'TemplateTypes': {
            create: function (options) {
                return new TemplateTypeModel(options.data);
            }
        },
        'Sections': {
            create: function (options) {
                return new IGSectionModel(options.data);
            }
        }
    };

    self.Id = ko.observable();
    self.Identifier = ko.observable();
    self.Name = ko.observable();
    self.DisplayName = ko.observable();
    self.WebDisplayName = ko.observable();
    self.WebDescription = ko.observable();
    self.WebReadmeOverview = ko.observable();
    self.TypeId = ko.observable();
    self.ConsolidatedFormat = ko.observable();
    self.PreviousVersionName = ko.observable();
    self.PreviousVersionId = ko.observable();
    self.CurrentVersion = ko.observable();
    self.DisableVersionFields = ko.observable();
    self.OrganizationId = ko.observable();
    self.CardinalityZeroOrOne = ko.observable();
    self.CardinalityExactlyOne = ko.observable();
    self.CardinalityAtLeastOne = ko.observable();
    self.CardinalityZeroOrMore = ko.observable();
    self.CardinalityZero = ko.observable();
    self.TemplateTypes = ko.observableArray();
    self.CustomSchematrons = ko.observableArray();
    self.DeletedCustomSchematrons = ko.observableArray([]);
    self.PreviousIgs = ko.observableArray([]);
    self.ViewPermissions = ko.observableArray([]);
    self.EditPermissions = ko.observableArray([]);
    self.DefaultViewPermissions = ko.observableArray([]);
    self.DefaultEditPermissions = ko.observableArray([]);
    self.Sections = ko.observableArray([]);
    self.AccessManagerId = ko.observable();
    self.Html = ko.observable();
    self.AllowAccessRequests = ko.observable(false);
    self.NotifyNewPermissions = ko.observable(false);
    self.Categories = ko.observableArray([]);
    self.CategoryModels = ko.observableArray([]);
    self.Volume1Type = ko.observable('html');

    self.imageOpts = function () {
        return {
            listUrl: '/api/ImplementationGuide/' + data.Id + '/Images',
            baseUrl: '/api/ImplementationGuide/' + data.Id + '/Image/'
        };
    };

    self.ConsolidatedFormatString = ko.computed({
        read: function () { return (self.ConsolidatedFormat() ? "true" : "false"); },
        write: function (value) { self.ConsolidatedFormat(value == "true" ? true : false); }
    });

    self.AllowAccessRequestsString = ko.computed({
        read: function () { return (self.AllowAccessRequests() ? "true" : "false"); },
        write: function (value) { self.AllowAccessRequests(value == "true" ? true : false); }
    });

    ko.mapping.fromJS(data, mapping, self);

    if (self.Html()) {
        self.Volume1Type('html');
    } else if (self.Sections().length > 0) {
        self.Volume1Type('defined');
    }

    self.Volume1Type.subscribe(function (newType, oldType) {
        if (newType == oldType) {
            return;
        }

        if (newType == 'defined') {
            self.Html('');
        } else if (newType == 'html') {
            self.Sections().splice(0, self.Sections().length);
        }
    });

    for (var i in self.Categories()) {
        var category = self.Categories()[i];
        var categoryModel = new CategoryModel(category);
        self.CategoryModels.push(categoryModel);
    }

    self.Sections.sort(function (a, b) {
        var aOrder = a.Order();
        var bOrder = b.Order();

        return aOrder < bOrder ? -1 : (aOrder > bOrder ? 1 : 0);
    });

    self.CategoryModels.subscribe(function (newCategoryModels) {
        var categories = [];

        for (var i in newCategoryModels) {
            categories.push(newCategoryModels[i].Value());
        }

        self.Categories(categories);
    });

    self.Validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, uniqueImplementationGuideName: self.Id }),
        Identifier: self.Identifier.extend({ required: true }),
        TypeId: self.TypeId.extend({ required: true }),
        CardinalityZeroOrOne: self.CardinalityZeroOrOne.extend({ required: true }),
        CardinalityExactlyOne: self.CardinalityExactlyOne.extend({ required: true }),
        CardinalityAtLeastOne: self.CardinalityAtLeastOne.extend({ required: true }),
        CardinalityZeroOrMore: self.CardinalityZeroOrMore.extend({ required: true }),
        CardinalityZero: self.CardinalityZero.extend({ required: true }),
        CategoryModels: self.CategoryModels.extend({ categoriesValid: true })
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });

    self.GetEditUsers = ko.computed(function () {
        var editUsers = [];

        for (var i in self.EditPermissions()) {
            var currentPermission = self.EditPermissions()[i];

            if (currentPermission.Type() == 2) {
                editUsers.push(currentPermission);
            }
        }

        return editUsers;
    });
};

var IGSectionModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Heading', 'Content', 'Level', 'Order']
    };

    self.Id = ko.observable();
    self.Heading = ko.observable();
    self.Content = ko.observable();
    self.Level = ko.observable(1);
    self.Order = ko.observable(0);

    ko.mapping.fromJS(data, mapping, self);
};

var CustomSchematronModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Phase', 'PatternId', 'PatternContent' ]
    };

    self.Id = ko.observable();
    self.Phase = ko.observable();
    self.PatternId = ko.observable();
    self.PatternContent = ko.observable();

    ko.mapping.fromJS(data, mapping, self);
};

var UserModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Type', 'Name' ]
    };

    self.UniqueId = ko.observable(createUUID());
    self.Id = ko.observable();
    self.Type = ko.observable();
    self.Name = ko.observable();
    self.IsDefault = ko.observable(false);

    ko.mapping.fromJS(data, mapping, self);
};

var TemplateTypeModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'DefaultName', 'Name', 'Description' ]
    };

    self.DefaultName = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();

    ko.mapping.fromJS(data, mapping, self);
};