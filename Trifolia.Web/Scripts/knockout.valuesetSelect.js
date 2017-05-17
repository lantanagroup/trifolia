var ValuesetSelectViewModel = function (idAccessor) {
    var self = this;

    self.DisplayDiv = ko.observable();
    self.ModalDiv = ko.observable();
    self.SearchQuery = ko.observable('');
    self.SelectValuesets = ko.observableArray(null);
    self.SelectedValueset = ko.observable();
    self.Display = ko.observable();
    self.Buttons = ko.observableArray([]);
    self.Label = ko.observable();
    self.Disabled = ko.observable(false);
    self.IsSearching = ko.observable(false);
    self.IsSmall = ko.observable(false);
    self.CanTypeAhead = ko.observable(true);
    self.ShowTypeAheadResults = ko.observable(false);
    self.TypeAheadResults = ko.observableArray([]);
    self.SelectedTypeAheadIndex = ko.observable();
    self.CurrentSearchPage = ko.observable(1);

    var idAccessorSubscription, displaySubscription;
    var updatingDisplay = false;
    var lostFocus = false;

    var updateDisplay = function (newValue, notify) {
        updatingDisplay = !notify;
        self.Display(newValue);
        updatingDisplay = false;
    };

    self.IdChanged = function () {
        if (!idAccessor()) {
            self.Display('');
            return;
        }

        $.ajax({
            url: '/api/Terminology/ValueSet/' + idAccessor(),
            success: function (valueset) {
                var sortedIdentifiers = _.chain(valueset.Identifiers)
                    .sortBy(function (identifier) {
                        return identifier.IsDefault;
                    })
                    .reverse()
                    .value();

                var display = valueset.Name;

                if (sortedIdentifiers.length > 0) {
                    display += ' (' + sortedIdentifiers[0].Identifier + ')';
                }

                updateDisplay(display, false);
            }
        });
    };
    idAccessorSubscription = idAccessor.subscribe(self.IdChanged);

    self.Clear = function () {
        self.Display('');
        idAccessor(null);
    };

    /* Search in modal */
    self.SearchValuesets = function () {
        if (self.CurrentSearchPage() == 1) {
            self.SelectValuesets(null);
        }

        var url = '/api/Terminology/ValueSets/SortedAndPaged?rows=5&page=' + self.CurrentSearchPage() + '&sort=Name&order=asc';

        if (self.SearchQuery()) {
            url += '&search=' + encodeURIComponent(self.SearchQuery());
        }

        self.IsSearching(true);
        $.ajax({
            url: url,
            success: function (results) {
                ko.mapping.fromJS({ SelectValuesets: results }, {}, self);
                self.IsSearching(false);
            }
        });
    };

    self.SearchMoreResults = function () {
        self.CurrentSearchPage(self.CurrentSearchPage() + 1);
        self.SearchValuesets();
    };

    var delayTimer = null;
    var delaySearch = function () {
        if (delayTimer) {
            clearTimeout(delayTimer);
        }

        delayTimer = setTimeout(function () {
            self.CurrentSearchPage(1);
            self.SearchValuesets();
        }, 1000);
    };
    self.SearchQuery.subscribe(delaySearch);

    /* Type ahead */
    self.TypeAheadSearch = function () {
        self.TypeAheadResults([]);
        self.ShowTypeAheadResults(false);

        if (!self.Display()) {
            return;
        }

        var url = '/api/Terminology/ValueSets/SortedAndPaged?rows=5&page=1&sort=Name&order=asc';

        if (self.Display()) {
            url += '&search=' + encodeURIComponent(self.Display());
        }

        $.ajax({
            url: url,
            cache: false,
            success: function (results) {
                ko.mapping.fromJS({ TypeAheadResults: results.Items }, {}, self);
                self.ShowTypeAheadResults(true);
            }
        });
    };

    var typeAheadDelayTimer = null;
    var typeAheadDelaySearch = function () {
        if (updatingDisplay) {
            return;
        }

        lostFocus = false;

        if (typeAheadDelayTimer) {
            clearTimeout(typeAheadDelayTimer);
        }

        typeAheadDelayTimer = setTimeout(self.TypeAheadSearch, 1000);
    };
    displaySubscription = self.Display.subscribe(typeAheadDelaySearch);

    self.SelectTypeAhead = function (typeAheadResultIndex, confirm) {
        lostFocus = false;              // Make sure that losing focus doesn't clear out the value we are selecting
        self.SelectedTypeAheadIndex(typeAheadResultIndex);

        if (confirm) {
            var typeAheadResult = self.TypeAheadResults()[typeAheadResultIndex];

            idAccessorSubscription.dispose();
            idAccessor(typeAheadResult.Id());

            var display = typeAheadResult.Name() + ' (' + typeAheadResult.Identifiers() + ')';
            updateDisplay(display, false);

            idAccessorSubscription = idAccessor.subscribe(self.IdChanged);

            self.TypeAheadResults([]);
            self.ShowTypeAheadResults(false);
            self.SelectedTypeAheadIndex(undefined);
        }
    };

    self.DisplayFocusLost = function (sender, e) {
        lostFocus = true;

        // Make sure that losing focus doesn't clear out the value
        setTimeout(function () {
            if (self.ShowTypeAheadResults() && lostFocus) {
                self.ShowTypeAheadResults(false);
                self.TypeAheadResults('');
                updateDisplay('', false);
            } else if (!self.Display() && !updatingDisplay) {
                idAccessor(null);
            }
        }, 300);
    };

    self.DisplayKeyPress = function (sender, e) {
        if (!self.ShowTypeAheadResults() || self.TypeAheadResults().length == 0) {
            return true;
        }

        if (e.keyCode == 40 || (e.keyCode == 9 && !e.shiftKey)) {      // arrow down
            if (self.SelectedTypeAheadIndex() == undefined || self.SelectedTypeAheadIndex() == self.TypeAheadResults().length - 1) {
                self.SelectedTypeAheadIndex(0);
            } else if (self.SelectedTypeAheadIndex() < self.TypeAheadResults().length - 1) {
                self.SelectedTypeAheadIndex(self.SelectedTypeAheadIndex() + 1);
            }
            return false;
        } else if (e.keyCode == 38 || (e.keyCode == 9 && e.shiftKey)) {       // arrow up
            if (self.SelectedTypeAheadIndex() == 0) {
                self.SelectedTypeAheadIndex(self.TypeAheadResults().length - 1);
            } else if (self.SelectedTypeAheadIndex() > 0 || self.SelectedTypeAheadIndex() != undefined) {
                self.SelectedTypeAheadIndex(self.SelectedTypeAheadIndex() - 1);
            }
            return false;
        } else if (e.keyCode == 13) {
            if (self.SelectedTypeAheadIndex() >= 0) {
                self.SelectTypeAhead(self.SelectedTypeAheadIndex(), true);
            }
            return false;
        } else if (e.keyCode == 27) {
            self.IdChanged();
            self.TypeAheadResults([]);
            self.ShowTypeAheadResults(false);
        }

        return true;
    };

    /* Other */
    self.ModalDiv.subscribe(function () {
        if (!self.ModalDiv()) {
            return;
        }

        $(self.ModalDiv()).modal({
            show: false,
            backdrop: 'static',
            keyboard: false
        });
    });

    self.Open = function () {
        $(self.ModalDiv()).modal('show');
    };

    self.Select = function (valueset) {
        self.SelectedValueset(valueset);
    };

    self.OK = function () {
        if (!self.SelectedValueset()) {
            alert("You must select a value set before accepting...");
            return;
        }

        var display = self.SelectedValueset().Name() + ' (' + self.SelectedValueset().Identifiers() + ')';
        self.Display(display);
        idAccessor(self.SelectedValueset().Id());
        $(self.ModalDiv()).modal('hide');
    };
};

ko.bindingHandlers.valuesetSelect = {
    init: function (element, valueAccessor, allBindings) {
        var value = valueAccessor();
        var allBindingsUnwrapped = allBindings();

        var valuesetSelectViewModel = new ValuesetSelectViewModel(value);

        valuesetSelectViewModel.Buttons(allBindingsUnwrapped.buttons);
        valuesetSelectViewModel.Label(allBindingsUnwrapped.label);
        valuesetSelectViewModel.IsSmall(allBindingsUnwrapped.small || false);
        valuesetSelectViewModel.CanTypeAhead(allBindingsUnwrapped.canTypeAhead || false);

        if (allBindingsUnwrapped.disable) {
            if (typeof allBindingsUnwrapped.disable === 'function') {
                valuesetSelectViewModel.Disabled(allBindingsUnwrapped.disable() || false);
                allBindingsUnwrapped.disable.subscribe(function (newDisabled) {
                    valuesetSelectViewModel.Disabled(newDisabled);
                });
            } else {
                valuesetSelectViewModel.Disabled(allBindingsUnwrapped.disable || false);
            }
        }

        if (value()) {
            valuesetSelectViewModel.IdChanged();
        }

        ko.renderTemplate('selectValuesetDialogTemplate', valuesetSelectViewModel, {
            afterRender: function (renderedElement) {
                var modalDiv = renderedElement[4];
                $(modalDiv).detach().appendTo($('body'));
                valuesetSelectViewModel.ModalDiv(modalDiv);
            }
        }, element, "replaceNode");
    },
    update: function (element, valueAccessor, allBindings) {
    }
};