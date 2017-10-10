angular.module('Trifolia').directive('ngInclude', function ($http, $templateCache, $sce, $compile) {
    return {
        restrict: 'ECA',
        priority: 1000,
        link: function (scope, $element, $attr) {
            if ($attr.replace !== undefined) {
                var theSrc = $attr.src || $attr.ngInclude;
                scope.$watch($sce.parseAsResourceUrl(theSrc), function ngIncludeWatchAction(src) {
                    if (src) {
                        $http.get(src, { cache: $templateCache }).success(function (response) {
                            var e = $compile(response)(scope);
                            $element.replaceWith(e);
                        });
                    }
                });
            }
        }
    };
});

angular.module('Trifolia').directive('templateSelect', function ($uibModal, $q, TemplateService) {
    return {
        restrict: 'E',
        scope: {
            'templateId': '=templateId',
            'caption': '@caption',
            'captionAsLabel': '=?captionAsLabel',
            'size': '@?size',
            'restrictType': '=?restrictType',
            'restrictedType': '=?restrictedType',
            'onChanged': '&?onChanged',
            'formGroup': '=formGroup',
            'allowNew': '=?allowNew',
            'implementationGuideId': '=?implementationGuideId'
        },
        templateUrl: '/Scripts/angular/templates/templateSelect.html',
        link: function ($scope, $element, $attr) {
            $scope.smallFields = $scope.size === 'sm' ? true : false;
            $scope.selectedTemplate = null;
            $scope.restrictType = $scope.restrictType === 'undefined' ? false : $scope.restrictType;
            $scope.allowNew = $scope.allowNew === 'undefined' ? false : $scope.allowNew === true;
            $scope.captionAsLabel = $scope.captionAsLabel === 'undefined' ? false : $scope.captionAsLabel === true;

            $scope.searchTemplates = function (query) {
                var deferred = $q.defer();

                var searchOptions = {
                    count: 10,
                    queryText: query
                };

                if ($scope.restrictType && $scope.restrictedType) {
                    searchOptions.filterContextType = $scope.restrictedType;
                }

                TemplateService.getTemplates(searchOptions)
                    .then(function (results) {
                        deferred.resolve(results.Items);
                    })
                    .catch(deferred.reject);

                return deferred.promise;
            };

            $scope.templateSelected = function (selectedTemplate) {
                if (selectedTemplate) {
                    $scope.templateId = selectedTemplate.Id;
                } else {
                    $scope.templateId = null;
                }

                $scope.onChanged();
            };

            $scope.initTemplateSelect = function () {
                // If id already set, get template info to display
                if ($scope.templateId) {
                    TemplateService.getTemplate($scope.templateId)
                        .then(function (template) {
                            $scope.selectedTemplate = template;
                        })
                        .catch(function (err) {
                            console.log('Error getting information for the selected template for field: ' + $scope.caption);
                        });
                } else {
                    $scope.selectedTemplate = null;
                }
            };

            $scope.clearSelection = function () {
                $scope.selectedTemplate = null;
                $scope.templateSelected(null);
            };

            $scope.openNew = function () {
                var modalInstance = $uibModal.open({
                    templateUrl: '/Scripts/angular/templates/newTemplateModal.html',
                    controller: 'NewTemplateModalCtrl',
                    size: 'lg',
                    resolve: {
                        implementationGuideId: function () { return $scope.implementationGuideId; },
                        restrictType: function () { return $scope.restrictType; },
                        restrictedType: function () { return $scope.restrictedType; }
                    }
                });

                modalInstance.result.then(function (selectedItem) {
                    $scope.selectedTemplate = selectedItem;
                    $scope.templateSelected(selectedItem);
                });
            };

            $scope.openModal = function () {
                var modalInstance = $uibModal.open({
                    templateUrl: '/Scripts/angular/templates/templateSelectModal.html',
                    controller: 'SelectTemplateModalCtrl',
                    size: 'lg',
                    resolve: {
                        caption: function () { return $scope.caption; },
                        restrictType: function () { return $scope.restrictType },
                        restrictedType: function () { return $scope.restrictedType },
                        selectMultiple: function () { return false; },
                        selectedTemplates: function () { return null; }
                    }
                });

                modalInstance.result.then(function (selectedItem) {
                    $scope.selectedTemplate = selectedItem;
                    $scope.templateSelected(selectedItem);
                });
            };

            // Watch for the model to change, and update the selected template accordingly
            $scope.$watch('templateId', function (newTemplateId, oldTemplateId) {
                if (newTemplateId !== oldTemplateId) {
                    $scope.initTemplateSelect();
                }
            });
        }
    };
});

angular.module('Trifolia').directive('multipleTemplateSelect', function ($uibModal, $q, TemplateService) {
    return {
        restrict: 'E',
        scope: {
            'selectedTemplateIds': '=templateIds',
            'caption': '@caption',
            'restrictType': '=?restrictType',
            'restrictedType': '=?restrictedType',
            'formGroup': '=formGroup',
            'implementationGuideId': '=?implementationGuideId',
            'onChanged': '&?onChanged'
        },
        templateUrl: '/Scripts/angular/templates/multipleTemplateSelect.html',
        link: function ($scope, $element, $attr) {
            $scope.selectedTemplates = [];
            $scope.restrictType = $scope.restrictType === 'undefined' ? false : $scope.restrictType;

            var changingTemplateIds = false;

            if (!($scope.selectedTemplateIds instanceof Array)) {
                $scope.selectedTemplateIds = [];
            }

            $scope.selectedTemplatesChanged = function () {
                changingTemplateIds = true;

                for (var i in $scope.selectedTemplates) {
                    var templateId = $scope.selectedTemplates[i].Id;

                    if ($scope.selectedTemplateIds.indexOf(templateId) < 0) {
                        $scope.selectedTemplateIds.push(templateId);
                    }
                }

                $scope.onChanged();
                changingTemplateIds = false;
            };

            $scope.initTemplateSelect = function () {
                // Find selectedTemplateIds not in the selectedTemplates list and request a retrieve from the server
                var promises = _.chain($scope.selectedTemplateIds)
                    .map(function (templateId) {
                        return TemplateService.getTemplate(templateId);
                    })
                    .value();

                $q.all(promises)
                    .then(function (templates) {
                        $scope.selectedTemplates = templates;
                    })
                    .catch(function (err) {
                        console.log('Error getting information for the selected template for field: ' + $scope.caption);
                    });
            };

            $scope.removeSelectedTemplate = function (template) {
                var selectedTemplatesIndex = $scope.selectedTemplates.indexOf(template);
                var templateIdsIndex = $scope.selectedTemplateIds.indexOf(template.Id);

                $scope.selectedTemplates.splice(selectedTemplatesIndex, 1);

                changingTemplateIds = true;
                $scope.selectedTemplateIds.splice(templateIdsIndex, 1);
                changingTemplateIds = false;

                $scope.onChanged();
            };

            $scope.openModal = function () {
                var modalInstance = $uibModal.open({
                    templateUrl: '/Scripts/angular/templates/templateSelectModal.html',
                    controller: 'SelectTemplateModalCtrl',
                    size: 'lg',
                    resolve: {
                        caption: function () { return $scope.caption; },
                        restrictType: function () { return $scope.restrictType },
                        restrictedType: function () { return $scope.restrictedType },
                        selectMultiple: function () { return true; },
                        selectedTemplates: function () { return $scope.selectedTemplates; }
                    }
                });

                modalInstance.result.then(function (selectedItems) {
                    var changed = false;

                    _.each(selectedItems, function (selectedItem) {
                        var found = _.find($scope.selectedTemplates, function (next) {
                            return next.Id == selectedItem.Id;
                        });

                        if (!found) {
                            changed = true;
                            $scope.selectedTemplates.push(selectedItem);
                        }
                    });

                    for (var i = $scope.selectedTemplates.length - 1; i >= 0; i--) {
                        var found = _.find(selectedItems, function (next) {
                            return next.Id == $scope.selectedTemplates[i].Id;
                        });

                        if (!found) {
                            changed = true;
                            $scope.selectedTemplates.splice(i, 1);
                        }
                    }

                    if (changed) {
                        $scope.selectedTemplatesChanged();
                    }
                });
            };

            $scope.$watch('selectedTemplateIds', function (newTemplateIds, oldTemplateIds) {
                if (!changingTemplateIds) {
                    $scope.initTemplateSelect();
                }
            }, true);
        }
    };
});

angular.module('Trifolia').controller('SelectTemplateModalCtrl', function ($scope, $uibModalInstance, TemplateService, caption, restrictType, restrictedType, selectMultiple, selectedTemplates) {
    $scope.caption = caption;
    $scope.searchText = '';
    $scope.searchResults = null;
    $scope.selectMultiple = selectMultiple;
    $scope.selectedTemplates = selectedTemplates instanceof Array ? angular.copy(selectedTemplates) : [];

    $scope.search = function () {
        var searchOptions = {
            count: 100,
            queryText: $scope.searchText
        };

        if (restrictType && restrictedType) {
            searchOptions.filterContextType = restrictedType;
        }

        TemplateService.getTemplates(searchOptions)
            .then(function (results) {
                $scope.searchResults = results;
            });
    };

    $scope.getTemplateDisplayName = function (template) {
        var name = template.Name;

        if (!name) {
            return '';
        }

        if (name.length > 40) {
            return name.substring(0, 40) + '...';
        }

        return name;
    };

    $scope.selectTemplate = function (template) {
        var found = _.find($scope.selectedTemplates, function (next) {
            return next.Id == template.Id;
        });

        if (found) {
            var index = $scope.selectedTemplates.indexOf(found);
            $scope.selectedTemplates.splice(index, 1);
        } else {
            $scope.selectedTemplates.push(template);
        }
    };

    $scope.isTemplateSelected = function (template) {
        var found = _.find($scope.selectedTemplates, function (next) {
            return next.Id == template.Id;
        });

        return !!found;
    };

    $scope.select = function (template) {
        if ($scope.selectMultiple) {
            $uibModalInstance.close($scope.selectedTemplates);
        } else {
            $uibModalInstance.close(template);
        }
    };

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

angular.module('Trifolia').controller('NewTemplateModalCtrl', function ($scope, $uibModalInstance, $uibModal, ImplementationGuideService, implementationGuideId, restrictType, restrictedType) {
    $scope.template = {
        Name: '',
        Oid: '',
        Bookmark: '',
        IsOpen: true,
        StatusId: null,
        Description: '',
        Notes: '',
        OwningImplementationGuideId: implementationGuideId,
        TemplateTypeId: null,
        ImpliedTemplateId: null,
        PrimaryContextType: '',
        PrimaryContext: ''
    };
    $scope.identifier = {
        base: '',
        ext: ''
    };
    $scope.statuses = [];
    $scope.implementationGuides = [];
    $scope.implementationGuide = null;
    $scope.templateTypes = [];
    $scope.templateType = null;
    $scope.restrictedType = restrictedType;

    $scope.identifierChanged = function () {

    };

    $scope.implementationGuideChanged = function () {
        if (!$scope.template.OwningImplementationGuideId) {
            $scope.implementationGuide = null;
        } else {
            $scope.implementationGuide = _.find($scope.implementationGuides, function (implementationGuide) {
                return implementationGuide.Id === $scope.template.OwningImplementationGuideId;
            });

            // Load template types
            ImplementationGuideService.getTemplateTypes($scope.template.OwningImplementationGuideId)
                .then(function (templateTypes) {
                    $scope.templateTypes = templateTypes;
                })
                .catch(function (err) {
                    // TODO
                });
        }
    };

    $scope.templateTypeChanged = function () {
        if (!$scope.template.TemplateTypeId) {
            $scope.templateType = null;
        } else {
            $scope.templateType = _.find($scope.templateTypes, function (templateType) {
                return templateType.Id === $scope.template.TemplateTypeId;
            });

            if ($scope.templateType) {
                $scope.template.PrimaryContext = $scope.templateType.RootContext;
                $scope.template.PrimaryContextType = $scope.templateType.RootContextType;
            } else {
                $scope.template.PrimaryContext = '';
                $scope.template.PrimaryContextType = '';
            }
        }
    };

    $scope.isTypeValid = function () {
        return !restrictType || ($scope.templateType && $scope.templateType.RootContextType === restrictedType);
    };

    $scope.isValid = function () {
        if (!$scope.isTypeValid()) {
            return false;
        }

        // TODO
        return false;
    };

    $scope.openAppliesTo = function () {
        var modalInstance = $uibModal.open({
            templateUrl: '/Scripts/angular/templates/appliesToModal.html',
            controller: 'AppliesToModalCtrl',
            size: 'lg',
            resolve: {
                baseType: function () { return $scope.templateType.RootContextType }
            }
        });

        modalInstance.result.then(function (selectedItem) {
        });
    };

    $scope.ok = function () {
        // TODO
        // Save the template
        // Return the template when closing the modal
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('close');
    };

    $scope.init = function () {
        // Load all editable implementation guides
        ImplementationGuideService.getEditable($scope.template.OwningImplementationGuideId)
            .then(function (implementationGuides) {
                $scope.implementationGuides = implementationGuides;

                // Find the implementation guide that is pre-selected
                $scope.implementationGuide = _.find(implementationGuides, function (implementationGuide) {
                    return implementationGuide.Id === $scope.template.OwningImplementationGuideId;
                });

                $scope.implementationGuideChanged();
            })
            .catch(function (err) {
                // TODO
            });
    };
});

angular.module('Trifolia').controller('AppliesToModalCtrl', ['$scope', '$uibModalInstance', 'EditorService', 'implementationGuideId', 'baseType', function ($scope, $uibModalInstance, EditorService, implementationGuideId, baseType) {
    $scope.message = '';
    $scope.nodes = [];
    $scope.flattenedNodes = [];

    function select(node) {
        $uibModalInstance.close({
            primaryContext: node.Context,
            primaryContextType: node.DataType
        });
    };

    function cancel() {
        $uibModalInstance.dismiss('close');
    };

    function flattenNodes(parent, level) {
        var list = !parent ? $scope.nodes : parent.Children;

        _.each(list, function (node) {
            node.$whitespace = '';
            for (var i = 0; i < level; i++) {
                node.$whitespace += '    ';
            }

            $scope.flattenedNodes.push(node);

            if (node.$expanded) {
                flattenNodes(node, level + 1);
            }
        });
    };

    function toggleNode(node) {
        if (!node.$loaded) {
            EditorService.getNodes(implementationGuideId, node.DataType)
                .then(function (nodes) {
                    node.Children = nodes;
                    node.$loaded = true;

                    node.$expanded = !node.$expanded;
                    $scope.flattenedNodes = [];
                    flattenNodes(null, 0);
                })
                .catch(function (err) {
                    console.log(err);
                });
        } else {
            node.$expanded = !node.$expanded;
            $scope.flattenedNodes = [];
            flattenNodes(null, 0);
        }
    };

    function init() {
        EditorService.getNodes(implementationGuideId, baseType)
            .then(function (nodes) {
                $scope.nodes = nodes;
                $scope.flattenedNodes = [];
                flattenNodes(null, 0);
            })
            .catch(function (err) {
                console.log(err);
            });
    };

    $scope.select = select;
    $scope.cancel = cancel;
    $scope.init = init;
    $scope.toggleNode = toggleNode;
}]);