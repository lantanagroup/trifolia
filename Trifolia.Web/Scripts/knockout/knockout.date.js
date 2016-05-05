ko.bindingHandlers.date = {
    init: function (element, valueAccessor, allBindings) {
        var value = valueAccessor();
        var allBindingsAccessor = allBindings();

        var options = allBindingsAccessor.dateOptions || {
            format: 'mm/dd/yyyy',
            todayBtn: 'linked',
            autoclose: true,
            todayHighlight: true
        };

        $(element).datepicker(options)
            .on('changeDate', function (ev) {
                var date = ev.date.getDate();
                var month = ev.date.getMonth() + 1; //Months are zero based
                var year = ev.date.getFullYear();

                $(element).attr('isUpdating', true);
                value(month + '/' + date + '/' + year);
                $(element).removeAttr('isUpdating');
            })
            .on('clearDate', function (ev) {
                $(element).attr('isUpdating', true);
                value('');
                $(element).removeAttr('isUpdating');
            });

        $(element).datepicker('update', value());

        /*
        $(element).change(function () {
            var newVal = $(element).val();

            if (!newVal) {
                value(null);
            }
        });
        */
    },
    update: function (element, valueAccessor) {
        var value = valueAccessor();

        if ($(element).attr('isUpdating') === "true") {
            return;
        }

        $(element).datepicker('update', value());
    }
};