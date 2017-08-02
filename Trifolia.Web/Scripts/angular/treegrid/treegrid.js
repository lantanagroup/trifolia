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
                onNodeExpanded: '&nodeExpanded',
                onSearchConstraint: '&searchConstraint',
                validateNode: '&validateNode'
            },
            link: function ($scope, $element, $attributes) {
                $scope.flattenedNodes = [];
                $scope.selectedNode = null;
                $scope.numberSearch = null;

                function numberSearchChanged() {
                    $scope.onSearchConstraint({ number: $scope.numberSearch });
                };

                function getCellDisplay(node, column) {
                    if (node.Constraint) {
                        return node.Constraint[column];
                    }

                    return node[column];
                };

                function getNodeTabs(node) {
                    var tabs = '';

                    for (var i = 0; i < node.$level; i++) {
                        tabs += '    ';
                    }

                    return tabs;
                };

                function toggleExpand(node, expanded) {
                    node.$expanded = expanded || !node.$expanded;
                    $scope.onNodeExpanded({ selectedNode: node });
                    $scope.flattenedNodes = getFlattenedNodes();
                };

                function toggleSelect(node) {
                    if ($scope.selectedNode != node) {
                        $scope.selectedNode = node;
                    } else {
                        $scope.selectedNode = null;
                    }

                    $scope.onNodeSelected({ selectedNode: $scope.selectedNode });
                };

                function isNodeValid(node) {
                    var message = $scope.validateNode({ node: node });

                    if (message) {
                        node.$$valid = false;
                        node.$$message = message;
                    } else {
                        node.$$valid = true;
                    }

                    return node.$$valid;
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

                $scope.numberSearchChanged = numberSearchChanged;
                $scope.getCellDisplay = getCellDisplay;
                $scope.getNodeTabs = getNodeTabs;
                $scope.toggleExpand = toggleExpand;
                $scope.toggleSelect = toggleSelect;
                $scope.isNodeValid = isNodeValid;

                $scope.$watch('nodes', function () {
                    $scope.flattenedNodes = getFlattenedNodes();
                }, true);

                $scope.$on('selectNode', function (event, node) {
                    $scope.selectedNode = node;
                    $scope.onNodeSelected({ selectedNode: node });
                });

                $scope.$on('expandNode', function (event, node) {
                    $scope.toggleExpand(node, true);
                });
            }
        };
    });