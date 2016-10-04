var IP = IP || {};

(function (root, ko) {

	var viewModel = function (state) {
		var self = this;

		this.HasBeenRun = ko.observable(state.hasBeenRun || false);

		this.ProcessingSourceLocationList = ko.observableArray([]);

		this.ProcessingSourceLocationArtifactId = state.ProcessingSourceLocation || 0;

		this.ProcessingSourceLocation = ko.observable(self.ProcessingSourceLocationArtifactId).extend({
			required: true
		});

		this.ProcessingSourceLocation.isModified(false);

		this.updateProcessingSourceLocation = function (value) {
			self.Fileshare(undefined);
			self.Fileshare.isModified(false);

			if (self.locationSelector) {
				self.locationSelector.clear();
			}

			if (!!value) {
				self.getDirectories(value);
			}

			self.toggleLocation(!!value);
		};

		this.Fileshare = ko.observable(state.Fileshare).extend({
			required: {
				onlyIf: function () {
					return self.ProcessingSourceLocation();
				}
			}
		});

		this.onDOMLoaded = function () {
			if (self.HasBeenRun()) {
				self.toggleLocation(false);
			} else {
				self.locationSelector = new LocationJSTreeSelector();
				self.locationSelector.init(self.Fileshare(), [], {
					onNodeSelectedEventHandler: function (node) { self.Fileshare(node.id) }
				});

				self.toggleLocation(!!self.ProcessingSourceLocation());
			}

			self.ProcessingSourceLocation.isModified(false);
		};

		this.getDirectories = function (artifacId) {
			root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure", artifacId),
				data: {
					sourceWorkspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
				}
			}).then(function (result) {
				self.locationSelector.reload(result);
			}).fail(function (error) {
				root.message.error.raise(error);
				self.toggleLocation(false);
			});
		};

		this.toggleLocation = function (enabled) {
			var $el = $("#location-select");
			$el.toggleClass('location-disabled', !enabled);
			$el.children().each(function (i, e) {
				$(e).toggleClass('location-disabled', !enabled);
			});
		};

		this.SelectedDataFileFormat = ko.observable(state.SelectedDataFileFormat).extend({
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
		} ();

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

		this.DataFileEncodingTypeValue = state.DataFileEncodingType || "";

		this.DataFileEncodingType = ko.observable(self.DataFileEncodingTypeValue).extend({
			required: true
		});

		this.TextFileEncodingTypeValue = state.TextFileEncodingType || "";

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

		this.ExportImages.subscribe(self._updateImageFileFormat);

		self._updateImageFileFormat();

		this.ProductionPrecedence = ko.observable(state.ProductionPrecedence).extend({
			required: {
				onlyIf: function () {
					return self.ExportImages();
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
			if (!value) {
				return "";
			}

			return value.map(function (x) {
				return x.displayName;
			}).join(", ");
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
		});

		Picker.create("textPrecedencePicker", "ListPicker", textPrecedencePickerViewModel);

		this.openTextPrecedencePicker = function () {
			textPrecedencePickerViewModel.open(self.TextPrecedenceFields());
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

		Picker.create("imageProductionPicker", "ListPicker", imageProductionPickerViewModel);

		this.openImageProductionPicker = function () {
			imageProductionPickerViewModel.open(self.ImagePrecedence());
		};

		this.errors = ko.validation.group(this, { deep: true });

		this.getSelectedOption = function () {
			return {
				"AppendOriginalFileName" : self.AppendOriginalFileName(),
				"ColumnSeparator": self.ColumnSeparator(),
				"ExportNatives": self.ExportNatives(),
				"ExportNativesToFileNamedFrom": self.SelectedExportNativesWithFileNameFrom(),
				"DataFileEncodingType": self.DataFileEncodingType(),
				"ExportFullTextAsFile": self.ExportTextFieldsAsFiles(),
				"ExportImages": self.ExportImages(),
				"ExportMultipleChoiceFieldsAsNested": self.ExportMultipleChoiceFieldsAsNested(),
				"FilePath": self.FilePath(),
				"Fileshare": self.Fileshare(),
				"ProcessingSourceLocation": self.ProcessingSourceLocation(),
				"ImagePrecedence": self.ImagePrecedence(),
				"IncludeOriginalImages": self.IncludeOriginalImages(),
				"MultiValueSeparator": self.MultiValueSeparator(),
				"NestedValueSeparator": self.NestedValueSeparator(),
				"NewlineSeparator": self.NewlineSeparator(),
				"OverwriteFiles": self.OverwriteFiles(),
				"ProductionPrecedence": self.ProductionPrecedence(),
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
				"IncludeNativeFilesPath": self.IncludeNativeFilesPath()
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

			self.model = new viewModel($.extend({}, self.ipModel.sourceConfiguration, { hasBeenRun: ip.hasBeenRun }));
			self.model.errors = ko.validation.group(self.model);

			var processingSourceLocationListPromise = root.data.ajax({
				type: "get",
				url: root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocations"),
				data: {
					sourceWorkspaceArtifactId: root.utils.getParameterByName("AppID", window.top)
				}
			}).fail(function (error) {
				IP.message.error.raise("No processing source locations were returned from source provider");
			});

			root.data.deferred()
				.all([processingSourceLocationListPromise])
				.then(function (result) {
					self.model.ProcessingSourceLocationList(result[0]);
					self.model.ProcessingSourceLocation(self.model.ProcessingSourceLocationArtifactId);
					self.model.ProcessingSourceLocation.isModified(false);

					if (!self.model.HasBeenRun()) {
						if (self.model.ProcessingSourceLocationArtifactId > 0) {
							self.model.updateProcessingSourceLocation(self.model.ProcessingSourceLocationArtifactId)
						}

						self.model.ProcessingSourceLocation.subscribe(function (value) {
							self.model.updateProcessingSourceLocation(value);
						});
					}
				});
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

				if (typeof (self.ipModel.Map) === 'string') {
					self.ipModel.Map = JSON.parse(self.ipModel.Map);
				}

				$.extend(self.ipModel.sourceConfiguration, settings);
				self.ipModel.sourceConfiguration.TargetWorkspaceArtifactId = self.ipModel.sourceConfiguration.SourceWorkspaceArtifactId; // this is needed as long as summary page displays destination workspace

				self.ipModel.sourceConfiguration = JSON.stringify(self.ipModel.sourceConfiguration);

				var destination = JSON.parse(self.ipModel.destination);
				destination.Provider = "Load File";
				destination.DoNotUseFieldsMapCache = false;
				self.ipModel.destination = JSON.stringify(destination);

				self.ipModel.Map = JSON.stringify(self.ipModel.Map);

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
					var formattedMessage = result.message.replace(new RegExp('\r?\n', 'g'), '.<br />');
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