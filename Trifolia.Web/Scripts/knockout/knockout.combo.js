(function ($) {
    $.each(['show', 'hide'], function (i, ev) {
        var el = $.fn[ev];
        $.fn[ev] = function () {
            this.trigger(ev);
            return el.apply(this, arguments);
        };
    });
})(jQuery);

var GenericCombo = function (valueField, textField) {
    var self = this;
    var initializing = true;

    var calculateWidth = function (element, allBindings) {
        var width = allBindings().width || $(element).attr('width') || $(element).width();
        var eleWidth = width;

        if (width.toString().indexOf('%', width.toString().length - 1) !== -1) {
            var parentWidth = $(parent).width();
            eleWidth = (parentWidth / parseInt(width.substring(0, width.length - 1))) * 100;
        }

        return eleWidth;
    };
    
    self.Initializing = true;
    self.GetData = null;
    self.ValidValue = null;
    self.ValueType = 'string';
    self.DisplayFormatter = function (row) {
        return row[self.TextField];
    };
    self.TypeAheadFilter = function (q, row) {
        return row[self.TextField] && row[self.TextField].toLowerCase().indexOf(q.toLowerCase()) >= 0;
    };
    self.ValueField = valueField ? valueField : 'value';
    self.TextField = textField ? textField : 'text';
    
    self.Binding = {
        init: function (element, valueAccessor, allBindings) {
            var value = valueAccessor();
            var comboDisabled = typeof (allBindings().comboDisabled) !== 'undefined' ? allBindings().comboDisabled : null;
            var comboEditable = typeof (allBindings().comboEditable) !== 'undefined' ? allBindings().comboEditable : true;
            var onChange = typeof (allBindings().onChange) !== 'undefined' ? allBindings().onChange : null;
            var requireSelection = typeof (allBindings().requireSelection) !== 'undefined' ? allBindings().requireSelection : true;
            var valueType = typeof (allBindings().valueType) !== 'undefined' ? allBindings().valueType : self.ValueType;
            var comboDisabledVal = false;

            if (!valueType) {
                valueType = self.ValueType;
            }

            if (typeof (comboDisabled) === 'function') {
                comboDisabledVal = comboDisabled();

                comboDisabled.subscribe(function (newComboDisabledVal) {
                    var setValue = typeof (value()) !== 'undefined' && value() != null ? value().toString() : '';
                    /*$(element).combobox({
                        disabled: newComboDisabledVal,
                        value: setValue
                    });*/
                });
            } else if (typeof (comboDisabled) == 'object') {
                comboDisabledVal = comboDisabled;
            }
            
            /*
            $(element).combobox({
                valueField: self.ValueField,
                textField: self.TextField,
                width: calculateWidth(element, allBindings),
                disabled: comboDisabledVal,
                formatter: self.DisplayFormatter,
                filter: self.TypeAheadFilter,
                editable: comboEditable,
                onChange: function (newValue) {
                    var data = $(element).combobox('getData');
                    var foundItem = false;

                    if (value() == newValue) {
                        return;
                    }

                    if (self.ValidValue && !self.ValidValue(newValue)) {
                        return;
                    }

                    if (requireSelection) {
                        for (var i in data) {
                            if (data[i][self.ValueField] == newValue) {
                                foundItem = data[i];
                            }
                        }
                    } else {
                        foundItem = true;
                    }

                    if (foundItem) {
                        if (valueType == 'int') {
                            newValue = parseInt(newValue);

                            if (isNaN(newValue)) {
                                newValue = '';
                            }
                        } else if (valueType == 'boolean') {
                            if (newValue == 'true' || newValue == 'True') {
                                newValue = true;
                            } else {
                                newValue = false;
                            }
                        }

                        value(newValue);

                        if (onChange) {
                            onChange(newValue, foundItem);
                        }
                    } else {
                        value('');

                        if (onChange) {
                            onChange('');
                        }
                    }
                }
            });
            */
            
            $(parent).on('resize', function () {
                if ($(element).is(':visible')) {
                    var newWidth = calculateWidth(element, allBindings);
                    //$(element).combobox('resize', newWidth);
                }
            });

            if (self.GetData) {
                self.GetData(element, valueAccessor, allBindings, function (data) {
                    /*
                    $(element).combobox({
                        data: data
                    });
                    */

                    var setValue = typeof (value()) !== 'undefined' && value() != null ? value().toString() : '';
                    //$(element).combobox('setValue', setValue);

                    self.Initializing = false;
                });
            } else {
                var setValue = typeof (value()) !== 'undefined' && value() != null ? value().toString() : '';
                //$(element).combobox('setValue', setValue);

                self.Initializing = false;
            }
        },
        update: function (element, valueAccessor) {
            if (self.Initializing) {
                return;
            }

            var value = valueAccessor();

            if (typeof (value()) !== 'undefined' && value() != null) {
                //$(element).combobox('setValue', value().toString());
            }
        }
    };
};

/* Simple Combo */
var combo = new GenericCombo();
ko.bindingHandlers.combo = combo.Binding;
