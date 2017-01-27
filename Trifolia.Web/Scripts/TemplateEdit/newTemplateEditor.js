angular.module('NewEditor', ['ui.bootstrap'])
    .controller('EditorController', function ($scope, EditorService, ImplementationGuideService) {
        $scope.implementationGuides = [];
        $scope.template = null;

        $scope.init = function (templateId, defaultImplementationGuideId) {
            ImplementationGuideService.getEditable()
                .then(function (implementationGuides) {
                    $scope.implementationGuides = implementationGuides;
                });

            EditorService.getTemplate(templateId)
                .then(function (template) {
                    $scope.template = template;
                });
        };
    })
    .factory('EditorService', function ($http, $q) {
        var service = {};

        service.getTemplate = function (templateId) {
            if (!templateId) {
                return $q.resolve(new TemplateModel());
            }

            var deferred = $q.defer();
            var url = '/api/Template/Edit/' + templateId + '/MetaData';

            $http.get(url)
                .then(function (results) {
                    deferred.resolve(new TemplateModel(results.data));
                }, function (err) {
                    alert('Error getting template');
                    console.log(err);
                });

            return deferred.promise;
        };

        return service;
    })
    .factory('ImplementationGuideService', function ($http, $q) {
        var service = {};

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

var TemplateModel = function (data) {
    var self = this;

    self.Id = null;
    self.Name = null;

    angular.extend(self, data);
};