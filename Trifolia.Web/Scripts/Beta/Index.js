var BetaRegistrationViewModel = function () {
    var self = this;

    self.Name = ko.observable();
    self.Company = ko.observable();
    self.Email = ko.observable();
    self.Phone = ko.observable();
    self.ContactPreference = ko.observable();

    self.Validation = ko.validatedObservable({
        Name: self.Name.extend({ required: true, maxLength: 255 }),
        Company: self.Company.extend({ required: true, maxLength: 255 }),
        Email: self.Email.extend({ required: true, email: true, maxLength: 255 }),
        Phone: self.Phone.extend({ required: true, maxLength: 255 }),
        ContactPreference: self.ContactPreference.extend({ required: true })
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });
};