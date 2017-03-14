describe('EditorController', function () {
    beforeEach(module('Trifolia'));

    var $controller;

    beforeEach(inject(function (_$controller_) {
        // The injector unwraps the underscores (_) from around the parameter names when matching
        $controller = _$controller_;
    }));

    describe('parseIdentifier', function () {
        it('should parse identifier so that the implementation guide\'s identifier is the base (without a trailing slash)', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'http://my.test/ig/base'
            };

            $scope.parseIdentifier('http://my.test/ig/base/profile_id');

            expect($scope.identifier.base).toBe('http://my.test/ig/base/');
            expect($scope.identifier.ext).toBe('profile_id');
        });

        it('should parse identifier so that the implementation guide\'s identifier is the base (with a trailing slash)', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'http://my.test/ig/base/'
            };

            $scope.parseIdentifier('http://my.test/ig/base/profile_id');

            expect($scope.identifier.base).toBe('http://my.test/ig/base/');
            expect($scope.identifier.ext).toBe('profile_id');
        });

        it('should parse urn:oid: identifier and match it to the implementation guide\'s identifier', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'urn:oid:2.16.1.844'
            };

            $scope.parseIdentifier('urn:oid:2.16.1.844.1.244');

            expect($scope.identifier.base).toBe('urn:oid:2.16.1.844.');
            expect($scope.identifier.ext).toBe('1.244');
        });

        it('should parse urn:oid: identifier with a different oid than the implementation guide', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'urn:oid:2.16.1.844'
            };

            $scope.parseIdentifier('urn:oid:2.16.1.42.33');

            expect($scope.identifier.base).toBe('urn:oid:');
            expect($scope.identifier.ext).toBe('2.16.1.42.33');
        });

        it('should parse urn:hl7ii: identifier', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'urn:oid:2.16.1.844'
            };

            $scope.parseIdentifier('urn:hl7ii:2.16.840.1.4422');

            expect($scope.identifier.base).toBe('urn:hl7ii:');
            expect($scope.identifier.ext).toBe('2.16.840.1.4422');
        });

        it('should parse http:// identifier that is different from implementation guide', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'http://my.test.ig/base'
            };

            $scope.parseIdentifier('http://my.ig.test/profile_id');

            expect($scope.identifier.base).toBe('http://');
            expect($scope.identifier.ext).toBe('my.ig.test/profile_id');
        });

        it('should parse https:// identifier that is different from implementation guide', function () {
            var $scope = {};
            var controller = $controller('EditorController', { $scope: $scope });
            $scope.implementationGuide = {
                Identifier: 'https://my.test.ig/base'
            };

            $scope.parseIdentifier('https://my.ig.test/profile_id');

            expect($scope.identifier.base).toBe('https://');
            expect($scope.identifier.ext).toBe('my.ig.test/profile_id');
        });
    });
});