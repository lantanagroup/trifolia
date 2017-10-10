angular.module('Trifolia').controller('NewTemplateController', ['$scope', '$uibModal', 'ImplementationGuideService', 'EditorService', 'TemplateService', 'blockUI', function ($scope, $uibModal, ImplementationGuideService, EditorService, TemplateService, blockUI) {
    
    function init() {
        initVars();

        //Initialize selectable implementation guides
        ImplementationGuideService.getEditable()
            .then(function (implementationGuides) {
                $scope.implementationGuides = implementationGuides;
            });
    }

    function initVars() {
        $scope.template = {};
        $scope.nodes = [];
        $scope.implementationGuides = {};
        $scope.templateTypes = {};
        $scope.identifier = {
            base: "",
            ext: ""
        }
    }

    function createBookmark() {
        var foundImplementationGuide = _.find($scope.implementationGuides, function (implementationGuide) {
            return implementationGuide.Id == $scope.template.OwningImplementationGuideId;
        });
        
        $scope.template.Bookmark = TemplateService.generateBookmark($scope.template.Name, foundImplementationGuide && foundImplementationGuide.Namespace === 'http://hl7.org/fhir');
    };

    function updateIdentifier() {
        if ($scope.identifier.base) {
            $scope.template.Oid = $scope.identifier.base + $scope.identifier.ext;
        }

        $scope.isIdentifierUnique = false;
        $scope.isIdentifierValid = isIdentifierValid();
        $scope.isIdentifierRightSize = isIdentifierRightSize();

        TemplateService.validateIdentifier($scope.template.Oid)
            .then(function (isUnique) {
                $scope.isIdentifierUnique = isUnique;

                if (!$scope.isIdentifierUnique || !$scope.isIdentifierValid || !$scope.isIdentifierRightSize) {
                    $scope.invalid = true;
                }
                else {
                    $scope.invalid = false;
                }
            });
    };

    function isIdentifierValid() {
        var foundMatch = false;
        var oid = $scope.template.Oid;
        var oidLoc = oid.substring(oid.indexOf(":", oid.indexOf(":") + 1) + 1);
        oidLoc = oidLoc.indexOf(":") != -1 ? oidLoc.replace(":", "/") : oidLoc;
        if (oid.match(/^urn:oid:[0-2](\.(0|[1-9][0-9]*))*$/g)) {
            foundMatch = true;
        }

        if (oid.match(/^urn:hl7ii:[0-2](\.(0|[1-9][0-9]*))*\:(.+)$/g)) {
            foundMatch = true;
        }

        if (oid.match(/^http[s]?:\/\/(.+)/g)) {
            foundMatch = true;
        }

        return foundMatch;
    }

    function isIdentifierRightSize() {
        return $scope.template.Oid.length <= 255;
    }

    function initializeTemplateTypes(implementationGuideId) {
        //If this runs while the Id is empty, do nothing so we don't generate errors
        if (!implementationGuideId) return;

        ImplementationGuideService.getTemplateTypes(implementationGuideId)
            .then(function (results) {
                $scope.templateTypes = results;

                if ($scope.template.TemplateTypeId) {
                    var foundTemplateType = _.find($scope.templateTypes, function (templateType) {
                        return templateType.Id == $scope.template.TemplateTypeId;
                    });

                    if (!foundTemplateType) {
                        $scope.template.TemplateTypeId = null;
                    }
                }
            })
            .catch(function (err) {
                console.log(err);
            });
    };

    function setPrimaryContext() {
        for (index in $scope.templateTypes) {
            if ($scope.templateTypes[index].Id === $scope.template.TemplateTypeId) {
                $scope.template.PrimaryContext = $scope.templateTypes[index].RootContext;
                $scope.template.PrimaryContextType = $scope.templateTypes[index].RootContextType;
                break;
            }
        }
    };

    function selectContext() {
        var templateType = _.find($scope.templateTypes, function (templateType) {
            return templateType.Id == $scope.template.TemplateTypeId;
        });

        if (!templateType) {
            return;
        }

        var modalInstance = $uibModal.open({
            templateUrl: '/Scripts/angular/templates/appliesToModal.html',
            controller: 'AppliesToModalCtrl',
            size: 'lg',
            resolve: {
                implementationGuideId: function () { return $scope.template.OwningImplementationGuideId; },
                baseType: function () { return templateType.RootContextType; }
            }
        });

        modalInstance.result.then(function (contextInfo) {
            $scope.template.PrimaryContext = contextInfo.primaryContext;
            $scope.template.PrimaryContextType = contextInfo.primaryContextType;
        });
    };

    function save() {

        var data = {
            Template: angular.copy($scope.template),
            RemovedConstraints: {},
            Constraints: {}
        };

        data.Template.Extensions = {};

        //This specifies which portion of the UI is to be blocked based on the template view
        var templateBlockUI = blockUI.instances.get('templateBlock');
        templateBlockUI.start("Saving...");

        // Recursively updates the constraints in currentList (and its children)
        // based on the newList. newList is plain JS objects, not KO'd objects.
        var updateConstraints = function (currentList, newList) {

            for (var i in currentList) {
                var currentConstraint = currentList[i];
                var newConstraint = newList[i];

                currentConstraint.Id = newConstraint.Id;
                currentConstraint.Number = newConstraint.Number;
                currentConstraint.IsNew = false;

                updateConstraints(currentConstraint.Children, newConstraint.Children);
            }
        };

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

                templateBlockUI.stop();

                location.href = "/TemplateManagement/Edit/Id/" + response.data.TemplateId;
            });

    }

    function cancel() {
        initVars();
        //Potentially redirect to listing after cancel
        //location.href = "/TemplateManagement/List/";
    }

    $scope.init = init;
    $scope.createBookmark = createBookmark;
    $scope.updateIdentifier = updateIdentifier;
    $scope.initializeTemplateTypes = initializeTemplateTypes;
    $scope.setPrimaryContext = setPrimaryContext;
    $scope.save = save;
    $scope.cancel = cancel;
    $scope.selectContext = selectContext;
    
}]);