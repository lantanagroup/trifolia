userProfileMap = {
    'include': ['firstName', 'lastName', 'phone', 'email', 'okayToContact', 'organization', 'organizationType']
};

userProfile = function () {
    var self = this;

    self.userName = ko.observable('');
    self.accountOrganization = ko.observable('');

    self.firstName = ko.observable('');
    self.lastName = ko.observable('');
    self.phone = ko.observable('');
    self.email = ko.observable('');
    self.okayToContact = ko.observable(false);
    self.organization = ko.observable('');
    self.organizationType = ko.observable('');
    self.apiKey = ko.observable('');

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
};

myProfileViewModel = function (loadDataUrl, saveDataUrl) {
    var self = this;

    self.dataUrl = loadDataUrl;
    self.saveUrl = saveDataUrl;
    self.model = new userProfile();
    self.authorizationHeader = ko.observable('');
    self.orgTypes = [
        'Provider',
        'Regulator',
        'Vendor',
        'Other'
    ];

    var validProfile = ko.validatedObservable(self.model);

    self.isValidProfile = ko.computed(function () {
        return validProfile.isValid();
    });
    
    $.ajax({
        url: self.dataUrl,
        cache: false,
        contentType: 'application/json; charset=utf-8',
        complete: function (jqXHR, textStatus) {
            var aModel = JSON.parse(jqXHR.responseText);
            ko.mapping.fromJS(aModel, userProfileMap, self.model);
            self.model.dirtyFlag = ko.dirtyFlag(self.model);
        }
    });

    self.generateAuthorizationHeader = function () {
        var timestamp = Date.now();
        var salt = Math.random();
        var properties = self.model.userName() + '|' + self.model.accountOrganization() + '|' + timestamp + '|' + salt + '|';
        var hashContent = properties + self.model.apiKey();
        var authBasicValue = properties + CryptoJS.SHA1(hashContent).toString(CryptoJS.enc.Base64);
        var b64AuthBasicValue = btoa(authBasicValue.toString(CryptoJS.enc.Base64));
        
        self.authorizationHeader('Bearer ' + b64AuthBasicValue);

        setTimeout(300000, function () {
            self.authorizationHeader('');
        });
    };

    self.generateApiKey = function () {
        var s4 = function () {
            return Math.floor((1 + Math.random()) * 0x10000)
              .toString(16)
              .substring(1);
        }
        var guid = s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();

        self.model.apiKey(guid);
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
                ko.mapping.fromJS(updatedModel, userProfileMap, self.model);
                self.model.dirtyFlag = ko.dirtyFlag(self.model);
            }
        });
    };
};