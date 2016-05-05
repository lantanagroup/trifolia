function HasTemplateEditPermission(templateId) {
    var hasEditPermission = false;
    var data = {
        templateId: templateId
    };

    $.ajax({
        async: false,
        type: "POST",
        url: "/PermissionManagement/HasTemplateEditPermission",
        data: data,
        complete: function (result) {
            hasEditPermission = result.responseText == "true";
        }
    });

    return hasEditPermission;
}

function HasTemplateViewPermission(templateId) {
    var hasViewPermission = false;
    var data = {
        templateId: templateId
    };

    $.ajax({
        async: false,
        type: "POST",
        url: "/PermissionManagement/HasTemplateViewPermission",
        data: data,
        complete: function (result) {
            hasViewPermission = result.responseText == "true";
        }
    });

    return hasViewPermission;
}

function HasImplementationGuideEditPermission(implementationGuideId) {
    var hasEditPermission = false;
    var data = {
        implementationGuideId: implementationGuideId
    };

    $.ajax({
        async: false,
        type: "POST",
        url: "/PermissionManagement/HasImplementationGuideEditPermission",
        data: data,
        complete: function (result) {
            hasEditPermission = result.responseText == "true";
        }
    });

    return hasEditPermission;
}

function HasImplementationGuideViewPermission(implementationGuideId) {
    var hasViewPermission = false;
    var data = {
        implementationGuideId: implementationGuideId
    };

    $.ajax({
        async: false,
        type: "POST",
        url: "/PermissionManagement/HasImplementationGuideViewPermission",
        data: data,
        complete: function (result) {
            hasViewPermission = result.responseText == "true";
        }
    });

    return hasViewPermission;
}

function HasSecurables(securables) {
    var hasSecurables = false;

    $.ajax({
        async: false,
        type: "POST",
        url: "/api/Security/HasSecurables",
        contentType: 'application/json',
        data: JSON.stringify(securables),
        complete: function (result) {
            hasSecurables = result.responseText == "true";
        }
    });

    return hasSecurables;
}

function HasSecurable(securable) {
    var hasSecurable = false;

    $.ajax({
        async: false,
        type: "POST",
        url: "/api/Security/HasSecurables",
        contentType: 'application/json',
        data: JSON.stringify([securable]),
        complete: function (result) {
            hasSecurables = result.responseText == "true";
        }
    });

    return hasSecurables;
}