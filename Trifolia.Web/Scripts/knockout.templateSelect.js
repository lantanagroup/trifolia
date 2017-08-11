var TemplateSelectViewModel = function (idAccessor, isReference) {
	var self = this;

	self.DisplayDiv = ko.observable();
	self.ModalDiv = ko.observable();
	self.SearchQuery = ko.observable('');
	self.SelectTemplates = ko.observableArray(null);
	self.SelectedTemplate = ko.observable();
	self.Display = ko.observable();
	self.FilterImplementationGuideId = ko.observable(null);
	self.ignoreSelfOid = ko.observable();
	self.FilterContextType = ko.observable(null);
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

	var idAccessorSubscription, displaySubscription, displayFocusedSubscription;
	var updatingDisplay = false;
	var lostFocus = false;
	var dialogOpen = false;

	var updateDisplay = function (newValue, notify) {
	    updatingDisplay = !notify;
	    self.Display(newValue);
	    updatingDisplay = false;
	};

	self.IdChanged = function () {
	    if (!idAccessor()) {
	        updateDisplay('', false);
	        return;
        }

        var url = '/api/Template/' + encodeURIComponent(idAccessor());

        if (isReference) {
            url = '/api/Template/Identifier?identifier=' + encodeURIComponent(idAccessor());
        }

	    $.ajax({
	        url: url,
	        success: function (template) {
	            var display = template.Name + ' (' + template.Oid + ')';
	            updateDisplay(display, false);
            },
            error: function (err, data, other) {
                console.log(err);
            }
	    });
	};
	idAccessorSubscription = idAccessor.subscribe(self.IdChanged);

	self.Clear = function () {
	    updateDisplay('', false);
	    idAccessor(null);
	};

    /* Search in modal */
	self.SearchTemplates = function () {
	    if (!dialogOpen) {
	        return;
	    }

	    if (self.CurrentSearchPage() == 1) {
	        self.SelectTemplates(null);
	    }

		var url = '/api/Template?count=5&page=' + self.CurrentSearchPage();

		if (self.SearchQuery()) {
			url += '&queryText=' + encodeURIComponent(self.SearchQuery());
		}

		if (self.FilterImplementationGuideId()) {
			url += '&filterImplementationGuideId=' + self.FilterImplementationGuideId();
		}

		if (self.FilterContextType()) {
		    url += '&filterContextType=' + encodeURIComponent(self.FilterContextType());
		}

		if (self.ignoreSelfOid())
		    url += '&selfOid=' + self.ignoreSelfOid();

		self.IsSearching(true);
		$.ajax({
		    url: url,
            cache: false,
            success: function (results) {
                var templates = ko.mapping.fromJS(results);
                
                if (self.SelectTemplates() == null) {
                    self.SelectTemplates(templates);
                } else {
                    var moreTemplates = self.SelectTemplates().Items();
                    moreTemplates = moreTemplates.concat(templates.Items());
                    self.SelectTemplates().Items(moreTemplates);
                }

			    self.IsSearching(false);
			}
		});
	};

	self.SearchMoreResults = function () {
	    self.CurrentSearchPage(self.CurrentSearchPage() + 1);
	    self.SearchTemplates();
	};

	var delayTimer = null;
	var delaySearch = function () {
	    if (delayTimer) {
			clearTimeout(delayTimer);
		}

	    delayTimer = setTimeout(function () {
	        self.CurrentSearchPage(1);
	        self.SearchTemplates();
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

	    var url = '/api/Template?count=5';

	    if (self.Display()) {
	        url += '&queryText=' + encodeURIComponent(self.Display());
	    }

	    if (self.FilterImplementationGuideId()) {
	        url += '&filterImplementationGuideId=' + self.FilterImplementationGuideId();
	    }

	    if (self.FilterContextType()) {
	        url += '&filterContextType=' + encodeURIComponent(self.FilterContextType());
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
	    lostFocus = false;          // Make sure that losing focus doesn't clear out the value we are selecting
	    self.SelectedTypeAheadIndex(typeAheadResultIndex);

	    if (confirm) {
	        var typeAheadResult = self.TypeAheadResults()[typeAheadResultIndex];

	        idAccessorSubscription.dispose();
	        idAccessor(typeAheadResult.Id());
            
	        var display = typeAheadResult.Name() + ' (' + typeAheadResult.Oid() + ')';
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
	    dialogOpen = true;
	    self.SearchTemplates();
	};

	self.Select = function (template) {
		self.SelectedTemplate(template);
	};

	self.OK = function () {
		if (!self.SelectedTemplate()) {
			alert("You must select a template before accepting...");
			return;
		}

		var display = self.SelectedTemplate().Name() + ' (' + self.SelectedTemplate().Oid() + ')';
		updateDisplay(display, false);
		idAccessor(self.SelectedTemplate().Id());
		$(self.ModalDiv()).modal('hide');
		dialogOpen = false;
	};

	self.Cancel = function () {
	    $(self.ModalDiv()).modal('hide');
	    dialogOpen = false;
	};
};

ko.bindingHandlers.templateSelect = {
	init: function (element, valueAccessor, allBindings) {
		var value = valueAccessor();
        var allBindingsUnwrapped = allBindings();
        var isReference = false;

        if (typeof allBindingsUnwrapped.isReference === 'function')
            isReference = allBindingsUnwrapped.isReference();
        else
            isReference = allBindingsUnwrapped.isReference;

		var templateSelectViewModel = new TemplateSelectViewModel(value, isReference);

		if (typeof allBindingsUnwrapped.oid === 'function' && allBindingsUnwrapped.oid)
		    templateSelectViewModel.ignoreSelfOid(allBindingsUnwrapped.oid());

		if (typeof allBindingsUnwrapped.filterImplementationGuideId === 'object') {
			templateSelectViewModel.FilterImplementationGuideId(allBindingsUnwrapped.filterImplementationGuideId);
			templateSelectViewModel.SearchTemplates();
		} else if (typeof allBindingsUnwrapped.filterImplementationGuideId === 'function') {
			var setFilterImplementationGuideId = function() {
			    if (templateSelectViewModel.IsSearching()) {
			        var searchWaitSubscription;
			        searchWaitSubscription = templateSelectViewModel.IsSearching.subscribe(function(isSearching) {
			            if (!isSearching) {
			                searchWaitSubscription.dispose();
			                templateSelectViewModel.FilterImplementationGuideId(allBindingsUnwrapped.filterImplementationGuideId());
			                templateSelectViewModel.SearchTemplates();
			            }
			        });
			    } else {
			        templateSelectViewModel.FilterImplementationGuideId(allBindingsUnwrapped.filterImplementationGuideId());
			        templateSelectViewModel.SearchTemplates();
			    }
			};

			allBindingsUnwrapped.filterImplementationGuideId.subscribe(setFilterImplementationGuideId);
			setFilterImplementationGuideId();
		}

		templateSelectViewModel.Buttons(allBindingsUnwrapped.buttons);
		templateSelectViewModel.Label(allBindingsUnwrapped.label);
		templateSelectViewModel.IsSmall(allBindingsUnwrapped.small || false);
		templateSelectViewModel.CanTypeAhead(allBindingsUnwrapped.canTypeAhead || false);

		if (allBindingsUnwrapped.disable) {
		    if (typeof allBindingsUnwrapped.disable === 'function') {
		        templateSelectViewModel.Disabled(allBindingsUnwrapped.disable() || false);
		        allBindingsUnwrapped.disable.subscribe(function (newDisabled) {
		            templateSelectViewModel.Disabled(newDisabled);
		        });
		    } else {
		        templateSelectViewModel.Disabled(allBindingsUnwrapped.disable || false);
		    }
		}

		if (allBindingsUnwrapped.filterContextType) {
		    if (typeof allBindingsUnwrapped.filterContextType === 'function') {
		        var filterContextType = allBindingsUnwrapped.filterContextType();

		        // Ignore FHIR ResourceReference, since those can be anything
		        if (filterContextType != 'ResourceReference' && filterContextType != 'Reference') {
		            templateSelectViewModel.FilterContextType(allBindingsUnwrapped.filterContextType());
		            allBindingsUnwrapped.filterContextType.subscribe(function (newFilterContextType) {
		                templateSelectViewModel.FilterContextType(newFilterContextType);
		            });
		        }
		    } else {
		        // Ignore FHIR ResourceReference, since those can be anything
		        if (allBindingsUnwrapped.filterContextType != 'ResourceReference' && allBindingsUnwrapped.filterContextType != 'Reference') {
		            templateSelectViewModel.FilterContextType(allBindingsUnwrapped.filterContextType);
		        }
		    }
		}

		if (value()) {
		    templateSelectViewModel.IdChanged();
		}

		ko.renderTemplate('selectTemplateDialogTemplate', templateSelectViewModel, {
		    afterRender: function (renderedElement) {
		        var modalDiv = renderedElement[3];
		        $(modalDiv).detach().appendTo($('body'));
				templateSelectViewModel.ModalDiv(modalDiv);
			}
		}, element, "replaceNode");
	},
	update: function (element, valueAccessor, allBindings) {
	}
};