angular.module('NewEditor', ['ui.bootstrap', 'igniteui-directives'])
    .filter('implementationGuideName', function (ImplementationGuideService) {
        var implementationGuides = null;
        var isLoading = false;

        implementationGuideNameFilter.$stateful = true;

        function implementationGuideNameFilter(input) {
            if (implementationGuides == null) {
                if (!isLoading) {
                    isLoading = true;
                    ImplementationGuideService.getAll()
                        .then(function (results) {
                            implementationGuides = results.Items;
                            isLoading = false;
                        });
                }
                return 'Loading...';
            } else {
                var foundImplementationGuide = _.find(implementationGuides, function (implementationGuide) {
                    return implementationGuide.Id == input;
                });

                if (foundImplementationGuide) {
                    return foundImplementationGuide.Title;
                }
            }

            return 'Not found';
        };

        return implementationGuideNameFilter;
    })
    .controller('EditorController', function ($scope, $interval, EditorService, ImplementationGuideService) {
        $scope.implementationGuides = [];
        $scope.template = null;
        $scope.nodes = [];
        $scope.ds = new $.ig.TreeHierarchicalDataSource({
            dataSource: $scope.nodes,
            treeDS: {
                childDataKey: "Context",
                initialExpandDepth: 1
            }
        });
        $scope.gridOptions = {
            width: '100%',
            height: '100%',
            dataSource: $scope.ds,
            dataSourceType: 'igTreeHierarchicalDataSource',
            autoGenerateColumns: false,
            primaryKey: "Context",
            autoCommit: true,
            childDataKey: "files",
            initialExpandDepth: 2,
            autofitLastColumn: true,
            features: [{
                name: "Selection",
            }, {
                name: "Updating",
                enableAddRow: false,
                enableDeleteRow: false,
                editMode: 'none'
            }],
            columns: [{
                headerText: 'Context',
                key: 'Context',
                width: '200px',
                dataType: 'string'
            }, {
                headerText: 'Number',
                key: 'Number',
                width: '100px',
                dataType: 'string'
            }, {
                headerText: 'Conformance',
                key: 'Conformance',
                width: '100px',
                dataType: 'string'
            }, {
                headerText: 'Cardinality',
                key: 'Cardinality',
                width: '100px',
                dataType: 'string'
            }, {
                headerText: 'Data Type',
                key: 'DataType',
                width: '100px',
                dataType: 'string'
            }, {
                headerText: 'Value',
                key: 'Value',
                width: '100px',
                dataType: 'string'
            }]
        };

        $scope.init = function (templateId, defaultImplementationGuideId) {
            ImplementationGuideService.getEditable()
                .then(function (implementationGuides) {
                    $scope.implementationGuides = implementationGuides;
                });

            EditorService.getTemplate(templateId)
                .then(function (template) {
                    $scope.template = template;

                    return EditorService.getNodes($scope.template.OwningImplementationGuideId, $scope.template.PrimaryContextType);
                })
                .then(function (nodes) {
                    /*
                    for (var i in nodes) {
                        $('#constraintsGrid').igTreeGridUpdating('addRow', nodes[i]);
                    }
                    */
                    $scope.ds.dataBind();
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
                        nodes.push(new NodeModel(results.data[i]));
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

var TemplateModel = function (data) {
    var self = this;

    self.Id = null;
    self.Name = null;

    angular.extend(self, data);
};

var NodeModel = function (data) {
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