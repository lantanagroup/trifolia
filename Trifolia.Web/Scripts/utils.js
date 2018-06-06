GUID = function () {
    var S4 = function () {
        return Math.floor(
                Math.random() * 0x10000 /* 65536 */
            ).toString(16);
    };

    return (
            S4() + S4() + "-" +
            S4() + "-" +
            S4() + "-" +
            S4() + "-" +
            S4() + S4() + S4()
        );
};

function loadHelpTopic(selector, url) {
    $.get(url, function (data) {
        var bodyContent = data.replace(/^[\S\s]*<body[^>]*?>/i, "").replace(/<\/body[\S\s]*$/i, "");
        var body = $(bodyContent).find('.main-content');
        body.find('.additionalFormats').remove();
        body.find('img[src]').each(function () {
            var img = this;
            var src = $(img).attr('src');
            $(img).attr('src', '/Help/' + src);
        });
        $(selector).append(body);
    });
};

function createUUID() {
    // http://www.ietf.org/rfc/rfc4122.txt
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";  // bits 12-15 of the time_hi_and_version field to 0010
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);  // bits 6-7 of the clock_seq_hi_and_reserved to 01
    s[8] = s[13] = s[18] = s[23] = "-";

    var uuid = s.join("");
    return uuid;
};

function stringFormat(str, arguments) {
    for (i = 0; i < arguments.length; i++) {
        var repl = '{' + '}';
        str = str.replace(repl, arguments[i]);
    }

    return str;
}

function getTemplateViewUrl(id, oid) {
    var oidRegex = /^urn:oid:(.+)/g;
    var iiRegex = /^urn:hl7ii:(.+)?:(.+)?/g;
    var uriRegex = /^uri:(.+)/g;

    try {
        var oidMatch = oidRegex.exec(oid);

        if (oidMatch) {
            return '/TemplateManagement/View/OID/' + oidMatch[1];
        }

        var iiMatch = iiRegex.exec(oid);

        if (iiMatch) {
            return '/TemplateManagement/View/II/' + iiMatch[1] + '/' + iiMatch[2];
        }

        var uriMatch = uriRegex.exec(oid);

        if (uriMatch) {
            var uri = uriMatch[1];

            if (uri.indexOf(':') >= 0 || uri.indexOf('/') >= 0 || uri.lastIndexOf('.') == oid.length - 1) {
                if (id) {
                    return '/TemplateManagement/View/Id/' + id;
                }
            }

            return '/TemplateManagement/View/URI?uri=' + encodeURIComponent(uri);
        }

        if (oid.indexOf(':') >= 0 || oid.indexOf('/') >= 0 || oid.lastIndexOf('.') == oid.length - 1) {
            return '/TemplateManagement/View/Id/' + id;
        }

        return 'javascript:alert(\'Incorrect pattern for template identifier detected!\')';
    }
    catch (ex) {
        console.log("Error building view url for template");
    }
}

function getTemplateEditUrl(id, oid) {
    var oidRegex = /^urn:oid:(.+)/g;
    var iiRegex = /^urn:hl7ii:(.+)?:(.+)?/g;
    var uriRegex = /^uri:(.+)/g;

    var oidMatch = oidRegex.exec(oid);

    if (oidMatch) {
        return '/TemplateManagement/Edit/OID/' + oidMatch[1];
    }

    var iiMatch = iiRegex.exec(oid);

    if (iiMatch) {
        return '/TemplateManagement/Edit/II/' + iiMatch[1] + '/' + iiMatch[2];
    }

    var uriMatch = uriRegex.exec(oid);

    if (uriMatch) {
        var uri = uriMatch[1];

        if (uri.indexOf(':') >= 0 || uri.indexOf('/') >= 0 || uri.lastIndexOf('.') == oid.length - 1) {
            return '/TemplateManagement/Edit/Id/' + id;
        }

        return '/TemplateManagement/Edit/URI/' + uri;
    }

    if (oid.indexOf(':') >= 0 || oid.indexOf('/') >= 0 || oid.lastIndexOf('.') == oid.length - 1) {
        return '/TemplateManagement/Edit/Id/' + id;
    }

    return 'javascript:alert(\'Incorrect pattern for template identifier detected!\')';
}

function formatDateObj(date) {
    if (!date) {
        return '';
    }

    date = new Date(date);
    var curr_date = date.getDate();
    var curr_month = date.getMonth() + 1; //Months are zero based
    var curr_year = date.getFullYear();

    return curr_month + "/" + curr_date + "/" + curr_year;
}

function fireTrifoliaEvent(eventName) {
    if (!document.trifoliaEvents) {
        document.trifoliaEvents = {};
    }

    if (!document.trifoliaEvents[eventName]) {
        document.trifoliaEvents[eventName] = [];
    }

    document.trifoliaEvents[eventName].push(new Date());
}

function findTrifoliaEvent(eventName) {
    if (!document.trifoliaEvents || !document.trifoliaEvents[eventName]) {
        return [];
    }

    return document.trifoliaEvents[eventName];
}

function joinUrl() {
    var joined = '';

    for (var i = 0; i < arguments.length; i++) {
        var arg = arguments[i].toString();

        // All but first arg should have a beginning /
        while (i != 0 && arg.indexOf('/') != 0) {
            arg = '/' + arg;
        }

        // No args should have an ending /
        while (arg.lastIndexOf('/') == arg.length - 1) {
            arg = arg.substring(0, arg.length - 1);
        }

        joined += arg;
    }

    return joined;
}

function WhatsNewViewModel(versionNumber) {
    var self = this;
    var whatsNewUrl = '/Help/Whatsnew.html?v=' + versionNumber;
    var cookieKey = 'whatsNewLastSeenVersion';
    var lastSeenVersionNumber = $.cookie(cookieKey);

    self.HideNextTime = ko.observable(lastSeenVersionNumber === versionNumber);

    self.CloseWhatsNew = function () {
        $('#whatsNewDialog').modal('hide');
    };

    self.OpenWhatsNew = function () {
        $('#whatsNewDialog').modal('show');
    };

    self.Initialize = function () {
        var shouldShow = lastSeenVersionNumber !== versionNumber;
        var versionExt = versionNumber ? '?' + versionNumber : '';
        var whatsNewUrl = '/Help/Whatsnew.html' + versionExt;
        loadHelpTopic('#whatsNewBody', whatsNewUrl);

        $('#whatsNewDialog').on('hidden.bs.modal', function (e) {
            if (self.HideNextTime()) {
                $.cookie(cookieKey, versionNumber);
            } else {
                $.removeCookie(cookieKey);
            }
        });

        $("#whatsNewDialog").modal({
            backdrop: 'static',
            show: shouldShow
        });
    };

    self.Initialize();
}