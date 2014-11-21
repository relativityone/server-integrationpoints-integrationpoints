var IP = IP || {};
(function (root, ko) {
	var step = function (settings) {
		var self = this;
		self.settings = settings;

		this.template = ko.observable();
		this.hasTemplate = false;
		this.getTemplate = function () {
			IP.data.ajax({}, { dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
			});
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			root.data.ajax({}, { type: 'get', url: 'http://localhost/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/ValidationConnection' }).then(function (result) {
				if (result.success) {
					d.resolve();
				} else {
					d.reject();
				}
			});
			return d.promise;
		};
	};

	var step = new step({
		url: 'http://localhost/Relativity/CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/IntegrationPoints/StepDetails2',
		templateID: 'step2'
	});

	root.points.steps.push(step);


})(IP, ko);