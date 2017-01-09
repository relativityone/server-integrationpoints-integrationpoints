'use strict';
(function (windowObj, root, ko) {

	ko.validation.rules.pattern.message = 'Invalid.';
	ko.validation.registerExtenders();

	ko.validation.configure({
		insertMessages: false,
	}, true);

	ko.extenders.deferValidation = function (target, option) {
		if (option) {
			target.subscribe(function () {
				target.isModified(false);
			});
		}
		return target;
	};

	var ImportTypeEnum = windowObj.RelativityImport.ImportTypeEnum;

	var viewModel = function () {
		var self = this;

		self.selectedImportType = ko.observable().extend({
			required: true
		});
		self.setSelectedImportType = function (data) {
			self.selectedImportType(data);
		}

		self.selectedImportType.subscribe(function (data) {
			IP.frameMessaging().publish('importType', data);
			self.setSelectedImportType(data);
		});

		self.importTypes = ko.observableArray([]);

		self.populateFileColumnHeaders = ko.observable();
		self.setPopulateFileColumnHeaders = function (data) {
			self.populateFileColumnHeaders(data);
		};

		self.startLine = ko.observable("1").extend({
			validation: {
				validator: function (val) {
					var intVal = parseInt(val);
					return intVal > 0;
				},
				message: 'The field must be greater than 0.'
			},
			required: true
		});

		self.fileContainsColumn = ko.observable("true");

		//image/production import knockout bindings
		self.autoNumberPages = ko.observable("false");
		self.copyFilesToDocumentRepository = ko.observable("true");

		self.OverwriteOptions = ko.observableArray(['Append Only', 'Overlay Only', 'Append/Overlay']);
		self.SelectedOverwrite = ko.observable(self.SelectedOverwrite || 'Append Only').extend({
			required: true
		});

		self.overlayIdentifiers = ko.observableArray([]);
		self.selectedOverlayIdentifier = ko.observable().extend({
			required: {
				onlyIf: function() {
					return self.SelectedOverwrite() === "Overlay Only";
				}
			}
		});

		self.productionSets = ko.observableArray([]);
		self.selectedProductionSets = "Select...";
		self.selectedProductionSets = ko.observable().extend({
			required: {
				onlyIf: function() {
					return self.selectedImportType() === ImportTypeEnum.Production;
				}
			}
		});

		//delimiters for document import
		self.asciiDelimiters = ko.observableArray([]);
		self.setAsciiDelimiters = function (data) {
			self.asciiDelimiters(data);
		};

		self.selectedColumnAsciiDelimiter = ko.observable();
		self.setSelectedColumnAsciiDelimiters = function (data) {
			self.selectedColumnAsciiDelimiter(data);
		};

		self.selectedQuoteAsciiDelimiter = ko.observable();
		self.setSelectedQuoteAsciiDelimiters = function (data) {
			self.selectedQuoteAsciiDelimiter(data);
		};

		self.selectedNewLineAsciiDelimiter = ko.observable();
		self.setSelectedNewLineAsciiDelimiters = function (data) {
			self.selectedNewLineAsciiDelimiter(data);
		};

		self.selectedMultiLineAsciiDelimiter = ko.observable();
		self.setSelectedMultiLineAsciiDelimiters = function (data) {
			self.selectedMultiLineAsciiDelimiter(data);
		};

		self.selectedNestedValueAsciiDelimiter = ko.observable();
		self.setSelectedNestedValueAsciiDelimiters = function (data) {
			self.selectedNestedValueAsciiDelimiter(data);
		};

		self.ProcessingSourceLocationArtifactId = 0;

		self.ProcessingSourceLocationList = ko.observableArray([]);

		self.HasBeenRun = ko.observable(false);

		self.ProcessingSourceLocation = ko.observable(self.ProcessingSourceLocationArtifactId).extend({
			required: true,
			deferValidation: true
		});

		self.Fileshare = ko.observable().extend({
			required: true,
			deferValidation: true
		});

		self.GetSelectedProcessingSourceLocationPath = function (artifactId) {
			var selectedPath = ko.utils.arrayFirst(self.ProcessingSourceLocationList(), function (item) {
				if (item.artifactId === artifactId) {
					return item;
				}
			});
			return selectedPath;
		};

		self.DataFileEncodingTypeValue = "Select...";

		self.DataFileEncodingType = ko.observable(self.DataFileEncodingTypeValue).extend({
			required: true
		});

		//Populate file encoding dropdown
		self.FileEncodingTypeList = ko.observableArray([]);
		self._UpdateFileEncodingTypeList = function () {
			$.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
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
				//self.FileEncodingTypeList([new Group("", [new Option("Select...", "")]), new Group("Favorite", favorite), new Group("Others", others)]);

				self.FileEncodingTypeList([new Group("", [new Option("Unicode (UTF-8)", "utf-8")]), new Group("Favorite", favorite), new Group("Others", others)]);
				self.ExtractedTextFileEncodingList(result);

				self.DataFileEncodingType(self.DataFileEncodingTypeValue);
				self.DataFileEncodingType.isModified(false);
			});
		}	

		self.ExtractedTextFieldContainsFilePath = ko.observable("false");

		self.ExtractedTextFileEncoding = ko.observable().extend({
			required: {
				onlyIf: function() {
					return self.selectedImportType() === ImportTypeEnum.Image && self.ExtractedTextFieldContainsFilePath() === "true";
				}
			}
		});
		self.ExtractedTextFileEncodingList = ko.observableArray([]);

		self._UpdateFileEncodingTypeList();
	}
	windowObj.RelativityImport.koModel = new viewModel();
	windowObj.RelativityImport.koErrors = ko.validation.group(windowObj.RelativityImport.koModel);
	ko.applyBindings(windowObj.RelativityImport.koModel);

})(this, IP, ko);
