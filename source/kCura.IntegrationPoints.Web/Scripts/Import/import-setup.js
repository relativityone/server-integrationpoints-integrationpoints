'use strict';
(function (windowObj, root) {
    windowObj.RelativityImport.UI = {};

    //Setup UI

    //Element names
    var PROGRESS_BUTTONS = 'progressButtons';
    var CUSTOM_BUTTON = 'importCustomButton';
    var BUTTON_UL = 'importCustomButtonUl';
    var PREVIEW_FILE_LI = 'dd-previewFile';
    var PREVIEW_ERROR_LI = 'dd-previewErrors';
    var PREVIEW_CHOICE_LI = 'dd-previewChoiceFolder';
    var FILE_ENCODING_COLUMN_COLUMN_DD = 'import-column';
    var FILE_ENCODING_COLUMN_QUOTE_DD = 'import-quote';
    var FILE_ENCODING_COLUMN_NEWLINE_DD = 'import-newline';
    var FILE_ENCODING_COLUMN_MULTIVALUE_DD = 'import-multiValue';
    var FILE_ENCODING_COLUMN_NESTEDVALUE_DD = 'import-nestedValue';
    var FILE_ENCODING_DATA_SELECTOR = 'dataFileEncodingSelector';
    var CONFIGURATION_FRAME = 'configurationFrame';
    var JSTREE_HOLDER_DIV = 'jstree-holder-div';
    var PROCESSING_SOURCE_DROP_DOWN = 'processingSources';
    var BODY_CONTAINER = 'bodyContainer';
    var MODAL_OVERLAY = 'ui-widget-overlay';
    var MODAL_GRPHIC = 'import-load';

    var workspaceId = ("/" + windowObj.RelativityImport.WorkspaceId);

    windowObj.RelativityImport.UI.Elements = {
        PROGRESS_BUTTONS: PROGRESS_BUTTONS,
        CUSTOM_BUTTON: CUSTOM_BUTTON,
        BUTTON_UL: BUTTON_UL,
        PREVIEW_FILE_LI: PREVIEW_FILE_LI,
        PREVIEW_ERROR_LI: PREVIEW_ERROR_LI,
        PREVIEW_CHOICE_LI: PREVIEW_CHOICE_LI,
        CONFIGURATION_FRAME: CONFIGURATION_FRAME
    }

    var idSelector = function (name) { return '#' + name; }
    var classSelector = function (name) { return '.' + name; }
    windowObj.RelativityImport.UI.idSelector = idSelector;

    var assignDropdownHandler = function () {
        var content = windowObj.parent.$(idSelector(BUTTON_UL));
        content.slideToggle();
        var btn = windowObj.parent.$(idSelector(CUSTOM_BUTTON));
        btn.click(function () {
            content.slideToggle();
        });
    };
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
            var baseUrlCache = root.utils.getBaseURL();
            $.ajax({
                url: baseUrlCache + workspaceId + "/api/ImportProviderDocument/LoadFileHeaders",
                type: 'POST',
                data: { '': JSON.stringify(windowObj.RelativityImport.GetCurrentUiModel()) },
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

    var populateCachedState = function () {
        var artifactId = windowObj.RelativityImport.GetCachedUiModel.ProcessingSourceLocation;
        var processingSourceLocationStructure = windowObj.RelativityImport.GetCachedUiModel.LoadFile;
        var importType = windowObj.RelativityImport.GetCachedUiModel.ImportType;
        var lineNumber = windowObj.RelativityImport.GetCachedUiModel.LineNumber;
        var encodingType = windowObj.RelativityImport.GetCachedUiModel.EncodingType;
        var asciiColumn = windowObj.RelativityImport.GetCachedUiModel.AsciiColumn;
        var asciiQuote = windowObj.RelativityImport.GetCachedUiModel.AsciiQuote;
        var asciiNewLine = windowObj.RelativityImport.GetCachedUiModel.AsciiNewLine;
        var asciiMultiLine = windowObj.RelativityImport.GetCachedUiModel.AsciiMultiLine;
        var asciiNestedValue = windowObj.RelativityImport.GetCachedUiModel.AsciiNestedValue;
        var hasColumnName = windowObj.RelativityImport.GetCachedUiModel.HasColumnName;

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
    };

    windowObj.RelativityImport.checkValueForImportType = function () {
        var chosenType = windowObj.RelativityImport.koModel.selectedImportType();
        if (chosenType === 'image') {
            windowObj.RelativityImport.disablePreviewButton(false);
        } else if (chosenType === 'production') {
            windowObj.RelativityImport.disablePreviewButton(false);
        } else {
            windowObj.RelativityImport.disablePreviewButton(true);
        };
    };

    windowObj.RelativityImport.koModel.selectedImportType.subscribe(function () {
        windowObj.RelativityImport.checkValueForImportType();
    });

    windowObj.RelativityImport.disablePreviewButton = function (bool) {
        var windowPar = windowObj.parent;
        windowPar.$(idSelector(CUSTOM_BUTTON)).prop('disabled', bool);
    };

    var assignDropdownItemHandlers = function () {
        var windowPar = windowObj.parent;
        var windowTop = windowObj.top;
        var baseUrlCache = root.utils.getBaseURL();

        var preFile = windowPar.$(idSelector(PREVIEW_FILE_LI));
        var preError = windowPar.$(idSelector(PREVIEW_ERROR_LI));
        var preChoice = windowPar.$(idSelector(PREVIEW_CHOICE_LI));

        var openPreviewWindow = function (previewType) {
            windowPar.RelativityImportPreviewSettings = {};
            windowPar.RelativityImportPreviewSettings = windowObj.RelativityImport.GetCurrentUiModel();
            $.extend(windowPar.RelativityImportPreviewSettings, { PreviewType: previewType });
            windowPar.$(idSelector(BUTTON_UL)).slideUp();

            windowPar.open(baseUrlCache + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");
            return false;
        };

        preFile.click(function () {
            return openPreviewWindow('file');
        });
        preError.click(function () {
            return openPreviewWindow('errors');
        });
        preChoice.click(function () {
            console.log("Preview choice has been selected");
        });
    }

    //Add dropdown to DOM, init handlers for the dropdown and each of the items added
    windowObj.RelativityImport.UI.initCustomDropdown = function () {
        var options = {};
        options[PREVIEW_FILE_LI] = "Preview File";
        options[PREVIEW_ERROR_LI] = "Preview Errors";
        options[PREVIEW_CHOICE_LI] = "Preview Choices & Folders";

        var source = windowObj.parent.$(idSelector(PROGRESS_BUTTONS));
        source.append('<button class="button generic positive" id="' + CUSTOM_BUTTON + '" disabled><i class="icon-chevron-down" style="float: right;"></i>Preview File</button>');

        var previewFile = windowObj.parent.$(idSelector(CUSTOM_BUTTON));
        previewFile.append('<ul id="' + BUTTON_UL + '"></ul>');
        var dropdown = windowObj.parent.$(idSelector(BUTTON_UL));

        $.each(options, function (val, text) {
            dropdown.append($('<li class="importPreviewDropdownItem" id=' + val + '></li>').html(text));
        });
        assignDropdownHandler();
        assignDropdownItemHandlers();
    };

    windowObj.RelativityImport.UI.removeCustomDropdown = function () {
        var windowPar = windowObj.parent;
        windowPar.$(idSelector(CUSTOM_BUTTON)).remove();
    };

    windowObj.RelativityImport.UI.addSiteCss = function () {
        var windowPar = windowObj.parent;
        windowPar.$(idSelector(CONFIGURATION_FRAME)).css({ "min-width": '900px' });
        $(idSelector(JSTREE_HOLDER_DIV)).css({ 'min-height': '50%' });
        $(idSelector(JSTREE_HOLDER_DIV)).height('auto');
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
                IP.frameMessaging().dFrame.IP.message.error.raise("Failed to load Directories for the selected Source Location.");
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
        IP.frameMessaging().dFrame.IP.message.error.raise("Failed to load Processing Source Locations.");
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
})(this, IP);