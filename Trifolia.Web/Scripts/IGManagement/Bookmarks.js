var EditBookmarksViewModel = function (implementationGuideId) {
    var self = this;

    self.Model = ko.observable(new EditBookmarksModel());

    self.Initialize = function () {
        $.blockUI({ message: 'Loading...' });
        $.ajax({
            url: '/api/ImplementationGuide/' + implementationGuideId + '/Edit/Bookmark',
            success: function (results) {
                var model = new EditBookmarksModel(results);
                self.Model(model);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Save = function () {
        var data = ko.mapping.toJS(self.Model());
        
        $.blockUI({ message: 'Saving...' });
        $.ajax({
            method: 'POST',
            url: '/api/ImplementationGuide/Edit/Bookmark',
            data: data,
            success: function () {
                $.unblockUI();
                alert("Successfully saved changes!");
            }
        });
    };

    self.RegenerateAll = function () {
        $.blockUI({ message: 'Regenerating bookmarks...' });
        $.ajax({
            method: 'POST',
            url: '/api/ImplementationGuide/' + implementationGuideId + '/Edit/Bookmark/Regenerate',
            success: function () {
                $.unblockUI();
                alert("Successfully re-generated bookmarks!");

                self.Initialize();
            }
        });
    };

    self.Cancel = function () {
        location.href = '/IGManagement/View/' + implementationGuideId;
    };

    self.Initialize();
};

var EditBookmarksModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'ImplementationGuideId', 'Name', 'TemplateItems' ],
        'TemplateItems': {
            create: function (options) {
                return new TemplateItemModel(options.data);
            }
        }
    };

    self.ImplementationGuideId = ko.observable();
    self.Name = ko.observable();
    self.TemplateItems = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);

    self.IsValid = ko.computed(function () {
        var foundInvalid = ko.utils.arrayFirst(self.TemplateItems(), function (templateItem) {
            return !templateItem.IsValid();
        });

        return !foundInvalid;
    });
};

var TemplateItemModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Name', 'Bookmark' ]
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Bookmark = ko.observable();

    ko.mapping.fromJS(data, mapping, self);

    self.Validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, maxLength: 255 }),
        Bookmark: self.Bookmark.extend({ required: true, maxLength: 40 })
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });
};