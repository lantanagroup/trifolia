angular.module('Trifolia')
    .factory('EditorService', function ($http, $q) {
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
                    alert('Error getting constraints');
                    console.log(err);
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
                    alert('Error getting template');
                    console.log(err);
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
                    alert('Error getting nodes');
                    console.log(err);
                });

            return deferred.promise;
        };

        return service;
    })
    .factory('TemplateService', function ($http, $q) {
        var service = {};

        service.getTemplates = function (count, page, sortProperty, sortDescending, queryText, filterName, filterOid, filterImplementationGuideId, filterTemplateTypeId, filterOrganizationId, filterContextType) {
            var params = {};

            params['count'] = count ? count : 50;
            params['page'] = page ? page : 1;
            params['sortProperty'] = sortProperty ? sortProperty : 'Name';
            params['sortDescending'] = sortDescending != undefined && sortDescending != null ? sortDescending : false;

            if (queryText) {
                params['queryText'] = queryText;
            }
            
            if (filterName) {
                params['filterName'] = filterName;
            }

            if (filterOid) {
                params['filterOid'] = filterOid;
            }

            if (filterImplementationGuideId) {
                params['filterImplementationGuideId'] = filterImplementationGuideId;
            }

            if (filterTemplateTypeId) {
                params['filterTemplateTypeId'] = filterTemplateTypeId;
            }

            if (filterOrganizationId) {
                params['filterOrganizationId'] = filterOrganizationId;
            }

            if (filterContextType) {
                params['filterContextType'] = filterContextType;
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
                    alert(err);
                });

            return deferred.promise;
        };

        return service;
    })
    .factory('ImplementationGuideService', function ($http, $q) {
        var service = {};

        service.getAll = function () {
            var deferred = $q.defer();

            $http.get('/api/ImplementationGuide')
                .then(function (results) {
                    deferred.resolve(results.data);
                }, function (err) {
                    alert('Errror retrieving list of implementation guides');
                    console.log(err);
                });

            return deferred.promise;
        };

        service.getEditable = function () {
            var deferred = $q.defer();

            $http.get('/api/ImplementationGuide/Editable')
                .then(function (results) {
                    // TODO: Filter out non-published IGs, unless they are from the same implementation guide as the IG
                    var filtered = _.filter(results.data, function (implementationGuide) {
                        return !implementationGuide.IsPublished;
                    });

                    deferred.resolve(filtered);
                }, function (err) {
                    alert('Errror retrieving list of editable implementation guides');
                    console.log(err);
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