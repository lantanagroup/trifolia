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

            if (defaultValue != undefined) {
                return defaultValue;
            }
        },
        identifierRegex: /^(http\:\/\/(.+?)\.(.+))|(https:\/\/(.+?)\.(.+))|(urn:oid:([\d+][\.]?)+)|(urn:hl7ii:([\d+][\.]?)+:(.+?))$/g,
        oidRegex: /^urn:oid:([\d+][\.]?)+$/g,
        hl7iiRegex: /^urn:hl7ii:([\d+][\.]?)+:(.+?)$/g,
        urlRegex: /(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9]\.[^\s]{2,})/,
        emailRegex: /^([a-z0-9_\.-]+)@([\da-z\.-]+)\.([a-z\.]{2,6})$/
    };
});

angular.module('Trifolia').factory('EditorService', function ($http, $q) {
    var service = {};

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

    return service;
});

angular.module('Trifolia').factory('TemplateService', function ($http, $q) {
    var service = {};

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

    return service;
});

angular.module('Trifolia').factory('ImplementationGuideService', function ($http, $q) {
    var service = {};

    service.getAll = function () {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide')
            .then(function (results) {
                deferred.resolve(results.data);
            }, function (err) {
                console.log('Errror retrieving list of implementation guides');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getEditable = function (includeImplementationGuideId) {
        var deferred = $q.defer();

        $http.get('/api/ImplementationGuide/Editable')
            .then(function (results) {
                // Filter out non-published IGs, unless they are from the same implementation guide as the IG
                var filtered = _.filter(results.data, function (implementationGuide) {
                    return !implementationGuide.IsPublished || implementationGuide.Id == includeImplementationGuideId;
                });

                deferred.resolve(filtered);
            }, function (err) {
                console.log('Errror retrieving list of editable implementation guides');
                console.log(err);
                deferred.reject(err);
            });

        return deferred.promise;
    };

    service.getTemplateTypes = function (implementationGuideId) {
        var deferred = $q.defer();
        $http.get('/api/ImplementationGuide/' + implementationGuideId + '/TemplateType')
            .then(function (results) {
                deferred.resolve(results.data);
            }, function (err) {
                console.log('Error retriving template types for implementation guide');
                console.log(err);
                deferred.reject(err);
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