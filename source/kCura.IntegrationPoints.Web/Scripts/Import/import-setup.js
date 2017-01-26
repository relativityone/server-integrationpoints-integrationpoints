﻿'use strict';
(function (windowObj, root) {
	windowObj.RelativityImport.UI = {};

	//Setup UI

	//Element names
	var CONFIGURATION_FRAME = 'configurationFrame';
	var FILE_ENCODING_COLUMN_COLUMN_DD = 'import-column';
	var FILE_ENCODING_COLUMN_QUOTE_DD = 'import-quote';
	var FILE_ENCODING_COLUMN_NEWLINE_DD = 'import-newline';
	var FILE_ENCODING_COLUMN_MULTIVALUE_DD = 'import-multiValue';
	var FILE_ENCODING_COLUMN_NESTEDVALUE_DD = 'import-nestedValue';
	var FILE_ENCODING_DATA_SELECTOR = 'dataFileEncodingSelector';
	var JSTREE_HOLDER_DIV = 'jstree-holder-div';
	var PROCESSING_SOURCE_DROP_DOWN = 'processingSources';
	var BODY_CONTAINER = 'bodyContainer';
	var MODAL_OVERLAY = 'ui-widget-overlay';
	var MODAL_GRPHIC = 'import-load';

	var workspaceId = ("/" + windowObj.RelativityImport.WorkspaceId);
	var windowPar = windowObj.parent;
	var windowTop = windowObj.top;
	var baseUrlCache = root.utils.getBaseURL();

	var idSelector = function (name) { return '#' + name; }
	var classSelector = function (name) { return '.' + name; }
	windowObj.RelativityImport.UI.idSelector = idSelector;

	/*todo change column back to 19*/
	var assignAsciiDropDownDefault = function (array) {
		windowObj.RelativityImport.koModel.setSelectedColumnAsciiDelimiters(array[43].asciiID);
		windowObj.RelativityImport.koModel.selectedQuoteAsciiDelimiter(array[253].asciiID);
		windowObj.RelativityImport.koModel.selectedNewLineAsciiDelimiter(array[173].asciiID);
		windowObj.RelativityImport.koModel.selectedMultiLineAsciiDelimiter(array[58].asciiID);
		windowObj.RelativityImport.koModel.selectedNestedValueAsciiDelimiter(array[91].asciiID);
	};

	var updateHeaders = function () {
		if (!isEmpty(windowObj.RelativityImport.koModel.Fileshare())) {
			$.ajax({
				url: baseUrlCache + workspaceId + "/api/ImportProviderDocument/LoadFileHeaders",
				type: 'POST',
				data: { '': JSON.stringify(windowObj.parent.RelativityImport.GetCurrentUiModel()) },
				success: function (data) {
					windowObj.RelativityImport.koModel.setPopulateFileColumnHeaders(data);
				},
				error: function (error) {
					console.log(error);
				}
			});
		};
	};

	var isEmpty = function (str) {
		return (!str || 0 === str.length);
	};

	var fileHeaderColumn = $(idSelector(FILE_ENCODING_COLUMN_COLUMN_DD));
	var fileHeaderQuote = $(idSelector(FILE_ENCODING_COLUMN_QUOTE_DD));
	var fileHeaderNewLine = $(idSelector(FILE_ENCODING_COLUMN_NEWLINE_DD));
	var fileHeaderMultiValue = $(idSelector(FILE_ENCODING_COLUMN_MULTIVALUE_DD));
	var fileHeaderNestedValue = $(idSelector(FILE_ENCODING_COLUMN_NESTEDVALUE_DD));
	var dataEncoding = $(idSelector(FILE_ENCODING_DATA_SELECTOR));

	dataEncoding.change(function () {
		updateHeaders();
	});

	fileHeaderColumn.change(function () {
		updateHeaders();
	});
	fileHeaderQuote.change(function () {
		updateHeaders();
	});
	fileHeaderNewLine.change(function () {
		updateHeaders();
	});
	fileHeaderMultiValue.change(function () {
		updateHeaders();
	});
	fileHeaderNestedValue.change(function () {
		updateHeaders();
	});

	windowObj.RelativityImport.closePreviewBtn = function () {
		windowObj.parent.$(idSelector(BUTTON_UL)).hide();
	};

	//Convert between the format that it needs to be in when saved and the display format in the dropdown
	var convertToDisplayText = function (overwriteMode) {
		if (overwriteMode === 'AppendOnly') {
			return 'Append Only';
		} else if (overwriteMode === 'OverlayOnly') {
			return 'Overlay Only';
		} else if (overwriteMode === 'AppendOverlay') {
			return 'Append/Overlay';
		}
	}

	var populateCachedState = function () {
		//Parent
		var lineNumber = windowObj.RelativityImport.GetCachedUiModel.LineNumber;
		var importType = windowObj.RelativityImport.GetCachedUiModel.ImportType;
		var processingSourceLocationStructure = windowObj.RelativityImport.GetCachedUiModel.LoadFile;
		var artifactId = windowObj.RelativityImport.GetCachedUiModel.ProcessingSourceLocation;

		var ImportTypeEnum = windowObj.RelativityImport.ImportTypeEnum;

		if (importType === ImportTypeEnum.Document) {
			//Document
			var encodingType = windowObj.RelativityImport.GetCachedUiModel.EncodingType;
			var asciiColumn = windowObj.RelativityImport.GetCachedUiModel.AsciiColumn;
			var asciiQuote = windowObj.RelativityImport.GetCachedUiModel.AsciiQuote;
			var asciiNewLine = windowObj.RelativityImport.GetCachedUiModel.AsciiNewLine;
			var asciiMultiLine = windowObj.RelativityImport.GetCachedUiModel.AsciiMultiLine;
			var asciiNestedValue = windowObj.RelativityImport.GetCachedUiModel.AsciiNestedValue;
			var hasColumnName = windowObj.RelativityImport.GetCachedUiModel.HasColumnName;

			//Document repopulate model
			windowObj.RelativityImport.koModel.ProcessingSourceLocation(artifactId);
			windowObj.RelativityImport.koModel.Fileshare(processingSourceLocationStructure);
			windowObj.RelativityImport.koModel.selectedImportType(importType);
			windowObj.RelativityImport.koModel.startLine(lineNumber);
			windowObj.RelativityImport.koModel.DataFileEncodingType(encodingType);
			windowObj.RelativityImport.koModel.selectedColumnAsciiDelimiter(asciiColumn);
			windowObj.RelativityImport.koModel.selectedQuoteAsciiDelimiter(asciiQuote);
			windowObj.RelativityImport.koModel.selectedNewLineAsciiDelimiter(asciiNewLine);
			windowObj.RelativityImport.koModel.selectedMultiLineAsciiDelimiter(asciiMultiLine);
			windowObj.RelativityImport.koModel.selectedNestedValueAsciiDelimiter(asciiNestedValue);
			windowObj.RelativityImport.koModel.fileContainsColumn(hasColumnName);
		} else {

			//ImageProduction
			var autoNumberImages = windowObj.RelativityImport.GetCachedUiModel.AutoNumberImages;
			var selectedOverwrite = windowObj.RelativityImport.GetCachedUiModel.ImportOverwriteMode;
			var extractedTextFieldContainsFilePath = windowObj.RelativityImport.GetCachedUiModel.ExtractedTextFieldContainsFilePath;
			var overlayIdentifier = windowObj.RelativityImport.GetCachedUiModel.OverlayIdentifier;
			var extractedTextFileEncoding = windowObj.RelativityImport.GetCachedUiModel.ExtractedTextFileEncoding;
			var copyFilesToDocumentRepo = windowObj.RelativityImport.GetCachedUiModel.CopyFilesToDocumentRepository;
			var selectedCaseFileRepoPath = windowObj.RelativityImport.GetCachedUiModel.SelectedCaseFileRepoPath;
			var productionSet = windowObj.RelativityImport.GetCachedUiModel.ProductionArtifactId;

			//ImageProduction repopulate model
			windowObj.RelativityImport.koModel.ProcessingSourceLocation(artifactId);
			windowObj.RelativityImport.koModel.Fileshare(processingSourceLocationStructure);
			windowObj.RelativityImport.koModel.startLine(lineNumber);
			windowObj.RelativityImport.koModel.selectedImportType(importType);
			windowObj.RelativityImport.koModel.autoNumberPages(autoNumberImages);
			windowObj.RelativityImport.koModel.SelectedOverwrite(convertToDisplayText(selectedOverwrite));
			windowObj.RelativityImport.koModel.selectedOverlayIdentifier(overlayIdentifier);
			windowObj.RelativityImport.koModel.ExtractedTextFieldContainsFilePath(extractedTextFieldContainsFilePath);
			windowObj.RelativityImport.koModel.ExtractedTextFileEncoding(extractedTextFileEncoding);
			windowObj.RelativityImport.koModel.copyFilesToDocumentRepository(copyFilesToDocumentRepo);
			windowObj.RelativityImport.koModel.selectedRepo(selectedCaseFileRepoPath);
			windowObj.RelativityImport.koModel.selectedProductionSets(productionSet);
		}
	};

	windowObj.RelativityImport.checkValueForImportType = function () {

		var chosenType = windowObj.RelativityImport.koModel.selectedImportType();
		if (chosenType === windowObj.RelativityImport.ImportTypeEnum.Document) {
			windowObj.parent.RelativityImport.PreviewOptions.disablePreviewButton(true);
		}
		else {
			windowObj.parent.RelativityImport.PreviewOptions.disablePreviewButton(false);
		};
	};

	windowObj.RelativityImport.koModel.selectedImportType.subscribe(function () {
		windowObj.RelativityImport.checkValueForImportType();
	});

	windowObj.parent.RelativityImport.PreviewOptions.UI.addSiteCss = function () {
		windowPar.$(idSelector(CONFIGURATION_FRAME)).css({ "min-width": '900px' });
		$(idSelector(JSTREE_HOLDER_DIV)).css({ 'min-height': '50%' });
		$(idSelector(JSTREE_HOLDER_DIV)).height('auto');
		$('body').addClass('import-body-style');
	};

	windowObj.RelativityImport.enableLocation = function (en) {
		var $el = $("#location-select");
		$el.toggleClass('location-disabled', !en);
		$el.children().each(function (i, e) {
			$(e).toggleClass('location-disabled', !en);
		});
		if (en) {
			$('#loadData').show();
		};
	};

	windowObj.RelativityImport.enableLoadModal = function (bool) {
		var $el = windowObj.parent.$(idSelector(BODY_CONTAINER));
		var overlay =
			"<div class='" + MODAL_OVERLAY + "'></div>";

		if (bool) {
			$el.after(overlay); windowObj.parent.$(classSelector(MODAL_OVERLAY)).after("<div class='" + MODAL_GRPHIC + "'></div>");
		} else {
			windowObj.parent.$(classSelector(MODAL_OVERLAY)).remove();
			windowObj.parent.$(classSelector(MODAL_GRPHIC)).remove();
		};
	};

	windowObj.RelativityImport.locationSelector = new LocationJSTreeSelector();

	//pass in the selectFilesOnly optional parameter so that location-jstree-selector will only allow us to select files
	windowObj.RelativityImport.locationSelector.init(windowObj.RelativityImport.koModel.Fileshare(), [], {
		onNodeSelectedEventHandler: function (node) { windowObj.RelativityImport.koModel.Fileshare(node.id) },
		selectFilesOnly: true
	});

	//Create a function so that this can be triggered when we get the full model and can check if the destination object is an RDO or not
	windowObj.RelativityImport.GetImportTypes = function (isRdo) {
		var getImportTypesUrl = root.utils.generateWebAPIURL("/ImportProviderDocument/GetImportTypes") + '?isRdo=' + isRdo;
		$.getJSON(getImportTypesUrl, function (data) {
			windowObj.RelativityImport.koModel.importTypes(data);
			windowObj.RelativityImport.koModel.setSelectedImportType(windowObj.RelativityImport.ImportTypeEnum.Document);
		});
	};

	windowObj.RelativityImport.enableLocation(false);

	windowObj.RelativityImport.getDirectories = function () {
		var reloadTree = function (params, onSuccess, onFail) {
			var isRoot = params.id === '#';
			var path = params.id;
			if (isRoot) {
				path = windowObj.RelativityImport.koModel.ProcessingSourceLocationPath;
			}
			$.ajax({
				type: "post",
				contentType: "application/x-www-form-urlencoded; charset=UTF-8",
				url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationSubItems", isRoot) + '?includeFiles=true',
				data: { '': path }
			}).then(function (result) {
				onSuccess(result);
				windowObj.RelativityImport.enableLocation(true);
			}).fail(function (error) {
				onFail(error);
				IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve directories and subfolders info. Please contact system administrator");
			});
		};
		windowObj.RelativityImport.locationSelector.reloadWithRoot(reloadTree);
	};

	windowObj.RelativityImport.enableLoadModal(true);
	$.ajax({
		type: "get",
		url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocations"),
		data: {
			sourceWorkspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
		}
	}).done(function (data) {
		windowObj.RelativityImport.enableLoadModal(false);
		windowObj.RelativityImport.koModel.ProcessingSourceLocationList(data);
		if (windowObj.RelativityImport.GetCachedUiModel) {
			populateCachedState();
			windowObj.RelativityImport.checkValueForImportType();
		};

		$(idSelector(PROCESSING_SOURCE_DROP_DOWN)).change(function (c, item) {
			windowObj.RelativityImport.koModel.ProcessingSourceLocationPath = windowObj.RelativityImport.koModel.GetSelectedProcessingSourceLocationPath(windowObj.RelativityImport.koModel.ProcessingSourceLocation()).location;
			windowObj.RelativityImport.getDirectories();
			windowObj.RelativityImport.enableLocation(true);
		});
	}).fail(function () {
		windowObj.RelativityImport.enableLoadModal(false);
		IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve processing source locations. Please contact system administrator");
	});

	$.ajax({
		url: root.utils.getBaseURL() + workspaceId + "/api/ImportProviderDocument/GetAsciiDelimiters",
		type: 'GET',
		contentType: "application/json",
		success: function (data) {
			var array = [];
			$.each(data, function (index, value) {
				array.push({ "asciiID": (index + 1), "asciiText": value })
			}
			);
			windowObj.RelativityImport.koModel.setAsciiDelimiters(array);
			assignAsciiDropDownDefault(array);
		}
	});

	$.ajax({
		url: root.utils.generateWebAPIURL("Production/GetProductionsForImport"),
		type: "GET",
		data: {
			sourceWorkspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
		},
		success: function(data) {
			windowObj.RelativityImport.koModel.productionSets(data);
		}
	});

	$.ajax({
		type: 'GET',
		url: IP.utils.generateWebAPIURL('ImportProviderImage/GetOverlayIdentifierFields'),
		data: {
			workspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
		},
		success: function (data) {
			windowObj.RelativityImport.koModel.overlayIdentifiers(data);
		}
	});

	windowObj.RelativityImport.setDefaultFileRepo = function () {
		$.ajax({
			type: 'GET',
			url: IP.utils.generateWebAPIURL('ImportProviderImage/GetDefaultFileRepo'),
			data: {
				workspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
			},
			success: function (data) {
				windowObj.RelativityImport.koModel.selectedRepo(data);
			}
		})
	};

	$.ajax({
		type: 'GET',
		url: IP.utils.generateWebAPIURL('ImportProviderImage/GetFileRepositories'),
		data: {
			workspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
		},
		success: function (data) {
			windowObj.RelativityImport.koModel.fileRepositories(data);
			windowObj.RelativityImport.setDefaultFileRepo();
		}
	});

	$('#btnDefaultFileRepo').click(function () {
		windowObj.RelativityImport.setDefaultFileRepo();
	});
})(this, IP);