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

angular.module('Trifolia').service('ImportService', function ($http, $q) {
    return {
        importValueSet: function (source, id, username, password) {
            var url = '/api/Import/ValueSet';
            var body = {
                Source: source,
                Id: id,
                Username: username,
                Password: password
            };

            var deferred = $q.defer();

            $http.post(url, body)
                .then(function (results) {
                    deferred.resolve(results.data);
                })
                .catch(deferred.reject);

            return deferred.promise;
        }
    };
});

angular.module('Trifolia').service('UserService', function ($http, $q) {
    var service = {};

    service.getMyUser = function () {
        var deferred = $q.defer();

        $http.get('/api/User/Me')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(deferred.reject);

        return deferred.promise;
    };

    service.saveMyUser = function (model) {
        return $http.post('/api/User/Me', model);
    };

    service.getReleaseAnnouncementSubscription = function () {
        var deferred = $q.defer();

        $http.get('/api/User/Me/ReleaseAnnouncement')
            .then(function (results) {
                deferred.resolve(results);
            })
            .catch(deferred.reject);

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
            .catch(deferred.reject);

        return deferred.promise;
    };

    return service;
});

angular.module('Trifolia').service('ConfigService', function ($http, $q) {
    var service = {};

    service.getEnableReleaseAnnouncements = function () {
        var deferred = $q.defer();

        $http.get('/api/Config/ReleaseAnnouncement')
            .then(function (results) {
                deferred.resolve(results.data);
            })
            .catch(deferred.reject);

        return deferred.promise;
    };

    return service;
});