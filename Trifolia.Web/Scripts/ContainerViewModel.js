var ContainerViewModel = function () {
    var self = this;

    self.Me = ko.observable();

    self.Initialize = function () {
        $(document).ajaxError(function (event, jqXHR, settings, thrownError) {
            if (jqXHR.responseJSON && jqXHR.responseJSON.ExceptionMessage) {
                console.log(jqXHR.responseJSON.ExceptionMessage);
                alert(jqXHR.responseJSON.ExceptionMessage);
            } else {
                console.log(jqXHR.responseText);
                //alert("An error occurred while processing the request");
            }
        });

        $.ajax({
            url: '/api/Auth/WhoAmI',
            cache: false,
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ Me: results }, {}, self);
            }
        });
    };

    self.DisplayName = ko.computed(function () {
        if (!self.Me()) {
            return 'Log In';
        }

        return self.Me().Name();
    });

    self.DisplayToolTip = ko.computed(function () {
        if (!self.Me()) {
            return '';
        }

        return self.Me().UserName();
    });

    self.HasSecurable = function (securableNames) {
        if (!self.Me()) {
            return false;
        }

        for (var securableNameIndex in securableNames) {
            var securableName = securableNames[securableNameIndex];

            var foundSecurable = ko.utils.arrayFirst(self.Me().Securables(), function (securable) {
                return securable == securableName;
            });

            if (foundSecurable)
                return true;
        }

        return false;
    };

    self.Initialize();
};