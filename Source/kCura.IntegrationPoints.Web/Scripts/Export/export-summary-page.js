var IP = IP || {};

var loadData = function (ko, dataContainer) {
	
	var Model = function (dataContainer) {
		var self = this;

		this.hasErrors = dataContainer.hasErrors;
		this.logErrors = dataContainer.logErrors;
		this.emailNotification = dataContainer.emailNotification;
		this.name = dataContainer.name;
		this.settings = dataContainer.sourceConfiguration;
		this.transferredRdoTypeName = dataContainer.transferredRdoTypeName;
		this.fileShareLocation = "FileShare: .\\" + self.settings.Fileshare;

		this.sourceDetails = function () {
			if (self.isRdoExportMode()) {
				return "RDO: " + self.transferredRdoTypeName + "; " + self.settings.ViewName;
			}
			if (self.settings.ExportType == ExportEnums.SourceOptionsEnum.SavedSearch) {
				return "Saved search: " + self.settings.SavedSearch;
			}
			if (self.settings.ExportType == ExportEnums.SourceOptionsEnum.Folder) {
				return "Folder: " + self.settings.FolderFullName;
			}
			if (self.settings.ExportType == ExportEnums.SourceOptionsEnum.FolderSubfolder) {
				return "Folder + Subfolders: " + self.settings.FolderFullName;
			}
			if (self.settings.ExportType == ExportEnums.SourceOptionsEnum.Production) {
				return "Production: " + self.settings.ProductionName;
			}
		};

		this.isRdoExportMode = function () {
			return self.transferredRdoTypeName !== "Document";
		}

		this.isProductionSet = function () {
			return self.settings.ExportType == ExportEnums.SourceOptionsEnum.Production;
		};

		this.textAndNativeFileNames = function () {
			var namedAfter = "";
			for (var i = 0; i < ExportEnums.ExportNativeWithFilenameFromTypes.length; i++) {
				if (ExportEnums.ExportNativeWithFilenameFromTypes[i].value == self.settings.ExportNativesToFileNamedFrom) {
					namedAfter = ExportEnums.ExportNativeWithFilenameFromTypes[i].key;
				}
			}
			return "Named after: " + namedAfter + (self.settings.AppendOriginalFileName ? "; Append Original File Names" : "");
		};

		this.volumeInfo = function () {
			return self.settings.VolumePrefix + "; " + self.settings.VolumeStartNumber + "; " + self.settings.VolumeDigitPadding + "; " + self.settings.VolumeMaxSize;
		};

		this.subdirectoryInfo = function () {

			return (self.settings.ExportImages ? self.settings.SubdirectoryImagePrefix + "; " : "")
				+ (self.settings.ExportNatives ? self.settings.SubdirectoryNativePrefix + "; " : "")
				+ (self.settings.ExportFullTextAsFile ? self.settings.SubdirectoryTextPrefix + "; " : "")
				+ self.settings.SubdirectoryStartNumber + "; " + self.settings.SubdirectoryDigitPadding + "; " + self.settings.SubdirectoryMaxFiles;
		};

		this.exportType = function () {
			return "Load file"
			+ (self.settings.ExportImages ? "; Images" : "")
			+ (self.settings.ExportNatives ? "; Natives" : "")
			+ (self.settings.ExportFullTextAsFile ? "; Text As Files" : "");
		};

		this.filePath = function () {
			var filePathType = "";
			for (var i = 0; i < ExportEnums.FilePathType.length; i++) {
				if (ExportEnums.FilePathType[i].value == self.settings.FilePath) {
					filePathType = ExportEnums.FilePathType[i].key;
				}
			}
			return (self.settings.IncludeNativeFilesPath ? "Include" : "Do not include")
			+ ("; " + filePathType)
			+ (self.settings.FilePath == ExportEnums.FilePathTypeEnum.UserPrefix ? (": " + self.settings.UserPrefix) : "");
		};

		this.setEncoding = function (dataFileEncodingName, callback) {
			IP.data.ajax({ type: "get", url: IP.utils.generateWebAPIURL("GetAvailableEncodings") }).then(function (result) {
				var encoding = ko.utils.arrayFirst(result, function (item) {
					return (item.name === dataFileEncodingName);
				});
				callback(encoding.displayName);
			});
		}

		this.concatenateLoadFileFormat = function (displayName) {
			var fileFormat = "";
			var dataFileFormat = ko.utils.arrayFirst(ExportEnums.DataFileFormats, function (item) {
				return item.value === self.settings.SelectedDataFileFormat;
			});
			fileFormat = dataFileFormat.key;

			self.LoadFileFormat(fileFormat + "; " + displayName);
		};

		this.LoadFileFormat = ko.observable();
		this.setLoadFileFormat = function () {
			self.setEncoding(self.settings.DataFileEncodingType, self.concatenateLoadFileFormat);
		};
		this.setLoadFileFormat();

		this.imageFileType = function () {
			if (self.settings.ExportImages) {
				for (var i = 0; i < ExportEnums.ImageFileTypes.length; i++) {
					if (ExportEnums.ImageFileTypes[i].value == self.settings.SelectedImageFileType) {
						return ExportEnums.ImageFileTypes[i].key;
					}
				}
			}
			return "";
		};

		this.imageDataFileFormat = function () {
			if (self.settings.ExportImages) {
				for (var i = 0; i < ExportEnums.ImageDataFileFormats.length; i++) {
					if (ExportEnums.ImageDataFileFormats[i].value == self.settings.SelectedImageDataFileFormat) {
						return ExportEnums.ImageDataFileFormats[i].key;
					}
				}
			}
			return "";
		};

		this.imagePrecedenceList = function () {
			var text = "";
			if (self.settings.ExportImages) {
				if (self.settings.ProductionPrecedence === ExportEnums.ProductionPrecedenceTypeEnum.Original) {
					text = "Original";
				}
				else {
					text = "Produced: ";
					if (self.settings.ImagePrecedence.length > 0) {
						for (var i = 0; i < self.settings.ImagePrecedence.length; i++) {
							text += self.settings.ImagePrecedence[i].displayName + "; ";
						}
					}
				}
			}
			return text;
		};

		this.textPrecenceList = function () {
			var text = "";
			if (self.settings.ExportFullTextAsFile && self.settings.TextPrecedenceFields.length > 0) {
				for (var i = 0; i < self.settings.TextPrecedenceFields.length; i++) {
					text += self.settings.TextPrecedenceFields[i].displayName + "; ";
				}
				text = text.substring(0, text.length - 2);
			}
			return text;
		};

		this.TextFileEncoding = ko.observable();
		this.setTextFileEncoding = function () {
			self.setEncoding(self.settings.TextFileEncodingType, self.TextFileEncoding);
		}
		this.setTextFileEncoding();
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};