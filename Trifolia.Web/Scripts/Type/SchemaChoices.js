var SchemaChoicesViewModel = function (implementationGuideTypeId) {
    var self = this;

    self.Choices = ko.observableArray([]);

    self.Save = function () {
        var data = ko.mapping.toJS(self.Choices());

        $.blockUI();
        $.ajax({
            url: '/api/Type/' + implementationGuideTypeId + '/SchemaChoice',
            data: JSON.stringify(data),
            dataType: 'json',
            method: 'POST',
            contentType: 'application/json',
            error: function (err) {
                alert(typeof err == 'object' ? err.responsText : err);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    // Initialization
    $.blockUI();
    $.ajax({
        url: '/api/Type/' + implementationGuideTypeId + '/SchemaChoice',
        success: function (results) {
            for (var i in results) {
                self.Choices.push(new SchemaChoice(results[i]));
            }
        },
        error: function (err) {
            alert(err);
        },
        complete: function () {
            $.unblockUI();
        }
    });
};

var SchemaChoice = function (data) {
    var self = this;
    var mapping = {
        'copy': ['ComplexTypeName', 'CalculatedName', 'SourceUri', 'XPath', 'Documentation', 'ChildrenElements']
    };

    self.ComplexTypeName = null;
    self.CalculatedName = null;
    self.DefinedName = ko.observable();
    self.SourceUri = null;
    self.XPath = null;
    self.Documentation = null;
    self.ChildrenElements = [];

    self.GetChoiceTooltip = function () {
        var html = 'XPath: ' + self.XPath + '<br/>\nSource URI: ' + self.SourceUri + '<br/>\nChild Elements: ' + self.ChildrenElements.join(', ');
        return html;
    };

    ko.mapping.fromJS(data, mapping, self);
}