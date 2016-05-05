var BulkCopyModel = function (templateId) {
    var self = this;

    // Properties
    self.CurrentStep = ko.observable(1);
    self.ExcelFields = ko.observableArray([]);
    self.TemplateMetaDataSheet = ko.observable();
    self.ConstraintChangesSheet = ko.observable();
    self.UploadData = ko.observable(new ExcelUploadModel());
    self.TemplateFields = ko.observableArray([
        { Name: 'IGNORE', Value: '' },
        { Name: 'Name', Value: 'TemplateName' },
        { Name: 'OID', Value: 'TemplateOid' },
        { Name: 'Bookmark', Value: 'TemplateBookmark' },
        { Name: 'Description', Value: 'TemplateDescription' }
    ]);
    self.ConstraintFields = ko.observableArray([
        { Name: 'IGNORE', Value: '' },
        { Name: 'Template', Value: 'ConstraintTemplate' },
        { Name: 'Number', Value: 'ConstraintNumber' },
        { Name: 'Data Type', Value: 'ConstraintDataType' },
        { Name: 'Conformance Verb', Value: 'ConstraintConformance' },
        { Name: 'Cardinality', Value: 'ConstraintCardinality' },
        { Name: 'Value Conformance', Value: 'ConstraintValueConformance' },
        { Name: 'Code', Value: 'ConstraintCode' },
        { Name: 'Display', Value: 'ConstraintDisplay' },
        { Name: 'Value Set (OID)', Value: 'ConstraintValueSet' },
        { Name: 'Value Set Date (mm/dd/yyyy)', Value: 'ConstraintValueSetDate' },
        { Name: 'Code System', Value: 'ConstraintCodeSystem' },
        { Name: 'Binding', Value: 'ConstraintBinding' },
        { Name: 'Contained Template (OID)', Value: 'ConstraintContainedTemplate' },
        { Name: 'Description', Value: 'ConstraintDescription' },
        { Name: 'Label', Value: 'ConstraintLabel' },
    ]);
    self.ExistingTemplates = ko.observableArray([]);
    self.IsCopying = ko.observable(false);
    self.CopyResults = ko.observable(new CopyResultsModel());
    self.ConfirmOverwrite = ko.observable(false);

    // Computed Properties
    self.ExternalTemplates = ko.computed(function () {
        return ko.utils.arrayFilter(self.ExistingTemplates(), function (item) {
            return item.IsSameImplementationGuide() == false;
        });
    });

    self.TemplateExcelSheets = ko.computed(function () {
        var sheets = [ '' ];
        for (var i = 0; i < self.ExcelFields().length; i++) {
            var currentSheetName = self.ExcelFields()[i].SheetName();

            if (self.ConstraintChangesSheet() != currentSheetName) {
                sheets.push(currentSheetName);
            }
        }
        return sheets;
    });

    self.ConstraintExcelSheets = ko.computed(function () {
        var sheets = [ '' ];
        for (var i = 0; i < self.ExcelFields().length; i++) {
            var currentSheetName = self.ExcelFields()[i].SheetName();

            if (self.TemplateMetaDataSheet() != currentSheetName) {
                sheets.push(currentSheetName);
            }
        }
        return sheets;
    });

    self.AvailableMappingFields = function (sheetName, mappingFields, currentFieldMapping) {
        var fields = [];
        
        var sheet = ko.utils.arrayFirst(self.ExcelFields(), function (item) {
            return item.SheetName() == sheetName;
        });

        if (!sheet) {
            return fields;
        }

        ko.utils.arrayForEach(mappingFields, function (fieldItem) {
            if (fieldItem.Value == '' || fieldItem.Value == currentFieldMapping) {
                fields.push(fieldItem);
                return;
            }

            var foundMapping = ko.utils.arrayFirst(sheet.Columns(), function (columnItem) {
                return columnItem.MappedField() == fieldItem.Value;
            });

            if (!foundMapping) {
                fields.push(fieldItem);
            }
        });

        return fields;
    };

    self.ExcelFieldsForTemplateMetaData = function () {
        var sheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
            return item.SheetName() == self.TemplateMetaDataSheet();
        });

        if (sheet) {
            return sheet.Columns;
        }

        return null;
    };

    self.ExcelFieldsForConstraintChanges = function () {
        var sheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
            return item.SheetName() == self.ConstraintChangesSheet();
        });

        if (sheet) {
            return sheet.Columns;
        }

        return null;
    };

    self.Step2Validation = ko.validatedObservable({
        TemplateMetaDataSheet: self.TemplateMetaDataSheet.extend({ required: true })
    });

    self.NextButtonEnabled = ko.computed(function () {
        if (self.CurrentStep() == 1) {
            return self.UploadData().Validation().isValid();
        } else if (self.CurrentStep() == 2) {
            return self.Step2Validation().isValid();
        } else if (self.CurrentStep() == 3) {
            return self.ExternalTemplates().length == 0 && (self.ExistingTemplates().length == 0 || self.ConfirmOverwrite());
        }

        return true;
    });

    self.NextButtonText = ko.computed(function () {
        if (self.CurrentStep() == 4) {
            return "Go to Template List";
        } else if (self.CurrentStep() == 3) {
            return "Begin Copy";
        } else {
            return "Next";
        }
    });

    self.NextButtonClicked = function () {
        if (self.CurrentStep() == 1) {
            step1Next();
        } else if (self.CurrentStep() == 2) {
            step2Next();
        } else if (self.CurrentStep() == 3) {
            step3Next();
        } else if (self.CurrentStep() == 4) {
            step4Next();
        }
    };

    self.CancelClicked = function () {
        location.href = '/TemplateManagement/List';
    };

    var getCopyModel = function () {
        var templateSheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
            return item.SheetName() == self.TemplateMetaDataSheet();
        });

        var constraintSheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
            return item.SheetName() == self.ConstraintChangesSheet();
        });

        var model = {
            BaseTemplateId: templateId,
            TemplateSheet: ko.toJS(templateSheet),
            ConstraintSheet: ko.toJS(constraintSheet)
        };

        return model;
    };

    self.DownloadConfigClicked = function () {
        var data = getCopyModel();
        var jsonData = JSON.stringify(data);

        $.fileDownload("/api/BulkCopy/Config", {
            httpMethod: "POST",
            data: data
        });
    }

    var applyConfig = function (configData) {
        var xmlText = atob(configData);
        var xml = $(xmlText);

        var templateMetaDataSheetEle = xml.find("TemplateMetaDataSheet");
        var templateMetaDataSheet = templateMetaDataSheetEle.length > 0 ? templateMetaDataSheetEle[0].innerText : null;
        self.TemplateMetaDataSheet(templateMetaDataSheet);

        var constraintChangesSheetEle = xml.find("ConstraintChangesSheet").first();
        var constraintChangesSheet = constraintChangesSheetEle.length > 0 ? constraintChangesSheetEle[0].innerText : null;
        self.ConstraintChangesSheet(constraintChangesSheet);

        var applyColumnMapping = function (sheetName, letter, mappedField) {
            var sheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
                return item.SheetName() == sheetName;
            });

            if (!sheet) {
                return;
            }

            var column = ko.utils.arrayFirst(sheet.Columns(), function (item, index) {
                return item.Letter() == letter;
            });

            if (column) {
                column.MappedField(mappedField);
            }
        }
        
        xml.find("TemplateColumns > ColumnConfig").each(function (index, item) {
            var letter = $(item).find("Letter")[0].innerText;
            var mappedField = $(item).find("MappedField")[0].innerText;

            applyColumnMapping(self.TemplateMetaDataSheet(), letter, mappedField);
        });

        xml.find("ConstraintColumns > ColumnConfig").each(function (index, item) {
            var letter = $(item).find("Letter")[0].innerText;
            var mappedField = $(item).find("MappedField")[0].innerText;

            applyColumnMapping(self.ConstraintChangesSheet(), letter, mappedField);
        });
    };

    // Actions
    var step1Next = function () {
        // TODO: Validate
        if (!self.UploadData().ExcelFile()) {
            alert("You have not selected an excel file to upload.");
            return;
        }

        var data = ko.mapping.toJS(self.UploadData());

        $("#BulkCopyContainer").block();

        // Get info from server
        $.ajax({
            method: "POST",
            url: "/api/BulkCopy/Parse",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                ko.mapping.fromJS({ ExcelFields: response }, {}, self);

                $("#BulkCopyContainer").unblock();
                self.CurrentStep(2);

                if (self.UploadData().ConfigFile()) {
                    applyConfig(self.UploadData().ConfigFile());
                }
            }
        });
    };

    var step2Next = function () {
        var mappedColumnExists = function (sheet, columnMapping) {
            var sheet = ko.utils.arrayFirst(self.ExcelFields(), function (item, index) {
                return item.SheetName() == sheet;
            });

            if (!sheet) {
                return null;
            }

            var column = ko.utils.arrayFirst(sheet.Columns(), function (item, index) {
                return item.MappedField() == columnMapping;
            });

            if (column) {
                return true;
            }

            return false;
        }

        // Validate Template Fields
        if (!mappedColumnExists(self.TemplateMetaDataSheet(), 'TemplateName')) {
            alert("You must specify a Template Name mapping before continuing...");
            return;
        }

        if (!mappedColumnExists(self.TemplateMetaDataSheet(), 'TemplateOid')) {
            alert("You must specify a Template OID mapping before continuing...");
            return;
        }

        if (!mappedColumnExists(self.TemplateMetaDataSheet(), 'TemplateBookmark')) {
            alert("You must specify a Template Bookmark mapping before continuing...");
            return;
        }

        // Validate Constraint Fields
        if (self.ConstraintChangesSheet()) {
            if (!mappedColumnExists(self.ConstraintChangesSheet(), 'ConstraintTemplate')) {
                alert("When specifying constraint changes, the constraint changes sheet must have a field for \"Template\"");
                return;
            }

            if (!mappedColumnExists(self.ConstraintChangesSheet(), 'ConstraintNumber')) {
                alert("When specifying constraint changes, the constraint changes sheet must have a field for \"Number\"");
                return;
            }
        }

        self.CurrentStep(3);

        // Check for templates that already exist
        var templateSheet = ko.utils.arrayFirst(self.ExcelFields(), function (item) {
            return item.SheetName() == self.TemplateMetaDataSheet();
        });

        var oidColumn = ko.utils.arrayFirst(templateSheet.Columns(), function (item) {
            return item.MappedField() == "TemplateOid";
        });

        var templateOids = [];
        ko.utils.arrayForEach(templateSheet.Rows(), function (row) {
            var rowOidColumn = ko.utils.arrayFirst(row.Cells(), function (cell) {
                return cell.Letter() == oidColumn.Letter();
            });

            templateOids.push(rowOidColumn.Value());
        });

        var data = {
            sourceTemplateId: templateId,
            templateOids: templateOids
        };

        $("#BulkCopyContainer").block();

        // Check for templates that already exist
        $.ajax({
            method: 'GET',
            url: '/api/BulkCopy/Templates/Existing',
            data: data,
            traditional: true,
            success: function (existingTemplates) {
                ko.mapping.fromJS({ ExistingTemplates: existingTemplates }, {}, self);
                $("#BulkCopyContainer").unblock();
            }
        });
    };

    var step3Next = function () {
        self.IsCopying(true);
        self.CurrentStep(4);

        var data = getCopyModel();

        $.ajax({
            method: 'POST',
            url: "/api/BulkCopy/Copy",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                ko.mapping.fromJS({ CopyResults: response }, {}, self);
                self.IsCopying(false);
            }
        });
    };

    var step4Next = function () {
        location.href = '/TemplateManagement/List';
    };
};

var ExcelUploadModel = function () {
    var self = this;

    self.ExcelFileInfo = ko.observable();
    self.ExcelFileUrl = ko.observable();
    self.ExcelFile = ko.observable();
    self.ConfigFileInfo = ko.observable();
    self.ConfigFileUrl = ko.observable();
    self.ConfigFile = ko.observable();
    self.FirstRowIsHeader = ko.observable(false);

    self.Validation = ko.validatedObservable({
        ExcelFileInfo: self.ExcelFileInfo.extend({ required: true })
    });
};

var CopyResultsModel = function () {
    var self = this;
    
    self.Errors = ko.observableArray([]);
    self.NewTemplates = ko.observableArray([]);
};