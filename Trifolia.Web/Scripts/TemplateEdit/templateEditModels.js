var NodeModel = function (data, parent, viewModel) {
    var self = this;
    var mapping = {
        ignore: ['AreChildrenLoaded', 'Parent', 'Children', 'IsExpanded', 'Constraint']
    };

    /* Properties: Set by client */
    self.Id = ko.observable(createUUID());
    self.AreChildrenLoaded = ko.observable(false);
    self.ChildrenLoadingPromise = ko.observable(null);
    self.Parent = ko.observable(parent);
    self.Children = ko.observableArray([]);
    self.IsExpanded = ko.observable(false);
    self.Constraint = ko.observable();

    /* Properties: Set by server */
    self.HasChildren = ko.observable(false);
    self.Context = ko.observable('');
    self.Conformance = ko.observable('');
    self.Cardinality = ko.observable('');
    self.DataType = ko.observable('');
    self.IsChoice = ko.observable(false);

    self.IsValid = ko.computed(function () {
        return !self.Constraint() || self.Constraint().IsValid();
    });

    self.IsChildOfChoice = function () {
        return self.Parent() && self.Parent().IsChoice();
    };

    /* Properties: Computable */
    self.DisplayValue = ko.computed(function () {
        if (!self.Constraint()) {
            return;
        }

        var constraintValue = self.Constraint().Value();
        var constraintDisplayName = self.Constraint().ValueDisplayName();

        if (constraintValue && constraintDisplayName) {
            return constraintValue + ", " + constraintDisplayName;
        } else if (constraintValue) {
            return constraintValue;
        } else if (constraintDisplayName) {
            return constraintDisplayName;
        }
    });

    self.IsAttribute = ko.computed(function() {
        return self.Context() && self.Context().indexOf("@") == 0;
    });

    self.HasWarning = ko.computed(function () {
        var hasWarning = false;

        if (self.Constraint() && self.Constraint().IsPrimitive()) {
            hasWarning = true;
        } else if (!self.Context() && self.Constraint()) {
            hasWarning = true;
        }

        return hasWarning;
    });

    self.DisplayTitle = ko.computed(function () {
        if (self.Constraint() && self.Constraint().IsPrimitive()) {
            return "This constraint is a primitive constraint. Primitive constraints are not recommended, unless absolutely necessary.";
        } else if (!self.Context() && self.Constraint()) {
            return "This constraint has an incorrect context of " + self.Constraint().Context() + " and cannot be matched to the schema. This is likely due to legacy/bad data. It is recommended to delete and re-create this constraint.";
        }

        return "";
    });

    self.DisplayContext = ko.computed(function () {
        if (!self.Context()) {
            if (self.Constraint() && self.Constraint().Context()) {
                return self.Constraint().Context();
            }

            return "N/A";
        }

        return self.Context();
    });

    self.ComputedNumber = ko.computed(function () {
        if (self.Constraint()) {
            var uniqueNumber = self.Constraint().Number();

            if (!uniqueNumber) {
                uniqueNumber = 'AUTO';
            }

            if (self.Constraint().DisplayNumber()) {
                return self.Constraint().DisplayNumber();
            }

            return uniqueNumber;
        }
    });

    self.ComputedNumberTooltip = ko.computed(function () {
        if (self.ComputedNumber() == 'AUTO') {
            return 'The conformance number will be automatically generated after the template has been saved.';
        }
    });

    self.DisplayConformance = ko.computed(function () {
        if (self.Constraint()) {
            return self.Constraint().Conformance();
        } else {
            return self.Conformance();
        }
    });

    self.DisplayCardinality = ko.computed(function () {
        if (self.Constraint()) {
            return self.Constraint().Cardinality();
        } else {
            return self.Cardinality();
        }
    });

    self.DisplayDataType = ko.computed(function () {
        if (self.Constraint() && self.Constraint().DataType()) {
            return self.Constraint().DataType();
        } else {
            return self.DataType();
        }
    });

    self.DisplayBranchRoot = ko.computed(function () {
        if (self.Constraint()) {
            return self.Constraint().IsBranch() ? "Yes" : "No";
        }
    });

    self.DisplayBranchIdentifier = ko.computed(function () {
        if (self.Constraint()) {
            return self.Constraint().IsBranchIdentifier() ? "Yes" : "No";
        }
    });

    self.DisplayNotes = ko.computed(function () {
        if (self.Constraint()) {
            return self.Constraint().Notes();
        }
    });

    /* Methods */
    self.RemoveConstraint = function () {
        // Remove all children constraints
        ko.utils.arrayForEach(self.Children(), function (nodeItem) {
            nodeItem.RemoveConstraint();
        });

        self.Constraint(null);
    };

    self.CreateComputable = function () {
        var createDefaultComputable = function (node) {
            var parentConstraint = node.Parent() ? node.Parent().Constraint() : null;
            var constraint = new ConstraintModel(null, parentConstraint, viewModel);
            var siblingNodes = node.Parent() ? node.Parent().Children : viewModel.Nodes;
            var siblingConstraints = node.Parent() && node.Parent().Constraint() ? node.Parent().Constraint().Children : viewModel.Constraints;
            var nodeIndex = siblingNodes.indexOfId(node);
            var constraintIndex = 0;

            // Determine where to place the constraint in the list. This is important so that when saving,
            // the list is sent back to the server in the correct order.
            for (var i = nodeIndex - 1; i >= 0; i--) {
                var siblingConstraint = siblingNodes()[i].Constraint();

                if (siblingConstraint) {
                    constraintIndex = siblingConstraints.indexOfId(siblingConstraint) + 1;
                    break;
                }
            }

            constraint.Context(node.DisplayContext());
            constraint.Conformance(node.DisplayConformance());
            constraint.Cardinality(node.DisplayCardinality());
            constraint.IsChoice(node.IsChoice());
            constraint.IsPrimitive(false);

            // Set as branch identifier if parent is a branch root, default conformance is SHALL
            if (parentConstraint && parentConstraint.IsBranch() && constraint.Conformance() == "SHALL") {
                constraint.IsBranchIdentifier(true);
            }

            if (constraintIndex <= siblingConstraints().length - 1) {
                siblingConstraints.splice(constraintIndex, 0, constraint);
            } else {
                siblingConstraints.push(constraint);
            }
            node.Constraint(constraint);
            constraint.ConstraintAndProseChanged();
            constraint.SubscribeForUpdates();
        };

        // Make sure the parent node has a computable before creating this node's computable.
        // because we are calling CreateComputable() on the parent, this will recurse all the way to the root level nodes
        if (self.Parent() && !self.Parent().Constraint()) {
            self.Parent().CreateComputable();
        }

        createDefaultComputable(self);
    };

    /* Constructor */
    ko.mapping.fromJS(data, mapping, self);
};

var ConstraintModel = function (data, parent, viewModel) {
    var self = this;
    var mapping = {
        include: ['Id', 'Number', 'Context', 'Conformance', 'Cardinality', 'DataType', 'Children', 'IsBranch', 'IsBranchIdentifier', 'PrimitiveText',
            'ContainedTemplateId', 'ValueConformance', 'Binding', 'Value', 'ValueConformance', 'ValueDisplayName', 'ValueSetId', 'ValueSetDate',
            'ValueCodeSystemId', 'Description', 'Notes', 'Label', 'IsPrimitive', 'IsHeading', 'HeadingDescription', 'IsSchRooted', 'IsInheritable',
            'Schematron', 'IsNew', 'Category', 'DisplayNumber', 'MustSupport', 'IsModifier', 'IsChoice', 'IsFixed'],
        ignore: ['Parent', 'BindingType', 'IsValueSetStatic', 'Order', 'IsAutomaticSchematron', 'IsStatic'],
        Children: {
            create: function (options) {
                var newChildConstraint = new ConstraintModel(options.data, self, viewModel);
                newChildConstraint.SubscribeForUpdates();
                return newChildConstraint;
            }
        }
    };

    /* Set by server */
    self.Id = ko.observable(createUUID());          // Id is automatically set by server for existing constraints, but generated by client for new constraints
    self.IsNew = ko.observable(true);

    self.Number = ko.observable();
    self.DisplayNumber = ko.observable();
    self.Context = ko.observable();
    self.Conformance = ko.observable();
    self.Cardinality = ko.observable();
    self.DataType = ko.observable();
    self.Children = ko.observableArray([]);
    self.Parent = ko.observable(parent);
    self.IsBranch = ko.observable();
    self.IsBranchIdentifier = ko.observable();
    self.PrimitiveText = ko.observable();
    self.ContainedTemplateId = ko.observable();
    self.Binding = ko.observable();
    self.Value = ko.observable();
    self.ValueConformance = ko.observable();
    self.ValueDisplayName = ko.observable();
    self.ValueSetId = ko.observable();
    self.ValueSetDate = ko.observable();
    self.ValueCodeSystemId = ko.observable();
    self.Description = ko.observable();
    self.Notes = ko.observable();
    self.Label = ko.observable();
    self.IsPrimitive = ko.observable();
    self.IsHeading = ko.observable(false);
    self.HeadingDescription = ko.observable();
    self.IsSchRooted = ko.observable(false);
    self.IsInheritable = ko.observable(true);
    self.Schematron = ko.observable();
    self.NarrativeProseHtml = ko.observable();
    self.Category = ko.observable('');
    self.IsModifier = ko.observable(false);
    self.MustSupport = ko.observable(true);
    self.IsChoice = ko.observable(false);
    self.IsFixed = ko.observable(false);

    self.IsChildOfChoice = ko.computed(function () {
        if (!self.Parent() || !self.Parent().IsChoice()) {
            return false;
        }

        return true;
    });
    
    self.Categories = ko.computed({
        read: function () {
            var categories = [];

            if (self.Category()) {
                var categorySplit = self.Category().split(',');

                for (var i in categorySplit) {
                    var category = categorySplit[i].replace(/\#\#\#/g, ",");
                    categories.push(category);
                }
            }

            return categories;
        }, write: function (value) {
            var cleanCategories = [];

            for (var i in value) {
                var category = value[i];
                category = category.replace(/,/g, "###");
                cleanCategories.push(category);
            }

            var writeCategory = cleanCategories.join(',');
            self.Category(writeCategory);
        }
    });

    /* Set by client */
    self.BindingType = ko.observable('None');

    /* Computables */
    self.IsAnalystComputable = ko.computed(function () {
        return viewModel.IsAnalyst() && viewModel.IsEditorMaximized() && !self.IsPrimitive();
    });

    self.IsAnalystPrimitive = ko.computed(function () {
        return viewModel.IsAnalyst() && viewModel.IsEditorMaximized && self.IsPrimitive();
    });

    self.IsEngineer = ko.computed(function () {
        return viewModel.IsEngineer() && viewModel.IsEditorMaximized();
    });

    self.IsTechEditorComputable = ko.computed(function () {
        return viewModel.IsTechEditor() && viewModel.IsEditorMaximized() && !self.IsPrimitive();
    });

    self.IsTechEditorPrimitive = ko.computed(function () {
        return viewModel.IsTechEditor() && viewModel.IsEditorMaximized() && self.IsPrimitive();
    });

    self.IsAutomaticSchematron = ko.computed({
        read: function () {
            return self.Schematron() == null || self.Schematron() == '';
        },
        write: function (value) {
            if (value) {
                self.Schematron('');
            } else {
                self.Schematron('not(tested)');
            }
        },
        owner: this
    });

    self.IsStatic = ko.computed({
        read: function () {
            if (self.Binding() == 'STATIC') {
                return true;
            }

            return false;
        },
        write: function (val) {
            if (val == true) {
                self.Binding('STATIC');
            } else if (val == false) {
                self.Binding('DYNAMIC');
            } else {
                self.Binding('');
            }
        }
    });

    self.IsBranchDisabled = ko.computed(function () {
        if (self.IsBranch()) {
            return false;
        }

        var isAttribute = self.Context() && self.Context().length > 0 ? self.Context().indexOf('@') == 0 : false;
        return (!self.IsBranch() && self.IsBranchIdentifier()) || isAttribute;
    });

    self.IsBranchIdentifierDisabled = ko.computed(function () {
        if (!self.IsBranchIdentifier() && self.IsBranch()) {
            return true;
        } else if (self.IsBranchIdentifier()) {
            return false;
        }

        var parent = self.Parent();
        var foundBranch = false;
        while (parent) {
            if (parent.IsBranch()) {
                foundBranch = true;
                break;
            }
            parent = parent.Parent();
        }

        return !foundBranch;
    });

    /* Methods */
    self.ConstraintAndProseChanged = function () {
        var data = ko.mapping.toJS(self);

        // Mark the template as changed
        viewModel.IsModified(true);

        // Update the narrative prose
        $.ajax({
            url: '/api/Template/Edit/Prose',
            method: 'POST',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            async: true,
            success: function (narrativeProse) {
                if (narrativeProse && narrativeProse.length > 0) {
                    narrativeProse = narrativeProse.replace('\n', '<br/>\n');
                }
                self.NarrativeProseHtml(narrativeProse);
            }
        });
    };

    self.ConstraintChanged = function () {
        // Mark the template as changed
        viewModel.IsModified(true);
    };

    self.ConformanceChanged = function (newValue) {
        if (self.Parent() && self.Parent().IsBranch() && self.Conformance() == "SHALL") {
            var msg = "The parent constraint is a branch root. Do you want to mark this constraint as a branch identifier?";
            if (!self.IsBranchIdentifier() && confirm(msg)) {
                self.IsBranchIdentifier(true);
            }
        }
    };

    self.BranchRootChanged = function (newValue) {
        if (!newValue) {
            var childIdentifiers = [];
            var recursivelyFindIdentifiers = function (parent) {
                ko.utils.arrayForEach(parent.Children(), function (child) {
                    if (!child.IsBranch() && child.IsBranchIdentifier()) {
                        childIdentifiers.push(child);
                    }

                    recursivelyFindIdentifiers(child);
                });
            };

            recursivelyFindIdentifiers(self);

            var msg = 'Do you want to remove the "branch identifier" flag from ' + childIdentifiers.length + ' constraints?';

            if (childIdentifiers.length > 0 && confirm(msg)) {
                ko.utils.arrayFilter(childIdentifiers, function (childIdentifier) {
                    childIdentifier.IsBranchIdentifier(false);
                });
            }
        }
    };

    var conformanceSub, cardinalitySub, dataTypeSub, containedTemplateIdSub,
        valueSub, valueDisplayNameSub, valueConformanceSub, valueSetIdSub,
        valueCodeSystemIdSub, valueSetDateSub, primitiveTextSub, isBranchSub,
        isBranchIdentifierSub, primitiveTextSub, bindingSub, isHeadingSub,
        headingDescriptionSub, descriptionSub, labelSub, numberSub, notesSub,
        schematronSub, isSchRootedSub, isInheritableSub, categorySub,
        mustSupportSub, isFixedSub, isModifierSub;

    self.SubscribeForUpdates = function () {
        categorySub = self.Category.subscribe(function (newVal) {
            console.log("Conformance (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        conformanceSub = self.Conformance.subscribe(function (newVal) {
            console.log("Conformance (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        cardinalitySub = self.Cardinality.subscribe(function (newVal) {
            console.log("Cardinality (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        dataTypeSub = self.DataType.subscribe(function (newVal) {
            console.log("Data Type (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        containedTemplateIdSub = self.ContainedTemplateId.subscribe(function (newVal) {
            console.log("Contained Template (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueSub = self.Value.subscribe(function (newVal) {
            console.log("Value (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueDisplayNameSub = self.ValueDisplayName.subscribe(function (newVal) {
            console.log("Value Display Name (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueConformanceSub = self.ValueConformance.subscribe(function (newVal) {
            console.log("Value Conformance (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueSetIdSub = self.ValueSetId.subscribe(function (newVal) {
            console.log("Value Set (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueCodeSystemIdSub = self.ValueCodeSystemId.subscribe(function (newVal) {
            console.log("Code System (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        valueSetDateSub = self.ValueSetDate.subscribe(function (newVal) {
            console.log("Value Set Date (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        primitiveTextSub = self.PrimitiveText.subscribe(function (newVal) {
            console.log("Primitive Text (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        isBranchSub = self.IsBranch.subscribe(function (newVal) {
            console.log("Branch Root (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        bindingSub = self.Binding.subscribe(function (newVal) {
            console.log("Binding (and prose) changed");
            self.ConstraintAndProseChanged(newVal);
        });
        isBranchIdentifierSub = self.IsBranchIdentifier.subscribe(function (newVal) {
            console.log("Branch Identifier changed");
            self.ConstraintAndProseChanged(newVal);
        });
        isHeadingSub = self.IsHeading.subscribe(function (newVal) {
            console.log("Heading changed");
            self.ConstraintChanged(newVal);
        });
        headingDescriptionSub = self.HeadingDescription.subscribe(function (newVal) {
            console.log("Heading Description changed");
            self.ConstraintChanged(newVal);
        });
        descriptionSub = self.Description.subscribe(function (newVal) {
            console.log("Description changed");
            self.ConstraintChanged(newVal);
        });
        labelSub = self.Label.subscribe(function (newVal) {
            console.log("Label changed");
            self.ConstraintAndProseChanged(newVal);
        });
        //numberSub = self.Number.subscribe(self.ConstraintAndProseChanged);
        notesSub = self.Notes.subscribe(function (newVal) {
            console.log("Notes changed");
            self.ConstraintChanged(newVal);
        });
        schematronSub = self.Schematron.subscribe(function (newVal) {
            console.log("Schematron changed");
            self.ConstraintChanged(newVal);
        });
        isSchRootedSub = self.IsSchRooted.subscribe(function (newVal) {
            console.log("SCH Rooted changed");
            self.ConstraintChanged(newVal);
        });
        isInheritableSub = self.IsInheritable.subscribe(function (newVal) {
            console.log("Inheritable changed");
            self.ConstraintChanged(newVal);
        });
        mustSupportSub = self.MustSupport.subscribe(function (newVal) {
            console.log('Must Support changed');
            self.ConstraintChanged(newVal);
        });
        isFixedSub = self.IsFixed.subscribe(function (newVal) {
            console.log('Is Fixed changed');
            self.ConstraintChanged(newVal);
        });
        isModifierSub = self.IsModifier.subscribe(function (newVal) {
            console.log('Is Modifier changed');
            self.ConstraintChanged(newVal);
        });

        self.Conformance.subscribe(self.ConformanceChanged);
        self.IsBranch.subscribe(self.BranchRootChanged);
    };

    /* Constructor */
    ko.mapping.fromJS(data, mapping, self);

    // Convert nulls to undefined for things that are bound via options with a caption
    if (self.ValueCodeSystemId() == null)
        self.ValueCodeSystemId(undefined);

    if ((self.Value() || self.ValueDisplayName()) && !self.ValueSetId()) {
        self.BindingType('SingleValue');
    } else if (!self.Value() && !self.ValueDisplayName() && self.ValueSetId()) {
        self.BindingType('ValueSet');
    } else if (!self.Value() && !self.ValueDisplayName() && !self.ValueSetId() && self.ValueCodeSystemId()) {
        self.BindingType('CodeSystem');
    } else if (self.Value() || self.ValueDisplayName() || self.ValueSetId() || self.ValueSetDate() || self.ValueCodeSystemId()) {
        self.BindingType('Other');
    }

    self.BindingType.subscribe(function (newValue) {
        var singleValueHasValue = self.ValueCodeSystemId() || self.ValueSetId() || self.ValueSetDate() || self.ValueConformance();
        var valueSetHasValue = self.Value() || self.ValueDisplayName() || self.ValueCodeSystemId();;
        var codeSystemHasValue = self.Value() || self.ValueDisplayName() || self.ValueSetId();
        var noneHasValue = self.Value() || self.ValueDisplayName() || self.ValueSetId() || self.ValueCodeSystemId() || self.ValueSetDate() || self.Binding() != 'DEFAULT';

        if (newValue == 'SingleValue') {
            if (singleValueHasValue && confirm('Do you want to remove the values from the "Code System", "Value Set", "Binding Date", and "Value Conformance" fields?')) {
                self.ValueCodeSystemId(null);
                self.ValueSetId("");
                self.ValueSetDate("");
                self.ValueConformance("");
                self.Binding("STATIC");
            }
        } else if (newValue == 'ValueSet') {
            if (valueSetHasValue && confirm('Do you want to remove the values from the "Code", "Display" and "Code System" fields?')) {
                self.Value("");
                self.ValueDisplayName("");
                self.ValueCodeSystemId(null);
            }
        } else if (newValue == 'CodeSystem') {
            if (codeSystemHasValue && confirm('Do you want to remove the values from the "Code", "Display", and "Value Set" fields?')) {
                self.Value("");
                self.ValueDisplayName("");
                self.ValueSetId("");
            }
        } else if (newValue == 'None') {
            if (noneHasValue && confirm('Do you want to remove the values from all binding-related fields?')) {
                self.Value("");
                self.ValueDisplayName("");
                self.ValueSetId("");
                self.ValueCodeSystemId(null);
                self.ValueSetDate("");
                self.Binding(null);
            }
        }
    });

    var validationOptions = {
        Cardinality: self.Cardinality.extend({ maxLength: 8, constraintCardinalityRequired: self.IsChildOfChoice, constraintCardinalityFormat: self.IsModifier })
    }

    var validation = ko.validatedObservable(validationOptions);

    self.IsValid = ko.computed(function () {
        return validation.isValid();
    });

    self.ValidationMessages = ko.computed(function () {
        var messages = [];

        var addMessage = function (obj) {
            if (!obj || !obj.isValid || obj.isValid() || !obj.error) {
                return;
            }

            messages.push(obj.error());
        };

        for (var i in validationOptions) {
            addMessage(validationOptions[i]);
        }

        return messages.join('<br/>\n');
    });
};

var TemplateExtensionModel = function (data, parent) {
    var self = this;
    var mapping = {
        include: [ 'Identifier', 'Type', 'Value' ]
    };

    self.Identifier = ko.observable('');
    self.Type = ko.observable('');
    self.Value = ko.observable('');

    var readCoding = function (index) {
        return function () {
            if (!self.Value()) {
                return '';
            }

            var valueSplit = self.Value().split('|');

            if (valueSplit.length != 3) {
                return '';
            }

            return valueSplit[index];
        };
    };

    var writeCoding = function (index) {
        return function (value) {
            var valueSplit;

            if (self.Value()) {
                valueSplit = self.Value().split('|');
            }

            if (!valueSplit || valueSplit.length != 3) {
                valueSplit = ['', '', ''];
            }

            valueSplit[index] = value;

            self.Value(valueSplit.join('|'));
        };
    };

    self.ValueCodingCode = ko.computed({
        read: readCoding(0),
        write: writeCoding(0)
    });

    self.ValueCodingDisplay = ko.computed({
        read: readCoding(1),
        write: writeCoding(1)
    });

    self.ValueCodingSystem = ko.computed({
        read: readCoding(2),
        write: writeCoding(2)
    });

    var validation = ko.validatedObservable({
        Identifier: self.Identifier.extend({
            required: { message: 'Identifier is required.' },
            extensionIdentifierFormat: true,
            extensionIdentifierUnique: { template: parent, thisExtension: self },
            maxLength: 255
        }),
        Type: self.Type.extend({ required: { message: 'Type is required.' }, maxLength: 50 }),
        Value: self.Value.extend({ required: { message: 'Value is required.' }, maxLength: 255 })
    });

    self.IsValid = ko.computed(function () {
        return validation.isValid();
    });

    ko.mapping.fromJS(data, mapping, self);
};

var TemplateModel = function (data, viewModel) {
    var self = this;
    var mapping = {
        include: [ 'Id', 'Name', 'Oid', 'Bookmark', 'IsOpen', 'TemplateTypeId', 'PrimaryContext', 'PrimaryContextType',
            'OwningImplementationGuideId', 'ImpliedTemplateId', 'StatusId', 'Description', 'Notes', 'Author', 'AuthorId',
            'PreviousVersionLink', 'PreviousVersionName', 'PreviousVersionOid', 'OrganizationName', 'MoveUrl', 'TemplateTypeAbbreviation', 'Locked',
            'ContainedByTemplates', 'ImpliedByTemplates', 'Extensions' ],
        ignore: ['SubscribeChanges', 'IsValid'],
        Extensions: {
            create: function (options) {
                return new TemplateExtensionModel(options.data, self);
            }
        }
    };

    var templateChanged = function () {
        viewModel.IsModified(true);
    };

    var updatePrimaryContextType = function (newPrimaryContextType) {
        // Remove all nodes (but not constraints) from the view model when the context of the template changes
        viewModel.Nodes([]);

        // Re-initialize the top-level of nodes
        viewModel.InitializeNodeLevel(null);
    };

    var templateOidChanged = function(newTemplateOid) {
        if (viewModel.IsFhirExtension()) {
            var urlConstraints = $.grep(viewModel.Constraints(), function(a) {
                return a.Context() == '@url' && !a.Parent();
            });

            if (urlConstraints.length > 0) {
                // Set this in case the user doesn't specify an oid for the template before selecting the template type
                // (which automatically sets the binding type to SingleValue only if a value is specified on the constraint first)
                urlConstraints[0].BindingType('SingleValue');
                urlConstraints[0].Value(newTemplateOid);
                urlConstraints[0].ConstraintAndProseChanged();
            }
        }
    };

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Oid = ko.observable();
    self.Bookmark = ko.observable();
    self.IsOpen = ko.observable(true);
    self.TemplateTypeId = ko.observable();
    self.PrimaryContext = ko.observable();
    self.PrimaryContextType = ko.observable();
    self.OwningImplementationGuideId = ko.observable();
    self.ImpliedTemplateId = ko.observable();
    self.StatusId = ko.observable();
    self.Description = ko.observable();
    self.Notes = ko.observable();
    self.ContainedByTemplates = ko.observableArray([]);
    self.ImpliedByTemplates = ko.observableArray([]);
    self.Extensions = ko.observableArray([]);
    self.NewExtension = ko.observable(new TemplateExtensionModel(null, self));
    self.AuthorId = ko.observable();
    self.Author = ko.observable();

    self.IsOpenString = ko.computed({
        read: function () { return (self.IsOpen() ? "true" : "false"); },
        write: function (value) { self.IsOpen(value == "true" ? true : false); }
    });
    
    // Fields for information purposes only
    self.PreviousVersionLink = ko.observable();
    self.PreviousVersionName = ko.observable();
    self.PreviousVersionOid = ko.observable();
    self.OrganizationName = ko.observable();
    self.MoveUrl = ko.observable();
    self.TemplateTypeAbbreviation = ko.observable();
    self.Locked = ko.observable(false);
    self.ValidationResults = ko.observableArray([]);

    self.IsNew = ko.observable(true);

    self.IdentifierPrefix = ko.computed({
        read: function () {
            var identifier = self.Oid();
            var prefix = '';

            if (!identifier) {
                return '';
            }

            var baseIdentifier = viewModel.ImplementationGuideBaseIdentifier();

            if (baseIdentifier && identifier.indexOf(baseIdentifier) == 0) {
                prefix = baseIdentifier;
            } else if (identifier.indexOf('http://') == 0 || identifier.indexOf('https://') == 0) {
                prefix = identifier.substring(0, identifier.lastIndexOf('/') + 1);
            } else if (identifier.indexOf('urn:oid:') == 0) {
                prefix = identifier.substring(0, 8);
            } else if (identifier.indexOf('urn:hl7ii:') == 0) {
                prefix = identifier.substring(0, 10);
            }

            return prefix;
        },
        write: function (val) {
            var afix = self.IdentifierAfix();
            self.Oid(val + afix);
        }
    });

    self.IdentifierAfix = ko.computed({
        read: function () {
            if (!self.Oid()) {
                return '';
            }

            var prefix = self.IdentifierPrefix();
            var afix = self.Oid().substring(prefix.length);
            return afix;
        },
        write: function (val) {
            var prefix = self.IdentifierPrefix();
            self.Oid(prefix + val);
        }
    });

    var validation = ko.validatedObservable({
        Name: self.Name.extend({ required: { message: 'Name is required.' }, maxLength: 255 }),
        Oid: self.Oid.extend({ required: { message: 'Identifier is required.' }, maxLength: 255, templateOidFormat: true, templateIdentifierUnique: self.Id}),
        Bookmark: self.Bookmark.extend({ required: { message: 'Bookmark is required.' }, maxLength: 40 }),
        IsOpen: self.IsOpen.extend({ required: { message: 'Extensibility is required.' } }),
        TemplateTypeId: self.TemplateTypeId.extend({ required: { message: 'Template/Profile Type is required.' } }),
        PrimaryContext: self.PrimaryContext.extend({ required: { message: 'Primary Context is required.' } }),
        PrimaryContextType: self.PrimaryContextType.extend({ required: { message: 'Primary Context Type is required.' } }),
        OwningImplementationGuideId: self.OwningImplementationGuideId.extend({ required: { message: 'Owning Implementation Guide is required.' } }),
        IdentifierPrefix: self.IdentifierPrefix.extend({ required: { message: 'Identifier\'s prefix is required.' } }),
        IdentifierAfix: self.IdentifierAfix.extend({ required: { message: 'Identifier\'s afix is required.' } }),
        ImpliedTemplateId: self.ImpliedTemplateId.extend({
            ignoreSelfTemplateId: {
                impliedId: self.ImpliedTemplateId,
                id: self.Id
            }})
    });

    self.AddExtension = function () {
        self.Extensions.push(self.NewExtension());
        self.NewExtension(new TemplateExtensionModel(null, self));
    };

    self.RemoveExtension = function (index) {
        self.Extensions.splice(index, 1);
    };

    self.ValidationWarnings = ko.computed(function () {
        return ko.utils.arrayFilter(self.ValidationResults(), function (item) {
            return item.Level() == 'Warning';
        });
    });

    self.ValidationErrors = ko.computed(function () {
        return ko.utils.arrayFilter(self.ValidationResults(), function (item) {
            return item.Level() == 'Error';
        });
    });

    self.IsValid = ko.computed(function () {
        for (var i in self.Extensions()) {
            if (!self.Extensions()[i].IsValid()) {
                return false;
            }
        }

        return validation.isValid();
    });

    self.DisableTypeFields = ko.computed(function () {
        return !self.IsNew();
    });

    self.DisableAppliesToButton = ko.computed(function () {
        return self.DisableTypeFields() || viewModel.IsFhir();
    });

    self.DisableOidField = ko.computed(function() {
        return !self.TemplateTypeId() || self.DisableFields();
    });

    self.DisableFields = ko.computed(function () {
        if (self.IsNew()) {
            return false;
        }

        return !viewModel.IsAnalyst() || self.Locked();
    });

    self.DisableEngineerFields = ko.computed(function () {
        if (self.IsNew()) {
            return false;
        }

        return viewModel.IsEngineer() || self.Locked();
    });

    self.OldTemplateEditorUrl = ko.computed(function () {
        if (self.Id()) {
            return '/Account/Edit/ModifyTemplateSettings.aspx?Id=' + self.Id();
        } else {
            return '/Account/Edit/ModifyTemplateSettings.aspx';
        }
    });

    self.SaveDisabled = ko.computed(function () {
        return self.Locked() || !self.IsValid();
    });

    var templateBookmarkChanged = function (newValue, oldValue) {
        var baseIdentifier = viewModel.ImplementationGuideBaseIdentifier();
        var isUrl = baseIdentifier ? baseIdentifier.indexOf('http://') == 0 || baseIdentifier.indexOf('https://') == 0 : false;

        if (!isUrl) {
            return;
        }

        if (!self.IdentifierAfix() || (oldValue && oldValue.toLowerCase() == self.IdentifierAfix().toLowerCase())) {
            self.IdentifierAfix(newValue);
        }

        templateChanged();
    };

    self.permittedTemplateUsers = function (tId) {
        var userInfo;
        var users = {};

        $.ajax({
            url: "/api/Template/Permissions/" + tId,
            async: false,
            success: function (results) {
                userInfo = results;
            }
        });

        userInfo.sort(function (a, b) {
            var aL = a.Name.toLowerCase(), bL = b.Name.toLowerCase();

            if (aL > bL) return 1;
            if (aL < bL) return -1;
            else return 0;
        });

        return userInfo;
    }
    

    /* Methods */
    self.SubscribeChanges = function () {
        self.Name.subscribe(templateChanged);
        self.Oid.subscribe(templateChanged);
        self.Bookmark.subscribeChanged(templateBookmarkChanged);
        self.IsOpen.subscribe(templateChanged);
        self.TemplateTypeId.subscribe(templateChanged);
        self.PrimaryContext.subscribe(templateChanged);
        self.PrimaryContextType.subscribe(templateChanged);
        self.OwningImplementationGuideId.subscribe(templateChanged);
        self.ImpliedTemplateId.subscribe(templateChanged);
        self.StatusId.subscribe(templateChanged);
        self.Description.subscribe(templateChanged);
        self.Notes.subscribe(templateChanged);
        self.Author.subscribe(templateChanged);

        self.PrimaryContextType.subscribe(updatePrimaryContextType);
        self.Oid.subscribe(templateOidChanged);
    };

    // Map data to object
    ko.mapping.fromJS(data, mapping, self);
};

var CodeSystemModel = function (data) {
    var self = this;

    self.Id = ko.observable();
    self.Name = ko.observable();
    self.Oid = ko.observable();

    self.Display = ko.computed(function () {
        return self.Name() + ' (' + self.Oid() + ')';
    });

    ko.mapping.fromJS(data, {}, self);
};