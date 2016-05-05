ko.observableArray.fn.indexOfId = function (node) {
    for (var i = 0; i < this().length; i++) {
        var currentConstraint = this()[i];

        if (currentConstraint.Id() == node.Id()) {
            return i;
        }
    }

    return -1;
};

ko.bindingHandlers.disableAll = {
    update: function(element, valueAccessor, allBindingsAccessor) {
        var aa = allBindingsAccessor();
        var va = valueAccessor();
        var value = ko.utils.unwrapObservable(va);
        var formFields = $(element).find('input, select, textarea, button');
        
        if( value )
            formFields.attr('disabled', 'disabled');
        else
            formFields.removeAttr('disabled');
    }
};

ko.bindingHandlers.enableAll = {
    update: function(element, valueAccessor, allBindingsAccessor) {
        var aa = allBindingsAccessor();
        var va = valueAccessor();
        var value = ko.utils.unwrapObservable(va);
        var formFields = $(element).find('input, select, textarea, button');
        
        if( value )
            formFields.removeAttr('disabled');
        else
            formFields.attr('disabled', 'disabled');
    }
};