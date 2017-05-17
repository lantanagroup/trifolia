var TemplateViewModel = function (templateId) {
    var self = this;
    var mapping = {
        'Constraints': {
            create: function (options) {
                return new ConstraintModel(options.data);
            }
        },
        'Actions': {
            create: function (options) {
                return new ActionModel(options.data);
            }
        },
        'Samples': {
            create: function (options) {
                return new XmlSampleModel(options.data);
            }
        }
    };

    /* Properties: Set by client */
    self.Initialized = ko.observable(false);

    /* Properties: Set by server */
    self.Id = ko.observable();
    self.Author = ko.observable();
    self.Name = ko.observable();
    self.Oid = ko.observable();
    self.Bookmark = ko.observable();
    self.ImplementationGuideType = ko.observable();
    self.ImplementationGuideTypeId = ko.observable();
    self.ImpliedTemplate = ko.observable();
    self.ImpliedTemplateId = ko.observable();
    self.ImpliedTemplateOid = ko.observable();
    self.ImpliedTemplateImplementationGuide = ko.observable();
    self.Description = ko.observable();
    self.Notes = ko.observable();
    self.ShowNotes = ko.observable();
    self.IsOpen = ko.observable();
    self.Organization = ko.observable();
    self.ImplementationGuide = ko.observable();
    self.ImplementationGuideId = ko.observable();
    self.Type = ko.observable();
    self.CanEdit = ko.observable();
    self.CanEditPublishSettings = ko.observable();
    self.CanCopy = ko.observable();
    self.CanVersion = ko.observable();
    self.CanDelete = ko.observable();
    self.CanEditGreen = ko.observable();
    self.CanMove = ko.observable();
    self.HasPreviousVersion = ko.observable();
    self.PreviousVersionTemplateName = ko.observable();
    self.PreviousVersionTemplateId = ko.observable();
    self.Status = ko.observable();
    self.HasGreenModel = ko.observable();
    self.Actions = ko.observableArray([]);
    self.Constraints = ko.observableArray([]);
    self.ContainedByTemplates = ko.observableArray([]);
    self.ContainedTemplates = ko.observableArray([]);
    self.Samples = ko.observableArray([]);
    self.Changes = ko.observable();
    self.ViewChangesMode = ko.observable('Inline');
    self.StructureDefinitionJSON = ko.observable('');
    self.StructureDefinitionXML = ko.observable('');

    /* Computables */
    self.Extensibility = ko.computed(function () {
        if (self.IsOpen()) {
            return "Open";
        } else {
            return "Closed";
        }
    });

    self.ImplementationGuideUrl = ko.computed(function () {
        return '/IGManagement/View/' + self.ImplementationGuideId();
    });

    self.ImpliedTemplateUrl = ko.computed(function () {
        return '/TemplateManagement/View/Id/' + self.ImpliedTemplateId();
    });

    self.PreviousVersionUrl = ko.computed(function () {
        return '/TemplateManagement/View/Id/' + self.PreviousVersionTemplateId();
    });

    self.GetChangeTooltip = function (changeType) {
        switch (changeType) {
            case 0:
                return 'Added';
            case 1:
                return 'Modified';
            case 2:
                return 'Removed';
            default:
                return '';
        }
    };

    /* Init */
    self.Initialize = function () {
        fireTrifoliaEvent('templateLoading');

        self.Changes(null);

        $.ajax({
            method: 'GET',
            url: '/api/Template/' + templateId,
            success: function (template) {
                ko.mapping.fromJS(template, mapping, self);
                self.Initialized(true);
                fireTrifoliaEvent('templateLoaded');

                var fhirIgType = _.find(trifoliaConfig.FhirIgTypes, function (fhirIgType) {
                    return fhirIgType.Id == self.ImplementationGuideTypeId();
                });

                if (fhirIgType) {
                    var url = fhirIgType.BaseUrl + 'StructureDefinition/' + self.Bookmark();
                    
                    $.ajax({
                        url: url,
                        success: function (results) {
                            self.StructureDefinitionJSON(JSON.stringify(results, null, '\t'));
                        },
                        error: handleAjaxError('Error getting JSON StructureDefinition for template/profile')
                    });

                    $.ajax({
                        url: url,
                        dataType: 'text',
                        headers: {
                            'Accept': 'application/xml'
                        },
                        success: function (results) {
                            var prettyXml = vkbeautify.xml(results)
                            self.StructureDefinitionXML(prettyXml);
                        },
                        error: handleAjaxError('Error getting XML StructureDefinition for template/profile')
                    });
                }
            },
            error: handleAjaxError('Error getting template/profile')
        });

        $.ajax({
            url: '/api/Template/' + templateId + '/Changes',
            success: function (results) {
                if (results) {
                    ko.mapping.fromJS({ Changes: results }, {}, self);
                }
            },
            error: handleAjaxError('Error getting changes for template/profile')
        });
    };

    self.Initialize();
};

var ConstraintModel = function (data) {
    var self = this;
    var mapping = {
        include: [ 'Prose', 'IsHeading', 'HeadingTitle', 'HeadingDescription', 'Description', 'Label', 'Children' ],
        'Children': {
            create: function (options) {
                return new ConstraintModel(options.data);
            }
        }
    };

    self.Prose = ko.observable();
    self.IsHeading = ko.observable();
    self.HeadingTitle = ko.observable();
    self.HeadingDescription = ko.observable();
    self.Description = ko.observable();
    self.Label = ko.observable();
    self.Children = ko.observableArray([]);

    self.ProseDisplay = ko.computed(function () {
        if (!self.Prose()) {
            return '';
        }

        if (self.Prose().indexOf("<p>") == 0) {
            return $(self.Prose())[0].innerHTML;
        }

        return self.Prose();
    });

    ko.mapping.fromJS(data, mapping, self);
};

var ActionModel = function (data) {
    var self = this;

    self.Text = ko.observable();
    self.Url = ko.observable();
    self.Disabled = ko.observable(false);
    self.ToolTip = ko.observable();

    ko.mapping.fromJS(data, {}, self);
};

var XmlSampleModel = function (data) {
    var self = this;

    self.Name = ko.observable();
    self.Sample = ko.observable();

    ko.mapping.fromJS(data, {}, self);
};