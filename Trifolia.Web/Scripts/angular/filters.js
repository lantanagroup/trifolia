angular.module('Trifolia')
    .filter('implementationGuideName', function (ImplementationGuideService) {
        var implementationGuides = null;
        var isLoading = false;

        implementationGuideNameFilter.$stateful = true;

        function implementationGuideNameFilter(input) {
            if (implementationGuides == null) {
                if (!isLoading) {
                    isLoading = true;
                    ImplementationGuideService.getImplementationGuides()
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
        }

        return implementationGuideNameFilter;
    });