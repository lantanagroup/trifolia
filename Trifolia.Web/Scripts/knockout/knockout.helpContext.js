ko.bindingHandlers.helpContext = {
    init: function (element, valueAccessor) {
        var helpContextId = valueAccessor();
        $(element).helpContext(helpContextId);
    }
};

ko.bindingHandlers.helpTooltip = {
    init: function (element, valueAccessor) {
        var options = valueAccessor() || {};

        if (typeof options.trigger === "undefined") {
            options.trigger = "click";
        }

        $(element)
            .addClass("glyphicon")
            .addClass("glyphicon-question-sign")
            .addClass("clickable")
            .tooltip(options);
    }
};