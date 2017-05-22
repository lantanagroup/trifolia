angular.module('Trifolia')
    .controller('EditorController', function ($scope, $interval, $location, EditorService, ImplementationGuideService, TemplateService) {
        $scope.implementationGuides = [];
        $scope.template = null;
        $scope.constraints = [];
        $scope.nodes = [];
        $scope.selectedNode = null;
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
        $scope.isDebug = true;
        $scope.isFhir = false;

        // TODO
        $scope.showMoveUp = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint) {
                return false;
            }

            return true;
        };

        // TODO
        $scope.moveUp = function (selectedNode) {
            alert('todo');
        };

        // TODO
        $scope.showMoveDown = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint) {
                return false;
            }
            
            return true;
        };

        // TODO
        $scope.moveDown = function (selectedNode) {
            alert('todo');
        };

        // TODO
        $scope.createComputableConstraint = function (selectedNode) {
            // recursively create computable constraints for parent nodes that don't have constraints, first.

            // create a new constraint based on the node selected. initially, data-type should be left empty (representing "Default"), copy the context, cardinality and conformance from the node
            
            alert('todo');
        };

        // TODO
        $scope.createPrimitiveConstraint = function (selectedNode) {
            // selectedNode may be undefined/null. In this case, add a primitive to the top/root level

            // if selectedNode is not undefined/null, create a primtive within the selectedNode

            // if selectedNode does not have a Constraint associated with it yet, then use createComputableConstraint() to create one for it

            alert('todo');
        };

        // TODO
        $scope.duplicateConstraint = function (selectedNode) {
            if (!selectedNode.Constraint) {
                return;
            }

            // create a copy of the node so that a new node shows up in the constraints table. can use angular.copy()

            // create a copy of the constraint. associate it with the copy of the node.

            // update the copy of the constraint to not have the same number. the number should be automatically generated. any customized number should be removed, as well.

            // position the new constraint in the list immediately after the selected constraint

            // set the selectedNode to the duplicate node

            alert('todo');
        };

        // TODO
        $scope.deleteConstraint = function (selectedNode) {
            if (!selectedNode.Constraint) {
                return;
            }

            // if the constraint has not already been saved (doesn't have an Id value), just remove the constraint

            // if the constraint HAS been saved (has an Id value), then flag it as deleted

            alert('todo');
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

                    var foundIg = _.find($scope.implementationGuides, function (ig) {
                        return ig.Id == $scope.template.OwningImplementationGuideId;
                    });

                    $scope.implementationGuide = foundIg;
                    $scope.isFhir = foundIg ? foundIg.Namespace == 'http://hl7.org/fhir' : false;

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

            if (igIdentifier) {
                if (igIdentifier.indexOf('http://') == 0 || igIdentifier.indexOf('https://') == 0) {
                    if (igIdentifier.lastIndexOf('/') != igIdentifier.length - 1) {
                        igIdentifier += '/';
                    }
                } else if (igIdentifier.indexOf('urn:oid:') == 0 && igIdentifier.lastIndexOf('.') != igIdentifier.length - 1) {
                    igIdentifier += '.';
                }
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
    })
    .controller('EditorTemplateSearchController', function ($scope, TemplateService) {
        $scope.templateSearch = {
            query: '',
            results: null
        };

        $scope.openTemplate = function (templateId, newWindow) {
            if (!newWindow) {
                $scope.init(templateId);
            } else {
                window.open('/TemplateManagement/Edit/Id/' + templateId + '/V2');
            }
        };

        $scope.searchTemplates = function () {
            // $scope.template comes from parent scope
            var implementationGuideId = $scope.template ? $scope.template.OwningImplementationGuideId : null;

            var searchOptions = {
                count: 150,
                queryText: $scope.templateSearch.query,
                filterImplementationGuideId: implementationGuideId,
                inferred: false
            };

            TemplateService.getTemplates(searchOptions)
                .then(function (results) {
                    $scope.templateSearch.results = results;
                });
        };

        $scope.$watch('template.OwningImplementationGuideId', function () {
            $scope.searchTemplates();
        });
    });