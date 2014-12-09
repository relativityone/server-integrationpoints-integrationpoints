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
		url: 'http://localhost/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/ConfigurationDetail',
		templateID: 'configuration'
	});

	root.points.steps.push(step);


})(IP, ko);