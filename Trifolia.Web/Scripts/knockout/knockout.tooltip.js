ko.bindingHandlers.tooltip = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = valueAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
        var options = allBindingsAccessor().tooltipOptions || {};

        if (valueUnwrapped) {
            $(element).attr('title', valueUnwrapped);
        }

        $(element).tooltip(options);

        if (typeof value === 'function' && value.subscribe) {
            value.subscribe(function (newValue) {
                var options = allBindingsAccessor().tooltipOptions || {};

                if (value) {
                    $(element).attr('title', newValue);
                }

                $(element).tooltip(options);
            });
        }
    },
};