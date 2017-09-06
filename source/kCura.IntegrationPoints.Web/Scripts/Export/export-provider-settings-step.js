var IP = IP || {};

(function (root, ko) {

	var ExportDestinationLocationService = function () {
		var self = this;

		self.isProcessingSourceLocationEnabled = function (successCallback, failCallback) {
			root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("ResourcePool/IsProcessingSourceLocationEnabled")
			}).then(successCallback).fail(failCallback);
		};

		self.loadRootDataTransferLocation = function (integrationPointTypeIdentifier, successCallback, failCallback) {
			root.data.ajax({
				type: "post",
				contentType: "application/x-www-form-urlencoded; charset=UTF-8",
				url: root.utils.generateWebAPIURL("DataTransferLocation/GetRoot", integrationPointTypeIdentifier)
			}).then(successCallback).fail(failCallback);
		};

		self.getProcessingSourceLocationSubItems = function (path, isRoot, successCallback, failCallback) {
			root.data.ajax({
				type: "post",
				contentType: "application/json",
				url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationSubItems", isRoot),
				data: JSON.stringify(path)
			}).then(successCallback).fail(failCallback);
		};

		self.getFileshareSubItems = function (path, isRoot, integrationPointTypeIdentifier, successCallback, failCallback) {
			root.data.ajax({
				type: "post",
				contentType: "application/json",
				url: root.utils.generateWebAPIURL("DataTransferLocation/GetStructure", integrationPointTypeIdentifier) + "?isRoot=" + isRoot,
				data: JSON.stringify(path)
			}).then(successCallback).fail(failCallback);
		}

		return {
			isProcessingSourceLocationEnabled: self.isProcessingSourceLocationEnabled,
			loadRootDataTransferLocation: self.loadRootDataTransferLocation,
			getProcessingSourceLocationSubItems: self.getProcessingSourceLocationSubItems,
			getFileshareSubItems: self.getFileshareSubItems
		}
	};

	var ExportDestinationLocationViewModel = function (state) {
		var FILESHARE_EXPORT_LOCATION_ARTIFACT_ID = -1;
		var self = this;
		self.ExportDestinationLocationService = new ExportDestinationLocationService();

		self.rootDataTransferLocation = "";

		self.ProcessingSourceLocationList = ko.observableArray([]);

		self.ProcessingSourceLocation = ko.observable().extend({
			required: true
		});

		self.IsProcessingSourceLocationEnabled = ko.observable(false);

		self.IsExportFolderCreationEnabled = ko.observable(state.IsAutomaticFolderCreationEnabled === undefined ? true : state.IsAutomaticFolderCreationEnabled);
		self.IsExportFolderCreationEnabled.subscribe(function () {
			self.fileShareDisplayText();
		});

		self.Fileshare = ko.observable(state.Fileshare).extend({
			required: true
		});

		self.fileShareDisplayText = function () {
			var fileshare = self.Fileshare();
			if (!fileshare) {
				return "Select...";
			}

			var output;
			if (self.isProcessingSourceLocationSelected()) {
				output = fileshare;
			}
			else {
				output = "EDDS" + state.SourceWorkspaceArtifactId + "\\" + fileshare;
			}

			if (self.IsExportFolderCreationEnabled()) {
				output += "\\" + state.name + "_{TimeStamp}";
			}

			return output;
		};

		self.updateProcessingSourceLocation = function (value, isInitializationCall) {
			var disableDirectorySelector = function () {
				self.locationSelector.toggle(false);
				self.Fileshare(null);
			};

			var enableDirectorySelector = function () {
				self.locationSelector.toggle(true);
			};

			self.locationSelector.clear();

			if (value === null) {
				disableDirectorySelector();
				return;
			}

			if (!isInitializationCall) {
				self.Fileshare(null);
			}

			self.loadDirectories();

			if (!value) {
				disableDirectorySelector();
			}
			else {
				enableDirectorySelector();
			}
		};

		self.onLoadded = function () {
			var s = function (result) {
				if (result) {
					self.IsProcessingSourceLocationEnabled(true);
				}
			};
			var f = function () {

			};
			self.ExportDestinationLocationService.isProcessingSourceLocationEnabled(s, f);


			self.InitializeLocationSelector();
			self.loadRootDataTransferLocation();
		};

		self.InitializeLocationSelector = function() {
			self.locationSelector = new LocationJSTreeSelector();
			self.locationSelector.init(self.Fileshare(),
				[],
				{
					onNodeSelectedEventHandler: function(node) {
						self.Fileshare(node.id);
					}
				});
		};

		self.loadRootDataTransferLocation = function() {
			var success = function(result) {
				self.rootDataTransferLocation = result;
				self.loadDirectories();
				self.loadProcessingSourceLocations();
			};

			var fail = function(error) {
				IP.message.error.raise("Can not retrieve data transfer location root path");
			};
			self.ExportDestinationLocationService.loadRootDataTransferLocation(state.integrationPointTypeIdentifier,
				success,
				fail);
		};

		self.loadProcessingSourceLocations = function () {
			if (!self.IsProcessingSourceLocationEnabled()) {
				var locations = [];
				var fileShareExportLocation = self.createProcessingSourceListItemForFileshare();
				locations.push(fileShareExportLocation);
				self.xyz(locations, FILESHARE_EXPORT_LOCATION_ARTIFACT_ID);
				return;
			}


			var processingSourceLocationListPromise = root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocations"),
				data: {
					sourceWorkspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
				}
			}).fail(function(error) {
				IP.message.error.raise("No processing source locations were returned from source provider");
			});

			root.data.deferred()
				.all([processingSourceLocationListPromise])
				.then(function(result) {
					var locations = result[0];
					var fileShareExportLocation = self.createProcessingSourceListItemForFileshare();
					locations.unshift(fileShareExportLocation);

					var initialProcessingSourceLocationArtifactId = self.getInitialProcessingSourceLocationArtifactId();
					self.xyz(locations, initialProcessingSourceLocationArtifactId);
				});
		};

		self.xyz = function (locations, selectedLocationId) { // TODO rename
			self.ProcessingSourceLocationList(locations);

			self.ProcessingSourceLocation(selectedLocationId);
			self.ProcessingSourceLocation.isModified(false);
			self.updateProcessingSourceLocation(self.ProcessingSourceLocation(), true);

			self.ProcessingSourceLocation.subscribe(function (value) {
				self.updateProcessingSourceLocation(value);
			});
		};

		self.loadDirectories = function () {
			var processingSourceLocationArtifactId = self.ProcessingSourceLocation();
			if (!processingSourceLocationArtifactId) {
				return;
			}
			var processingSourceLocation = self.getSelectedProcessingSourceLocationViewModel(processingSourceLocationArtifactId);
			var $locationErrorContainer = $("#processingLocationErrorContainer");

			var createErrorCallback = function (callback) {
				return function (error) {
					callback(error);
					IP.message.error.raise(error, $locationErrorContainer);
				};
			};

			var reloadTreeProcessingSourceLocation = function (params, onSuccess, onFail) {
				IP.message.error.clear($locationErrorContainer);

				var isRoot = params.id === '#';
				var path = params.id;
				if (isRoot) {
					path = processingSourceLocation.location;
				}

				self.ExportDestinationLocationService.getProcessingSourceLocationSubItems(path, isRoot, onSuccess, createErrorCallback(onFail));
			};

			var reloadTreeFileshare = function (params, onSuccess, onFail) {
				IP.message.error.clear($locationErrorContainer);

				var isRoot = params.id === '#';
				var path = params.id;
				if (isRoot) {
					path = self.rootDataTransferLocation;
				}

				self.ExportDestinationLocationService.getFileshareSubItems(path, isRoot, state.integrationPointTypeIdentifier, onSuccess, createErrorCallback(onFail));
			};

			if (processingSourceLocation.isFileshare) {
				self.locationSelector.reloadWithRoot(reloadTreeFileshare);
			} else {
				self.locationSelector.reloadWithRoot(reloadTreeProcessingSourceLocation);
			}
		};

		self.isProcessingSourceLocationSelected = function () {
			var processingSourceLocationId = self.ProcessingSourceLocation();
			if (processingSourceLocationId) {
				var psl = self.getSelectedProcessingSourceLocationViewModel(processingSourceLocationId);
				return !!psl && !psl.isFileshare;
			}
			return false;
		};

		self.getSelectedProcessingSourceLocationViewModel = function (artifactId) {
			var selectedPath = ko.utils.arrayFirst(self.ProcessingSourceLocationList(), function (item) {
				if (item.artifactId === artifactId) {
					return item;
				}
			});
			return selectedPath;
		};

		self.createProcessingSourceListItemForFileshare = function () {
			return {
				artifactId: FILESHARE_EXPORT_LOCATION_ARTIFACT_ID,
				location: ".\\EDDS" + state.SourceWorkspaceArtifactId + "\\" + self.rootDataTransferLocation,
				isFileshare: true
			};
		};

		self.getInitialProcessingSourceLocationArtifactId = function () {
			if (state.ProcessingSourceLocation) {
				return state.ProcessingSourceLocation;
			} else if (state.Fileshare) { // case when user created IP before PSL support was added
				return FILESHARE_EXPORT_LOCATION_ARTIFACT_ID;
			}
		};
	}

	var viewModel = function (state) {
		var self = this;
		this.exportDestinationLocationViewModel = new ExportDestinationLocationViewModel(state);

		this.IPName = state.name;
		this.ArtifactTypeID = state.artifactTypeId;
		this.DefaultRdoTypeId = state.defaultRdoTypeId;

		this.ExportRdoMode = function () {
			return self.ArtifactTypeID !== self.DefaultRdoTypeId;
		}

		this.onDOMLoaded = function () {
			self.exportDestinationLocationViewModel.onLoadded();
		};

		this.SelectedDataFileFormat = ko.observable(state.SelectedDataFileFormat || ExportEnums.Defaults.DataFileFormatValue).extend({
			required: true
		});

		this.ColumnSeparator = ko.observable(state.ColumnSeparator || 20).extend({
			required: {
				onlyIf: function () {
					return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.QuoteSeparator = ko.observable(state.QuoteSeparator || 254).extend({
			required: {
				onlyIf: function () {
					return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.NewlineSeparator = ko.observable(state.NewlineSeparator || 174).extend({
			required: {
				onlyIf: function () {
					return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.MultiValueSeparator = ko.observable(state.MultiValueSeparator || 59).extend({
			required: {
				onlyIf: function () {
					return self.SelectedDataFileFormat() === ExportEnums.DataFileFormatEnum.Custom;
				}
			}
		});
		this.NestedValueSeparator = ko.observable(state.NestedValueSeparator || 92).extend({
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

		self.UpdateIsCustomDataFileFormatChanged = function (value) {
			self.isCustom(value === ExportEnums.DataFileFormatEnum.Custom);
			if (value === ExportEnums.DataFileFormatEnum.Custom) {
				self.isCustomDisabled(undefined);
			} else {
				self.isCustomDisabled(true);
			}
		};
		self.UpdateIsCustomDataFileFormatChanged(state.SelectedDataFileFormat);

		this.SelectedDataFileFormat.subscribe(self.UpdateIsCustomDataFileFormatChanged);

		this.ExportNatives = ko.observable(state.ExportNatives || false);
		this.ExportNatives.subscribe(function (value) {
			if (!value) {
				self.SelectedImageFileType(0);
				self.IncludeNativeFilesPath(self.IncludeNativeFilesPathLastValue);
			}
			if (value) {
				self.IncludeNativeFilesPathLastValue = self.IncludeNativeFilesPath();
				self.IncludeNativeFilesPath(true);
			}
		});

		this.ExportTextFieldsAsFiles = ko.observable(state.ExportFullTextAsFile || false);

		this.OverwriteFiles = ko.observable(state.OverwriteFiles || false);

		this.DataFileEncodingTypeValue = state.DataFileEncodingType || ExportEnums.Defaults.EncodingValue;

		this.DataFileEncodingType = ko.observable(self.DataFileEncodingTypeValue).extend({
			required: true
		});

		this.TextFileEncodingTypeValue = state.TextFileEncodingType || ExportEnums.Defaults.EncodingValue;

		this.TextFileEncodingType = ko.observable(self.TextFileEncodingTypeValue).extend({
			required: {
				onlyIf: function () {
					return self.ExportTextFieldsAsFiles();
				}
			}
		});

		this.FileEncodingTypeList = ko.observableArray([]);
		this._UpdateFileEncodingTypeList = function () {
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
				function Group(label, children) {
					this.label = ko.observable(label);
					this.children = ko.observableArray(children);
				};
				function Option(displayName, name) {
					this.displayName = ko.observable(displayName);
					this.name = ko.observable(name);
				};

				var favorite = [];
				var others = [];

				for (var i = 0; i < result.length; i++) {
					var option = new Option(result[i].displayName, result[i].name);

					if ($.inArray(result[i].name, ['utf-16', 'utf-16BE', 'utf-8', 'Windows-1252']) >= 0) {
						favorite.push(option);
					} else {
						others.push(option);
					}
				}

				// By default user should see only 4 default options: Unicode, Unicode (Big-Endian), Unicode (UTF-8), Western European (Windows) as in RDC
				self.FileEncodingTypeList([new Group("", [new Option("Select...", "")]), new Group("Favorite", favorite), new Group("Others", others)]);

				self.DataFileEncodingType(self.DataFileEncodingTypeValue);
				self.DataFileEncodingType.isModified(false);

				self.TextFileEncodingType(self.TextFileEncodingTypeValue);
				self.TextFileEncodingType.isModified(false);
			});
		}

		self._UpdateFileEncodingTypeList();

		this.ExportImages = ko.observable(state.ExportImages || false);

		this.SelectedImageDataFileFormat = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.ExportImages();
				}
			}
		});

		this.ImageFileFormatList = ko.observableArray([]);

		this._updateImageFileFormat = function () {
			var setSelectedImageDataFileFormat = function () {
				if (state.SelectedImageDataFileFormat === 0) {
					self.SelectedImageDataFileFormat("0");
				}
				else if (state.SelectedImageDataFileFormat === undefined) {
					self.SelectedImageDataFileFormat(self.ImageFileFormatList()[0].value.toString());
				}
				else {
					self.SelectedImageDataFileFormat(state.SelectedImageDataFileFormat.toString());
				}
			}

			var formats = [ExportEnums.ImageDataFileFormats[0], ExportEnums.ImageDataFileFormats[1], ExportEnums.ImageDataFileFormats[2]];
			if (self.ExportImages()) {
				var defaultOption = { key: "Select...", value: "" };
				self.ImageFileFormatList([defaultOption].concat(formats));

				setSelectedImageDataFileFormat();

				self.SelectedImageDataFileFormat.isModified(false);
			}
			else {
				self.ImageFileFormatList([ExportEnums.ImageDataFileFormats[3]].concat(formats));

				setSelectedImageDataFileFormat();
			}
		}

		this.IsProductionExport = ko.observable(state.ExportType === ExportEnums.SourceOptionsEnum.Production);

		this.AppendOriginalFileName = ko.observable(state.AppendOriginalFileName || false);

		this.SelectedExportNativesWithFileNameFrom = ko.observable(state.ExportNativesToFileNamedFrom || false).extend({
			required: {
				onlyIf: function () {
					return self.AppendOriginalFileName();
				}
			}
		});

		this.IsCustomFileNameOptionSelected = function () {
			return self.SelectedExportNativesWithFileNameFrom() === ExportEnums.ExportNativeWithFilenameFromTypesEnum.Custom;
		};


		this.ExportImages.subscribe(self._updateImageFileFormat);

		self._updateImageFileFormat();

		this.ProductionPrecedence = ko.observable(state.ProductionPrecedence).extend({
			required: {
				onlyIf: function () {
					return self.ExportImages();
				}
			}
		});

		this.ProductionPrecedence.subscribe(function (value) {
			if (!self.IsProductionExport()) {
				if (value === ExportEnums.ProductionPrecedenceTypeEnum.Produced) {
					self.SelectedExportNativesWithFileNameFrom(ExportEnums.ExportNativeWithFilenameFromTypesEnum.BeginProductionNumber);
				}
				else {
					self.SelectedExportNativesWithFileNameFrom(ExportEnums.ExportNativeWithFilenameFromTypesEnum.Identifier);
				}
			}
		});

		this.IsProductionPrecedenceSelected = function () {
			return self.ProductionPrecedence() === ExportEnums.ProductionPrecedenceTypeEnum.Produced;
		}

		this.IncludeOriginalImages = ko.observable(state.IncludeOriginalImages || false);

		this.SelectedImageFileType = ko.observable(!self.ExportNatives() ? 0 : state.SelectedImageFileType).extend({
			required: {
				onlyIf: function () {
					return self.ExportImages();
				}
			}
		});

		this.SubdirectoryImagePrefix = ko.observable(state.SubdirectoryImagePrefix || "IMG").extend({
			required: {
				onlyIf: function () {
					return self.ExportImages();
				}
			},
			maxLength: {
				onlyIf: function () {
					return self.ExportImages();
				},
				params: 256
			},
			textFieldWithoutSpecialCharacters: {
				onlyIf: function () {
					return self.ExportImages();
				}
			}
		});
		this.SubdirectoryNativePrefix = ko.observable(state.SubdirectoryNativePrefix || "NATIVE").extend({
			required: {
				onlyIf: function () {
					return self.ExportNatives();
				}
			},
			maxLength: {
				onlyIf: function () {
					return self.ExportNatives();
				},
				params: 256
			},
			textFieldWithoutSpecialCharacters: {
				onlyIf: function () {
					return self.ExportNatives();
				}
			}
		});
		this.SubdirectoryTextPrefix = ko.observable(state.SubdirectoryTextPrefix || "TEXT").extend({
			required: {
				onlyIf: function () {
					return self.ExportTextFieldsAsFiles();
				}
			},
			maxLength: {
				onlyIf: function () {
					return self.ExportTextFieldsAsFiles();
				},
				params: 256
			},
			textFieldWithoutSpecialCharacters: {
				onlyIf: function () {
					return self.ExportTextFieldsAsFiles();
				}
			}
		});

		this.IsVolumeAndSubdirectioryDetailVisible = function () {
			return self.ExportImages() || self.ExportNatives() || self.ExportTextFieldsAsFiles();
		}

		this.SubdirectoryStartNumber = ko.observable(state.SubdirectoryStartNumber || 1).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.SubdirectoryDigitPadding = ko.observable(state.SubdirectoryDigitPadding || 3).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			max: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 256
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.SubdirectoryMaxFiles = ko.observable(state.SubdirectoryMaxFiles || 500).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			max: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 2000000
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.VolumePrefix = ko.observable(state.VolumePrefix || "VOL").extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			maxLength: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 256
			},
			textFieldWithoutSpecialCharacters: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.VolumeStartNumber = ko.observable(state.VolumeStartNumber || 1).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.VolumeDigitPadding = ko.observable(state.VolumeDigitPadding || 2).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			max: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 256
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});
		this.VolumeMaxSize = ko.observable(state.VolumeMaxSize || 4400).extend({
			required: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
			},
			min: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				},
				params: 1
			},
			nonNegativeNaturalNumber: {
				onlyIf: function () {
					return self.IsVolumeAndSubdirectioryDetailVisible();
				}
			}
		});

		function pad(str, max) {
			return str.length < max ? pad("0" + str, max) : str;
		};

		this.SubdirectoryDigitText = ko.computed(function () {
			return pad(self.SubdirectoryStartNumber().toString(), parseInt(self.SubdirectoryDigitPadding()));
		}, this);

		this.FilePath = ko.observable(state.FilePath || ExportEnums.FilePathTypeEnum.Relative).extend({
			required: true
		});

		this.UserPrefix = ko.observable(state.UserPrefix || "").extend({
			required: {
				onlyIf: function () {
					return self.FilePath() == ExportEnums.FilePathTypeEnum.UserPrefix;
				}
			},
			maxLength: {
				onlyIf: function () {
					return self.FilePath() == ExportEnums.FilePathTypeEnum.UserPrefix;
				},
				params: 256
			},
			textFieldWithoutSpecialCharacters: {
				onlyIf: function () {
					return self.FilePath() == ExportEnums.FilePathTypeEnum.UserPrefix;
				}
			}
		});

		this.isUserPrefix = ko.observable((self.FilePath() == ExportEnums.FilePathTypeEnum.UserPrefix));

		this.FilePath.subscribe(function (value) {
			self.isUserPrefix(value == ExportEnums.FilePathTypeEnum.UserPrefix);
			if (value === ExportEnums.FilePathTypeEnum.UserPrefix) {
				self.isUserPrefix(true);

				if (self.UserPrefix() !== "") {
					self.UserPrefix.notifySubscribers();
				}
			} else {
				self.isUserPrefix(false);
			}
		});

		this.IncludeNativeFilesPath = ko.observable(state.IncludeNativeFilesPath || false);
		this.IncludeNativeFilesPathLastValue = this.IncludeNativeFilesPath();

		this.ExportMultipleChoiceFieldsAsNested = ko.observable(state.ExportMultipleChoiceFieldsAsNested || false);

		var getTextRepresentation = function (value) {
			if (!value || value.length === 0) {
				return "Select...";
			}

			return value.map(function (x) {
				return x.displayName;
			}).join("; ");
		};
		this.TextPrecedenceFields = ko.observable(state.TextPrecedenceFields || [])
			.extend({
				required: {
					onlyIf: function () {
						return self.ExportTextFieldsAsFiles();
					}
				}
			});

		this.TextPrecedenceSelection = ko.pureComputed(function () {
			return getTextRepresentation(self.TextPrecedenceFields());
		});

		var textPrecedencePickerViewModel = new TextPrecedencePickerViewModel(function (fields) {
			self.TextPrecedenceFields(fields);
		}, self.ArtifactTypeID);

		Picker.create("Fileshare", "textPrecedencePicker", "ListPicker", textPrecedencePickerViewModel);

		this.openTextPrecedencePicker = function () {
			textPrecedencePickerViewModel.open(self.TextPrecedenceFields());
		};

		var exportDetailsTooltipViewModel = new TooltipViewModel(TooltipDefs.ExportDetails, TooltipDefs.ExportDetailsTitle);

		Picker.create("Tooltip", "tooltipExportTypeId", "TooltipView", exportDetailsTooltipViewModel);

		this.openExportDetailsTooltip = function (data, event) {
			exportDetailsTooltipViewModel.open(event);
		};

		this.ImagePrecedence = ko.observable(state.ImagePrecedence || [])
			.extend({
				required: {
					onlyIf: function () {
						return self.ExportImages() && self.IsProductionPrecedenceSelected();
					}
				}
			});

		this.ImagePrecedenceSelection = ko.pureComputed(function () {
			return getTextRepresentation(self.ImagePrecedence());
		});

		var imageProductionPickerViewModel = new ImageProductionPickerViewModel(function (productions) {
			self.ImagePrecedence(productions);
		});

		Picker.create("Fileshare", "imageProductionPicker", "ListPicker", imageProductionPickerViewModel);

		this.openImageProductionPicker = function () {
			imageProductionPickerViewModel.open(self.ImagePrecedence());
		};

		var availableFields = state.availableFields || [];

		var getDefaultFileSelections = function (availFields) {
			var field = availFields[0];
			for (var index = 0; index < availFields.length; ++index) {
				if (availFields[index].isIdentifier === true) {
					field = availFields[index];
					break;
				}
			}
			return [new FileNameEntry(field.displayName, field.fieldIdentifier, "F")];
		}

		self.FileNameParts = ko.observable(state.FileNameParts || getDefaultFileSelections(availableFields));

		var exportHelper = new ExportHelper();

		var getFileNameSelectionRepresentation = function () {
			var fileNameParts = self.FileNameParts();
			return exportHelper.convertFileNamePartsToText(fileNameParts);
		};

		self.exportFileNameViewModel = new ExportProviderFileNameViewModel(availableFields, function (selectedFileNameParts) {
			self.FileNameParts(selectedFileNameParts);
		});

		self.FileNameSelection = ko.pureComputed(function () {
			return getFileNameSelectionRepresentation(self.FileNameParts()) + ".{File Extension}";
		});

		Picker.create("Modals", "file-naming-option-modal", "ExportFileNamingOptionView", self.exportFileNameViewModel,
			{
				autoOpen: false,
				modal: false,
				width: "800px",
				modal: true,
				"min-width": "1000px",
				height: "auto",
				width: "auto",
				resizable: false,
				draggable: false,
				closeOnEscape: true,
				position: {
					my: "center",
					at: "center"
				}
			}
		);
		this.openFileNamingOptionsPicker = function () {
			self.exportFileNameViewModel.open(self.FileNameParts());
		}

		this.errors = ko.validation.group(this, { deep: true });

		this.getSelectedOption = function () {
			var processingSourceLocation = self.exportDestinationLocationViewModel.ProcessingSourceLocation();
			return {
				"AppendOriginalFileName": self.AppendOriginalFileName(),
				"ColumnSeparator": self.ColumnSeparator(),
				"ExportNatives": self.ExportNatives(),
				"ExportNativesToFileNamedFrom": self.SelectedExportNativesWithFileNameFrom(),
				"DataFileEncodingType": self.DataFileEncodingType(),
				"ExportFullTextAsFile": self.ExportTextFieldsAsFiles(),
				"ExportImages": self.ExportImages(),
				"ExportMultipleChoiceFieldsAsNested": self.ExportMultipleChoiceFieldsAsNested(),
				"FilePath": self.FilePath(),
				"Fileshare": self.exportDestinationLocationViewModel.Fileshare(),
				"ImagePrecedence": self.ImagePrecedence(),
				"IncludeOriginalImages": self.IncludeOriginalImages(),
				"MultiValueSeparator": self.MultiValueSeparator(),
				"NestedValueSeparator": self.NestedValueSeparator(),
				"NewlineSeparator": self.NewlineSeparator(),
				"OverwriteFiles": self.OverwriteFiles(),
				"ProductionPrecedence": self.ProductionPrecedence(),
				"ProcessingSourceLocation": processingSourceLocation !== undefined ? processingSourceLocation : null,
				"QuoteSeparator": self.QuoteSeparator(),
				"SelectedDataFileFormat": self.SelectedDataFileFormat(),
				"SelectedImageDataFileFormat": self.SelectedImageDataFileFormat(),
				"SelectedImageFileType": self.SelectedImageFileType(),
				"SubdirectoryDigitPadding": self.SubdirectoryDigitPadding(),
				"SubdirectoryImagePrefix": self.SubdirectoryImagePrefix(),
				"SubdirectoryMaxFiles": self.SubdirectoryMaxFiles(),
				"SubdirectoryNativePrefix": self.SubdirectoryNativePrefix(),
				"SubdirectoryStartNumber": self.SubdirectoryStartNumber(),
				"SubdirectoryTextPrefix": self.SubdirectoryTextPrefix(),
				"TextFileEncodingType": self.TextFileEncodingType(),
				"TextPrecedenceFields": self.TextPrecedenceFields(),
				"UserPrefix": self.UserPrefix(),
				"VolumeDigitPadding": self.VolumeDigitPadding(),
				"VolumeMaxSize": self.VolumeMaxSize(),
				"VolumePrefix": self.VolumePrefix(),
				"VolumeStartNumber": self.VolumeStartNumber(),
				"IncludeNativeFilesPath": self.IncludeNativeFilesPath(),
				"IsAutomaticFolderCreationEnabled": self.exportDestinationLocationViewModel.IsExportFolderCreationEnabled(),
				"FileNameParts": self.FileNameParts()
			};
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
				var detailsLoaded = 'details-loaded';
				root.messaging.publish(detailsLoaded);
			});
		}

		self.ipModel = {};
		self.model = {};

		self.loadModel = function (ip) {
			self.ipModel = ip;

			self.model = new viewModel($.extend({}, self.ipModel.sourceConfiguration,
				{
					artifactTypeId: ip.artifactTypeID,
					defaultRdoTypeId: ip.DefaultRdoTypeId,
					integrationPointTypeIdentifier: ip.IntegrationPointTypeIdentifier,
					name: ip.name,
					isExportFolderCreationEnabled: ip.sourceConfiguration.IsAutomaticFolderCreationEnabled,
					availableFields: ip.fileNamingFieldsList
				}));

			self.model.errors = ko.validation.group(self.model);
		};

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				var settings = self.model.getSelectedOption();

				if (settings.TextFileEncodingType === 'Select...') {
					settings.TextFileEncodingType = '';
				}

				if (settings.DataFileEncodingType === 'Select...') {
					settings.DataFileEncodingType = '';
				}

				if (typeof (self.ipModel.sourceConfiguration) === 'string') {
					self.ipModel.sourceConfiguration = JSON.parse(self.ipModel.sourceConfiguration);
				}

				if (typeof (self.ipModel.map) === 'string') {
					self.ipModel.map = JSON.parse(self.ipModel.map);
				}

				$.extend(self.ipModel.sourceConfiguration, settings);
				self.ipModel.sourceConfiguration.TargetWorkspaceArtifactId = self.ipModel.sourceConfiguration.SourceWorkspaceArtifactId; // this is needed as long as summary page displays destination workspace

				self.ipModel.sourceConfiguration = JSON.stringify(self.ipModel.sourceConfiguration);

				var destination = JSON.parse(self.ipModel.destination);
				destination.Provider = "Load File";
				destination.DoNotUseFieldsMapCache = false;
				self.ipModel.destination = JSON.stringify(destination);

				self.ipModel.map = JSON.stringify(self.ipModel.map);

				Picker.closeDialog("textPrecedencePicker");
				Picker.closeDialog("imageProductionPicker");

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				root.message.error.raise("Resolve all errors before proceeding");
				d.reject();
			}

			return d.promise;
		};

		self.back = function () {
			var d = root.data.deferred().defer();

			$.extend(self.ipModel.sourceConfiguration, self.model.getSelectedOption());

			Picker.closeDialog("textPrecedencePicker");
			Picker.closeDialog("imageProductionPicker");

			d.resolve(self.ipModel);

			return d.promise;
		}

		self.validate = function (model) {
			var d = root.data.deferred().defer();

			IP.data.ajax({
				type: 'POST',
				url: IP.utils.generateWebAPIURL('ExportSettingsValidation/ValidateSettings'),
				data: JSON.stringify(model)
			}).then(function (result) {
				if (!result.isValid) {
					var formattedMessage = result.messages.join('\r?\n').replace(new RegExp('\r?\n', 'g'), '.<br />');
					window.Dragon.dialogs.showConfirmWithCancelHandler({
						message: formattedMessage,
						title: 'Integration Point Validation',
						showCancel: true,
						width: 450,
						success: function (calls) {
							calls.close();
							d.resolve(true);
						},
						cancel: function (calls) {
							calls.close();
							d.resolve(false);
						}
					});
				} else {
					d.resolve(true);
				}
			}).fail(function (error) {
				d.reject(error);
			});

			return d.promise;
		}
	};

	var step = new stepModel({
		url: IP.utils.generateWebURL('IntegrationPoints', 'ExportProviderSettings'),
		templateID: 'exportProviderSettingsStep',
		isForRelativityExport: true
	});

	root.points.steps.push(step);
})(IP, ko);