ko.validation.rules['extensionIdentifierUnique'] = {
    validator: function (val, otherVal) {
        var template = otherVal.template;
        var thisExtension = otherVal.thisExtension;

        for (var i in template.Extensions()) {
            if (template.Extensions()[i] == thisExtension) {
                continue;
            }

            if (template.Extensions()[i].Identifier() == val) {
                return false;
            }
        }

        return true;
    },
    message: 'The extension\'s identifier must be unique.'
};

ko.validation.rules['extensionIdentifierFormat'] = {
    validator: function (val, otherVal) {
        var regex = /^([a-z][a-z0-9+.-]*):(?:\/\/((?:(?=((?:[a-z0-9-._~!$&'()*+,;=:]|%[0-9A-F]{2})*))(\3)@)?(?=(\[[0-9A-F:.]{2,}\]|(?:[a-z0-9-._~!$&'()*+,;=]|%[0-9A-F]{2})*))\5(?::(?=(\d*))\6)?)(\/(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/]|%[0-9A-F]{2})*))\8)?|(\/?(?!\/)(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/]|%[0-9A-F]{2})*))\10)?)(?:\?(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/?]|%[0-9A-F]{2})*))\11)?(?:#(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/?]|%[0-9A-F]{2})*))\12)?$/gi;
        return regex.test(val);
    },
    message: 'The extension identifier\'s format is not valid.'
};

ko.validation.rules['constraintCardinalityFormat'] = {
    validator: function (val, isModifier) {

        var regex = new RegExp(/^([0-9]*)[\.][\.]([0-9]*|[\*])$/gm);
        var matching = regex.exec(val);
        if (matching) {
            if (matching[1].length <= 3 && matching[2].length <= 3) {
                if (parseInt(matching[1]) <= parseInt(matching[2]) || matching[2] == '*') {
                    if (isModifier() && parseInt(matching[1]) >= 1) {
                        return true;
                    } else if (isModifier()) {
                        return false;
                    }
                    return true;
                }
            }
        }
        return false;
    },
    message: 'The cardinality is not properly formatted (e.g. 0..1 and each value is < 1000) or the element is a modifier and doesn\'t have a cardinality of at least 1..# .'
};

ko.validation.rules['templateNameUnique'] = {
    validator: function (val, otherVal) {
        var isValid = false;
        $.ajax({
            url: '/api/Template/Validate/Name/' + encodeURIComponent(val),
            async: false,
            cache: false,
            success: function (data) {
                isValid = data;
            }
        });
        return isValid;
    },
    message: 'This name is not available.'
};

ko.validation.rules['templateOidFormat'] = {
    validator: function (val, otherVal) {
        var foundMatch = false;

        if (val.match(/^urn:oid:[0-2](\.(0|[1-9][0-9]*))*$/g)) {
            foundMatch = true;
        }

        if (val.match(/^urn:hl7ii:[0-2](\.(0|[1-9][0-9]*))*\:(.+)$/g)) {
            foundMatch = true;
        }

        if (val.match(/^uri:(.+)/g)) {
            foundMatch = true;
        } 

        if (val.match(/^http[s]?:\/\/(.+)/g)) {
            foundMatch = true;
        }

        return foundMatch;
    },
    message: 'This identifier is not valid. Valid formats are "urn:hl7ii:<OID>:<VERSION>", "urn:oid:<OID>", "uri:<URI>" and "http(s)://<URL>".'
};

ko.validation.rules['codeSystemOidFormat'] = {
    validator: function (val, otherVal) {
        var foundMatch = false;

        if (val.match(/^urn:oid:[0-2](\.(0|[1-9][0-9]*))*$/g)) {
            foundMatch = true;
        }

        if (val.match(/^http[s]?:\/\/(.+)/g)) {
            foundMatch = true;
        }

        return foundMatch;
    },
    message: 'This identifier is not valid. Valid formats are "urn:oid:<OID>" and "http(s)://<URL>".'
};

ko.validation.rules['valueSetOidFormat'] = {
    validator: function (val, otherVal) {
        var foundMatch = false;

        if (val.match(/^urn:oid:[0-2](\.(0|[1-9][0-9]*))*$/g)) {
            foundMatch = true;
        }

        if (val.match(/^http[s]?:\/\/(.+)/g)) {
            foundMatch = true;
        }

        return foundMatch;
    },
    message: 'This identifier is not valid. Valid formats are "urn:oid:<OID>" and "http(s)://<URL>".'
};

ko.validation.rules['valueSetOidUnique'] = {
    validator: function (val, otherVal) {
        var isValid = false;
        var url = '/api/Terminology/ValueSet/Find?identifier=' + encodeURIComponent(val);

        var ignoreValueSetId = otherVal;

        if (typeof otherVal === 'function') {
            ignoreValueSetId = otherVal();
        }

        $.ajax({
            url: url,
            async: false,
            cache: false,
            success: function (data) {
                if (!data || data == ignoreValueSetId) {
                    isValid = true;
                }
            }
        });

        return isValid;
    },
    message: 'This value set identifier is not available.'
};

ko.validation.rules['codeSystemOidUnique'] = {
    validator: function (val, otherVal) {
        var isValid = false;
        var url = '/api/Terminology/CodeSystem/Find?identifier=' + encodeURIComponent(val);

        var ignoreCodeSystemId = otherVal;

        if (typeof otherVal === 'function') {
            ignoreCodeSystemId = otherVal();
        }

        $.ajax({
            url: url,
            async: false,
            cache: false,
            success: function (data) {
                if (!data || data == ignoreCodeSystemId) {
                    isValid = true;
                }
            }
        });

        return isValid;
    },
    message: 'This code system identifier is not available.'
};

ko.validation.rules['templateIdentifierUnique'] = {
    validator: function (val, otherVal) {
        var isValid = false;
        var url = '/api/Template/Validate/Oid?identifier=' + encodeURIComponent(val);

        if (typeof otherVal === 'function') {
            var ignoreTemplateId = otherVal();
        }        

        if (ignoreTemplateId) {
            url += '&ignoreTemplateId=' + ignoreTemplateId;
        }

        $.ajax({
            url: url,
            async: false,
            cache: false,
            success: function (data) {
                isValid = data;
            }
        });
        return isValid;
    },
    message: 'This template identifier is not available.'
};

ko.validation.rules['hl7iiValidation'] = {
    validator: function (val, otherVal) {
        var prevVersionOid = null;
        if (typeof otherVal === 'function') {
            prevVersionOid = otherVal();
            if (prevVersionOid.indexOf('urn:oid:') != 0 && prevVersionOid.indexOf('urn:hl7ii:') != 0) {
                return true;
            }
        } else if (otherVal === false) return true;

        //Check if the previous version is an HL7 oid. If not, don't do validation.
        if (val.indexOf('urn:hl7ii:') != 0) {
            return true;
        }

        //Obtain the oid of the inputted value for the url
        var oid = val.replace(/([^\:]*\:){2}/, '');
        oid = oid.substring(0, oid.indexOf(':'));

        //Obtain the oid of the previous version
        prevVersionOid = prevVersionOid.replace(/([^\:]*\:){2}/, '');
        prevVersionOid = prevVersionOid.substring(0, prevVersionOid.indexOf(':'));

        if (prevVersionOid != oid) return false;

        return true;
    },
    message: 'The OID doesn\'t match the previous version\'s OID.'
};

ko.validation.rules['igFileUrlUnique'] = {
    validator: function (val, otherVal) {
        var isValid = false;

        if (!val) {
            return true;
        }

        var url = '/api/ImplementationGuide/URL/Validate?url=' + encodeURIComponent(val);

        var ignoreFileId = otherVal;

        if (typeof otherVal === 'function') {
            ignoreFileId = otherVal();
        }

        url += '&ignoreFileId=';

        if (ignoreFileId) {
            url += ignoreFileId;
        }

        $.ajax({
            url: url,
            async: false,
            cache: false,
            success: function (data) {
                isValid = data;
            }
        });

        return isValid;
    },
    message: 'This URL is already in use.'
};

ko.validation.rules['igFileUrl'] = {
    validator: function (val, otherVal) {
        var regex = /^[a-zA-Z0-9/_]*$/;

        return regex.test(val);
    },
    message: 'The URL contains invalid characters.'
};

ko.validation.rules['sourceUrl'] = {
    validator: function (val, otherVal) {
        if (!val) {
            return true;
        }

        var urlRegex = /((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)/;
        return urlRegex.test(val);
    },
    message: 'The format of the URL is incorrect.'
};

ko.validation.registerExtenders();