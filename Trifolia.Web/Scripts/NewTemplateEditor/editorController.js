angular.module('Trifolia')
    .controller('EditorController', function ($scope, $interval, EditorService, ImplementationGuideService) {
        $scope.implementationGuides = [];
        $scope.template = null;
        $scope.constraints = [];
        $scope.nodes = [];
        $scope.selectedNode = null;
        $scope.leftNavExpanded = false;

        $scope.toggleLeftNav = function () {
            $scope.leftNavExpanded = !$scope.leftNavExpanded;
        };

        $scope.nodeExpanded = function (selectedNode) {
            return EditorService.getNodes($scope.template.OwningImplementationGuideId, selectedNode.DataType)
                .then(function (nodes) {
                    if (selectedNode.Constraint) {
                        associateNodes(nodes, selectedNode.Constraint.Children);
                    }

                    selectedNode.Children = nodes;
                });
        };

        $scope.nodeSelected = function (selectedNode) {
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
    });