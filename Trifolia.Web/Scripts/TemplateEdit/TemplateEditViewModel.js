var templateEditViewModel = function (templateId, defaults) {
    var self = this;

    /* Properties */
    self.TemplateId = ko.observable(templateId);
    self.Constraints = ko.observableArray([]);
    self.RemovedConstraints = ko.observableArray([]);
    self.Nodes = ko.observableArray([]);
    self.AppliesToNodes = ko.observableArray([]);
    self.CurrentAppliesToNode = ko.observable();
    self.CurrentNode = ko.observable();
    self.ConstraintNote = ko.observable();
    self.ViewMode = ko.observable('Analyst');
    self.IsEditorMaximized = ko.observable(true);
    self.IsModified = ko.observable(false);
    self.SearchConstraintNumber = ko.observable();
    self.Status = ko.observable();
    self.QuickEditTemplateId = ko.observable();
    self.TemplateTypes = ko.observableArray([]);
    self.ImplementationGuides = ko.observableArray([]);
    self.CodeSystems = ko.observableArray([]);
    self.PublishStatuses = ko.observableArray([]);
    self.Categories = ko.observableArray([]);
    self.AvailableExtensions = ko.observableArray([]);
    self.SelectedAvailableExtensionId = ko.observable();

    function getTitleBarDisplay() {
        var display = 'Trifolia: Edit Template';

        if (self.IsModified()) {
            display = '* ' + display;
        }

        if (self.Template() && self.Template().Name()) {
            display += ' - ' + self.Template().Name();
        }

        return display;
    };

    /**
     * Check to see if a constraint is a duplicate node within the same level of the tree
     */
    self.isDuplicateNode = function () {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint() || !self.Constraints()) {
            return false;
        }

        var constraint = self.CurrentNode().Constraint();
        var siblings = constraint.Parent() ? constraint.Parent().Children : self.Constraints();

        var found = _.find(siblings, function (sibling) {
            return constraint.Context() === sibling.Context() && constraint.Id() != sibling.Id();
        })
        return found ? true : false;
    }

    /**
     * Checks to see if there's a possible shift up for the node (duplicate above the node in the tree)
     */
    self.showMoveUp = function () {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint() || !self.Constraints()) {
            return false;
        }

        var constraint = self.CurrentNode().Constraint();
        var siblings = constraint.Parent() ? constraint.Parent().Children : self.Constraints;
        var index = siblings.indexOf(constraint);

        return index !== -1 && index != 0 && siblings().length > 0 && siblings()[index - 1].Context() === constraint.Context();
    };

    /**
     * Checks to see if there's a possible shift down for the node (duplicate below the node in the tree)
     */
    self.showMoveDown = function () {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint() || !self.Constraints()) {
            return false;
        }

        var constraint = self.CurrentNode().Constraint();
        var siblings = constraint.Parent() ? constraint.Parent().Children : self.Constraints;
        var index = siblings.indexOf(constraint);

        return index !== -1 && siblings().length > 0 && index < siblings().length - 1 && siblings()[index + 1].Context() === constraint.Context();
    }

    /**
     * Swaps the order of the tree such that the duplicate constraint above the currently examined constraint exchange places
     */
    self.moveUp = function () {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint() || !self.Constraints()) {
            return false;
        }

        var constraint = self.CurrentNode().Constraint();
        var siblings = constraint.IsPrimitive() ? constraint.Parent().Children : self.Constraints;
        var index = siblings.indexOf(constraint);

        if (index != 0 && siblings()[index - 1].Context() === constraint.Context()) {
            var tmp = siblings()[index];
            siblings()[index] = siblings()[index - 1];
            siblings()[index - 1] = tmp;
            siblings.valueHasMutated();

            var siblingNodes = self.CurrentNode().Parent() ? self.CurrentNode().Parent().Children : self.Nodes;
            var currentNodeIndex = siblingNodes.indexOf(self.CurrentNode());
            var tmpNode = siblingNodes.splice(currentNodeIndex, 1);
            siblingNodes.splice(currentNodeIndex - 1, 0, tmpNode[0]);
        }
    };

    /**
     * Swaps the order of the tree such that the duplicate constraint below the currently examined constraint exchange places
     */
    self.moveDown = function () {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint() || !self.Constraints()) {
            return false;
        }

        var constraint = self.CurrentNode().Constraint();
        var siblings = constraint.IsPrimitive() ? constraint.Parent().Children : self.Constraints;
        var index = siblings.indexOf(constraint);

        if (index != siblings().length - 1 && siblings()[index + 1].Context() === constraint.Context()) {
            var tmp = siblings()[index];
            siblings()[index] = siblings()[index + 1];
            siblings()[index + 1] = tmp;
            siblings.valueHasMutated();

            var siblingNodes = self.CurrentNode().Parent() ? self.CurrentNode().Parent().Children : self.Nodes;
            var currentNodeIndex = siblingNodes.indexOf(self.CurrentNode());
            var tmpNode = siblingNodes.splice(currentNodeIndex, 1);
            siblingNodes.splice(currentNodeIndex + 1, 0, tmpNode[0]);
        }
    }

    self.IsFhir = ko.observable(false);
    self.IsFhirExtension = ko.observable(false);

    self.IsAnalyst = ko.computed(function () {
        return self.ViewMode() == 'Analyst';
    });

    self.IsTechEditor = ko.computed(function () {
        return self.ViewMode() == 'Editor';
    });

    self.IsEngineer = ko.computed(function () {
        return self.ViewMode() == 'Engineer';
    });

    var createNewTemplate = function () {
        var newTemplate = new TemplateModel(null, self);
        newTemplate.OwningImplementationGuideId(defaults.ImplementationGuideId);
        return newTemplate;
    };
    self.Template = ko.observable(createNewTemplate());

    self.ImplementationGuideChanged = function () {
        var implementationGuideId = self.Template().OwningImplementationGuideId();

        self.InitializePublishStatuses(implementationGuideId);
        self.InitializeTemplateTypes(implementationGuideId);
        self.InitializeCategories(implementationGuideId);
        self.InitializeAvailableExtensions(implementationGuideId);
    };

    self.GetRetiredStatusId = function () {
        var retiredStatuses = ko.utils.arrayFilter(self.PublishStatuses(), function (status) {
            return status.Name == 'Retired';
        });

        if (retiredStatuses.length == 1) {
            return retiredStatuses[0].Id;
        }
    };

    self.DisableConstraints = ko.computed(function () {
        return self.Template().StatusId() == self.GetRetiredStatusId();
    });

    self.DisableImpliedTemplate = ko.computed(function () {
        var disableFields = self.Template().DisableFields();
        var isRetired = self.Template().StatusId() == self.GetRetiredStatusId();

        return disableFields || isRetired;
    });

    self.StatusChanged = function (newStatusId, oldStatusId) {
        if (newStatusId == oldStatusId) {
            return;
        }

        // If the status of the template is being set to retired, 
        // confirm if we should remove constraints and the implied template
        if (newStatusId == self.GetRetiredStatusId() && confirm("Retiring a template requires that the new version of the template has no constraints, and no implied template. Do you want to continue? (Note: these changes will not be persisted until you press 'Save')")) {
            // Remove constraints
            for (var i = 0; i < self.Nodes().length; i++) {
                var node = self.Nodes()[i];

                if (node && node.Constraint()) {
                    if (self.RemoveConstraint(node)) {
                        i--;
                    }
                }
            }

            // Remove the implied tempalte
            self.Template().ImpliedTemplateId(null);
        }
    };

    /* Regenerates the bookmark field's value based on the name of the template
    * and the abbreviation associated with the template/profile type.
    */
    self.RegenerateBookmark = function () {
        var templateName = self.Template().Name();

        if (!templateName) {
            self.Template().Bookmark(null);
            return;
        }

        var replacement = self.IsFhir() ? '-' : '_';
        var templateTypeAbbreviation = self.Template().TemplateTypeAbbreviation();
        var newBookmark = templateName.replace(/ /gi, replacement);
        var cleanupRegexExp = !self.IsFhir() ? '[^\w\s]' : '[^\w\s-]';

        // If not FHIR, then make sure the bookmark does not have special characters in it like -
        if (!self.IsFhir()) {
            newBookmark = newBookmark.replace(/[^\w\s]/gi, '');
        }

        if (templateTypeAbbreviation != null) {
            newBookmark = templateTypeAbbreviation + "_" + newBookmark;
        }

        self.Template().Bookmark(newBookmark.substring(0, 39));
    };

    self.NameChanged = function () {
        document.title = getTitleBarDisplay();
        self.RegenerateBookmark();
    };

    // Subscribe to changes to the ViewMode() observable and store the value in 
    // cookies so that when the user next opens the template editor, the same ViewMode() is selected
    self.ViewMode.subscribe(function (newViewMode) {
        $.cookie('templateEditorViewMode', newViewMode);
    });

    // Update the browser's title for the page when the IsModified() observable changes
    self.IsModified.subscribe(function (newValue) {
        document.title = getTitleBarDisplay();
    });

    self.DisableConstraintFields = function() {
        if (!self.CurrentNode() || !self.CurrentNode().Constraint()) {
            return false;
        }

        if (self.Template().Locked()) {
            return true;
        }

        return self.IsExtensionUrl(self.CurrentNode().Constraint());
    };

    self.DisableConstraintRemove = function() {
        return self.DisableConstraintFields();
    };

    self.DisableAddChildPrimitive = function() {
        return self.DisableConstraintFields();
    };

    self.DisableDuplicateConstraint = function() {
        return self.DisableConstraintFields();
    };

    self.IsExtensionUrl = function(constraint) {
        var isUrlConstraint = constraint.Context() == '@url' && !constraint.Parent();
        return self.IsFhirExtension() && isUrlConstraint;
    };

    var initializeFhirProperties = function () {
        if (!self.Template().TemplateTypeId()) {
            self.IsFhir(false);
            self.IsFhirExtension(false);
            return;
        }

        var foundTemplateType = ko.utils.arrayFirst(self.TemplateTypes(), function (templateType) {
            return templateType.Id == self.Template().TemplateTypeId();
        });

        var isFhir = false;

        if (foundTemplateType) {
            isFhir = _.some(trifoliaConfig.FhirIgTypes, function (fhirIgType) {
                return fhirIgType.Name == foundTemplateType.ImplementationGuideType;
            });
        }

        self.IsFhir(isFhir);
        self.IsFhirExtension(isFhir && foundTemplateType.RootContextType == 'Extension');
    };

    self.TemplateTypeChanged = function () {
        var foundTemplateType = ko.utils.arrayFirst(self.TemplateTypes(), function (templateType) {
            return templateType.Id == self.Template().TemplateTypeId();
        });

        self.RegenerateBookmark();

        if (!foundTemplateType) {
            self.Nodes.splice(0, self.Nodes().length);
        } else {
            self.Template().TemplateTypeAbbreviation(foundTemplateType.Abbreviation);
            self.Template().PrimaryContext(foundTemplateType.RootContext);
            self.Template().PrimaryContextType(foundTemplateType.RootContextType);

            initializeFhirProperties();

            if (!self.Template().Oid() && self.IsFhir()) {
                if (self.ImplementationGuideBaseIdentifier()) {
                    self.Template().IdentifierPrefix(self.ImplementationGuideBaseIdentifier());
                } else {
                    self.Template().IdentifierPrefix(location.origin + '/api/FHIR2/StructureDefinition/');
                }
            }

            self.InitializeAppliesToNodeLevel(null)
                .then(function () {
                    if (self.IsFhirExtension()) {
                        // Make sure a constraint is created for @url that matches the identifier of the profile/extension
                        var urlConstraints = $.grep(self.Constraints(), function (a) {
                            return a.Context() == '@url' && !a.Parent();
                        });
                        var urlNodes = $.grep(self.Nodes(), function (a) {
                            return a.Context() == '@url' && !a.Parent();
                        });
                        var urlConstraint = urlConstraints.length == 0 ? null : urlConstraints[0];
                        var urlNode = urlNodes.length == 0 ? null : urlNodes[0];

                        if (!urlConstraint && urlNode) {
                            urlConstraint = new ConstraintModel({
                                Cardinality: '1..1',
                                Conformance: 'SHALL',
                                Context: '@url',
                                Value: self.Template().Oid()
                            }, null, self);
                            urlConstraint.ConstraintAndProseChanged();

                            self.Constraints.push(urlConstraint);
                            urlNode.Constraint(urlConstraint);
                        } else if (urlConstraint) {
                            urlConstraint.Value(self.Oid());
                        }
                    }
                })
                .catch(function (err) {
                    alert('An error occurred while initializing the node');
                    console.log(err);
                });
        }
    };

    /* 
    * Public Methods 
    */

    self.AddAvailableExtension = function () {
        if (!self.SelectedAvailableExtensionId()) {
            return;
        }

        var selectedAvailableExtension = $.grep(self.AvailableExtensions(), function(a) {
            return a.Id == self.SelectedAvailableExtensionId();
        })[0];

        var findNode = function (parent, context) {
            var nodes = parent ? parent.Children() : self.Nodes();

            var foundNodes = $.grep(nodes, function (a) {
                return a.Context() == context;
            });

            if (foundNodes.length > 0) {
                return foundNodes[0];
            }
        };

        var getExtensionConstraints = function () {
            var deferred = Q.defer();

            $.ajax({
                url: '/api/Template/' + self.SelectedAvailableExtensionId() + '/Constraint',
                success: function (constraints) {
                    deferred.resolve(constraints);
                },
                error: function (err) {
                    deferred.reject(err);
                }
            });

            return deferred.promise;
        };

        var addNewConstraints = function (extensionConstraints, parentNode) {
            var deferred = Q.defer();

            if (!parentNode) {
                parentNode = self.CurrentNode();
            }

            self.ExpandNode(parentNode, true)
                .then(function () {
                    var childConstraintPromises = [];

                    for (var i in extensionConstraints) {
                        var extensionConstraint = extensionConstraints[i];
                        var node = findNode(parentNode, extensionConstraint.Context);

                        if (node) {
                            node.CreateComputable();
                            node.Constraint().Conformance(extensionConstraint.Conformance);
                            node.Constraint().Cardinality(extensionConstraint.Cardinality);

                            if (extensionConstraint.Value) {
                                node.Constraint().Value(extensionConstraint.Value);
                                node.Constraint().BindingType('SingleValue');
                            }

                            if (parentNode == self.CurrentNode() && extensionConstraint.Context == '@url') {
                                node.Constraint().IsBranchIdentifier(true);
                            }

                            if (extensionConstraint.ChildConstraints && extensionConstraint.ChildConstraints.length > 0) {
                                var childConstraintPromise = addNewConstraints(extensionConstraint.ChildConstraints, node);
                                childConstraintPromises.push(childConstraintPromise);
                            }
                        } else {
                            throw 'Element/attribute not found to constraint in extension';
                        }
                    }

                    return Q.all(childConstraintPromises);
                })
                .then(function() {
                    deferred.resolve();
                })
                .catch(function (err) {
                    deferred.reject(err);
                });

             return deferred.promise;
        };

        getExtensionConstraints()
            .then(function (extensionConstraints) {
                // Create the extension constraint
                self.CurrentNode().CreateComputable();
                self.CurrentNode().Constraint().Conformance("SHALL");
                self.CurrentNode().Constraint().Cardinality("1..1");
                self.CurrentNode().Constraint().IsBranch(true);

                return addNewConstraints(extensionConstraints);
            })
            .then(function() {
                self.ApplyWidths();
            })
            .catch(function (err) {
                alert('An error occurred while adding the extension');
                console.log(err);
            });
    };

    /* Executed when the quick-edit button is pressed.
    * If no quick-edit template is selected, the method does nothing.
    * Redirects the user to edit the selected template.
    */
    self.QuickEditTemplate = function () {
        if (!self.QuickEditTemplateId()) {
            return;
        }

        location.href = '/TemplateManagement/Edit/Id/' + self.QuickEditTemplateId();
    };

    /* Detect key presses on the SearchConstraint textbox.
    * When the user presses the <enter> key, trigger the FindConstraintNode() method
    */
    self.SearchConstraintKeyPress = function (target, event) {
        if (event.charCode == 13) {
            self.FindConstraintNode(self.SearchConstraintNumber());
            return false;
        }

        return true;
    };

    /* Finds and selects a node that has a constraint
    * based on the number specified. This method requires
    * a recursive find due to the hierarchy of the 
    * self.Nodes() property. Not only does the method
    * select the node, but it expands any parents of the node
    * within the hierarchy so the user can see where in the
    * tree the node is.
    */
    self.FindConstraintNode = function (number) {
        if (!number) {
            return;
        }

        var foundConstraint;
        var findConstraint = function (parent) {
            if (parent.DisplayNumber() == number || parent.Number() == number) {
                return parent;
            }
            
            var foundConstraint;

            ko.utils.arrayForEach(parent.Children(), function (item) {
                if (!foundConstraint) {
                    foundConstraint = findConstraint(item);
                }
            });

            return foundConstraint;
        }

        // Search node for the constraint
        ko.utils.arrayForEach(self.Constraints(), function (item) {
            if (!foundConstraint) {
                foundConstraint = findConstraint(item);
            }
        });

        // Expand the constraint
        if (foundConstraint) {
            var constraintTree = [];
            var current = foundConstraint.Parent();

            // Build the constraint tree, so we can expand each level in order
            while (current) {
                constraintTree.splice(0, 0, current);
                current = current.Parent();
            }

            // Expand each node in the tree
            var parentNode;
            for (var i in constraintTree) {
                current = constraintTree[i];

                var nodes = parentNode ? parentNode.Children() : self.Nodes();
                var foundNode = ko.utils.arrayFirst(nodes, function (item) {
                    return item.Constraint() == current;
                });

                if (foundNode) {
                    self.ExpandNode(foundNode, false);
                    parentNode = foundNode;
                }
            }

            // Find the constraint's node at the last level of the expanded tree and select it
            var nodes = parentNode ? parentNode.Children() : self.Nodes();
            var foundConstraintNode = ko.utils.arrayFirst(nodes, function(item) {
                return item.Constraint() == foundConstraint;
            });

            if (foundConstraintNode) {
                self.CurrentNode(foundConstraintNode);
            }

            // Scroll the browser to the element
            var foundElement = $("div[nodeId=" + foundConstraintNode.Id() + "]");
            $('html, body').animate({
                scrollTop: foundElement.offset().top
            }, 2000);
        } else {
            alert("Constraint not found!");
        }
    };

    /* Opens the "Edit Note" dialog.
    * So that the user can cancel the edit, self.ConstraintNote()
    * property is used to hold the temporary value of the note.
    * Only when self.EditNoteSave() is called (by the user cliking
    * the OK button), is the temporary note property stored back
    * on the constraint.
    */
    self.EditNote = function (node) {
        $("#editNoteDialog").modal("show");
        self.ConstraintNote(self.CurrentNode().Constraint().Notes());
    };

    /*
    * Bound to the "Edit Note" dialog's OK button. Saves the temporary
    * note back to the constraint and closes the dialog. Runs ApplyWidths()
    * so that the note icon in the constraint tree does not adversely
    * affect the view of the tree.
    */
    self.EditNoteSave = function () {
        self.CurrentNode().Constraint().Notes(self.ConstraintNote());

        $("#editNoteDialog").modal("hide");
        self.ConstraintNote('');
        self.ApplyWidths();
    };

    self.ConformanceNumberChanged = function () {
        self.ApplyWidths();
        $('#constraintNumberEdit').slideToggle();
    };

    /* Bound to the "Edit Note" dialog's Cancel button. Closes the dialog
    * and reverts the temporary note.
    */
    self.EditNoteCancel = function (node) {
        $("#editNoteDialog").modal("hide");
        self.ConstraintNote('');
    };

    /* Event handler for when a constraint's data type has changed.
    * This is used by KO custom bindings so that when a constraint's data
    * type changes, the new node is refreshed so that it reflects the 
    * correct children of the data type.
    */
    self.ConstraintDataTypeChanged = function (oldDataType, newDataType) {
        self.CurrentNode().Children.splice(0, self.CurrentNode().Children().length);
        self.CurrentNode().AreChildrenLoaded(false);

        if (self.CurrentNode().IsExpanded()) {
            self.InitializeNodeLevel(self.CurrentNode(), true)
                .then(function () {
                    self.ApplyWidths();
                })
                .catch(function (err) {
                    alert('An error occurred initializing the node');
                    console.log(err);
                });
        }
    };

    /* Duplicates a constraint by creating a copy of the entire node and constraint.
    * Not all fields within the constraint are copied (only the ones that apply to
    * the schema, such as Context, Conformance, Cardinality and Data Type).
    * The new node and constraint is placed directly after the node and constraint
    * being copied.
    */
    self.DuplicateConstraint = function (node) {
        var constraint = node.Constraint();
        var siblingNodes = node.Parent() ? node.Parent().Children : self.Nodes;
        var siblingConstraints = constraint.Parent() ? constraint.Parent().Children : self.Constraints;
        var nodeIndex = siblingNodes.indexOfId(node);
        var constraintIndex = siblingConstraints.indexOfId(constraint);

        var newNode = new NodeModel(null, node.Parent(), self);
        newNode.HasChildren(node.HasChildren());
        newNode.Context(node.Context());
        newNode.Conformance(node.Conformance());
        newNode.Cardinality(node.Cardinality());
        newNode.DataType(node.DataType());

        var constraintJS = ko.mapping.toJS(constraint);
        constraintJS.Id = null;
        constraintJS.IsNew = true;
        constraintJS.Number = null;
        constraintJS.Children = [];

        var newConstraint = new ConstraintModel(constraintJS, constraint.Parent(), self);
        newConstraint.ConstraintAndProseChanged();
        newConstraint.SubscribeForUpdates();
        newNode.Constraint(newConstraint);

        // Add the new objects to the sibling lists
        siblingNodes.splice(nodeIndex + 1, 0, newNode);
        siblingConstraints.splice(constraintIndex + 1, 0, newConstraint);

        // Select the duplicated constraint
        self.CurrentNode(newNode);
        self.ApplyWidths();
    };

    /* Creates a new primitive constraint (and node) at the end of the
    * current level. If parentNode is null, then the level is assumed to
    * be the root (self.Nodes). If the parentNode does not have a constraint
    * associated with it, a default constraint is created for all parents
    */
    self.AddPrimitive = function (parentNode) {
        var addPrimitive = function () {
            if (parentNode && !parentNode.Constraint()) {
                parentNode.CreateComputable();
            }

            var parentConstraint = parentNode ? parentNode.Constraint() : null;

            // Create the constraint
            var newConstraint = new ConstraintModel(null, parentConstraint, self);
            newConstraint.IsPrimitive(true);
            newConstraint.Schematron('not(.)');
            newConstraint.Conformance("SHALL");

            // Create the node
            var newNode = new NodeModel(null, parentNode, self);
            newNode.Constraint(newConstraint);

            // Add the node and constraint to the list
            if (parentNode) {
                parentConstraint.Children.push(newConstraint);
                parentNode.Children.push(newNode);

                // Make sure the parent node indicates it has children. Ex: attributes don't have children by default
                parentNode.HasChildren(true);
            } else {
                self.Constraints.push(newConstraint);
                self.Nodes.push(newNode);
            }

            newConstraint.SubscribeForUpdates();

            self.CurrentNode(newNode);
            self.ApplyWidths();
        };

        if (parentNode && !parentNode.IsExpanded()) {
            self.ExpandNode(parentNode, true)
                .then(function () {
                    addPrimitive();
                })
                .catch(function (err) {
                    alert('An error occurred while expanding the node');
                    console.log(err);
                });
        } else {
            addPrimitive();
        }
    };

    /* Removes a constraint from the node specified.
    * If the constraint is a primitive, both the constraint and the node itself are removed.
    * If the constraint is a duplicate (identified by the node's unique index), the node is also removed.
    */
    self.RemoveConstraint = function (node) {
        var removedNode = false;
        var constraint = node.Constraint();
        var constraintSiblings = constraint.Parent() ? constraint.Parent().Children : self.Constraints;
        var nodeSiblings = node.Parent() ? node.Parent().Children : self.Nodes;
        var constraintIndex = constraintSiblings.indexOfId(constraint);
        var nodeIndex = nodeSiblings.indexOfId(node);
        var parentNode = node.Parent();

        if (!constraint.IsNew()) {
            self.RemovedConstraints.push(constraint);
        }

        constraintSiblings.splice(constraintIndex, 1);
        node.RemoveConstraint();            // Recursively removes constraints from children

        // Remove the node if it is primitive or a duplicate
        if (constraint.IsPrimitive() || !node.Context()) {
            nodeSiblings.splice(nodeIndex, 1);
            self.CurrentNode(null);
        } else if (nodeIndex > 0) {
            var previousNode = nodeSiblings()[nodeIndex - 1];

            if (previousNode.Context() == node.Context()) {
                nodeSiblings.splice(nodeIndex, 1);
                self.CurrentNode(previousNode);
                removedNode = true;
            }
        }

        // parent node should not have children if it is an attribute and no longer has primitives
        if (parentNode && (parentNode.IsAttribute() || !parentNode.Context()) && parentNode.Children().length == 0) {
            node.Parent().HasChildren(false);
        }

        // Mark template as changed
        self.IsModified(true);      // TODO: Test

        self.ApplyWidths();
        return removedNode;
    };

    /* Returns an array of nodes based on the parentNode specified.
    * If no parent node is specified, returns the root-level of nodes.
    * If a parent IS specified, returns the children of the parent node.
    * This method is used by the recursive template in the view.
    */
    self.GetNodes = function (parentNode) {
        if (!parentNode) {
            return self.Nodes();
        } else {
            return parentNode.Children();
        }
    };

    /* Expands the specified parent node. Determines if the children of the
    * node have been loaded already. If not, asynchronously calls InitializeNodeLevel
    * to make an AJAX call to the server for the children.
    */
    self.ExpandNode = function (parentNode, isAsync) {
        var deferred = Q.defer();

        self.InitializeNodeLevel(parentNode, isAsync)
            .then(function () {
                parentNode.IsExpanded(true);
                parentNode.IsExpanded.valueHasMutated();
                self.ApplyWidths();
                deferred.resolve();
            })
            .catch(function(err) {
                deferred.reject(err);
            });

        return deferred.promise;
    };

    /* Expands nodes in the "Applies To" dialog.
    * If the children of the node have not been loaded, performs an
    * ajax query to the server to get the child nodes (asynchronously).
    */
    self.ExpandAppliesToNode = function (parentNode) {
        if (!parentNode.AreChildrenLoaded()) {
            self.InitializeAppliesToNodeLevel(parentNode)
                .then(function () {
                    parentNode.IsExpanded(true);
                })
                .catch(function (err) {
                    alert('An error occurred while initializing the node');
                    console.log(err);
                });
        } else {
            parentNode.IsExpanded(true);
        }
    };

    /* Collapses the specified node. 
    * Un-sets the CurrentNode if it is a child of the node being collapsed.
    */
    self.CollapseNode = function (node) {
        node.IsExpanded(false);
        self.ApplyWidths();

        // Determine if the CurrentNode() is a the collapsing node or one of its children
        // and un-select the node if that's true
        var isSelectedParent = false;
        var current = self.CurrentNode();

        while (current) {
            if (current == node) {
                isSelectedParent = true;
            }
            
            current = current.Parent();
        }

        if (isSelectedParent) {
            self.CurrentNode(null);
        }
    };
    
    /* Sets the width of constraint cells to be the same maximum width
    * based on the content within the cells. This creates the grid effect.
    */ 
    self.ApplyWidths = function () {
        var applyWidths = function (selector, minWidth) {
            $(selector).css('min-width', minWidth + 'px');
            var maxWidth = 0;
            $(selector).each(function (item) {
                if ($(this).width() > maxWidth) {
                    maxWidth = $(this).width();
                }
            });
            $(selector).css('min-width', (maxWidth + 20) + 'px');
        };

        applyWidths(".constraintNumber", 50);
        applyWidths(".constraintContext", 100);
        applyWidths(".constraintDataType", 150);
        applyWidths(".constraintValue", 100);
    };

    /* Sets the CurrentNode to the node specified.
    */
    self.SelectNode = function (node) {
        self.CurrentNode(node);
    }

    self.GetDataTypes = function (baseDataType) {
        // Get list of data types for node
        var implementationGuideId = self.Template().OwningImplementationGuideId();
        var dataTypes = [];

        $.ajax({
            method: "GET",
            url: "/api/Template/Edit/DerivedType/" + implementationGuideId + "/" + baseDataType,
            async: false,
            success: function (results) {
                dataTypes.push({ Value: '', Text: 'DEFAULT' });
                if (results) {
                    ko.utils.arrayForEach(results, function (result) {
                        dataTypes.push({ Value: result, Text: result });
                    });
                }
            }
        });

        return dataTypes;
    };

    /* Determines if the node specified is the same as the CurrentAppliesToNode()
    * so that the node can be highlighted in the tree-grid. This is only used
    * by the "Applies To" dialog.
    */ 
    self.IsCurrentAppliesToNode = function (node) {
        if (!node || !self.CurrentAppliesToNode()) {
            return false;
        }

        return node.Id() == self.CurrentAppliesToNode().Id();
    }

    /* Determines if the node specified is the same as the CurrentNode()
    * so that the node can be highlighted in the tree-grid.
    */
    self.IsCurrentNode = function (node) {
        if (!node || !self.CurrentNode()) {
            return false;
        }

        return node.Id() == self.CurrentNode().Id();
    }

    /* Private Methods */

    var getNodePath = function (node) {
        var current = node;
        var nodePath = '';

        while (current) {
            if (nodePath) {
                nodePath = current.Context() + '/' + nodePath;
            } else {
                nodePath = current.Context();
            }
            current = current.Parent();
        }

        return nodePath;
    };
    
    /* Initializes a level of schema nodes in the applies-to node-set.
    * This is only used for the "Applies To" dialog functionality.
    */
    self.InitializeAppliesToNodeLevel = function (node) {
        if (!self.Template().OwningImplementationGuideId()) {
            return Q.resolve();
        }

        var deferred = Q.defer();

        // Remove already existing nodes when initializing the root level
        if (!node) {
            self.AppliesToNodes.splice(0, self.AppliesToNodes().length);
        }

        var url = '/api/Template/Edit/Schema/' + self.Template().OwningImplementationGuideId() + '?';

        if (node && node.DataType()) {
            url += 'parentType=' + encodeURIComponent(node.DataType()) + '&';
        } else if (self.Template().PrimaryContextType()) {
            url += 'parentType=' + encodeURIComponent(self.Template().PrimaryContextType()) + '&';

            if (node) {
                url += 'path=' + encodeURIComponent(getNodePath(node)) + '&';
            }
        }

        // Don't include attributes
        url += 'includeAttributes=false&';

        $.ajax({
            url: url,
            async: true,
            success: function (results) {
                ko.utils.arrayForEach(results, function (item) {
                    var newNode = new NodeModel(item, node, self);

                    if (node) {
                        node.Children.push(newNode);
                    } else {
                        self.AppliesToNodes.push(newNode);
                    }
                });

                deferred.resolve();
            }
        });

        return deferred.promise;
    };

    /* Initializes a level of schema nodes in the constraint tree's node set.
    * After getting the nodes from the server, the method creates a NodeModel()
    * for each node, and associates it to a constraint. Primitives within the 
    * parent constraint are always added to the bottom of the node's Children 
    * array. In some cases, NodeModel() entries need to be duplicated to support 
    * multiple constraints with the same context.
    */
    self.InitializeNodeLevel = function (node, async) {
        if (!self.Template().OwningImplementationGuideId()) {
            return Q.resolve();
        } else if (node && node.ChildrenLoadingPromise()) {
            return node.ChildrenLoadingPromise();
        }
        
        var deferred = Q.defer();
        var url = '/api/Template/Edit/Schema/' + self.Template().OwningImplementationGuideId();
        var constraintList = node && node.Constraint() ? node.Constraint().Children : null;
        var nodeList = node ? node.Children : self.Nodes;

        if (!node) {
            constraintList = self.Constraints;
        }

        if (node && node.DisplayDataType()) {
            url += '?parentType=' + encodeURIComponent(node.DisplayDataType());
        } else if (self.Template().PrimaryContextType()) {
            url += '?parentType=' + encodeURIComponent(self.Template().PrimaryContextType());
        }

        var createNode = function (item) {
            var newNode = new NodeModel(item, node, self);
            nodeList().push(newNode);
            return newNode;
        };

        // Context is the context of the constraint we are looking for
        var findChildConstraints = function (context) {
            var matchingConstraints = [];

            if (constraintList) {
                for (var i in constraintList()) {
                    if (!constraintList().hasOwnProperty(i)) {
                        continue;
                    }

                    var constraint = constraintList()[i];

                    if (constraint.Context() == context) {
                        matchingConstraints.push(constraint);
                    }
                }
            }

            return matchingConstraints;
        };

        var fixIncorrectOrder = function () {
            var nodesWithConstraints = [];
            for (var i in nodeList()) {
                if (nodeList()[i].Constraint()) {
                    nodesWithConstraints.push(nodeList()[i]);
                }
            }

            if (!constraintList) {
                if (nodesWithConstraints.length != 0) {
                    alert("There are more nodes with constraints in the tree than there are constraints on the template.");
                }

                return Q.resolve();
            } else if (nodesWithConstraints.length > constraintList().length) {
                alert("There are more nodes with constraints in the tree than there are constraints on the template.");
                return Q.resolve();
            } else if (nodesWithConstraints.length < constraintList().length) {
                alert("There are more constraints than there are nodes associated with constraints.");
                return Q.resolve();
            }

            var hasBeenNotified = false;

            // Go through the list of nodes and make sure that each node matches the order
            // in which the constraint appears
            for (var i = 0; i < constraintList().length; i++) {
                var node = nodesWithConstraints[i];
                var constraint = constraintList()[i];

                if (node.Context() && node.Context() != constraint.Context()) {
                    if (!hasBeenNotified) {
                        if (!confirm("Constraints on the template are out of order, and may export incorrectly. This issue may not be apparent in the template editor, but will be seen when viewing and exporting the template. Would you like to fix the problem now?")) {
                            return;
                        }

                        hasBeenNotified = true;
                    }

                    // If the node and constraint at the same index does not have the same context
                    // then they are out of order. Look for a constraint that DOES have the same context
                    // and move it to this location.
                    for (var x = i; x < constraintList().length; x++) {
                        if (constraintList()[x].Context() == node.Context()) {
                            var matchedConstraint = constraintList()[x];
                            constraintList().splice(x, 1);      // Remove the constraint from its current position
                            constraintList().splice(i, 0, matchedConstraint);       // Insert the constraint in the new position
                            self.IsModified(true);
                            break;
                        }
                    }
                }
            }

            // Tell knockout to update the node list observables
            if (hasBeenNotified) {
                nodeList.valueHasMutated();
            }
        };

        var associateConstraints = function () {
            if (constraintList) {
                for (var x in constraintList()) {
                    var childConstraint = constraintList()[x];

                    // Look for constraints that haven't been matched to a node, and create a node for them
                    var foundNode = false;
                    var nodes = nodeList();

                    for (var y = 0; y < nodes.length; y++) {
                        if (nodes[y].Constraint() == childConstraint) {
                            foundNode = true;
                            break;
                        }
                    }

                    if (!foundNode) {
                        var newNode = createNode(null);
                        newNode.Constraint(childConstraint);
                        newNode.HasChildren(childConstraint.Children().length > 0);
                    }
                }
            }

            if (node) {
                node.AreChildrenLoaded(true);
            }

            // Tell knockout to update the node list observables
            nodeList.valueHasMutated();
        };

        var shouldCallServer = !node || !node.Constraint() || !node.Constraint().IsPrimitive();

        if (node && ((node.IsAttribute() && node.Constraint().Children().length == 0) || node.AreChildrenLoaded()))
            shouldCallServer = false;

        // Only get the list of nodes (for the schema) from the server if 
        // 1) There is no parent node (this means that we are getting the nodes for the root level of the tree).
        // 2) or There is a node, but no constraint. This means we either already have the node definition, or it potentially a primitive.
        // 3) or There is a node and a constraint, and the constraint is not a primitive. There are no association to the schema for primitive constraints.
        // [NO LONGER TRUE (?)] 4) and never call to the server when the node is an attribute (attributes don't have children). 
        if (shouldCallServer) {
            if (node) {
                node.ChildrenLoadingPromise(deferred.promise);
            }

            if (node && !node.DisplayDataType()) {
                var path = '';
                var next = node;

                while (next != null) {
                    path = next.Context() + (path ? '/' + path : '');
                    next = next.Parent();
                }

                url += '&path=' + encodeURIComponent(path);
            }

            $.ajax({
                url: url,
                async: async,
                success: function (results) {
                    // Loop through each node result and find constraints that should be associated with the nodes
                    for (var i in results) {
                        if (!results.hasOwnProperty(i)) {
                            continue;
                        }

                        var item = results[i];
                        var newNode = createNode(item);
                        var foundConstraints = findChildConstraints(newNode.Context());

                        for (var constraintIndex in foundConstraints) {
                            var constraint = foundConstraints[constraintIndex];

                            if (constraintIndex == 0) {
                                // Always associate the first constraint with the first node
                                newNode.Constraint(constraint);
                                newNode.HasChildren(newNode.HasChildren() || constraint.Children().length > 0);
                            } else {
                                // Duplicate the node if there are multiple constraints associated with the same node context
                                var nextNode = createNode(item);
                                nextNode.Constraint(constraint);
                                nextNode.HasChildren(nextNode.HasChildren() || constraint.Children().length > 0);
                            }
                        };
                    };

                    associateConstraints();

                    fixIncorrectOrder();

                    if (node) {
                        node.ChildrenLoadingPromise(null);
                    }

                    // Done, notify
                    deferred.resolve();
                }
            });
        } else {
            associateConstraints();

            deferred.resolve();
        }

        return deferred.promise;
    };

    /* Called when the user is attempting to leave the template editor.
    * If the template has been modified, the user is warned before leaving and
    * given an opportunity to stay on the template editor page.
    */
    self.ConfirmLeave = function () {
        if (self.IsModified()) {
            return 'Changes have been made to the template. Are you sure you want to leave?';
        }
    };

    /* Saves the template to the server.
    * Checks that the template is valid, first.
    * Creates a complex object of template, removed constraints and current constraints
    * and executes an AJAX call back to the server.
    * If the response indicates an error, the error is displayed in the status.
    * If successful, and the actionAfter is not list/view, then the template id
    * is set on the view model's TemplateId() observable, and the viewmodel's Refresh() is called.
    * If successful, and the actionAfter is list, the user is redirected to the browse templates page.
    * If successful, and the actionAfter is view, the user is redirected to the view template page.
    */
    self.Save = function (actionAfter) {
        if (!self.Template().IsValid()) {
            alert("One or more fields on the 'Template' tab are not valid. Cannot save...");
            return;
        }

        var data = {
            Template: ko.mapping.toJS(self.Template()),
            RemovedConstraints: ko.mapping.toJS(self.RemovedConstraints()),
            Constraints: ko.mapping.toJS(self.Constraints())
        };

        var removeIdFromNewConstraints = function (list) {
            ko.utils.arrayForEach(list, function (constraint) {
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

                currentConstraint.Id(newConstraint.Id);
                currentConstraint.Number(newConstraint.Number);
                currentConstraint.IsNew(false);

                updateConstraints(currentConstraint.Children(), newConstraint.Children);
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
                    self.TemplateId(response.TemplateId);
                    self.Template().Id(response.TemplateId);
                    self.Template().AuthorId(response.AuthorId);
                    self.IsModified(false);

                    if (actionAfter == 'list') {
                        self.Status("Done saving... Redirecting to Browse Templates.");
                        location.href = '/TemplateManagement/List';
                    } else if (actionAfter == 'view') {
                        self.Status("Done saving... Redirecting to View Template.");
                        location.href = getTemplateViewUrl(self.Template().Id(), self.Template().Oid());
                    } else if (actionAfter == 'publishSettings') {
                        self.Status("Done saving... Redirecting to Publish Settings.");
                        location.href = '/TemplateManagement/PublishSettings?id=' + self.TemplateId();
                    } else {
                        self.Status("Done saving.");

                        // Empty the removed constraints list
                        self.RemovedConstraints([]);

                        // Update the constraints on the client (possibly new IDs and new Numbers)
                        updateConstraints(self.Constraints(), response.Constraints);
                        ko.mapping.fromJS({ ValidationResults: response.ValidationResults }, {}, self.Template());

                        // Clear the status after 10 seconds
                        setTimeout(function () {
                            self.Status("");
                        }, 10000);
                    }
                }
            },
            complete: function () {
                $.unblockUI();
            }
        });
    };

    self.GetImplementationGuideDisplay = function() {
        var implementationGuide = $.grep(self.ImplementationGuides(), function(a) {
            return a.Id == self.Template().OwningImplementationGuideId();
        });

        if (implementationGuide.length == 1) {
            return implementationGuide[0].Name;
        }

        return '';
    };

    self.GetTemplateTypeDisplay = function() {
        var templateType = $.grep(self.TemplateTypes(), function(a) {
            return a.Id == self.Template().TemplateTypeId();
        });

        if (templateType.length == 1) {
            return templateType[0].Name;
        }

        return '';
    };

    /* Determines if the user can cancel and view the template based
    * on whether or not the template editor represents a new template, or
    * an existing template.
    */
    self.CanCancelAndView = ko.computed(function () {
        return self.Template().Id();
    });

    /* Cancels editing a template by redirecting the user's browser to 
    * either the "Browse Templates" list or the "View Template" page.
    * The UI disables the "Cancel and View" button when editing a new
    * new template so that this method will not be called with the 'view'
    * option for a template that does not exist.
    */
    self.Cancel = function (action) {
        if (action == 'list') {
            location.href = '/TemplateManagement/List';
        } else if (action == 'view') {
            if (!self.TemplateId()) {
                return;
            }

            location.href = getTemplateViewUrl(self.Template().Id(), self.Template().Oid());
        }
    };

    self.InitializeCodeSystems = function () {
        self.CodeSystems([]);

        var startTime = new Date().getTime();
        $.ajax({
            url: '/api/Terminology/CodeSystem/Basic',
            method: 'GET',
            async: true,
            success: function (codeSystems) {
                var mapping = {
                    CodeSystems: {
                        create: function (options) {
                            return new CodeSystemModel(options.data);
                        }
                    }
                };
                ko.mapping.fromJS({ CodeSystems: codeSystems }, mapping, self);
                console.log('Done loading code systems: ' + (new Date().getTime() - startTime) + ' milliseconds');
            }
        });
    };

    self.ImplementationGuideBaseIdentifier = ko.computed(function () {
        if (!self.Template().OwningImplementationGuideId()) {
            return '';
        }

        var ig = _.find(self.ImplementationGuides(), function (ig) {
            return ig.Id == self.Template().OwningImplementationGuideId();
        });

        if (!ig || !ig.Identifier) {
            return '';
        }

        var initIdentifier = ig.Identifier;

        if (initIdentifier.indexOf('http://') == 0 || initIdentifier.indexOf('https://') == 0) {
            if (initIdentifier.lastIndexOf('/') != initIdentifier.length - 1) {
                initIdentifier += '/';
            }

            var initIdentifierAfix = 'StructureDefinition/';

            if (initIdentifier.lastIndexOf(initIdentifierAfix) != initIdentifier.length - initIdentifierAfix.length) {
                initIdentifier += 'StructureDefinition/';
            }
        }

        return initIdentifier;
    });

    self.InitializeImplementationGuides = function () {
        var startTime = new Date().getTime();
        $.ajax({
            url: '/api/ImplementationGuide/Editable',
            method: 'GET',
            async: false,
            success: function (implementationGuides) {
                var filtered = ko.utils.arrayFilter(implementationGuides, function (item) {
                    return item.Id == self.Template().OwningImplementationGuideId() || !item.IsPublished;
                });

                self.ImplementationGuides(implementationGuides);
                console.log('Done loading implementation guides: ' + (new Date().getTime() - startTime) + ' milliseconds');
            }
        });
    };

    self.InitializePublishStatuses = function (implementationGuideId) {
        self.PublishStatuses([]);

        if (!implementationGuideId) {
            return;
        }

        var startTime = new Date().getTime();
        $.ajax({
            url: '/api/Template/Edit/PublishStatus/' + implementationGuideId,
            method: 'GET',
            async: false,
            success: function (publishStatuses) {
                self.PublishStatuses(publishStatuses);
                console.log('Done loading publish statuses: ' + (new Date().getTime() - startTime) + ' milliseconds');
            }
        });
    };

    self.InitializeTemplateTypes = function (implementationGuideId) {
        var deferred = Q.defer();
        self.TemplateTypes([]);

        if (!implementationGuideId) {
            return Q.resolve([]);
        }

        var startTime = new Date().getTime();
        $.ajax({
            method: 'GET',
            url: '/api/ImplementationGuide/' + implementationGuideId + '/TemplateType',
            async: false,       // Knockout is not recognizing changes to self.TemplateTypes sporratically when this runs async:true
            success: function (templateTypes) {
                self.TemplateTypes(templateTypes);

                console.log('Done loading template/profile types (count: ' + self.TemplateTypes().length + '): ' + (new Date().getTime() - startTime) + ' milliseconds');
                deferred.resolve(templateTypes);
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    self.InitializeCategories = function (implementationGuideId) {
        self.Categories([]);

        if (!implementationGuideId) {
            return Q.resolve();
        }

        var deferred = Q.defer();        
        var startTime = new Date().getTime();

        $.ajax({
            method: 'GET',
            url: '/api/Template/Edit/' + implementationGuideId + '/Category',
            success: function (categories) {
                self.Categories(categories);
                console.log('Done loading categories: ' + (new Date().getTime() - startTime) + ' milliseconds');
                deferred.resolve();
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    self.InitializeAvailableExtensions = function (implementationGuideId) {
        self.AvailableExtensions([]);

        if (!implementationGuideId) {
            return Q.resolve();
        }
        
        var deferred = Q.defer();
        var startTime = new Date().getTime();

        $.ajax({
            url: '/api/Template/Edit/' + implementationGuideId + '/Extension',
            success: function (extensions) {
                self.AvailableExtensions(extensions);
                console.log('Done loading available extension templates/profiles: ' + (new Date().getTime() - startTime) + ' milliseconds');
                deferred.resolve();
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    /* Refreshes the template, constraint and nodes within the view model.
    * Ajax synchronous query is made for the template meta-data. The UI
    * is blocked until all data is done loading.
    * When refreshing a new template, the IsModified observable is set to
    * true immediately and the UI is unblocked separately because
    * the initializeConstraints() promise will not be resolved when 
    * editing a new template.
    */
    self.Refresh = function () {
        $.blockUI({ message: 'Loading template' });

        var defaultViewMode = $.cookie('templateEditorViewMode');

        if (defaultViewMode) {
            self.ViewMode(defaultViewMode);
        } else {
            $.cookie('templateEditorViewMode', 'Analyst')
        }

        self.Constraints([]);
        self.Nodes([]);
        self.CurrentNode(null);

        initializeTemplate()
            .then(function () {
                initializeFhirProperties();

                self.InitializePublishStatuses(self.Template().OwningImplementationGuideId());
                self.InitializeCategories(self.Template().OwningImplementationGuideId());
                self.InitializeAvailableExtensions(self.Template().OwningImplementationGuideId());

                return initializeConstraints();
            })
            .then(function() {
                return self.InitializeNodeLevel(null, true);
            })
            .then(function () {
                $.unblockUI();
            })
            .catch(function (err) {
                alert('An error occurred');
                console.log(err);
            });

        if (!self.TemplateId()) {
            self.IsModified(true);
            $.unblockUI();
        } else {
            self.IsModified(false);
        }
    };

    /* Initializes the template meta-data based on the view model's 
    * TemplateId() observable. After the template is loaded, the view
    * model subscribes to changes so that IsModified() can be updated
    * only AFTER the meta-data is loaded. The IsNew() is set to false
    * after the model has loaded.
    */
    var initializeTemplate = function () {
        if (!self.TemplateId()) {
            self.Template().SubscribeChanges();
            return Q.resolve(self.Template());
        }

        var deferred = Q.defer();
        var startTime = new Date().getTime();
        $.ajax({
            url: '/api/Template/Edit/' + self.TemplateId() + '/MetaData',
            async: false,
            success: function (result) {
                result.StatusId = result.StatusId ? result.StatusId : undefined;        // Convert null to undefined for select binding purposes

                var template = new TemplateModel(result, self);
                self.Template(template);
                self.Template().SubscribeChanges();
                self.Template().IsNew(false);

                document.title = getTitleBarDisplay();

                // Initialize the template types before we bind to self.Template so that it doesn't screw up the template's TemplateTypeId()
                self.InitializeTemplateTypes(template.OwningImplementationGuideId())
                    .then(function() {
                        console.log('Done loading template meta-data: ' + (new Date().getTime() - startTime) + ' milliseconds');
                        deferred.resolve(template);
                    })
                    .catch(function(err) {
                        deferred.reject(err);
                    });
            },
            error: function (err) {
                deferred.reject(err);
            }
        });

        return deferred.promise;
    };

    /* Initializes the constraints for the view model's TemplateId()
    * observable. After each of the constraints are loaded into the 
    * model, subscriptions are registered for the constraints so that
    * loading the constraint models doesn't trigger the IsModified() flag
    * to be set immediately. The ConstraintModel constructor is called
    * recursively within the ConstraintModel to load the models of each
    * child constraint. 
    */
    var initializeConstraints = function () {
        if (!self.TemplateId()) {
            return Q.resolve();
        }

        var deferred = Q.defer();
        var startTime = new Date().getTime();
        $.ajax({
            url: '/api/Template/Edit/' + self.TemplateId() + '/Constraint',
            async: true,
            success: function (result) {
                for (var i in result) {
                    var item = result[i];
                    var newConstraint = new ConstraintModel(item, null, self);
                    newConstraint.IsNew(false);
                    newConstraint.SubscribeForUpdates();

                    self.Constraints.push(newConstraint);
                };

                console.log('Done loading constraints: ' + (new Date().getTime() - startTime) + ' milliseconds');
                deferred.resolve();
            }
        });

        return deferred.promise;
    };

    self.InitializeCodeSystems();
    self.InitializeImplementationGuides();

    // Immediately begin refreshing the template meta-data, constraints and
    // schema nodes now that all observables and functions have been defined.
    self.Refresh();
};