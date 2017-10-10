angular.module('Trifolia').controller('EditorController', function ($scope, $interval, $location, $window, $sce, $q, $uibModal, EditorService, ImplementationGuideService, TemplateService, blockUI) {
    $scope.implementationGuides = [];
    $scope.template = null;
    $scope.constraints = [];
    $scope.nodes = [];
    $scope.selectedNode = null;
    $scope.activeTab = 'metadata';
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
    $scope.removedConstraints = [];
    $scope.saving = false;
    $scope.permissions = [];

    /**
     * Check to see if a constraint is a duplicate node within the same level of the tree
     * @param {any} selectedNode
     */
    function isDuplicateNode(selectedNode) {
        if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
            return false;
        }
        var constraint = selectedNode.Constraint;
        var siblings = constraint.Parent ? constraint.Parent.Children : $scope.constraints;

        var found = _.find(siblings, function (sibling) {
            return constraint.Context === sibling.Context && constraint.Id != sibling.Id;
        });

        return found ? true : false;
    };

    /**
     * Checks to see if there's a possible shift up for the node (duplicate above the node in the tree)
     * @param {any} selectedNode
     */
    function showMoveUp(selectedNode) {
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
    function showMoveDown(selectedNode) {
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
     * @param {any} selectedNode
     */
    function moveUp(selectedNode) {
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

            templateChanged();
        }
    };


    /**
     * Swaps the order of the tree such that the duplicate constraint below the currently examined constraint exchange places
     */
    function moveDown(selectedNode) {
        if (!selectedNode || !selectedNode.Constraint || !$scope.constraints) {
            return false;
        }

        var constraint = selectedNode.Constraint;
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

            templateChanged();
        }
    };

    /**
     * Creates a constraint based on the base element/attribute (node) selected on the schema.
     * This function should not be called unless the user has selected a node within the schema.
     * The conformance, cardinality and data type are defaulted to whatever is default for the node in the schema.
     * @param {any} selectedNode
     */
    function createComputableConstraint(selectedNode) {
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
            templateChanged();
            //constraint.SubscribeForUpdates();
        };

        // recursively create computable constraints for parent nodes that don't have constraints, first.
        if (selectedNode.Parent && !selectedNode.Parent.Constraint) {
            createComputableConstraint(selectedNode.Parent);
        }

        createDefaultComputable(selectedNode);
        templateChanged();
    };

    /**
     * Creates a constraint that is based on free/narrative text, not associated with a node in the schema.
     * All primitive constraints are added to the end of the node list, so that the original order of the
     * nodes in the schema are preserved/prioritized.
     * @param {any} selectedNode
     */
    function createPrimitiveConstraint(selectedNode) {
        var primitive = {};
        primitive.Constraint = {
            IsPrimitive: true,
            Conformance: "SHALL"
        };

        // if selectedNode does not have a Constraint associated with it yet, then use createComputableConstraint() to create one for it
        if (selectedNode && !selectedNode.Constraint) {
            createComputableConstraint(selectedNode);
        }

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

    /**
     * Creates a copy of the selected node, duplicating all fields of the constraint except for the constraint number.
     * The copy is inserted immediately following the node being copied.
     * The selected node can be either a computable constraint or a primitive constraint.
     * @param {any} selectedNode
     */
    function duplicateConstraint(selectedNode) {
        if (!selectedNode.Constraint) {
            return;
        }

        // create a copy of the node so that a new node shows up in the constraints table. can use angular.copy()
        var newNode = angular.copy(selectedNode);
        var siblings = selectedNode.Parent ? selectedNode.Parent.Children : $scope.nodes;

        // position the new node in the list immediately after the selected node
        var nodeIndex = siblings.indexOf(selectedNode);
        nodeIndex < siblings.length - 1 ? siblings.splice(nodeIndex, 0, newNode) : siblings.push(newNode);

        // create a copy of the constraint
        var newConstraint = angular.copy(selectedNode.Constraint);
        newConstraint.Id = null;
        newConstraint.Number = "AUTO";
        newConstraint.isNew = true;
        var constraint = selectedNode.Constraint;
        var sibConstraints = constraint.Parent ? constraint.Parent.Children : $scope.constraints;

        // position the new constraint in the list immediately after the selected constraint
        var constraintIndex = sibConstraints.indexOf(constraint);
        constraintIndex < sibConstraints.length - 1 ? sibConstraints.splice(constraintIndex, 0, newConstraint) : sibConstraints.push(newConstraint);

        // update the copy of the constraint to not have the same number. the number should be automatically generated. any customized number should be removed, as well.
        newNode.Constraint = newConstraint;

        // set the selectedNode to the duplicate node
        selectedNode = newNode;
    };

    //Not used but potentially useful in the future
    var getParent = function (node) {
        var searchChildren = function (parent) {
            var result;
            var found = false;
            var children = parent.Children;
            for (var i in children) {
                if (children[i].$$hashKey === node.$$hashKey) found = true;
                if (!found && children[i].Children.length > 0) {
                    result = searchChildren(children[i].Children, node);
                    if (result) break;
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
        for (var n in $scope.nodes) {
            if ($scope.nodes[n].$$hashKey === node.$$hashKey) break; //do nothing, parent is root node
            parent = searchChildren($scope.nodes[n]);  //else, search children of node
            if (parent) break;
        }
        return parent;
    };

    /**
     * Deletes the constraint associated with the selected node.
     * If the constraint is a brand new constraint, and hasn't been persisted yet, it is permanently removed.
     * If the constraint is already persisted to the database, it is added to the list of removed constraints
     * so that when the changes are saved, the server knows to delete the constraint.
     * @param {any} selectedNode
     */
    function deleteConstraint(selectedNode) {
        if (!selectedNode.Constraint) {
            return;
        }

        var constraintIndex = $scope.constraints.indexOf(selectedNode.Constraint);

        // if the constraint has not already been saved (doesn't have an Id value), just remove the constraint
        // if the constraint HAS been saved (has an Id value), then flag it as deleted and remove (TODO: How??)
        if (selectedNode.Constraint.Id) {
            $scope.removedConstraints.push(constraint);
        }

        if (constraintIndex !== -1) $scope.constraints.splice(constraintIndex, 1);

        delete selectedNode.Constraint;
        templateChanged();
    };

    /**
     * Validates the specified node's constraint details.
     * If a message is returned, the node's constraint is invalid. The message indicates why.
     * If no message is returned, the constraint is valid.
     * @param {any} node
     */
    function isNodeValid(node) {
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

    /**
     * Validates the entire template. Recursively checks if constraints within the template have any
     * errors. Returns true if valid, otherwise false.
     */
    function isValid() {
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
            if (!node || !node.$$valid) {
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

    /**
     * Persists the changes in the template and constraints to the server/database.
     * Updates the template's ID and constraint's IDs and Numbers when complete.
     */
    function save() {
        
        if (!isValid()) {
            return;
        }

        var data = {
            Template: angular.copy($scope.template),
            RemovedConstraints: angular.copy($scope.removedConstraints),
            Constraints: angular.copy($scope.constraints)
        };

        delete data.Template.ValidationResults;

        var removeIdFromNewConstraints = function (list) {
            _.forEach(list, function (constraint) {
                if (constraint.IsNew) {
                    constraint.Id = null;
                }

                // Must remove NarrativeProseHtml because it represents an object as a result of 
                // $sce.trustAsHtml, so that NarrativeProseHtml can be bound with ng-bind-html in the view.
                // Newtonsoft's JsonConvert has problems converting an object to a string.
                delete constraint.NarrativeProseHtml;

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

        //This specifies which portion of the UI is to be blocked based on the constraint panel view
        var constraintBlockUI = blockUI.instances.get('constraintBlock');
        constraintBlockUI.start("Saving...");

        // TODO: Handle errors from the save operation on the server, and let the user know that an error occurred

        EditorService.save(JSON.stringify(data))
            .then(function (response) {
                if (response.Error) {
                    $scope.message = response.Error;
                    return;
                }

                // Update the template id on the client so that we save against the new template going forward
                $scope.template.Id = response.data.TemplateId;
                $scope.template.AuthorId = response.data.AuthorId;
                $scope.isModified = false;

                // Empty the removed constraints list
                $scope.removedConstraints = [];

                // Update the constraints on the client (possibly new IDs and new Numbers)
                updateConstraints($scope.constraints, response.data.Constraints);
                $scope.template.ValidationResults = response.data.ValidationResults;

                constraintBlockUI.stop();
            });
    };

    function discard() {
        init($scope.template.Id);
    };

    function unlock() {
        if (confirm('This template/profile is currently published. Are you sure you want to unlock the template/profile for editing?')) {
            $scope.isLocked = false;
        }

        // TODO: Not necessarily for initial release of new editor... but, should audit when this occurs. 
    };

    /**
     * Event handler for the tree-grid's nodeExpanded event. Occurs when the tree
     * grid expands a node. This handler associates constraints to the expanded node.
     * @param {any} selectedNode
     */
    function nodeExpanded(selectedNode) {
        if (selectedNode.Children.length === 0) {
            return EditorService.getNodes($scope.template.OwningImplementationGuideId, selectedNode.DataType)
                .then(function (nodes) {
                    if (selectedNode.Constraint) {
                        associateNodes(nodes, selectedNode.Constraint.Children);
                    }

                    selectedNode.Children = nodes;
                });
        } else {
            return $q.resolve();
        }
    };

    /**
     * Event handler for the tree-grid's nodeSelected event.
     * Sets the selectedNode property of this controller to the selected node in the tree
     * @param {any} selectedNode
     */
    function nodeSelected(selectedNode) {
        $scope.selectedNode = selectedNode;
    };

    /**
     * Searches the server for value sets matching the query.
     * Returns a promise, which resolves to a list of objects containing Id and Display.
     * @param {any} query
     */
    function getValueSets(query) {
        // TODO: Use a factory/service to query the server for matching value sets, as an asynchronous/promise return.
        // Return no more than 10... They must refine their search if they want a shorter list.
        // Change the model used by the server to represent ValueSetId as a complex ValueSet object instead. This
        // will allow angular-bootstrap's typeahead to initialize with the correct display and id to start with.

        return $q.resolve([
            { Id: 1, Display: 'Test Value Set 1' },
            { Id: 2, Display: 'Test Value Set 2' },
            { Id: 3, Display: 'Test Value Set 3' }
        ]);
    };

    /**
     * Searches the server for code systems matching the query.
     * Returns a promise, which resolves to a list of objects containing Id and Display.
     * @param {any} query
     */
    function getCodeSystems(query) {
        // TODO: Use a factory/service to query the server for matching code systems, as an asynchronous/promise return.
        // Return no more than 10... They must refine their search if they want a shorter list.
        // Change the model used by the server to represent CodeSystemId as a complex CodeSystem object instead. This
        // will allow angular-bootstrap's typeahead to initialize with the correct display and id to start with.
        return [
            { Id: 1, Display: 'Test Code System 1' },
            { Id: 2, Display: 'Test Code System 2' },
            { Id: 3, Display: 'Test Code System 3' }
        ];
    };

    /**
     * Gets a list of the data-types that are available for the selected node.
     * Returns a promise, which resolves to a list of strings each representing a data-type option.
     * @param {any} selectedNode
     */
    function getDataTypes(selectedNode) {
        if (!selectedNode) {
            return $q.resolve([]);
        }

        // TODO
        // Return a promise that results in a list of strings representing the possible data-types for the node
        // Add a call in EditorService that calls api/Template/Edit/DerivedType/{implementationGuideId}/{dataType}
    };

    /**
     * Event triggered when the data-type for a constraint changes.
     * When the data-type changes, the node within the tree should be re-initialized/re-expanded
     * because the elements/attributes within the node may change depending on the data-type selected.
     * @param {any} selecedNode
     */
    function dataTypeChanged(selecedNode) {
        if (!selectedNode || !selectedNode.Constraint) {
            return;
        }

        // TODO
    };

    /**
     * Initializes properties on a constraint for use within the editor.
     * Sets the $$bindingType and sets the NarrativeProseHtml from $sce
     * @param {any} constraint
     */
    function initConstraint(constraint) {
        constraint.NarrativeProseHtml = $sce.trustAsHtml(constraint.NarrativeProseHtml);

        if ((constraint.Value || constraint.ValueDisplayName) && !constraint.ValueSetId) {
            constraint.$$bindingType = 'SingleValue';
        } else if (!constraint.Value && !constraint.ValueDisplayName && constraint.ValueSetId) {
            constraint.$$bindingType = 'ValueSet';
        } else if (!constraint.Value && !constraint.ValueDisplayName && !constraint.ValueSetId && constraint.ValueCodeSystemId) {
            constraint.$$bindingType = 'CodeSystem';
        } else if (constraint.Value || constraint.ValueDisplayName || constraint.ValueSetId || constraint.ValueSetDate || constraint.ValueCodeSystemId) {
            constraint.$$bindingType = 'Other';
        }

        _.each(constraint.Children, initConstraint);
    };

    /**
     * Initializes the editor with the specified template and optionally
     * the default implementation guide for creating a new template as part
     * of a pre-determined IG.
     * @param {int} templateId
     * @param {int} defaultImplementationGuideId
     */
    function init(templateId, defaultImplementationGuideId) {
        $scope.selectedNode = null;
        $scope.isModified = false;
        $scope.nodes = [];
        $scope.constraints = [];
        $scope.isLocked = false;

        TemplateService.getTemplatePermissions(templateId)
            .then(function (permissions) {
                $scope.permissions = permissions;
            })
            .catch($scope.handleHttpError);

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
                parseIdentifier($scope.template.Oid);

                return EditorService.getConstraints(templateId);
            })
            .then(function (constraints) {
                // Set extra properties on each constraint, such as $$bindingType
                _.each(constraints, initConstraint);

                $scope.constraints = constraints;

                return EditorService.getNodes($scope.template.OwningImplementationGuideId, $scope.template.PrimaryContextType)
                    .then(function (nodes) {
                        associateNodes(nodes, $scope.constraints);
                        $scope.nodes = nodes;
                    });
            })
            .catch($scope.handleHttpError);
    };

    /**
     * Triggered when the name of the template has changed.
     */
    function nameChanged() {
        // TODO: Regenerate the bookmark based on the name of the template. Has a max length of 40 characters.
        // Replace with logic from previous template editor.
        $scope.template.Bookmark = TemplateService.generateBookmark($scope.template.Name, $scope.isFhir);

        templateChanged();
    };

    /**
     * Updates the template'd identifier based on the base and extension specified in the UI
     */
    function identifierChanged() {
        if ($scope.template) {
            $scope.template.Oid = $scope.identifier.base + $scope.identifier.ext;
        }

        templateChanged();
    };

    /**
     * Parses the specified identifier into the base and extension properties on the scope of the controller.
     * @param {string} identifier
     */
    function parseIdentifier(identifier) {
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

    function associateNodes(nodes, constraints) {
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

    /**
     * Called when a value for a field in the UI is changed. Simlpy sets "isModified" scope property to true, for now.
     * Avoided handling this with a $watch because watches are heavy. This functionality allows for injecting
     * additional logic in the future when changes are made.
     * @param {any} constraint The constraint that has changed within the template, if any.
     */
    function templateChanged(constraint) {
        $scope.isModified = true;

        if (constraint) {
            EditorService.getNarrative(constraint)
                .then(function (results) {
                    constraint.NarrativeProseHtml = $sce.trustAsHtml(results);
                })
                .catch(handleHttpError);
        }
    };

    /**
     * Reloads the current page in the browser. This is a hard reload... All scripts are reloaded as well.
     * If the user changes the template using the template searcher functionality, they may revert back to editing
     * the template they started editing when they entered the editor.
     */
    function reload() {
        $window.location.reload();
    };

    /**
     * Selects a constraint within the tree based on the constraint number specified.
     * The node is what needs to be selected, not the constraint. First finds the constraint
     * that needs to be selected and builds an array representing the tree of constraints.
     * Then goes through each item in the tree array and expands each node, and selects the final node
     * representing the constraint.
     * @param {string} constraintNumber
     */
    function selectConstraint(constraintNumber) {
        var findConstraintTree = function (children, tree) {
            for (var i = 0; i < children.length; i++) {
                var child = children[i];
                var childTree = [child].concat(tree || []);

                if (child.Number == constraintNumber) {
                    return childTree;
                }

                var foundTree = findConstraintTree(child.Children, childTree);

                if (foundTree) {
                    return foundTree;
                }
            }
        };

        var foundTree = findConstraintTree($scope.constraints);
        var currentNode = null;

        var expandNodes = function () {
            if (foundTree.length == 0) {
                return;
            }

            var nodes = currentNode ? currentNode.Children : $scope.nodes;
            var constraint = foundTree[foundTree.length - 1];

            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i].Constraint == constraint) {
                    if (foundTree.length == 1) {
                        // This is the last constraint, need to select instead of expand
                        $scope.$broadcast('selectNode', nodes[i]);
                        $scope.activeTab = 'constraints';

                        setTimeout(function () {
                            $('.tree-grid').scrollTo('.constraint-row.danger');
                        });
                    } else {
                        currentNode = nodes[i];
                        nodeExpanded(currentNode)
                            .then(function () {
                                $scope.$broadcast('expandNode', currentNode);
                                foundTree.pop();    // Remove the constraint we just expanded

                                // Recursively expand additional nodes
                                expandNodes();
                            });
                    }
                }
            }
        };

        expandNodes();
    };

    function addContainedTemplate(constraint, dataType) {
        var modalInstance = $uibModal.open({
            templateUrl: 'addContainedTemplate.html',
            controller: 'AddContainedTemplateController',
            controllerAs: '$ctrl',
            size: 'lg',
            resolve: {
                filterContextType: function () {
                    if (!$scope.isFhir) {
                        return dataType;
                    } else {
                        // TODO: Pass the allowed types for the element in the FHIR spec, ex: [Organization, Patient, Device]
                    }
                }
            }
        });

        modalInstance.result.then(function (selectedItem) {
            constraint.References.push({
                ReferenceIdentifier: selectedItem.Oid,
                ReferenceDisplay: selectedItem.Name,
                ReferenceType: 0
            });
            templateChanged(constraint);
        }, function () {
            // Modal dismissed
        });
    };

    /**
     * Handles an HTTP error by parsing it and determining if there is a message to be displayed.
     * TODO: This should be re-factored into the HelpService.
     * @param {any} err
     */
    function handleHttpError(err) {
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

    $scope.isDuplicateNode = isDuplicateNode;
    $scope.showMoveUp = showMoveUp;
    $scope.showMoveDown = showMoveDown;
    $scope.moveUp = moveUp;
    $scope.moveDown = moveDown;
    $scope.createComputableConstraint = createComputableConstraint;
    $scope.createPrimitiveConstraint = createPrimitiveConstraint;
    $scope.duplicateConstraint = duplicateConstraint
    $scope.deleteConstraint = deleteConstraint;
    $scope.isNodeValid = isNodeValid;
    $scope.isValid = isValid;
    $scope.save = save;
    $scope.discard = discard;
    $scope.unlock = unlock;
    $scope.nodeExpanded = nodeExpanded;
    $scope.nodeSelected = nodeSelected;
    $scope.init = init;
    $scope.identifierChanged = identifierChanged;
    $scope.nameChanged = nameChanged;
    $scope.handleHttpError = handleHttpError;
    $scope.reload = reload;
    $scope.templateChanged = templateChanged;
    $scope.getCodeSystems = getCodeSystems;
    $scope.getValueSets = getValueSets;
    $scope.selectConstraint = selectConstraint;
    $scope.addContainedTemplate = addContainedTemplate;
});

angular.module('Trifolia').controller('EditorTemplateSearchController', function ($scope, TemplateService) {
    $scope.templateSearch = {
        query: '',
        results: null
    };

    /**
     * Opens the specified template in either the current window or a new tab/window of the browser.
     * Uses scope inheritance to call init() on the EditorController if opening in the current window.
     * @param {any} templateId
     * @param {any} newWindow
     */
    function openTemplate(templateId, newWindow) {
        if (!newWindow) {
            $scope.init(templateId);
        } else {
            window.open('/TemplateManagement/Edit/Id/' + templateId + '/V2');
        }
    };

    /**
     * Searches for templates using the criteria indicated by scope properties (such as 'query').
     * Calls TemplateService.getTemplates() to perform the search. Asynchornous operation that sets
     * $scope.results to the search results when complete.
     */
    function searchTemplates() {
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

    $scope.searchTemplates = searchTemplates;
    $scope.openTemplate = openTemplate;

    $scope.$watch('template.OwningImplementationGuideId', function () {
        $scope.searchTemplates();
    });
});

angular.module('Trifolia').controller('AddContainedTemplateController', function ($scope, $uibModalInstance, TemplateService, filterContextType) {
    $scope.message = '';
    $scope.query = '';
    $scope.results = [];

    function search() {
        var searchOptions = {
            count: 150,
            queryText: $scope.query,
            filterContextType: filterContextType
        };

        TemplateService.getTemplates(searchOptions)
            .then(function (results) {
                $scope.results = results;
            })
            .catch(function (err) {
                $scope.message = err;
            });
    };

    function select(template) {
        $uibModalInstance.close(template);
    };

    $scope.search = search;
    $scope.select = select;
});