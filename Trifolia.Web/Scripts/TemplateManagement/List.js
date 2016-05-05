var BrowseTemplatesViewModel = function () {
    var self = this;

    self.Model = ko.observable();
    self.ItemCount = ko.observable(50);
    self.PageCount = ko.observable(parseInt($.cookie('browse_templates_page_count')) || 1);
    self.SortProperty = ko.observable($.cookie('browse_templates_sort_property') || 'Name');
    self.SortDescending = ko.observable($.cookie('browse_templates_sort_desc') === "true" || false);
    self.SearchQuery = ko.observable($.cookie('browse_templates_search_query') || '');
    self.FilterName = ko.observable($.cookie('browse_templates_filter_name') || '');
    self.FilterOid = ko.observable($.cookie('browse_templates_filter_oid') || '');
    self.FilterImplementationGuideId = ko.observable();
    self.FilterTemplateTypeId = ko.observable();
    self.FilterOrganizationId = ko.observable();
    self.TemplateTypes = ko.observableArray([]);
    self.ImplementationGuides = ko.observableArray([]);
    self.Organizations = ko.observableArray([]);

    self.PageCount.subscribe(function () {
        console.log('changed');
    });

    var searchTimeout = null;
    var delayedSearch = function (newVal) {
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }

        searchTimeout = setTimeout(self.Search, 500);
    };

    self.TotalPages = ko.computed(function () {
        if (!self.Model() || self.Model().TotalItems() == 0) {
            return 0;
        }

        var ret = Math.ceil(self.Model().TotalItems() / self.ItemCount());

        return ret < 1 ? 1 : ret;
    });

    self.TotalItems = ko.computed(function () {
        if (!self.Model() || self.Model().TotalItems() == 0) {
            return 0;
        }

        return self.Model().TotalItems();
    });

    self.Initialize = function () {
        $.ajax({
            url: '/api/ImplementationGuide/All/TemplateType',
            cache: false,
            async: false,
            success: function (templateTypes) {
                ko.mapping.fromJS({ TemplateTypes: templateTypes }, {}, self);
                self.FilterTemplateTypeId($.cookie('browse_templates_filter_templatetypeid') || '');
            }
        });

        $.ajax({
            url: '/api/ImplementationGuide',
            cache: false,
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ ImplementationGuides: results.Items }, {}, self);
                self.FilterImplementationGuideId($.cookie('browse_templates_filter_implementationguideid') || '');
            }
        });

        if (containerViewModel.HasSecurable(['OrganizationList'])) {
            $.ajax({
                url: '/api/Organization',
                cache: false,
                async: false,
                success: function (results) {
                    ko.mapping.fromJS({ Organizations: results }, {}, self);
                    self.FilterOrganizationId($.cookie('browse_templates_filter_organizationid') || '');
                }
            });
        }

        self.LoadTemplates();
    };

    self.LoadTemplates = function () {
        fireTrifoliaEvent('templatesLoading');

        var url = '/api/Template?count=' + self.ItemCount() +
                '&page=' + self.PageCount() +
                '&sortProperty=' + self.SortProperty() +
                '&sortDescending=' + self.SortDescending() +
                '&queryText=' + encodeURIComponent(self.SearchQuery()) +
                '&filterName=' + encodeURIComponent(self.FilterName()) +
                '&filterOid=' + encodeURIComponent(self.FilterOid()) +
                '&filterImplementationGuideId=' + self.FilterImplementationGuideId() +
                '&filterTemplateTypeId=' + self.FilterTemplateTypeId() +
                '&filterOrganizationId=' + self.FilterOrganizationId();

        $.ajax({
            url: url,
            cache: false,
            success: function (model) {
                ko.mapping.fromJS({ Model: model }, {}, self);
                fireTrifoliaEvent('templatesLoaded');
            }
        });
    };

    self.ToggleSort = function (property) {
        if (self.SortProperty() != property) {
            self.SortDescending(false);
            self.SortProperty(property);
        } else {
            self.SortDescending(!self.SortDescending());
        }

        $.cookie('browse_templates_sort_property', self.SortProperty());
        $.cookie('browse_templates_sort_desc', self.SortDescending());

        self.LoadTemplates();
    };

    self.ClearFilter = function () {
        self.FilterName('');
        self.FilterOid('');
        self.FilterImplementationGuideId(undefined);
        self.FilterTemplateTypeId(undefined);
        self.FilterOrganizationId(undefined);
    };

    self.Search = function () {
        if (!self.SearchQuery()) {
            $.removeCookie('browse_templates_search_query');
        } else {
            $.cookie('browse_templates_search_query', self.SearchQuery());
        }

        if (!self.FilterName()) {
            $.removeCookie('browse_templates_filter_name');
        } else {
            $.cookie('browse_templates_filter_name', self.FilterName());
        }

        if (!self.FilterOid()) {
            $.removeCookie('browse_templates_filter_oid');
        } else {
            $.cookie('browse_templates_filter_oid', self.FilterOid());
        }

        if (!self.FilterImplementationGuideId()) {
            $.removeCookie('browse_templates_filter_implementationguideid');
        } else {
            $.cookie('browse_templates_filter_implementationguideid', self.FilterImplementationGuideId());
        }

        if (!self.FilterTemplateTypeId()) {
            $.removeCookie('browse_templates_filter_templatetypeid');
        } else {
            $.cookie('browse_templates_filter_templatetypeid', self.FilterTemplateTypeId());
        }

        if (!self.FilterOrganizationId()) {
            $.removeCookie('browse_templates_filter_organizationid');
        } else {
            $.cookie('browse_templates_filter_organizationid', self.FilterOrganizationId());
        }

        self.PageCount(1);
        $.cookie('browse_templates_page_count', 1);
        self.LoadTemplates();
    };

    self.PreviousPage = function () {
        self.PageCount(self.PageCount() - 1);
        $.cookie('browse_templates_page_count', self.PageCount());
        self.LoadTemplates();
    };

    self.NextPage = function () {
        self.PageCount(self.PageCount() + 1);
        $.cookie('browse_templates_page_count', self.PageCount());
        self.LoadTemplates();
    };

    self.LastPage = function () {
        self.PageCount(self.TotalPages());
        $.cookie('browse_templates_page_count', self.PageCount());
        self.LoadTemplates();
    };

    self.FirstPage = function () {
        self.PageCount(1);
        $.cookie('browse_templates_page_count', self.PageCount());
        self.LoadTemplates();
    };

    self.ViewTemplateUrl = function (template) {
        return getTemplateViewUrl(template.Id(), template.Oid());
    };

    self.EditTemplateUrl = function (template) {
        return getTemplateEditUrl(template.Id(), template.Oid());
    };

    self.WatchFilters = function () {
        self.SearchQuery.subscribe(delayedSearch);
        self.FilterName.subscribe(delayedSearch);
        self.FilterOid.subscribe(delayedSearch);
        self.FilterImplementationGuideId.subscribe(delayedSearch);
        self.FilterTemplateTypeId.subscribe(delayedSearch);
        self.FilterOrganizationId.subscribe(delayedSearch);
    };

    self.Initialize();
};