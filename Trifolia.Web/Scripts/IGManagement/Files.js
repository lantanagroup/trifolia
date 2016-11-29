FilesModel = function (id, name) {
    var self = this;
    var mapping = {
        ignore: ['NewFile'],
        include: ['Id', 'Name'],
        'Files': {
            create: function (options) {
                return new FileModel(options.data);
            }
        }
    };

    self.Id = ko.observable(id);
    self.Name = ko.observable(name);
    self.Files = ko.observableArray();
    self.FilesNotRemoved = ko.computed(function () {
        return ko.utils.arrayFilter(self.Files(), function (item) {
            return item.IsRemoved() == false;
        });
    });
    self.CurrentFile = ko.observable();
    self.NewFile = ko.observable(new FileModel());
    self.TempDescription = ko.observable("");
    self.FileHistory = ko.observableArray([]);

    self.GetTypeDisplay = function (value) {
        switch (value.toString()) {
            case '0':
                return 'Implementation Guide Document';
            case '1':
                return 'Schematron';
            case '2':
                return 'Schematron Helper';
            case '3':
                return 'Vocabulary';
            case '4':
                return 'Sample (deliverable)';
            case '5':
                return 'Test Sample (good)';
            case '6':
                return 'Test Sample (bad)';
            case '7':
                return 'Data Snapshot (JSON)';
            case '8':
                return 'Image';
            case '9':
                return 'FHIR Resource Instance (XML or JSON)';
            default:
                return 'Unknown';
        }
    }

    self.RefreshFiles = function () {
        var data = {
            implementationGuideId: self.Id()
        };

        $.ajax({
            type: 'POST',
            url: '/IGManagement/Files/All',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            complete: function (jqXHR, statusText) {
                if (statusText == 'success') {
                    var responseData = JSON.parse(jqXHR.responseText);
                    ko.mapping.fromJS(responseData, mapping, self);
                } else {
                    alert('An error occurred while retrieving the files associated with this implementation guide.');
                }
            }
        });
    };

    self.AddFileClicked = function () {
        var newFile = new FileModel();
        var dateNow = new Date();
        newFile.Date((dateNow.getMonth() + 1) + '/' + dateNow.getDate() + '/' + dateNow.getFullYear() + ' ' + dateNow.getHours() + ':' + dateNow.getMinutes() + ':' + dateNow.getSeconds());
        self.NewFile(newFile);

        // Detect when the name matches an already existing file
        newFile.nameSubscription = newFile.Name.subscribe(function (fileName) {
            for (var i = 0; i < self.Files().length; i++) {
                var currentFile = self.Files()[i];

                if (currentFile.Name() == fileName) {
                    newFile.FileId(currentFile.FileId());
                    newFile.Description(currentFile.Description());
                    newFile.Url(currentFile.Url());
                    newFile.Type(currentFile.Type());
                    newFile.TypeEnabled(false);
                    return;
                }
            }

            newFile.FileId(null);
            newFile.Description(null);
            newFile.Type(0);
            newFile.TypeEnabled(true);
        });

        $('#addFileDialog').modal('show');
    };

    self.AddFileOkClicked = function () {
        var newFile = self.NewFile();
        var insertIndex = null;

        if (newFile.FileId()) {
            var replacementFile = ko.utils.arrayFirst(self.Files(), function (item) {
                return newFile.FileId() === item.FileId();
            });

            insertIndex = self.Files().indexOf(replacementFile);
        }

        if (insertIndex != null) {
            self.Files.valueWillMutate();
            self.Files()[insertIndex] = newFile;
            self.Files.valueHasMutated();
        } else {
            self.Files.push(newFile);
        }

        newFile.nameSubscription.dispose();
        delete newFile.nameSubscription;

        $('#addFileDialog').modal('hide');
    };

    self.AddFileCancelClicked = function () {
        self.NewFile().nameSubscription.dispose();
        $('#addFileDialog').modal('hide');
    };

    self.RemoveFileClicked = function (file) {
        if (!confirm("Are you sure you want to remove this file?")) {
            return;
        }

        if (file.FileId())
            file.IsRemoved(true);
        else
            self.Files.remove(file);
    };

    self.EditDescriptionClicked = function (file) {
        self.CurrentFile(file);
        self.TempDescription(file.Description());
        $('#editDescriptionDialog').modal("show");
    };

    self.EditDescriptionOkClicked = function () {
        self.CurrentFile().Description(self.TempDescription());
        self.TempDescription("");
        self.CurrentFile(null);

        $('#editDescriptionDialog').modal("hide");
    };

    self.EditDescriptionCancelClicked = function () {
        self.TempDescription("");
        self.CurrentFile(null);

        $('#editDescriptionDialog').modal("hide");
    };

    self.ViewHistory = function (fileId) {
        $.ajax({
            url: '/IGManagement/Files/History?fileId=' + fileId,
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (results) {
                ko.mapping.fromJS({ FileHistory: results }, {}, self);
            }
        });

        $('#fileHistoryDialog').modal('show');
    };

    self.Save = function () {
        var data = ko.mapping.toJS(self);

        $.blockUI({ message: 'Saving...' });
        $.ajax({
            type: 'POST',
            url: '/IGManagement/Files/Save',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            complete: function (jqXHR, statusText) {
                if (statusText == 'success') {
                    alert("Saved files!");
                    $.unblockUI();
                    self.RefreshFiles();
                } else {
                    alert('An error occurred while saving changes.');
                }
            }
            //progress: function (evt) {
            //    if (evt.lengthComputable) {
            //        var percentComplete = Math.round((evt.loaded / evt.total) * 100);
            //        saveProgress.progressbar('setValue', percentComplete);
            //    }
            //}
        });
    };

    self.Cancel = function () {
        window.location = '/IGManagement/View?implementationGuideId=' + self.Id();
    };

    self.RefreshFiles();
};

FileModel = function (data) {
    var self = this;
    var mapping = {
        ignore: ['File', 'FileObjectURL', 'TypeEnabled'],
        include: ['FileId', 'IsRemoved', 'Data', 'VersionId', 'Name', 'Date', 'Type', 'Note', 'Description', 'MimeType', 'Url']
    };

    self.File = ko.observable();
    self.FileObjectURL = ko.observable();
    self.Data = ko.observable();

    self.FileId = ko.observable();
    self.IsRemoved = ko.observable(false);
    self.VersionId = ko.observable();
    self.Name = ko.observable();
    self.Date = ko.observable();
    self.Type = ko.observable();
    self.TypeEnabled = ko.observable(true);
    self.Note = ko.observable();
    self.Description = ko.observable();
    self.Url = ko.observable();
    self.MimeType = ko.observable();

    self.Type.subscribe(function (newValue) {
        if (newValue != '7') {
            self.Url('');
        }
    });

    self.File.subscribe(function (newValue) {
        if (newValue) {
            self.Name(newValue.name);

            if (newValue.type)
                self.MimeType(newValue.type);
            else
                self.MimeType('binary/octet-stream');
        } else {
            self.Name(undefined);
            self.MimeType(undefined);
        }
    });

    var newFileValidation = ko.validatedObservable({
        File: self.File.extend({
            required: true
        }),
        Type: self.Type.extend({
            required: true
        }),
        Description: self.Description.extend({
            required: true
        }),
        Url: self.Url.extend({
            igFileUrlUnique: self.FileId,
            igFileUrl: true,
            maxLength: 25
        })
    });

    self.NewFileIsValid = ko.computed(function () {
        return newFileValidation.isValid();
    });

    ko.mapping.fromJS(data, mapping, self);
};

FileVersionModel = function (data) {
    var self = this;

    self.FileId = ko.observable();
    self.VersionId = ko.observable();
    self.Date = ko.observable();
    self.Note = ko.observable();
};