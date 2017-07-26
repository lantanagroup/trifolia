angular.module('Trifolia').controller('ExportCtrl', function ($scope, $uibModal, $sce, ImplementationGuideService, UserService, ExportService) {
    $scope.selectedImplementationGuide = null;
    $scope.message = '';
    $scope.exportFormats = [
        { id: 0, name: 'Microsoft Word (DOCX)', securable: 'ExportWordDocuments' },
        { id: 1, name: 'Web (HTML)', securable: 'ExportWordDocuments' },
        { id: 2, name: 'Snapshot (JSON)', securable: 'ExportXML' },
        { id: 3, name: 'Native (XML, JSON)', securable: 'ExportXML' },
        { id: 4, name: 'FHIR Bundle (XML, JSON)', securable: 'ExportXML' },
        { id: 5, name: 'FHIR Build Package (XML, JSON)', securable: 'ExportXML' },
        { id: 6, name: 'Templates DSTU (XML, JSON)', securable: 'ExportXML' },
        { id: 7, name: 'Schematron (SCH)', securable: 'ExportSchematron' },
        { id: 8, name: 'Vocabulary (XLSX)', securable: 'ExportVocabulary' },
        { id: 9, name: 'Vocabulary (Native XML)', securable: 'ExportVocabulary' },
        { id: 10, name: 'Vocbulary (Single SVS XML)', securable: 'ExportVocabulary' },
        { id: 11, name: 'Vocabulary (Multiple SVS XML)', securable: 'ExportVocabulary' },
        { id: 12, name: 'Vocbulary Bundle (FHIR XML)', securable: 'ExportVocabulary' }
    ];
    $scope.templateSelectionFormats = [0, 2, 3, 6, 7];
    $scope.xmlFormats = [3, 4, 5, 6];
    $scope.categorySelectionFormats = $scope.xmlFormats.concat([0, 7, 2]);
    $scope.vocFormats = [9, 10, 11, 12];
    $scope.fhirFormats = $scope.vocFormats.concat([0, 1, 2, 3, 4, 5]);
    $scope.fhirOnlyFormats = [4, 5];
    $scope.categories = [];
    $scope.valueSets = [];
    $scope.templates = [];
    $scope.validationResults = {};
    $scope.isValidating = false;
    $scope.isGettingTemplates = false;
    $scope.isGettingValuesets = false;
    $scope.saveSettingsMessage = '';
    $scope.criteria = {
        ImplementationGuideId: null,
        TemplateIds: [],
        ParentTemplateIds: [],
        ExportFormat: null,
        IncludeInferred: true,
        SelectedCategories: [],
        ValueSetTables: true,
        MaximumValueSetMembers: 10,
        ValueSetAppendix: true,
        TemplateSortOrder: 1,
        DocumentTables: 1,
        TemplateTables: 1,
        IncludeXmlSample: true,
        IncludeChangeList: true,
        IncludeTemplateStatus: true,
        IncludeNotes: true,
        VocabularyFileName: 'voc.xml',
        IncludeVocabulary: false,
        IncludeCustomSchematron: true,
        DefaultSchematron: 'not(.)',
        Encoding: 0,
        ReturnJSON: false,
        ValueSetOid: [],
        ValueSetMaxMembers: []
    };

    $scope.isExportFormatSpecified = function () {
        return $scope.criteria.ExportFormat >= 0 &&
            $scope.criteria.ExportFormat !== null &&
            $scope.criteria.ExportFormat !== undefined;
    }

    $scope.getExportFormats = function () {
        // Get a list of the formats that the user has permission to export (based on securables)
        var permittedFormats = _.filter($scope.exportFormats, function (exportFormat) {
            return UserService.hasSecurable(exportFormat.securable);
        });

        // Get a list of the formats that are supported by the selected implementation guide
        var supportedFormats = _.filter(permittedFormats, function (exportFormat) {
            var isFhirImplementationGuide = $scope.selectedImplementationGuide && $scope.selectedImplementationGuide.TypeNamespace === 'http://hl7.org/fhir';
            var isFhirFormat = $scope.fhirFormats.indexOf(exportFormat.id) >= 0;
            
            if (isFhirImplementationGuide && !isFhirFormat) {
                return false;
            } else if (!isFhirImplementationGuide && $scope.fhirOnlyFormats.indexOf(exportFormat.id) >= 0) {
                return false;
            }

            return true;
        });

        return supportedFormats;
    };

    $scope.maximumValueSetMembersChanged = function () {
        for (var i = 0; i < $scope.criteria.ValueSetMaxMembers.length; i++) {
            $scope.criteria.ValueSetMaxMembers[i] = $scope.criteria.MaximumValueSetMembers;
        }
    };

    $scope.toggleSelectAllTemplates = function () {
        if ($scope.criteria.TemplateIds.length === $scope.templates.length) {
            $scope.criteria.TemplateIds = [];
        } else {
            $scope.criteria.TemplateIds = [];
            _.each($scope.templates, function (template) {
                $scope.criteria.TemplateIds.push(template.Id);
            });
        }
    };

    $scope.toggleSelectedTemplate = function (templateId) {
        var index = $scope.criteria.TemplateIds.indexOf(templateId);

        if (index >= 0) {
            $scope.criteria.TemplateIds.splice(index, 1);
        } else {
            $scope.criteria.TemplateIds.push(templateId);
        }
    };

    $scope.loadSettings = function () {
        if (!$scope.criteria.ImplementationGuideId || !($scope.criteria.ExportFormat >= 0)) {
            return;
        }

        // Don't get the default export settings before the templates and value sets are done loading.
        if ($scope.isGettingTemplates || $scope.isGettingValuesets) {
            return;
        }

        ExportService.getExportSettings($scope.criteria.ImplementationGuideId, $scope.criteria.ExportFormat)
            .then(function (settings) {
                // The number of value sets have changed. Only update the ones that are recorded in the settings
                for (var i = 0; i < settings.ValueSetOid.length; i++) {
                    var currentIndex = $scope.criteria.ValueSetOid.indexOf(settings.ValueSetOid[i]);

                    if (currentIndex >= 0) {
                        $scope.criteria.ValueSetMaxMembers[currentIndex] = settings.ValueSetMaxMembers[i];
                    }
                }

                delete settings.ValueSetOid;
                delete settings.ValueSetMaxMembers;

                angular.extend($scope.criteria, settings);
            })
            .catch(function (err) {
                console.error('Error loading default export settings: ' + err);
            });
    };

    $scope.saveDefaultSettings = function () {
        ExportService.saveExportSettings($scope.criteria)
            .then(function () {
                $scope.saveSettingsMessage = 'Successfully saved default export settings.';
            })
            .catch(function (err) {
                $scope.saveSettingsMessage = 'Error saving settings: ' + err;
            });
    };

    $scope.loadValueSets = function () {
        $scope.valueSets = [];
        $scope.criteria.ValueSetOid = [];
        $scope.criteria.ValueSetMaxMembers = [];

        if (!$scope.selectedImplementationGuide || !$scope.selectedImplementationGuide.Id) {
            return;
        }

        $scope.isGettingValuesets = true;

        ImplementationGuideService.getImplementationGuideValueSets($scope.selectedImplementationGuide.Id, false)
            .then(function (valueSets) {
                $scope.valueSets = valueSets;

                _.each($scope.valueSets, function (valueSet) {
                    $scope.criteria.ValueSetOid.push(valueSet.Oid);
                    $scope.criteria.ValueSetMaxMembers.push($scope.criteria.MaximumValueSetMembers);
                });
            })
            .catch(function (err) {
                $scope.message = err;
            })
            .finally(function () {
                $scope.isGettingValuesets = false;
                $scope.loadSettings();
            });
    };

    $scope.loadTemplates = function () {
        $scope.criteria.TemplateIds = [];

        if (!$scope.selectedImplementationGuide || !$scope.selectedImplementationGuide.Id) {
            return;
        }

        $scope.isGettingTemplates = true;

        ImplementationGuideService.getImplementationGuideTemplates($scope.selectedImplementationGuide.Id, null, $scope.criteria.IncludeInferred)
            .then(function (templates) {
                _.each(templates, function (template) {
                    $scope.criteria.TemplateIds.push(template.Id);
                });

                $scope.templates = templates;
            })
            .catch(function (err) {
                $scope.message = err;
            })
            .finally(function () {
                $scope.isGettingTemplates = false;
                $scope.loadSettings();
            });
    };

    $scope.getLevel = function (level) {
        switch (level) {
            case 0:
                return 'Warning';
            case 1:
                return 'Error';
            default:
                return '';
        }
    };

    $scope.hasValidationMessages = function () {
        if (!$scope.validationResults) {
            return false;
        }

        if ($scope.validationResults.Messages && $scope.validationResults.Messages.length > 0) {
            return true;
        }

        if ($scope.validationResults.TemplateResults && $scope.validationResults.TemplateResults.length > 0) {
            return true;
        }

        return false;
    };

    $scope.getTemplateValidationMessages = function () {
        var validationMessages = [];

        _.each($scope.validationResults.TemplateResults, function (templateResult) {
            validationMessages = validationMessages.concat(templateResult.Items);
        });

        return validationMessages;
    };

    $scope.loadValidationResults = function () {
        $scope.validationResults = {};

        if (!$scope.selectedImplementationGuide || !$scope.selectedImplementationGuide.Id) {
            return;
        }

        $scope.isValidating = true;

        ImplementationGuideService.validate($scope.selectedImplementationGuide.Id)
            .then(function (validationResults) {
                $scope.validationResults = validationResults;

                for (var i = 0; i < $scope.validationResults.Messages.length; i++) {
                    $scope.validationResults.Messages[i] = $sce.trustAsHtml($scope.validationResults.Messages[i]);
                }
            })
            .catch(function (err) {
                $scope.message = err;
            })
            .finally(function () {
                $scope.isValidating = false;
            });
    };

    $scope.loadCategories = function () {
        $scope.categories = [];

        if (!$scope.selectedImplementationGuide || !$scope.selectedImplementationGuide.Id) {
            return;
        }

        // Get the categories for the implementation guide
        ImplementationGuideService.getImplementationGuideCategories($scope.selectedImplementationGuide.Id)
            .then(function (categories) {
                $scope.categories = categories;
            })
            .catch(function (err) {
                $scope.message = err;
            });
    };

    $scope.isExportDisabled = function () {
        if ($scope.isValidating || $scope.isGettingTemplates || $scope.isGettingValuesets) {
            return true;
        }

        if (!$scope.selectedImplementationGuide || !$scope.selectedImplementationGuide.Id || $scope.criteria.ExportFormat === undefined) {
            return true;
        }

        if ($scope.validationResults.RestrictDownload) {
            return true;
        }

        return false;
    };

    $scope.openSearch = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'selectImplementationGuideModal.html',
            controller: 'SearchIGModalCtrl',
            size: 'lg'
        });

        modalInstance.result.then(function (selectedItem) {
            $scope.selectedImplementationGuide = selectedItem;
            $scope.criteria.ImplementationGuideId = selectedItem.Id;

            $scope.loadCategories();

            $scope.loadValueSets();

            $scope.loadTemplates();

            $scope.loadValidationResults();
        }, function () {
            // Do nothing when closed without selecting
        });
    };

    $scope.$watch('criteria', function () {
        $scope.saveSettingsMessage = '';
    }, true);
});

angular.module('Trifolia').controller('SearchIGModalCtrl', function ($scope, $uibModalInstance, ImplementationGuideService) {
    $scope.implementationGuides = [];
    $scope.filteredImplementationGuides = [];
    $scope.message = '';
    $scope.query = '';

    $scope.queryChanged = function () {
        if (!$scope.query) {
            $scope.filteredImplementationGuides = $scope.implementationGuides.Items;
        } else {
            $scope.filteredImplementationGuides = _.filter($scope.implementationGuides.Items, function (ig) {
                var combined = ig.Title + (ig.DisplayName || '') + ig.Identifier + (ig.Status || '') + (ig.Organization || '');
                return combined.toLowerCase().indexOf($scope.query.toLowerCase().trim()) >= 0;
            });
        }
    };

    $scope.select = function (implementationGuide) {
        $uibModalInstance.close(implementationGuide);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('close');
    };

    $scope.init = function () {
        ImplementationGuideService.getImplementationGuides()
            .then(function (implementationGuides) {
                $scope.implementationGuides = implementationGuides;
                $scope.queryChanged();
            })
            .catch(function (err) {
                $scope.message = err;
            });
    };
});