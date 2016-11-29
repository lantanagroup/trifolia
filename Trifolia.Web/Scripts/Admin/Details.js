var OrganizationEditModel = function (organizationId) {
    var self = this;

    self.Model = ko.observable(new OrganizationModel(null, self));
    self.OriginalCurrentUser = ko.observable();
    self.CurrentUser = ko.observable();
    self.OriginalCurrentGroup = ko.observable();
    self.CurrentGroup = ko.observable();
    self.Roles = ko.observableArray([]);

    self.GetGroupName = function (groupId) {
        var foundGroup = ko.utils.arrayFirst(self.Model().Groups(), function (group) {
            return group.Id() == groupId;
        });

        if (!foundGroup) {
            return '';
        }

        return foundGroup.Name();
    };

    self.GetRoleName = function (roleId) {
        var foundRole = ko.utils.arrayFirst(self.Roles(), function (role) {
            return role.Id() == roleId;
        });

        if (!foundRole) {
            return '';
        }

        return foundRole.Name();
    };

    self.GetUsers = function () {
        if (!self.Model() || !self.Model().Users()) {
            return [];
        }

        return self.Model().Users.sort(function (left, right) {
            var leftName = left.FullName() ? left.FullName().toLowerCase() : '';
            var rightName = right.FullName() ? right.FullName().toLowerCase() : '';

            return leftName > rightName ? 1 : (leftName < rightName ? -1 : 0);
        });
    };

    self.GetGroups = function () {
        if (!self.Model() || !self.Model().Groups()) {
            return [];
        }

        return self.Model().Groups.sort(function (left, right) {
            var leftName = left.Name() ? left.Name().toLowerCase() : '';
            var rightName = right.Name() ? right.Name().toLowerCase() : '';

            return leftName > rightName ? 1 : (leftName < rightName ? -1 : 0);
        });
    };

    self.EditUser = function (user) {
        self.OriginalCurrentUser(user);
        
        if (user) {
            self.CurrentUser(new OrganizationUserModel(ko.mapping.toJS(user), self));
        } else {
            self.CurrentUser(new OrganizationUserModel(null, self));
        }

        $('#editUserDialog').modal('show');
    };

    self.SaveUser = function () {
        $.ajax({
            method: 'POST',
            url: '/api/Organization/' + self.Model().Id() + '/User',
            data: ko.mapping.toJS(self.CurrentUser()),
            success: function (result) {
                var updatedUser = new OrganizationUserModel(result, self);

                if (self.OriginalCurrentUser()) {
                    var index = self.Model().Users.indexOf(self.OriginalCurrentUser());
                    self.Model().Users.splice(index, 1, updatedUser);
                } else {
                    self.Model().Users.push(updatedUser);
                }

                self.CancelEditUser();
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while saving the user");
            }
        });
    };

    self.CancelEditUser = function () {
        self.OriginalCurrentUser(null);
        self.CurrentUser(null);
        $('#editUserDialog').modal('hide');
    };

    self.DeleteUser = function (user) {
        if (!confirm("Are you sure you want to delete this user?")) {
            return;
        };

        $.ajax({
            method: 'DELETE',
            url: '/api/Organization/' + self.Model().Id() + '/User/' + user.Id(),
            success: function () {
                self.Model().Users.remove(user);
                alert('Successfully deleted user');
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while deleting the user");
            }
        });
    };

    self.EditUserGroups = function (user) {
        self.CurrentUser(user);
        $('#editUserGroupsDialog').modal('show');
    };

    self.CancelEditUserGroups = function () {
        self.CurrentUser(null);
        $('#editUserGroupsDialog').modal('hide');
    };

    self.GetUserAvailableGroups = ko.computed(function () {
        if (!self.CurrentUser()) {
            return [];
        }

        return ko.utils.arrayFilter(self.Model().Groups(), function (group) {
            var foundGroup = ko.utils.arrayFirst(self.CurrentUser().Groups(), function (currentGroup) {
                return group.Id() == currentGroup;
            });

            return !foundGroup;
        });
    });

    self.AssignGroup = function (groupId) {
        $.ajax({
            method: 'POST',
            url: '/api/Organization/' + self.Model().Id() + '/User/' + self.CurrentUser().Id() + '/Group/' + groupId,
            success: function () {
                self.CurrentUser().Groups.push(groupId);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while assigning the user to the group");
            }
        });
    };

    self.UnassignGroup = function (groupId) {
        $.ajax({
            method: 'DELETE',
            url: '/api/Organization/' + self.Model().Id() + '/User/' + self.CurrentUser().Id() + '/Group/' + groupId,
            success: function () {
                self.CurrentUser().Groups.remove(groupId);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while assigning the user to the group");
            }
        });
    };

    self.EditUserRoles = function (user) {
        self.CurrentUser(user);
        $('#editUserRolesDialog').modal('show');
    };

    self.CancelEditUserRoles = function () {
        self.CurrentUser(null);
        $('#editUserRolesDialog').modal('hide');
    };

    self.GetUserAvailableRoles = ko.computed(function () {
        if (!self.CurrentUser()) {
            return [];
        }

        return ko.utils.arrayFilter(self.Roles(), function (role) {
            var foundRole = ko.utils.arrayFirst(self.CurrentUser().Roles(), function (currentRole) {
                return role.Id() == currentRole;
            });

            return !foundRole;
        });
    });

    self.AssignRole = function (roleId) {
        $.ajax({
            method: 'POST',
            url: '/api/Organization/' + self.Model().Id() + '/User/' + self.CurrentUser().Id() + '/Role/' + roleId,
            success: function () {
                self.CurrentUser().Roles.push(roleId);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while assigning the user to the role");
            }
        });
    };

    self.UnassignRole = function (roleId) {
        $.ajax({
            method: 'DELETE',
            url: '/api/Organization/' + self.Model().Id() + '/User/' + self.CurrentUser().Id() + '/Role/' + roleId,
            success: function () {
                self.CurrentUser().Roles.remove(roleId);
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while assigning the user to the role");
            }
        });
    };

    self.EditGroup = function (group) {
        self.OriginalCurrentGroup(group);

        if (group) {
            self.CurrentGroup(new OrganizationGroupModel(ko.mapping.toJS(group)));
        } else {
            self.CurrentGroup(new OrganizationGroupModel());
        }

        $('#editGroupDialog').modal('show');
    };

    self.SaveGroup = function () {
        $.ajax({
            method: 'POST',
            url: '/api/Organization/' + self.Model().Id() + '/Group',
            data: ko.mapping.toJS(self.CurrentGroup()),
            success: function (groupId) {
                self.CurrentGroup().Id(groupId);

                if (self.OriginalCurrentGroup()) {
                    var index = self.Model().Groups.indexOf(self.OriginalCurrentGroup());
                    self.Model().Groups.splice(index, 1, self.CurrentGroup());
                } else {
                    self.Model().Groups.push(self.CurrentGroup());
                }

                self.CancelEditGroup();
                alert("Successfully saved group");
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while saving the group");
            }
        });
    };

    self.CancelEditGroup = function () {
        self.OriginalCurrentGroup(null);
        self.CurrentGroup(null);
        $('#editGroupDialog').modal('hide');
    };

    self.DeleteGroup = function (group) {
        if (!confirm("Are you sure you want to delete this group?")) {
            return;
        };

        $.ajax({
            method: 'DELETE',
            url: '/api/Organization/' + self.Model().Id() + '/Group/' + group.Id(),
            success: function () {
                self.Model().Groups.remove(group);

                ko.utils.arrayForEach(self.Model().Users(), function (user) {
                    var foundGroup = ko.utils.arrayFirst(user.Groups(), function (groupId) {
                        return groupId == group.Id();
                    });

                    if (foundGroup) {
                        user.Groups.remove(group.Id());
                    }
                });

                alert('Successfully deleted group');
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred while deleting the group");
            }
        });
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Organization/' + organizationId + '/Role',
            cache: false,
            success: function (results) {
                ko.mapping.fromJS({ Roles: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Organization/' + organizationId,
            cache: false,
            success: function (results) {
                var model = new OrganizationModel(results, self);
                self.Model(model);
            },
            error: function (ex) {
                console.log(ex);
                alert('An error occurred getting the organization information');
            }
        });
    };

    self.Initialize();
};

var OrganizationModel = function (data, organizationEditModel) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'Users', 'Groups'],
        'Groups': {
            create: function (options) {
                return new OrganizationGroupModel(options.data);
            }
        },
        'Users': {
            create: function (options) {
                return new OrganizationUserModel(options.data, organizationEditModel);
            }
        }
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Users = ko.observableArray([]);
    self.Groups = ko.observableArray([]);

    ko.mapping.fromJS(data, mapping, self);
};

var OrganizationUserModel = function (data, organizationEditModel) {
    var self = this;
    var mapping = {
        include: ['Id', 'UserName', 'Email', 'FirstName', 'LastName', 'Phone', 'Email', 'Roles', 'Groups']
    };

    self.Id = ko.observable();
    self.UserName = ko.observable();
    self.Email = ko.observable();
    self.FirstName = ko.observable();
    self.LastName = ko.observable();
    self.Phone = ko.observable();
    self.Email = ko.observable();
    self.Roles = ko.observableArray([]);
    self.Groups = ko.observableArray([]);

    self.FullName = ko.computed(function () {
        return self.LastName() + ', ' + self.FirstName();
    });

    ko.mapping.fromJS(data, mapping, self);

    var validation = ko.validatedObservable({
        UserName: self.UserName.extend({
            required: true,
            maxLength: 255,
            uniqueUserName: {
                CurrentUser: self,
                OrganizationEditModel: organizationEditModel
            }
        }),
        FirstName: self.FirstName.extend({ required: true, maxLength: 255 }),
        LastName: self.LastName.extend({ required: true, maxLength: 255 }),
        Email: self.Email.extend({ required: true, maxLength: 255, email: true }),
        Phone: self.Phone.extend({ required: true, maxLength: 255 })
    });

    self.IsValid = ko.computed(function () {
        return validation().isValid();
    });
};

var OrganizationGroupModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name']
    };

    self.Id = ko.observable();
    self.Name = ko.observable();

    ko.mapping.fromJS(data, mapping, self);

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, maxLength: 255 })
    });

    self.IsValid = ko.computed(function () {
        return validation().isValid();
    });
};

ko.validation.rules['uniqueUserName'] = {
    validator: function (val, otherVal) {
        var currentUser = otherVal.CurrentUser;
        var organizationEditModel = otherVal.OrganizationEditModel;

        if (!organizationEditModel.Model()) {
            return false;
        }

        var foundUser = ko.utils.arrayFirst(organizationEditModel.Model().Users(), function (user) {
            return user.Id() != currentUser.Id() && user.UserName() == val;
        });

        return !foundUser;
    },
    message: 'This username is already in use.'
};
ko.validation.registerExtenders();