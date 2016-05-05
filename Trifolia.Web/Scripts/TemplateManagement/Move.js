var MoveTemplateViewModel = function (templateId) {
    var self = this;

    self.Template = ko.observable(new MoveModel());
    self.Nodes = ko.observableArray([]);
    self.Constraints = ko.observableArray([]);
    self.MoveStep = ko.observable(1);
    self.ImplementationGuides = ko.observableArray([]);
    self.TemplateTypes = ko.observableArray([]);
    self.AppliesToNodes = ko.observableArray([]);
    self.CurrentAppliesToNode = ko.observable();

    self.ImplementationGuideChanged = function (newImplementationGuideId) {
        self.TemplateTypes([]);

        if (!newImplementationGuideId) {
            return;
        }

        $.ajax({
            url: '/api/ImplementationGuide/' + newImplementationGuideId + '/TemplateType',
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ TemplateTypes: results }, {}, self);
            }
        });
    };
    self.Template().ImplementationGuideId.subscribe(self.ImplementationGuideChanged);

    self.TemplateTypeChanged = function (newTemplateTypeId) {
        var foundTemplateType = ko.utils.arrayFirst(self.TemplateTypes(), function (templateType) {
            return templateType.Id() == newTemplateTypeId;
        });

        self.Template().PrimaryContext(foundTemplateType.RootContext());
        self.Template().PrimaryContextType(foundTemplateType.RootContextType());
        self.InitializeAppliesToNodeLevel(null);
    };

    /* Expands nodes in the "Applies To" dialog.
    * If the children of the node have not been loaded, performs an
    * ajax query to the server to get the child nodes (asynchronously).
    */
    self.ExpandAppliesToNode = function (parentNode) {
        if (!parentNode.AreChildrenLoaded()) {
            self.InitializeAppliesToNodeLevel(parentNode, function () {
                parentNode.IsExpanded(true);
            });
        } else {
            parentNode.IsExpanded(true);
        }
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

    /* Expands nodes in the "Applies To" dialog.
    * If the children of the node have not been loaded, performs an
    * ajax query to the server to get the child nodes (asynchronously).
    */
    self.ExpandAppliesToNode = function (parentNode) {
        if (!parentNode.AreChildrenLoaded()) {
            self.InitializeAppliesToNodeLevel(parentNode, function () {
                parentNode.IsExpanded(true);
            });
        } else {
            parentNode.IsExpanded(true);
        }
    };

    /* Initializes a level of schema nodes in the applies-to node-set.
    * This is only used for the "Applies To" dialog functionality.
    */
    self.InitializeAppliesToNodeLevel = function (node, callback) {
        if (!self.Template().ImplementationGuideId()) {
            return;
        }

        // Remove already existing nodes when initializing the root level
        if (!node) {
            self.AppliesToNodes.splice(0, self.AppliesToNodes().length);
        }

        var url = '/api/Template/Edit/Schema/' + self.Template().ImplementationGuideId() + '?';

        if (node) {
            url += 'parentType=' + encodeURIComponent(node.DataType()) + '&';
        } else {
            var rootContextType = '';

            for (var i in self.TemplateTypes()) {
                if (self.TemplateTypes()[i].Id() == self.Template().TemplateTypeId()) {
                    rootContextType = self.TemplateTypes()[i].RootContextType();
                    break;
                }
            }

            url += 'parentType=' + encodeURIComponent(rootContextType) + '&';
        }

        // Don't include attributes
        url += 'includeAttributes=false&';

        $.ajax({
            url: url,
            async: true,
            success: function (results) {
                ko.utils.arrayForEach(results, function (item) {
                    var newNode = new NodeModel(item);

                    if (node) {
                        node.Children.push(newNode);
                    } else {
                        self.AppliesToNodes.push(newNode);
                    }
                });

                if (callback) {
                    callback();
                }
            }
        });
    };

    self.Initialize = function () {
        $.ajax({
            url: '/api/ImplementationGuide/Editable',
            async: false,
            success: function (results) {
                ko.mapping.fromJS({ ImplementationGuides: results }, {}, self);
            }
        });

        $.ajax({
            url: '/api/Template/' + templateId + '/Move',
            success: function (results) {
                ko.mapping.fromJS({ Template: results }, {}, self);

                self.Template().TemplateTypeId.subscribe(self.TemplateTypeChanged);
                self.InitializeAppliesToNodeLevel(null);
            }
        });
    };

    self.MoveStep2 = function () {
        $.ajax({
            method: 'POST',
            url: '/api/Template/Move/Constraint',
            data: ko.mapping.toJS(self.Template()),
            success: function (results) {
                ko.mapping.fromJS({ Constraints: results }, {}, self);
                self.MoveStep(2);
                self.LoadLevel();
            }
        });
    };

    self.FindConstraint = function (parentNode, context) {
        var parentConstraint = parentNode != null ? parentNode.Constraint() : null;
        var list = ko.utils.arrayFilter(self.Constraints(), function (constraint) {
            if (constraint.IsDeleted()) {
                return false;
            }

            if (parentConstraint) {
                return constraint.ParentId() == parentConstraint.Id();
            }

            return !constraint.ParentId();
        });

        var constraints = ko.utils.arrayFilter(list, function (constraint) {
            return constraint.Context() == context;
        });

        return constraints;
    };

    self.GetChildConstraints = function(parentConstraint) {
        var childConstraints = ko.utils.arrayFilter(self.Constraints(), function (constraint) {
            if (constraint.IsDeleted()) {
                return false;
            }

            if (parentConstraint) {
                return constraint.ParentId() == parentConstraint.Id();
            }

            return !constraint.ParentId();
        });

        return childConstraints;
    };

    self.LoadLevel = function (parentNode) {
        var url = '/api/Template/Edit/Schema/' + self.Template().ImplementationGuideId();
        var parentConstraint = parentNode != null ? parentNode.Constraint() : null;
        var list = parentNode ? parentNode.ChildNodes : self.Nodes;
        var parentDataType = null;
        
        var childConstraints = self.GetChildConstraints(parentConstraint);
        var populateUnmatched = function () {
            // Find unmatched constraints and create nodes for them
            var unmatchedConstraints = ko.utils.arrayFilter(childConstraints, function (constraint) {
                var foundNode = ko.utils.arrayFirst(list(), function (nodeItem) {
                    return nodeItem.Constraint() == constraint;
                });

                return !foundNode;
            });

            ko.utils.arrayForEach(unmatchedConstraints, function (constraint) {
                var errorNode = new NodeModel();
                errorNode.Context(constraint.Context());
                errorNode.IsError(true);
                errorNode.Constraint(constraint);
                list.push(errorNode);

                self.LoadLevel(errorNode);
            });
        };

        if (parentNode) {
            if (!parentNode.IsError()) {
                parentDataType = parentConstraint && parentConstraint.DataType() != 'N/A' ? parentConstraint.DataType() : parentNode.DataType();
            }
        } else {
            parentDataType = self.Template().PrimaryContextType();
        }

        // Make a call to get the schema nodes from the server if we have a data type
        if (parentDataType) {
            $.ajax({
                url: url + '?parentType=' + encodeURIComponent(parentDataType),
                success: function (results) {
                    ko.utils.arrayForEach(results, function (result) {
                        var nodeModel = new NodeModel(result);
                        var matchingConstraints = self.FindConstraint(parentNode, result.Context);

                        for (var i = 0; i < matchingConstraints.length; i++) {
                            var constraintChildren = ko.utils.arrayFilter(self.Constraints(), function (constraint) {
                                return constraint.ParentId() == matchingConstraints[i].Id();
                            });

                            var nodeModel = new NodeModel(result);
                            nodeModel.Constraint(matchingConstraints[i]);
                            list.push(nodeModel);

                            if (constraintChildren.length > 0) {
                                self.LoadLevel(nodeModel);
                            }
                        }

                        // Add a node item even if there are no constraints
                        if (matchingConstraints.length == 0) {
                            list.push(new NodeModel(result));
                        }
                    });

                    populateUnmatched();
                },
                error: function () {
                    var errorNode = new NodeModel();
                    errorNode.Context('Error retrieving data from server');
                    list.push(errorNode);
                }
            });
        } else {
            populateUnmatched();
        }
    };

    self.RemoveConstraint = function(constraint, removeChildren) {
        constraint.IsDeleted(true);

        var childConstraints = self.GetChildConstraints(constraint);

        ko.utils.arrayForEach(childConstraints, function (childConstraint) {
            if (removeChildren) {
                self.RemoveConstraint(childConstraint, removeChildren);
            } else {
                childConstraint.ParentId(null);
            }
        });
    };

    self.RemoveConstraintButton = function (node) {
        if (!node.Constraint()) {
            return;
        }

        var removeChildren = confirm("Click OK to remove children, or cancel to move children up in the hierarchy.");
        self.RemoveConstraint(node.Constraint(), removeChildren);

        // Refresh the nodes
        self.Nodes([]);
        self.LoadLevel();
    };

    self.Cancel = function () {
        location.href = '/TemplateManagement/View/Id/' + templateId;
    };

    self.Finish = function () {
        var data = {
            Template: ko.mapping.toJS(self.Template()),
            Constraints: ko.mapping.toJS(self.Constraints())
        };

        $.ajax({
            method: 'POST',
            url: '/api/Template/Move',
            data: data,
            //dataType: 'json',
            //contentType: 'application/json; charset=utf-8',
            success: function () {
                alert("Successfully moved template!");
                location.href = '/TemplateManagement/View/Id/' + templateId;
            }
        });
    };

    self.Initialize();
};

var MoveModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'TemplateId', 'TemplateName', 'ImplementationGuideId', 'TemplateTypeId', 'PrimaryContext', 'PrimaryContextType', 'IsPublished' ]
    };

    self.TemplateId = ko.observable();
    self.TemplateName = ko.observable();
    self.ImplementationGuideId = ko.observable();
    self.TemplateTypeId = ko.observable();
    self.PrimaryContext = ko.observable();
    self.PrimaryContextType = ko.observable();
    self.IsPublished = ko.observable();

    ko.mapping.fromJS(data, mapping, self);

    self.Validation = ko.validatedObservable({
        ImplementationGuideId: self.ImplementationGuideId.extend({ required: true }),
        TemplateTypeId: self.TemplateTypeId.extend({ required: true }),
        PrimaryContext: self.PrimaryContext.extend({ required: { message: 'Primary Context is required.', params: true } }),
        PrimaryContextType: self.PrimaryContextType.extend({ required: { message: 'Primary Context Type is required.', params: true } })
    });

    self.IsValid = ko.computed(function () {
        return self.Validation().isValid();
    });
};

var NodeModel = function (data) {
    var self = this;

    self.Context = ko.observable();
    self.Conformance = ko.observable();
    self.Cardinality = ko.observable();
    self.DataType = ko.observable();
    self.HasChildren = ko.observable();

    self.Constraint = ko.observable();
    self.ChildNodes = ko.observableArray([]);
    self.IsError = ko.observable(false);

    // For the applies-to functionality
    self.Id = ko.observable(createUUID());
    self.IsExpanded = ko.observable(false); 
    self.AreChildrenLoaded = ko.observable(false);
    self.Children = ko.observableArray([]);

    ko.mapping.fromJS(data, {}, self);
};