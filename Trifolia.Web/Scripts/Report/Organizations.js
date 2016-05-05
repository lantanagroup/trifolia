var OrganizationsReportModel = function () {
    var self = this;
    var mapping = {

    };

    self.Organizations = ko.observable();
    self.FilterOkayToContact = ko.observable();

    self.FilterUsers = function (users) {
        var filtered = [];

        for (var i in users) {
            if (!self.FilterOkayToContact() || self.FilterOkayToContact() == users[i].OkayToContact().toString()) {
                filtered.push(users[i]);
            }
        }

        return filtered;
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Report/Organization',
            success: function (results) {
                ko.mapping.fromJS({ Organizations: results }, mapping, self);
                $('.nav-tabs').tab();
                $('#orgTabs a:first').tab('show');
            }
        });
    };

    self.Initialize();
};