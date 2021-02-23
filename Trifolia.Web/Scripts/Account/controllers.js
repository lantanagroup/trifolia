angular.module('Trifolia').controller('MyProfileCtrl', function ($scope, $q, $http, UserService, ConfigService) {
    $scope.userModel = null;
    $scope.message = '';
    $scope.messageIsWarning = false;
    $scope.enableReleaseAnnouncements = false;
    $scope.subscribed = false;
    $scope.orgTypes = [
        'Provider',
        'Regulator',
        'Vendor',
        'Other'
    ];
    $scope.umlsCredentialsConfirmed = true;

    $scope.toggleReleaseAnnouncementSubscription = function () {
        if (!$scope.subscribed) {
            UserService.subscribeToReleaseAnnouncements()
                .then(function () {
                    $scope.subscribed = true;
                })
                .catch(function (err) {
                    $scope.message = err;
                    $scope.messageIsWarning = true;
                });
        } else {
            UserService.unsubscribeFromReleaseAnnouncements()
                .then(function () {
                    $scope.subscribed = false;
                })
                .catch(function (err) {
                    $scope.message = err;
                    $scope.messageIsWarning = true;
                });
        }
    };

    $scope.umlsCredentialsChanged = function () {
        if (!$scope.userModel.UMLSApiKey) {
            $scope.umlsCredentialsConfirmed = true;
            return;
        }

        $scope.umlsCredentialsConfirmed = false;
    };

    $scope.validateUMLSApiKey = function () {
        UserService.validateUMLSApiKey($scope.userModel.UMLSApiKey)
            .then(function (results) {
                if (!results.CredentialsValid) {
                    $scope.message = 'Invalid UMLS API Key or license.';
                    $scope.messageIsWarning = true;
                } else if (!results.LicenseValid) {
                    $scope.umlsCredentialsConfirmed = true;
                    $scope.message = 'UMLS API Key does not have a valid/active license to UMLS/VSAC data.';
                    $scope.messageIsWarning = true;
                } else {
                    $scope.umlsCredentialsConfirmed = true;
                    $scope.message = 'UMLS API Key is valid.';
                    $scope.messageIsWarning = false;
                }
            })
            .catch(function (err) {
                $scope.message = err;
                $scope.messageIsWarning = true;
            });
    };

    $scope.save = function () {
        UserService.saveMyUser($scope.userModel)
            .then(function () {
                $scope.message = 'Successfully saved profile information';
                $scope.messageIsWarning = false;
            })
            .catch(function (err) {
                $scope.message = err;
                $scope.messageIsWarning = true;
            });
    };

    $scope.init = function () {
        var promises = [UserService.getMyUser(), ConfigService.getEnableReleaseAnnouncements(), UserService.getReleaseAnnouncementSubscription()];

        $q.all(promises)
            .then(function (results) {
                var userModel = results[0];
                var enableReleaseAnnouncements = results[1];
                var subscribed = results[2];
                $scope.userModel = userModel;
                $scope.enableReleaseAnnouncements = enableReleaseAnnouncements;
                $scope.subscribed = subscribed;

                if ($scope.userModel.OpenIdConfigUrl) {
                    return $http.get($scope.userModel.OpenIdConfigUrl);
                }
            })
            .then(function (results) {
                if (results && results.data) {
                    $scope.openIdConfig = results.data;
                }
            })
            .catch(function (err) {
                $scope.message = err;
                $scope.messageIsWarning = true;
            });
    };
});