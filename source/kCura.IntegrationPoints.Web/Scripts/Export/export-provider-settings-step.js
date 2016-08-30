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

	var viewModel = function (m) {
		var state = $.extend({}, {}, m);
		var self = this;

		// TODO: reintroduce this functionality: IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun()
		this.HasBeenRun = ko.observable(false);

		this.Fileshare = ko.observable(state.Fileshare).extend({
			required: true
		});

		this.IncludeNativeFilesPath = ko.observable(state.IncludeNativeFilesPath || false);

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

		this.CopyFileFromRepository = ko.observable(state.CopyFileFromRepository || false);
		this.CopyFileFromRepository.subscribe(function (value) {
			if (!value) {
				self.SelectedImageFileType(0);
			}
		});

		this.ExportTextFieldsAsFilesChecked = ko.observable(state.ExportFullTextAsFile || false);

		this.OverwriteFiles = ko.observable(state.OverwriteFiles || false);

		this.DataFileEncodingType = ko.observable().extend({
			required: true
		});

		this.TextFileEncodingType = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.ExportTextFieldsAsFilesChecked();
				}
			}
		});

		this.getFileEncodingTypeName = function (value) {
			if (self.FileEncodingTypeList().length === 3) {
				var ungroupedFileEncodingList = self.FileEncodingTypeList()[0].children()
		            .concat(self.FileEncodingTypeList()[1].children())
		            .concat(self.FileEncodingTypeList()[2].children());
				var selectedFileEncodingType = ko.utils.arrayFirst(ungroupedFileEncodingList, function (item) {
					return item.name === value;
				});

				return selectedFileEncodingType.name;
			}
		};

		this.FileEncodingTypeList = ko.observableArray([]);
		if (self.FileEncodingTypeList.length === 0) {
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
				self.FileEncodingTypeList([new Group("", [defaultOption]), new Group("Favorite", favorite), new Group("Others", others)]);

				self.DataFileEncodingType(self.getFileEncodingTypeName(state.DataFileEncodingType || ""));
				self.TextFileEncodingType(self.getFileEncodingTypeName(state.TextFileEncodingType || ""));
			});
		}
		else {
			self.DataFileEncodingType(self.getFileEncodingTypeName(state.DataFileEncodingType));
			self.TextFileEncodingType(self.getFileEncodingTypeName(state.TextFileEncodingType));
		}

		this.ExportImagesChecked = ko.observable(state.ExportImagesChecked || false).extend({
			required: true
		});

		this.ProductionPrecedence = ko.observable(state.ProductionPrecedence).extend({
			required: {
				onlyIf: function () {
					return self.ExportImagesChecked();
				}
			}
		});

		this.IsProductionPrecedenceSelected = function () {
			return self.ProductionPrecedence() === ExportEnums.ProductionPrecedenceTypeEnum.Produced;
		}

		this.SelectedImageDataFileFormat = ko.observable(state.SelectedImageDataFileFormat).extend({
			required: {
				onlyIf: function () {
					return self.ExportImagesChecked();
				}
			}
		});

		this.IncludeOriginalImages = ko.observable(state.IncludeOriginalImages || false);

		this.SelectedImageFileType = ko.observable(!self.CopyFileFromRepository() ? 0 : state.SelectedImageFileType).extend({
			required: {
				onlyIf: function () {
					return self.ExportImagesChecked();
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
			}
		});

		this.isUserPrefix = ko.observable(false);

		this.FilePath.subscribe(function (value) {
			self.isUserPrefix(value == ExportEnums.FilePathTypeEnum.UserPrefix);
		});

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
						return self.ExportTextFieldsAsFilesChecked();
					}
				}
			});

		this.TextPrecedenceSelection = ko.pureComputed(function () {
			return getTextRepresentation(self.TextPrecedenceFields());
		});

		var textPrecedencePickerViewModel = new TextPrecedencePickerViewModel(function (fields) {
			self.TextPrecedenceFields(fields);
		});

		Picker.create("ListPicker", textPrecedencePickerViewModel);

		this.openTextPrecedencePicker = function () {
			textPrecedencePickerViewModel.open(self.TextPrecedenceFields());
		};

		this.ImagePrecedence = ko.observable(state.ImagePrecedence || [])
			.extend({
				required: {
					onlyIf: function () {
						return self.ExportImagesChecked() && self.IsProductionPrecedenceSelected();
					}
				}
			});

		this.ImagePrecedenceSelection = ko.pureComputed(function () {
			return getTextRepresentation(self.ImagePrecedence());
		});

		var imageProductionPickerViewModel = new ImageProductionPickerViewModel(function (productions) {
			self.ImagePrecedence(productions);
		});

		Picker.create("ListPicker", imageProductionPickerViewModel);

		this.openImageProductionPicker = function () {
			imageProductionPickerViewModel.open(self.ImagePrecedence());
		};

		this.errors = ko.validation.group(this, { deep: true });

		this.getSelectedOption = function () {
			return {
				"ColumnSeparator": self.ColumnSeparator(),
				"CopyFileFromRepository": self.CopyFileFromRepository(),
				"DataFileEncodingType": self.DataFileEncodingType(),
				"ExportFullTextAsFile": self.ExportTextFieldsAsFilesChecked(),
				"ExportImagesChecked": self.ExportImagesChecked(),
				"ExportMultipleChoiceFieldsAsNested": self.ExportMultipleChoiceFieldsAsNested(),
				"FilePath": self.FilePath(),
				"Fileshare": self.Fileshare(),
				"ImagePrecedence": self.ImagePrecedence(),
				"IncludeNativeFilesPath": self.IncludeNativeFilesPath(),
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
				"VolumeStartNumber": self.VolumeStartNumber()
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
				self.onDOMLoaded();
				root.messaging.publish('details-loaded');
			});
		}

		self.ipModel = {};
		self.model = {};

		self.onDOMLoaded = function () {
			console.log($('select#location-select'));
			$('select#location-select').mousedown(function () {
				if (self.treeVisible) {
					//self.setSelection(self.SelectedFolderPath);
				}
				self.setTreeVisibility(!self.treeVisible);
			});
			self.initJsTree();
			self.setTreeVisibility(self.treeVisible);

		};

		//jsTree
		self.initJsTree = function () {
			$('div#browser-tree').jstree({
				'core': {
					'data': [{
						"text": "Root node",
						"children": [{
							"text": "Child node 1"
						}, {
							"text": "Child node 2"
						}]
					}]
				}
			});

			$('div#browser-tree').on('select_node.jstree', function (evt, data) {

				self.setSelection(data.node.text);
				self.SelectedFolderPath = data.node.id;
				self.setTreeVisibility(false);
			}
          );
		};
		self.treeVisible = false;
		self.SelectedFolderPath = '';

		self.setTreeVisibility = function (visible) {
			if (visible) {
				$('#jstree-holder-div').width($('select#location-select').outerWidth());
				$('#jstree-holder-div').show();
				self.treeVisible = true;
			} else {
				$('#jstree-holder-div').width($('select#location-select').outerWidth());
				$('#jstree-holder-div').hide();
				self.treeVisible = false;
			}
		};

		self.clearSelection = function () {
			$('select#location-select').empty();
			$('select#location-select').prop('selectedIndex', 0);
			$('select#location-select option:selected').hide();
		};

		self.setSelection = function (newValue) {
			$('select#location-select').empty();
			$('select#location-select').append('<option>' + newValue + '</option>');
			$('select#location-select').prop('selectedIndex', 0);
			$('select#location-select option:selected').hide();
		};


		//jstree



		self.loadModel = function (ip) {
			self.ipModel = ip;

			self.model = new viewModel();
			self.model.errors = ko.validation.group(self.model);
		};

		self.submit = function () {
			var d = root.data.deferred().defer();

			if (self.model.errors().length === 0) {
				var settings = self.model.getSelectedOption();

				$.extend(self.ipModel.sourceConfiguration, settings);
				self.ipModel.sourceConfiguration.TargetWorkspaceArtifactId = self.ipModel.sourceConfiguration.SourceWorkspaceArtifactId;
				self.ipModel.sourceConfiguration = JSON.stringify(self.ipModel.sourceConfiguration);

				var destination = JSON.parse(self.ipModel.destination);
				destination.Provider = "Fileshare";
				destination.DoNotUseFieldsMapCache = false;
				self.ipModel.destination = JSON.stringify(destination);

				d.resolve(self.ipModel);
			} else {
				self.model.errors.showAllMessages();
				d.reject();
			}

			return d.promise;
		};

		self.back = function () {
			var d = root.data.deferred().defer();

			d.resolve();

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