

ko.bindingHandlers.localizationContext = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        element.localizationContext = valueAccessor();
    }
};

ko.bindingHandlers.localization = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var getContext = function () {
            var current = element;
            var context = element.localizationContext;

            while (current && !context) {
                context = current.localizationContext;
                current = current.parentNode;
            }

            return context;
        };

        var context = typeof (allBindings().localizationContext) !== 'undefined' ? allBindings().localizationContext : getContext();
        var localizationBindings = valueAccessor();

        if (!context) {
            console.log("Could not find context for localization binding");
            return;
        }

        for (var i in localizationBindings) {
            var currentLocalizationBinding = localizationBindings[i];

            if (!context[currentLocalizationBinding]) {
                console.log("Could not find localization property in context for " + currentLocalizationBinding);
                continue;
            }

            if (i === 'title') {
                $(element).attr('title', context[currentLocalizationBinding]);
            } else if (i === 'html') {
                $(element).html(context[currentLocalizationBinding]);
            }
        }
    }
};