var BrowseTerminologyViewModel = function () {
    var self = this;
    
    self.ValueSetSort = ko.observable($.cookie('BrowseTerminology_ValueSetSort') || 'Name');
    self.ValueSetPage = ko.observable($.cookie('BrowseTerminology_ValueSetPage') || 1);
    self.ValueSetRows = ko.observable($.cookie('BrowseTerminology_ValueSetRows') || 20);
    self.ValueSetOrder = ko.observable($.cookie('BrowseTerminology_ValueSetOrder') || 'asc');
    self.ValueSetQuery = ko.observable($.cookie('BrowseTerminology_ValueSetQuery') || '');
    self.ValueSetResults = ko.observable();
    self.CurrentValueSet = ko.observable();

    self.CodeSystemSort = ko.observable($.cookie('BrowseTerminology_CodeSystemSort') || 'Name');
    self.CodeSystemPage = ko.observable($.cookie('BrowseTerminology_CodeSystemPage') || 1);
    self.CodeSystemRows = ko.observable($.cookie('BrowseTerminology_CodeSystemRows') || 20);
    self.CodeSystemOrder = ko.observable($.cookie('BrowseTerminology_CodeSystemOrder') || 'asc');
    self.CodeSystemQuery = ko.observable($.cookie('BrowseTerminology_CodeSystemQuery') || '');
    self.CodeSystemResults = ko.observable();
    self.CurrentCodeSystem = ko.observable();

    self.CodeSystems = ko.observableArray([]);

    self.HandleValueSetSearchKeyPress = function (data, e) {
        if (e.keyCode == 13) {
            self.SearchValueSets();
            return false;
        } else if (e.keyCode == 27) {
            self.ValueSetQuery('');
            self.SearchValueSets();
            return false;
        }

        return true;
    };

    self.HandleCodeSystemSearchKeyPress = function (data, e) {
        if (e.keyCode == 13) {
            self.SearchCodeSystems();
            return false;
        } else if (e.keyCode == 27) {
            self.CodeSystemQuery('');
            self.SearchCodeSystems();
            return false;
        }

        return true;
    };

    self.CodeSystemTotalPages = ko.computed(function () {
        if (!self.CodeSystemResults() || self.CodeSystemResults().total() == 0) {
            return 0;
        }

        var pages = Math.round(self.CodeSystemResults().total() / self.CodeSystemRows());
        return pages < 1 ? 1 : pages;
    });

    self.ValueSetTotalPages = ko.computed(function () {
        if (!self.ValueSetResults() || self.ValueSetResults().TotalItems() == 0) {
            return 0;
        }

        var pages = Math.round(self.ValueSetResults().TotalItems() / self.ValueSetRows());
        return pages < 1 ? 1 : pages;
    });

    self.Initialize = function () {
        self.LoadAllCodeSystems();

        var promises = [self.LoadValueSets(), self.LoadCodeSystems()];

        $.blockUI({ message: 'Loading...' });
        Q.all(promises)
            .done(function () {
                $.unblockUI();
            });
    };

    self.LoadAllCodeSystems = function () {
        $.ajax({
            url: '/api/Terminology/CodeSystem/Basic',
            success: function (results) {
                ko.mapping.fromJS({ CodeSystems: results }, {}, self);
            }
        });
    };

    self.LoadValueSets = function () {
        if (!containerViewModel.HasSecurable(['ValueSetList'])) {
            return Q.resolve();
        }

        var deferred = Q.defer();
        console.log("Searching value sets at " + new Date());

        $('#valuesets').block({ message: "Loading/searching value sets..." });
        $.ajax({
            url: '/api/Terminology/ValueSets/SortedAndPaged?sort=' + self.ValueSetSort() + '&page=' + self.ValueSetPage() + '&rows=' + self.ValueSetRows() + '&order=' + self.ValueSetOrder() + '&search=' + self.ValueSetQuery(),
            cache: false,
            success: function (results) {
                console.log("Done searching value sets at " + new Date());
                self.ValueSetResults(new BrowseValueSetResults(results));
                console.log("Done loading value sets at " + new Date());
                deferred.resolve();
            },
            error: function(err) {
                deferred.reject(err);
            },
            complete: function () {
                $('#valuesets').unblock();
            }
        });

        return deferred.promise;
    };
    
    self.LoadCodeSystems = function () {
        if (!containerViewModel.HasSecurable(['CodeSystemList'])) {
            return Q.resolve();
        }

        var deferred = Q.defer();
        $('#codesystems').block({ message: "Loading/searching code systems..." });

        $.ajax({
            url: '/api/Terminology/CodeSystems/SortedAndPaged?sort=' + self.CodeSystemSort() + '&page=' + self.CodeSystemPage() + '&rows=' + self.CodeSystemRows() + '&order=' + self.CodeSystemOrder() + '&search=' + self.CodeSystemQuery(),
            cache: false,
            success: function (results) {
                self.CodeSystemResults(new BrowseCodeSystemResults(results));
                deferred.resolve();
            },
            error: function(err) {
                deferred.reject(err);
            },
            complete: function () {
                $('#codesystems').unblock();
            }
        });

        return deferred.promise;
    };

    /* Code Systems */
    self.EditCodeSystem = function (codeSystem) {
        if (codeSystem) {
            if (!codeSystem.PermitModify() || !(codeSystem.CanModify() || codeSystem.CanOverride())) {
                return;
            }

            if (!codeSystem.CanModify() && codeSystem.CanOverride()) {
                if (!confirm("This code system is used by a published implementation guide. Are you sure you want to edit this code system?")) {
                    return;
                }
            }
        }

        var tempCodeSystem = new BrowseCodeSystem(null, true);

        if (codeSystem) {
            tempCodeSystem = new BrowseCodeSystem(ko.mapping.toJS(codeSystem), true);
        }

        self.CurrentCodeSystem(tempCodeSystem);
        $('#editCodeSystemDialog').modal('show');
    };

    self.ToggleCodeSystemSort = function (property) {
        if (self.CodeSystemSort() != property) {
            self.CodeSystemSort(property);
            self.CodeSystemOrder('asc');
        } else {
            self.CodeSystemOrder(self.CodeSystemOrder() == 'desc' ? 'asc' : 'desc');
        }

        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.CodeSystemFirstPage = function () {
        self.CodeSystemPage(1);
        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.CodeSystemPreviousPage = function () {
        self.CodeSystemPage(self.CodeSystemPage() - 1);
        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.CodeSystemNextPage = function () {
        self.CodeSystemPage(self.CodeSystemPage() + 1);
        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.CodeSystemLastPage = function () {
        self.CodeSystemPage(self.CodeSystemTotalPages());
        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.SearchCodeSystems = function () {
        self.CodeSystemPage(1);
        self.LoadCodeSystems();
        self.SetCodeSystemCookies();
    };

    self.SetCodeSystemCookies = function () {
        $.cookie('BrowseTerminology_CodeSystemQuery', self.CodeSystemQuery());
        $.cookie('BrowseTerminology_CodeSystemPage', self.CodeSystemPage());
        $.cookie('BrowseTerminology_CodeSystemRows', self.CodeSystemRows());
        $.cookie('BrowseTerminology_CodeSystemSort', self.CodeSystemSort());
        $.cookie('BrowseTerminology_CodeSystemOrder', self.CodeSystemOrder());
    };

    self.GetCodeSystemName = function (codeSystemId) {
        var foundCodeSystem = ko.utils.arrayFirst(self.CodeSystems(), function (codeSystem) {
            return codeSystem.Id() == codeSystemId;
        });

        if (foundCodeSystem) {
            return foundCodeSystem.Name();
        }

        return '';
    };

    self.SaveCodeSystem = function () {
        $.ajax({
            method: 'POST',
            url: '/api/Terminology/CodeSystems/Save',
            data: ko.mapping.toJS(self.CurrentCodeSystem()),
            success: function () {
                self.LoadCodeSystems();
                self.LoadAllCodeSystems();
                alert('Successfully saved code system');
                self.CancelEditCodeSystem();
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while saving the code system.');
            }
        });
    };

    self.CancelEditCodeSystem = function () {
        self.CurrentCodeSystem(null);
        $('#editCodeSystemDialog').modal('hide');
    };

    self.RemoveCodeSystem = function (codeSystem) {
        if (!codeSystem.PermitModify() || !(codeSystem.CanModify() || codeSystem.CanOverride())) {
            return;
        }

        if (!confirm("Are you sure you want to remove this code system?")) {
            return;
        }

        $.ajax({
            method: 'DELETE',
            url: '/api/Terminology/CodeSystem/' + codeSystem.Id(),
            success: function () {
                self.LoadCodeSystems();
                self.LoadAllCodeSystems();
                alert('Successfully deleted code system!');
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while deleting the value set');
            }
        });
    };

    /* Value Sets */
    self.EditValueSetConcepts = function (valueSet) {
        if (valueSet) {
            if (!valueSet.PermitModify() || !(valueSet.CanModify() || valueSet.CanOverride())) {
                return;
            }

            if (!valueSet.CanModify() && valueSet.CanOverride()) {
                if (!confirm("This valueset is used by a published implementation guide. Are you sure you want to edit this valueset?")) {
                    return;
                }
            }
        }

        location.href = '/TerminologyManagement/ValueSet/Edit/' + valueSet.Id() + '/Concept';
    };

    self.EditValueSet = function (valueSet) {
        if (valueSet) {
            if (!valueSet.PermitModify() || !(valueSet.CanModify() || valueSet.CanOverride())) {
                return;
            }

            if (!valueSet.CanModify() && valueSet.CanOverride()) {
                if (!confirm("This valueset is used by a published implementation guide. Are you sure you want to edit this valueset?")) {
                    return;
                }
            }
        }

        var tempValueSet = new BrowseValueSet(null, true);

        if (valueSet) {
            tempValueSet = new BrowseValueSet(ko.mapping.toJS(valueSet), true);

            tempValueSet.LoadConcepts(function () {
                self.CurrentValueSet(tempValueSet);
                $('#editValueSetDialog').modal('show');
            });
        } else {
            self.CurrentValueSet(tempValueSet);
            $('#editValueSetDialog').modal('show');
        }
    };

    self.ToggleValueSetSort = function (property) {
        if (self.ValueSetSort() != property) {
            self.ValueSetSort(property);
            self.ValueSetOrder('asc');
        } else {
            self.ValueSetOrder(self.ValueSetOrder() == 'desc' ? 'asc' : 'desc');
        }

        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.ValueSetPreviousPage = function () {
        self.ValueSetPage(self.ValueSetPage() - 1);
        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.ValueSetFirstPage = function () {
        self.ValueSetPage(1);
        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.ValueSetNextPage = function () {
        self.ValueSetPage(self.ValueSetPage() + 1);
        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.ValueSetLastPage = function () {
        self.ValueSetPage(self.ValueSetTotalPages());
        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.SearchValueSets = function () {
        self.ValueSetPage(1);
        self.LoadValueSets();
        self.SetValueSetCookies();
    };

    self.SetValueSetCookies = function () {
        $.cookie('BrowseTerminology_ValueSetQuery', self.ValueSetQuery());
        $.cookie('BrowseTerminology_ValueSetPage', self.ValueSetPage());
        $.cookie('BrowseTerminology_ValueSetRows', self.ValueSetRows());
        $.cookie('BrowseTerminology_ValueSetSort', self.ValueSetSort());
        $.cookie('BrowseTerminology_ValueSetOrder', self.ValueSetOrder());
    };

    self.SaveValueSet = function () {
        var data = {
            ValueSet: ko.mapping.toJS(self.CurrentValueSet())
        };

        $.ajax({
            method: 'POST',
            url: '/api/Terminology/ValueSet/Save',
            data: data,
            success: function () {
                self.LoadValueSets();
                alert('Successfully saved value set!');
                self.CancelEditValueSet();
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while saving the value set.');
            }
        });
    };

    self.CancelEditValueSet = function () {
        self.CurrentValueSet(null);
        $('#editValueSetDialog').modal('hide');
    };

    self.RemoveValueSet = function (valueSet) {
        if (!valueSet.PermitModify() || !(valueSet.CanModify() || valueSet.CanOverride())) {
            return;
        }

        if (!confirm("Are you sure you want to remove this value set?")) {
            return;
        }

        $.ajax({
            method: 'DELETE',
            url: '/api/Terminology/ValueSet/' + valueSet.Id(),
            success: function () {
                self.LoadValueSets();
                alert('Successfully deleted value set!');
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while deleting the value set');
            }
        });
    };

    self.Initialize();
};

var BrowseCodeSystemResults = function (data) {
    var self = this;
    var mapping = {
        include: ['total', 'rows'],
        'rows': {
            create: function (options) {
                return new BrowseCodeSystem(options.data);
            }
        }
    };

    self.total = ko.observable();
    self.rows = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var BrowseCodeSystem = function (data, validateOid) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'Description', 'Oid', 'ConstraintCount', 'MemberCount', 'PermitModify', 'CanModify', 'CanOverride']
    };

    // Server properties
    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();
    self.Oid = ko.observable();
    self.ConstraintCount = ko.observable();
    self.MemberCount = ko.observable();
    self.PermitModify = ko.observable(true);
    self.CanModify = ko.observable(true);
    self.CanOverride = ko.observable(false);

    self.IsPublished = ko.computed(function () {
        return self.PermitModify() && !self.CanModify();
    });

    ko.mapping.fromJS(data, mapping, self);

    var oidValidation = {
        required: true,
        maxLength: 255,
        codeSystemOidFormat: true
    };

    if (validateOid) {
        oidValidation['codeSystemOidUnique'] = self.Id;
    }

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, maxLength: 255 }),
        Oid: self.Oid.extend(oidValidation)
    });

    self.IsValid = ko.computed(function () {
        return validation().isValid();
    });
};

var BrowseValueSetResults = function (data) {
    var self = this;
    var mapping = {
        include: ['TotalItems', 'Items'],
        'Items': {
            create: function (options) {
                return new BrowseValueSet(options.data);
            }
        }
    };

    self.TotalItems = ko.observable();
    self.Items = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var BrowseValueSet = function (data, validateOid) {
    var self = this;
    var mapping = {
        include: ['Id', 'Code', 'Name', 'Description', 'Oid', 'IsComplete', 'PermitModify', 'CanModify', 'CanOverride', 'IsIntentional', 'IntentionalDefinition', 'SourceUrl' ]
    };

    // Server properties
    self.Id = ko.observable();
    self.Name = ko.observable('');
    self.Description = ko.observable('');
    self.Oid = ko.observable('');
    self.IsComplete = ko.observable();
    self.PermitModify = ko.observable();
    self.CanModify = ko.observable(true);
    self.CanOverride = ko.observable(false);
    self.Code = ko.observable('');
    self.IsIntentional = ko.observable(false);
    self.IntentionalDefinition = ko.observable('');
    self.SourceUrl = ko.observable();
    self.ConceptPage = ko.observable(1);
    self.ConceptLastPage = ko.observable(1);
    self.ConceptsLoaded = ko.observable(false);
    self.Concepts = [];
    self.ConceptSearchQuery = ko.observable('');
    self.SearchedConcepts = ko.observableArray([]);

    self.IsValid = function () {
        // TODO
    };

    self.GoToFirstPage = function () {
        self.ConceptPage(1);
    };
    
    self.GoToLastPage = function () {
        self.ConceptPage(self.ConceptLastPage());
    };

    self.PreviousPage = function () {
        self.ConceptPage(self.ConceptPage() - 1);
    };

    self.NextPage = function () {
        self.ConceptPage(self.ConceptPage() + 1);
    };

    self.SearchConcepts = function () {
        self.SearchedConcepts(self.Concepts);

        if (self.ConceptSearchQuery() && self.ConceptSearchQuery().length > 3) {
            self.SearchedConcepts([]);
            var searchRegex = new RegExp(self.ConceptSearchQuery(), 'gi');

            for (var i in self.Concepts) {
                if (searchRegex.test(self.Concepts[i].Code) || searchRegex.test(self.Concepts[i].DisplayName)) {
                    self.SearchedConcepts.push(self.Concepts[i]);
                }
            }
        }

        self.ConceptPage(1);
        self.ConceptLastPage(Math.ceil(self.SearchedConcepts().length / 25));
    };

    self.GetConcepts = function () {
        var startIndex = (self.ConceptPage() - 1) * 25;
        var endIndex = startIndex + 25;
        var ret = [];

        if (endIndex > self.SearchedConcepts().length - 1) {
            endIndex = self.SearchedConcepts().length - 1;
        }

        for (var i = startIndex; i <= endIndex; i++) {
            ret.push(self.SearchedConcepts()[i]);
        }

        return ret;
    };

    // Client properties
    self.IsExpanded = ko.observable(false);
    self.Concepts = [];

    self.IsPublished = ko.computed(function () {
        return self.PermitModify() && !self.CanModify();
    });

    self.ShowMenu = ko.computed(function () {
        return self.PermitModify() && (self.CanModify() || self.CanOverride());
    });

    self.LoadConcepts = function (callback) {
        $.ajax({
            url: '/api/Terminology/ValueSet/' + self.Id() + '/Concepts',
            success: function (results) {
                self.Concepts = results.rows;
                self.SearchConcepts();
                callback();
            }
        });
    };

    self.ToggleValueSet = function () {
        if (!self.ConceptsLoaded()) {
            self.LoadConcepts(function () {
                self.ConceptsLoaded(true);
                self.IsExpanded(true);
            });
        } else {
            self.IsExpanded(!self.IsExpanded());
        }
    };

    ko.mapping.fromJS(data, mapping, self);
};