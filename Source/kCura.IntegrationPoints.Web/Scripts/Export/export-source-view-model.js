var ExportSourceViewModel = function (state) {
	var self = this;
	var sourceConfiguration = state.sourceConfiguration || {};

	self.ArtifactTypeID = state.artifactTypeID;
	self.Cache = state.cache;
	self.DefaultRdoTypeId = state.DefaultRdoTypeId;
	self.HasBeenRun = ko.observable(state.hasBeenRun);

	self.GetInitTypeOfExport = function (sourceConfiguration) {
		if (sourceConfiguration.ExportType === parseInt(sourceConfiguration.ExportType)) {
			return sourceConfiguration.ExportType;
		}
		// If configuration is not set than for Document rdo type choose SavedSearch...
		if (self.ArtifactTypeID === self.DefaultRdoTypeId) {
			return ExportEnums.SourceOptionsEnum.SavedSearch;
		}
		//...in other case we are in "RDO export" mode so we need to choose Folder/Subfolder Export Type (this is driven by RDC code)
		return ExportEnums.SourceOptionsEnum.FolderSubfolder;
	}

	var initTypeOfExport = self.GetInitTypeOfExport(sourceConfiguration);
	self.TypeOfExport = ko.observable(initTypeOfExport);

	self.ExportRdoMode = function () {
		return self.ArtifactTypeID != self.DefaultRdoTypeId;
	}

	// saved searches

	self.SavedSearches = ko.observableArray();

	self.IsSavedSearchSelected = function () {
		return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.SavedSearch;
	};

	self.SavedSearchArtifactId = ko.observable(sourceConfiguration.SavedSearchArtifactId).extend({
		required: {
			onlyIf: function () {
				return self.IsSavedSearchSelected();
			}
		}
	});

	self.SavedSearchesTree = ko.observable();

	self.GetSelectedSavedSearch = function (artifactId) {
		var selectedSavedSearch = ko.utils.arrayFirst(self.SavedSearches(), function (item) {
			if (item.value === artifactId) {
				return item;
			}
		});

		return selectedSavedSearch;
	};


	var savedSearchPickerViewModel = new SavedSearchPickerViewModel(function (value) {
		self.SavedSearchArtifactId(value.id);
	}, IsSavedSearchTreeNode);

	Picker.create("Fileshare", "savedSearchPicker", "SavedSearchPicker", savedSearchPickerViewModel);

	self.OpenSavedSearchPicker = function () {
		savedSearchPickerViewModel.open(self.SavedSearchesTree(), self.SavedSearchArtifactId());
	};

	self.UpdateSelectedSavedSearch = function (artifactId) {
		var selectedSearch = self.GetSelectedSavedSearch(artifactId);

		if (!!selectedSearch) {
			self.SavedSearchArtifactId(selectedSearch.value);
		} else {
			self.SavedSearchArtifactId(undefined);
		}
	};

	self.UpdateSavedSearches = function (artifactId) {
		self.SavedSearchesTree(self.Cache.SavedSearchesResult);
		self.SavedSearches(FlatSavedSearches(self.Cache.SavedSearchesResult));
		self.UpdateSelectedSavedSearch(artifactId || self.SavedSearchArtifactId());
	};

	// folders and subfolders

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

		return isFolderOrSubfolderSelected;
	};

	self.FolderArtifactId = ko.observable(sourceConfiguration.FolderArtifactId).extend({
		required: {
			onlyIf: function () {
				return self.IsFolderOrSubfolderSelected() && !self.ExportRdoMode();
			}
		}
	});

	self.FolderArtifactName = ko.observable(sourceConfiguration.FolderArtifactName).extend({
		required: {
			onlyIf: function () {
				return self.IsFolderOrSubfolderSelected() && !self.ExportRdoMode();
			}
		}
	});

	self.Folders = ko.observable();

	// views

	self.AvailableViews = ko.observableArray();

	self.ViewId = ko.observable(sourceConfiguration.ViewId).extend({
		required: {
			onlyIf: function () {
				return self.IsFolderOrSubfolderSelected();
			}
		}
	});

	self.GetSelectedView = function (artifactId) {
		artifactId = artifactId || self.ViewId();

		var selectedView = ko.utils.arrayFirst(self.AvailableViews(), function (item) {
			if (item.artifactId === artifactId) {
				return item;
			}
		});

		return selectedView;
	};

	self.UpdateSelectedView = function (artifactId) {
		var selectedView = self.GetSelectedView(artifactId);

		if (!!selectedView) {
			self.ViewId(selectedView.artifactId);
		} else {
			self.ViewId(undefined);
		}
	};

	self.UpdateViews = function (viewId) {
		self.AvailableViews(self.Cache.ViewsResult[0]);
		self.UpdateSelectedView(viewId || self.ViewId());
	};

	// productions

	self.ProductionSets = ko.observableArray();

	self.ProductionName = ko.observable(sourceConfiguration.ProductionName);

	self.IsProductionSelected = function () {
		return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Production;
	};

	self.ProductionId = ko.observable(sourceConfiguration.ProductionId).extend({
		required: {
			onlyIf: function () {
				return self.IsProductionSelected();
			}
		}
	});

	self.ProductionId.subscribe(function (value) {
		var selectedProduction = self.GetSelectedProduction(value);

		if (!!selectedProduction) {
			self.ProductionName(selectedProduction.displayName);
		} else {
			self.ProductionName(undefined);
		}
	});

	self.GetSelectedProduction = function (artifactId) {
		var selectedProduction = ko.utils.arrayFirst(self.ProductionSets(), function (item) {
			if (item.artifactID === artifactId) {
				return item;
			}
		});
		return selectedProduction;
	};

	self.UpdateSelectedProduction = function (artifactId) {
		var selectedProduction = self.GetSelectedProduction(artifactId);

		if (!!selectedProduction) {
			self.ProductionId(selectedProduction.artifactID);
			self.ProductionName(selectedProduction.displayName);
		} else {
			self.ProductionId(undefined);
			self.ProductionName(undefined);
		}
	};

	self.UpdateProductions = function (artifactId) {
		self.ProductionSets(self.Cache.ProductionsResult);
		self.UpdateSelectedProduction(artifactId || self.ProductionId());
	}

	// -----
	self.IsLocationSelectorInitialized = false;
	self.InitializeLocationSelector = function () {
		if (self.IsLocationSelectorInitialized) {
			return;
		}
		var typeOfExport = self.TypeOfExport();
		if (typeOfExport !== ExportEnums.SourceOptionsEnum.Folder &&
			typeOfExport !== ExportEnums.SourceOptionsEnum.FolderSubfolder) {
			return;
		}

		self.LocationSelector = new LocationJSTreeSelector();

		self.LocationSelector.init(self.FolderArtifactName(), [], {
			onNodeSelectedEventHandler: function (node) {
				self.FolderArtifactId(node.id);
				self.getFolderPath(IP.utils.getParameterByName("AppID", window.top), node.id);
			}
		});
		self.LocationSelector.toggle(true);

		self.Folders.subscribe(function (value) {
			self.LocationSelector.reloadWithRootWithData(value);
		});

		self.getFolderAndSubfolders();
		self.IsLocationSelectorInitialized = true;
	};

	self.getFolderPath = function (destinationWorkspaceId, folderArtifactId) {
		IP.data.ajax({
				contentType: "application/json",
				dataType: "json",
				headers: { "X-CSRF-Header": "-" },
				type: "POST",
				url: IP.utils.generateWebAPIURL("SearchFolder/GetFullPathList",
					destinationWorkspaceId,
					folderArtifactId,
					0),
				async: true
			})
			.then(function (result) {
				if (result[0]) {
					self.FolderArtifactName(result[0].fullPath);
				}
			});
	}

	self.getFolderAndSubfolders = function (folderArtifactId) {
		var reloadTree = function (params, onSuccess, onFail) {
			IP.data.ajax({
				type: "POST",
				url: IP.utils.generateWebAPIURL("SearchFolder/GetStructure",
					IP.utils.getParameterByName("AppID", window.top),
					params.id != "#" ? params.id : "0",
					0)
			}).then(function (result) {
				onSuccess(result);
				if (!!folderArtifactId) {
					self.FolderArtifactId(folderArtifactId);
					self.FolderArtifactName(self.getFolderPath(IP.utils.getParameterByName("AppID", window.top), folderArtifactId));
					self.FolderArtifactName.isModified(false);
				}
				self.foldersStructure = result;
			}).fail(function (error) {
				onFail(error);
				IP.frameMessaging().dFrame.IP.message.error.raise(error);
			});
		};

		self.LocationSelector.reloadWithRootWithData(reloadTree);
	};

	self.Reload = function () {
		switch (self.TypeOfExport()) {
		case ExportEnums.SourceOptionsEnum.Folder:
		case ExportEnums.SourceOptionsEnum.FolderSubfolder:

			var viewsPromise = IP.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("WorkspaceView/GetViews", self.ArtifactTypeID)
			}).fail(function (error) {
				IP.message.error.raise("No views were returned from the source provider.");
			});

			var viewPromiseDone = function(result) {
				self.Cache.ViewsResult = result;
				self.UpdateViews(currentViewId);
				self.InitializeLocationSelector();
			};

			if (self.ExportRdoMode()) {
				var currentViewId = self.ViewId();
				viewsPromise.done(viewPromiseDone);
			}
			else if (typeof (self.Cache.ViewsResult) === 'undefined') {
				var currentViewId = self.ViewId();

				IP.data.deferred().all([viewsPromise]).then(viewPromiseDone);
			} else {
				self.UpdateViews();
			}
			break;

		case ExportEnums.SourceOptionsEnum.Production:
			if (typeof (self.Cache.ProductionsResult) === 'undefined') {
				var productionSetsPromise = IP.data.ajax({
					type: "get",
					url: IP.utils.generateWebAPIURL("Production/GetProductionsForExport"),
					data: {
						sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top)
					}
				}).fail(function (error) {
					IP.message.error.raise("No production sets were returned from the source provider.");
				});

				var currentProductionId = self.ProductionId();

				IP.data.deferred().all(productionSetsPromise).then(function (result) {
					self.Cache.ProductionsResult = result;
					self.UpdateProductions(currentProductionId);
				});
			} else {
				self.UpdateProductions();
			}
			break;

		case ExportEnums.SourceOptionsEnum.SavedSearch:
			if (typeof (self.Cache.SavedSearchesResult) === 'undefined') {
				var savedSearchesTreePromise = IP.data.ajax({
					type: 'get',
					url: IP.utils.generateWebAPIURL('SavedSearchesTree', IP.utils.getParameterByName("AppID", window.top))
				}).fail(function (error) {
					IP.message.error.raise(error);
				});

				var currentSavedSearchArtifactId = self.SavedSearchArtifactId();

				IP.data.deferred().all(savedSearchesTreePromise).then(function (result) {
					self.Cache.SavedSearchesResult = result;
					self.UpdateSavedSearches(currentSavedSearchArtifactId);
				});
			} else {
				self.UpdateSavedSearches();
			}
			break;
		}
	};
};