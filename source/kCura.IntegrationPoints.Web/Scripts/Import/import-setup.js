(function (windowObj) {
    windowObj.RelativityImport = {
        UI: {}
    };

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
        console.log('in assignDropdownHandler')
        var content = windowObj.parent.$(idSelector(BUTTON_UL));
        content.slideToggle();
        var btn = windowObj.parent.$(idSelector(CUSTOM_BUTTON));
        btn.click(function () {
            content.slideToggle();
        });
    }

    var assignDropdownItemHandlers = function () {
        console.log('in assignDropdownItemHandlers')
        var preFile = windowObj.parent.$(idSelector(PREVIEW_FILE_LI));
        var preError = windowObj.parent.$(idSelector(PREVIEW_ERROR_LI));
        var preChoice = windowObj.parent.$(idSelector(PREVIEW_CHOICE_LI));
        preFile.on("click", function () {
            console.log("preview file click handler");
            window.open(root.utils.getBaseURL() + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");
            windowObj.ImportSettings = ImportSettingsModel();
            $.extend(windowObj.ImportSettings, { PreviewType: 'file', WorkspaceId: root.utils.getParameterByName('AppID', window.top) });

            windowObj.parent.$(idSelector(PREVIEW_FILE_LI)).close();
            return false;
        });
        preError.click(function () {
            console.log("preview error click handler");
            window.open(root.utils.getBaseURL() + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");
            windowObj.ImportSettings = ImportSettingsModel();
            $.extend(windowObj.ImportSettings, { PreviewType: 'errors', WorkspaceId: root.utils.getParameterByName('AppID', window.top) });

            windowObj.parent.$(idSelector(PREVIEW_ERROR_LI)).close();
            return false;
        });
        preChoice.click(function () {
            console.log("Preview choice has been selected");
        });
    }

    //Add dropdown to DOM, init handlers for the dropdown and each of the items added
    windowObj.RelativityImport.UI.initCustomDropdown = function () {
        console.log('initCustomDropdown');
        var options = {};
        options[PREVIEW_FILE_LI] = "Preview File";
        options[PREVIEW_ERROR_LI] = "Preview Errors";
        options[PREVIEW_CHOICE_LI] = "Preview Choices & Folders";

        var source = windowObj.parent.$(idSelector(PROGRESS_BUTTONS));
        source.append('<button class="button generic positive"id="' + CUSTOM_BUTTON + '"><i class="icon-chevron-down" style="float: right;"></i>Preview File</button>');

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

})(this);