var emailNameRequired;

var requireEmailandName = function (logIn) {
    emailNameRequired = logIn;
}

var SupportRequest = function () {
    var self = this;

    self.showEmailName = ko.observable(emailNameRequired);
    self.Name = ko.observable();
    self.Email = ko.observable();
    self.Priority = ko.observable();
    self.Summary = ko.observable();
    self.Type = ko.observable();
    self.Details = ko.observable();

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: emailNameRequired }),
        Email: self.Email.extend({ required: emailNameRequired }),
        Summary: self.Summary.extend({ required: true, maxLength: 254 }),
        Details: self.Details.extend({ required: true })
    });

    self.IsValid = ko.computed(function () {
        return validation.isValid();
    });
}

var SupportViewModel = function () {
    var self = this;

    $.ajax({
        async: false,
        url: '/api/Auth/WhoAmI',
        complete: function (jqXHR, textstatus) {
            requireEmailandName(!jqXHR.responseJSON);
        }
    });

    self.Request = ko.observable(new SupportRequest());

    self.SubmitSupportRequest = function () {
        if (!self.Request().IsValid()) {
            alert('Please fix any validation errors before submitting.');
            return;
        }

        $("#supportPopup").block();

        var data = {
            "SupportName": self.Request().Name(),
            "SupportEmail": self.Request().Email(),
            "SupportSummary": self.Request().Summary(),
            "SupportType": self.Request().Type(),
            "SupportPriority": self.Request().Priority(),
            "SupportDetails": self.Request().Details()
        };

        $.ajax({
            type: "POST",
            url: "/Support/SubmitSupportRequest",
            data: data,
            complete: function (jqXHR, textStatus) {
                if (textStatus != 'success') {
                    alert('There was an error submitting your support ticket');
                } else {
                    self.CancelSupportRequest();
                    alert('Support Ticket Submitted Successfully');
                }

                $("#supportPopup").unblock();
            }
        });
    };

    self.CancelSupportRequest = function () {
        $("#supportPopup").modal('hide');
        self.Request(new SupportRequest());
    };

    self.ShowSupportRequest = function () {
        $("#supportPopup").modal('show');
    };
};