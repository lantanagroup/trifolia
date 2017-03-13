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

function loadHelpTopics(versionNumber) {
    var versionExt = versionNumber ? '?' + versionNumber : '';
    var whatsNewUrl = '/Help/Whatsnew.html' + versionExt;
    var introUrl = '/Help/Introduction.html' + versionExt;

    loadHelpTopic('#whatsnew', whatsNewUrl);
    loadHelpTopic('#intro', introUrl);
}