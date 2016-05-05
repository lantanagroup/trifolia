ko.bindingHandlers.file = {
    init: function (element, valueAccessor) {
        $(element).change(function () {
            var file = this.files[0];
            if (ko.isObservable(valueAccessor())) {
                valueAccessor()(file);
            }
        });
    },

    update: function (element, valueAccessor, allBindingsAccessor) {
        var file = ko.utils.unwrapObservable(valueAccessor());
        var bindings = allBindingsAccessor();

        if (bindings.fileBinaryData && ko.isObservable(bindings.fileBinaryData)) {
            if (!file) {
                bindings.fileBinaryData(null);
            } else {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var result = e.target.result || {};
                    var resultParts = result.split(',');

                    if (resultParts.length === 2) {
                        bindings.fileBinaryData(resultParts[1]);
                    }

                    if (bindings.fileObjectURL && ko.isObservable(bindings.fileObjectURL)) {
                        var oldUrl = bindings.fileObjectURL();
                        if (oldUrl) {
                            window.URL.revokeObjectURL(oldUrl);
                        }
                        bindings.fileObjectURL(file && window.URL.createObjectURL(file));
                    }
                };
                reader.readAsDataURL(file);
            }
        }
    }
};

ko.validation.makeBindingHandlerValidatable("file");