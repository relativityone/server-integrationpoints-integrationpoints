'use strict';
(function (windowObj, root, ko) {
    //Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();
	var ImportTypeEnum = windowObj.RelativityImport.ImportTypeEnum;
    var currentSettingsFromUi = function () {
        var model = {
            WorkspaceId: windowObj.RelativityImport.WorkspaceId,
            ImportType: windowObj.RelativityImport.koModel.selectedImportType(),
            HasColumnName: windowObj.RelativityImport.koModel.fileContainsColumn(),
            LineNumber: windowObj.RelativityImport.koModel.startLine(),
            LoadFile: windowObj.RelativityImport.koModel.Fileshare(),
            EncodingType: windowObj.RelativityImport.koModel.DataFileEncodingType(),
            AsciiColumn: windowObj.RelativityImport.koModel.selectedColumnAsciiDelimiter(),
            AsciiQuote: windowObj.RelativityImport.koModel.selectedQuoteAsciiDelimiter(),
            AsciiNewLine: windowObj.RelativityImport.koModel.selectedNewLineAsciiDelimiter(),
            AsciiMultiLine: windowObj.RelativityImport.koModel.selectedMultiLineAsciiDelimiter(),
            AsciiNestedValue: windowObj.RelativityImport.koModel.selectedNestedValueAsciiDelimiter(),
            ProcessingSourceLocation: windowObj.RelativityImport.koModel.ProcessingSourceLocation()
        };

        console.log(model);
        return model;
    };

	var currentImageSettingsFromUi = function () {
		var importType = windowObj.RelativityImport.koModel.selectedImportType();
		var imageImport = 'true';
		var forProduction = 'false';
		if (importType === ImportTypeEnum.Production) {
			forProduction = 'true';
		}
		
		var model = {
			ImageImport: imageImport,
			ForProduction: forProduction,
			AutoNumberImages: windowObj.RelativityImport.koModel.autoNumberPages(),
			SelectedOverwrite: windowObj.RelativityImport.koModel.SelectedOverwrite()
		};

		return model;
	};

	var validationCheck = function (self) {
		var results = windowObj.RelativityImport.koErrors();
		console.log(results);
		if (results.length > 0) {
			root.frameMessaging().dFrame.IP.message.error.raise("Resolve all errors before proceeding");
			$('.import-validation-error').append(windowObj.RelativityImport.koErrors.showAllMessages());

		} else {
			var current = currentSettingsFromUi();
			var stringified = JSON.stringify(current);
			//if we are an image import, make sure other setting get into destination configuration.
			if (current.ImportType !== ImportTypeEnum.Document) {
				$.extend(current, currentImageSettingsFromUi());
				
				var fullModel = windowObj.RelativityImport.FullIPModel;

				var destinationConfig = JSON.parse(fullModel.destination);
				//Also put these values on the destination config
				fullModel.map = '[]';
				fullModel.SelectedOverwrite = current.SelectedOverwrite;

				$.extend(destinationConfig, currentImageSettingsFromUi());
				fullModel.destination = JSON.stringify(destinationConfig);
				fullModel.sourceConfiguration = JSON.stringify(current);
				self.publish("saveState", stringified);
				self.publish('saveCompleteImage', JSON.stringify(fullModel));
			}
			else {
				self.publish("saveState", stringified);
				self.publish('saveComplete', stringified);
			}

			windowObj.RelativityImport.disablePreviewButton(false);
			IP.frameMessaging().dFrame.IP.message.error.clear();
		}
	};

    windowObj.RelativityImport.GetCurrentUiModel = currentSettingsFromUi;

    //An event raised when the user has clicked the Next or Save button.
    //Leaving the custom settings page and going to field mapping screen.
    message.subscribe('submit', function () {
        //Execute save logic that persists the root.
        validationCheck(this);
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
    message.subscribe('loadFullState', function (fullModel) {

    	//look at fullModel.artifactTypeID to get destination object
    	var isRdoImport = fullModel.artifactTypeID !== fullModel.DefaultRdoTypeId;
    	windowObj.RelativityImport.GetImportTypes(isRdoImport);

    	var model = fullModel.sourceConfiguration;
    	windowObj.RelativityImport.FullIPModel = fullModel;
        //closing preview btn if the user opens the btn and then goes back to step2 from step 3
        var $el = currentSettingsFromUi();
        if ($el.ImportType === ImportTypeEnum.Document) { windowObj.RelativityImport.closePreviewBtn(); };

        if (!!model) {
            windowObj.RelativityImport.GetCachedUiModel = JSON.parse(model);
        };

        // adding horizontal scroll
        windowObj.RelativityImport.UI.addSiteCss();

        if (windowObj.parent.$(windowObj.RelativityImport.UI.idSelector(windowObj.RelativityImport.UI.Elements.CUSTOM_BUTTON)).length < 1) {
            windowObj.RelativityImport.UI.initCustomDropdown();
        };

        //TODO: Populate UI with values from model object
    });

})(this, IP, ko);