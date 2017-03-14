// Karma configuration
// Generated on Tue Mar 14 2017 13:41:57 GMT-0700 (Pacific Daylight Time)

module.exports = function (config) {
    config.set({

        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',


        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],


        // list of files / patterns to load in the browser
        files: [
          '../Trifolia.Web/Scripts/lib/jquery/jquery-1.10.2.min.js',
          '../Trifolia.Web/Scripts/lib/q/q.js',
          '../Trifolia.Web/Scripts/lib/lodash.min.js',
          '../Trifolia.Web/Scripts/lib/angular/angular.min.js',
          '../Trifolia.Web/Scripts/lib/angular/angular-route.min.js',
          '../Trifolia.Web/Scripts/lib/angular/angular-highlight.min.js',
          '../Trifolia.Web/Scripts/lib/angular/ui-bootstrap-tpls-0.12.1.min.js',

          '../Trifolia.Web/Scripts/angular/startup.js',
          '../Trifolia.Web/Scripts/angular/services.js',
          '../Trifolia.Web/Scripts/angular/filters.js',
          '../Trifolia.Web/Scripts/angular/directives.js',
          '../Trifolia.Web/Scripts/angular/treegrid/treegrid.js',
          '../Trifolia.Web/Scripts/NewTemplateEditor/editorController.js',

          './Karma/angular-mocks.js',
          './Karma/Tests/**'
        ],


        // list of files to exclude
        exclude: [
            '../Trifolia.Web/Scripts/lib/angular/angular-highlight.min.js'
        ],


        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
        },


        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress'],


        // web server port
        port: 9876,


        // enable / disable colors in the output (reporters and logs)
        colors: true,


        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: false,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ['Chrome'],


        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false,

        // Concurrency level
        // how many browser should be started simultaneous
        concurrency: Infinity,

        client: {
            captureConsole: true
        }
    })
}
