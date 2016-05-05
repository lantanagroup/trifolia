var typeSubstitues = {
    'System.Int32': 'Integer',
    'System.String': 'String',
    'System.Boolean': 'Boolean',
    'System.DateTime': 'DateTime',
    'System.Nullable': 'Nullable',
    'System.Collections.Generic.List': 'List',
    'System.Collections.Generic.IEnumerable': 'List'
};

var getTypeSubstitute = function (type) {
    if (typeSubstitues[type]) {
        return typeSubstitues[type];
    }

    return type;
};

angular.module('ApiHelp', ['ui.bootstrap', 'ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/overview', {
                templateUrl: 'overview.html',
                controller: 'OverviewCtrl'
            })
            .when('/controller/:controllerName', {
                templateUrl: 'controller.html',
                controller: 'ControllerCtrl'
            })
            .when('/model/:modelName', {
                templateUrl: 'model.html',
                controller: 'ModelCtrl'
            })
            .otherwise({
                redirectTo: '/overview'
            });
    }])
    .run(function ($anchorScroll, $rootScope, $http, $sce) {
        //$anchorScroll.yOffset = 50;
        $rootScope.ApiDescriptions = [];
        $rootScope.Controllers = [];
        $rootScope.Models = [];
        $rootScope.ApiKey = '';
        $rootScope.Organization = '';
        $rootScope.UserName = '';

        $rootScope.FindModel = function (name) {
            if (!name) {
                return;
            }

            name = name.replace(/\+/g, '.');

            if (name.indexOf('System.Collections.Generic.List<') == 0) {
                name = name.substring(32, name.length - 1);
            } else if (name.indexOf('List<') == 0) {
                name = name.substring(5, name.length - 1);
            } else if (name.indexOf('System.Collections.Generic.IEnumerable<') == 0) {
                name = name.substring(39, name.length - 1);
            } else if (name.indexOf('IEnumerable<') == 0) {
                name = name.substring(12, name.length - 1);
            }

            for (var i in $rootScope.Models) {
                if ($rootScope.Models[i].Name == name || $rootScope.Models[i].FullName == name) {
                    return $rootScope.Models[i];
                }
            }
        };        

        $rootScope.GetTypeDisplay = function (type) {
            if (!type) {
                return '';
            }

            var typeSplit = type.split(': ');
            var genericRegex = new RegExp('^(.+?)<(.+?)>$', 'gi');
            var wrapBegin = '';
            var wrapEnd = '';
            var typeName = getTypeSubstitute(type);
            var type = typeSplit.shift();
            var description = typeSplit.length > 0 ? typeSplit.join(':') : '';

            if (genericRegex.test(type)) {
                var matches = genericRegex.exec(type);
                matches = genericRegex.exec(type);

                if (matches[1].indexOf('Hl7.Fhir.Model') == 0) {
                    var resourceType = matches[1].substring(15).toLowerCase();
                    wrapBegin = '<a href="http://www.hl7.org/fhir/dstu2/' + resourceType + '.html" target="_new">' + getTypeSubstitute(matches[1]) + '</a>&lt;';
                } else {
                    wrapBegin = getTypeSubstitute(matches[1]) + '&lt;';
                }

                wrapEnd = '&gt;';
                typeName = getTypeSubstitute(matches[2]);
            }

            if (typeName.indexOf('Hl7.Fhir.Model') == 0) {
                var resourceType = typeName.substring(15).toLowerCase();
                var url = 'http://www.hl7.org/fhir/dstu2/' + resourceType + '.html';
                wrapBegin = wrapBegin + '<a href="' + url + '" target="_new">';
                wrapEnd = '</a>' + wrapEnd;
            } else {
                var foundModel = $rootScope.FindModel(typeName);

                if (foundModel) {
                    wrapBegin = wrapBegin + '<a href="#/model/' + typeName + '">';
                    wrapEnd = '</a>' + wrapEnd;
                }
            }

            var ret = wrapBegin + typeName + wrapEnd;

            if (description) {
                ret += '<br/>' + description;
            }

            return $sce.trustAsHtml(ret);
        };

        $http.get('/api/Help/_meta')
            .then(function (results) {
                $rootScope.ApiDescriptions = results.data;

                for (var i in $rootScope.ApiDescriptions) {
                    if ($rootScope.Controllers.indexOf($rootScope.ApiDescriptions[i].Controller) == -1) {
                        $rootScope.Controllers.push($rootScope.ApiDescriptions[i].Controller);
                    }
                }

                for (var i in $rootScope.ApiDescriptions) {
                    var apiDesc1 = $rootScope.ApiDescriptions[i];

                    for (var x in $rootScope.ApiDescriptions) {
                        var apiDesc2 = $rootScope.ApiDescriptions[x];

                        if (apiDesc1 != apiDesc2 && apiDesc1.Controller == apiDesc2.Controller && apiDesc1.Name == apiDesc2.Name) {
                            apiDesc2.Name += '$';
                        }
                    }
                }
            })
            .catch(function (err) {
                alert(err);
            });

        $http.get('/api/Help/_meta/type')
            .then(function (results) {
                $rootScope.Models = results.data;
            })
            .catch(function (err) {
                alert(err);
            });
    });

angular.module('ApiHelp').controller('OverviewCtrl', function ($scope, $rootScope, $routeParams) {

});

angular.module('ApiHelp').controller('ControllerCtrl', function ($scope, $rootScope, $routeParams, $location) {
    $scope.SelectedControllerName = $routeParams.controllerName;

    var generateAuthorizationHeader = function () {
        if (!$rootScope.UserName || !$rootScope.Organization || !$rootScope.ApiKey) {
            return;
        }

        var timestamp = Date.now();
        var salt = Math.random();
        var properties = $rootScope.UserName + '|' + $rootScope.Organization + '|' + timestamp + '|' + salt + '|';
        var hashContent = properties + $rootScope.ApiKey;
        var authBasicValue = properties + CryptoJS.SHA1(hashContent).toString(CryptoJS.enc.Base64);
        var b64AuthBasicValue = btoa(authBasicValue.toString(CryptoJS.enc.Base64));

        return 'Bearer ' + b64AuthBasicValue;
    };

    $scope.GetActionLink = function (actionName) {
        return '#' + $location.$$path + '#' + actionName;
    };

    $scope.GetUrlDisplay = function (url) {
        if (!url || url.indexOf('?') < 0) {
            return url;
        }

        return url.substring(0, url.lastIndexOf('?'));
    };

    $scope.GetControllerMethods = function () {
        var ret = [];

        for (var i in $rootScope.ApiDescriptions) {
            if ($rootScope.ApiDescriptions[i].Controller == $scope.SelectedControllerName) {
                ret.push($rootScope.ApiDescriptions[i]);
            }
        }

        return ret;
    };

    $scope.AddParam = function (actionDescription) {
        actionDescription.Parameters.push(actionDescription.NewParam);
        actionDescription.NewParam = { Name: '', Documentation: '', Type: 'System.String', RequestValue: '', IsCustom: true };
    };

    $scope.RemoveParam = function (actionDescription, index) {
        actionDescription.Parameters.splice(index, 1);
    };

    $scope.TestAction = function (action) {
        action.ResponseStatus =
            action.ResponseBody =
            action.ResponseStatusText = '';

        var setResponse = function (a1, a2, a3) {
            var response = a2 == 'success' ? a3 : a1;

            $scope.$apply(function () {
                if (action.RequestFormat == 'application/json') {
                    try {
                        action.ResponseBody = JSON.stringify(JSON.parse(response.responseText), null, '\t');
                    } catch (ex) {
                        action.ResponseBody = response.responseText;
                    }
                } else {
                    action.ResponseBody = response.responseText;
                }

                action.ResponseStatus = response.status;
                action.ResponseStatusText = response.statusText;
            });
        };

        var url = '/' + action.RelativePath;

        if (url.indexOf('?') < 0) {
            url += '?';
        }

        for (var i in action.Parameters) {
            var repKey1 = '{' + action.Parameters[i].Name + '}';
            var repKey2 = '{' + action.Parameters[i].Name + '?}';
            var repValue = action.Parameters[i].RequestValue ? action.Parameters[i].RequestValue : '';

            if (repValue) {
                repValue = encodeURIComponent(repValue);
            }

            if (url.indexOf(repKey1) < 0 && url.indexOf(repKey2) < 0) {
                url += '&' + action.Parameters[i].Name + '=' + repValue;
            } else {
                url = url.replace(repKey1, repValue).replace(repKey2, repValue);
            }
        }

        var authorizationHeader = generateAuthorizationHeader();
        var headers = {
            'Content-Type': action.RequestFormat,
            'Accept': action.RequestFormat
        };

        if (authorizationHeader) {
            headers['Authorization'] = authorizationHeader;
        }

        $.ajax({
            method: action.HttpMethod,
            url: url,
            headers: headers,
            success: setResponse,
            error: setResponse,
            data: action.RequestBody
        });
    };
});

angular.module('ApiHelp').controller('ModelCtrl', function ($scope, $rootScope, $routeParams) {
    $scope.SelectedModel = null;

    var getModelJSExample = function (model) {
        var example = {};

        for (var i in model.Properties) {
            var property = model.Properties[i];
            var exampleProperty = {};

            if (!property.Type) {
                continue;
            }

            if (property.Type.indexOf('System.Collections.Generic.List<') == 0 || property.Type.indexOf('List<') == 0 || property.Type.indexOf('System.Collections.Generic.IEnumerable<') == 0 || property.Type.indexOf('IEnumerable<') == 0 || property.Type.indexOf('[]') > 0) {
                var propertyModel = $rootScope.FindModel(property.Type);

                if (propertyModel) {
                    exampleProperty = [getModelJSExample(propertyModel)];
                } else {
                    exampleProperty = [];
                }
            } else if (property.Type == 'System.String') {
                exampleProperty = 'Test Value';
            } else if (property.Type == 'System.DateTime') {
                exampleProperty = new Date().toString();
            } else if (property.Type == 'System.Int32') {
                exampleProperty = 2;
            } else if (property.Type == 'System.Boolean') {
                exampleProperty = false;
            }

            example[property.Name] = exampleProperty;
        }

        return example;
    };

    $scope.GetModelJSONExample = function () {
        var example = getModelJSExample($scope.SelectedModel);
        return JSON.stringify(example, null, '\t');
    };

    $rootScope.$watch('Models', function () {
        $scope.SelectedModel = $rootScope.FindModel($routeParams.modelName);
    }, true);
});