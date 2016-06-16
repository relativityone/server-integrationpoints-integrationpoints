var IP = IP || {};

(function (root, ko) {

	var Step = function (settings) {
		var self = this;
		self.settings = settings;

		self.template = ko.observable();
		self.hasTemplate = false;
		self.getTemplate = function () {
			root.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.hasTemplate = true;
				self.template(self.settings.templateID);
				root.messaging.publish('details-loaded');
			});
		}

		self.model = {};
		self.loadModel = function (ip) {
			self.model = ip;
			self.model.SelectedOverwrite = "Append/Overlay"; // hardcoded as this value doesn't relate to export

			root.data.ajax({
				type: 'post',
				url: root.utils.generateWebAPIURL('SourceFields'),
				data: JSON.stringify({
					options: self.model.sourceConfiguration,
					type: self.model.source.selectedType
				})
			}).then(function (result) {
				var fieldMap = [];

				result.forEach(function (e, i) {
					fieldMap.push({
						sourceField: {
							displayName: e.displayName,
							isIdentifier: e.isIdentifier,
							fieldIdentifier: e.fieldIdentifier,
							isRequired: e.isRequired
						},
						destinationField: {
							displayName: e.displayName,
							isIdentifier: e.isIdentifier,
							fieldIdentifier: e.fieldIdentifier,
							isRequired: e.isRequired
						},
						fieldMapType: e.isIdentifier ? "Identifier" : "None"
					});
				});

				self.model.Map = JSON.stringify(fieldMap);
			}).fail(function (error) {
				IP.message.error.raise("No attributes were returned from the source provider.");
			});
		}

		self.submit = function () {
			var d = root.data.deferred().defer();
			d.resolve(self.model);

			return d.promise;
		}
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3Export'),
		templateID: 'step3Export',
		isForRelativityExport: true
	});

	root.points.steps.push(step);

})(IP, ko);