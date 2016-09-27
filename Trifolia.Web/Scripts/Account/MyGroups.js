var MyGroupsViewModel = function () {
    var self = this;
    var mapping = {
        Groups: {
            create: function (options) {
                return new GroupModel(options.data);
            }
        },
        AllGroups: {
            create: function (options) {
                return new GroupModel(options.data);
            }
        }
    };

    self.Groups = ko.observableArray();
    self.AllGroups = ko.observableArray([]);
    self.CurrentGroup = ko.observable();

    self.DeleteGroup = function (groupId) {
        $.ajax({
            url: '/api/Group/' + groupId,
            method: 'DELETE',
            success: function () {
                var foundMyGroup = _.find(self.Groups(), function (group) {
                    return group.Id() == groupId;
                });
                var foundGroup = _.find(self.AllGroups(), function (group) {
                    return group.Id() == groupId;
                });

                if (foundMyGroup) {
                    var foundMyGroupIndex = self.Groups.indexOf(foundMyGroup);
                    self.Groups.splice(foundMyGroupIndex, 1);
                }

                if (foundGroup) {
                    var foundGroupIndex = self.AllGroups.indexOf(foundGroup);
                    self.AllGroups.splice(foundGroupIndex, 1);
                }
            },
            error: function (err) {
                alert('An error occurred while deleting the group');
            }
        });
    };

    self.JoinGroup = function (group) {
        if (group.Disclaimer()) {
            self.CurrentGroup(group);
            $('#GroupDisclaimerDialog').modal('show');
        } else {
            self.DoJoinGroup(group.Id());
        }
    };

    self.DoJoinGroup = function (groupId) {
        $.ajax({
            url: '/api/Group/' + groupId + '/User',
            method: 'POST',
            success: function (joined) {
                self.CurrentGroup(null);
                $('#GroupDisclaimerDialog').modal('hide');

                if (joined) {
                    var foundGroup = _.find(self.AllGroups(), function (group) {
                        return group.Id() == groupId;
                    });

                    var foundGroupIndex = self.AllGroups.indexOf(foundGroup);
                    self.AllGroups.splice(foundGroupIndex, 1);

                    self.Groups.push(foundGroup);
                } else {
                    alert('An email has been sent to the manager(s) of the group requesting to be added');
                }
            },
            error: function (err) {
                alert('An error ocurred while joining the group');
            }
        });
    };

    self.LeaveGroup = function (groupId) {
        if (!confirm('Are you sure you want to leave the group?')) {
            return;
        }

        $.ajax({
            url: '/api/Group/' + groupId + '/User',
            method: 'DELETE',
            success: function () {
                var foundGroup = _.find(self.Groups(), function (group) {
                    return group.Id() == groupId;
                });
                var foundGroupIndex = self.Groups.indexOf(foundGroup);

                // Move the group from current groups to available groups
                self.Groups.splice(foundGroupIndex, 1);
                self.AllGroups.push(foundGroup);
            },
            error: function (err) {
                alert('An error ocurred while leaving theg group');
            }
        });
    };

    self.CancelJoinGroup = function () {
        self.CurrentGroup(null);
        $('#GroupDisclaimerDialog').modal('hide');
    };

    self.ViewGroup = function (group) {
        self.CurrentGroup(group);
        $('#ViewGroupDialog').modal('show');
    };

    $.ajax({
        url: '/api/Group/My',
        success: function (groups) {
            groups = groups.sort(function (a, b) {
                var aName = a.Name ? a.Name : '';
                var bName = b.Name ? b.Name : '';

                return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
            });

            ko.mapping.fromJS({ Groups: groups }, mapping, self);
        },
        error: function (err) {
            alert('An error ocurred while getting your groups');
        }
    });

    $.ajax({
        url: '/api/Group?onlyNotMember=true',
        success: function (groups) {
            groups = groups.sort(function (a, b) {
                var aName = a.Name ? a.Name : '';
                var bName = b.Name ? b.Name : '';

                return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
            });

            ko.mapping.fromJS({ AllGroups: groups }, mapping, self);
        },
        error: function (err) {
            alert('An error ocurred while getting all groups');
        }
    });
};

var GroupModel = function (data) {
    var self = this;

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();
    self.Disclaimer = ko.observable();
    self.IsManager = ko.observable();

    self.ViewDisabled = ko.computed(function () {
        return !self.Description() && !self.Disclaimer();
    });

    ko.mapping.fromJS(data, {}, self);
};