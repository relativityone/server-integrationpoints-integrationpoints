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
var ldapHelper = (function (data) {
	var _checkLdap = function (localModel) {
		
		IP.frameMessaging().dFrame.IP.reverseMapFields = false;  // set the flag so that the fields can be reversed or not ;
		return IP.data.ajax({
			url: IP.utils.generateWebAPIURL('Ldap', 'CheckLdap'),
			data: JSON.stringify({
				settings: createSettingsModel(localModel),
				credentials: createSecuredConfiguration(localModel)
			}),
			type: 'Post'
		});
	};

	return {
		checkLdap: _checkLdap
	}

})(IP.data);

function createSettingsModel(model) {
	return JSON.stringify({
		connectionPath: model.connectionPath,
		filter: model.filter,
		connectionAuthenticationType: model.connectionAuthenticationType,
		importNested: model.importNested
	});
}

function createSecuredConfiguration(model) {
	return JSON.stringify({
		userName: model.userName,
		password: model.password
	});
}

(function (helper) {
	var message = IP.frameMessaging();
	var pageModel = {};

	function checkLdap(localModel) {
		return helper.checkLdap(localModel).fail(function (e) {
			message.publish('saveError', 'Unable to connect to source using the specified settings. Check Errors tab for details.');
		});
	}

	message.subscribe('submit', function () {
		var localModel = ko.toJS(pageModel);
		var stringifiedModel = createSettingsModel(localModel);
		this.publish("saveState", stringifiedModel); //save the model incase of error
		var self = this;
		if (pageModel.errors().length === 0) {
			checkLdap(localModel).then(function () {
				self.publish('saveComplete', stringifiedModel);
			});
		} else {
			pageModel.errors.showAllMessages();
		}

		var destinationModel = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model;
		destinationModel.SecuredConfiguration = createSecuredConfiguration(localModel);
	});

	var viewModel = function (model) {
		var state = $.extend({}, {}, model);
		this.connectionPath = ko.observable(state.connectionPath).extend({
			required: true
		});
		this.filter = ko.observable(state.filter);
		this.auth = ko.observableArray([
			{ name: 'Anonymous', id: 16 },
			{ name: 'FastBind', id: 32 },
			{ name: 'Secure Socket Layer', id: 2 }
		]);

		var securedConfiguration = JSON.parse(state.SecuredConfiguration || '{}');

		this.userName = ko.observable(securedConfiguration.userName);
		this.password = ko.observable(securedConfiguration.password);
		
		this.connectionAuthenticationType = ko.observable(state.connectionAuthenticationType).extend({
			required: true
		});

		this.importNested = ko.observable(state.importNested || 'false');

		this.errors = ko.validation.group(this, { deep: true });
	};

	message.subscribe("back", function () {
		this.publish("saveState", JSON.stringify(ko.toJS(pageModel)));
	});

	message.subscribe('load', function (model) {

		var _bind = function (model) {
			pageModel = new viewModel(model);
			ko.applyBindings(pageModel, document.getElementById('ldapConfiguration'));
		};

		if (typeof model === "string") {
			try {
				model = JSON.parse(model);
			} catch (e) {
				model = {};
			}
			_bind(model);
		} else {
			_bind({});
		}
	});
})(ldapHelper);