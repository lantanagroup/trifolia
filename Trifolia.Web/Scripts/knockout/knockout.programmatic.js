/*
 * Knockout jQuery Data-Bind plugin
 * (c) Adam Pflug
 * License: MIT (http://www.opensource.org/licenses/mit-license.php)
 */

(function (jQuery, undefined) {
    /*global ko*/
    var $ = jQuery;

    var bindToBindingHandler = function (options) {
        /*
         * dependentObservable will be called every time one of the observable it depends
         * on changes. Also, options will still be accessable because it's in the closure.
         * This should be more efficient than regular knockout because bindings don't need
         * to be reparsed each time an observable changes its value.
         */
        var bindingContext = ko.bindingContext ? new ko.bindingContext(ko.utils.unwrapObservable(options.viewModel)) : {};
        if (options.bindingHandler && typeof options.bindingHandler.init == "function") {
            options.bindingHandler.init(
                options.element,
                options.valueAccessor,
                options.allBindingsAccessor,
                options.viewModel,
                bindingContext
            );
        }
        var updateListener = new ko.dependentObservable(function () {
            if (options.bindingHandler && typeof options.bindingHandler.update == "function") {
                options.bindingHandler.update(
                    options.element,
                    options.valueAccessor,
                    options.allBindingsAccessor,
                    options.viewModel,
                    bindingContext
                );
            }
        }, window);
        return updateListener;
    };

    $.fn.databind = function (bindings, viewModel) {
        var buildValueAccessor = function (value) {
            return function () { return value; };
        };

        // process each of the databindings in turn
        this.each(function () {
            var bindingProperty;
            var existingBindings = $(this).data("knockoutProgrammatic.bindings");
            if (existingBindings === undefined) {
                existingBindings = {};
                $(this).data("knockoutProgrammatic.bindings", existingBindings);
            }
            var listeners = $(this).data("knockoutProgrammatic.listeners");
            if (listeners === undefined) {
                listeners = {};
                $(this).data("knockoutProgrammatic.listeners", listeners);
            }
            var allBindingsAccessor = function () { return existingBindings; };

            for (bindingProperty in bindings)  {
                if (bindings.hasOwnProperty(bindingProperty)) {
                    if (existingBindings[bindingProperty]) {
                        throw new $.databind.DuplicateBindingException(bindingProperty, this);
                    } else {
                        existingBindings[bindingProperty] = bindings[bindingProperty];
                    }
                }
            }

            for (bindingProperty in bindings) {
                if (bindings.hasOwnProperty(bindingProperty)) {
                    listeners[bindingProperty] = bindToBindingHandler({
                        element: this,
                        valueAccessor: buildValueAccessor(bindings[bindingProperty]),
                        allBindingsAccessor: allBindingsAccessor,
                        bindingHandler: ko.bindingHandlers[bindingProperty],
                        viewModel: viewModel || window
                    });
                }
            }
        });
        return this; // preserve jquery chaining support
    };

    $.fn.undatabind = function (bindings) {
        // convert bindings to remove into an array
        if (typeof bindings === 'string') {
            bindings = [bindings];
        }

        // process each of the databindings in turn
        this.each(function () {
            var i, len;
            var listeners = $(this).data("knockoutProgrammatic.listeners");
            var context = $(this).data("knockoutProgrammatic.bindings");
            if (bindings === undefined) {
                // unbind all bindings
                bindings = [];
                for (i in listeners) {
                    if (listeners.hasOwnProperty(i)){
                        bindings.push(i);
                    }
                }
            }
            // unbind databindings
            for (i = 0, len = bindings.length; i < len; i++) {
                // unbind
                listeners[bindings[i]].dispose();
                delete listeners[bindings[i]];
                delete context[bindings[i]];
            }
        });
        return this; // preserve jquery chaining support
    };

    $.databind = $.databind || {};
    $.databind.DuplicateBindingException = function (property, element) {
        this.property = property;
        this.element = element;
        this.message= "Binding '"+this.property+"' already exists on this element";
    };
    $.databind.DuplicateBindingException.prototype.toString = function () {
        return this.message;
    };

    // hook into jQuery.cleanData to clean up on remove
    var _cleanData = jQuery.cleanData;
    jQuery.cleanData = function (elems) {
        for ( var i = 0, elem; (elem = elems[i]) !== undefined; i++ ) {
            if ($(elem).data('knockoutProgrammatic.listeners') !== undefined) {
                $(elem).undatabind();
            }
        }
        _cleanData.apply(jQuery, arguments);
    };
})(jQuery);