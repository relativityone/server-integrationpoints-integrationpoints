var IP = IP || {};

ko.validation.rules.pattern.message = 'Invalid.';

ko.validation.init({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
}, true);

(function (root, ko) {

	var viewModel = function () {
		var self = this;

		self.availableFields = ko.observableArray([]);
		self.selectedAvailableFields = ko.observableArray([]);

		self.mappedFields = ko.observableArray([]).extend({
			minLength: {
				params: 1,
				message: "Please select at least one field."
			}
		});
		self.selectedMappedFields = ko.observableArray([]);

		self.addField = function () {
			IP.workspaceFieldsControls.add(
				self.availableFields,
				self.selectedAvailableFields,
				self.mappedFields
			);
		};

		self.addAllFields = function () {
			IP.workspaceFieldsControls.add(
				self.availableFields,
				self.availableFields,
				self.mappedFields
			);
		};

		self.removeField = function () {
			IP.workspaceFieldsControls.add(
				self.mappedFields,
				self.selectedMappedFields,
				self.availableFields
			);
		};

		self.removeAllFields = function () {
			IP.workspaceFieldsControls.add(
				self.mappedFields,
				self.mappedFields,
				self.availableFields
			);
		};

		self.moveFieldTop = function () {
			IP.workspaceFieldsControls.moveTop(
				self.mappedFields,
				self.selectedMappedFields()
			);
		};

		self.moveFieldUp = function () {
			IP.workspaceFieldsControls.up(
				self.mappedFields,
				self.selectedMappedFields
			);
		};

		self.moveFieldDown = function () {
			IP.workspaceFieldsControls.down(
				self.mappedFields,
				self.selectedMappedFields
			);
		};

		self.moveFieldBottom = function () {
			IP.workspaceFieldsControls.moveBottom(
				self.mappedFields,
				self.selectedMappedFields()
			);
		};
	};

	var Step = function (settings) {
		var self = this;
		var _cache = {
			availableFields: [],
			mappedFields: []
		};

		self.settings = settings;
		self.template = ko.observable();
		self.hasTemplate = false;
		self.getTemplate = function () {
			root.data.ajax({
				dataType: 'html',
				cache: true,
				type: 'get',
				url: self.settings.url
			}).then(function (result) {
				$('body').append(result);
				self.hasTemplate = true;
				self.template(self.settings.templateID);
				root.messaging.publish('details-loaded');
			});
		}

		self.ipModel = {};
		self.model = {};

		self.loadModel = function (ip) {
			self.ipModel = ip;
			self.ipModel.SelectedOverwrite = "Append/Overlay"; // hardcoded as this value doesn't relate to export

			self.model = new viewModel();
			self.model.errors = ko.validation.group(self.model);

			if (_cache.mappedFields.length > 0 || _cache.availableFields.length > 0) {
				self.model.availableFields(_cache.availableFields);
				self.model.mappedFields(_cache.mappedFields);

				return;
			}

			var exportableFieldsPromise = root.data.ajax({
				type: 'post',
				url: root.utils.generateWebAPIURL('ExportFields/Exportable'),
				data: JSON.stringify({
					options: self.ipModel.sourceConfiguration,
					type: self.ipModel.source.selectedType
				})
			}).fail(function (error) {
				IP.message.error.raise("No attributes were returned from the source provider.");
			});

			var availableFieldsPromise = root.data.ajax({
				type: 'post',
				url: root.utils.generateWebAPIURL('ExportFields/Available'),
				data: JSON.stringify({
					options: self.ipModel.sourceConfiguration,
					type: self.ipModel.source.selectedType
				})
			}).fail(function (error) {
				IP.message.error.raise("No attributes were returned from the source provider.");
			});

			var mappedFieldsPromise;
			if (self.ipModel.artifactID > 0) {
				mappedFieldsPromise = root.data.ajax({
					type: 'get',
					url: root.utils.generateWebAPIURL('FieldMap', self.ipModel.artifactID)
				});
			} else {
				mappedFieldsPromise = [];
			}
			
			var getMappedFields = function (fields) {
				var _fields = ko.utils.arrayMap(fields, function (_item1) {
					var _field = ko.utils.arrayFilter(self.model.availableFields(), function (_item2) {
						return (_item1.sourceField) ? 
							(_item2.fieldIdentifier === _item1.sourceField.fieldIdentifier) : 
							(_item2.fieldIdentifier === _item1.fieldIdentifier);
					});
					return _field[0];
				});
				return _fields;
			};

			root.data.deferred()
				.all([exportableFieldsPromise, availableFieldsPromise, mappedFieldsPromise])
				.then(function (result) {
					self.model.availableFields(result[0]);

					var mappedFields = (result[2] && result[2].length) ? 
						getMappedFields(result[2]) :
						getMappedFields(result[1]);
					
					self.model.selectedAvailableFields(mappedFields);
					self.model.addField();
				});
		}

		self.submit = function () {
			var d = root.data.deferred().defer();

			var fieldMap = [];

			self.model.mappedFields().forEach(function (e, i) {
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

			self.ipModel.Map = JSON.stringify(fieldMap);

			if (self.model.errors().length === 0) {
				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		}

		root.messaging.subscribe("back", function () {
			_cache.availableFields = self.model.availableFields();
			_cache.mappedFields = self.model.mappedFields();
			console.log("back");
		});
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3Export'),
		templateID: 'step3Export',
		isForRelativityExport: true
	});

	root.points.steps.push(step);

})(IP, ko);