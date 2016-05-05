var ViewTypesViewModel = function () {
    var self = this;

    self.Types = ko.observableArray([]);

    self.Initialize = function () {
        $.ajax({
            url: '/api/Type',
            success: function (results) {
                ko.mapping.fromJS({ Types: results }, {}, self);
            }
        });
    };

    self.Add = function () {
        location.href = '/IGTypeManagement/Edit';
    };

    self.Edit = function (igType) {
        location.href = '/IGTypeManagement/Edit/' + igType.Id();
    };

    self.Delete = function (igType) {
        if (igType.TemplateCount() > 0) {
            alert("Cannot delete an implementation guide type that is assocaited with templates. Move templates to a different implementation guide type first.");
            return;
        }

        if (!confirm("Are you sure you want to delete this implementation guide type?")) {
            return;
        }

        $.blockUI({ message: "Deleting implementation guide type..." });
        $.ajax({
            method: 'DELETE',
            url: '/api/Type/' + igType.Id(),
            success: function () {
                self.Initialize();
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Initialize();
};