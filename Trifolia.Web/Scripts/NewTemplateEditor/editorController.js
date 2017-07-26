angular.module('Trifolia')
    .controller('EditorController', function ($scope, $interval, $location, $window, EditorService, ImplementationGuideService, TemplateService) {
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
        $scope.leftNavOpened = false;
        $scope.isDebug = true;
        $scope.isFhir = false;
        $scope.isModified = false;
        $scope.authTimeout = false;
        $scope.message = '';

        /**
         * Check to see if a constraint is a duplicate node within the same level of the tree
         */
        $scope.isDuplicateNode = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
                return false;
            }
            var constraint = selectedNode.Constraint;
            var siblings = constraint.Parent ? constraint.Parent.Children : $scope.constraints;

            var found = _.find(siblings, function (sibling) {
                return constraint.Context === sibling.Context && constraint.Id != sibling.Id;
            });

            return found ? true : false;
        }

        /**
         * Checks to see if there's a possible shift up for the node (duplicate above the node in the tree)
         */
        $scope.showMoveUp = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
                return false;
            }
            var constraint = selectedNode.Constraint;
            var siblings = constraint.Parent ? constraint.Parent.Children : $scope.constraints;
            var index = siblings.indexOf(constraint);

            if (index === -1) return false;

            return index != 0 && siblings.length > 0 && siblings[index - 1].Context === constraint.Context;
        };


        /**
         * Checks to see if there's a possible shift down for the node (duplicate below the node in the tree)
         */
        $scope.showMoveDown = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
                return false;
            }
            var constraint = selectedNode.Constraint;
            var siblings = constraint.Parent ? constraint.Parent.Children : $scope.constraints;
            var index = siblings.indexOf(constraint);

            if (index === -1) return false;

            return index < siblings.length - 1 && siblings.length > 0 && siblings[index + 1].Context === constraint.Context;
        };

        /**
         * Swaps the order of the tree such that the duplicate constraint above the currently examined constraint exchange places
         */
        $scope.moveUp = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
                return;
            }

            var constraint = selectedNode.Constraint;
            var siblings = constraint.IsPrimitive ? constraint.Parent.Children : $scope.constraints;
            var index = siblings.indexOf(constraint);

            if (index != 0 && siblings[index - 1].Context === constraint.Context) {
                //Rearrange the constraints first
                var tmp = siblings[index];
                siblings[index] = siblings[index - 1];
                siblings[index - 1] = tmp;
                
                //Now rearrange the nodes
                var siblingNodes = selectedNode.Parent ? selectedNode.Parent.Children : $scope.nodes;
                var currentNodeIndex = siblingNodes.indexOf(selectedNode);
                var tmpNode = siblingNodes.splice(currentNodeIndex, 1);
                siblingNodes.splice(currentNodeIndex - 1, 0, tmpNode[0]);

                $scope.templateChanged();
            }
        };

        /**
         * Swaps the order of the tree such that the duplicate constraint below the currently examined constraint exchange places
         */
        $scope.moveDown = function (selectedNode) {
            if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
                return false;
            }

            var constraint = selectedNode.Constraint();
            var siblings = constraint.IsPrimitive ? constraint.Parent.Children : $scope.constraints;
            var index = siblings.indexOf(constraint);

            if (index != siblings.length - 1 && siblings[index + 1].Context === constraint.Context) {
                var tmp = siblings[index];
                siblings[index] = siblings[index + 1];
                siblings[index + 1] = tmp;

                var siblingNodes = selectedNode.Parent ? selectedNode.Parent.Children : $scope.nodes;
                var currentNodeIndex = siblingNodes.indexOf(selectedNode);
                var tmpNode = siblingNodes.splice(currentNodeIndex, 1);
                siblingNodes.splice(currentNodeIndex + 1, 0, tmpNode[0]);

                $scope.templateChanged();
            }
        };

        // TODO
        $scope.createComputableConstraint = function (selectedNode) {

            // create a new constraint based on the node selected. initially, data-type should be left empty (representing "Default"), copy the context, cardinality and conformance from the node
            var createDefaultComputable = function (node) {
                var parentConstraint = node.Parent ? node.Parent.Constraint : null;
                var siblingNodes = node.Parent ? node.Parent.Children : $scope.nodes;
                var siblingConstraints = node.Parent && node.Parent.Constraint ? node.Parent.Constraint.Children : $scope.constraints;
                var nodeIndex = siblingNodes.indexOf(node);
                var constraintIndex = 0;

                // Determine where to place the constraint in the list. This is important so that when saving,
                // the list is sent back to the server in the correct order.
                for (var i = nodeIndex - 1; i >= 0; i--) {
                    var siblingConstraint = siblingNodes[i].Constraint;

                    if (siblingConstraint) {
                        constraintIndex = siblingConstraints.indexOf(siblingConstraint) + 1;
                        break;
                    }
                }
                //var constraint = new ConstraintModel(null, parentConstraint, $scope);
                var constraint = {};

                constraint.Context = node.Context;
                constraint.Conformance = node.Conformance;
                constraint.Cardinality = node.Cardinality;
                constraint.isNew = true;
                //constraint.IsChoice = node.IsChoice;
                constraint.IsPrimitive = false;

                // Set as branch identifier if parent is a branch root, default conformance is SHALL
                if (parentConstraint && parentConstraint.IsBranch && constraint.Conformance == "SHALL") {
                    constraint.IsBranchIdentifier = true;
                } else {
                    constraint.IsBranchIdentifier = false
                }

                constraintIndex <= siblingConstraints.length - 1 ? siblingConstraints.splice(constraintIndex, 0, constraint) : siblingConstraints.push(constraint);
                
                node.Constraint = constraint;
                constraint.ConstraintAndProseChanged = true;
                $scope.templateChanged();
                //constraint.SubscribeForUpdates();
            };

            // recursively create computable constraints for parent nodes that don't have constraints, first.
            if (selectedNode.Parent && !selectedNode.Parent.Constraint) {
                $scope.createComputableConstraint(selectedNode.Parent);
            }
            
            createDefaultComputable(selectedNode);
            $scope.templateChanged();
        };

        // TODO
        $scope.createPrimitiveConstraint = function (selectedNode) {
            
            var primitive = {};
            primitive.Constraint = {
                IsPrimitive: true,
                Conformance: "SHALL"
            };

            // if selectedNode does not have a Constraint associated with it yet, then use createComputableConstraint() to create one for it
            if (selectedNode && !selectedNode.Constraint) $scope.createComputableConstraint(selectedNode);

            // selectedNode may be undefined/null. In this case, add a primitive to the top/root level
            if (!selectedNode) {
                $scope.nodes.push(primitive);
                $scope.constraints.push(primitive.Constraint);
            } else {
                if (!selectedNode.HasChildren())
                selectedNode.Children.push(primitive);
                selectedNode.Constraint.Children.push(primitive.Constraint);
            }
        };

        // TODO
        $scope.duplicateConstraint = function (selectedNode) {
            if (!selectedNode.Constraint) {
                return;
            }

            // create a copy of the node so that a new node shows up in the constraints table. can use angular.copy()
            var newNode, newConstraint;
            angular.copy(selectedNode, newNode);
            var siblings = selectedNode.Parent ? selectedNode.Parent.Children : $scope.nodes;
            
            // position the new node in the list immediately after the selected node
            var nodeIndex = siblings.indexOf(selectedNode);
            nodeIndex < siblings.length - 1 ? siblings.splice(nodeIndex, 0, newNode) : siblings.push(newNode);

            // create a copy of the constraint
            angular.copy(selectedNode.Constraint, newConstraint);
            newConstraint.isNew = true;
            var constraint = selectedNode.Constraint;
            var sibConstraints = constraint.Parent ? constraint.Parent.Children : $scope.constraints;
            
            // position the new constraint in the list immediately after the selected constraint
            var constraintIndex = sibConstraints.indexOf(constraint);
            constraintIndex < sibConstraints.length - 1 ? sibConstraints.splice(constraintIndex, 0, newConstraint) : sibConstraints.push(newConstraint);

            // update the copy of the constraint to not have the same number. the number should be automatically generated. any customized number should be removed, as well.
            //TODO

            // set the selectedNode to the duplicate node
            selectedNode = newNode;
        };

        //Not used but potentially useful in the future
        var getParent = function (node) {
            var searchChildren = function (parent) {
                var result;
                var found = false;
                var children = parent.Children;
                for(var i in children){
                    if (children[i].$$hashKey === node.$$hashKey) found = true;
                    if (!found && children[i].Children.length > 0) {
                        result = searchChildren(children[i].Children, node);
                        if(result) break;
                    }
                    else if (found) {
                        result = parent;
                        break;
                    }
                }
                return result;
            };
            var parent;
            //Start with root's children
            for(var n in $scope.nodes){
                if($scope.nodes[n].$$hashKey === node.$$hashKey) break; //do nothing, parent is root node
                parent = searchChildren($scope.nodes[n]);  //else, search children of node
                if (parent) break;
            }
            return parent;
        };

        // TODO
        $scope.deleteConstraint = function (selectedNode) {
            if (!selectedNode.Constraint) {
                return;
            }

            var constraintIndex = $scope.constraints.indexOf(selectedNode.Constraint);

            // if the constraint has not already been saved (doesn't have an Id value), just remove the constraint
            // if the constraint HAS been saved (has an Id value), then flag it as deleted and remove (TODO: How??)
            if (selectedNode.Constraint.Id) {
                if (!$scope.removedConstraints) $scope.removedConstraints = [];
                $scope.removedConstraints.push(constraint);
            }

            if (constraintIndex !== -1) $scope.constraints.splice(constraintIndex, 1);

            delete selectedNode.Constraint;
            $scope.templateChanged();

        };

        // If a message is returned, the node is invalid. If nothing is returned, it is valid.
        $scope.isNodeValid = function (node) {
            if (!node.Constraint) {
                return;
            }

            var invalidChildren = _.filter(node.Children, function (child) {
                return $scope.isNodeValid(child);
            });

            if (invalidChildren.length > 0) {
                return 'A child node/constraint is invalid.';
            }

            var constraint = node.Constraint;

            if (!constraint.Cardinality) {
                return 'The cardinality of a constraint is required';
            }

            var nodeCard = node.Cardinality;
            var constraintCard = node.Constraint.Cardinality;

            if (!/\d+\.\.[\d+|*]/.test(constraintCard)) {
                return 'The format of the cardinality is incorrect. Must be in the form of "X..Y" where X >= 0, X <= Y, and Y can be set to * to represent infinity.';
            }

            var startingNumber = parseInt(constraintCard.substring(0, constraintCard.indexOf('..')));
            var endingNumberString = constraintCard.substring(constraintCard.indexOf('..') + 2);
            var endingNumber = endingNumberString == '*' ? 1000000 : parseInt(endingNumberString);

            if (nodeCard.endsWith('..1') && endingNumber > 1) {
                return 'The schema allows only one, but you have constrained the node in the schema to allow multiple';
            } else if (nodeCard.endsWith('..0') && !constraintCard.endsWith('..0')) {
                return 'The schema does not allow any';
            } else if (startingNumber > endingNumber) {
                return 'The starting cardinality cannot be greater than the ending cardinality.';
            }
        };

        $scope.isValid = function () {
            if (!$scope.template) {
                return false;
            }

            if (!$scope.template.Name) {
                return false;
            }

            if (!$scope.template.Oid) {
                return false;
            }

            // TODO: Other template metadata tests
            
            var isAllNodesValid = function (node) {
                if (!node.$$valid) {
                    return false;
                }

                for (var i = 0; i < node.Children.length; i++) {
                    if (!isAllNodesValid(node.Children[i])) {
                        return false;
                    }
                }

                return true;
            };

            for (var i = 0; i < $scope.nodes.length; i++) {
                if (!isAllNodesValid($scope.nodes[i])) {
                    return false;
                }
            }

            return true;
        };

        $scope.save = function () {
            if (!$scope.isValid()) {
                return;
            }

            var data = {
                Template: $scope.template,
                RemovedConstraints: $scope.removedConstraints,
                Constraints: $scope.constraints
            };

            var removeIdFromNewConstraints = function (list) {
                _.forEach(list, function (constraint) {
                    if (constraint.IsNew) {
                        constraint.Id = null;
                    }

                    removeIdFromNewConstraints(constraint.Children);
                });
            };

            removeIdFromNewConstraints(data.Constraints);

            // Recursively updates the constraints in currentList (and its children)
            // based on the newList. newList is plain JS objects, not KO'd objects.
            var updateConstraints = function (currentList, newList) {
                if (currentList.length != newList.length) {
                    throw "Updating client-side constraints failed. Server and client mis-match.";
                }

                for (var i in currentList) {
                    var currentConstraint = currentList[i];
                    var newConstraint = newList[i];

                    currentConstraint.Id = newConstraint.Id;
                    currentConstraint.Number = newConstraint.Number;
                    currentConstraint.IsNew = false;

                    updateConstraints(currentConstraint.Children, newConstraint.Children);
                }
            };

            $.blockUI({ message: "Saving..." });
            $.ajax({
                method: 'POST',
                url: '/api/Template/Edit/Save',
                data: JSON.stringify(data),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    if (response.Error) {
                        alert(response.Error);
                    } else if (response.TemplateId) {
                        // Update the template id on the client so that we save against the new template going forward
                        $scope.TemplateId = response.TemplateId;
                        $scope.Template.Id = response.TemplateId;
                        $scope.Template.AuthorId = response.AuthorId;
                        $scope.isModified = false;

                        if (actionAfter == 'list') {
                            $scope.Status = "Done saving... Redirecting to Browse Templates.";
                            location.href = '/TemplateManagement/List';
                        } else if (actionAfter == 'view') {
                            $scope.Status = "Done saving... Redirecting to View Template.";
                            location.href = getTemplateViewUrl($scope.Template.Id, $scope.Template.Oid);
                        } else if (actionAfter == 'publishSettings') {
                            $scope.Status("Done saving... Redirecting to Publish Settings.");
                            location.href = '/TemplateManagement/PublishSettings?id=' + $scope.TemplateId;
                        } else {
                            self.Status("Done saving.");

                            // Empty the removed constraints list
                            self.RemovedConstraints = [];

                            // Update the constraints on the client (possibly new IDs and new Numbers)
                            updateConstraints($scope.Constraints, response.Constraints);
                            angular.extend($scope.Template, { ValidationResults: response.ValidationResults });

                            // Clear the status after 10 seconds
                            setTimeout(function () {
                                $scope.Status = "";
                            }, 10000);
                        }
                    }
                },
                complete: function () {
                    $.unblockUI();
                }
            });

        };

        $scope.discard = function () {
            $scope.init($scope.template.Id);
        };

        $scope.unlock = function () {
            $scope.isLocked = false;
        };

        $scope.nodeExpanded = function (selectedNode) {
            if (selectedNode.Children.length === 0) {
                return EditorService.getNodes($scope.template.OwningImplementationGuideId, selectedNode.DataType)
                    .then(function (nodes) {
                        if (selectedNode.Constraint) {
                            associateNodes(nodes, selectedNode.Constraint.Children);
                        }

                        selectedNode.Children = nodes;
                    });
            }
        };

        $scope.nodeSelected = function (selectedNode) {
            $scope.selectedNode = selectedNode;
        };

        $scope.init = function (templateId, defaultImplementationGuideId) {
            $scope.selectedNode = null;
            $scope.isModified = false;
            $scope.nodes = [];
            $scope.constraints = [];
            $scope.isLocked = false;

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
                    $scope.isLocked = template.Locked;

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
                })
                .catch($scope.handleHttpError);
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

        $scope.templateChanged = function () {
            $scope.isModified = true;
        };

        $scope.reload = function () {
            $window.location.reload();
        };

        $scope.handleHttpError = function (err) {
            $scope.leftNavOpened = false;

            if (err.status == 401) {
                $scope.authTimeout = true;
            } else if (err.data && typeof err.data === 'string') {
                $scope.message = err.data;
            } else if (err.message) {
                $scope.message = err.message;
            } else if (typeof err === 'string') {
                $scope.message = err;
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
                })
                .catch($scope.handleHttpError);
        };

        $scope.$watch('template.OwningImplementationGuideId', function () {
            $scope.searchTemplates();
        });
    });