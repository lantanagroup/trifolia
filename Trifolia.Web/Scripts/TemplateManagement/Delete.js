var DeleteTemplateViewModel = function (templateId) {
    var self = this;

    self.Template = ko.observable();
    self.ReplaceTemplateId = ko.observable();

    self.Initialize = function () {
        $.ajax({
            url: '/api/Template/' + templateId,
            success: function (result) {
                ko.mapping.fromJS({ Template: result }, {}, self);
            }
        });
    };

    self.Cancel = function () {
        location.href = '/TemplateManagement/View/Id/' + templateId;
    };

    self.Delete = function () {
        if (!confirm("Are you absolutely sure you want to delete this template?")) {
            return;
        }

        var url = '/api/Template/' + templateId;

        if (self.ReplaceTemplateId()) {
            url = url + '?replaceTemplateId=' + self.ReplaceTemplateId();
        }

        $.blockUI({ message: 'Deleting...' });
        $.ajax({
            method: 'DELETE',
            url: url,
            success: function () {
                alert('Successfully deleted template.');
                location.href = '/TemplateManagement/List';
            },
            error: function (ex) {
                console.log(ex);
                alert("An error occurred processing the request...");
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Initialize();
};