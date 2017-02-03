angular.module('NewEditor', ['ui.bootstrap', 'igniteui-directives'])
    .run(function ($templateCache) {
        // <script type="text/ng-template"> ... is preferred, but VS 2012 doesn't give intellisense there
        angular.element('script[type="text/html"]').each(function(idx, el) {
            $templateCache.put(el.id, el.innerHTML);
        });
    })
    .filter('implementationGuideName', function (ImplementationGuideService) {
        var implementationGuides = null;
        var isLoading = false;

        implementationGuideNameFilter.$stateful = true;

        function implementationGuideNameFilter(input) {
            if (implementationGuides == null) {
                if (!isLoading) {
                    isLoading = true;
                    ImplementationGuideService.getAll()
                        .then(function (results) {
                            implementationGuides = results.Items;
                            isLoading = false;
                        });
                }
                return 'Loading...';
            } else {
                var foundImplementationGuide = _.find(implementationGuides, function (implementationGuide) {
                    return implementationGuide.Id == input;
                });

                if (foundImplementationGuide) {
                    return foundImplementationGuide.Title;
                }
            }

            return 'Not found';
        };

        return implementationGuideNameFilter;
    })
    .controller('EditorController', function ($scope, $interval, EditorService, ImplementationGuideService) {
        $scope.implementationGuides = [];
        $scope.template = null;
        $scope.constraints = [];
        $scope.nodes = [];
        $scope.selectedNode = null;

        $scope.nodeExpanded = function (node) {
            return EditorService.getNodes($scope.template.OwningImplementationGuideId, node.DataType)
                .then(function (nodes) {
                    if (node.Constraint) {
                        associateNodes(nodes, node.Constraint.Children);
                    }

                    node.Children = nodes;
                });
        };

        $scope.treeGridNodeSelected = function (selectedNode) {
            $scope.selectedNode = selectedNode;
        };

        $scope.init = function (templateId, defaultImplementationGuideId) {
            ImplementationGuideService.getEditable()
                .then(function (implementationGuides) {
                    $scope.implementationGuides = implementationGuides;
                });

            EditorService.getTemplate(templateId)
                .then(function (template) {
                    $scope.template = template;

                    return EditorService.getConstraints(templateId);
                })
                .then(function (constraints) {
                    $scope.constraints = constraints;

                    return EditorService.getNodes($scope.template.OwningImplementationGuideId, $scope.template.PrimaryContextType)
                        .then(function (nodes) {
                            associateNodes(nodes, $scope.constraints);
                            $scope.nodes = nodes;
                        });
                });
        };

        var associateNodes = function (nodes, constraints) {
            for (var x = 0; x < nodes.length; x++) {
                var node = nodes[x];

                var foundConstraints = _.filter(constraints, function (constraint) {
                    return constraint.Context == node.Context;
                });

                if (foundConstraints.length > 0) {
                    node.Constraint = foundConstraints[0];

                    for (var i = 1; i < foundConstraints.length; i++) {
                        var nodeClone = JSON.parse(JSON.stringify(node));
                        nodeClone.Constraint = foundConstraints[i];
                        nodes.splice(x + 1, 0, nodeClone);
                        x++;
                    }
                }
            }
        };
    })
    .factory('EditorService', function ($http, $q) {
        var service = {};

        service.getConstraints = function (templateId) {
            if (!templateId) {
                return $q.resolve([]);
            }

            var deferred = $q.defer();
            var url = '/api/Template/Edit/' + templateId + '/Constraint';

            $http.get(url)
                .then(function (results) {
                    deferred.resolve(results.data);
                }, function (err) {
                    alert('Error getting constraints');
                    console.log(err);
                });

            return deferred.promise;
        };

        service.getTemplate = function (templateId) {
            if (!templateId) {
                return $q.resolve(new TemplateModel());
            }

            var deferred = $q.defer();
            var url = '/api/Template/Edit/' + templateId + '/MetaData';

            $http.get(url)
                .then(function (results) {
                    deferred.resolve(new TemplateModel(results.data));
                }, function (err) {
                    alert('Error getting template');
                    console.log(err);
                });

            return deferred.promise;
        };

        service.getNodes = function (implementationGuideId, parentType) {
            var deferred = $q.defer();
            var url = '/api/Template/Edit/Schema/' + implementationGuideId + '?';

            if (parentType) {
                url += 'parentType=' + encodeURIComponent(parentType);
            }

            $http.get(url)
                .then(function (results) {
                    var nodes = [];

                    for (var i in results.data) {
                        nodes.push(new NodeModel(results.data[i]));
                    }

                    deferred.resolve(nodes);
                }, function (err) {
                    alert('Error getting nodes');
                    console.log(err);
                });

            return deferred.promise;
        };

        return service;
    })
    .factory('ImplementationGuideService', function ($http, $q) {
        var service = {};

        service.getAll = function () {
            var deferred = $q.defer();

            $http.get('/api/ImplementationGuide')
                .then(function (results) {
                    deferred.resolve(results.data);
                }, function (err) {
                    alert('Errror retrieving list of implementation guides');
                    console.log(err);
                });

            return deferred.promise;
        };

        service.getEditable = function () {
            var deferred = $q.defer();

            $http.get('/api/ImplementationGuide/Editable')
                .then(function (results) {
                    // TODO: Filter out non-published IGs, unless they are from the same implementation guide as the IG
                    var filtered = _.filter(results.data, function (implementationGuide) {
                        return !implementationGuide.IsPublished;
                    });

                    deferred.resolve(filtered);
                }, function (err) {
                    alert('Errror retrieving list of editable implementation guides');
                    console.log(err);
                });

            return deferred.promise;
        };

        return service;
    })
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
    .directive('treeGrid', function (EditorService) {
        return {
            restrict: 'E',
            templateUrl: 'treeGrid.html',
            scope: {
                nodes: '=nodes',
                template: '=template',
                constraints: '=constraints',
                onNodeSelected: '&nodeSelected',
                onNodeExpanded: '&nodeExpanded'
            },
            link: function ($scope, $element, $attributes) {
                $scope.flattenedNodes = [];
                $scope.selectedNode = null;

                $scope.getNodeTabs = function (node) {
                    var tabs = '';

                    for (var i = 0; i < node.$level; i++) {
                        tabs += '    ';
                    }

                    return tabs;
                };

                $scope.toggleExpand = function (node) {
                    node.$expanded = !node.$expanded;
                    $scope.onNodeExpanded({ node });
                    $scope.flattenedNodes = getFlattenedNodes();
                };

                $scope.select = function (node) {
                    $scope.selectedNode = node;
                    $scope.onNodeSelected({ selectedNode: node });
                };

                var getFlattenedNodes = function () {
                    var flattenNodes = function (flattened, parent, level) {
                        for (var i = 0; i < parent.Children.length; i++) {
                            var node = parent.Children[i];

                            node.$level = level;
                            flattened.push(node);

                            if (node.$expanded) {
                                flattenNodes(flattened, node, level + 1);
                            }
                        }
                    };

                    var flattened = [];
                    flattenNodes(flattened, { Children: $scope.nodes }, 0);
                    return flattened;
                };

                $scope.$watch('nodes', function () {
                    $scope.flattenedNodes = getFlattenedNodes();
                }, true);
            }
        };
    });

var TemplateModel = function (data) {
    var self = this;

    self.Id = null;
    self.Name = null;

    angular.extend(self, data);
};

var NodeModel = function (data) {
    var self = this;

    self.Cardinality = '';
    self.Conformance = '';
    self.Context = '';
    self.DataType = '';
    self.HasChildren = false;
    self.IsChoice = false;
    self.Children = [];

    // Constraint Properties
    self.Number = '';
    self.Value = '';

    angular.extend(self, data);
};