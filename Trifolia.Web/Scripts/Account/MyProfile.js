var userProfile = function () {
    var self = this;
    var mapping = {
        'include': [
            'firstName',
            'lastName',
            'phone',
            'email',
            'okayToContact',
            'organization',
            'organizationType',
            'authToken',
            'openIdConfigUrl']
    };

    self.userName = ko.observable('');

    self.firstName = ko.observable('');
    self.lastName = ko.observable('');
    self.phone = ko.observable('');
    self.email = ko.observable('');
    self.okayToContact = ko.observable(false);
    self.organization = ko.observable('');
    self.organizationType = ko.observable('');
    self.authToken = ko.observable('');
    self.openIdConfigUrl = ko.observable('');

    self.validation = ko.validatedObservable({
        firstName: self.firstName.extend({ required: true, maxLength: 125 }),
        lastName: self.lastName.extend({ required: true, maxLength: 125 }),
        phone: self.phone.extend({ required: true, maxLength: 20 }),
        email: self.email.extend({ required: true, maxLength: 255, email: true }),
        organization: self.organization.extend({ maxLength: 50 })
    });

    self.isValid = ko.computed(function () {
        return self.validation.isValid();
    });

    self.loadJs = function (data) {
        ko.mapping.fromJS(data, mapping, self);
    };
};

var myProfileViewModel = function (loadDataUrl, saveDataUrl) {
    var self = this;

    self.dataUrl = loadDataUrl;
    self.saveUrl = saveDataUrl;
    self.model = new userProfile();
    self.openIdConfig = ko.observable(null);
    self.orgTypes = [
        'Provider',
        'Regulator',
        'Vendor',
        'Other'
    ];
    self.releaseAnnouncementsSubscription = ko.observable(null);

    var validProfile = ko.validatedObservable(self.model);

    self.isValidProfile = ko.computed(function () {
        return validProfile.isValid();
    });

    self.getReleaseAnnouncementSubscription = function () {
        $.ajax({
            url: '/api/User/Me/ReleaseAnnouncement',
            success: self.releaseAnnouncementsSubscription,
            error: function (err) {
                console.log(err);
                alert('Error getting release announcement subscription status');
            }
        });
    };

    self.toggleReleaseAnnouncementSubscription = function () {
        $.ajax({
            url: '/api/User/Me/ReleaseAnnouncement',
            method: self.releaseAnnouncementsSubscription() == true ? 'DELETE' : 'POST',
            success: self.getReleaseAnnouncementSubscription,
            error: function (err) {
                alert('Error toggling release announcement subscription');
            }
        });
    };

    self.saveChanges = function () {
        if (!self.isValidProfile()) {
            alert('Please fix errors before saving');
            return;
        }

        $('#mainBody').block('Saving changes...');

        var data = ko.mapping.toJS(self.model);
        var stringData = JSON.stringify(data);

        $.ajax({
            url: self.saveUrl,
            type: 'POST',
            dataType: 'json',
            data: stringData,
            contentType: 'application/json; charset=utf-8',
            complete: function (jqXHR, textStatus) {
                $("#mainBody").unblock();

                if (textStatus != 'success') {
                    alert('There was an error saving your changes; please submit a support ticket');
                } else {
                    alert('Changes Saved Successfully');
                }
            },
            success: function (updatedModel) {
                self.model.loadJs(updatedModel);
                self.model.dirtyFlag = ko.dirtyFlag(self.model);
            }
        });
    };

    $.ajax({
        url: self.dataUrl,
        cache: false,
        contentType: 'application/json; charset=utf-8',
        success: function (aModel) {
            self.model.loadJs(aModel);
            self.model.dirtyFlag = ko.dirtyFlag(self.model);

            if (aModel.openIdConfigUrl) {
                $.ajax({
                    url: aModel.openIdConfigUrl,
                    success: function (results) {
                        self.openIdConfig(JSON.stringify(results, null, '\t'));
                    },
                    error: function (err) {
                        alert('Error getting open id configuration');
                    }
                });
            }
        }
    });

    self.getReleaseAnnouncementSubscription();
};