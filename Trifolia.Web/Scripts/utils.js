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
                return '/TemplateManagement/View/Id/' + id;
            }

            return '/TemplateManagement/View/URI/' + uri;
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