ko.bindingHandlers.sceditor = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var value = valueAccessor();

        setTimeout(function () {
            $(element).sceditor({
                plugins: 'xhtml',
                style: '/Styles/jquery.sceditor.default.min.css',
                toolbar: 'bold,italic,underline,strike,subscript,superscript|left,center,right,justify|font,size,color,removeformat|cut,copy,paste,pastetext|bulletlist,orderedlist,indent,outdent|table|code,quote|horizontalrule,image,email,link,unlink,anchor|emoticon,youtube,date,time|ltr,rtl|print,maximize,source',
                imageOpts: allBindingsAccessor().imageOpts
            });

            var instance = $(element).sceditor('instance');

            instance.nodeChanged(function (e) {
                var newValue = instance.val();
                value(newValue);
            });
            
            instance.keyUp(function (e) {
                var newValue = instance.val();
                value(newValue);
            });

            instance.trifoliaEditorChange = function () {
                var newValue = instance.val();
                value(newValue);
            };
        }, 300);
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
    }
};