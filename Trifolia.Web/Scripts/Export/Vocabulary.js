var ExportVocabularyViewModel = function (implementationGuideId) {
    var self = this;

    self.ValueSets = ko.observableArray();
    self.ImplementationGuideId = ko.observable();
    self.Name = ko.observable();
    self.CancelUrl = ko.observable();
    self.MaximumMembers = ko.observable();

    self.LoadValueSets = function () {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/ValueSet',
            cache: false,
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ ValueSets: results }, {}, self);
            }
        });
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/Export/' + implementationGuideId + '/Vocabulary',
            cache: false,
            async: false,
            success: function (results) {
                ko.mapping.fromJS(results, {}, self);
            }
        });

        self.LoadValueSets();
    };

    self.Export = function () {
        $("#ExportVocabularyForm").attr("action", "/api/Export/Vocabulary");
        $("#ExportVocabularyForm").submit();
    };

    self.Cancel = function () {
        location.href = self.CancelUrl();
    };

    self.Initialize();
};

function ExportFormatChanged() {
    var exportFormat = $("#ExportFormat").val();

    if (exportFormat == 1 || exportFormat == 2)
        $("#Encoding").removeAttr('disabled');
    else
        $("#Encoding").attr('disabled','disabled');

    RefreshValueSets();
}