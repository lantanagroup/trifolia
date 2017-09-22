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

        var importSources = [{
            id: 1,
            display: 'VSAC'
        }, {
            id: 2,
            display: 'PHIN VADS'
        }, {
            id: 3,
            display: 'ROSE TREE'
        }];

        $scope.getImportSourceDisplay = function (id) {
            if (!id) {
                return '';
            }

            return _.find(importSources, function (importSource) {
                return importSource.id == id;
            }).display;
        };

        $scope.openImportValueSet = function (source, id) {
            var modalInstance = $uibModal.open({
                controller: 'ImportValueSetModalController',
                templateUrl: 'importValueSetModal.html',
                size: 'lg',
                resolve: {
                    source: function () { return source; },
                    id: function () { return id; }
                }
            });

            modalInstance.result.then(function () {
                $scope.message = 'Successfully imported value set!';
                $scope.search();
            });
        };

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
        $scope.codeSystem = codeSystem ? codeSystem : {
            Id: null,
            Name: '',
            Oid: '',
            Description: ''
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

        $scope.identifierChanged = function (identifier) {
            $scope.$$typeError = false;
            $scope.$$valueError = false;

            if (identifier.Type === undefined) {
                identifier.$$typeError = true;
                identifier.$$typeErrorMessage = 'You must specify a type for the identifier.';
                return;
            }

            if (!identifier || !identifier.Identifier) {
                identifier.$$valueError = true;
                identifier.$$valueErrorMessage = 'You must specify a value for the identifier.';
                return;
            }

            // Look for the identifier in the current value set first, so we don't have to make an AJAX GET if we don't need to
            var found = _.find($scope.valueSet.Identifiers, function (nextIdentifier) {
                return nextIdentifier != identifier && nextIdentifier.Identifier.toLowerCase().trim() == identifier.Identifier.toLowerCase().trim();
            });

            if (found) {
                identifier.$$valueError = true;
                identifier.$$valueErrorMessage = 'This identifier is a duplicate.';
                return;
            }

            var oidRegex = new RegExp(HelperService.oidRegex, 'gi');
            var hl7iiRegex = new RegExp(HelperService.hl7iiRegex, 'gi');
            var urlRegex = new RegExp(HelperService.urlRegex, 'gi');

            if (identifier.Type == 0 && !oidRegex.test(identifier.Identifier)) {
                identifier.$$valueError = true;
                identifier.$$valueErrorMessage = 'The format of the identifier is incorrect. Please specify the identifier in the format urn:oid:XX.YY.ZZ';
                return;
            } else if (identifier.Type == 1 && !hl7iiRegex.test(identifier.Identifier)) {
                identifier.$$valueError = true;
                identifier.$$valueErrorMessage = 'The format of the identifier is incorrect. Please specify the identifier in the format urn:hl7ii:XX.YY.ZZ:aa';
                return;
            } else if (identifier.Type == 2 && !urlRegex.test(identifier.Identifier)) {
                identifier.$$valueError = true;
                identifier.$$valueErrorMessage = 'The format of the identifier is incorrect. Please specify the identifier in the format http(s)://xxx.yyy/zzz';
                return;
            }

            // Ask the server to validate the identifier, to see if other value sets use the same identifier
            TerminologyService.validateValueSetIdentifier(identifier.Identifier, identifier.Id)
                .then(function (isValid) {
                    identifier.$$valueError = !isValid;

                    if (!isValid) {
                        identifier.$$valueErrorMessage = 'This identifier is in use by another value set.';
                    }
                })
                .catch(function (err) {
                    alert('Error while validating the identifier: ' + err);
                });
        };

        $scope.hasActiveIdentifier = function () {
            var activeIdentifiers = _.filter($scope.valueSet.Identifiers, function (identifier) {
                return !identifier.ShouldRemove && !identifier.$$typeError && !identifier.$$valueError;
            });

            return activeIdentifiers.length > 0;
        };

        $scope.getAvailableIdentifierTypes = function (identifier) {
            if (identifier && identifier.ShouldRemove) {
                return $scope.identifierOptions;
            }

            return _.filter($scope.identifierOptions, function (identifierType) {
                var foundIdentifier = _.filter($scope.valueSet.Identifiers, function (next) {
                    return next != identifier && next.Type == identifierType.value && !next.ShouldRemove;
                });

                if (foundIdentifier.length > 0) {
                    return false;
                }

                return true;
            });
        };

        $scope.isValid = function () {
            if (!$scope.valueSet.Name) {
                return false;
            }

            var activeIdentifiers = _.filter($scope.valueSet.Identifiers, function (identifier) {
                return !identifier.ShouldRemove;
            });

            if (activeIdentifiers.length == 0) {
                return false;
            }

            var invalidIdentifiers = _.filter($scope.valueSet.Identifiers, function (identifier) {
                return identifier.$$valueError || identifier.$$typeError;
            });

            if (invalidIdentifiers.length > 0) {
                return false;
            }

            if (!$scope.valueSet.IsComplete && !$scope.valueSet.SourceUrl) {
                return false;
            }

            return true;
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
            var hasDefault = _.filter($scope.valueSet.Identifiers, function (identifier) {
                return identifier.IsDefault;
            }).length > 0;
            var availableIdentifierTypes = $scope.getAvailableIdentifierTypes();

            if (availableIdentifierTypes.length == 0) {
                alert('You cannot add a new identifier because there is already one identifier for each type allowed.');
                return;
            }

            var newIdentifier = {
                Id: null,
                Type: availableIdentifierTypes[0].value,
                Identifier: '',
                IsDefault: !hasDefault
            };

            $scope.identifierChanged(newIdentifier);
            $scope.valueSet.Identifiers.push(newIdentifier);

            if (!hasDefault) {
                $scope.defaultIdentifierChanged(newIdentifier);
            }
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
    })
    .controller('ImportValueSetModalController', function ($scope, $uibModalInstance, $sce, ImportService, UserService, source, id) {
        $scope.source = source;
        $scope.id = id;
        $scope.disableSource = source ? true : false;
        $scope.disableId = id ? true : false;
        $scope.message = '';
        $scope.canImportVSAC = UserService.hasSecurable(['ImportVSAC']);
        $scope.canImportPHINVADS = UserService.hasSecurable(['ImportPHINVADS']);

        $scope.isValid = function () {
            return $scope.source && $scope.id;
        };

        $scope.ok = function () {
            ImportService.importValueSet($scope.source, $scope.id)
                .then(function (results) {
                    if (results.Success) {
                        $uibModalInstance.close();
                    } else {
                        $scope.message = $sce.trustAsHtml(results.Message);
                    }
                })
                .catch(function (err) {
                    if (typeof err === 'string') {
                        $scope.message = $sce.trustAsHtml(err);
                    } else if (typeof err.data === 'string') {
                        $scope.message = $sce.trustAsHtml(err.data);
                    } else if (err.message) {
                        $scope.message = $sce.trustAsHtml(err.message);
                    }
                });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('close');
        };
    });