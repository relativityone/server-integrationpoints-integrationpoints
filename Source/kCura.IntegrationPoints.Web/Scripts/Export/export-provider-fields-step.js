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
	var viewModel = function (state) {
		var self = this;

		// TODO: reintroduce this functionality: IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun()
		self.HasBeenRun = ko.observable(false);

		self.savedSearches = ko.observableArray(state.SavedSearches);

		self.savedSearch = ko.observable(state.SavedSearch).extend({
			required: true
		});

		self.startExportAtRecord = ko.observable(state.StartExportAtRecord || 1).extend({
			required: true
		});

		self.fields = new FieldMappingViewModel();
	};

	var stepModel = function (settings) {
		var self = this;

		var _cache = {
			SavedSearches: [],
			AvailableFields: [],
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

			if (!self.ipModel.sourceConfiguration) {
				self.ipModel.sourceConfiguration = {
					SourceWorkspaceArtifactId: IP.data.params['appID']
				};
			}

			self.model = new viewModel($.extend({}, self.ipModel, _cache));
			self.model.errors = ko.validation.group(self.model);

			self.getAvailableFieldsFor = function (artifactId) {
				self.ipModel.sourceConfiguration.SavedSearchArtifactId = artifactId;

				root.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ExportFields/Available'),
					data: JSON.stringify({
						options: self.ipModel.sourceConfiguration,
						type: self.ipModel.source.selectedType
					})
				}).then(function (result) {
					self.model.fields.selectedAvailableFields(result);
					self.model.fields.addField();
				}).fail(function (error) {
					IP.message.error.raise("No attributes were returned from the source provider.");
				});
			};

			self.model.savedSearch.subscribe(function (selected) {
				if (!!selected) {
					self.getAvailableFieldsFor(selected);
				}
			});

			self.updateSelectedSavedSearch = function () {
				var selectedSavedSearch = ko.utils.arrayFirst(self.model.savedSearches(), function (item) {
					if (item.value === self.ipModel.sourceConfiguration.SavedSearchArtifactId) {
						return item;
					}
				});

				if (!!selectedSavedSearch) {
					self.model.savedSearch(selectedSavedSearch.value);
				}
			};

			var savedSearchesPromise;
			if (_cache.SavedSearches.length > 0) {
				savedSearchesPromise = _cache.SavedSearches;
			} else {
				savedSearchesPromise = root.data.ajax({
					type: 'get',
					url: root.utils.generateWebAPIURL('SavedSearchFinder')
				}).fail(function (error) {
					IP.message.error.raise("No saved searches were returned from the source provider.");
				});
			}

			var exportableFieldsPromise;
			if (_cache.AvailableFields.length > 0) {
				exportableFieldsPromise = _cache.AvailableFields;
			} else {
				exportableFieldsPromise = root.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ExportFields/Exportable'),
					data: JSON.stringify({
						options: self.ipModel.sourceConfiguration,
						type: self.ipModel.source.selectedType
					})
				}).fail(function (error) {
					IP.message.error.raise("No exportable fields were returned from the source provider.");
				});
			}

			var availableFieldsPromise;
			if (self.ipModel.sourceConfiguration.SavedSearchArtifactId > 0) {
				availableFieldsPromise = root.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ExportFields/Available'),
					data: JSON.stringify({
						options: self.ipModel.sourceConfiguration,
						type: self.ipModel.source.selectedType
					})
				}).fail(function (error) {
					IP.message.error.raise("No available fields were returned from the source provider.");
				});
			} else {
				availableFieldsPromise = [];
			}

			var mappedFieldsPromise;
			if (self.ipModel.artifactID > 0) {
				mappedFieldsPromise = root.data.ajax({
					type: 'get',
					url: root.utils.generateWebAPIURL('FieldMap', self.ipModel.artifactID)
				});
			} else if (!!self.ipModel.Map) {
				mappedFieldsPromise = self.ipModel.Map;
			} else {
				mappedFieldsPromise = [];
			}

			var getMappedFields = function (fields) {
				var _fields = ko.utils.arrayMap(fields, function (_item1) {
					var _field = ko.utils.arrayFilter(self.model.fields.availableFields(), function (_item2) {
						return (_item1.sourceField) ?
							(_item2.fieldIdentifier === _item1.sourceField.fieldIdentifier) :
							(_item2.fieldIdentifier === _item1.fieldIdentifier);
					});
					return _field[0];
				});
				return _fields;
			};

			root.data.deferred()
				.all([savedSearchesPromise, exportableFieldsPromise, availableFieldsPromise, mappedFieldsPromise])
				.then(function (result) {
					self.model.savedSearches(result[0]);
					self.updateSelectedSavedSearch();

					self.model.fields.availableFields(result[1]);

					var mappedFields = (result[3] && result[3].length) ?
						getMappedFields(result[3]) :
						getMappedFields(result[2]);

					self.model.fields.selectedAvailableFields(mappedFields);
					self.model.fields.addField();
				});
		}

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				// update integration point's model
				self.ipModel.sourceConfiguration.SavedSearchArtifactId = self.model.savedSearch();
				self.ipModel.sourceConfiguration.StartExportAtRecord = self.model.startExportAtRecord();

				var fieldMap = [];
				var hasIdentifier = false;

				self.model.fields.mappedFields().forEach(function (e, i) {
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

				// we need to have an identifier field in order not to break export
				// based on sync worker which performs field mapping
				if (!hasIdentifier) {
					fieldMap[0].sourceField.isIdentifier = true;
					fieldMap[0].destinationField.isIdentifier = true;
					fieldMap[0].fieldMapType = "Identifier";
				}

				// self.ipModel.Map = JSON.stringify(fieldMap);
				self.ipModel.Map = fieldMap;

				// update cache	
				_cache.SavedSearches = self.model.savedSearches();
				_cache.AvailableFields = self.model.fields.availableFields();

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		}

		self.back = function () {
			var d = root.data.deferred().defer();

			if (self.model) {
				if (typeof self.model.savedSearches === 'function') {
					_cache.SavedSearches = self.model.savedSearches();
				}
				// if (typeof self.model.savedSearch === 'function') {
				// 	_cache.savedSearch = self.model.savedSearch();
				// }
				// if (typeof self.model.startExportAtRecord === 'function') {
				// 	_cache.startExportAtRecord = self.model.startExportAtRecord();
				// }
				if (typeof self.model.fields.availableFields === 'function') {
					_cache.AvailableFields = self.model.fields.availableFields();
				}
				// if (typeof self.model.fields.mappedFields === 'function') {
				// 	_cache.mappedFields = self.model.fields.mappedFields();
				// }
			}

			d.resolve();

			return d.promise;
		}
	};

	var step = new stepModel({
		url: IP.utils.generateWebURL('IntegrationPoints', 'ExportProviderFields'),
		templateID: 'exportProviderFieldsStep',
		isForRelativityExport: true
	});

	root.points.steps.push(step);
})(IP, ko);