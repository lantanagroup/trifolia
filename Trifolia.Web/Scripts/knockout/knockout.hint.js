ko.bindingHandlers.hint = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
        $(element).attr('title', valueUnwrapped);
        $(element).hint();
    },
};