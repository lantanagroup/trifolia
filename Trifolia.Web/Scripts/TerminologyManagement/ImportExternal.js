var ImportExternalViewModel = function () {
    var self = this;

    self.SearchSource = ko.observable('phinVads');
    self.SearchOid = ko.observable('');
    self.ImportValueSet = ko.observable();

    self.Search = function () {
        if (!self.SearchOid()) {
            alert('No OID to search for!');
            return;
        }

        self.ImportValueSet(null);

        var url = '';

        if (self.SearchSource() == 'phinVads') {
            url = '/api/Terminology/Import/PhinVads/Search';
        } else if (self.SearchSource() == 'roseTree') {
            url = '/api/Terminology/Import/RoseTree/Search';
        }

        url += '?oid=' + self.SearchOid();

        $.blockUI({ message: 'Searching...' });
        $.ajax({
            url: url,
            success: function (results) {
                if (!results) {
                    alert('No value set found!');
                } else {
                    ko.mapping.fromJS({ ImportValueSet: results }, {}, self);
                }
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
        if (!self.ImportValueSet()) {
            alert('No value set found during search to import!');
            return;
        }

        $.blockUI({ message: 'Importing...' });
        $.ajax({
            method: 'POST',
            url: '/api/Terminology/Import/External',
            data: ko.mapping.toJS(self.ImportValueSet()),
            success: function () {
                alert('Successfully imported value set!');
                self.ImportValueSet(null);
                self.SearchOid('');
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