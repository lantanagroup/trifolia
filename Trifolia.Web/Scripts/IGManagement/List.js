var BrowseImplementationGuideViewModel = function (listMode) {
    var self = this;

    var caseInsensitiveStringCompare = function (a, b) {
        return a.toLowerCase().localeCompare(b.toLowerCase());
    };

    var sortItems = function (left, right) {
        var lessThan = self.SortAscending() ? -1 : 1;
        var greaterThan = self.SortAscending() ? 1 : -1;

        var leftVal = left.Title();
        var rightVal = right.Title();

        switch (self.SortField()) {
            case 'Name':
                leftVal = left.Title().trim().toLowerCase();
                rightVal = right.Title().trim().toLowerCase();
                break;
            case 'Status':
                leftVal = left.Status().trim().toLowerCase();
                rightVal = right.Status().trim().toLowerCase();
                break;
            case 'PublishDate':
                if (!left.PublishDate()) {
                    leftVal = null;
                } else {
                    leftVal = new Date(left.PublishDate());
                }

                if (!right.PublishDate()) {
                    rightVal = null;
                } else {
                    rightVal = new Date(right.PublishDate());
                }

                break;
            case 'Organization':
                leftVal = left.Organization().trim().toLowerCase();
                rightVal = right.Organization().trim().toLowerCase();
                break;
            case 'Type':
                leftVal = left.Type().trim().toLowerCase();
                rightVal = right.Type().trim().toLowerCase();
                break;
        }

        if (leftVal === null && rightVal !== null)
            return lessThan;
        else if (rightVal === null && leftVal !== null)
            return greaterThan;

        return leftVal === rightVal ? 0 : (leftVal < rightVal ? lessThan : greaterThan);
    };

    // Properties
    self.Model = ko.observable(null);
    self.SearchQuery = ko.observable($.cookie('BrowseImplementationGuide_SearchQuery') || '');
    self.SortField = ko.observable($.cookie('BrowseImplementationGuide_SortField') || 'Name');
    self.SortAscending = ko.observable($.cookie('BrowseImplementationGuide_SortAscending') === "false" ? false : true);
    self.FilterType = ko.observable($.cookie('BrowseImplementationGuide_FilterType') || '');
    self.FilterStatus = ko.observable($.cookie('BrowseImplementationGuide_FilterStatus') || '');
    self.FilterOrganization = ko.observable($.cookie('BrowseImplementationGuide_FilterOrganization') || '');
    self.UnauthorizedImplementationGuides = ko.observableArray([]);
    self.AccessRequestMessage = ko.observable();
    self.RequestEditAccess = ko.observable(false);
    self.RequestAccessMessage = ko.observable('');
    self.Types = ko.observableArray(self.FilterType() ? [self.FilterType()] : []);
    self.Statuses = ko.observableArray(self.FilterStatus() ? [self.FilterStatus()] : []);
    self.Organizations = ko.observableArray(self.FilterOrganization() ? [self.FilterOrganization()] : []);

    var storeCookies = function() {
        $.cookie('BrowseImplementationGuide_SortField', self.SortField());
        $.cookie('BrowseImplementationGuide_SortAscending', self.SortAscending());
        $.cookie('BrowseImplementationGuide_SearchQuery', self.SearchQuery());
        $.cookie('BrowseImplementationGuide_FilterType', self.FilterType());
        $.cookie('BrowseImplementationGuide_FilterStatus', self.FilterStatus());
        $.cookie('BrowseImplementationGuide_FilterOrganization', self.FilterOrganization());
    };

    self.SearchQuery.subscribe(storeCookies);
    self.SortField.subscribe(storeCookies);
    self.SortAscending.subscribe(storeCookies);
    self.FilterType.subscribe(storeCookies);
    self.FilterStatus.subscribe(storeCookies);
    self.FilterOrganization.subscribe(storeCookies);

    self.GetSortIcon = function (sortField) {
        return {
            'glyphicon-text-height': self.SortField() !== sortField,
            'glyphicon-sort-by-alphabet': self.SortField() === sortField && self.SortAscending(),
            'glyphicon-sort-by-alphabet-alt': self.SortField() === sortField && !self.SortAscending()
        };
    };

    self.RequestEditAccessString = ko.computed({
        read: function () {
            return self.RequestEditAccess().toString();
        },
        write: function (value) {
            self.RequestEditAccess(value === "true");
        }
    });

    self.ShowEdit = ko.computed(function () {
        return !listMode || listMode === 'Default';
    });

    self.Title = ko.computed(function () {
        switch (listMode) {
            case 'Default':
                return 'Browse Implementation Guides';
            case 'Files':
                return 'Manage Implementation Guide Files';
            case 'Test':
                return 'Test Implementation Guide';
            case 'ExportMSWord':
                return 'Export MS Word';
            case 'ExportXML':
                return 'Export XML';
            case 'ExportVocab':
                return 'Export Vocabulary';
            case 'ExportSchematron':
                return 'Export Schematron';
            case 'ExportGreen':
                return 'Export Green Artifacts';
        }

        return 'UNKNOWN: ';
    });

    // Methods
    self.Initialize = function () {
        fireTrifoliaEvent('implementationGuidesLoading');

        // Get list of templates
        $.ajax({
            url: "/api/ImplementationGuide?listMode=" + listMode,
            async: true,
            success: function (model) {
                ko.mapping.fromJS({ Model: model }, {}, self);
                self.Model().Items.sort(sortItems);

                var types = _.chain(self.Model().Items())
                    .map(function (item) {
                        return item.Type();
                    })
                    .uniq()
                    .value();
                self.Types(types.sort(caseInsensitiveStringCompare));

                var statuses = _.chain(self.Model().Items())
                    .map(function (item) {
                        return item.Status();
                    })
                    .uniq()
                    .value();
                self.Statuses(statuses.sort(caseInsensitiveStringCompare));

                var organizations = _.chain(self.Model().Items())
                    .map(function (item) {
                        return item.Organization();
                    })
                    .uniq()
                    .value();
                self.Organizations(organizations.sort(caseInsensitiveStringCompare));

                fireTrifoliaEvent('implementationGuidesLoaded');
            }
        });

        $.ajax({
            url: "/api/ImplementationGuide/Unauthorized",
            async: true,
            success: function (results) {
                var sortedResults = _.sortBy(results, function (result) {
                    return result.Name;
                });

                ko.mapping.fromJS({ UnauthorizedImplementationGuides: sortedResults }, {}, self);
            }
        });
    };

    self.OpenRequestAccess = function () {
        $('#requestAccessDialog').modal('show');
    };

    self.RequestAccess = function (implementationGuideId) {
        self.AccessRequestMessage('');
        var implementationGuide = ko.utils.arrayFirst(self.UnauthorizedImplementationGuides(), function (item) {
            return item.Id() === implementationGuideId;
        });

        var url = '/api/ImplementationGuide/' + implementationGuideId + '/RequestAuthorization';
        url += '?edit=' + self.RequestEditAccess();
        url += '&message=' + encodeURIComponent(self.RequestAccessMessage());

        $.ajax({
            url: url,
            method: 'POST',
            success: function (results) {
                self.AccessRequestMessage('Successfully sent access request for ' + implementationGuide.Name() + '.');
                self.UnauthorizedImplementationGuides.remove(implementationGuide);
                self.RequestEditAccess(false);
            },
            error: function (err) {
                self.AccessRequestMessage('An error occurred while sending the request!');
            }
        });
    };

    self.ToggleSort = function (sortField) {
        if (self.SortField() === sortField) {
            self.SortAscending(!self.SortAscending());
        } else {
            self.SortField(sortField);
            self.SortAscending(true);
        }

        self.Model().Items.sort(sortItems);
    };

    self.GetItems = ko.computed(function () {
        var filterType = self.FilterType();
        var filterStatus = self.FilterStatus();
        var filterOrganization = self.FilterOrganization();
        var searchQuery = self.SearchQuery();

        if (!self.Model()) {
            return [];
        }

        if (!searchQuery && !filterType && !filterStatus && !filterOrganization) {
            return self.Model().Items();
        }

        var matches = [];
        var regexp = new RegExp(self.SearchQuery(), 'gi');

        for (var i = 0; i < self.Model().Items().length; i++) {
            var item = self.Model().Items()[i];
            var itemDate = formatDateObj(item.PublishDate());

            var matched = !searchQuery;

            if (searchQuery && regexp.test(item.Title())) {
                matched = true;
            }

            if (searchQuery && regexp.test(itemDate)) {
                matched = true;
            }

            if (searchQuery && regexp.test(item.Status())) {
                matched = true;
            }

            if (searchQuery && regexp.test(item.Organization())) {
                matched = true;
            }

            if (self.FilterType() && item.Type() !== filterType) {
                matched = false;
            }

            if (self.FilterStatus() && item.Status() != filterStatus) {
                matched = false;
            }

            if (self.FilterOrganization() && item.Organization() != filterOrganization) {
                matched = false;
            }

            if (matched) {
                matches.push(item);
            }
        }

        return matches;
    });

    self.Add = function () {
        location.href = '/IGManagement/Edit';
    };

    self.SelectDisplayText = ko.computed(function() {
        switch (listMode) {
            case 'Default':
                return 'View';
            default:
                return 'Select';
        }
    });

    self.Initialize();
};