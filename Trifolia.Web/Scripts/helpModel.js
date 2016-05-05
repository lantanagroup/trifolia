var HelpModel = function (helpMapping, dialogEle) {
    var self = this;
    var windowType;
    var pageUrl;

    var getPartialPageId = function (pageId) {
        var pageIdSplitIndex = pageId.indexOf('?');

        if (pageIdSplitIndex > 0) {
            return pageId.substring(0, pageIdSplitIndex);
        }
    };

    self.setHelp = function (url) {
        var anchorName = url.indexOf("#") >= 0 ? url.substring(url.indexOf("#") + 1) : null;

        $('#helpFrame').attr('src', url);
        $('#helpFrame').on('load', function () {
            $('#helpFrame').contents().find('#topic_header').remove();
            $('#helpFrame').contents().find('#topic_footer').remove();

            if (anchorName) {
                $('#helpFrame').scrollTo('a[name=' + anchorName + ']');
            }
        });
    }

    self.getHelpNode = function (pageId) {
        var helpPages = $(helpMapping).find("help").find("helpPages add");

        for (var i = 0; i < helpPages.length; i++) {
            var routeRegexString = helpPages[i].attributes['route'].value;
            routeRegexString = routeRegexString
                .replace(/\*/g, '(.*?)');
            var routeRegex = new RegExp('^' + routeRegexString + '$', 'gi');

            if (routeRegex.test(pageId)) {
                return helpPages[i];
            }
        }
    };

    self.getHelpPageUrl = function(pageId) {
        var rootLocation = $(helpMapping).find('help')[0].attributes['rootLocation'].value;
        var found = self.getHelpNode(pageId);

        if (found) {
            var helpPageLocation = found.attributes['helpLocation'].value;
            return rootLocation + helpPageLocation;
        }

        return "";
    }

    self.getHelpPageType = function (pageId) {
        var found = self.getHelpNode(pageId);

        if (found) {
            if (found.attributes['windowType']) {
                return found.attributes['windowType'].value;
            } else {
                return 'PopupWindow';
            }
        }

        return 'NotFound';
    }

    self.showHelpContext = function (key) {
        var helpContextUrl = self.getHelpPageUrl(key);

        if (!helpContextUrl || helpContextUrl == '') {
            console.log('No help found for the specified context key of ' + key);
            return;
        }

        self.setHelp(helpContextUrl);
        $(dialogEle).modal('show');
    };

    self.showHelpMenu = function() {
        if (windowType == 'NewWindow') {
            window.open(pageUrl, "_TDBHelpWindow");
        } else if (windowType == 'PopupWindow') {
            self.setHelp(pageUrl);
            $(dialogEle).modal('show');
        } else {
            alert("Sorry... no help information is available for this page yet.");
        }
    }

    self.initialize = function () {
        windowType = self.getHelpPageType(location.pathname);

        if (windowType != 'NotFound') {
            pageUrl = self.getHelpPageUrl(location.pathname);
        }

        $(dialogEle).find("iframe").load(function (item) {
            $(item.target).contents().find('p.MCWebHelpFramesetLink a').attr('target', '_blank');
        });

        $(dialogEle).modal({
            show: false,
            onOpen: function () {
                $(dialogEle).find('.panel-body').css('overflow', 'hidden');
            }
        });
    };

    self.initialize();
};

(function ($) {

    $.fn.helpContext = function (key) {
        $(this)
            .html('')
            .addClass('helpContext')
            .click(function (event) {
                helpModel.showHelpContext(key);
                event.stopPropagation();
            });

        return this;
    };

}(jQuery));