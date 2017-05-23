angular.module('Trifolia')
    .directive('ngInclude', function ($http, $templateCache, $sce, $compile) {
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
    })
    .directive('templateSelect', function ($uibModal, $q, TemplateService) {
        return {
            restrict: 'E',
            scope: {
                'templateId': '=templateId',
                'caption': '@caption',
                'size': '@?size',
                'restrictType': '=?restrictType',
                'restrictedType': '=?restrictedType',
                'onChanged': '&?onChanged',
                'formGroup': '=formGroup'
            },
            templateUrl: '/Scripts/angular/templates/templateSelect.html',
            link: function ($scope, $element, $attr) {
                $scope.smallFields = $scope.size == 'sm' ? true : false;
                $scope.selectedTemplate = null;
                $scope.restrictType = $scope.restrictType === 'undefined' ? false : $scope.restrictType;

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

                $scope.openModal = function () {
                    var modalInstance = $uibModal.open({
                        templateUrl: '/Scripts/angular/templates/templateSelectModal.html',
                        controller: function ($scope, $uibModalInstance, TemplateService, caption, restrictType, restrictedType) {
                            $scope.caption = caption;
                            $scope.searchText = '';
                            $scope.searchResults = null;

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

                            $scope.select = function (template) {
                                $uibModalInstance.close(template);
                            };

                            $scope.close = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        },
                        size: 'lg',
                        resolve: {
                            caption: function () { return $scope.caption; },
                            restrictType: function () { return $scope.restrictType },
                            restrictedType: function () { return $scope.restrictedType }
                        }
                    });

                    modalInstance.result.then(function (selectedItem) {
                        $scope.selectedTemplate = selectedItem;
                        $scope.templateSelected(selectedItem);
                    });
                };

                // Watch for the model to change, and update the selected template accordingly
                $scope.$watch('templateId', function (newTemplateId, oldTemplateId) {
                    if (newTemplateId != oldTemplateId) {
                        $scope.initTemplateSelect();
                    }
                });
            }
        };
    });