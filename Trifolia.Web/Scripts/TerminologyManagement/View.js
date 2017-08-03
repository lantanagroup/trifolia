var ViewValueSetViewModel = function (valueSetId) {
    var self = this;

    self.Message = ko.observable('');
    self.Concepts = ko.observableArray([]);
    self.CodeSystems = [];
    self.Page = ko.observable(1);
    self.LastPage = ko.observable(1);
    self.Loading = ko.observable(true);
    self.SearchQuery = ko.observable('');
    self.ValueSet = ko.observable('');
    self.Relationships = ko.observableArray([]);

    self.CompleteText = ko.observable('');
    self.CompleteHint = ko.observable('');

    /* METHODS */

    self.ImportSourceDisplay = ko.computed(function () {
        var importSource = self.ValueSet() ? self.ValueSet().ImportSource() : null;

        switch (importSource)
        {
            case 1:
                return 'VSAC';
            case 2:
                return 'PHIN VADS';
            case 3:
                return 'ROSE TREE';
            default:
                return '';
        }
    });

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

    self.Search = function () {
        self.Page(1);
        self.Refresh();
    };

    self.GetWellFormattedDate = function (dateString) {
        if (!dateString) {
            return '';
        }

        var date = new Date(dateString);
        return date.toLocaleDateString();
    };

    self.GetBindingStrengthDisplay = function (strength) {
        if (strength == 0) {
            return 'Static';
        }
        
        return 'Dynamic';
    };

    self.Refresh = function (init) {
        self.Loading(true);

        if (init) {
            $.ajax({
                url: '/api/Terminology/CodeSystem/Basic',
                success: function (results) {
                    self.CodeSystems = results;
                }
            });

            $.ajax({
                url: '/api/Terminology/ValueSet/' + valueSetId,
                success: function (results) {
                    var valueSet = ko.mapping.fromJS(results);
                    self.ValueSet(valueSet);

                    if (valueSet.IsComplete()) {
                        self.CompleteText('Complete');
                        self.CompleteHint('The value set include all concepts per its definition.');
                    } else {
                        self.CompleteText('Incomplete');
                        self.CompleteHint('This value set includes a subset of the concepts according to its definition.');
                    }
                }
            });

            $.ajax({
                url: '/api/Terminology/ValueSet/' + valueSetId + '/Relationships',
                success: function (results) {
                    var relationships = ko.mapping.fromJS(results);
                    self.Relationships(relationships());
                }
            });
        }

        var count = 20;
        var conceptUrl = '/api/Terminology/ValueSet/' + valueSetId + '/Concepts?page=' + self.Page() + '&count=' + count;

        if (self.SearchQuery()) {
            conceptUrl += '&query=' + encodeURIComponent(self.SearchQuery());
        }

        $.ajax({
            url: conceptUrl,
            success: function (results) {
                var concepts = ko.mapping.fromJS(results.rows, {});
                self.Concepts(concepts());

                self.LastPage(Math.ceil(results.total / count));
                self.Loading(false);
            },
            error: function (err) {
                if (err.responseText) {
                    self.Message(err.responseText);
                } else {
                    console.log(err);
                    self.Message('An error occurred while retrieving the concepts for the value set.');
                }
            }
        });
    };

    self.Refresh(true);
};