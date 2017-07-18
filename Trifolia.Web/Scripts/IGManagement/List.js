var BrowseImplementationGuideViewModel = function (listMode) {
    var self = this;

    var sortItems = function (left, right) {
        var lessThan = self.SortAscending() ? -1 : 1;
        var greaterThan = self.SortAscending() ? 1 : -1;

        var leftVal = left.Title();
        var rightVal = right.Title();

        switch (self.SortField()) {
            case 'Name':
                leftVal = left.Title();
                rightVal = right.Title();
                break;
            case 'Status':
                leftVal = left.Status();
                rightVal = right.Status();
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
                leftVal = left.Organization();
                rightVal = right.Organization();
                break;
        }

        if (leftVal == null && rightVal != null)
            return lessThan;
        else if (rightVal == null && leftVal != null)
            return greaterThan;

        return leftVal == rightVal ? 0 : (leftVal < rightVal ? lessThan : greaterThan);
    };

    // Properties
    self.Model = ko.observable(null);
    self.SearchQuery = ko.observable($.cookie('BrowseImplementationGuide_SearchQuery') || '');
    self.SortField = ko.observable($.cookie('BrowseImplementationGuide_SortField') || 'Name');
    self.SortAscending = ko.observable($.cookie('BrowseImplementationGuide_SortAscending') == "false" ? false : true);
    self.UnauthorizedImplementationGuides = ko.observableArray([]);
    self.AccessRequestMessage = ko.observable();
    self.RequestEditAccess = ko.observable(false);
    self.RequestAccessMessage = ko.observable('');

    var storeCookies = function() {
        $.cookie('BrowseImplementationGuide_SortField', self.SortField());
        $.cookie('BrowseImplementationGuide_SortAscending', self.SortAscending());
        $.cookie('BrowseImplementationGuide_SearchQuery', self.SearchQuery());
    };

    self.SearchQuery.subscribe(storeCookies);
    self.SortField.subscribe(storeCookies);
    self.SortAscending.subscribe(storeCookies);

    self.RequestEditAccessString = ko.computed({
        read: function () {
            return self.RequestEditAccess().toString();
        },
        write: function (value) {
            self.RequestEditAccess(value === "true");
        }
    });

    self.ShowEdit = ko.computed(function () {
        return !listMode || listMode == 'Default';
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
            return item.Id() == implementationGuideId;
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
        if (self.SortField() == sortField) {
            self.SortAscending(!self.SortAscending());
        } else {
            self.SortField(sortField);
            self.SortAscending(true);
        }

        self.Model().Items.sort(sortItems);
    };

    self.GetItems = ko.computed(function () {
        if (!self.Model()) {
            return [];
        }

        if (self.SearchQuery().length == 0) {
            return self.Model().Items();
        }

        var matches = [];
        var regexp = new RegExp(self.SearchQuery(), 'gi');

        for (var i = 0; i < self.Model().Items().length; i++) {
            var item = self.Model().Items()[i];
            var itemDate = formatDateObj(item.PublishDate());

            var matched = false;

            if (regexp.test(item.Title())) {
                matched = true;
            }

            if (regexp.test(itemDate)) {
                matched = true;
            }

            if (regexp.test(item.Status())) {
                matched = true;
            }

            if (regexp.test(item.Organization())) {
                matched = true;
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