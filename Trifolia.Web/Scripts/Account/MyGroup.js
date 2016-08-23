var MyGroupViewModel = function (groupId) {
    var self = this;

    self.GroupId = ko.observable(groupId);
    self.Group = ko.observable(new MyGroupModel());
    self.SelectedUserIds = ko.observableArray([]);
    self.SearchQuery = ko.observable();
    self.SearchResults = ko.observableArray();
    self.IsAddingManager = false;
    self.CurrentUser = ko.observable();

    self.AddManager = function () {
        self.IsAddingManager = true;
        $('#AddUserDialog').modal('show');
    };

    self.AddMember = function () {
        self.IsAddingManager = false;
        $('#AddUserDialog').modal('show');
    };

    self.AddUserOk = function () {
        var addUser = function (userId) {
            var deferred = Q.defer();
            var url = '/api/Group/My/' + self.GroupId() + '/User/' + userId;

            if (self.IsAddingManager) {
                url += '?isManager=true';
            }

            $.ajax({
                url: url,
                method: 'POST',
                success: function (result) {
                    var user = ko.mapping.fromJS(result);
                    deferred.resolve(user);
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
                var list = self.IsAddingManager ? self.Group().Managers : self.Group().Members;

                _.each(results, function (result) {
                    list.push(result);
                });

                list = list.sort(function (a, b) {
                    return a.Name() == b.Name() ? 0 : (a.Name() < b.Name() ? -1 : 1);
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
        var url = '/api/Group/My/' + self.GroupId() + '/User/' + userId;

        if (isManager) {
            url += '?isManager=true';
        }

        $.ajax({
            url: url,
            method: 'DELETE',
            success: function () {
                var list = isManager ? self.Group().Managers : self.Group().Members;
                var foundUser = _.find(list(), function (user) {
                    return user.Id() == userId;
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
        include: ['Id', 'Name', 'Description', 'Disclaimer', 'IsOpen', 'Managers', 'Members']
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Description = ko.observable();
    self.Disclaimer = ko.observable();
    self.IsOpen = ko.observable(false);
    self.Managers = ko.observableArray();
    self.Members = ko.observableArray();

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true})
    });

    ko.mapping.fromJS(data, mapping, this);

    self.toJS = function () {
        return ko.mapping.toJS(self, mapping);
    };
};