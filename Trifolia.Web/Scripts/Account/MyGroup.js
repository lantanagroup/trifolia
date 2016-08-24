var MyGroupViewModel = function (groupId) {
    var self = this;
    var isAddingManager = false;

    self.GroupId = ko.observable(groupId);
    self.Group = ko.observable(new MyGroupModel());
    self.Managers = ko.observableArray([]);
    self.Members = ko.observableArray([]);
    self.SelectedUserIds = ko.observableArray([]);
    self.SearchQuery = ko.observable();
    self.SearchResults = ko.observableArray();
    self.CurrentUser = ko.observable();

    self.AddManager = function () {
        isAddingManager = true;
        $('#AddUserDialog').modal('show');
    };

    self.AddMember = function () {
        isAddingManager = false;
        $('#AddUserDialog').modal('show');
    };

    self.AddUserOk = function () {
        var addUser = function (userId) {
            var deferred = Q.defer();
            var url = '/api/Group/' + self.GroupId() + '/Member/' + userId;

            if (isAddingManager) {
                url = '/api/Group/' + self.GroupId() + '/Manager/' + userId;
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
                var list = isAddingManager ? self.Managers : self.Members;

                _.each(results, function (result) {
                    var foundUser = _.find(list(), function (user) {
                        return user.Id == result.Id;
                    })

                    if (!foundUser) {
                        list.push(result);
                    }
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
        self.SelectedUserId(null);
        $('#AddUserDialog').modal('hide');
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

    self.RemoveUser = function (userId, isManager) {
        var url = '/api/Group/' + self.GroupId() + '/Member/' + userId;

        if (isManager) {
            url = '/api/Group/' + self.GroupId() + '/Manager/' + userId;
        }

        $.ajax({
            url: url,
            method: 'DELETE',
            success: function () {
                var list = isManager ? self.Managers : self.Members;
                var foundUser = _.find(list(), function (user) {
                    return user.Id == userId;
                });

                if (foundUser) {
                    var index = list().indexOf(foundUser);
                    list.splice(index, 1);
                }
            },
            error: function (err) {
                alert('An error ocurred while removing the manager/member from the group');
            }
        });
    };

    var watchFields = function () {
        self.Group().Name.subscribe(self.SaveChanges);
        self.Group().Description.subscribe(self.SaveChanges);
        self.Group().Disclaimer.subscribe(self.SaveChanges);
        self.Group().IsOpen.subscribe(self.SaveChanges);
    };

    self.SaveChanges = function () {
        var isNew = !self.GroupId();
        var url = '/api/Group/My';
        var group = self.Group().toJS();

        if (groupId) {
            url += '/' + groupId;
        }

        $.ajax({
            url: url,
            method: isNew ? 'POST' : 'PUT',
            data: group,
            success: function (results) {
                if (isNew) {
                    location.href = '/Account/Group/' + results.Id;
                }
            },
            error: function (err) {
                alert('Error saving changes to group');
            }
        });

        if (isNew) {
            watchFields();
        }
    };

    if (groupId) {
        $.ajax({
            url: '/api/Group/My/' + groupId,
            success: function (group) {
                ko.mapping.fromJS(group, null, self.Group());

                watchFields();
            },
            error: function (err) {
                alert('An error ocurred while getting your group');
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

        $.ajax({
            url: '/api/Auth/WhoAmI',
            success: function (userModel) {
                self.CurrentUser(userModel);
            },
            error: function (err) {
                alert('An error occurred while getting the current user');
            }
        });
    }
};

var MyGroupModel = function (data) {
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
        Name: self.Name.extend({ required: true})
    });

    ko.mapping.fromJS(data, mapping, this);

    self.toJS = function () {
        return ko.mapping.toJS(self, mapping);
    };
};