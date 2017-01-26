'use strict';
(function (windowObj, root, ko) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();
	var ImportTypeEnum = windowObj.RelativityImport.ImportTypeEnum;
	var currentImportType = windowObj.RelativityImport.koModel.selectedImportType();
	// used for all models
	var currentLoadFileSettings = function () {
		var model = {
			WorkspaceId: windowObj.RelativityImport.WorkspaceId,
			ImportType: windowObj.RelativityImport.koModel.selectedImportType(),
			ProcessingSourceLocation: windowObj.RelativityImport.koModel.ProcessingSourceLocation(),
			LoadFile: windowObj.RelativityImport.koModel.Fileshare(),
			LineNumber: windowObj.RelativityImport.koModel.startLine()
		}
		return model;
	}

	var documentModel = function () {
		var loadFileModel = currentLoadFileSettings();
		var model = {

			WorkspaceId: loadFileModel.WorkspaceId,
			ImportType: loadFileModel.ImportType,
			ProcessingSourceLocation: loadFileModel.ProcessingSourceLocation,
			LoadFile: loadFileModel.LoadFile,
			LineNumber: loadFileModel.LineNumber,
			HasColumnName: windowObj.RelativityImport.koModel.fileContainsColumn(),
			EncodingType: windowObj.RelativityImport.koModel.DataFileEncodingType(),
			AsciiColumn: windowObj.RelativityImport.koModel.selectedColumnAsciiDelimiter(),
			AsciiQuote: windowObj.RelativityImport.koModel.selectedQuoteAsciiDelimiter(),
			AsciiNewLine: windowObj.RelativityImport.koModel.selectedNewLineAsciiDelimiter(),
			AsciiMultiLine: windowObj.RelativityImport.koModel.selectedMultiLineAsciiDelimiter(),
			AsciiNestedValue: windowObj.RelativityImport.koModel.selectedNestedValueAsciiDelimiter(),
		};
		return model;
	};

	var imageProductionModel = function () {
		var loadFileModel = currentLoadFileSettings();
		var importType = windowObj.RelativityImport.koModel.selectedImportType();
		var imageImport = true;
		var forProduction = false;
		if (importType === ImportTypeEnum.Production) {
			forProduction = true;
		}

		//a valid encoding is needed to instantiate the ImportSettings object
		var etFileEncoding = "UTF-8";
		if(windowObj.RelativityImport.koModel.ExtractedTextFieldContainsFilePath() === 'true') {
			etFileEncoding = windowObj.RelativityImport.koModel.ExtractedTextFileEncoding();
		}

		var model = {
			ImageImport: imageImport,
			ForProduction: forProduction,
			ProductionArtifactId: windowObj.RelativityImport.koModel.selectedProductionSets(),
			AutoNumberImages: windowObj.RelativityImport.koModel.autoNumberPages(),
			ImportOverwriteMode: ko.toJS(windowObj.RelativityImport.koModel.SelectedOverwrite).replace('/', '').replace(' ', ''),
			IdentityFieldId: windowObj.RelativityImport.koModel.selectedOverlayIdentifier(),
			ExtractedTextFieldContainsFilePath: windowObj.RelativityImport.koModel.ExtractedTextFieldContainsFilePath(),
			ExtractedTextFileEncoding: etFileEncoding,
			CopyFilesToDocumentRepository: windowObj.RelativityImport.koModel.copyFilesToDocumentRepository(),
			SelectedCaseFileRepoPath: windowObj.RelativityImport.koModel.selectedRepo()
		};

		if (model.CopyFilesToDocumentRepository === 'true') {
			model.ImportNativeFileCopyMode = 'CopyFiles';
		}
		$.extend(model, currentLoadFileSettings());

		return model;
	};

	var validationCheck = function (self) {
		var results = windowObj.RelativityImport.koErrors();
		console.log(results);
		if (results.length > 0) {
			root.frameMessaging().dFrame.IP.message.error.raise("Resolve all errors before proceeding");
			$('.import-validation-error').append(windowObj.RelativityImport.koErrors.showAllMessages());

		} else {
			var current = documentModel();
			var stringified = JSON.stringify(current);
			//if we are an image import, make sure other setting get into destination configuration.

			if (currentLoadFileSettings().ImportType !== ImportTypeEnum.Document) {
				$.extend(current, imageProductionModel());

				var fullModel = windowObj.RelativityImport.FullIPModel;

				var destinationConfig = JSON.parse(fullModel.destination);
				//Also put these values on the destination config
				fullModel.map = '[]';
				fullModel.SelectedOverwrite = windowObj.RelativityImport.koModel.SelectedOverwrite();

				$.extend(destinationConfig, imageProductionModel());
				fullModel.destination = JSON.stringify(destinationConfig);
				fullModel.sourceConfiguration = JSON.stringify(current);
				self.publish("saveState", stringified);
				self.publish('saveCompleteImage', JSON.stringify(fullModel));
			}
			else {
				self.publish("saveState", stringified);
				self.publish('saveComplete', stringified);
			}

			windowObj.parent.RelativityImport.PreviewOptions.disablePreviewButton(false);
			IP.frameMessaging().dFrame.IP.message.error.clear();
		}
	};

	windowObj.parent.RelativityImport.GetCurrentUiModel = documentModel;

    //An event raised when the user has clicked the Next or Save button.
    //Leaving the custom settings page and going to field mapping screen.
    message.subscribe('submit', function () {
        //Execute save logic that persists the root.
    	validationCheck(this);
    	windowObj.parent.RelativityImport.CurrentUiModel = documentModel();
    });

    //An event raised when a user clicks the Back button.
    //Leaving the custom settings page and going back to the first RIP screen
    message.subscribe('back', function () {
    	//Execute save logic that persists the root.
	    var current;
    	var importType = windowObj.RelativityImport.koModel.selectedImportType();
    	if (importType === ImportTypeEnum.Document) {
    		current = documentModel();
    	} else {
    		current = imageProductionModel();
    	};
        var stringified = JSON.stringify(current);
        this.publish("saveState", stringified);

		//Revert CSS back to normal
        windowObj.parent.RelativityImport.PreviewOptions.UI.UndoRepositionProgressButtons();

        windowObj.parent.RelativityImport.PreviewOptions.UI.removePreviewButton();
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
    	var $el = currentLoadFileSettings();
        if ($el.ImportType === ImportTypeEnum.Document) { windowObj.parent.RelativityImport.PreviewOptions.closePreviewBtn(); };

        if (!!model) {
            windowObj.RelativityImport.GetCachedUiModel = JSON.parse(model);
        };

        //Adding horizontal scroll to entire child div
        windowObj.parent.RelativityImport.PreviewOptions.UI.addSiteCss();

        if (windowObj.parent.$(windowObj.RelativityImport.UI.idSelector(windowObj.parent.RelativityImport.PreviewOptions.UI.Elements.CUSTOM_BUTTON)).length === 0) {

			//Create custom btn - Preview Options
        	windowObj.parent.RelativityImport.PreviewOptions.UI.initCustomDropdown();

        	//Overwritting the CSS for the progressbuttons for Preview Options
        	windowObj.parent.RelativityImport.PreviewOptions.UI.repositionProgressButtons();

        	//Disable preview options btn on step2
        	windowObj.parent.RelativityImport.PreviewOptions.disablePreviewButton(true);
        };
    });

})(this, IP, ko);