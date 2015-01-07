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

(function () {
	var message = IP.frameMessaging();
	var pageModel = {};
	message.subscribe('submit', function () {
		var localModel = ko.toJS(pageModel);
		this.publish("saveState", localModel); //save the model incase of error
		var self = this;
		if (pageModel.errors().length === 0) {
			IP.data.ajax({
				url: IP.utils.generateWebURL('IntegrationPoints', 'CheckLdap'),
				data: JSON.stringify(localModel),
				type: 'Post'
			}).then(function () {
				self.publish('saveComplete', ko.toJS(pageModel));
			},
			function (e) {
				self.publish('saveError', 'Unable to connect to source using the specified settings.');
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
		this.connectionAuthenticationType = ko.observable(state.connectionAuthenticationType);
		this.importNested = ko.observable(state.importNested || 'false');

		this.errors = ko.validation.group(this, { deep: true });
	};

	message.subscribe("back", function () {
		this.publish("saveState", ko.toJS(pageModel));
	});

	message.subscribe('load', function (model) {
		if (typeof (model) === "string") {
			try {
				model = JSON.parse(model);
			} catch (e) {
				model = {};
			}
		}
		pageModel = new viewModel(model);
		ko.applyBindings(pageModel, document.getElementById('ldapConfiguration'));
	});

})();