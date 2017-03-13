$.sceditor.command.set("anchor", {
    exec: function (caller) {
        var editor = this;

        var content = $('<div></div>');
        var anchorNameField = $('<input type="text" class="form-control" placeholder="Anchor ID" />');
        var okButton = $('<button type="button" class="btn btn-primary">OK</button>');
        content.append(anchorNameField);
        content.append(okButton);

        okButton.click(function () {
            if (!anchorNameField.val()) {
                alert('You must specify an anchor ID');
                return;
            }

            var range = editor.getRangeHelper().selectedRange();

            if (range.startContainer.localName == 'p') {
                $(range.startContainer).wrapInner('<a id="' + anchorNameField.val() + '"></a>');
            } else {
                editor.insert('<a id="' + anchorNameField.val() + '">', '</a>');
            }

            anchorNameField.val('');
			editor.closeDropDown(true);
        });

        editor.createDropDown(caller, 'create-anchor', content);
    },
    txtExec: function(caller) {

        var editor = this;

        var content = $('<div></div>');
        var anchorNameField = $('<input type="text" class="form-control" placeholder="Anchor ID" />');
        var okButton = $('<button type="button" class="btn btn-primary">OK</button>');
        content.append(anchorNameField);
        content.append(okButton);

        okButton.click(function () {
            if (!anchorNameField.val()) {
                alert('You must specify an anchor ID');
                return;
            }

            editor.insert('<a id="' + anchorNameField.val() + '">', '</a>');
            anchorNameField.val('');
            editor.closeDropDown(true);
        });

        editor.createDropDown(caller, 'create-anchor', content);
    },
    tooltip: 'Anchor ID'
});

$.sceditor.command.set("image", {
    exec: function (caller) {
        var editor = this;
        
        var imageBase = editor.opts.imageOpts().baseUrl;
        var content = $('<div />');
        var buttonPanel = $('<div />');
        var okButton = $('<button type="button" class="btn btn-primary">OK</button>');

        okButton.click(function (e) {
            var selectedImage = content.find('.sceditor-customimage.selected');

            if (selectedImage.length > 0) {
                var item = selectedImage.data('fileitem');
                editor.insert('<img src="' + imageBase + item.FileName + '" />');
                editor.updateOriginal();

                if (editor.trifoliaEditorChange) {
                    editor.trifoliaEditorChange();
                }
            }
            editor.closeDropDown(true);
            e.preventDefault();
        });

        var images = [];

        $.getJSON(editor.opts.imageOpts().listUrl)
            .done(function (data) {

                images = data;

                $.each(data, function (i, item) {
                    var img = $('<img class="sceditor-customimage" src="' + imageBase + item.FileName + '" />')
                                .click(function () {
                                    var isSelected = $(this).hasClass('selected');
                                    content.find('.sceditor-customimage').removeClass('selected');
                                    $(this).toggleClass('selected', !isSelected);
                                });

                    img.data('fileitem', item);

                    content.append(img);    
                });

                buttonPanel.append(okButton);
                content.append(buttonPanel);

                editor.createDropDown(caller, 'insertimage', content);
            });
    },
    tooltip: 'Insert an image'
});