angular.module('Trifolia').service('TerminologyService', function ($q, $http, HelperService) {
    var service = {};

    service.searchValueSets = function (query, sort, page, rows, order) {
        var url = HelperService.buildUrl('/api/Terminology/ValueSets/SortedAndPaged', {
            search: query,
            sort: sort,
            page: page,
            rows: rows,
            order: order
        });
        var deferred = $q.defer();

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error searching for value sets');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getValueSet = function (valueSetId) {
        var url = '/api/Terminology/ValueSet/' + valueSetId;
        var deferred = $q.defer();

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error getting value set');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getValueSetRelationships = function (valueSetId) {
        var url = '/api/Terminology/ValueSet/' + valueSetId + '/Relationships';
        var deferred = $q.defer();

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error getting value set relationships');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.validateValueSetIdentifier = function (valueSetIdentifier, identifierId) {
        var url = HelperService.buildUrl('/api/Terminology/ValueSet/$validateIdentifier', {
            identifier: valueSetIdentifier,
            identifierId: identifierId
        });
        var deferred = $q.defer();

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error validating value set identifier');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.saveValueSet = function (valueSet) {
        var url = '/api/Terminology/ValueSet';
        var deferred = $q.defer();

        $http.post(url, valueSet)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error saving value set');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.removeValueSet = function (valueSetId, replaceValueSetId) {
        var url = '/api/Terminology/ValueSet/' + valueSetId;
        url = HelperService.buildUrl(url, {
            replaceValueSetId: replaceValueSetId
        });
        var deferred = $q.defer();

        $http.delete(url)
            .then(function () {
                deferred.resolve();
            })
            .catch(function (err) {
                console.log('Error removing value set');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.searchCodeSystems = function (query, sort, page, rows, order) {
        var url = HelperService.buildUrl('/api/Terminology/CodeSystem', {
            search: query,
            sort: sort,
            page: page,
            rows: rows,
            order: order
        });
        var deferred = $q.defer();

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error searching for code systems');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.saveCodeSystem = function (codeSystem) {
        var url = '/api/Terminology/CodeSystem';
        var deferred = $q.defer();

        $http.post(url, codeSystem)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error saving code system');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.deleteCodeSystem = function (codeSystemId) {
        var url = '/api/Terminology/CodeSystem/' + codeSystemId;
        return $http.delete(url);
    };

    return service;
});

angular.module('Trifolia').service('HelperService', function ($httpParamSerializer, $cookies) {
    return {
        getQueryParams: function() {
            if (!location.search) {
                return {};
            }

            var search = location.search.substring(1);
            var paramSplit = search.split('&');
            var params = {};

            _.each(paramSplit, function (param) {
                var valueSplit = param.split('=');

                if (valueSplit.length == 1) {
                    params[valueSplit[0]] = true;
                } else if (valueSplit.length == 2) {
                    params[valueSplit[0]] = valueSplit[1];
                }
            });

            return params;
        },
        buildUrl: function (url, params) {
            var serializedParams = $httpParamSerializer(params);

            if (serializedParams.length > 0) {
                url += ((url.indexOf('?') === -1) ? '?' : '&') + serializedParams;
            }

            return url;
        },
        getCookieInteger: function (key, defaultValue) {
            if ($cookies.get(key)) {
                return parseInt($cookies.get(key));
            }

            if (defaultValue !== undefined) {
                return defaultValue;
            }
        },
        identifierRegex: '(^http[s]?\:\/\/(.+?)\.(.+)\/?(.*)$)|(^urn:oid:([0-9]+[\\.]?)+$)|(^urn:hl7ii:([0-9]+[\\.]?)+:(.+?)$)',
        oidRegex: '^urn:oid:([0-9]+[\\.]?)+$',
        hl7iiRegex: '^urn:hl7ii:([0-9]+[\\.]?)+:(.+?)$',
        urlRegex: '^http[s]?\:\/\/(.+?)\.(.+)\/?(.*)$',
        emailRegex: '^([a-z0-9_\.-]+)@([\da-z\.-]+)\.([a-z\.]{2,6})$',
        getErrorMessage: function (err) {
            if (err.data && err.data.Message) {
                return err.data.Message;
            } else if (typeof err.data === 'string' && err.data) {
                return err.data;
            } else if (err.message) {
                return err.message;
            } else if (err.statusText) {
                return err.statusText;
            }

            return err;
        }
    };
});

angular.module('Trifolia').service('ImportService', function ($http, $q, HelperService) {
    return {
        importValueSet: function (source, id) {
            var url = '/api/Import/ValueSet';
            var body = {
                Source: source,
                Id: id
            };

            var deferred = $q.defer();

            $http.post(url, body)
                .then(function (results) {
                    deferred.resolve(results.data);
                })
                .catch(function (err) {
                    deferred.reject(HelperService.getErrorMessage(err));
                });

            return deferred.promise;
        }
    };
});

angular.module('Trifolia').service('ExportService', function ($http, $q, HelperService) {
    var service = {};

    service.getExportSettings = function (implementationGuideId, format) {
        var deferred = $q.defer();
        var url = '/api/Export/Settings?implementationGuideId=' + encodeURIComponent(implementationGuideId) + '&format=' + encodeURIComponent(format);

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.saveExportSettings = function (settings) {
        var deferred = $q.defer();
        var url = '/api/Export/Settings?implementationGuideId=' + encodeURIComponent(settings.ImplementationGuideId) + '&format=' + encodeURIComponent(settings.ExportFormat);
        $http.post(url, settings)
            .then(function () {
                deferred.resolve();
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    return service;
});

angular.module('Trifolia').service('UserService', function ($http, $q, HelperService) {
    var service = {};

    service.getMyUser = function () {
        var deferred = $q.defer();

        $http.get('/api/User/Me')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.saveMyUser = function (model) {
        var deferred = $q.defer();

        $http.post('/api/User/Me', model)
            .then(deferred.resolve)
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getReleaseAnnouncementSubscription = function () {
        var deferred = $q.defer();

        $http.get('/api/User/Me/ReleaseAnnouncement')
            .then(function (results) {
                deferred.resolve(results);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.subscribeToReleaseAnnouncements = function () {
        return $http.post('/api/User/Me/ReleaseAnnouncement');
    };

    service.unsubscribeFromReleaseAnnouncements = function () {
        return $http.delete('/api/User/Me/ReleaseAnnouncement');
    };

    service.validateUmlsCredentials = function (username, password) {
        var deferred = $q.defer();
        var data = {
            Username: username,
            Password: password
        };

        $http.post('/api/User/ValidateUmlsCredentials', data)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.hasSecurable = function (securableNames) {
        if (!containerViewModel) {
            console.log('containerViewModel does not exist');
            return false;
        }

        if (typeof securableNames === 'string') {
            securableNames = [securableNames];
        }

        if (!(securableNames instanceof Array)) {
            console.log('UserService.hasSecurable() securableNames is not an array');
            return false;
        }

        return containerViewModel.HasSecurable(securableNames);
    };

    return service;
});

angular.module('Trifolia').service('ConfigService', function ($http, $q, HelperService) {
    var service = {};

    service.getEnableReleaseAnnouncements = function () {
        var deferred = $q.defer();

        $http.get('/api/Config/ReleaseAnnouncement')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    return service;
});

angular.module('Trifolia').service('ImplementationGuideService', function ($http, $q, HelperService) {
    var service = {};

    service.validate = function (implementationGuideId) {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide/' + encodeURIComponent(implementationGuideId) + '/Validate')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getImplementationGuides = function () {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getImplementationGuideCategories = function (implementationGuideId) {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide/' + encodeURIComponent(implementationGuideId) + '/Category')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getImplementationGuideValueSets = function (implementationGuideId, onlyStatic) {
        if (onlyStatic === 'undefined') {
            onlyStatic = false;
        }

        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide/' + encodeURIComponent(implementationGuideId) + '/ValueSet?onlyStatic=' + encodeURIComponent(onlyStatic))
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getImplementationGuideTemplates = function (implementationGuideId, parentTemplateIds, inferred, categories) {
        var deferred = $q.defer();
        var url = '/api/ImplementationGuide/' + encodeURIComponent(implementationGuideId) + '/Template?';

        if (parentTemplateIds) {
            url += 'parentTemplateIds=' + encodeURIComponent(parentTemplateIds.join(',')) + '&';
        }

        if (typeof(inferred) !== 'undefined') {
            url += 'inferred=' + encodeURIComponent(inferred) + '&';
        }

        if (categories) {
            url += 'categories=' + encodeURIComponent(categories.join(',')) + '&';
        }

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getTemplateTypes = function (implementationGuideId) {
        var deferred = $q.defer();
        $http.get('/api/ImplementationGuide/' + implementationGuideId + '/TemplateType')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    service.getEditable = function (includeImplementationGuideId) {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide/Editable')
            .then(function (results) {
                // Filter out non-published IGs, unless they are from the same implementation guide as the IG
                var filtered = _.filter(results.data, function (implementationGuide) {
                    return !implementationGuide.IsPublished || implementationGuide.Id === includeImplementationGuideId;
                });

                deferred.resolve(filtered);
            })
            .catch(function (err) {
                deferred.reject(HelperService.getErrorMessage(err));
            });

        return deferred.promise;
    };

    return service;
});

angular.module('Trifolia').factory('TemplateService', function ($http, $q) {
    var service = {};

    service.generateBookmark = function (templateName, isFhir) {
        if (!templateName) {
            return '';
        }

        var replacement = isFhir ? '-' : '_';
        var newBookmark = templateName.replace(/ /gi, replacement);
        var cleanupRegexExp = isFhir ? '[^\w\s]' : '[^\w\s-]';

        if (!isFhir) {
            newBookmark = newBookmark.replace(/[^\w\s]/gi, '');
        }

        return newBookmark.substring(0, 39);
    };

    service.getTemplatePermissions = function (templateId) {
        var deferred = $q.defer();
        var url = '/api/Template/' + templateId + '/Permissions';

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(deferred.reject);

        return deferred.promise;
    };

    service.getTemplate = function (templateId) {
        var deferred = $q.defer();
        var url = '/api/Template/' + templateId;

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(deferred.reject);

        return deferred.promise;
    };

    service.getTemplates = function (options) {
        var paramOptions = {
            count: 50,
            page: 1,
            sortProperty: 'Name',
            sortDescending: false,
            queryText: '',
            filterName: null,
            filterOid: null,
            filterImplementationGuideId: null,
            filterTemplateTypeId: null,
            filterOrganizationId: null,
            filterContextType: null,
            inferred: true
        };
        angular.extend(paramOptions, options);

        var params = {};

        params['count'] = paramOptions.count;
        params['page'] = paramOptions.page;
        params['sortProperty'] = paramOptions.sortProperty;
        params['sortDescending'] = paramOptions.sortDescending;
        params['inferred'] = paramOptions.inferred;

        if (paramOptions.queryText) {
            params['queryText'] = paramOptions.queryText;
        }

        if (paramOptions.filterName) {
            params['filterName'] = paramOptions.filterName;
        }

        if (paramOptions.filterOid) {
            params['filterOid'] = paramOptions.filterOid;
        }

        if (paramOptions.filterImplementationGuideId) {
            params['filterImplementationGuideId'] = paramOptions.filterImplementationGuideId;
        }

        if (paramOptions.filterTemplateTypeId) {
            params['filterTemplateTypeId'] = paramOptions.filterTemplateTypeId;
        }

        if (paramOptions.filterOrganizationId) {
            params['filterOrganizationId'] = paramOptions.filterOrganizationId;
        }

        if (paramOptions.filterContextType) {
            params['filterContextType'] = paramOptions.filterContextType;
        }

        var url = '/api/Template?';
        var queryArray = _.map(Object.keys(params), function (paramKey) {
            return paramKey + '=' + encodeURIComponent(params[paramKey]);
        });
        var queryString = queryArray.join('&');

        var deferred = $q.defer();

        $http.get(url + queryString)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log('Error searching templates');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.validateIdentifier = function (identifier, ignoreTemplateId) {
        var deferred = $q.defer();
        var url = '/api/Template/Validate/Oid?identifier=' + encodeURIComponent(identifier);

        if (ignoreTemplateId) {
            url += '&ignoreTemplateId=' + encodeURIComponent(ignoreTemplateId);
        }

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(function (err) {
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    return service;
});

angular.module('Trifolia').filter('contains', function () {
    return function (array, needle) {
        return array.indexOf(needle) >= 0;
    };
});

angular.module('Trifolia').factory('EditorService', function ($http, $q) {
    var service = {};

    service.getNarrative = function (constraint) {
        var deferred = $q.defer();

        var copy = angular.copy(constraint);
        delete copy.$$bindingType;
        delete copy.$$hashKey;
        delete copy.NarrativeProseHtml;

        $http.post('/api/Template/Edit/Prose', copy)
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(deferred.reject);

        return deferred.promise;
    };

    service.getConstraints = function (templateId) {
        if (!templateId) {
            return $q.resolve([]);
        }

        var deferred = $q.defer();
        var url = '/api/Template/Edit/' + templateId + '/Constraint';

        $http.get(url)
            .then(function (results) {
                deferred.resolve(results.data);
            }, function (err) {
                console.log('Error getting constraints');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getTemplate = function (templateId) {
        if (!templateId) {
            return $q.resolve(new EditorTemplateModel());
        }

        var deferred = $q.defer();
        var url = '/api/Template/Edit/' + templateId + '/MetaData';

        $http.get(url)
            .then(function (results) {
                deferred.resolve(new EditorTemplateModel(results.data));
            }, function (err) {
                console.log('Error getting template');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getNodes = function (implementationGuideId, parentType) {
        var deferred = $q.defer();
        var url = '/api/Template/Edit/Schema/' + implementationGuideId + '?';

        if (parentType) {
            url += 'parentType=' + encodeURIComponent(parentType);
        }

        $http.get(url)
            .then(function (results) {
                var nodes = [];

                for (var i in results.data) {
                    nodes.push(new EditorNodeModel(results.data[i]));
                }

                deferred.resolve(nodes);
            }, function (err) {
                console.log('Error getting nodes');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.save = function (data) {
        var deferred = $q.defer();
        $http.post('/api/Template/Edit/Save', data)
            .then(function (response) {
                deferred.resolve(response);
            }, function (error) {
                console.log("Error occurred during save: ");
                console.log(error);
                deferred.reject(error);
            });
        return deferred.promise;
    };

    return service;
});

var EditorNodeModel = function (data) {
    var self = this;

    self.Cardinality = '';
    self.Conformance = '';
    self.Context = '';
    self.DataType = '';
    self.HasChildren = false;
    self.IsChoice = false;
    self.Children = [];

    // Constraint Properties
    self.Number = '';
    self.Value = '';

    angular.extend(self, data);
};

var EditorTemplateModel = function (data) {
    var self = this;

    self.Id = null;
    self.Name = null;

    angular.extend(self, data);
};