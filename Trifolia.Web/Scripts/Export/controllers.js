angular.module('Trifolia').controller('ExportCtrl', function ($scope, $uibModal, ImplementationGuideService) {
    $scope.selectedImplementationGuide = null;
    $scope.message = '';
    $scope.exportFormats = [
        { id: 0, name: 'Microsoft Word (DOCX)' },
        { id: 1, name: 'Web (HTML)' },
        { id: 2, name: 'Snapshot (JSON)' },
        { id: 3, name: 'Native (XML, JSON)' },
        { id: 4, name: 'FHIR Bundle (XML, JSON)' },
        { id: 5, name: 'FHIR Build Package (XML, JSON)' },
        { id: 6, name: 'Templates DSTU (XML, JSON)' },
        { id: 7, name: 'Schematron (SCH)' },
        { id: 8, name: 'Vocabulary (XLSX)' },
        { id: 9, name: 'Vocabulary (Native XML)' },
        { id: 10, name: 'Vocbulary (Single SVS XML)' },
        { id: 11, name: 'Vocabulary (Multiple SVS XML)' },
        { id: 12, name: 'Vocbulary Bundle (FHIR XML)' }
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
    $scope.criteria = {
        selectedExportFormat: null,
        includeInferred: true,
        selectedCategories: [],
        valueSetTables: true,
        maximumMembers: 10,
        valuesetsAsAppendix: true
    };

    $scope.getExportFormats = function() {
        return _.filter($scope.exportFormats, function (exportFormat) {
            var isFhirImplementationGuide = $scope.selectedImplementationGuide && $scope.selectedImplementationGuide.TypeNamespace == 'http://hl7.org/fhir';
            var isFhirFormat = $scope.fhirFormats.indexOf(exportFormat.id) >= 0;
            
            if (isFhirImplementationGuide && !isFhirFormat) {
                return false;
            } else if (!isFhirImplementationGuide && $scope.fhirOnlyFormats.indexOf(exportFormat.id) >= 0) {
                return false;
            }

            // TODO: Check if the user has the securable to export this format

            return true;
        });
    };

    $scope.maximumMembersChanged = function () {
        _.each($scope.valueSets, function (valueSet) {
            valueSet.MaximumMembers = $scope.criteria.maximumMembers;
        });
    };

    $scope.loadTemplates = function () {
        $scope.criteria.selectedTemplates = [];

        ImplementationGuideService.getImplementationGuideTemplates($scope.selectedImplementationGuide.Id, null, $scope.criteria.includeInferred)
            .then(function (templates) {
                _.each(templates, function (template) {
                    $scope.criteria.selectedTemplates.push(template.Id);
                });

                $scope.templates = templates;
            })
            .catch(function (err) {
                $scope.message = err;
            });
    };

    $scope.toggleSelectAllTemplates = function () {
        if ($scope.criteria.selectedTemplates.length == $scope.templates.length) {
            $scope.criteria.selectedTemplates = [];
        } else {
            $scope.criteria.selectedTemplates = [];
            _.each($scope.templates, function (template) {
                $scope.criteria.selectedTemplates.push(template.Id);
            });
        }
    };

    $scope.toggleSelectedTemplate = function (templateId) {
        var index = $scope.criteria.selectedTemplates.indexOf(templateId);

        if (index >= 0) {
            $scope.criteria.selectedTemplates.splice(index, 1);
        } else {
            $scope.criteria.selectedTemplates.push(templateId);
        }
    };

    $scope.loadValueSets = function () {
        ImplementationGuideService.getImplementationGuideValueSets($scope.selectedImplementationGuide.Id, false)
            .then(function (valueSets) {
                $scope.valueSets = valueSets;

                _.each($scope.valueSets, function (valueSet) {
                    valueSet.MaximumMembers = $scope.criteria.maximumMembers;
                });
            })
            .catch(function (err) {
                $scope.message = err;
            });
    };

    $scope.openSearch = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'selectImplementationGuideModal.html',
            controller: 'SearchIGModalCtrl',
            size: 'lg'
        });

        modalInstance.result.then(function (selectedItem) {
            $scope.selectedImplementationGuide = selectedItem;

            // Get the categories for the implementation guide
            ImplementationGuideService.getImplementationGuideCategories($scope.selectedImplementationGuide.Id)
                .then(function (categories) {
                    $scope.categories = categories;
                })
                .catch(function (err) {
                    $scope.message = err;
                });

            $scope.loadValueSets();

            $scope.loadTemplates();
        }, function () {
            // Do nothing when closed without selecting
        });
    };
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