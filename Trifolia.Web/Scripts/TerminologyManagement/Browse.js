angular.module('Trifolia')
    .controller('BrowseTerminologyController', function ($scope, $cookies, HelperService) {
        $scope.showValueSets = containerViewModel.HasSecurable(['ValueSetList']);
        $scope.showCodeSystems = containerViewModel.HasSecurable(['CodeSystemList']);
        $scope.currentTab = HelperService.getCookieInteger('BrowseTerminology_CurrentTab', 0);

        $scope.tabChanged = function (tabIndex) {
            $scope.currentTab = tabIndex;
            $cookies.put('BrowseTerminology_CurrentTab', tabIndex);
            $scope.$broadcast('CurrentTabChanged');
        };
    })
    .controller('BrowseValueSetsController', function ($uibModal, $scope, $sce, $cookies, TerminologyService, HelperService) {
        $scope.canEdit = containerViewModel.HasSecurable(['ValueSetEdit']);
        $scope.criteria = {
            query: $cookies.get('BrowseTerminology_ValueSetQuery') || '',
            sort: $cookies.get('BrowseTerminology_ValueSetSort') || 'Name',
            order: $cookies.get('BrowseTerminology_ValueSetOrder') || 'asc',
            page: HelperService.getCookieInteger('BrowseTerminology_ValueSetPage', 1),
            rows: HelperService.getCookieInteger('BrowseTerminology_ValueSetRows', 20)
        };
        $scope.searchResults = [];
        $scope.isSearching = false;
        $scope.totalPages = 0;

        $scope.search = function () {
            $scope.searchResults = [];
            $scope.totalPages = 0;
            $scope.isSearching = true;

            $cookies.put('BrowseTerminology_ValueSetQuery', $scope.criteria.query);
            $cookies.put('BrowseTerminology_ValueSetSort', $scope.criteria.sort);
            $cookies.put('BrowseTerminology_ValueSetOrder', $scope.criteria.order);
            $cookies.put('BrowseTerminology_ValueSetPage', $scope.criteria.page);
            $cookies.put('BrowseTerminology_ValueSetRows', $scope.criteria.rows);

            TerminologyService.searchValueSets($scope.criteria.query, $scope.criteria.sort, $scope.criteria.page, $scope.criteria.rows, $scope.criteria.order)
                .then(function (results) {
                    $scope.searchResults = results;
                    _.each($scope.searchResults.Items, function (searchResult) {
                        var identifiers = searchResult.Identifiers.split(',');
                        searchResult.IdentifiersDisplay = $sce.trustAsHtml(identifiers.join('<br/>'));
                        searchResult.disableModify = !searchResult.PermitModify || !(searchResult.CanModify || searchResult.CanOverride);
                    });
                    $scope.totalPages = Math.ceil($scope.searchResults.TotalItems / $scope.criteria.rows);
                })
                .catch(function (err) {
                    alert('An error occurred while searching for value sets: ' + err);
                })
                .finally(function () {
                    $scope.isSearching = false;
                });
        };

        $scope.changePage = function (page) {
            switch (page) {
                case 'next':
                    $scope.criteria.page++;
                    break;
                case 'previous':
                    $scope.criteria.page--;
                    break;
                case 'last':
                    $scope.criteria.page = $scope.totalPages;
                    break;
                case 'first':
                    $scope.criteria.page = 1;
                    break;
            }

            $scope.search();
        };

        $scope.toggleSort = function (propertyName) {
            if ($scope.criteria.sort == propertyName) {
                $scope.criteria.order = ($scope.criteria.order == 'desc' ? 'asc' : 'desc');
            } else {
                $scope.criteria.sort = propertyName;
                $scope.criteria.order = 'asc';
            }

            $scope.search();
        };

        $scope.editValueSet = function (valueSet) {
            var modalInstance = $uibModal.open({
                controller: 'EditValueSetModalController',
                templateUrl: 'editValueSetModal.html',
                size: 'lg',
                resolve: {
                    valueSetId: function () { return valueSet ? valueSet.Id : null; }
                }
            });

            modalInstance.result.then(function () {
                $scope.search();
            });
        };

        $scope.removeValueSet = function (valueSet) {
            var modalInstance = $uibModal.open({
                controller: 'RemoveValueSetModalController',
                templateUrl: 'removeValueSetModal.html',
                size: 'lg',
                resolve: {
                    valueSetId: function () { return valueSet.Id; }
                }
            });

            modalInstance.result.then($scope.search);
        };

        $scope.contextTabChanged = function () {
            if ($scope.currentTab == 0) {
                $scope.search();
            }
        };

        $scope.$on('CurrentTabChanged', $scope.contextTabChanged);
    })
    .controller('BrowseCodeSystemsController', function ($uibModal, $scope, $cookies, HelperService, TerminologyService) {
        $scope.canEdit = containerViewModel.HasSecurable(['CodeSystemEdit']);
        $scope.criteria = {
            query: $cookies.get('BrowseTerminology_CodeSystemQuery') || '',
            page: HelperService.getCookieInteger('BrowseTerminology_CodeSystemPage', 1),
            rows: HelperService.getCookieInteger('BrowseTerminology_CodeSystemRows', 20),
            sort: $cookies.get('BrowseTerminology_CodeSystemSort') || 'Name',
            order: $cookies.get('BrowseTerminology_CodeSystemOrder') || 'asc',
        };
        $scope.searchResults = [];
        $scope.isSearching = false;
        $scope.totalPages = 0;

        $scope.search = function () {
            $scope.searchResults = [];
            $scope.totalPages = 0;
            $scope.isSearching = true;

            $cookies.put('BrowseTerminology_CodeSystemQuery', $scope.criteria.query);
            $cookies.put('BrowseTerminology_CodeSystemSort', $scope.criteria.sort);
            $cookies.put('BrowseTerminology_CodeSystemOrder', $scope.criteria.order);
            $cookies.put('BrowseTerminology_CodeSystemPage', $scope.criteria.page);
            $cookies.put('BrowseTerminology_CodeSystemRows', $scope.criteria.rows);

            TerminologyService.searchCodeSystems($scope.criteria.query, $scope.criteria.sort, $scope.criteria.page, $scope.criteria.rows, $scope.criteria.order)
                .then(function (results) {
                    $scope.searchResults = results;
                    _.each($scope.searchResults.rows, function (searchResult) {
                        searchResult.disableModify = !searchResult.PermitModify || !(searchResult.CanModify || searchResult.CanOverride);
                    });
                    $scope.totalPages = Math.ceil($scope.searchResults.total / $scope.criteria.rows);
                })
                .catch(function (err) {
                    alert('An error occurred while searching for value sets: ' + err);
                })
                .finally(function () {
                    $scope.isSearching = false;
                });
        };

        $scope.changePage = function (page) {
            switch (page) {
                case 'next':
                    $scope.criteria.page++;
                    break;
                case 'previous':
                    $scope.criteria.page--;
                    break;
                case 'last':
                    $scope.criteria.page = $scope.totalPages;
                    break;
                case 'first':
                    $scope.criteria.page = 1;
                    break;
            }

            $scope.search();
        };

        $scope.toggleSort = function (propertyName) {
            if ($scope.criteria.sort == propertyName) {
                $scope.criteria.order = ($scope.criteria.order == 'desc' ? 'asc' : 'desc');
            } else {
                $scope.criteria.sort = propertyName;
                $scope.criteria.order = 'asc';
            }

            $scope.search();
        };

        $scope.editCodeSystem = function (codeSystem) {
            var modalInstance = $uibModal.open({
                controller: 'EditCodeSystemModalController',
                templateUrl: 'editCodeSystemModal.html',
                size: 'lg',
                resolve: {
                    codeSystem: function () { return angular.copy(codeSystem); }
                }
            });

            modalInstance.result.then(function () {
                $scope.search();
            });
        };

        $scope.removeCodeSystem = function (codeSystem) {
            if (!confirm('Are you sure you want to delete this code system and codes related to it?')) {
                return;
            }

            TerminologyService.deleteCodeSystem(codeSystem.Id)
                .then($scope.search)
                .catch(function (err) {
                    alert('An error occurred while deleting the code system: ' + err);
                });
        };

        $scope.contextTabChanged = function () {
            if ($scope.currentTab == 1) {
                $scope.search();
            }
        };

        $scope.$on('CurrentTabChanged', $scope.contextTabChanged);
    })
    .controller('RemoveValueSetModalController', function ($q, $uibModalInstance, $scope, TerminologyService, HelperService, valueSetId) {
        $scope.replaceValueSet = null;
        $scope.relationships = [];
        $scope.bindingStrengths = [{ value: 0, display: 'Static' }, { value: 1, display: 'Dynamic' }];

        $scope.init = function () {
            TerminologyService.getValueSetRelationships(valueSetId)
                .then(function (results) {
                    $scope.relationships = results;
                })
                .catch(function (err) {
                    alert('An error occurred while getting the relationships to the value set: ' + err);
                });
        };

        $scope.searchValueSets = function (query) {
            var deferred = $q.defer();

            TerminologyService.searchValueSets(query, 'Name', 1, 10, 'asc')
                .then(function (results) {
                    var mapped = _.map(results.Items, function (valueSet) {
                        var ret = {
                            id: valueSet.Id,
                            name: valueSet.Name,
                            identifiers: valueSet.Identifiers.split(',')
                        };

                        ret.display = ret.name;

                        if (ret.identifiers.length > 0) {
                            ret.display += ' (' + ret.identifiers[0] + ')';
                        }

                        return ret;
                    });

                    deferred.resolve(mapped);
                })
                .catch(deferred.reject);

            return deferred.promise;
        };

        $scope.ok = function () {
            var replaceValueSetId = $scope.replaceValueSet ? $scope.replaceValueSet.id : null;
            TerminologyService.removeValueSet(valueSetId, replaceValueSetId)
                .then(function () {
                    $uibModalInstance.close();
                })
                .catch(function (err) {
                    alert('An error occurred while removing the value set: ' + err);
                });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    })
    .controller('EditCodeSystemModalController', function ($uibModalInstance, $scope, TerminologyService, HelperService, codeSystem) {
        $scope.identifierRegex = HelperService.identifierRegex;
        $scope.identifierOptions = [{ value: 0, display: 'OID' }, { value: 1, display: 'HL7-II' }, { value: 2, display: 'HTTP' }];
        $scope.codeSystem = codeSystem ? codeSystem : {
            Id: null,
            Name: '',
            Description: '',
            Identifiers: []
        };
        $scope.newIdentifier = {
            Type: 0,
            Identifier: '',
            IsDefault: false,
            IsRemoved: false
        };

        $scope.isNewIdentifierFormatInvalid = function () {
            if (!$scope.newIdentifier.Identifier) {
                return 'Identifier is required.';
            }

            switch ($scope.newIdentifier.Type) {
                case 0:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.oidRegex)) {
                        return 'Identifier must be in the format urn:oid:XXXX';
                    }
                    break;
                case 1:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.hl7iiRegex)) {
                        return 'Identifier must be in the format urn:hl7ii:XXXX:YYYY';
                    }
                    break;
                case 2:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.urlRegex)) {
                        return 'Identifier must be in the format http[s]://XXXX.YYY/';
                    }
                    break;
            }
        };

        $scope.identifierChanged = function (identifier) {
            if (!identifier || !identifier.Identifier) {
                return;
            }

            // Look for the identifier in the current value set first, so we don't have to make an AJAX GET if we don't need to
            var found = _.find($scope.codeSystem.Identifiers, function (nextIdentifier) {
                return nextIdentifier.Identifier.toLowerCase().trim() == identifier.Identifier.toLowerCase().trim();
            });

            if (found) {
                $scope.newIdentifierIsUnique = false;
                return;
            }

            // Ask the server to validate the identifier, to see if other value sets use the same identifier
            TerminologyService.validateCodeSystemIdentifier(identifier.Identifier, identifier.Id)
                .then(function (isValid) {
                    $scope.newIdentifierIsUnique = isValid;
                })
                .catch(function (err) {
                    alert('Error while validating the identifier: ' + err);
                });
        };

        $scope.defaultIdentifierChanged = function (defaultIdentifier) {
            // Remove "default" from other identifiers. Only one can be default at a time.
            if (defaultIdentifier.IsDefault) {
                _.each($scope.codeSystem.Identifiers, function (identifier) {
                    if (identifier != defaultIdentifier) {
                        identifier.IsDefault = false;
                    }
                });
            }
        };

        $scope.removeIdentifier = function (identifier) {
            if (!identifier.Id) {
                var index = $scope.codeSystem.Identifiers.indexOf(identifier);
                $scope.codeSystem.Identifiers.splice(index, 1);
            } else {
                identifier.IsRemoved = true;
                identifier.IsDefault = false;
            }
        };

        $scope.addIdentifier = function () {
            $scope.codeSystem.Identifiers.push($scope.newIdentifier);
            $scope.defaultIdentifierChanged($scope.newIdentifier);

            $scope.newIdentifier = {
                Id: null,
                Type: 0,
                Identifier: '',
                IsDefault: false,
                IsRemoved: false
            };
        };

        $scope.ok = function () {
            TerminologyService.saveCodeSystem($scope.codeSystem)
                .then(function (updatedCodeSystem) {
                    $uibModalInstance.close(updatedCodeSystem);
                })
                .catch(function (err) {
                    alert('An error occurred while saving the code system: ' + err);
                });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    })
    .controller('EditValueSetModalController', function ($uibModalInstance, $scope, TerminologyService, HelperService, valueSetId) {
        $scope.valueSet = {
            Id: null,
            Name: '',
            Code: '',
            Description: '',
            IntentionalDefinition: '',
            IsIntentional: false,
            IsComplete: false,
            SourceUrl: '',
            Identifiers: []
        };
        $scope.newIdentifier = {
            Id: null,
            Type: 0,
            Identifier: '',
            IsDefault: false
        };
        $scope.newIdentifierIsUnique = false;
        $scope.identifierOptions = [{ value: 0, display: 'OID' }, { value: 1, display: 'HL7-II' }, { value: 2, display: 'HTTP' }];
        $scope.urlRegex = HelperService.urlRegex;

        $scope.init = function () {
            if (valueSetId) {
                TerminologyService.getValueSet(valueSetId)
                    .then(function (valueSet) {
                        $scope.valueSet = valueSet;
                    })
                    .catch(function (err) {
                        alert('Error getting value set: ' + err);
                    });
            }
        };

        $scope.isNewIdentifierFormatInvalid = function () {
            if (!$scope.newIdentifier.Identifier) {
                return 'Identifier is required.';
            }

            switch ($scope.newIdentifier.Type) {
                case 0:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.oidRegex)) {
                        return 'Identifier must be in the format urn:oid:XXXX';
                    }
                    break;
                case 1:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.hl7iiRegex)) {
                        return 'Identifier must be in the format urn:hl7ii:XXXX:YYYY';
                    }
                    break;
                case 2:
                    if (!$scope.newIdentifier.Identifier.match(HelperService.urlRegex)) {
                        return 'Identifier must be in the format http[s]://XXXX.YYY/';
                    }
                    break;
            }
        };

        $scope.identifierChanged = function (identifier) {
            if (!identifier || !identifier.Identifier) {
                return;
            }

            // Look for the identifier in the current value set first, so we don't have to make an AJAX GET if we don't need to
            var found = _.find($scope.valueSet.Identifiers, function (nextIdentifier) {
                return nextIdentifier.Identifier.toLowerCase().trim() == identifier.Identifier.toLowerCase().trim();
            });

            if (found) {
                $scope.newIdentifierIsUnique = false;
                return;
            }

            // Ask the server to validate the identifier, to see if other value sets use the same identifier
            TerminologyService.validateValueSetIdentifier(identifier.Identifier, identifier.Id)
                .then(function (isValid) {
                    $scope.newIdentifierIsUnique = isValid;
                })
                .catch(function (err) {
                    alert('Error while validating the identifier: ' + err);
                });
        };

        $scope.defaultIdentifierChanged = function (defaultIdentifier) {
            // Remove "default" from other identifiers. Only one can be default at a time.
            if (defaultIdentifier.IsDefault) {
                _.each($scope.valueSet.Identifiers, function (identifier) {
                    if (identifier != defaultIdentifier) {
                        identifier.IsDefault = false;
                    }
                });
            }
        };

        $scope.removeIdentifier = function (identifier) {
            if (!identifier.Id) {
                var index = $scope.valueSet.Identifiers.indexOf(identifier);
                $scope.valueSet.Identifiers.splice(index, 1);
            } else {
                identifier.ShouldRemove = true;
                identifier.IsDefault = false;
            }
        };

        $scope.addIdentifier = function () {
            $scope.valueSet.Identifiers.push($scope.newIdentifier);
            $scope.defaultIdentifierChanged($scope.newIdentifier);

            $scope.newIdentifier = {
                Id: null,
                Type: 0,
                Identifier: '',
                IsDefault: false
            };
        };

        $scope.ok = function () {
            TerminologyService.saveValueSet($scope.valueSet)
                .then(function (updatedValueSet) {
                    $uibModalInstance.close(updatedValueSet);
                })
                .catch(function (err) {
                    alert('Error saving value set: ' + err);
                });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    });