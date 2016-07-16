var OrganizationListViewModel = function () {
    var self = this;

    self.Organizations = ko.observableArray([]);
    self.NewOrganizationName = ko.observable().extend({ required: true, maxLength: 255 });

    self.EditDetails = function (organization) {
        location.href = '/Organization/Details/' + organization.Id();
    };

    self.ShowAddOrganization = function () {
        self.NewOrganizationName('');
        $('#addOrganizationDialog').modal('show');
    };

    self.AddOrganization = function () {
        var data = {
            Name: self.NewOrganizationName()
        };

        $.ajax({
            method: 'POST',
            url: '/api/Organization',
            data: data,
            success: function () {
                self.Initialize();
                self.CancelAddOrganization();
                alert('Successfully saved organization!');
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while saving the organization');
            }
        });
    };

    self.CancelAddOrganization = function () {
        self.NewOrganizationName('');
        $('#addOrganizationDialog').modal('hide');
    };

    self.Delete = function (organization) {
        if (!confirm("Are you sure you want to delete this organization?")) {
            return;
        }

        $.ajax({
            method: 'DELETE',
            url: '/api/Organization/' + organization.Id(),
            success: function () {
                self.Initialize();
                alert('Successfully deleted organization!');
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while deleting the organization');
            }
        });
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Organization',
            success: function (results) {
                ko.mapping.fromJS({ Organizations: results }, {}, self);
            }
        });
    };

    self.Initialize();
};