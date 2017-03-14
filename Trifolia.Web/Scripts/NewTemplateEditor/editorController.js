angular.module('Trifolia')
    .controller('EditorController', function ($scope, $interval, $location, EditorService, ImplementationGuideService, TemplateService) {
        $scope.implementationGuides = [];
        $scope.template = null;
        $scope.constraints = [];
        $scope.nodes = [];
        $scope.selectedNode = null;
        $scope.leftNavExpanded = false;
        $scope.statuses = [
            { Id: 1, Status: 'Draft' },
            { Id: 2, Status: 'Ballot' },
            { Id: 3, Status: 'Published' },
            { Id: 4, Status: 'Deprecated' },
            { Id: 5, Status: 'Retired' }];
        $scope.implementationGuide = null;
        $scope.identifier = {
            base: null,
            ext: null
        }
        $scope.templateSearch = {
            query: '',
            results: null
        };
        $scope.isDebug = true;

        $scope.toggleLeftNav = function () {
            $scope.leftNavExpanded = !$scope.leftNavExpanded;
        };

        $scope.openTemplate = function (templateId, newWindow) {
            if (!newWindow) {
                $scope.init(templateId);
            } else {
                window.open('/TemplateManagement/Edit/Id/' + templateId + '/V2');
            }
        };

        $scope.searchTemplates = function () {
            var implementationGuideId = $scope.template ? $scope.template.OwningImplementationGuideId : null;
            TemplateService.getTemplates(150, 1, null, true, $scope.templateSearch.query, null, null, implementationGuideId, null, null, null, null, false)
                .then(function (results) {
                    $scope.templateSearch.results = results;
                });
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

                    return EditorService.getTemplate(templateId);
                })
                .then(function (template) {
                    $scope.template = template;

                    $scope.searchTemplates();

                    var foundIg = _.find($scope.implementationGuides, function (ig) {
                        return ig.Id == $scope.template.OwningImplementationGuideId;
                    });

                    $scope.implementationGuide = foundIg;

                    // Parse the identifier now that we have the implementation guide and template retrieved
                    $scope.parseIdentifier($scope.template.Oid);

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

        $scope.updateIdentifier = function () {
            if ($scope.template) {
                $scope.template.Oid = $scope.identifier.base + $scope.identifier.ext;
            }
        };

        $scope.parseIdentifier = function (identifier) {
            if (!identifier) {
                $scope.identifier.base = null;
                $scope.identifier.ext = null;
                return;
            }

            var igIdentifier = $scope.implementationGuide ? $scope.implementationGuide.Identifier : null;

            if (igIdentifier.indexOf('http://') == 0 || igIdentifier.indexOf('https://') == 0) {
                if (igIdentifier.lastIndexOf('/') != igIdentifier.length - 1) {
                    igIdentifier += '/';
                }
            } else if (igIdentifier.indexOf('urn:oid:') == 0 && igIdentifier.lastIndexOf('.') != igIdentifier.length - 1) {
                igIdentifier += '.';
            }

            // See if the identifier starts with the base identifier of the implementation guide
            if (igIdentifier && identifier.indexOf(igIdentifier) == 0) {
                $scope.identifier.base = igIdentifier;
                $scope.identifier.ext = identifier.substring(igIdentifier.length);
            } else if (identifier.indexOf('urn:oid:') == 0) {
                $scope.identifier.base = 'urn:oid:';
                $scope.identifier.ext = identifier.substring($scope.identifier.base.length);
            // Look for identifier that starts with urn:oid:
            } else if (identifier.indexOf('urn:hl7ii:') == 0) {
                $scope.identifier.base = 'urn:hl7ii:';
                $scope.identifier.ext = identifier.substring($scope.identifier.base.length);
            // Look for identifier that starts with http://
            } else if (identifier.indexOf('http://') == 0) {
                $scope.identifier.base = 'http://';
                $scope.identifier.ext = identifier.substring($scope.identifier.base.length);
            // Look for identifier that starts with https://
            } else if (identifier.indexOf('https://') == 0) {
                $scope.identifier.base = 'https://';
                $scope.identifier.ext = identifier.substring($scope.identifier.base.length);
            } else {
                $scope.identifier.base = '';
                $scope.identifier.ext = identifier;
            }
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