(function (windowObj, root) {
	windowObj.RelativityImport = {};
	windowObj.RelativityImport.PreviewOptions = {};
	windowObj.RelativityImport.PreviewOptions.UI = {};

	var PROGRESS_BUTTONS = 'progressButtons';
	var CUSTOM_BUTTON = 'importCustomButton';
	var BUTTON_UL = 'importCustomButtonUl';
	var PREVIEW_FILE_LI = 'preFile';
	var PREVIEW_ERROR_LI = 'preErrors';
	var PREVIEW_CHOICE_LI = 'preChoice';
	var SHOWN = 'shown';
	var SELECT = 'select';

	var windowPar = windowObj.parent;
	var baseUrlCache = root.utils.getBaseURL();

	windowObj.RelativityImport.PreviewOptions.UI.Elements = {
		PROGRESS_BUTTONS: PROGRESS_BUTTONS,
		CUSTOM_BUTTON: CUSTOM_BUTTON,
		BUTTON_UL: BUTTON_UL,
		PREVIEW_FILE_LI: PREVIEW_FILE_LI,
		PREVIEW_ERROR_LI: PREVIEW_ERROR_LI,
		PREVIEW_CHOICE_LI: PREVIEW_CHOICE_LI,
	}

	//put enum on parent so it can be accessed by both import-setup.js and import preview pop up page
	windowObj.PreviewTypeEnum = {
		File: 0,
		Errors: 1,
		Folders: 2
	};

	var idSelector = function (name) { return '#' + name; }
	var classSelector = function (name) { return '.' + name; }
	windowObj.RelativityImport.PreviewOptions.UI.idSelector = idSelector;

	//Will "close" Preview options btn if open during iframe navigation
	windowObj.RelativityImport.PreviewOptions.closePreviewBtn = function () {
		windowObj.$(classSelector(SELECT)).addClass('collapsed');
	};

	//Remove Preview options btn on back
	windowObj.RelativityImport.PreviewOptions.UI.removePreviewButton = function () {
		windowObj.$(idSelector(CUSTOM_BUTTON)).remove();
	}

	//Click event for Preview Options btn
	var previewOptionsEvent = function () {
		windowObj.$(classSelector(SELECT)).on('click', function () {
			$(this).toggleClass('collapsed');
		});
	};

	//Helper Method for changing the CSS for the preview options btn
	windowObj.RelativityImport.PreviewOptions.disablePreviewButton = function (bool) {
		if (bool) {
			windowObj.$(classSelector(SHOWN)).css({ 'pointer-events': 'none' });
			windowObj.$(classSelector(SHOWN)).addClass('disableDiv');
			windowObj.$(idSelector(CUSTOM_BUTTON)).off("click");
		} else {
			windowObj.$(classSelector(SHOWN)).css({ 'pointer-events': '' });
			windowObj.$(classSelector(SHOWN)).removeClass('disableDiv');
			previewOptionsEvent();
		}
	};

	//Helper Method for the options for the Preview Options Btn
	var assignDropdownItemHandlers = function() {
		var preFile = windowObj.$(idSelector(PREVIEW_FILE_LI));
		var preError = windowObj.$(idSelector(PREVIEW_ERROR_LI));
		var preChoice = windowObj.$(idSelector(PREVIEW_CHOICE_LI));

		var openPreviewWindow = function(previewType) {
			windowObj.RelativityImportPreviewSettings = {};
			windowObj.RelativityImportPreviewSettings = windowObj.RelativityImport.CurrentUiModel;
			$.extend(windowObj.RelativityImportPreviewSettings, { PreviewType: previewType });
			windowObj.$(idSelector(BUTTON_UL)).slideUp();

			windowObj.open(baseUrlCache + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");
			return false;
		};

		preFile.click(function () {
			windowObj.RelativityImport.PreviewOptions.closePreviewBtn();
			return openPreviewWindow(windowObj.PreviewTypeEnum.File);
		});
		preError.click(function () {
			windowObj.RelativityImport.PreviewOptions.closePreviewBtn();
			return openPreviewWindow(windowObj.PreviewTypeEnum.Errors);
		});
		preChoice.click(function () {
			windowObj.RelativityImport.PreviewOptions.closePreviewBtn();
			return openPreviewWindow(windowObj.PreviewTypeEnum.Folders);
		});
	};

	//Helper Method used to detect if the user's web broswer is IE
	windowObj.RelativityImport.PreviewOptions.detectIE = function() {
		var ua = window.navigator.userAgent;

		// IE 10
		// ua = 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)';

		// IE 11
		// ua = 'Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko';

		// Edge 12 (Spartan)
		// ua = 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36 Edge/12.0';

		// Edge 13
		// ua = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586';

		var msie = ua.indexOf('MSIE ');
		if (msie > 0) {
			// IE 10 or older => return version number
			return true;
		}

		var trident = ua.indexOf('Trident/');
		if (trident > 0) {
			// IE 11 => return version number
			var rv = ua.indexOf('rv:');
			return true;
		}

		var edge = ua.indexOf('Edge/');
		if (edge > 0) {
			// Edge (IE 12+) => return version number
			return true;
		}

		// other browser
		return false;
	};

	//Updates the CSS for the Preview Options Btn depending on the broswer
	windowObj.RelativityImport.PreviewOptions.renderCssForDifferentBrowser = function() {
		var browsers = ['chrome', 'IE/Edge', 'firefox'];

		for (var i = 0; i < browsers.length; i++) {
			// initial check to see if broswer is Chrome or Firefox
			if (!!window.chrome) { //chrome or Edge = return true
				if (windowObj.RelativityImport.PreviewOptions.detectIE()) {
					//render Edge CSS
					windowObj.$(idSelector(PROGRESS_BUTTONS)).css({ 'margin-top': '-55px', 'position': 'absolute' });
					break;
				}
				//chrome
				windowObj.$(idSelector(PROGRESS_BUTTONS)).css({ 'margin-top': '30px', 'position': 'absolute' });
				break;
			} else if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
				// render css for FireFox
				windowObj.$(idSelector(PROGRESS_BUTTONS)).css({ 'margin-top': '-55px', 'position': 'absolute' });
				break;
			} else {
				windowObj.$(idSelector(PROGRESS_BUTTONS)).css({ 'margin-top': '-55px', 'position': 'absolute' });
				break;
			}
		}
	};

	//Updating the CSS for ProgressButtons to allow for Preview Options
	windowObj.RelativityImport.PreviewOptions.UI.repositionProgressButtons = function () {
		windowObj.RelativityImport.PreviewOptions.renderCssForDifferentBrowser();
		windowObj.$(idSelector(PROGRESS_BUTTONS)).addClass('flex-container');
		windowObj.$(idSelector("stepProgress")).css({ 'width': '80%' });
		windowObj.$(idSelector("back")).css({ 'margin-right': '5px' });

	};

	//Removing CSS for ProgressButtons to return to normal state
	windowObj.RelativityImport.PreviewOptions.UI.UndoRepositionProgressButtons = function() {
		windowObj.$(idSelector(PROGRESS_BUTTONS)).css({ 'margin-top': '', 'position': '' });
		windowObj.$(idSelector(PROGRESS_BUTTONS)).removeClass('flex-container');
		windowObj.$(idSelector("stepProgress")).css({ 'width': '85%' });
		windowObj.$(idSelector("back")).css({ 'margin-right': '', 'display': '' });
		windowObj.$(idSelector("next")).css({ 'margin-right': '', 'display': '' });
		windowObj.$(classSelector("verticalSplit")).remove();
	};

	//Add dropdown to DOM, init handlers for the dropdown and each of the items added
	windowObj.RelativityImport.PreviewOptions.UI.initCustomDropdown = function () {
		var source = windowObj.$(idSelector(PROGRESS_BUTTONS));
		source.append(
			'<span class="verticalSplit"></span>' +
			'<div class="select collapsed" id="importCustomButton">' +
				'<div class="shown" id="title"><i class="icon-chevron-down" style="float: right; padding-right: 3px;"></i>Preview Options</div>' +
				'<div class="option" id="preFile">File</div>' +
				'<div class="option" id="preErrors">Errors</div>' +
				'<div class="option" id="preChoice">Choices & Folders</div>' +
			'</div>');

		assignDropdownItemHandlers();
	};

})(this, IP);