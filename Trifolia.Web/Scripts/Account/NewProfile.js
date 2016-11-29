userProfile = function (redirectUrl) {
    var self = this;

    self.FirstName = ko.observable('');
    self.LastName = ko.observable('');
    self.Phone = ko.observable('');
    self.Email = ko.observable('');
    self.OkayToContact = ko.observable(true);
    self.Organization = ko.observable('');
    self.OrganizationType = ko.observable('');
    self.RedirectUrl = ko.observable(redirectUrl);

    self.validation = ko.validatedObservable({
        FirstName: self.FirstName.extend({ required: true, maxLength: 125 }),
        LastName: self.LastName.extend({ required: true, maxLength: 125 }),
        Phone: self.Phone.extend({ required: true, maxLength: 20 }),
        Email: self.Email.extend({ required: true, maxLength: 255, email: true }),
        Organization: self.Organization.extend({ maxLength: 50 })
    });

    self.isValid = ko.computed(function () {
        return self.validation.isValid();
    });
};

newProfileViewModel = function (redirectUrl) {
    var self = this;

    self.Model = new userProfile(redirectUrl);
    self.OrgTypes = [
        'Provider',
        'Regulator',
        'Vendor',
        'Other'
    ];
    self.enableReleaseAnnouncement = ko.observable(false);

    self.SaveChanges = function () {
        if (!self.Model.isValid()) {
            showError('Please fix errors before saving');
            return;
        }

        $('#mainBody').block('Saving changes...');

        var data = ko.toJS(self.Model);

        $.ajax({
            url: '/Account/CompleteProfile',
            type: 'POST',
            data: data,
            complete: function (jqXHR, textStatus) {
                $("#mainBody").unblock();

                if (textStatus != 'success') {
                    alert('There was an error saving your changes; please submit a support ticket');
                } else {
                    var responseRedirectUrl = JSON.parse(jqXHR.responseText);
                    location.href = responseRedirectUrl;
                }
            }
        });
    };

    // Initialization
    $.ajax({
        url: '/api/Config/ReleaseAnnouncement',
        success: function (enableReleaseAnnouncement) {
            self.enableReleaseAnnouncement(enableReleaseAnnouncement);
        },
        error: function (err) {
            console.log(err);
            alert('Error determining if release announcements are supported');
        }
    });
};