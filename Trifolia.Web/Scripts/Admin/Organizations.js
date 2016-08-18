var OrganizationListViewModel = function () {
    var self = this;

    self.Organizations = ko.observableArray([]);
    self.NewOrganizationName = ko.observable().extend({ required: true, maxLength: 255 });
    self.CurrentOrganizationId = ko.observable();

    self.EditOrganization = function (organization) {
        if (organization) {
            self.NewOrganizationName(organization.Name());
            self.CurrentOrganizationId(organization.Id());
        } else {
            self.NewOrganizationName('');
            self.CurrentOrganizationId(null);
        }
        $('#editOrganizationDialog').modal('show');
    };

    self.SaveOrganization = function () {
        var data = {
            Id: self.CurrentOrganizationId() ? self.CurrentOrganizationId() : null,
            Name: self.NewOrganizationName()
        };

        $.ajax({
            method: 'POST',
            url: '/api/Organization',
            data: data,
            success: function () {
                self.Initialize();
                self.CancelEditOrganization();
                alert('Successfully saved organization!');
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred while saving the organization');
            }
        });
    };

    self.CancelEditOrganization = function () {
        self.CurrentOrganizationId(null);
        self.NewOrganizationName('');
        $('#editOrganizationDialog').modal('hide');
    };

    self.DeleteOrganization = function (organization) {
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