ko.bindingHandlers.spinedit = {
	init: function (element, valueAccessor, allBindingsAccessor) {
	    var value = valueAccessor();
	    var allBindings = allBindingsAccessor();
	    var options = allBindings.spineditOptions || {};
	    var treatZeroAsNull = allBindings.treatZeroAsNull || false;

	    if (typeof options.maximum === 'undefined') {
	        options.maximum = 10000000;     // 10 million
	    }

	    $(element)
            .spinedit(options)
            .on("valueChanged", function (e) {
                if (e.value == 0 && treatZeroAsNull) {
                    value(null);
                } else {
                    value(e.value);
                }
            });
	},
	update: function (element, valueAccessor, allBindingsAccessor) {
	    var value = valueAccessor();
	    $(element).spinedit('setValue', value());
	}
};