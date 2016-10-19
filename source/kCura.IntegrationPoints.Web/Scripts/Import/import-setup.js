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
    windowObj.RelativityImport.UI.idSelector = idSelector;

    var assignDropdownHandler = function() {
        var content = windowObj.parent.$(idSelector(BUTTON_UL));
        content.slideToggle();
        var btn = windowObj.parent.$(idSelector(CUSTOM_BUTTON));
        btn.click(function() {
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

    var populateFileColumnHeaders = function () {
        var baseUrlCache = root.utils.getBaseURL();
        $.ajax({
            url: baseUrlCache + "/api/ImportProviderDocument/LoadFileHeaders",
            type: 'POST',
            async: false,
            data: { '': JSON.stringify(windowObj.RelativityImport.GetCurrentUiModel()) },
            success: function (data) {
                console.log('ajax success');
                console.log(data);
                windowObj.RelativityImport.koModel.setPopulateFileColumnHeaders(data);
            },
            error: function (error) {
                console.log('ajax FAIL');
                console.log(error);
            }
        });
    };

    var updateHeaders = function() {
        if (!isEmpty(windowObj.RelativityImport.koModel.Fileshare())) {
            populateFileColumnHeaders();
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
        source.append('<button class="button generic positive" id="' + CUSTOM_BUTTON + '"><i class="icon-chevron-down" style="float: right;"></i>Preview File</button>');

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

    windowObj.RelativityImport.UI.addSiteCss = function() {
        var windowPar = windowObj.parent;
        windowPar.$(idSelector(CONFIGURATION_FRAME)).css({ "min-width": '900px' });
    };

    windowObj.RelativityImport.enableLocation = function (en) {
        var $el = $("#location-select");
        $el.toggleClass('location-disabled', !en);
        $el.children().each(function (i, e) {
            $(e).toggleClass('location-disabled', !en);
        });
    };

    windowObj.RelativityImport.locationSelector = new LocationJSTreeSelector();

    //Work starts here

    //pass in the selectFilesOnly optional parameter so that location-jstree-selector will only allow us to select files
    windowObj.RelativityImport.locationSelector.init(windowObj.RelativityImport.koModel.Fileshare(), [], {
        onNodeSelectedEventHandler: function (node) { windowObj.RelativityImport.koModel.Fileshare(node.id) },
        selectFilesOnly: true
    });

    windowObj.RelativityImport.enableLocation(false);

    $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure"), function (data) {
        windowObj.RelativityImport.koModel.ProcessingSourceLocationList(data);
        windowObj.RelativityImport.koModel.ProcessingSourceLocation(windowObj.RelativityImport.koModel.ProcessingSourceLocationArtifactId);

        $("#processingSources").change(function (c, item) {
            var artifacId = $("#processingSources option:selected").val();
            var choiceName = $("#processingSources option:selected").text();
            $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure", artifacId) + '?includeFiles=true')
                .then(function (result) {
                    windowObj.RelativityImport.locationSelector.reload(result);
                    windowObj.RelativityImport.enableLocation(true);
                })
                .fail(function (error) {
                    root.message.error.raise("No attributes were returned from the source provider.");
                });
        });
    });

    $.ajax({
        url: root.utils.getBaseURL() + "/api/ImportProviderDocument/GetAsciiDelimiters",
        type: 'GET',
        contentType: "application/json",
        async: false,
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