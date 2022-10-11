var IP = IP || {};

(function (root, ko) {
	var viewModel = function (state) {
		var self = this;

		self.fields = new FieldMappingViewModel(false);
		var savedSearchService = new SavedSearchService();
		self.exportSource = new ExportSourceViewModel(state, savedSearchService);
		self.exportSource.Reload();

		self.startExportAtRecord = ko.observable(state.sourceConfiguration.StartExportAtRecord || 1).extend({
			required: true,
			min: 1,
			nonNegativeNaturalNumber: {}
		});

		self.onDOMLoaded = function () {
			self.exportSource.InitializeLocationSelector();
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
				self.model.onDOMLoaded();
				root.messaging.publish('details-loaded');
			});
		};

		self.ipModel = {};
		self.model = {};
		self.cache = {};

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

			self.model = new viewModel($.extend({}, self.ipModel, {
				cache: self.cache
			}));

			self.model.errors = ko.validation.group(self.model);
			self.model.errors.showAllMessages(false);

			self.model.fields.filterSourceField = ko.observable("");

			self.model.fields.filterFields = function (fields, filter) {
				let filterLowerCase = filter().toLowerCase();
				let filteredFields = []
				fields().forEach(field => {
					if (field.displayName.toLowerCase().indexOf(filterLowerCase) > -1) {
						filteredFields.push(field)
					}
				});
				return filteredFields;
			}

			self.getAvailableFields = function (fieldName, fieldValue) {
				self.ipModel.sourceConfiguration[fieldName] = fieldValue || 0;
				self.ipModel.sourceConfiguration.ExportType = self.model.exportSource.TypeOfExport();

				// check if its first run, to prevent mapping clearance when editing IP
				if (self.model.fields.firstRun) {
					self.model.fields.firstRun = false;
				} else {
					if (!!fieldValue) {
						root.data.ajax({
							type: 'post',
							url: root.utils.generateWebAPIURL('ExportFields/Available'),
							data: JSON.stringify({
								options: self.ipModel.sourceConfiguration,
								type: self.ipModel.source.selectedType,
								transferredArtifactTypeId: self.ipModel.artifactTypeID
							})
						}).then(function (result) {
							self.model.fields.removeAllFields();
							self.model.fields.selectedAvailableFields(ko.utils.arrayMap(result, function (_item1) {
								var _field = ko.utils.arrayFilter(self.model.fields.availableFields(), function (_item2) {
									return _item1.fieldIdentifier === _item2.fieldIdentifier;
								});
								return _field[0];
							}));
							self.model.fields.addField();
						}).fail(function (error) {
							IP.message.error.raise("No available fields were returned from the source provider.");
						});
					} else {
						self.model.fields.removeAllFields();
					}
				}
			};

			self.model.exportSource.TypeOfExport.subscribe(function (value) {
				self.ipModel.sourceConfiguration.ExportType = (value === parseInt(value)) ? value : undefined;

				self.ipModel.sourceConfiguration.SavedSearchArtifactId = 0;
				self.ipModel.sourceConfiguration.ProductionId = 0;
				self.ipModel.sourceConfiguration.ProductionName = undefined;
				self.ipModel.sourceConfiguration.ViewId = 0;

				if (self.shouldUpdateFieldMapping()) {
					self.model.fields.removeAllFields();
				}

				self.model.exportSource.Cache = self.cache = {};

				self.model.exportSource.SavedSearchArtifactId(undefined);
				self.model.exportSource.ProductionId(undefined);
				self.model.exportSource.ProductionName(undefined);
				self.model.exportSource.FolderArtifactName(undefined);
				self.model.exportSource.ViewId(undefined);

				self.model.exportSource.SavedSearchArtifactId.isModified(false);
				self.model.exportSource.ProductionId.isModified(false);
				self.model.exportSource.ProductionName.isModified(false);
				self.model.exportSource.FolderArtifactName.isModified(false);
				self.model.exportSource.ViewId.isModified(false);

				self.model.fields.mappedFields.isModified(false);

				self.model.exportSource.Reload();
			});

			var exportableFieldsPromise = root.data.ajax({
				type: 'post',
				url: root.utils.generateWebAPIURL('ExportFields/Exportable'),
				data: JSON.stringify({
					options: self.ipModel.sourceConfiguration,
					type: self.ipModel.source.selectedType,
					transferredArtifactTypeId: self.ipModel.artifactTypeID
				})
			}).fail(function (error) {
				IP.message.error.raise("No exportable fields were returned from the source provider.");
			});

			var mappedFieldsPromise;
			if (!!self.ipModel.map) {
				var map = self.ipModel.map;
				if (typeof (map) === 'string') {
					map = jQuery.parseJSON(self.ipModel.map);
				}
				mappedFieldsPromise = map;
			}
			else if (self.ipModel.artifactID > 0) {
				mappedFieldsPromise = root.data.ajax({
					type: 'get',
					url: root.utils.generateWebAPIURL('FieldMap', self.ipModel.artifactID)
				}).fail(function (error) {
					IP.message.error.raise("No mapped fields were returned from the source provider.");
				});
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

				_fields = _fields.filter(function (field) {
					return typeof field !== 'undefined'
				});

				return _fields;
			};

			root.data.deferred().all([exportableFieldsPromise, mappedFieldsPromise]).then(function (result) {

				var availableFields = result[0];
				var mappedFields = result[1];

				self.model.fields.createRenamedFileds(availableFields, mappedFields);
				self.model.fields.availableFields(availableFields);
				self.model.fields.selectedAvailableFields(getMappedFields(mappedFields));
				self.model.fields.addField();

				// flag used to prevent mapping clearance
				self.model.fields.firstRun = self.ipModel.artifactID > 0;

				self.model.exportSource.SavedSearchArtifactId.subscribe(function (value) {
					if (value && self.shouldUpdateFieldMapping()) {
						self.getAvailableFields("SavedSearchArtifactId", value);
					}
				});

				self.model.exportSource.ProductionId.subscribe(function (value) {
					if (value && self.shouldUpdateFieldMapping()) {
						self.getAvailableFields("ProductionId", value);
					}
				});

				self.model.exportSource.ViewId.subscribe(function (value) {
					if (self.shouldUpdateFieldMapping()) {
						self.getAvailableFields("ViewId", value);
					}
				});
			});
		};

		self.updateModel = function () {
			self.ipModel.sourceConfiguration.StartExportAtRecord = self.model.startExportAtRecord();
			switch (self.ipModel.sourceConfiguration.ExportType) {
			case ExportEnums.SourceOptionsEnum.Folder:
			case ExportEnums.SourceOptionsEnum.FolderSubfolder:
				self.ipModel.sourceConfiguration.FolderArtifactId = self.model.exportSource.FolderArtifactId();
				self.ipModel.sourceConfiguration.FolderArtifactName = self.model.exportSource.FolderArtifactName();

				var selectedView = self.model.exportSource.GetSelectedView();
				if (selectedView) {
					self.ipModel.sourceConfiguration.ViewId = selectedView.artifactId;
					self.ipModel.sourceConfiguration.ViewName = selectedView.name;
				}
				break;
			case ExportEnums.SourceOptionsEnum.Production:
				self.ipModel.sourceConfiguration.ProductionId = self.model.exportSource.ProductionId();
				self.ipModel.sourceConfiguration.ProductionName = self.model.exportSource.ProductionName();
				break;

			case ExportEnums.SourceOptionsEnum.SavedSearch:
				self.ipModel.sourceConfiguration.SavedSearchArtifactId = self.model.exportSource.SavedSearchArtifactId();
				break;
			}
			self.ipModel.map = self.model.fields.getMappedFields();
			var fileNamingFieldsList = self.model.fields.availableFields().concat(self.model.fields.mappedFields());
			fileNamingFieldsList.sort(function (a, b) {
				if (a.displayName < b.displayName) return -1;
				if (a.displayName > b.displayName) return 1;
				return 0;
			});

			self.ipModel.fileNamingFieldsList = fileNamingFieldsList;
		};

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				Picker.closeDialog("savedSearchPicker");

				self.updateModel();

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		};

		self.back = function () {
			var d = root.data.deferred().defer();

			Picker.closeDialog("savedSearchPicker");

			self.updateModel();

			d.resolve(self.ipModel);

			return d.promise;
		};

		self.shouldUpdateFieldMapping = function () {
			return !self.ipModel.isEdit && !self.ipModel.isProfileLoaded;
		}
	};

	var step = new stepModel({
		url: IP.utils.generateWebURL('IntegrationPoints', 'ExportProviderFields'),
		templateID: 'exportProviderFieldsStep',
		isForRelativityExport: true
	});
	root.points.steps.push(step);
})(IP, ko);