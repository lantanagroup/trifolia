ko.bindingHandlers.datagrid = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var bindings = allBindingsAccessor();
        var ignoreSubscription = false;

        $(element).datagrid({
            onSelect: function (rowIndex, rowData, arg1, arg2, arg3) {
                var rowId = null;
                eval("rowId = rowData." + bindings.itemId + ";");

                ignoreSubscription = true;

                if (bindings.selectedIndex && ko.isObservable(bindings.selectedIndex)) {
                    bindings.selectedIndex(rowIndex);
                }

                if (bindings.selectedItemId && ko.isObservable(bindings.selectedItemId) && bindings.itemId) {
                    bindings.selectedItemId(rowId);
                }

                ignoreSubscription = false;
            }
        });

        if (bindings.selectedItemId && ko.isObservable(bindings.selectedItemId) && bindings.itemId) {
            bindings.selectedItemId.subscribe(function (newSelectedItemId) {
                if (!ignoreSubscription) {
                    if (newSelectedItemId) {
                        $(element).datagrid("selectRecord", newSelectedItemId);
                    } else {
                        $(element).datagrid("unselectAll");
                    }
                }
            });
        }

        if (bindings.selectedIndex && ko.isObservable(bindings.selectedIndex)) {
            bindings.selectedIndex.subscribe(function (newSelectedIndex) {
                if (!ignoreSubscription) {
                    if (newSelectedIndex && newSelectedIndex >= 0) {
                        $(element).datagrid("selectRow", newSelectedIndex);
                    } else {
                        $(element).datagrid("unselectAll");
                    }
                }
            });
        }
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = valueAccessor();
        var valueUnwrapped = value();

        $(element).datagrid({
            data: ko.toJS(valueUnwrapped)
        });
    }
};