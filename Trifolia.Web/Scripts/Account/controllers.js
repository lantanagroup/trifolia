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

    $scope.validateUmlsCredentials = function () {
        UserService.validateUmlsCredentials($scope.userModel.UmlsUsername, $scope.userModel.UmlsPassword)
            .then(function (results) {
                if (!results.CredentialsValid) {
                    $scope.message = 'Invalid UMLS credentials.';
                    $scope.messageIsWarning = true;
                } else if (!results.LicenseValid) {
                    $scope.umlsCredentialsConfirmed = true;
                    $scope.message = 'UMLS credentials do not have a valid/active license to UMLS/VSAC data.';
                    $scope.messageIsWarning = true;
                } else {
                    $scope.umlsCredentialsConfirmed = true;
                    $scope.message = 'UMLS credentials are valid, and have a valid/active license to UMLS/VSAC data.';
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
                $scope.openIdConfig = results.data;
            })
            .catch(function (err) {
                $scope.message = err;
                $scope.messageIsWarning = true;
            });
    };
});