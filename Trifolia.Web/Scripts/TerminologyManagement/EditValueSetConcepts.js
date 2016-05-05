var EditValueSetConceptsViewModel = function (valueSetId) {
    var self = this;

    self.Concepts = ko.observableArray([]);
    self.CodeSystems = [];
    self.Page = ko.observable(1);
    self.LastPage = ko.observable(1);
    self.Loading = ko.observable(true);
    self.SearchQuery = ko.observable('');
    self.NewConcept = ko.observable(new EditValueSetConceptModel());
    self.ValueSetName = ko.observable('');
    self.RemovedConcepts = ko.observableArray([]);
    self.ModifiedConcepts = ko.observableArray([]);

    self.Save = function () {
        var isEditingConcept = false;

        if (isEditingConcept) {
            alert("You are currently editing a concept. Cannot save until you have committed changes to all concepts being edited");
            return;
        }

        $.blockUI({ message: 'Saving...' });

        var saveModel = {
            ValueSetId: valueSetId,
            Concepts: self.ModifiedConcepts(),
            RemovedConcepts: self.RemovedConcepts()
        };

        $.ajax({
            method: 'POST',
            url: '/api/Terminology/ValueSet/Concepts',
            data: saveModel,
            success: function () {
                alert('Changes are saved!');
                self.ModifiedConcepts([]);
                self.RemovedConcepts([]);
                self.Refresh();
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while saving the code system.');
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.UndoRemove = function (index) {
        var concept = self.RemovedConcepts()[index];
        self.RemovedConcepts.splice(index, 1);
        self.Concepts.push(concept);
    };

    self.Remove = function (concept, isModified) {
        if (concept.Id()) {
            self.RemovedConcepts.push(concept);
        }

        if (!isModified) {
            var index = self.Concepts.indexOf(concept);
            self.Concepts.splice(index, 1);
        } else {
            var index = self.ModifiedConcepts.indexOf(concept);
            self.ModifiedConcepts.splice(index, 1);
        }
    };

    self.Add = function () {
        self.ModifiedConcepts.push(self.NewConcept());
        self.NewConcept(new EditValueSetConceptModel());
    };

    self.SaveConcept = function (concept) {
        var modifiedConceptIndex = self.ModifiedConcepts.indexOf(concept);

        if (modifiedConceptIndex < 0) {
            self.ModifiedConcepts.push(concept);
        }

        concept.IsEditing(false);
    };

    self.Search = function () {
        self.Page(1);
        self.Refresh();
    };

    self.GetCodeSystemName = function (codeSystemId) {
        for (var i in self.CodeSystems) {
            if (self.CodeSystems[i].Id == codeSystemId) {
                return self.CodeSystems[i].Name;
            }
        }
    };

    self.PreviousPage = function () {
        self.Page(self.Page() - 1);
        self.Refresh();
    };

    self.NextPage = function () {
        self.Page(self.Page() + 1);
        self.Refresh();
    };

    self.GoToFirstPage = function () {
        self.Page(1);
        self.Refresh();
    };

    self.GoToLastPage = function () {
        self.Page(self.LastPage());
        self.Refresh();
    };

    self.Refresh = function () {
        self.Loading(true);

        $.ajax({
            url: '/api/Terminology/CodeSystem/Basic',
            success: function (results) {
                self.CodeSystems = results;
            }
        });

        var count = 20;
        var conceptUrl = '/api/Terminology/ValueSet/' + valueSetId + '/Concepts?page=' + self.Page() + '&count=' + count;

        if (self.SearchQuery()) {
            conceptUrl += '&query=' + encodeURIComponent(self.SearchQuery());
        }

        $.ajax({
            url: conceptUrl,
            success: function (results) {
                self.Concepts([]);

                for (var i in results.rows) {
                    var conceptModel = new EditValueSetConceptModel(results.rows[i]);
                    self.Concepts.push(conceptModel);
                }

                self.LastPage(Math.ceil(results.total / count));
                self.Loading(false);
            }
        });

        $.ajax({
            url: '/api/Terminology/ValueSet/' + valueSetId,
            success: function (results) {
                self.ValueSetName(results.Name);
            }
        });
    };

    self.Refresh();
};

var EditValueSetConceptModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Code', 'DisplayName', 'CodeSystemId', 'Status', 'StatusDate']
    };
    var preEditingData = null;

    self.Id = ko.observable();
    self.Code = ko.observable();
    self.DisplayName = ko.observable();
    self.CodeSystemId = ko.observable();
    self.Status = ko.observable();
    self.StatusDate = ko.observable();
    self.IsEditing = ko.observable(false);
    self.Data = ko.observable(data);

    ko.mapping.fromJS(data, mapping, self);

    if (self.StatusDate() && self.StatusDate().indexOf('T') > 0) {
        var date = self.StatusDate().substring(8, 10);
        var month = self.StatusDate().substring(5, 7);
        var year = self.StatusDate().substring(0, 4);
        self.StatusDate(month + '/' + date + '/' + year);
    }

    var validation = ko.validatedObservable({
        Code: self.Code.extend({ required: true, maxLength: 255 }),
        DisplayName: self.DisplayName.extend({ required: true, maxLength: 255 }),
        CodeSystemId: self.CodeSystemId.extend({ required: true })
    });

    self.IsValid = ko.computed(function () {
        return validation().isValid();
    });

    self.Edit = function () {
        preEditingData = ko.mapping.toJS(self);
        self.IsEditing(true);
    };

    self.Cancel = function () {
        ko.mapping.fromJS(preEditingData, mapping, self);
        preEditingData = null;
        self.IsEditing(false);
    };
};