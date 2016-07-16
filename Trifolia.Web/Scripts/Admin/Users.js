var UsersViewModel = function () {
    var self = this, groups = [], roles = [];
    var mapping = {
        Users: {
            create: function (options) {
                return new UserModel(options.data);
            }
        }
    };

    self.Users = ko.observableArray();
    self.CurrentUser = ko.observable();
    self.SearchQuery = ko.observable();

    self.ClearCurrentUser = function () {
        self.CurrentUser(null);
    };

    self.EditUser = function (user) {
        self.CurrentUser(user);
        $('#EditUserModal').modal('show');
    };

    self.EditUserGroups = function (user) {
        self.CurrentUser(user);
        $('#EditUserGroupsModal').modal('show');
    };

    self.EditUserRoles = function (user) {
        self.CurrentUser(user);
        $('#EditUserRolesModal').modal('show');
    };

    self.GetFilteredUsers = ko.computed(function () {
        if (!self.SearchQuery()) {
            return self.Users();
        }

        var filteredUsers = _.filter(self.Users(), function (user) {
            var searchQueryRegex = new RegExp(self.SearchQuery(), 'gi');
            var userName = user.UserName() ? user.UserName() : '';
            var firstName = user.FirstName() ? user.FirstName() : '';
            var lastName = user.LastName() ? user.LastName() : '';
            var email = user.Email() ? user.Email() : '';

            return firstName.match(searchQueryRegex) || lastName.match(searchQueryRegex) || userName.match(searchQueryRegex) || email.match(searchQueryRegex);
        });

        return filteredUsers;
    });

    self.RemoveUser = function (user) {

    };

    self.AssignRole = function (user, roleId) {
        $.ajax({
            method: 'POST',
            url: '/api/User/' + user.Id() + '/Role/' + roleId,
            success: function () {
                user.Roles.push(roleId);
            }
        });
    };

    self.UnassignRole = function (user, roleId) {
        $.ajax({
            method: 'DELETE',
            url: '/api/User/' + user.Id() + '/Role/' + roleId,
            success: function () {
                var roleIndex = user.Roles.indexOf(roleId);
                user.Roles.splice(roleIndex, 1);
            }
        });
    };

    self.AssignGroup = function (user, groupId) {
        $.ajax({
            method: 'POST',
            url: '/api/User/' + user.Id() + '/Group/' + groupId,
            success: function () {
                user.Groups.push(groupId);
            }
        });
    };

    self.UnassignGroup = function (user, groupId) {
        $.ajax({
            method: 'DELETE',
            url: '/api/User/' + user.Id() + '/Group/' + groupId,
            success: function () {
                var groupIndex = user.Groups.indexOf(groupId);
                user.Groups.splice(groupIndex, 1);
            }
        });
    };

    self.GetGroupName = function (groupId) {
        return _.find(groups, function (group) {
            return group.Id == groupId;
        }).Name;
    };

    self.GetRoleName = function (roleId) {
        return _.find(roles, function (role) {
            return role.Id == roleId;
        }).Name;
    };

    self.GetUnassignedRoles = function () {
        if (!self.CurrentUser()) {
            return [];
        }

        var unassignedRoles = _.filter(roles, function (role) {
            return self.CurrentUser().Roles().indexOf(role.Id) < 0;
        });

        return unassignedRoles;
    };

    self.GetUnassignedGroups = function () {
        if (!self.CurrentUser()) {
            return [];
        }

        var unassignedGroups = _.filter(groups, function (group) {
            return self.CurrentUser().Groups().indexOf(group.Id) < 0;
        });

        return unassignedGroups;
    };

    $.ajax({
        url: '/api/User',
        success: function (results) {
            ko.mapping.fromJS({ Users: results }, mapping, self);
        }
    });

    $.ajax({
        url: '/api/Group',
        success: function (results) {
            groups = results;
        }
    });

    $.ajax({
        url: '/api/Role',
        success: function (results) {
            roles = results.Roles;
        }
    });
};

var UserModel = function (data) {
    var self = this;

    self.Id = ko.observable();
    self.UserName = ko.observable();
    self.FirstName = ko.observable();
    self.LastName = ko.observable();
    self.Email = ko.observable();
    self.Phone = ko.observable();

    self.Roles = ko.observableArray();
    self.Groups = ko.observableArray();

    ko.mapping.fromJS(data, null, self);
};