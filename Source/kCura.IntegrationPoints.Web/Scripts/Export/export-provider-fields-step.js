var IP = IP || {};

(function (root, ko) {
	var viewModel = function (state) {
		var self = this;

		self.HasBeenRun = ko.observable(state.hasBeenRun || false);

		self.savedSearches = ko.observableArray(state.SavedSearches);

		self.savedSearch = ko.observable(state.SavedSearch).extend({
			required: true
		});

		self.startExportAtRecord = ko.observable(state.StartExportAtRecord || 1).extend({
			required: true,
			min: 1,
			nonNegativeNaturalNumber: {}
		});

		self.fields = new FieldMappingViewModel();

		self.getSelectedSavedSearch = function (artifactId) {
			var selectedSavedSearch = ko.utils.arrayFirst(self.savedSearches(), function (item) {
				if (item.value === artifactId) {
					return item;
				}
			});

			return selectedSavedSearch;
		};
	};

	var stepModel = function (settings) {
		var self = this;

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

			if (typeof ip.sourceConfiguration === "string") {
				try {
					// parse config of existing IP
					this.ipModel.sourceConfiguration = JSON.parse(ip.sourceConfiguration);
				} catch (e) {
					// create new config
					this.ipModel.sourceConfiguration = {
						SourceWorkspaceArtifactId: IP.data.params['appID']
					};
				}
			}

			self.model = new viewModel($.extend({}, self.ipModel, { hasBeenRun: ip.hasBeenRun }));
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
					self.model.fields.mappedFields(result);
				}).fail(function (error) {
					IP.message.error.raise("No attributes were returned from the source provider.");
				});
			};

			self.updateSelectedSavedSearch = function () {
				var selectedSavedSearch = self.model.getSelectedSavedSearch(self.ipModel.sourceConfiguration.SavedSearchArtifactId);

				if (!!selectedSavedSearch) {
					self.model.savedSearch(selectedSavedSearch.value);
				}
			};

			var savedSearchesPromise = root.data.ajax({
				type: 'get',
				url: root.utils.generateWebAPIURL('SavedSearchFinder')
			}).fail(function (error) {
				IP.message.error.raise("No saved searches were returned from the source provider.");
			});

			var exportableFieldsPromise = root.data.ajax({
				type: 'post',
				url: root.utils.generateWebAPIURL('ExportFields/Exportable'),
				data: JSON.stringify({
					options: self.ipModel.sourceConfiguration,
					type: self.ipModel.source.selectedType
				})
			}).fail(function (error) {
				IP.message.error.raise("No exportable fields were returned from the source provider.");
			});

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

					self.model.savedSearch.subscribe(function (selected) {
						if (!!selected) {
							self.getAvailableFieldsFor(selected);
						} else {
							self.model.fields.mappedFields([]);
						}
					});
				});
		}

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				// update integration point's model
				var selectedSavedSearch = self.model.getSelectedSavedSearch(self.model.savedSearch());
				self.ipModel.sourceConfiguration.SavedSearchArtifactId = selectedSavedSearch.value;
				self.ipModel.sourceConfiguration.SavedSearch = selectedSavedSearch.displayName;
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

				self.ipModel.Map = fieldMap;

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		}

		self.back = function () {
			var d = root.data.deferred().defer();

			d.resolve(self.ipModel);

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