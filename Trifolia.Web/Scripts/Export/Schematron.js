TemplateItemModel = function (data) {
    var self = this;
    var mapping = {};

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.ThisIG = ko.observable();
    
    self.IsSelected = ko.observable(true);

    ko.mapping.fromJS(data, mapping, self);
};

var ExportSchematronViewModel = function (implementationGuideId) {
    var self = this;
    var mapping = {
        'Templates': {
            create: function (options) {
                return new TemplateItemModel(options.data);
            }
        }
    };

    self.ImplementationGuideId = ko.observable();
    self.Name = ko.observable();
    self.CancelUrl = ko.observable();
    self.Templates = ko.observableArray([]);
    self.SortProperty = ko.observable("Name");
    self.Categories = ko.observableArray([]);
    self.SelectedCategories = ko.observableArray([]);

    // Custom view properties
    self.CheckAllTemplates = ko.observable(true);
    self.IncludeInferred = ko.observable(true);
    self.IncludeInferred.ForBinding = ko.computed({
        read: function () {
            return self.IncludeInferred().toString();
        },
        write: function (value) {
            self.IncludeInferred(value === 'true');
        }
    });

    var sortTemplates = function () {
        self.Templates.sort(
            function (left, right) {
                if (self.SortProperty() == "Name")
                    return left.Name() == right.Name() ? 0 : (left.Name() < right.Name() ? -1 : 1);
                else if (self.SortProperty() == "OID")
                    return left.Oid() == right.Oid() ? 0 : (left.Oid() < right.Oid() ? -1 : 1);
                else if (self.SortProperty() == "ThisIG")
                    return left.ThisIG() == right.ThisIG() ? 0 : (left.ThisIG() < right.ThisIG() ? -1 : 1);
                return 0;
            });
    };

    self.ViewImplementationGuideLink = ko.computed(function () {
        var url = '';
        if (self.ImplementationGuideId !== undefined)
            url = '/IGManagement/View/' + self.ImplementationGuideId();
        return url;
    });

    self.TotalTemplates = ko.computed(function () {
        if (self.Templates() === undefined)
            return "";

        var ret = self.Templates().length + " total, " + $(".templateCheck:checked").length + " selected";
    });

    // Subscriptions
    self.CheckAllTemplates.subscribe(function (newValue) {
        for (var i = 0; i < self.Templates().length; i++) {
            var cTemplate = self.Templates()[i];
            cTemplate.IsSelected(newValue);
        }
    });

    // Events
    self.Export = function() {
        var selectedTemplates = $("input[name=TemplateIds]:checked");

        if (selectedTemplates.length == 0) {
            alert("You must select at least one template to export.");
            return;
        }

        $("#ExportSchematronForm").attr("action", "/api/Export/Schematron");
        $("#ExportSchematronForm").submit();
    }

    self.Cancel = function() {
        location.href = '/IGManagement/View/' + implementationGuideId;
    }

    self.SortTemplates = function (property) {
        self.SortProperty(property);
        sortTemplates();
    };

    self.RefreshTemplates = function () {
        self.Templates([]);

        var url = '/api/ImplementationGuide/' + implementationGuideId + '/Template?inferred=' + self.IncludeInferred();

        for (var i in self.SelectedCategories()) {
            var category = self.SelectedCategories()[i];

            if (category) {
                url += '&categories=' + encodeURIComponent(category);
            }
        }

        $.ajax({
            url: url,
            success: function (results) {
                ko.mapping.fromJS({ Templates: results }, mapping, self);
            }
        });
    };
    self.IncludeInferred.subscribe(self.RefreshTemplates);
    self.SelectedCategories.subscribe(self.RefreshTemplates);

    self.Initialize = function () {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/Schematron',
            success: function (results) {
                ko.mapping.fromJS(results, {}, self);
            }
        });

        self.RefreshTemplates();
    };

    self.Initialize();
};