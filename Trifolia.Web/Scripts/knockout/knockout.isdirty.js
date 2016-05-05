ko.dirtyFlag = function (root) {
    var self = this;

    var result = function() {},
        _initialState = ko.observable(ko.toJSON(root)),
        _isDirty = ko.observable(false);

    result.isDirty = ko.computed(function () {
        if (_initialState() !== ko.toJSON(root)) {
            _isDirty(true);
        }

        return _isDirty();
    });
    
    result.reset = function() {
        _initialState(ko.toJSON(root));
        _isDirty(false);
    };

    return result;
};