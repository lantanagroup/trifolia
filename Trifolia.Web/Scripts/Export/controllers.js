angular.module('Trifolia').controller('ExportCtrl', function ($scope, $uibModal, ImplementationGuideService) {
    $scope.selectedImplementationGuide = null;
    $scope.message = '';
    $scope.exportFormats = [{
        id: 'MSW',
        name: 'Microsoft Word (DOCX)'
    }, {
        id: 'HTML',
        name: 'Web (HTML)'
    }, {
        id: 'SNAP',
        name: 'Snapshot (JSON)'
    }, {
        id: 'XML',
        name: 'Native (XML)'
    }, {
        id: 'FHIR_XML',
        name: 'FHIR Bundle (XML)'
    }, {
        id: 'FHIR_BUILD_XML',
        name: 'FHIR Build Package (XML)'
    }, {
        id: 'FHIR_BUILD_JSON',
        name: 'FHIR Build Package (JSON)'
    }, {
        id: 'TEMPLATES',
        name: 'Templates DSTU (XML)'
    }, {
        id: 'SCH',
        name: 'Schematron (SCH)'
    }, {
        id: 'VOC_XLSX',
        name: 'Vocabulary (XLSX)'
    }, {
        id: 'VOC_XML',
        name: 'Vocabulary (Native XML)'
    }, {
        id: 'VOC_SINGLE_SVS',
        name: 'Vocbulary (Single SVS XML)'
    }, {
        id: 'VOC_MULTI_SVS',
        name: 'Vocabulary (Multiple SVS XML)'
    }, {
        id: 'VOC_FHIR_XML',
        name: 'Vocbulary Bundle (FHIR XML)'
    }];
    $scope.templateSelectionFormats = ['MSW', 'SNAP', 'XML', 'TEMPLATES', 'SCH'];
    $scope.xmlFormats = ['XML', 'TEMPLATES', 'FHIR_XML', 'FHIR_BUILD_XML', 'FHIR_BUILD_JSON', 'SNAP'];
    $scope.categorySelectionFormats = $scope.xmlFormats.concat(['MSW', 'SCH']);
    $scope.vocFormats = ['VOC_XML', 'VOC_SINGLE_SVS', 'VOC_MULTI_SVS', 'VOC_FHIR_XML'];
    $scope.fhirFormats = $scope.vocFormats.concat(['MSW', 'HTML', 'SNAP', 'XML', 'FHIR_XML', 'FHIR_BUILD_XML', 'FHIR_BUILD_JSON']);
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

    $scope.getPostUrl = function () {
        switch ($scope.criteria.selectedExportFormat) {
            case 'MSW':
                return '/api/Export/MSWord';
            case 'HTML':
                return '/api/Export/HTML';      // TODO
            case 'SNAP':
                return '/api/Export/Snapshot';  // TODO (move from IGController)
            case 'XML':
                return '/api/Export/Trifolia';
            case 'FHIR_XML':
            case 'FHIR_BUILD_XML':
            case 'FHIR_BUILD_JSON':
            case 'TEMPLATES':
                return '/api/Export/XML';
            case 'SCH':
                return '/api/Export/Schematron';
            case 'VOC_XLSX':
            case 'VOC_XML':
            case 'VOC_SINGLE_SVS':
            case 'VOC_MULTI_SVS':
            case 'VOC_FHIR_XML':
                return '/api/Export/Vocabulary';
        }
    };

    $scope.getExportFormats = function() {
        return _.filter($scope.exportFormats, function (exportFormat) {
            if ($scope.selectedImplementationGuide && $scope.selectedImplementationGuide.TypeNamespace == 'http://hl7.org/fhir') {
                if ($scope.fhirFormats.indexOf(exportFormat.id) < 0) {
                    return false;
                }
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