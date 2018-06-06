function loadHelpTopics(versionNumber) {
    var versionExt = versionNumber ? '?' + versionNumber : '';
    var introUrl = '/Help/Introduction.html' + versionExt;
    
    loadHelpTopic('#intro', introUrl);
}