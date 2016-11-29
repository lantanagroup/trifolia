var igViewApp = angular.module('igViewApp', ['hljs', 'ui.bootstrap', 'ngRoute']);

igViewApp.run(function ($templateCache, $anchorScroll) {
    // <script type="text/ng-template"> ... is preferred, but VS 2012 doesn't give intellisense there
    angular.element('script[type="text/html"]').each(function (idx, el) {
        $templateCache.put(el.id, el.innerHTML);
    });

    $anchorScroll.yOffset = 20;
});

igViewApp.config(function ($routeProvider, hljsServiceProvider) {
    hljsServiceProvider.setOptions({
        // replace tab with 4 spaces
        tabReplace: '    '
    });

    $routeProvider
        .when('/overview', {
            templateUrl: 'overview.html'
        })
        .when('/volume1:a?', {
            templateUrl: 'volume1.html'
        })
        .when('/volume2/:templateBookmark/:constraintNumber?', {
            templateUrl: 'template.html'
        })
        .when('/volume2', {
            templateUrl: 'volume2.html'
        })
        .when('/valuesets', {
            templateUrl: 'valuesets.html'
        })
        .when('/codesystems', {
            templateUrl: 'codesystems.html'
        })
        .when('/search', {
            templateUrl: 'search.html'
        })
        .when('/resources', {
            templateUrl: 'resources.html'
        })
        .when('/resources/:fileName', {
            templateUrl: 'resource.html',
            controller: 'ResourceCtrl'
        })
        .otherwise({
            redirectTo: '/overview'
        });
});

igViewApp.controller('ResourceCtrl', function ($scope, $routeParams, $sce) {
    $scope.fileName = $routeParams.fileName;

    $scope.$watch('Model', function (model) {
        if (!model) {
            return;
        }

        $scope.resourceInfo = _.find(model.FHIRResources, function (fhirResource) {
            return fhirResource.Name == $routeParams.fileName;
        });

        if ($scope.resourceInfo) {
            $scope.resource = JSON.parse($scope.resourceInfo.JsonContent);
            $scope.json = JSON.stringify($scope.resource, null, '\t');
            $scope.xml = vkbeautify.xml($scope.resourceInfo.XmlContent);

            if ($scope.resource.text && $scope.resource.text.div) {
                $scope.details = $sce.trustAsHtml($scope.resource.text.div);
            }
        }
    }, true);
});

igViewApp.directive('copyButton', function () {
    var copyText = function(element) {
        var doc = document, range, selection;
        if (doc.body.createTextRange) {
            range = document.body.createTextRange();
            range.moveToElementText(element);
            range.select();
            document.execCommand('copy');
        } else if (window.getSelection) {
            selection = window.getSelection();
            range = document.createRange();
            range.selectNodeContents(element);
            selection.removeAllRanges();
            selection.addRange(range);
            document.execCommand('copy');
        }

        window.getSelection().removeAllRanges();
    };

    return {
        restrict: 'E',
        scope: {},
        template: '<i ng-click="copy()" class="glyphicon glyphicon-export"></i>',
        link: function (scope, element) {
            scope.copy = function () {
                var parent = $(element).parent().find('pre');
                copyText(parent[0]);
            };
        }
    };
});

igViewApp.directive('fixHtmlContent', function ($location, $compile, $anchorScroll) {
    return {
        restrict: 'A',
        scope: '=',
        link: function (scope, element, attrs) {


            scope.linkToAnchor = function (id) {
                $location.hash(id);
                $anchorScroll();
            };

            scope.$watch(function () {
                $(element).find('a').each(function (index, item) {
                    if (!$(item).attr('href')) {
                        return;
                    }

                    var href = $(item).attr('href');

                    if (href.indexOf('#') === 0 && href.indexOf('#' + $location.$$path) !== 0) {

                        var itemEl = $(item);

                        var sectionRef = href.replace('#', '');

                        itemEl.attr('href', '');
                        itemEl.attr('ng-click', "linkToAnchor('" + sectionRef + "')");
                        var compiledEl = $compile(itemEl)(scope);

                        itemEl.replaceWith(compiledEl);
                    }
                });

                $(element).find('code').each(function (index, item) {
                    if ($(item).parents('div[hljs]').length > 0) {
                        return;
                    }

                    var compiledEle = $compile('<div hljs>' + $(item).text() + '</div>')(scope);
                    $(item).replaceWith(compiledEle);
                });
            });
        }
    };
});

igViewApp.directive('numberedHeading', function ($location, $compile, $anchorScroll) {
    return {
        restrict: 'A',
        scope: '=',
        link: function (scope, element, attrs) {

            scope.linkToAnchor = function (id) {
                $location.hash(id);
                $anchorScroll();
            };

            scope.$watch(function () {
                var segments = [];
                var headings = element.find(':header:not(.nocount):not(.numbered)');

                if (headings.length > 0) {
                    element.find('.table-of-contents').children().remove();
                }

                headings.each(function () {
                    var level = parseInt(this.nodeName.substring(1), 10);

                    if (segments.length == level) {
                        // from Hn to another Hn, just increment the last segment
                        segments[level - 1]++;
                    } else if (segments.length > level) {
                        // from Hn to Hn-x, slice off the last x segments, and increment the last of the remaining
                        segments = segments.slice(0, level);
                        segments[level - 1]++;
                    } else if (segments.length < level) {
                        // from Hn to Hn+x, (should always be Hn+1, but I'm doing some error checks anyway)
                        // add '1' x times.
                        for (var i = 0; i < (level - segments.length) ; i++) {
                            segments.push(1);
                        }
                    }

                    var headingText = '<span class="heading-number' + segments.length + '">' + segments.join('.') + '.</span><span>' + $(this).text() + '</span>';
                    $(this).text('');
                    $(this).append(headingText);
                    $(this).addClass('numbered');


                    if ($(this).attr('id')) {
                        $(this).prepend('<a id="' + $(this).attr('id') + '"></a>');

                        var linkElement = $('<li class="heading-level-' + segments.length + '"><a href="" ng-click="linkToAnchor(' + "'" + $(this).attr('id') + "'" + ')">' + headingText + '</a></li>');
                        var compiledLink = $compile(linkElement)(scope);
                        element.find('.table-of-contents')
                            .append(compiledLink);
                    } else {
                        element.find('.table-of-contents')
                            .append('<li class="heading-level-' + segments.length + '">' + headingText + '</li>');
                    }
                });
            });
        }
    };
});

igViewApp.directive('igDiagram', function ($location) {
    return {
        restrict: 'E',
        scope: {
            'Template': '=template',
            'Model': '=model'
        },
        template: '<div class="paper"></div>',
        link: function (scope, element, attrs) {
            var render = function () {
                var graph = new joint.dia.Graph;
                var paperOptions = {
                    el: element.find('.paper'),
                    width: element.offsetParent().width(),
                    height: 275,
                    gridSize: 1,
                    model: graph,
                    async: true
                };
                var paper = new joint.dia.Paper(paperOptions);

                var uml = joint.shapes.uml;

                var classes = {};
                var relations = [];
                var classTemplates = {};        // Mapping between classes and templates

                paper.on('cell:pointerclick', function (cellView, evt, x, y) {
                    var classId = cellView.model.id;

                    if (classTemplates[classId] && classTemplates[classId] != scope.Template) {
                        var selectedTemplate = classTemplates[classId];
                        location.href = '#/volume2/' + selectedTemplate.Bookmark;
                    }
                });

                var createClass = function (template) {
                    var identifierAttr = 'identifier:' + scope.Template.Identifier;
                    var width = template.Name.length * 5.5;

                    if (identifierAttr.length > template.Name.length) {
                        width = identifierAttr.length * 5.5;
                    }

                    var newClassOptions = {
                        size: { width: width, height: 100 },
                        name: template.Name.replace(/ /g, '_'),
                        attributes: [
                            identifierAttr
                        ],
                        attrs: {
                            '.uml-class-attrs-rect': {
                                fill: '#FFFFFF'
                            },
                            '.uml-class-methods-rect': {
                                fill: '#FFFFFF'
                            },
                            '.uml-class-name-rect': {
                                fill: '#FFFFFF'
                            }
                        }
                    };

                    for (var i in newClassOptions.attributes) {
                        var width = newClassOptions.attributes[i].length * 5;

                        if (newClassOptions.size.width < width) {
                            newClassOptions.size.width = width;
                        }
                    }

                    var newClass = new uml.Class(newClassOptions);
                    classes[template.Identifier] = newClass;
                    classTemplates[newClass.id] = template;

                    return newClass;
                };

                var thisTemplateClass = createClass(scope.Template);

                for (var i in scope.Template.ContainedByTemplates) {
                    var newClass = createClass(scope.Template.ContainedByTemplates[i]);
                    var relationship = new uml.Generalization({
                        source: { id: newClass.id },
                        target: { id: thisTemplateClass.id }
                    });
                    relations.push(relationship);
                }

                for (var i in scope.Template.ContainedTemplates) {
                    var newClass = createClass(scope.Template.ContainedTemplates[i]);
                    var relationship = new uml.Generalization({
                        source: { id: thisTemplateClass.id },
                        target: { id: newClass.id }
                    });
                    relations.push(relationship);
                }

                for (var i in scope.Template.ImplyingTemplates) {
                    var newClass = createClass(scope.Template.ImplyingTemplates[i]);
                    var relationship = new uml.Aggregation({
                        source: { id: newClass.id },
                        target: { id: thisTemplateClass.id }
                    });
                    relations.push(relationship);
                }

                if (scope.Template.ImpliedTemplate) {
                    var newClass = createClass(scope.Template.ImpliedTemplate);
                    var relationship = new uml.Aggregation({
                        source: { id: thisTemplateClass.id },
                        target: { id: newClass.id }
                    });
                    relations.push(relationship);
                }

                _.each(classes, function (c) { graph.addCell(c); });

                _.each(relations, function (r) { graph.addCell(r); });

                paper.scale(0.8, 0.8);

                var res = joint.layout.DirectedGraph.layout(graph, {
                    nodeSep: 50,
                    edgeSep: 80,
                    rankDir: "TB"
                });
                paper.setDimensions(res.width, res.height);
            };

            // This ensures the diagram is rendered after other angular events have processed
            // Otherwise, it is possible that the div that the diagram is contained within may
            // be hidden, and the diagram's rendering will be messed up. We must ensure the container
            // div is visible BEFORE the diagram is rendered
            setTimeout(render, 0);
        }
    };
});

igViewApp.service('DataService', function ($q, $http) {
    var service = {};
    var lastDataUrl;
    var lastData;
    var templates;

    service.GetData = function (templateIds, inferred) {
        var deferred = $q.defer();

        // If jsonData is %DATA% then we need to retrieve the data from the server
        // Otherwise, it is embedded in jsonData and we can immediately parse and bind to it
        if (jsonData != '%' + 'DATA' + '%') {
            deferred.resolve(jsonData);
        } else if (dataModelLocation) {
            var url = dataModelLocation;

            if (templateIds && templateIds.length > 0) {
                for (var i in templateIds) {
                    url += '&templateIds=' + templateIds[i];
                }
            }

            url += '&inferred=' + inferred;

            if (lastDataUrl == url) {
                deferred.resolve(lastData);
            } else {
                lastDataUrl = url;

                $http.get(url)
                    .success(function (data) {
                        lastData = data;
                        deferred.resolve(data);
                    })
                    .error(function (data) {
                        deferred.reject(data);
                    });
            }
        } else {
            deferred.reject('No data or location specified');
        }

        return deferred.promise;
    };

    service.GetTemplates = function (implementationGuideId) {
        var deferred = $q.defer();

        if (templates) {
            deferred.resolve(templates);
        } else {
            $http.get('/api/ImplementationGuide/' + implementationGuideId + '/Template')
                .success(function (results) {
                    templates = results;
                    deferred.resolve(results);
                })
                .error(function (results) {
                    deferred.reject(results);
                });
        }

        return deferred.promise;
    };

    return service;
});

igViewApp.controller('OptionsModalCtrl', function ($scope, $modalInstance, options, templates, allowDataChanges) {
    $scope.Options = JSON.parse(JSON.stringify(options));
    $scope.Templates = templates;
    $scope.AllowDataChanges = allowDataChanges;

    if ($scope.Options.ParentTemplates.length == 0) {
        $scope.Options.ParentTemplates.push(null);
    }

    $scope.Ok = function () {
        $modalInstance.close($scope.Options);
    };

    $scope.Cancel = function () {
        $modalInstance.dismiss();
    };

    $scope.$watch('Options', function () {
        var parentTemplates = $scope.Options.ParentTemplates;
        var lastParentTemplate = parentTemplates[parentTemplates.length - 1];

        for (var i = parentTemplates.length - 1; i >= 0; i--) {
            if (i != parentTemplates.length - 1 && !parentTemplates[i]) {
                parentTemplates.splice(i, 1);
            }
        }

        if (lastParentTemplate) {
            $scope.Options.ParentTemplates.push(null);
        }
    }, true);
});

igViewApp.controller('ViewCtrl', function ($rootScope, $scope, $http, $sce, $modal, $anchorScroll, $location, $route, $routeParams, $location, DataService) {
    $scope.BreadCrumbs = [{
        url: '#/overview',
        display: 'Home'
    }];
    $scope.Volume2Modes = {
        TemplateHierarchy: true,
        TemplateList: false,
        TemplateChanges: false
    };
    $scope.Model = null;
    $scope.IsDownloaded = window.location.protocol == 'file:';
    $scope.IsLoading = true;
    $scope.CurrentTableConstraints = [];
    $scope.CurrentValueSets = [];
    $scope.Volume2Filter = null;
    $scope.SearchText = null;
    $scope.Options = {
        ParentTemplates: [],
        Inferred: true,
        TemplateTabs: false
    };
    $scope.Template = null;
    $scope.TemplateHierarchy = [];
    $scope.SearchResults = {
        Templates: [],
        Constraints: [],
        ValueSets: [],
        CodeSystems: []
    };
    $scope.IsDebug = $location.$$search ? $location.$$search.debug : false;

    //#region: Changes Tab
    $scope.ShowTemplateChangesTab = function () {
        return !!$scope.Template && !!$scope.Template.Changes;
    };

    $scope.ViewTemplateChangesMode = 'Inline';

    $scope.GetChangeTooltip = function (changeType) {
        switch (changeType) {
            case 0:
                return 'Added';
            case 1:
                return 'Modified';
            case 2:
                return 'Removed';
            default:
                return '';
        }
    };
    //#endregion

    var getDataPromise;

    $rootScope.$on('$routeChangeStart', function (event, next, current) {
        var addBreadCrumb = function () {
            if (!next.$$route) {
                return;
            }

            var breadCrumb = {
                url: '#' + $location.path(),
                display: 'NO DISPLAY'
            };

            switch (next.$$route.templateUrl) {
                case 'template.html':
                    breadCrumb.display = $scope.Template.Name;
                    break;
                case 'valuesets.html':
                    breadCrumb.display = 'Value Sets';
                    break;
                case 'codesystems.html':
                    breadCrumb.display = 'Code Systems';
                    break;
                case 'overview.html':
                    breadCrumb.display = 'Home';
                    break;
                case 'volume1.html':
                    breadCrumb.display = 'Overview';
                    break;
                case 'volume2.html':
                    breadCrumb.display = 'Templates';
                    break;
                default:
                    return;
            }

            var lastBreadCrumb = $scope.BreadCrumbs.length > 0 ? $scope.BreadCrumbs[$scope.BreadCrumbs.length - 1] : null;

            if (!lastBreadCrumb || lastBreadCrumb.display != breadCrumb.display) {
                $scope.BreadCrumbs.push(breadCrumb);
            }
        };

        if (next && next.$$route && next.$$route.templateUrl == 'template.html') {
            getDataPromise
                .finally(function () {
                    for (var i in $scope.Model.Templates) {
                        if ($scope.Model.Templates[i].Bookmark == next.params.templateBookmark) {
                            $scope.Template = $scope.Model.Templates[i];
                        }
                    }

                    if ($scope.Template) {
                        $scope.CurrentTableConstraints.splice(0, $scope.CurrentTableConstraints.length);
                        $scope.CurrentValueSets.splice(0, $scope.CurrentValueSets.length);

                        var level = 0;
                        var loopConstraints = function (parent) {
                            level++;
                            var list = parent ? parent.Constraints : $scope.Template.Constraints;

                            for (var i in list) {
                                if (list[i].Context) {
                                    var whiteSpace = '';
                                    for (var x = 1; x < level; x++) {
                                        whiteSpace += '    ';
                                    }

                                    $scope.CurrentTableConstraints.push({
                                        Level: whiteSpace,
                                        Constraint: list[i]
                                    });
                                }

                                if (list[i].ValueSetIdentifier) {
                                    for (var x in $scope.Model.ValueSets) {
                                        var cValueSet = $scope.Model.ValueSets[x];

                                        if (cValueSet.Identifier == list[i].ValueSetIdentifier && $scope.CurrentValueSets.indexOf(cValueSet) < 0) {
                                            $scope.CurrentValueSets.push(cValueSet);
                                            break;
                                        }
                                    }
                                }

                                loopConstraints(list[i]);
                            }

                            level--;
                        };

                        // Populate the list of constraints for the table
                        loopConstraints();
                    }

                    addBreadCrumb();
                });
        } else {
            $scope.Template = null;
            $scope.CurrentTableConstraints.splice(0, $scope.CurrentTableConstraints.length);
            $scope.CurrentValueSets.splice(0, $scope.CurrentValueSets.length);

            addBreadCrumb();
        }
    });

    var updateLocation = function (selection) {
        switch (selection) {
            case 'Overview':
                $location.path('/overview');
                break;
            case 'Volume1':
                $location.path('/volume1');
                break;
            case 'Volume2':
                $location.path('/volume2');
                break;
            case 'ValueSets':
                $location.path('/valuesets');
                break;
            case 'CodeSystems':
                $location.path('/codesystems');
                break;
            default:
                $location.path('/volume2');
                break;
        }
    };

    $scope.Search = function () {
        if (!$scope.Model) {
            return;
        }

        $scope.BreadCrumbs = [{
            url: '#/overview',
            display: 'Overview'
        }];
        $location.path('/search');

        $scope.SearchResults = {
            Templates: [],
            Constraints: [],
            ValueSets: [],
            CodeSystems: []
        };

        var regex = new RegExp($scope.SearchText, 'gi');

        var searchConstraints = function (template, constraints) {
            for (var i in constraints) {
                var constraint = constraints[i];

                if (regex.test(constraint.Narrative)) {
                    $scope.SearchResults.Constraints.push({
                        Template: template,
                        Constraint: constraint
                    });
                }

                searchConstraints(template, constraint.Constraints);
            }
        };

        // Templates and constraints
        for (var i in $scope.Model.Templates) {
            var template = $scope.Model.Templates[i];

            if (regex.test(template.Name)) {
                $scope.SearchResults.Templates.push({
                    Priority: 2,
                    Template: template,
                    Name: template.Name
                });
            } else if (regex.test(template.Identifier)) {
                $scope.SearchResults.Templates.push({
                    Priority: 1,
                    Template: template,
                    Name: template.Name
                });
            } else if (regex.test(template.Description)) {
                $scope.SearchResults.Templates.push({
                    Priority: 3,
                    Template: template,
                    Name: template.Name
                });
            }

            searchConstraints(template, template.Constraints);
        }

        // Value sets
        for (var i in $scope.Model.ValueSets) {
            var valueSet = $scope.Model.ValueSets[i];

            if (regex.test(valueSet.Name)) {
                $scope.SearchResults.ValueSets.push({
                    Priority: 2,
                    ValueSet: valueSet,
                    Name: valueSet.Name
                });
            } else if (regex.test(valueSet.Identifier)) {
                $scope.SearchResults.ValueSets.push({
                    Priority: 1,
                    ValueSet: valueSet,
                    Name: valueSet.Name
                });
            } else if (regex.test(valueSet.Description)) {
                $scope.SearchResults.ValueSets.push({
                    Priority: 3,
                    ValueSet: valueSet,
                    Name: valueSet.Name
                });
            }
        }

        // Code systems
        for (var i in $scope.Model.CodeSystems) {
            var codeSystem = $scope.Model.CodeSystems[i];

            if (regex.test(codeSystem.Name)) {
                $scope.SearchResults.CodeSystems.push({
                    Priority: 2,
                    CodeSystem: codeSystem,
                    Name: codeSystem.Name
                });
            } else if (regex.test(codeSystem.Identifier)) {
                $scope.SearchResults.CodeSystems.push({
                    Priority: 1,
                    CodeSystem: codeSystem,
                    Name: codeSystem.Name
                });
            }
        }

        setTimeout(function () {
            $('#Search').highlight($scope.SearchText);
        }, 500);
    };

    $scope.GetValueSetAnchor = function (valueSet) {
        var anchor = valueSet.Name
            .replace(/\t/g, '_')
            .replace(/ /g, '_')
            .replace(/[^\w\s]/g, '');
        return anchor;
    };

    $scope.GetConstraintTableValue = function (constraint) {
        var value;

        if (constraint.Value) {
            value = constraint.Value;
        } else if (constraint.ContainedTemplate) {
            value = '<a href="#/volume2/' + constraint.ContainedTemplate.Bookmark + '">' + constraint.ContainedTemplate.Name + '</a>';
        } else if (constraint.ValueSetIdentifier) {
            var valueSet;

            for (var i in $scope.Model.ValueSets) {
                if ($scope.Model.ValueSets[i].Identifier == constraint.ValueSetIdentifier) {
                    valueSet = $scope.Model.ValueSets[i];
                    break;
                }
            }

            if (valueSet) {
                if ($scope.Options.TemplateTabs) {
                    value = '<a href="#/valuesets#' + $scope.GetValueSetAnchor(valueSet) + '">' + valueSet.Name + ' (' + valueSet.Identifier + ')</a>';
                } else {
                    value = '<a href="#' + $location.path() + '#' + $scope.GetValueSetAnchor(valueSet) + '">' + valueSet.Name + ' (' + valueSet.Identifier + ')</a>';
                }
            }
        }

        return $scope.GetHtml(value);
    };

    $scope.GetBreadCrumbDisplay = function (breadCrumb) {
        switch (breadCrumb) {
            case 'Overview':
                return 'Overview';
            case 'Volume1':
                return 'Overview';
            case 'Volume2':
                return 'Templates';
            case 'CodeSystems':
                return 'Code Systems';
            case 'ValueSets':
                return 'Value Sets';
            case 'Search':
                return 'Search Results';
            default:
                return breadCrumb.Name;
        }
    };

    $scope.GetBreadCrumbLink = function (breadCrumb) {
        switch (breadCrumb) {
            case 'Overview':
                return '#/overview';
            case 'Volume1':
                return '#/volume1';
            case 'Volume2':
                return '#/volume2';
            case 'CodeSystems':
                return '#/codesystems';
            case 'ValueSets':
                return '#/valuesets';
            case 'Search':
                return '#/search';
            default:
                return '#/volume2/' + breadCrumb.Identifier;
        }
    };

    $scope.SelectBreadCrumb = function (index) {
        if (index == $scope.BreadCrumbs.length - 1) {
            return;
        }

        $scope.BreadCrumbs.splice(index + 1, $scope.BreadCrumbs.length - (index + 1));
    };

    $scope.GetElementIdentifier = function (identifier) {
        if (!identifier) {
            return;
        }

        return identifier.replace(/\./g, '_').replace(/\:/g, '_');
    };

    $scope.GetDownloadLocation = function () {
        if (!$scope.Model) {
            return;
        }

        if ($scope.Model.ImplementationGuideFileId) {
            return '/IG/DownloadSnapshot?implementationGuideId=' + $scope.Model.ImplementationGuideId + '&fileId=' + $scope.Model.ImplementationGuideFileId;
        } else {
            var url = '/IG/Download?implementationGuideId=' + $scope.Model.ImplementationGuideId;

            if ($scope.Options.ParentTemplates && $scope.Options.ParentTemplates.length > 0) {
                for (var i in $scope.Options.ParentTemplates) {
                    if ($scope.Options.ParentTemplates[i]) {
                        url += '&templateIds=' + $scope.Options.ParentTemplates[i];
                    }
                }
            }

            url += '&inferred=' + $scope.Options.Inferred;

            return url;
        }
    };

    $scope.GetHtml = function (text) {
        return $sce.trustAsHtml(text);
    };

    $scope.HasRelationships = function (template) {
        if (!template) {
            return false;
        }

        return template.ImpliedTemplate ||
            template.ContainedTemplates.length > 0 ||
            template.ContainedByTemplates.length > 0 ||
            template.ImplyingTemplates.length > 0;
    };

    $scope.GetPath = function () {
        return $location.$$path;
    };

    $scope.SetVolume2Filter = function (filter) {
        $scope.Volume2Filter = filter;
    }

    $scope.GetTemplatesByType = function (templateTypeId) {
        if (!$scope.Model) {
            return [];
        }

        var retTemplates = [];
        var regex = null;

        if ($scope.Volume2Filter) {
            regex = new RegExp('^' + $scope.Volume2Filter, 'i');
        }

        for (var i in $scope.Model.Templates) {
            var template = $scope.Model.Templates[i];

            if (template.TemplateTypeId != templateTypeId) {
                continue;
            }

            if (regex && !regex.test(template.Name)) {
                continue;
            }

            retTemplates.push(template);
        }

        return retTemplates;
    };

    $scope.GetUncontainedTemplates = function () {
        var templates = [];

        for (var i in $scope.Model.Templates) {
            var template = $scope.Model.Templates[i];

            if (!template.ContainedByTemplates || template.ContainedByTemplates.length == 0) {
                templates.push(template);
            }
        }

        return templates;
    };

    $scope.GetChildTemplates = function (parent) {
        var templates = [];

        for (var i in parent.ContainedTemplates) {
            for (var x in $scope.Model.Templates) {
                if ($scope.Model.Templates[x].Identifier == parent.ContainedTemplates[i].Identifier) {
                    templates.push($scope.Model.Templates[x]);
                    break;
                }
            }
        }

        return templates;
    };

    $scope.LoadData = function () {
        var templateIds = [];

        for (var i in $scope.Options.ParentTemplates) {
            if ($scope.Options.ParentTemplates[i]) {
                templateIds.push($scope.Options.ParentTemplates[i]);
            }
        }

        $scope.IsLoading = true;

        getDataPromise = DataService.GetData(templateIds, $scope.Options.Inferred);
        getDataPromise
            .then(function (dataModel) {
                $scope.Model = dataModel;
                document.title = $scope.Model.ImplementationGuideName + ' - ' + $scope.Model.Status;

                if ($scope.Model.PublishDate) {
                    document.title += ' ' + $scope.Model.PublishDate;
                }

                var templateHierarchy = [];

                var findTemplate = function (identifier) {
                    for (var i in $scope.Model.Templates) {
                        if ($scope.Model.Templates[i].Identifier == identifier) {
                            return $scope.Model.Templates[i];
                        }
                    }
                };

                var alreadyExistsInHierarchy = function (identifier, hierarchyModel) {
                    var current = hierarchyModel;

                    while (current) {
                        if (current.Identifier == identifier) {
                            return true;
                        }

                        current = current.Parent;
                    }

                    return false;
                };

                var populateHierarchy = function (template, parent) {
                    var hierarchyModel = {
                        Name: template.Name,
                        Identifier: template.Identifier,
                        Bookmark: template.Bookmark,
                        Parent: parent,
                        Children: []
                    };

                    for (var i in template.ContainedTemplates) {
                        var containedTemplate = findTemplate(template.ContainedTemplates[i].Identifier);

                        if (!containedTemplate) {
                            continue;
                        }

                        if (alreadyExistsInHierarchy(containedTemplate.Identifier, hierarchyModel)) {
                            continue;
                        }

                        var childHierarchy = populateHierarchy(containedTemplate, hierarchyModel);
                        hierarchyModel.Children.push(childHierarchy);
                    }

                    return hierarchyModel;
                };

                var deleteParentFromModel = function (model) {
                    for (var i in model.Children) {
                        deleteParentFromModel(model.Children[i]);
                    }

                    delete model.Parent;
                };

                // Get top-level templates
                for (var i in $scope.Model.Templates) {
                    var template = $scope.Model.Templates[i];
                    var templateType = null;

                    if (!template.ContainedByTemplates || template.ContainedByTemplates.length == 0) {
                        var hierarchyModel = populateHierarchy(template);
                        var hierarchyRootModel = null;

                        for (var x in $scope.Model.TemplateTypes) {
                            if ($scope.Model.TemplateTypes[x].TemplateTypeId == template.TemplateTypeId) {
                                templateType = $scope.Model.TemplateTypes[x];
                                break;
                            }
                        }

                        for (var x in templateHierarchy) {
                            if (templateHierarchy[x].TemplateTypeId == templateType.TemplateTypeId) {
                                hierarchyRootModel = templateHierarchy[x];
                                break;
                            }
                        }

                        if (!hierarchyRootModel) {
                            hierarchyRootModel = {
                                TemplateTypeId: templateType.TemplateTypeId,
                                Name: templateType.Name,
                                Templates: []
                            };
                            templateHierarchy.push(hierarchyRootModel);
                        }

                        deleteParentFromModel(hierarchyModel);
                        hierarchyRootModel.Templates.push(hierarchyModel);
                    }
                }

                $scope.TemplateHierarchy = templateHierarchy;
            })
            .catch(function (err) {
                alert(err);
            })
            .finally(function () {
                $scope.IsLoading = false;
            });
    };

    $scope.EditOptions = function () {
        var modalInstance = $modal.open({
            templateUrl: 'options_modal.html',
            controller: 'OptionsModalCtrl',
            size: 'sm',
            resolve: {
                options: function () {
                    return $scope.Options;
                },
                templates: function () {
                    if (!$scope.Model.ImplementationGuideFileId && !$scope.IsDownloaded) {
                        return DataService.GetTemplates($scope.Model.ImplementationGuideId);
                    }

                    return [];
                },
                allowDataChanges: function () {
                    return !$scope.Model.ImplementationGuideFileId && !$scope.IsDownloaded;
                }
            }
        });

        modalInstance.result.then(function (options) {
            $scope.Options = options;

            $scope.LoadData();
        });
    };

    /*
        Initialization
    */
    $scope.LoadData();
});