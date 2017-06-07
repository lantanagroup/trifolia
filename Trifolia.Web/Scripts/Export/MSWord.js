var ExportMSWordViewModel = function (implementationGuideId) {
    var self = this;
    var mapping = {
    };

    /*
    Observables
    */
    self.ParentTemplateIds = ko.observableArray([{ Id: ko.observable() }]);
    self.IncludeInferred = ko.observable(true);
    self.ImplementationGuideId = ko.observable();
    self.Name = ko.observable();
    self.CanEdit = ko.observable();
    self.Templates = ko.observableArray([]);
    self.ValueSets = ko.observableArray([]);
    self.AllTemplatesChecked = ko.observable(true);
    self.TemplateChecked = ko.observable(true);
    self.MaximumValuesetMembers = ko.observable(10);
    self.GenerateValuesets = ko.observable(true);
    self.TemplateSortOrder = ko.observable();
    self.DocumentTables = ko.observable(1);
    self.TemplateTables = ko.observable(1);
    self.IncludeXmlSample = ko.observable(true);
    self.IncludeChangeList = ko.observable(true);
    self.IncludeTemplateStatus = ko.observable(true);
    self.IncludeNotes = ko.observable(false);
    self.ValuesetAppendix = ko.observable(false);
    self.SaveSettings = ko.observable(false);
    self.SelectedCategories = ko.observableArray([]);
    self.Categories = ko.observableArray([]);
    self.TemplateIds = ko.observableArray([]);

    /*
    Private Variables
    */
    var templatesLoaded = false;
    var infoLoaded = false;
    var valueSetsLoaded = false;

    /*
    Computable Properties
    */
    self.IncludeInferred.ForBinding = ko.computed({
        read: function () {
            return self.IncludeInferred().toString();
        },
        write: function (value) {
            self.IncludeInferred(value === 'true');
        }
    });

    self.GenerateValuesetsString = ko.computed({
        read: function () {
            return self.GenerateValuesets().toString();
        },
        write: function (value) {
            self.GenerateValuesets(value === "true");

            if (self.GenerateValuesets()) {
                self.MaximumValuesetMembers(10);
            } else {
                self.MaximumValuesetMembers(0);
            }
        }
    });

    self.ValuesetAppendixString = ko.computed({
        read: function () {
            return self.ValuesetAppendix().toString();
        },
        write: function (value) {
            self.ValuesetAppendix(value === "true");
        }
    });

    /*
    Private Methods
    */
    var loadSettings = function (cb) {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/Settings',
            cache: false,
            success: function (results) {
                if (results) {
                    self.DocumentTables(results.DocumentTables);
                    self.TemplateTables(results.TemplateTables);
                    self.IncludeXmlSample(results.IncludeXmlSample);
                    self.IncludeChangeList(results.IncludeChangeList);
                    self.IncludeTemplateStatus(results.IncludeTemplateStatus);
                    self.ValuesetAppendix(results.ValuesetAppendix);
                    self.GenerateValuesets(results.GenerateValuesets);
                    self.IncludeInferred(results.Inferred);
                    self.MaximumValuesetMembers(results.MaximumValuesetMembers);
                    self.SelectedCategories(results.SelectedCategories);
                    self.TemplateIds(results.TemplateIds);
                    self.TemplateSortOrder(results.TemplateSortOrder);

                    if (self.CanEdit()) {
                        self.IncludeNotes(results.IncludeNotes);
                    }

                    for (var i in results.ParentTemplateIds) {
                        self.ParentTemplateIds()[i].Id(results.ParentTemplateIds[i]);       // Set the Id() creates a new ParentTemplateIds entry via the subscribe/watch
                    }

                    if (results.ValueSetOid && results.ValueSetMaxMembers && results.ValueSetOid.length == results.ValueSetMaxMembers.length) {
                        for (var i in results.ValueSetOid) {
                            var cValueSetOid = results.ValueSetOid[i];
                            var cValueSetMaxMembers = results.ValueSetMaxMembers[i];
                            var foundValueSet = null;

                            for (var x in self.ValueSets()) {
                                if (self.ValueSets()[x].Oid() == cValueSetOid) {
                                    foundValueSet = self.ValueSets()[x];
                                    break;
                                }
                            }

                            if (foundValueSet) {
                                foundValueSet.MaxMembers(cValueSetMaxMembers);
                            }
                        }
                    }
                }


                if (cb) {
                    cb();
                }

                /*if (results && results.TemplateIds) {
                    $('.nav-tabs a[href="#templates"]').tab('show');
                    $('input[name="TemplateIds"]').prop("checked", false);

                    for (var i in results.TemplateIds) {
                        $('input[name="TemplateIds"][value=' + results.TemplateIds[i] + ']').prop("checked", true);
                    }
                    $('.nav-tabs a[href="#content"]').tab('show');
                }*/

            }
        });
    };

    /*
    Public Methods
    */

    self.ExportEnabled = ko.computed(function () {
        return templatesLoaded && infoLoaded && valueSetsLoaded;
    });

    self.AllTemplatesChecked.subscribe(function () {
        $('.templateIdCheckboxes').prop('checked', self.AllTemplatesChecked());
    });

    self.TemplateChecked = function (idObs) {
        var templateIds = self.TemplateIds();
        var tID = idObs();

        return templateIds.indexOf(tID) >= 0;
    };

    self.MaximumValuesetMembers.subscribe(function () {
        for (var i in self.ValueSets()) {
            self.ValueSets()[i].MaxMembers(self.MaximumValuesetMembers());
        }
    });

    var refreshTemplates = function (cb) {
        templatesLoaded = false;

        var url = '/api/ImplementationGuide/' + implementationGuideId + '/Template?inferred=' + self.IncludeInferred();

        for (var i in self.ParentTemplateIds()) {
            if (self.ParentTemplateIds()[i].Id()) {
                url += '&parentTemplateIds=' + self.ParentTemplateIds()[i].Id();
            }
        }

        for (var i in self.SelectedCategories()) {
            var category = self.SelectedCategories()[i];

            if (category) {
                url += '&categories=' + encodeURIComponent(category);
            }
        }

        $.ajax({
            url: url,
            success: function (results) {
                ko.mapping.fromJS({ Templates: results }, {}, self);

                templatesLoaded = true;

                if (cb) {
                    cb();
                }
            }
        });
    };

    var parentTemplateIdChanged = function () {
        var parentTemplateIds = self.ParentTemplateIds();
        var lastParentTemplateId = parentTemplateIds[parentTemplateIds.length - 1].Id();

        if (lastParentTemplateId) {
            var newParentTemplateId = {
                Id: ko.observable()
            };

            newParentTemplateId.Id.subscribe(parentTemplateIdChanged);
            self.ParentTemplateIds.push(newParentTemplateId);
        }

        if (templatesLoaded) {
            refreshTemplates();
        }
    };

    var templateInfoChanged = function () {
        if (templatesLoaded) {
            refreshTemplates();
        }
    };

    self.ParentTemplateIds()[0].Id.subscribe(parentTemplateIdChanged);
    self.IncludeInferred.subscribe(templateInfoChanged);
    self.SelectedCategories.subscribe(templateInfoChanged);

    var refreshInfo = function (cb) {
        infoLoaded = false;

        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/MSWord',
            success: function (results) {
                ko.mapping.fromJS(results, {}, self);

                infoLoaded = true;

                if (cb) {
                    cb();
                }
            }
        });
    };

    var refreshValueSets = function (cb) {
        valueSetsLoaded = false;

        $.ajax({
            url: '/api/ImplementationGuide/' + implementationGuideId + '/ValueSet?onlyStatic=false',
            cache: false,
            async: false,
            success: function (results) {
                for (var i in results) {
                    results[i].MaxMembers = self.MaximumValuesetMembers();
                }

                ko.mapping.fromJS({ ValueSets: results }, {}, self);

                valueSetsLoaded = true;

                if (cb) {
                    cb();
                }
            }
        });
    };

    self.Initialize = function () {
        refreshInfo(function () {
            refreshValueSets(function () {
                loadSettings(function () {
                    refreshTemplates();
                });
            });
        });
    };

    self.Export = function () {
        $("#ExportMSWordForm").attr("action", "/api/Export/MSWord");
        $("#ExportMSWordForm").submit();
    };

    self.Initialize();
};