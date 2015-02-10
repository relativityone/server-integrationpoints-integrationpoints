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
var ldapHelper = (function (data) {
	var _checkLdap = function (localModel) {
		return IP.data.ajax({
			url: IP.utils.generateWebURL('IntegrationPoints', 'CheckLdap'),
			data: JSON.stringify(localModel),
			type: 'Post'
		});
	};

	var _encrypt = function (model) {
		return IP.data.ajax({
			data: JSON.stringify(model),
			url: IP.utils.generateWebAPIURL('ldap', 'e'),
			type: 'post'
		});

	};

	return {
		checkLdap: _checkLdap,
		encryptSettings: _encrypt

	}

})(IP.data);


(function (helper) {
	var message = IP.frameMessaging();
	var pageModel = {};

	function checkLdap(localModel) {
		return helper.checkLdap(localModel).fail(function (e) {
			self.publish('saveError', 'Unable to connect to source using the specified settings.');
		});
	}

	function encrypt(localModel) {
		return helper.encryptSettings(localModel);
	}

	message.subscribe('submit', function () {
		var localModel = ko.toJS(pageModel);
		this.publish("saveState", localModel); //save the model incase of error
		var self = this;
		if (pageModel.errors().length === 0) {
			var p1 = checkLdap(localModel);
			var p2 = encrypt(localModel);
			IP.data.deferred().all(p1, p2).then(function () {
				p2.then(function (message) {
					self.publish('saveComplete', message);
				});
			});

		} else {
			pageModel.errors.showAllMessages();
		}
	});
	var viewModel = function (model) {
		var state = $.extend({}, {}, model);
		this.connectionPath = ko.observable(state.connectionPath).extend({
			required: true
		});
		this.filter = ko.observable(state.filter);
		this.auth = ko.observableArray([
			{ name: 'Anonymous', id: 16 },
			{ name: 'Encryption', id: 2 },
			{ name: 'FastBind', id: 32 },
			{ name: 'Secure Socket Layer', id: 2 }
		]);
		this.userName = ko.observable(state.userName);
		this.password = ko.observable(state.password);

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

		if (typeof model === "object") {
			model = JSON.stringify(model);
		}
		IP.data.ajax({ data: JSON.stringify(model), url: IP.utils.generateWebAPIURL('ldap', 'd'), type: 'post' }).then(function (result) {
			var jsonResult = result;
			if (typeof (jsonResult) === "string") {
				try {
					jsonResult = JSON.parse(jsonResult);
				} catch (e) {
					jsonResult = {};
				}
			}
			_bind(jsonResult);
		}, function () {
			_bind({});
		});

	});
})(ldapHelper);