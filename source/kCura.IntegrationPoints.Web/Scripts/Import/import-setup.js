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

    windowObj.RelativityImport.UI.Elements = {
        PROGRESS_BUTTONS: PROGRESS_BUTTONS,
        CUSTOM_BUTTON: CUSTOM_BUTTON,
        BUTTON_UL: BUTTON_UL,
        PREVIEW_FILE_LI: PREVIEW_FILE_LI,
        PREVIEW_ERROR_LI: PREVIEW_ERROR_LI,
        PREVIEW_CHOICE_LI: PREVIEW_CHOICE_LI
    }

    var idSelector = function (name) { return '#' + name; }
    windowObj.RelativityImport.UI.idSelector = idSelector;

    var assignDropdownHandler = function () {
        var content = windowObj.parent.$(idSelector(BUTTON_UL));
        content.slideToggle();
        var btn = windowObj.parent.$(idSelector(CUSTOM_BUTTON));
        btn.click(function () {
            content.slideToggle();
        });
    }

    var assignDropdownItemHandlers = function () {
        var preFile = windowObj.parent.$(idSelector(PREVIEW_FILE_LI));
        var preError = windowObj.parent.$(idSelector(PREVIEW_ERROR_LI));
        var preChoice = windowObj.parent.$(idSelector(PREVIEW_CHOICE_LI));
        preFile.click(function () {
            windowObj.RelativityImport.PreviewSettings = windowObj.RelativityImport.GetCurrentUiModel();
            $.extend(windowObj.RelativityImport.PreviewSettings, { PreviewType: 'file', WorkspaceId: root.utils.getParameterByName('AppID', window.top) });
            windowObj.parent.$(idSelector(BUTTON_UL)).slideUp();

            window.open(root.utils.getBaseURL() + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");

            return false;
        });
        preError.click(function () {
            windowObj.RelativityImport.PreviewSettings = windowObj.RelativityImport.GetCurrentUiModel();
            $.extend(windowObj.RelativityImport.PreviewSettings, { PreviewType: 'errors', WorkspaceId: root.utils.getParameterByName('AppID', window.top) });
            windowObj.parent.$(idSelector(BUTTON_UL)).slideUp();

            window.open(root.utils.getBaseURL() + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");

            return false;
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
            dropdown.append($('<li id=' + val + '></li>').html(text));
        });
        assignDropdownHandler();
        assignDropdownItemHandlers();
    };

    windowObj.RelativityImport.UI.removeCustomDropdown = function () {
        $(idSelector(CUSTOM_BUTTON)).remove();
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
            $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure", artifacId) + '?includeFiles=1')
                .then(function (result) {
                    windowObj.RelativityImport.locationSelector.reload(result);
                    windowObj.RelativityImport.enableLocation(true);
                })
                .fail(function (error) {
                    root.message.error.raise("No attributes were returned from the source provider.");
                });
        });
    });

    //function AsciiDelimitersModel(data) {
    //    var self = this;

    //    self.asciiID = ko.observable(data.asciiID);
    //};

    function AsciiViewModel() {
        var self = this;
        self.asciiDelimiters = ko.observableArray([]);

        $.ajax({
            url: root.utils.getBaseURL() + "/api/ImportProviderDocument/GetAsciiDelimiters",
            type: 'GET',
            contentType: "application/json",
            async: false,
            success: function (data) {
                var array = [];
                $.each(data, function (index, value) {
                        array.push({"asciiID": index , "asciiText": value })}
                );
                console.log(array[0]);
                //self.asciiDelimiters(array);
            }
        });

    };
    ko.applyBindings(new AsciiViewModel());

    ////TODO: get ascii delimiters populating a dropdown
    //var asciiDelimiter = function () {
    //    $.get(root.utils.getBaseURL() + "/api/ImportProviderDocument/GetAsciiDelimiters")
    //        .then(function (data) {
    //            console.log("GOT ASCII DELIMITERS FROM SERVER");
    //            console.log(data);
    //        })
    //        .fail(function (error) {
    //            console.log("Ascii delimter ajax failed.");
    //            console.log(error);
    //        });
    //};
    //asciiDelimiter();

})(this, IP);