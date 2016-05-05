DataTypesViewModel = function (loadAllUrl, saveUrl, loadSelectedUrl) {
    var self = this;
    self.loadAllUrl = loadAllUrl;
    self.saveUrl = saveUrl;
    self.loadSelectedUrl = loadSelectedUrl;

    self.allItems = ko.observableArray([]);
    self.selectedItems = ko.observableArray([]);

    self.initialModelState = ko.observable({});

    self.IsDirty = ko.observable(false);

    
    $(window).bind('beforeunload', function () {
        if (self.IsDirty()) {
            return "You have unsaved changes!";
        }
    });

    $.getJSON(self.loadSelectedUrl, function (data) {
        self.selectedItems(data);
        self.IsDirty(false);
    });

    self.selectedItems.subscribe(function () {
        self.IsDirty(true);
    });

    $.getJSON(self.loadAllUrl, function (data) {
        self.allItems(data);

        self.allItems.removeAll(self.selectedItems());
    });


    self.AddSelectedItems = function (selectedOptions) {

        moveSelectedItems(selectedOptions, self.allItems(), self.selectedItems());

        self.selectedItems.sort(); //this will call valuehasmutated, save a little performance...
        self.allItems.valueHasMutated();
    }

    self.RemoveSelectedItems = function (selectedOptions) {

        moveSelectedItems(selectedOptions, self.selectedItems(), self.allItems());

        self.allItems.sort();
        self.selectedItems.valueHasMutated();//want the subscribers to be notified

    }

    var moveSelectedItems = function(selectedItems, fromArray, toArray)
    {
        //flatten the selectedItems Array
        var items = ko.utils.arrayMap(selectedItems, function (item) {
            return item.value;
        });

        ko.utils.arrayPushAll(toArray, items);

        ko.utils.arrayForEach(items, function (item) {
            ko.utils.arrayRemoveItem(fromArray, item);
        });
    }

    self.AddAllItems = function () {
        ko.utils.arrayPushAll(self.selectedItems(), self.allItems());
        self.selectedItems.sort();
        self.allItems.removeAll();
    }

    self.RemoveAllItems = function () {
        ko.utils.arrayPushAll(self.allItems(), self.selectedItems());
        self.selectedItems.removeAll();
        self.allItems.sort();
    }

    self.cancelClick = function () {
        window.location.href = '/Account/IGTypeManagement/ImplementationGuideTypes.aspx';
    }

    self.saveAllChanges = function () {
        var stringData = JSON.stringify(self.selectedItems());

        $("#mainBody").block();

        $.ajax({
            url: self.saveUrl,
            type: 'POST',
            dataType: 'json',
            data: stringData,
            contentType: 'application/json; charset=utf-8',
            complete: function (jqXHR, textStatus) {
                $("#mainBody").unblock();

                showMessage('Changes Saved Successfully');

                supportPopup.Hide();
            },
            success: function (updatedModel) {
                self.IsDirty(false);
            },
            error: function(err){
                alert('There was an error saving your changes; please submit a support ticket');
            }
        });
        
    };
};