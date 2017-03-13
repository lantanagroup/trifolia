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
    .directive('templateSelect', function ($modal) {
        var TemplateSelectModalController = function ($scope, $modalInstance, TemplateService, caption) {
            $scope.caption = caption;
            $scope.searchText = '';
            $scope.searchResults = null;

            $scope.search = function () {
                TemplateService.getTemplates(100, 1, null, null, $scope.searchText)
                    .then(function (results) {
                        $scope.searchResults = results;
                    });
            };

            $scope.select = function (template) {
                $modalInstance.close();
            };

            $scope.close = function () {
                $modalInstance.dismiss('cancel');
            };
        };

        return {
            restrict: 'E',
            scope: {
                'caption': '@caption',
                'size': '@size'
            },
            templateUrl: 'templateSelect.html',
            link: function ($scope, $element, $attr) {
                $scope.smallFields = $scope.size == 'sm' ? true : false;

                $scope.openModal = function () {
                    var modalInstance = $modal.open({
                        templateUrl: 'templateSelectModal.html',
                        controller: TemplateSelectModalController,
                        size: 'lg',
                        resolve: {
                            caption: function () { return $scope.caption; }
                        }
                    });

                    modalInstance.result.then(function (selectedItem) {
                        // OK
                    }, function () {
                        // Cancel
                    });
                };

                $scope.clearSelection = function () {

                };
            }
        };
    });