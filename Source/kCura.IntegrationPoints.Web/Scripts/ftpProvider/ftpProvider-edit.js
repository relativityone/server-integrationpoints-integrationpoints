ko.validation.init({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
});

ko.validation.insertValidationMessage = function (element) {
	var errorContainer = document.createElement('div');
	var iconSpan = document.createElement('span');
	iconSpan.className = 'icon-error legal-hold field-validation-error';

	errorContainer.appendChild(iconSpan);

	$(element).parents('.field-value').eq(0).append(errorContainer);

	return iconSpan;
};

//Response Models
function SecurityResponse(val) {
	this.value = val;
}

var ftpHelper = (function (data) {
	var _validateSettings = function (localModel) {

		IP.frameMessaging().dFrame.IP.reverseMapFields = false;  // set the flag so that the fields can be reversed or not ;
		return IP.data.ajax({
			url: IP.utils.generateWebURL('FtpProvider', 'ValidateHostConnection'),
			data: JSON.stringify(localModel),
			type: 'post'
		});
	};

	var _getSettings = function () {
		var model = {
			host: $('#host').val(),
			port: $('#port').val(),
			protocol: $('#protocol').val(),
			filename_prefix: $('#filename_prefix').val(),
			timezone_offset: new Date().getTimezoneOffset()
		};
		return model;
	}

	var _getSerializedCredentials = function () {
		var model = {
			username: $('#username').val(),
			password: $('#password').val(),
		};
		return JSON.stringify(model);
	}

	var _getSerializedModel = function () {
		var settings = _getSettings();
		var serializedCredentials = _getSerializedCredentials();

		var model = {
			settings: JSON.stringify(settings),
			credentials: serializedCredentials
		};

		return model;
	};

	var _setSettings = function (model) {
		$('#host').val(model.host);
		$('#port').val(model.port);
		$("#protocol").select2("val", model.protocol);
		$('#filename_prefix').val(model.filename_prefix);
		$('#timezone_offset').val(new Date().getTimezoneOffset());
	}

	var _setCredentials = function (model) {
		$('#username').val(model.username);
		$('#password').val(model.password);
	}

	var _getColumnList = function (model) {
		return IP.data.ajax({
			cache: false,
			data: JSON.stringify(model),
			url: IP.utils.generateWebAPIURL('FtpProviderAPI', 'r'),
			async: false,
			type: 'post'
		});
	}
	return {
		validateSettings: _validateSettings,
		getSerializedModel: _getSerializedModel,
		getSettings: _getSettings,
		getSerializedCredentials: _getSerializedCredentials,
		setSettings: _setSettings,
		setCredentials: _setCredentials,
		getColumnList: _getColumnList
	}

})(IP.data);

(function (helper) {

	$("#protocol").select2({
		dropdownAutoWidth: false,
		dropdownCssClass: "filter-select",
		containerCssClass: "filter-container",
	});
	$("#protocol").parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");

	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	function validateSettings(model) {
		return helper.validateSettings(model);
	}
	function getColumnList(model) {
		return helper.getColumnList(model);
	}

	function getSerializedModel() {
		return helper.getSerializedModel();
	}

	function getSettings() {
		return helper.getSettings();
	}

	function getSerializedCredentials() {
		return helper.getSerializedCredentials();
	}

	function setModel(model) {
		helper.setSettings(model);

		if (model.SecuredConfiguration) {
			var securedConfiguration = JSON.parse(model.SecuredConfiguration);
			helper.setCredentials(securedConfiguration);
		}
	}

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.

		var serializedModel = getSerializedModel();
		
		var self = this;

		//Validate that the input will connect to a valid FTP/SFTP server
		//todo: how to do this: document.body.style.cursor = "progress";

		var p1 = validateSettings(serializedModel);
		p1.then(function () {
			getColumnList(serializedModel).then(function (columnList) {
				var settings = getSettings();

				settings.columnlist = columnList;

				var serializedSettings = JSON.stringify(settings);

				//update model with the value returned;
				document.getElementById('validation_message').innerHTML = "";
				self.publish("saveState", serializedSettings);
				
				//Communicate to the host page to continue.
				self.publish('saveComplete', serializedSettings);

				var stepModel = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model;
				stepModel.SecuredConfiguration = getSerializedCredentials();
				
			}, function (data) {

				if (data.status === '400') {
					IP.frameMessaging().dFrame.IP.message.error.raise("File not found.");
				} else {
					IP.frameMessaging().dFrame.IP.message.error.raise(data.responseText);
				}
			});
		}, function (error) {
			IP.frameMessaging().dFrame.IP.message.error.raise(error.statusText);
		});
	});

	//An event raised when a user clicks the Back button.
	message.subscribe('back', function () {
		var model = getSettingsModel();
		this.publish('saveState', JSON.stringify(ko.toJS(model)));
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (model) {
		var temp = isJson(model);
		if (temp) {
			setModel(JSON.parse(model));
		}
		
		function isJson(str) {
			try {
				JSON.parse(str);
			} catch (e) {
				return false;
			}
			return true;
		}
	});
})(ftpHelper);

//master toggle for protocol changes
function protocol_onchange() {
	toggle_port();
}
(function () {

	function toolTipViewModel() {

		self = this;
		self.ftpConfigDetailsTooltipViewModel = new TooltipViewModel(TooltipDefs.FtpConfigurationDetails, TooltipDefs.FtpConfigurationDetailsTitle);

		Picker.create("Tooltip", "tooltipFtpConfigId", "TooltipView", self.ftpConfigDetailsTooltipViewModel);

		self.openFtpConfigDetailsTooltip = function (data, event) {
			ftpConfigDetailsTooltipViewModel.open(event);
		};
	}
	ko.applyBindings(toolTipViewModel, document.documentElement);

})();
//toggle port based on protocol if it's not modified by the user
function toggle_port() {
	var port_selected = document.getElementById("port").value;

	// Two steps for drop down
	var protocol = document.getElementById("protocol");
	var protocol_selected = protocol.options[protocol.selectedIndex].text;

	// If user has entered something else, will not toggle
	if (port_selected === '' || port_selected === '21' || port_selected === '22') {
		if (protocol_selected === 'FTP') {
			$('#port').val(21);
		}
		else {
			$('#port').val(22);
		}
	}
}