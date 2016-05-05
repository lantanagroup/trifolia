var ImportExcelViewModel = function () {
    var self = this;

    self.FileInfo = ko.observable();
    self.File = ko.observable();
    self.CheckResults = ko.observable();

    self.GetChangeType = function (changeTypeId) {
        switch (changeTypeId) {
            case 1:
                return 'Add';
            case 2:
                return 'Update';
            default:
                return 'No Change';
        }
    };

    self.Check = function () {
        var data = {
            Data: ko.mapping.toJS(self.File()),
            FirstRowIsHeader: true
        };

        $.blockUI({ message: 'Checking import...' });
        $.ajax({
            method: 'POST',
            url: '/api/Terminology/Import/Excel/Check',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (results) {
                ko.mapping.fromJS({ CheckResults: results }, {}, self);
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.Import = function () {
        if (!self.CheckResults() || self.CheckResults().Errors().length != 0) {
            alert("Resolve all errors before committing the import.");
            return;
        }

        $.blockUI({ message: 'Importing...' });
        $.ajax({
            method: 'POST',
            url: '/api/Terminology/Import/Excel',
            data: ko.mapping.toJS(self.CheckResults()),
            success: function () {
                alert('Successfully imported value sets and concepts!');
                self.CheckResults(null);
            },
            error: function (ex) {
                var exception = JSON.parse(ex.responseText);
                alert(exception.ExceptionMessage);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };
};