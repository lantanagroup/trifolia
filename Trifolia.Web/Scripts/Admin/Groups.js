var GroupsViewModel = function () {
    var self = this;

    self.Groups = ko.observableArray();

    self.DeleteGroup = function (group) {
        if (!confirm('Are you sure you want to delete the group?')) {
            return;
        }

        $.ajax({
            url: '/api/Group/' + group.Id,
            method: 'DELETE',
            success: function () {
                var groupIndex = self.Groups.indexOf(group);
                self.Groups.splice(groupIndex, 1);
            },
            error: function (err) {
                alert('An error occurred while deleting the group');
            }
        });
    };

    $.ajax({
        url: '/api/Group',
        success: function (groups) {
            groups = groups.sort(function (a, b) {
                var aName = a.Name ? a.Name : '';
                var bName = b.Name ? b.Name : '';

                return aName.toLowerCase() == bName.toLowerCase() ? 0 : (aName.toLowerCase() < bName.toLowerCase() ? -1 : 1);
            });

            self.Groups(groups);
        },
        error: function (err) {
            alert('An error occurred while getting the groups');
        }
    });
};