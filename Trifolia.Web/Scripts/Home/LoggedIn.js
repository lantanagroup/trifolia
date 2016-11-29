function LoadWhatsNew(versionNumber) {
    var url = '/Help/Whatsnew.html';

    if (versionNumber) {
        url += '?' + versionNumber;
    }

    $.get(url, function (data) {
        var bodyContent = data.replace(/^[\S\s]*<body[^>]*?>/i, "").replace(/<\/body[\S\s]*$/i, "");
        var body = $(bodyContent);
        body.find('#topic_header_nav').remove();
        body.find('img[src]').each(function () {
            var img = this;
            var src = $(img).attr('src');
            $(img).attr('src', '/Help/' + src);
        });
        $('#whatsnew').append(body);
    });
}