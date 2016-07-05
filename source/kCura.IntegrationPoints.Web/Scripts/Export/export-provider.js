$(function (root) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	var viewModel;

	var prepareStepsFor = function (ipType) {
		var _swap = function (__step) {
			IP.frameMessaging().dFrame.IP.points.steps.steps[2] = IP.frameMessaging().dFrame.IP.points.steps.steps[3];
			IP.frameMessaging().dFrame.IP.points.steps.steps[3] = __step;
		};

		var _step = IP.frameMessaging().dFrame.IP.points.steps.steps[2];

		switch (ipType) {
			case 'relativity':
				if (_step.settings.isForRelativityExport) {
					_swap(_step);
				}
				break;
			case 'relativityExport':
				if (!_step.settings.isForRelativityExport) {
					_swap(_step);
				}
				break;
			default:
				break;
		}
	}

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.
		this.publish("saveState", JSON.stringify(ko.toJS(viewModel)));

		if (viewModel.errors().length === 0) {
			//Communicate to the host page that it to continue.
			this.publish('saveComplete', JSON.stringify(viewModel.getSelectedOption()));
		} else {
			viewModel.errors.showAllMessages();
		}

		// Modify destination object to contain target workspaceId
		var destinationJson = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination;
		var destination = JSON.parse(destinationJson);
		destination.CaseArtifactId = viewModel.TargetWorkspaceArtifactId();
		destination.Provider = "Fileshare";
		destination.DoNotUseFieldsMapCache = viewModel.WorkspaceHasChanged;
		destinationJson = JSON.stringify(destination);
		IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination = destinationJson;

		// going to the third step of relativity export provider
		prepareStepsFor('relativityExport');
	});

	//An event raised when a user clicks the Back button.
	message.subscribe('back', function () {
		//Execute save logic that persists the state.
		this.publish('saveState', JSON.stringify(ko.toJS(viewModel)));
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (m) {
		var _bind = function (m) {
			viewModel = new Model(m);
			ko.applyBindings(viewModel, document.getElementById('exportProviderConfiguration'));
		}

		// expect model to be serialized to string
		if (typeof m === "string") {
			try {
				m = JSON.parse(m);
			} catch (e) {
				m = undefined;
			}
			_bind(m);
		} else {
			_bind({});
		}

		// restore default steps configuration
		prepareStepsFor('relativity');
	});

	var Model = function (m) {

		var state = $.extend({}, {}, m);
		var self = this;

		this.workspaces = ko.observableArray(state.workspaces || []);
		this.savedSearches = ko.observableArray(state.savedSearches || []);

		this.HasBeenRun = ko.observable(IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun());

		this.Fileshare = ko.observable(state.Fileshare).extend({
			required: true
		});

		this.IncludeNativeFilesPath = ko.observable(state.IncludeNativeFilesPath || "true");
        
		this.SelectedDataFileFormat = ko.observable(state.SelectedDataFileFormat).extend({
			required: true
		});

		this.ColumnSeparator = ko.observable(state.ColumnSeparator).extend({
			required: {
				onlyIf: function () {
				    return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.QuoteSeparator = ko.observable(state.QuoteSeparator).extend({
			required: {
				onlyIf: function () {
				    return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.NewlineSeparator = ko.observable(state.NewlineSeparator).extend({
			required: {
				onlyIf: function () {
				    return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.MultiValueSeparator = ko.observable(state.MultiValueSeparator).extend({
			required: {
				onlyIf: function () {
				    return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.NestedValueSeparator = ko.observable(state.NestedValueSeparator).extend({
			required: {
				onlyIf: function () {
				    return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});

		this.isCustom = ko.observable(false);
		this.isCustomDisabled = ko.observable(true);

		this.separatorsList = function () {
			var result = [];
			for (var i = 0; i < 256; i++) {
				result.push({ key: String.fromCharCode(i) + " (ASCII:" + i + ")", value: i });
			}
			return result;
		}();

		this.SelectedDataFileFormat.subscribe(function (value) {
			//default values have been taken from RDC application
		    if (value === ExportEnums.DataFileFormatEnum.Concordance) {
				self.ColumnSeparator(20);
				self.QuoteSeparator(254);
				self.NewlineSeparator(174);
				self.MultiValueSeparator(59);
				self.NestedValueSeparator(92);
			}
		    if (value === ExportEnums.DataFileFormatEnum.CSV) {
				self.ColumnSeparator(44);
				self.QuoteSeparator(34);
				self.NewlineSeparator(10);
				self.MultiValueSeparator(59);
				self.NestedValueSeparator(92);
			}
		});

		this.SelectedDataFileFormat.subscribe(function (value) {
		    self.isCustom(value === ExportEnums.DataFileFormatEnum.Custom);
		    if (value === ExportEnums.DataFileFormatEnum.Custom) {
				self.isCustomDisabled(undefined);
			} else {
				self.isCustomDisabled(true);
			}
		});

		this.CopyFileFromRepository = ko.observable(state.CopyFileFromRepository || "false");
		this.CopyFileFromRepository.subscribe(function (value) {
			if (value === 'false') {
				self.SelectedImageFileType(0);
			}
		});

		this.OverwriteFiles = ko.observable(state.OverwriteFiles || "false");

		this.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId).extend({
			required: true
		});

		this.TargetWorkspaceArtifactId.subscribe(function (value) {
			if (self.TargetWorkspaceArtifactId !== value) {
				self.WorkspaceHasChanged = true;
			}
		});

		this.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId);

		this.SavedSearch = ko.observable(state.SavedSearch).extend({
			required: true
		});

		this.updateSelectedSavedSearch = function () {
			var selectedSavedSearch = ko.utils.arrayFirst(self.savedSearches(), function (item) {
				if (item.value === self.SavedSearchArtifactId()) {
					return item;
				}
			});

			self.SavedSearch(selectedSavedSearch);
		}

		if (self.savedSearches().length === 0) {
			// load saved searches
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('SavedSearchFinder') }).then(function (result) {
				self.savedSearches(result);
				self.updateSelectedSavedSearch();
			});
		} else {
			self.updateSelectedSavedSearch();
		}

		if (self.workspaces().length === 0) {
			// load workspaces
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('workspaceFinder') }).then(function (result) {
				self.workspaces(result);
			});
		}

		this.DataFileEncodingType = ko.observable().extend({
		    required: true
		});

		this.updateSelectedDataFileEncodingType = function (value) {
		    if (self.DataFileEncodingTypeList().length === 3) {
		        var ungroupedFileEncodingList = self.DataFileEncodingTypeList()[0].children()
																					.concat(self.DataFileEncodingTypeList()[1].children())
																					.concat(self.DataFileEncodingTypeList()[2].children())
		        var selectedDataFileEncodingType = ko.utils.arrayFirst(ungroupedFileEncodingList, function (item) {
		            return item.name === value;
		        });

		        self.DataFileEncodingType(selectedDataFileEncodingType.name);
		    }
		}

		this.DataFileEncodingTypeList = ko.observableArray([]);
		if (self.DataFileEncodingTypeList.length === 0) {
		    IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
		        function Group(label, children) {
		            this.label = ko.observable(label);
		            this.children = ko.observableArray(children);
		        }
		        var defaultOption = { displayName: "Select..", name: "" };
		        var favorite = [];
		        var others = [];
		        for (var i = 0; i < result.length; i++) {
		            if ($.inArray(result[i].name, ['utf-16', 'utf-16BE', 'utf-8', 'Windows-1252']) >= 0) {
		                favorite.push(result[i]);
		            }
		            else {
		                others.push(result[i]);
		            }
		        }
		        // By default user should see only 4 default options: Unicode, Unicode (Big-Endian), Unicode (UTF-8), Western European (Windows) as in RDC
		        self.DataFileEncodingTypeList([new Group("", [defaultOption]), new Group("Favorite", favorite), new Group("Others", others)]
                );
		        self.updateSelectedDataFileEncodingType(state.DataFileEncodingType || "");
		    });
		}
		else {
		    self.updateSelectedDataFileEncodingType(state.DataFileEncodingType);
		}

		this.ExportImagesChecked = ko.observable(state.ExportImagesChecked || "false").extend({
			required: true
		});
        
		this.SelectedImageDataFileFormat = ko.observable(state.SelectedImageDataFileFormat).extend({
			required: {
				onlyIf: function () {
					return self.ExportImagesChecked() === "true";
				}
			}
		});
        
		this.SelectedImageFileType = ko.observable(self.CopyFileFromRepository() === 'false' ? 0 : state.SelectedImageFileType).extend({
			required: {
				onlyIf: function () {
					return self.ExportImagesChecked() === "true";
				}
			}
		});

		this.SubdirectoryImagePrefix = ko.observable(state.SubdirectoryImagePrefix || "IMG").extend({
			required: true
		});
		this.SubdirectoryNativePrefix = ko.observable(state.SubdirectoryNativePrefix || "NATIVE").extend({
			required: true
		});
		this.SubdirectoryTextPrefix = ko.observable(state.SubdirectoryTextPrefix || "TEXT").extend({
			required: true
		});
		this.SubdirectoryStartNumber = ko.observable(state.SubdirectoryStartNumber || 1).extend({
			required: true
		});
		this.SubdirectoryDigitPadding = ko.observable(state.SubdirectoryDigitPadding || 3).extend({
			required: true
		});
		this.SubdirectoryMaxFiles = ko.observable(state.SubdirectoryMaxFiles || 500).extend({
			required: true
		});
		this.VolumePrefix = ko.observable(state.VolumePrefix || "VOL").extend({
			required: true
		});
		this.VolumeStartNumber = ko.observable(state.VolumeStartNumber || 1).extend({
			required: true
		});
		this.VolumeDigitPadding = ko.observable(state.VolumeDigitPadding || 2).extend({
			required: true
		});
		this.VolumeMaxSize = ko.observable(state.VolumeMaxSize || 650).extend({
			required: true
		});

		this.FilePath = ko.observable(state.FilePath || ExportEnums.FilePathTypeEnum.Relative).extend({
			required: true
		});

		this.UserPrefix = ko.observable(state.UserPrefix).extend({
			required: {
				onlyIf: function () {
				    return self.FilePath() == ExportEnums.FilePathTypeEnum.UserPrefix;
				}
			}
		});

		this.isUserPrefix = ko.observable(false);

		this.FilePath.subscribe(function (value) {
		    self.isUserPrefix(value == ExportEnums.FilePathTypeEnum.UserPrefix);
		});

		this.errors = ko.validation.group(this, { deep: true });

		this.getSelectedOption = function () {
			return {
				"SavedSearchArtifactId": self.SavedSearch().value,
				"SavedSearch": self.SavedSearch().displayName,
				"TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
				"SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
				"CopyFileFromRepository": self.CopyFileFromRepository(),
				"OverwriteFiles": self.OverwriteFiles(),
				"Fileshare": self.Fileshare(),
				"ExportImagesChecked": self.ExportImagesChecked(),
				"SelectedImageFileType": self.SelectedImageFileType(),
				"IncludeNativeFilesPath": self.IncludeNativeFilesPath(),
				"SelectedDataFileFormat": self.SelectedDataFileFormat(),
				"DataFileEncodingType": self.DataFileEncodingType(),
				"SelectedImageDataFileFormat": self.SelectedImageDataFileFormat(),
				"ColumnSeparator": self.ColumnSeparator(),
				"QuoteSeparator": self.QuoteSeparator(),
				"NewlineSeparator": self.NewlineSeparator(),
				"MultiValueSeparator": self.MultiValueSeparator(),
				"NestedValueSeparator": self.NestedValueSeparator(),
				"SubdirectoryImagePrefix": self.SubdirectoryImagePrefix(),
				"SubdirectoryNativePrefix": self.SubdirectoryNativePrefix(),
				"SubdirectoryTextPrefix": self.SubdirectoryTextPrefix(),
				"SubdirectoryStartNumber": self.SubdirectoryStartNumber(),
				"SubdirectoryDigitPadding": self.SubdirectoryDigitPadding(),
				"SubdirectoryMaxFiles": self.SubdirectoryMaxFiles(),
				"VolumePrefix": self.VolumePrefix(),
				"VolumeStartNumber": self.VolumeStartNumber(),
				"VolumeDigitPadding": self.VolumeDigitPadding(),
				"VolumeMaxSize": self.VolumeMaxSize(),
				"FilePath": self.FilePath(),
				"UserPrefix": self.UserPrefix()
			}
		}
	}
});
