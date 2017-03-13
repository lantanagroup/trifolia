angular.module('Trifolia')
    .directive('treeGrid', function () {
        return {
            restrict: 'E',
            templateUrl: '/Scripts/angular/treegrid/treegrid.html',
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

                $scope.getCellDisplay = function (node, column) {
                    if (node.Constraint) {
                        return node.Constraint[column];
                    }

                    return node[column];
                };

                $scope.isInvalidCardinality = function (node) {
                    if (!node.Constraint) {
                        return false;
                    }

                    var nodeCard = node.Cardinality;
                    var constraintCard = node.Constraint.Cardinality;

                    if (nodeCard.endsWith('..1') && constraintCard.endsWith('..*')) {
                        return 'The schema allows only one, but you have constrained the node in the schema to allow multiple';
                    } else if (nodeCard.endsWith('..0') && !constraintCard.endsWith('..0')) {
                        return 'The schema does not allow any';
                    }
                };

                $scope.getNodeTabs = function (node) {
                    var tabs = '';

                    for (var i = 0; i < node.$level; i++) {
                        tabs += '    ';
                    }

                    return tabs;
                };

                $scope.toggleExpand = function (node) {
                    node.$expanded = !node.$expanded;
                    $scope.onNodeExpanded({ selectedNode: node });
                    $scope.flattenedNodes = getFlattenedNodes();
                };

                $scope.toggleSelect = function (node) {
                    if ($scope.selectedNode != node) {
                        $scope.selectedNode = node;
                    } else {
                        $scope.selectedNode = null;
                    }

                    $scope.onNodeSelected({ selectedNode: $scope.selectedNode });
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