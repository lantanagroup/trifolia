ko.bindingHandlers.highlight = {
    init: function (element, valueAccessor) {
        var highlightText = valueAccessor();

        if (highlightText()) {
            setTimeout(function () {
                $(element).highlight(highlightText());
            });
        }

        if (typeof highlightText === 'function') {
            highlightText.subscribe(function (newVal, oldVal) {
                $(element).unhighlight();

                if (newVal) {
                    $(element).highlight(newVal);
                }
            });
        }
    }
};