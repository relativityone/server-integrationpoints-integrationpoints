'use strict';
(function (windowObj, root, ko) {
    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    var currentSettingsFromUi = function () {
        var model = {
            ImportType: windowObj.RelativityImport.koModel.selectedImportType(),
            HasStartLine: windowObj.RelativityImport.koModel.fileContainsColumn(),
            LineNumber: windowObj.RelativityImport.koModel.startLine(),
            LoadFile: windowObj.RelativityImport.koModel.Fileshare(),
            EncodingType: windowObj.RelativityImport.koModel.DataFileEncodingType(),
            AsciiColumn: windowObj.RelativityImport.koModel.selectedColumnAsciiDelimiter(),
            AsciiQuote: windowObj.RelativityImport.koModel.selectedQuoteAsciiDelimiter(),
            AsciiNewLine: windowObj.RelativityImport.koModel.selectedNewLineAsciiDelimiter(),
            AsciiMultiLine: windowObj.RelativityImport.koModel.selectedMultiLineAsciiDelimiter(),
            AsciiNestedValue: windowObj.RelativityImport.koModel.selectedNestedValueAsciiDelimiter()
        };

        console.log(model);
        return model;
    };

    windowObj.RelativityImport.GetCurrentUiModel = currentSettingsFromUi;

    //An event raised when the user has clicked the Next or Save button.
    //Leaving the custom settings page and going to field mapping screen.
    message.subscribe('submit', function () {
        //Execute save logic that persists the root.
        var current = currentSettingsFromUi();
        var stringified = JSON.stringify(current);

        this.publish("saveState", stringified);
        this.publish('saveComplete', stringified);

        //TODO: validation logic here to allow moving off the settings page (e.g. check for valid load file)
    });

    //An event raised when a user clicks the Back button.
    //Leaving the custom settings page and going back to the first RIP screen
    message.subscribe('back', function () {
        //Execute save logic that persists the root.
        var current = currentSettingsFromUi();
        var stringified = JSON.stringify(current);
        this.publish("saveState", stringified);

        windowObj.RelativityImport.UI.removeCustomDropdown();
    });

    //An event raised when the host page has loaded the current settings page.
    //Arriving at the custom settings page; either from hitting Back from field mapping, or Next from the first RIP screen
    message.subscribe('load', function (model) {

        if (windowObj.parent.$(windowObj.RelativityImport.UI.idSelector(windowObj.RelativityImport.UI.Elements.CUSTOM_BUTTON)).length < 1) {
            windowObj.RelativityImport.UI.initCustomDropdown();
        }

        //TODO: Populate UI with values from model object
    });

})(this, IP, ko);