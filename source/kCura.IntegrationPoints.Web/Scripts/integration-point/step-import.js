var IP = IP || {};
(function (root, ko) {
	var step = function (settings) {
		var self = this;
		self.settings = settings;

		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = {};
		this.frameBus = {};
		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				var frameName = 'configurationFrame';
				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
				$('#' + frameName).iFrameResize({ heightCalculationMethod: 'max' });
				//debugger;
				self.frameBus = IP.frameMessaging({ source: window[frameName].contentWindow });
			});
		};

		var bus = IP.frameMessaging();
		this.submit = function () {
			var d = root.data.deferred().defer();
			this.frameBus.publish('submit');
			bus.subscribe('saveComplete', function (data) {
				d.resolve();
			});
			return d.promise;
		};
	};

	var step = new step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'ConfigurationDetail'),
		templateID: 'configuration'
	});

	root.points.steps.push(step);

})(IP, ko);