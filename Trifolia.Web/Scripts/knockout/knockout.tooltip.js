ko.bindingHandlers.tooltip = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = valueAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
        $(element).attr('title', valueUnwrapped);
        $(element).tooltip();

        if (typeof value === 'function' && value.subscribe) {
            value.subscribe(function (newValue) {
                $(element).attr('title', newValue);
                $(element).tooltip();
            });
        }
    },
};