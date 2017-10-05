var ViewTypesViewModel = function () {
    var self = this;

    self.Types = ko.observableArray([]);

    self.Initialize = function () {
        $.ajax({
            url: '/api/Type',
            success: function (results) {
                ko.mapping.fromJS({ Types: results }, {}, self);
            }
        });
    };

    self.Initialize();
};