var RolesViewModel = function () {
    var self = this;

    self.Model = ko.observable();
    self.CurrentRole = ko.observable();
    self.SelectedAvailableSecurables = ko.observableArray([]);
    self.SelectedAssignedSecurables = ko.observableArray([]);
    self.AddRoleName = ko.observable('');
    self.Organizations = ko.observableArray([]);

    self.AddRoleValidation = ko.validatedObservable({
        AddRoleName: self.AddRoleName.extend({ required: true, maxLength: 255 })
    });

    self.RestrictOrganization = function(organization) {
        $.ajax({
            method: 'POST',
            url: '/api/Role/' + self.CurrentRole().Id() + '/Restrict/' + organization.Id(),
            success: function() {
                organization.Restricted(true);
            },
            error: function(ex) {
                console.log(ex);
                alert("An error occurred while restricting the organization");
            }
        });
    };

    self.UnrestrictOrganization = function (organization) {
        $.ajax({
            method: 'POST',
            url: '/api/Role/' + self.CurrentRole().Id() + '/Unrestrict/' + organization.Id(),
            success: function () {
                organization.Restricted(false);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while un-restricting the organization");
            }
        });
    };

    self.AssignSecurable = function (securable) {
        $.ajax({
            method: 'POST',
            url: '/api/Role/' + self.CurrentRole().Id() + '/Assign/' + securable.Id(),
            success: function () {
                var newSecurable = new SecurableModel(ko.mapping.toJS(securable));
                self.CurrentRole().AssignedSecurables.push(newSecurable);
            }
        });
    };

    self.UnassignSecurable = function (securable) {
        $.ajax({
            method: 'POST',
            url: '/api/Role/' + self.CurrentRole().Id() + '/Unassign/' + securable.Id(),
            success: function () {
                self.CurrentRole().AssignedSecurables.remove(securable);
            }
        });
    };

    self.GetUnrestrictedOrganizations = function () {
        return ko.utils.arrayFilter(self.CurrentRole().Organizations(), function (org) {
            return !org.Restricted();
        });
    };

    self.GetRestrictedOrganizations = function () {
        return ko.utils.arrayFilter(self.CurrentRole().Organizations(), function (org) {
            return org.Restricted();
        });
    };

    self.EditRoleRestrictions = function (role) {
        self.CurrentRole(role);
        $('#editRoleRestrictionsDialog').modal('show');
    };

    self.CloseEditRoleRestrictions = function (role) {
        self.CurrentRole(null);
        $('#editRoleRestrictionsDialog').modal('hide');
    };

    self.GetAvailableSecurables = function () {
        var securables = ko.utils.arrayFilter(self.Model().AllSecurables(), function (securable) {
            var foundSecurable = ko.utils.arrayFirst(self.CurrentRole().AssignedSecurables(), function (currentSecurable) {
                return currentSecurable.Id() == securable.Id();
            });

            return !foundSecurable;
        });

        return securables.sort(function(left, right) {
            return left.Name() > right.Name() ? 1 : (left.Name() < right.Name() ? -1 : 0);
        });
    };

    self.GetAssignedSecurables = ko.computed(function () {
        if (!self.CurrentRole()) {
            return [];
        }

        return self.CurrentRole().AssignedSecurables().sort(function (left, right) {
            return left.Name() > right.Name() ? 1 : (left.Name() < right.Name() ? -1 : 0);
        });
    });

    self.ShowAddRole = function () {
        self.AddRoleName('');
        $('#addRoleDialog').modal('show');
    };

    self.AddRole = function () {
        $.ajax({
            method: 'POST',
            url: '/api/Role/Add?roleName=' + encodeURIComponent(self.AddRoleName()),
            success: function (results) {
                var model = new RoleItemModel(results);

                // Initialize the organizations within the role
                ko.utils.arrayForEach(self.Organizations(), function (org) {
                    var roleOrganization = new RoleOrganization(ko.mapping.toJS(org));
                    model.Organizations.push(roleOrganization);
                });

                self.Model().Roles.push(model);

                self.AddRoleName('');
                $('#addRoleDialog').modal('hide');
            }
        });
    };

    self.EditRoleSecurables = function (role) {
        self.CurrentRole(role);
        $('#editRoleSecurablesDialog').modal('show');
    };

    self.CloseEditRoleSecurables = function () {
        self.CurrentRole(null);
        $('#editRoleSecurablesDialog').modal('hide');
    };

    self.DefaultRoleChanged = function (newRoleId) {
        $.ajax({
            method: 'POST',
            url: '/api/Role/' + newRoleId + '/SetDefault',
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while updating the default role");
            }
        });
    };

    self.RemoveRole = function (role) {
        if (!confirm("Are you sure you want to remove this role?")) {
            return;
        }

        $.ajax({
            method: 'DELETE',
            url: '/api/Role/' + role.Id(),
            success: function () {
                self.Model().Roles.remove(role);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred deleting the role.");
            }
        });
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Organization',
            cache: false,
            success: function (results) {
                ko.mapping.fromJS({ Organizations: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Role',
            cache: false,
            success: function (results) {
                var model = new RolesModel(results);
                self.Model(model);

                model.DefaultRoleId.subscribe(self.DefaultRoleChanged);
            }
        });
    };

    self.Initialize();
};

var RolesModel = function(data) {
    var self = this;
    var mapping = {
        include: [ 'DefaultRoleId', 'Roles', 'AllSecurables' ],
        Roles: {
            create: function(options) {
                return new RoleItemModel(options.data);
            }
        },
        'AllSecurables': {
            create: function(options) {
                return new SecurableModel(options.data);
            }
        }
    };

    self.DefaultRoleId = ko.observable();
    self.Roles = ko.observableArray([]);
    self.AllSecurables = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var RoleItemModel = function(data) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Name', 'AssignedSecurables', 'Organizations' ],
        'AssignedSecurables': {
            create: function(options) {
                return new SecurableModel(options.data);
            }
        },
        'Organizations': {
            create: function (options) {
                return new RoleOrganization(options.data);
            }
        }
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.AssignedSecurables = ko.observableArray([]);
    self.Organizations = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var SecurableModel = function(data) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Key', 'Name', 'Description' ]
    };

    self.Id = ko.observable();
    self.Key = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();

    ko.mapping.fromJS(data, mapping, self);
};

var RoleOrganization = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'Restricted']
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Restricted = ko.observable(false);

    ko.mapping.fromJS(data, mapping, self);
};