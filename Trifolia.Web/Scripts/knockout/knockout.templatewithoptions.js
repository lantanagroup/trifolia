ko.virtualElements.allowedBindings.templateWithOptions = true;

ko.bindingHandlers.templateWithOptions = {
    init: ko.bindingHandlers.template.init,
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, context) {
        var options = ko.utils.unwrapObservable(valueAccessor());
        //if options were passed attach them to $data
        if (options.templateOptions) {
            context.$templateOptions = ko.utils.unwrapObservable(options.templateOptions);
        }
        //call actual template binding
        ko.bindingHandlers.template.update(element, valueAccessor, allBindingsAccessor, viewModel, context);
        //clean up
        delete context.$data.$item;
    }
}