angular.module('Trifolia', ['ui.bootstrap', 'ngCookies'])
    .run(function ($templateCache) {
        // <script type="text/ng-template"> ... is preferred, but VS 2012 doesn't give intellisense there
        angular.element('script[type="text/html"]').each(function (idx, el) {
            $templateCache.put(el.id, el.innerHTML);
        });
    });