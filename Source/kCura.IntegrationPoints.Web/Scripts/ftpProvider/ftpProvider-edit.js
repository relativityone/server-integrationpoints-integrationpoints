ko.validation.configure({
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

	var _encrypt = function (model) {
		return IP.data.ajax({
			cache: false,
			data: JSON.stringify(model),
			url: IP.utils.generateWebAPIURL('FtpProviderAPI', 'e'),
			async: false,
			type: 'post'
		});

	};

	var _decrypt = function (model) {
		return IP.data.ajax({
			cache: false,
			data: JSON.stringify(model),
			url: IP.utils.generateWebAPIURL('FtpProviderAPI', 'd'),
			async: false,
			type: 'post'
		});
	};

	var _getModel = function () {
		var model = {
			host: $('#host').val(),
			port: $('#port').val(),
			protocol: $('#protocol').val(),
			username: $('#username').val(),
			password: $('#password').val(),
			filename_prefix: $('#filename_prefix').val(),
			timezone_offset: new Date().getTimezoneOffset()
		};
		return model;

	}

	var _setModel = function (model) {
		$('#host').val(model.host);
		$('#port').val(model.port);
		$("#protocol").select2("val", model.protocol);
		$('#username').val(model.username);
		$('#password').val(model.password);
		$('#filename_prefix').val(model.filename_prefix);
		$('#timezone_offset').val(new Date().getTimezoneOffset());
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
		getSettingsModel: _getModel,
		setSettingsModel: _setModel,
		encryptSettings: _encrypt,
		decryptSettings: _decrypt,
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
	function retrieveValueFromServer(model) {
		return helper.getColumnList(model);
	}

	function getSettingsModel() {
		return helper.getSettingsModel();
	}

	function setSettingsModel(model) {
		return helper.setSettingsModel(model);
	}

	function encryptSettings(model) {
		return helper.encryptSettings(model);
	}

	function decryptSettings(model) {
		return helper.decryptSettings(model);
	}

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.

		var localModel = getSettingsModel();
		var self = this;

		//Validate that the input will connect to a valid FTP/SFTP server
		//todo: how to do this: document.body.style.cursor = "progress";

		var p1 = validateSettings(localModel);
		p1.then(function () {
			retrieveValueFromServer(localModel).then(function (columnList) {

				localModel.columnlist = columnList;
				//update model with the value returned;
				document.getElementById('validation_message').innerHTML = "";
				self.publish("saveState", localModel);
				//Encrypt Model for DB save
				encryptSettings(localModel).then(function (encryptedModel) {
					//Communicate to the host page to continue.
					self.publish('saveComplete', encryptedModel);
				});
			}, function (data) {

				if (data.status == '400') {
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
		model.unencrypted = true;
		this.publish('saveState', JSON.stringify(ko.toJS(model)));
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (model) {
		var temp = isJson(model);
		if (temp && JSON.parse(model).unencrypted) {
			setSettingsModel(JSON.parse(model));
		}
		else {
			decryptSettings(model).then(function (decryptedJsonModel) {
				var decryptedModel = JSON.parse(decryptedJsonModel);
				setSettingsModel(decryptedModel);
			});
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


//validate the model by calling an action on the Controller
function validate_model(model) {
	var valid = false;

	//set async to false so the next page does not load before validation returns
	$.ajaxSetup({ async: false });

	var path = "/Relativity/CustomPages/GUID/Provider/ValidateHostConnection".replace("GUID", ApplicationGuid);

	var validation = $.post(path, { host: model.host, port: model.port, protocol: model.protocol, username: model.username, password: model.password, filename_prefix: model.filename_prefix }, function () {
		//changes the cursor to a progress wheel during validation
		document.body.style.cursor = "progress";
	})
		.done(function () {
			document.getElementById('validation_message').innerHTML = "";
			valid = true;
		})
		.fail(function (error) {
			IP.frameMessaging().dFrame.IP.message.error.raise(error.statusText);
		});

	return valid;
}

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
	ko.applyBindings(toolTipViewModel);

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