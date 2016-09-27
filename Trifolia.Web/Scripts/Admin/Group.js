var GroupViewModel = function (groupId) {
    var self = this;
    var newUserIsManager = false;

    self.Group = ko.observable();
    self.Managers = ko.observableArray([]);
    self.Members = ko.observableArray([]);
    self.SearchQuery = ko.observable('');
    self.SelectedUserIds = ko.observableArray([]);
    self.SearchResults = ko.observableArray([]);

    self.SaveGroup = function () {
        var group = self.Group().toJS();

        $.ajax({
            url: '/api/Group',
            method: 'POST',
            data: group,
            error: function (err) {
                alert('An error ocurred while saving changes');
            }
        });
    };

    self.AddManager = function () {
        newUserIsManager = true;
        $('#AddUserDialog').modal('show');
    };

    self.AddMember = function () {
        newUserIsManager = false;
        $('#AddUserDialog').modal('show');
    };

    self.AddUserOk = function () {
        var addUser = function (userId) {
            var deferred = Q.defer();
            var url = '/api/Group/' + groupId + '/Member/' + userId;

            if (newUserIsManager) {
                url = '/api/Group/' + groupId + '/Manager/' + userId;
            }

            $.ajax({
                url: url,
                method: 'POST',
                success: function (result) {
                    deferred.resolve(result);
                },
                error: function (err) {
                    deferred.reject();
                }
            });

            return deferred.promise;
        };

        var promises = [];

        for (var i in self.SelectedUserIds()) {
            promises.push(addUser(self.SelectedUserIds()[i]));
        }

        Q.all(promises)
            .then(function (results) {
                var list = newUserIsManager ? self.Managers : self.Members;

                _.each(results, function (result) {
                    var foundUser = _.find(list(), function (user) {
                        return user.Id == result.Id;
                    });

                    if (!foundUser)
                        list.push(result);
                });

                list = list.sort(function (a, b) {
                    var aName = a.Name ? a.Name : '';
                    var bName = b.Name ? b.Name : '';

                    return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
                });

                $('#AddUserDialog').modal('hide');
            })
            .catch(function () {
                alert('Error adding managers/members to group');
            });
    };

    self.AddUserCancel = function () {
        newUserIsManager = false;
        self.SearchResults([]);
        $('#AddUserDialog').hide();
    }

    self.RemoveUser = function (userId, isManager) {
        if (!confirm('Are you sure you want to remove this user?')) {
            return;
        }
        
        var url = '/api/Group/' + groupId + '/Member/' + userId;

        if (isManager) {
            url = '/api/Group/' + groupId + '/Manager/' + userId;
        }

        $.ajax({
            url: url,
            method: 'DELETE',
            success: function () {
                var list = isManager ? self.Managers : self.Members;
                var foundUser = _.find(list(), function (user) {
                    return user.Id == userId;
                });
                var foundUserIndex = list.indexOf(foundUser);
                list.splice(foundUserIndex, 1);
            },
            error: function (err) {
                alert('An error ocurred while deleting the manager/member');
            }
        });
    };

    self.SearchUsers = function () {
        self.SelectedUserIds([]);

        $.ajax({
            url: '/api/User/Search?searchText=' + encodeURIComponent(self.SearchQuery()),
            success: function (results) {
                self.SearchResults(results);
            },
            error: function (err) {
                alert('An error occurred while searching for users');
            }
        });
    };

    $.ajax({
        url: '/api/Group/' + groupId,
        success: function (results) {
            var group = new GroupModel(results);
            self.Group(group);

            group.Name.subscribe(self.SaveGroup);
            group.Description.subscribe(self.SaveGroup);
            group.Disclaimer.subscribe(self.SaveGroup);
            group.IsOpen.subscribe(self.SaveGroup);
        },
        error: function (err) {
            alert('An error occurred while getting the group');
        }
    });

    $.ajax({
        url: '/api/Group/' + groupId + '/Manager',
        success: function (results) {
            results = results.sort(function (a, b) {
                var aName = a.Name ? a.Name : '';
                var bName = b.Name ? b.Name : '';

                return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
            });

            self.Managers(results);
        },
        error: function (err) {
            alert('An error occurred while getting the managers for this group');
        }
    });

    $.ajax({
        url: '/api/Group/' + groupId + '/Member',
        success: function (results) {
            results = results.sort(function (a, b) {
                var aName = a.Name ? a.Name : '';
                var bName = b.Name ? b.Name : '';

                return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
            });

            self.Members(results);
        },
        error: function (err) {
            alert('An error occurred while getting the members for this group');
        }
    });
};

var GroupModel = function (data) {
    var self = this;
    var mapping = {
        include: ['Id', 'Name', 'Description', 'Disclaimer', 'IsOpen']
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();
    self.Disclaimer = ko.observable();
    self.IsOpen = ko.observable(false);

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true })
    });

    ko.mapping.fromJS(data, mapping, this);

    self.toJS = function () {
        return ko.mapping.toJS(self, mapping);
    };
};