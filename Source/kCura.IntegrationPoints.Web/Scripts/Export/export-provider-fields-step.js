var IP = IP || {};

(function (root, ko) {
	var viewModel = function (state) {
		var self = this;

		self.HasBeenRun = ko.observable(state.hasBeenRun || false);

		var sourceState = state.sourceConfiguration || {};
		self.TypeOfExport = ko.observable(sourceState.ExportType || ExportEnums.SourceOptionsEnum.SavedSearch);

		self.IsSavedSearchSelected = function () {
			return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.SavedSearch;
		};

		self.IsProductionSelected = function () {
			return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Production;
		};

		self.FolderLabelDescription = ko.observable();
		self.IsFolderOrSubfolderSelected = function () {
			var isFolderOrSubfolderSelected = false;
			if (self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Folder) {
				self.FolderLabelDescription(ExportEnums.SourceOptions[ExportEnums.SourceOptionsEnum.Folder].key);
				isFolderOrSubfolderSelected = true;
			}
			if (self.TypeOfExport() === ExportEnums.SourceOptionsEnum.FolderSubfolder) {
				self.FolderLabelDescription(ExportEnums.SourceOptions[ExportEnums.SourceOptionsEnum.FolderSubfolder].key);
				isFolderOrSubfolderSelected = true;
			}
			if (isFolderOrSubfolderSelected) {
				self.getFolderAndSubFolders();
			}
			return isFolderOrSubfolderSelected;
		};

		self.getFolderAndSubFolders = function () {
			root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("SearchFolder")
			}).then(function (result) {
				self.foldersStructure = result;
				self.locationSelector.reload(result);
			}).fail(function (error) {
				root.message.error.raise("No folders were returned from the source provider.");
			});
		};

		self.savedSearches = ko.observableArray(state.SavedSearches);

		self.savedSearch = ko.observable(state.SavedSearch).extend({
			required: {
				onlyIf: function () {
					return self.IsSavedSearchSelected();
				}
			}
		});

		self.savedSearchesTree = ko.observable();

		self.isSavedSearchTreeNode = function (node) {
			return !!node && (node.icon === "jstree-search" || node.icon === "jstree-search-personal");
		}

		var savedSearchPickerViewModel = new SavedSearchPickerViewModel(function (value) {
			self.savedSearch(value.id);
		}, self.isSavedSearchTreeNode);

		Picker.create("savedSearchPicker", "SavedSearchPicker", savedSearchPickerViewModel);

		self.openSavedSearchPicker = function () {
			savedSearchPickerViewModel.open(self.savedSearchesTree(), self.savedSearch());
		};

		self.SelectedSource = ko.observable();

		self.productionSets = ko.observableArray(state.productionSets);

		self.ProductionName = ko.observable(sourceState.ProductionName);
		self.ProductionId = ko.observable(sourceState.ProductionId).extend({
			required: {
				onlyIf: function () {
					return self.IsProductionSelected();
				}
			}
		});

		self.getSelectedProduction = function (artifactId) {
			var selectedProduction = ko.utils.arrayFirst(self.productionSets(), function (item) {
				if (item.artifactID === artifactId) {
					return item;
				}
			});
			return selectedProduction;
		};

		self.startExportAtRecord = ko.observable(state.StartExportAtRecord || 1).extend({
			required: true,
			min: 1,
			nonNegativeNaturalNumber: {}
		});

		self.FolderArtifactId = ko.observable(sourceState.FolderArtifactId).extend({
			required: {
				onlyIf: function () {
					return self.IsFolderOrSubfolderSelected();
				}
			}
		});
		self.FolderArtifactName = ko.observable(sourceState.FolderArtifactName).extend({
			required: {
				onlyIf: function () {
					return self.IsFolderOrSubfolderSelected();
				}
			}
		});

		self.getFolderFullName = function(currentFolder, folderId){
			if(currentFolder.id == folderId) {
				return currentFolder.text;
			} else {
				for(var i = 0; i < currentFolder.children.length; i++){
					var childFolderPath = self.getFolderFullName(currentFolder.children[i], folderId);
					if(childFolderPath != ""){
						return currentFolder.text + "/" + childFolderPath;
					}
				}
			}
			return "";
		};

		self.availableViews = ko.observableArray(['Test']);
		self.ViewName = ko.observable(sourceState.ViewName);
		self.ViewId = ko.observable(sourceState.ViewId).extend({
			required: {
				onlyIf: function () {
					return self.IsFolderOrSubfolderSelected();
				}
			}
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

		self.getSelectedView = function (artifactId) {
			var selectedView = ko.utils.arrayFirst(self.availableViews(), function (item) {
				if (item.artifactId === artifactId) {
					return item;
				}
			});

			return selectedView;
		};

		self.onDOMLoaded = function () {
			self.locationSelector = new LocationJSTreeSelector();
			if (self.HasBeenRun()) {
				self.locationSelector.toggle(false);
			} else {
				self.locationSelector.init(self.FolderArtifactName(), [], {
					onNodeSelectedEventHandler: function (node) {
						self.FolderArtifactName(node.text);
						self.FolderArtifactId(node.id);
					}
				});
				self.locationSelector.toggle(true);
			}
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

			self.model = new viewModel($.extend({}, self.ipModel, {
				hasBeenRun: ip.hasBeenRun
			}));
			self.model.errors = ko.validation.group(self.model);

			self.getAvailableFields = function () {
				root.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ExportFields/Available'),
					data: JSON.stringify({
						options: self.ipModel.sourceConfiguration,
						type: self.ipModel.source.selectedType
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
					IP.message.error.raise("No attributes were returned from the source provider.");
				});
			};

			self.updateSelectedSavedSearch = function () {
				var selectedSavedSearch = self.model.getSelectedSavedSearch(self.ipModel.sourceConfiguration.SavedSearchArtifactId);

				if (!!selectedSavedSearch) {
					self.model.savedSearch(selectedSavedSearch.value);
				}
			};

			self.updateSelectedProduction = function () {
				var selectedProduction = self.model.getSelectedProduction(self.ipModel.sourceConfiguration.ProductionId);

				if (!!selectedProduction) {
					self.model.ProductionId(selectedProduction.artifactID);
				}
			};

			self.updateSelectedView = function () {
				var selectedView = self.model.getSelectedView(self.ipModel.sourceConfiguration.ViewId);

				if (!!selectedView) {
					self.model.ViewId(selectedView.artifactId);
				}
			};

			var savedSearchesPromise = root.data.ajax({
				type: 'get',
				url: root.utils.generateWebAPIURL('SavedSearchFinder')
			}).fail(function (error) {
				IP.message.error.raise("No saved searches were returned from the source provider.");
			});

			var savedSearchesTreePromise = root.data.ajax({
				type: 'get',
				url: root.utils.generateWebAPIURL('SavedSearchesTree', self.ipModel.sourceConfiguration.SourceWorkspaceArtifactId)
			}).fail(function (error) {
				IP.message.error.raise(error);
			});

			var searchFoldersPromise = root.data.ajax({
				type: 'get',
				url: root.utils.generateWebAPIURL('SearchFolder')
			}).fail(function (error) {
				IP.message.error.raise("No search folders were returned from the source provider.");
			});

			var productionSetsPromise = root.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("Production/Productions"),
				data: {
					sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top)
				}
			}).fail(function (error) {
				IP.message.error.raise("No production sets were returned from the source provider.");
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

			var getViewsPromise = root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("WorkspaceView/GetViews", 10)
			}).fail(function (error) {
				root.message.error.raise("No views were returned from the source provider.");
			});

			var getSavedSearches = function (tree) {
				var _searches = [];
				var _iterate = function (node, depth) {
					if (self.model.isSavedSearchTreeNode(node)) {
						_searches.push({
							value: node.id,
							displayName: node.text
						});
					}

					for (var i = 0, len = node.children.length; i < len; i++) {
						_iterate(node.children[i], depth + 1);
					}
				};

				_iterate(tree, 0);

				return _searches;
			};

			root.data.deferred().all([
				savedSearchesTreePromise,
				exportableFieldsPromise,
				availableFieldsPromise,
				mappedFieldsPromise,
				getViewsPromise,
				productionSetsPromise
			]).then(function (result) {
				self.model.savedSearchesTree(result[0]);
				self.model.savedSearches(getSavedSearches(result[0]));
				self.updateSelectedSavedSearch();

				self.model.fields.availableFields(result[1]);

				var mappedFields = (result[3] && result[3].length) ?
					getMappedFields(result[3]) :
					getMappedFields(result[2]);

				self.model.fields.selectedAvailableFields(mappedFields);
				self.model.fields.addField();

				self.model.availableViews(result[4]);
				self.updateSelectedView();

				self.model.savedSearch.subscribe(function (selected) {
					if (!!selected) {
						self.ipModel.sourceConfiguration.SavedSearchArtifactId = selected;
						self.ipModel.sourceConfiguration.ExportType = ExportEnums.SourceOptionsEnum.SavedSearch;
						self.getAvailableFields();
					} else {
						self.model.fields.removeAllFields();
						self.ipModel.sourceConfiguration.SavedSearchArtifactId = 0;
					}
				});

				self.model.ViewId.subscribe(function (selected) {
					if (!!selected) {
						self.ipModel.sourceConfiguration.ViewId = self.model.ViewId();
						self.ipModel.sourceConfiguration.ExportType = self.model.TypeOfExport();
						self.getAvailableFields();
					} else {
						self.model.fields.removeAllFields();
						self.ipModel.sourceConfiguration.ViewId = 0;
					}
				});

				self.model.productionSets(result[5]);
				self.updateSelectedProduction();

				self.model.ProductionId.subscribe(function (selected) {
					if (!!selected) {
						self.ipModel.sourceConfiguration.ProductionId = self.model.ProductionId();
						self.ipModel.sourceConfiguration.ExportType = self.model.TypeOfExport();
						self.getAvailableFields();
					} else {
						self.model.fields.removeAllFields();
						self.ipModel.sourceConfiguration.ProductionId = 0;
					}
				});
			});
		}

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				// update integration point's model
				var exportType = self.model.TypeOfExport();
				self.ipModel.sourceConfiguration.ExportType = exportType;
				self.ipModel.sourceConfiguration.StartExportAtRecord = self.model.startExportAtRecord();

				if (exportType === ExportEnums.SourceOptionsEnum.SavedSearch) {
					var selectedSavedSearch = self.model.getSelectedSavedSearch(self.model.savedSearch());
					self.ipModel.sourceConfiguration.SavedSearchArtifactId = selectedSavedSearch.value;
					self.ipModel.sourceConfiguration.SavedSearch = selectedSavedSearch.displayName;
				} else if (exportType === ExportEnums.SourceOptionsEnum.Folder ||
                    exportType === ExportEnums.SourceOptionsEnum.FolderSubfolder) {

					self.ipModel.sourceConfiguration.FolderArtifactId = self.model.FolderArtifactId();
					self.ipModel.sourceConfiguration.FolderArtifactName = self.model.FolderArtifactName();
					self.ipModel.sourceConfiguration.FolderFullName = self.model.getFolderFullName(self.model.foldersStructure, self.model.FolderArtifactId());
					self.ipModel.sourceConfiguration.ViewId = self.model.ViewId();
					var selectedView = self.model.getSelectedView(self.model.ViewId());
					self.ipModel.sourceConfiguration.ViewName = selectedView.name;
				}
				else if (exportType === ExportEnums.SourceOptionsEnum.Production) {
					self.ipModel.sourceConfiguration.ProductionId = self.model.ProductionId();
					var selectedProduction = self.model.getSelectedProduction(self.model.ProductionId());
					self.ipModel.sourceConfiguration.ProductionName = selectedProduction.displayName;
				}

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

				Picker.closeDialog("savedSearchPicker");

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		}

		self.back = function () {
			var d = root.data.deferred().defer();

			Picker.closeDialog("savedSearchPicker");

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
