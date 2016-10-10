'use strict';
(function (windowObj, root, ko) {
    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    var currentSettingsFromUi = function () {
        var model = {
            ImportType: $('#import-importType option:selected').val(),
            //ProcessingSource: windowObj.import.StorageRoot ? windowObj.import.StorageRoot : "",
            //LoadDataFrom: windowObj.import.SelectedFolderPath ? windowObj.import.SelectedFolderPath : "",
            HasStartLine: $("#import-hascolumnnames-checkbox").attr("checked") ? true : false,
            LineNumber: $("#import-columnname-numbers").val(),
            LoadFile: windowObj.RelativityImport.koModel.Fileshare()
        };

        console.log(model);
        return model;
    };

    windowObj.RelativityImport.GetCurrentUiModel = currentSettingsFromUi;

    //An event raised when the user has clicked the Next or Save button.
    message.subscribe('submit', function () {
        //Execute save logic that persists the root.
        var current = currentSettingsFromUi();
        var stringified = JSON.stringify(current);

        this.publish("saveState", stringified);
        this.publish('saveComplete', stringified);

        //TODO: validation logic here to allow moving off the settings page (e.g. check for valid load file)
        /*
        if (parsedModel.CsvFilePath === '') {
            IP.frameMessaging().dFrame.IP.message.error.raise('Please select a load file to continue.');
        } else {
            //windowObj.parent.$('#previewFile').remove();
            //Communicate to the host page that it to continue.
            this.publish('saveComplete', localModel);
        }
        */
    });

    //An event raised when a user clicks the Back button.
    message.subscribe('back', function () {
        //Execute save logic that persists the root.
        console.log('back handler');
        var current = currentSettingsFromUi();
        var stringified = JSON.stringify(current);
        this.publish("saveState", stringified);

        /*
        this.publish('saveState', JSON.stringify(_getModel()));
            windowObj.RelativityImport.UI.removeCustomDropdown();
            */
    });

    //An event raised when the host page has loaded the current settings page.
    message.subscribe('load', function (model) {

        if (windowObj.parent.$(windowObj.RelativityImport.UI.idSelector(windowObj.RelativityImport.UI.Elements.CUSTOM_BUTTON)).length) {
            windowObj.RelativityImport.UI.removeCustomDropdown();
        } else {
            windowObj.RelativityImport.UI.initCustomDropdown();
        };

        //TODO: Populate UI with values from model object

        // if (model != '') {
        //    var sourceConfig = JSON.parse(model);
        //    windowObj.import.StorageRoot = sourceConfig.StorageRoot;
        //    windowObj.import.SelectedFolderPath = sourceConfig.CsvFilePath;
        // }
        //else {
        //    windowObj.import.StorageRoot = "";
        //    windowObj.import.SelectedFolderPath = "";
        // }
        //windowObj.import.IPFrameMessagingLoadEvent = true;
    });

})(this, IP, ko);