var IP = IP || {};

const documentArtifactTypeId = 10;

const mappingType = {
	Manual: "manual",
	Automap: "automap",
	SavedSearch: "savedsearch",
	View: "view",
	Loaded : "loaded"
};

IP.mappingType = mappingType.Loaded;

ko.validation.rules.pattern.message = 'Invalid.';

ko.validation.configure({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
});
ko.validation.rules['mustEqualMapped'] = {
	validator: function (value, params) {
		return value.length === params().length;
	},
	message: 'Some selected items have not been mapped.'
};

ko.validation.rules['identifierMustMappedWithAnotherIdentifier'] = {
	validator: function (value, params) {
		var targetMap = params();
		var isMappedCorrectly = false;
		$.each(value, function (index, item) {
			if (item.isIdentifier === true) {
				isMappedCorrectly = targetMap[index].isIdentifier;
				return;
			}
		});
		return isMappedCorrectly;
	},
	message: 'Identifier must be mapped with another identifier.'
};


ko.validation.rules['uniqueIdIsMapped'] = {
	validator: function (value, params) {
		var selectedUniqueIdName = params[0]();
		var rdoIdentifierName = params[1]();
		var containsIdentifier = false;
		var rdoIdentifierMapped = false;
		$.each(value, function (index, item) {
			if (item.isIdentifier === true) {
				rdoIdentifierMapped = true;
			}
			if (item.name == rdoIdentifierName) {
				containsIdentifier = true;
			}
		});
		if (containsIdentifier && rdoIdentifierMapped) {
			if ($('#uniquIdMissing').length) {
				IP.message.error.clear();
			}
			return true;
		}
		if (!rdoIdentifierMapped || !containsIdentifier) {
			var missingField = "";
			if (!rdoIdentifierMapped && !containsIdentifier) {
				if (rdoIdentifierName !== selectedUniqueIdName) {
					missingField = "The object identifier, " + rdoIdentifierName + ", and the unique identifier, " + selectedUniqueIdName;
				} else {
					missingField = "The object identifier, " + rdoIdentifierName;
				}
			}
			if (!rdoIdentifierMapped && containsIdentifier) {
				missingField = "The object identifier, " + rdoIdentifierName;
			}
			if (rdoIdentifierMapped && !containsIdentifier) {
				missingField = "The unique identifier, " + rdoIdentifierName;
			}
			IP.message.error.raise('<span id="uniquIdMissing"> ' + missingField + ', must be mapped.<span>');
			return false;
		}
		return true;
	},
	message: "The unique identifier field must be mapped."
};

ko.validation.rules['nativeFilePathMustBeMapped'] = {
	validator: function (value, params) {
		if (typeof (value) !== "undefined") {
			$.each(params[0](), function () {
				if (value === this.name) {
					return false;
				}
			});
		}
		return true;
	},
	message: 'The Native file path field must be mapped.'
};


ko.validation.rules['fieldsMustBeMapped'] = {
	validator: function (value, params) {
		var requiredFields = [];
		for (var i = 0; i < value.length; i++) {
			var current = value[i];
			if (current.isRequired) {
				requiredFields.push(current.name);
			}
		}

		if (requiredFields.length > 0) {
			var fieldMessage = requiredFields.sort().join(' and ');
			var fieldPlural = requiredFields.length === 1 ? 'field' : 'fields';
			IP.message.error.raise('<span id="missingFieldMessage">The ' + fieldMessage + ' ' + fieldPlural + ' must be mapped.</span>');
		} else {
			if ($('#missingFieldMessage').length) {
				IP.message.error.clear();
			}
		}
		return requiredFields.length === 0;
	},
	message: ''
};

ko.validation.registerExtenders();

ko.validation.insertValidationMessage = function (element) {
	var errorContainer = document.createElement('div');
	var iconSpan = document.createElement('span');
	iconSpan.className = 'icon-error legal-hold field-validation-error';

	errorContainer.appendChild(iconSpan);

	$(element).parents('.field-value').eq(0).append(errorContainer);

	return iconSpan;
};



(function (root, ko) {

	var objectIdentifierFieldSuffix = " [Object Identifier]";

	var mapFields = function (result) {
		return $.map(result, function (field) {
			var fieldDisplayName = field.name;
			if (!field.isIdentifier && field.type) {
				fieldDisplayName = field.name + " [" + field.type + "]";
			}
			else if (field.isIdentifier) {
				fieldDisplayName = field.name + objectIdentifierFieldSuffix;
			}
			return {
				name: field.name,
				displayName: fieldDisplayName,
				type: field.type,
				identifer: field.fieldIdentifier,
				isIdentifier: field.isIdentifier,
				isRequired: field.isIdentifier
			};
		});
	}

	var viewModel = function (model) {
		var self = this;

		self.setTitle = function (option, item) {
			option.title = item.displayName;
		}

		this.hasBeenLoaded = model.hasBeenLoaded;
		this.showErrors = ko.observable(false);
		var destinationModel = JSON.parse(model.destination);
		var artifactTypeId = destinationModel.artifactTypeID;
		var artifactId = model.artifactID || 0;
		this.workspaceFields = ko.observableArray([]).extend({
			fieldsMustBeMapped: {
				onlyIf: function () {
					return self.showErrors();
				}
			}
		});

		this.AllowUserToMapNativeFileField = ko.observable(model.SourceProviderConfiguration.importSettingVisibility.allowUserToMapNativeFileField);

		this.selectedUniqueId = ko.observable().extend({ required: { message: "Unique id required" } });
		this.rdoIdentifier = ko.observable();
		this.isAppendOverlay = ko.observable(true);
		self.SecuredConfiguration = model.SecuredConfiguration;
		self.CreateSavedSearchForTagging = destinationModel.CreateSavedSearchForTagging;

		this.mappedWorkspace = ko.observableArray([]).extend({
			uniqueIdIsMapped: {
				onlyIf: function () {
					return self.showErrors() && self.mappedWorkspace().length >= 0;
				},
				params: [self.selectedUniqueId, self.rdoIdentifier]
			}
		});

		this.sourceMapped = ko.observableArray([]).extend({
			mustEqualMapped: {
				onlyIf: function () {
					return self.showErrors();
				},
				params: this.mappedWorkspace
			}
		}).extend(
			{
				identifierMustMappedWithAnotherIdentifier: {
					onlyIf: function () {
						return self.showErrors() && model.SourceProviderConfiguration.onlyMapIdentifierToIdentifier;
					},
					params: this.mappedWorkspace
				}
			});

		if (typeof model.EntityManagerFieldContainsLink === "undefined") {
			model.EntityManagerFieldContainsLink = "true";
		}

		this.EntityManagerFieldContainsLink = ko.observable(model.EntityManagerFieldContainsLink || "false");
		this.sourceField = ko.observableArray([]);
		this.workspaceFieldSelected = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);
		this.IdentifierField = ko.observable(model.IPDestinationSettings.IdentifierField);
		this.showMapSavedSearchButton = ko.observable(false);

		//use this to bind which elements show up depending on if the user is accessing Relativity Provider or not
		this.IsRelativityProvider = ko.observable(IP.reverseMapFields);

		var isNonDocumentObjectFlow = IP.data.params['EnableSyncNonDocumentFlowToggleValue'] &&
										IP.data.params['TransferredRDOArtifactTypeID'] != documentArtifactTypeId &&
										this.IsRelativityProvider();
		self.IsNonDocumentObjectFlow = ko.observable();
		self.IsNonDocumentObjectFlow(isNonDocumentObjectFlow);

		this.overlay = ko.observableArray([]);
		this.nativeFilePathOption = ko.observableArray([]);
		this.hasParent = ko.observable(false);
		this.parentField = ko.observableArray([]);

		//We want to dispable options CopyFile, SetLinks and None for Copy Images job
		this.ImportNativeFileCopyModeEnabled = ko.observable(model.ImageImport === "true" ? "false" : "true");

		this.importNativeFile = ko.observable(model.importNativeFile || "false");

		var setDefaultImportNativeFileCopyMode = function (importNativeFile, importNativeFileCopyMode) {
			if (importNativeFile === undefined && importNativeFileCopyMode === undefined) {
				return "DoNotImportNativeFiles";    //TODO Replace string by variables
			}

			if (importNativeFileCopyMode) {
				return importNativeFileCopyMode;
			}

			return importNativeFile === "true" ? "CopyFiles" : "SetFileLinks";
		};
		this.importNativeFileCopyMode = ko.observable(setDefaultImportNativeFileCopyMode(model.importNativeFile, model.importNativeFileCopyMode));
		this.importNativeFileCopyMode.subscribe(function (copyMode) {
			if (self.ImageImport() === "false") {
				if (copyMode === "CopyFiles" || copyMode === "SetFileLinks") {
					self.importNativeFile("true");
				} else if (copyMode === "DoNotImportNativeFiles") {
					self.importNativeFile("false");
				}
			}
		});


		var copyNativeFileText = "Copy Native Files:";
		var copyFileToRepositoryText = "Copy Files to Repository:";
		this.copyNativeLabel = ko.observable(copyNativeFileText);
		this.ImageImport = ko.observable(model.ImageImport || "false");

		this.CheckRelativityProviderExportType = function (exportType) {
			if (this.IsRelativityProvider()) {
				var sourceModel = JSON.parse(model.sourceConfiguration);
				if (sourceModel.TypeOfExport && sourceModel.TypeOfExport === exportType) {
					return true;
				}
			}
			return false;
		};

		this.IsProductionExport = function () {
			return this.CheckRelativityProviderExportType(ExportEnums.SourceOptionsEnum.Production);
		};

		this.IsSavedSearchExport = function () {
			return this.CheckRelativityProviderExportType(ExportEnums.SourceOptionsEnum.SavedSearch);
		};

		this.IsViewExport = function () {
			return this.CheckRelativityProviderExportType(ExportEnums.SourceOptionsEnum.View);
		};

		this.importNativeFile.subscribe(function (importNative) {
			if (self.ImageImport() === "true") {
				self.importNativeFileCopyMode(importNative === "true" ? "CopyFiles" : "SetFileLinks");
			}
		});

		var setCopyFilesLabel = function (isImageImport) {
			if (isImageImport === "true") {
				self.copyNativeLabel(copyFileToRepositoryText);
			} else {
				self.copyNativeLabel(copyNativeFileText);
			}
		}
		setCopyFilesLabel(this.ImageImport());

		/********** Temporary UI Toggle**********/
		this.ImageImportVisible = ko.observable(self.IsRelativityProvider());

		this.ImageImport.subscribe(function (value) {
			setCopyFilesLabel(value);
			if (value === "true") {
				root.utils.UI.disable("#fieldMappings", true);
				self.UseFolderPathInformation("false");
				self.UseDynamicFolderPath("false");
				self.MoveExistingDocuments("false");
				self.FolderPathSourceField(null);
				self.autoFieldMapWithCustomOptions(function (identfier) {
					var name = identfier.name.replace(objectIdentifierFieldSuffix, "");
					self.IdentifierField(name);
				});
				self.ImportNativeFileCopyModeEnabled("false");
			}
			else {
				root.utils.UI.disable("#fieldMappings", false);
				self.ImportNativeFileCopyModeEnabled("true");
				self.importNativeFileCopyMode("DoNotImportNativeFiles");
			}
		});

		this.ProductionPrecedence = ko.observable(model.IPDestinationSettings.ProductionPrecedence || model.ProductionPrecedence).extend({
			required: {
				onlyIf: function () {
					return self.ImageImport();
				}
			}
		});

		this.IsProductionPrecedenceSelected = function () {
			return self.ProductionPrecedence() === ExportEnums.ProductionPrecedenceTypeEnum.Produced;
		}

		var getTextRepresentation = function (value) {
			if (!value || value.length === 0) {
				return "Select...";
			}

			return value.map(function (x) {
				return x.displayName;
			}).join("; ");
		};

		this.ImagePrecedence = ko.observable(model.IPDestinationSettings.ImagePrecedence || model.ImagePrecedence || [])
			.extend({
				required: {
					onlyIf: function () {
						return self.ImageImport() && self.IsProductionPrecedenceSelected();
					}
				}
			});

		this.ImagePrecedenceSelection = ko.pureComputed(function () {
			return getTextRepresentation(self.ImagePrecedence());
		});

		this.IncludeOriginalImages = ko.observable(model.IPDestinationSettings.IncludeOriginalImages || model.IncludeOriginalImages || false);

		var imageProductionPickerViewModel = new ImageProductionPickerViewModel(function (productions) {
			self.ImagePrecedence(productions);
		});

		Picker.create("Fileshare", "imageProductionPicker", "ListPicker", imageProductionPickerViewModel);

		this.openImageProductionPicker = function () {
			imageProductionPickerViewModel.open(self.ImagePrecedence());
		};

		this.onDOMLoaded = function () {
			root.utils.UI.disable("#fieldMappings", self.ImageImport() === "true");
		}


		this.SourceProviderConfiguration = ko.observable(model.SourceProviderConfiguration);

		this.OverwriteOptions = ko.observableArray(['Append Only', 'Overlay Only', 'Append/Overlay']);
		this.MultiSelectFieldOverlayBehaviors = ko.observableArray(['Merge Values', 'Replace Values', 'Use Field Settings']);
		this.FieldOverlayBehavior = ko.observable(model.FieldOverlayBehavior || 'Use Field Settings');

		self.OverwriteOptions = this.OverwriteOptions;
		self.FieldOverlayBehavior = this.FieldOverlayBehavior;

		this.SelectedOverwrite = ko.observable(model.SelectedOverwrite || 'Append Only');
		this.SelectedOverwrite.subscribe(function (newValue) {
			if (newValue != 'Append Only') {
				self.UseFolderPathInformation("false");
				self.UseDynamicFolderPath("false");
				self.FolderPathSourceField(null);
			} else {
				self.FieldOverlayBehavior('Use Field Settings');
			}
			self.MoveExistingDocuments("false");
			self.SelectedFolderPathType('No');
		});

		this.getFolderPathOptions = function (model) {
			if (!model) {
				return 'No';
			}
			if (model.UseFolderPathInformation == 'true') {
				return 'Read From Field';
			}
			if (model.UseDynamicFolderPath == 'true') {
				return 'Read From Folder Tree';
			}
			return 'No';
		}

		this.FolderPathOptions = ko.observableArray(['No', 'Read From Field', 'Read From Folder Tree']);
		this.SelectedFolderPathType = ko.observable(self.getFolderPathOptions(model));
		this.SelectedFolderPathType.subscribe(function (value) {
			if (value === 'No') {
				self.UseFolderPathInformation('false');
				self.FolderPathSourceField(null);
				self.UseDynamicFolderPath('false');
			} else if (value === 'Read From Field') {
				self.UseFolderPathInformation('true');
				self.FolderPathSourceField.isModified(false);
				self.UseDynamicFolderPath('false');
			} else {
				self.UseFolderPathInformation('false');
				self.FolderPathSourceField(null);
				self.UseDynamicFolderPath('true');
			}
		});

		this.UseDynamicFolderPath = ko.observable(model.UseDynamicFolderPath || "false");
		this.UseFolderPathInformation = ko.observable(model.UseFolderPathInformation || "false");
		this.FolderPathSourceField = ko.observable().extend(
			{
				required: {
					onlyIf: function () {
						return self.UseFolderPathInformation() === 'true';
					}
				}
			});
		this.FolderPathFields = ko.observableArray([]);
		if (self.FolderPathFields.length === 0) {
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('FolderPath', 'GetFields') }).then(function (result) {
				// GetFolderPathFields only returns fixed-length text and long text fields
				self.FolderPathFields(result);
				self.FolderPathSourceField(model.FolderPathSourceField);
			});
		}
		this.FolderPathImportProvider = ko.observableArray([]);
		this.MoveExistingDocuments = ko.observable(model.MoveExistingDocuments || "false");

		this.FolderPathImportProvider = ko.observableArray([]);
		this.ExtractedTextFieldContainsFilePath = ko.observable(model.ExtractedTextFieldContainsFilePath || "false");
		this.ExtractedTextFileEncoding = ko.observable(model.ExtractedTextFileEncoding || "utf-16").extend(
			{
				required: {
					onlyIf: function () {
						return self.ExtractedTextFieldContainsFilePath() === 'true';
					}
				}
			});

		this.LongTextColumnThatContainsPathToFullText = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.ExtractedTextFieldContainsFilePath() === 'true';
				}
			}
		});

		this.TotalLongTextFields = {};//has the full set of long text fields in workspace

		this.MappedLongTextFields = ko.observableArray([]); //only has the mapped long text fields
		if (self.MappedLongTextFields.length === 0) {
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('FolderPath', 'GetLongTextFields') }).then(function (result) {
				// Returns a list of all attributes in a workspace with the FieldCategory for long text
				self.TotalLongTextFields = result;
				self.LongTextColumnThatContainsPathToFullText(model.LongTextColumnThatContainsPathToFullText);
			});
		}

		this.populateExtractedText = function () {
			if ($.isEmptyObject(self.mappedWorkspace())) {
				self.MappedLongTextFields([]);
			} else {
				var mappedWorkspace = self.mappedWorkspace();
				var matchesContainer = [];
				for (var i = 0; i < mappedWorkspace.length; i++) {
					for (var j = 0; j < self.TotalLongTextFields.length; j++) {
						if (mappedWorkspace[i].name === self.TotalLongTextFields[j].displayName) {
							matchesContainer.push(self.TotalLongTextFields[j]);
							break;
						}
					}
				}

				self.MappedLongTextFields(matchesContainer);
			}
		};

		this.ExtractedTextFileEncodingList = ko.observableArray([]);
		if (self.ExtractedTextFileEncodingList.length === 0) {
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
				self.ExtractedTextFileEncodingList(result);
			});
		}

		this.nativeFilePathValue = ko.observableArray([]).extend({
			required: {
				onlyIf: function () {
					return self.AllowUserToMapNativeFileField() == true && (self.importNativeFile() == 'true' || self.importNativeFile == true) && self.showErrors();
				},
				message: 'The Native file path field is required.',
			}
		});

		this.isDocument = ko.observable("false");
		if (artifactTypeId == 10) {
			self.isDocument("true");
		}

		this.selectedIdentifier = ko.observable().extend({
			required: {
				onlyIf: function () {
					return !isNonDocumentObjectFlow && self.showErrors() && self.hasParent();
				},
				message: 'The Parent Attribute is required.',
			}
		});
		this.showManager = ko.observable(false);
		this.cacheMapped = ko.observableArray([]);

		if (!isNonDocumentObjectFlow){
			root.data.ajax({
				type: 'GET', url: root.utils.generateWebAPIURL('Entity/' + artifactTypeId)
			}).then(function (result) {
				self.showManager(result);
			});
		};

		function getCustomProviderSourceFields() {
			return root.data.ajax({
				type: 'Post',
				url: root.utils.generateWebAPIURL('SourceFields'),
				data: JSON.stringify({
					'options': model.sourceConfiguration,
					'type': model.source.selectedType,
					'credentials': self.SecuredConfiguration
				})
			})
		}

		function getWorkspaceFieldPromise() {
			return root.data.ajax({
				type: 'POST',
				url: root.utils.generateWebAPIURL('WorkspaceField'),
				data: JSON.stringify({
					settings: model.destination,
					credentials: self.SecuredConfiguration
				})
			});
		}

		function getSyncSourceFields() {
			return root.data.ajax({
				type: 'GET',
				url: root.utils.generateWebAPIURL('FieldMappings/GetMappableFieldsFromSourceWorkspace', root.data.params['TransferredRDOArtifactTypeID'])
			});
		}

		function getSyncDestinationFields() {

			var destinationWorkspaceId = destinationModel.CaseArtifactId;
			var destinationArtifactTypeId = destinationModel.DestinationArtifactTypeId;

			return root.data.ajax({
				type: 'GET',
				url: root.utils.generateWebURL(destinationWorkspaceId + '/api/FieldMappings/GetMappableFieldsFromDestinationWorkspace/' + destinationArtifactTypeId)
			}).then(function (result) {
				return result;
			});			
		}

		var sourceFieldPromise =
			(self.IsRelativityProvider()
				? getSyncSourceFields()
				: getCustomProviderSourceFields())
				.fail(function (error) {
					IP.message.error.raise("No attributes were returned from the source provider.");
				});

		var destinationPromise =
			(self.IsRelativityProvider()
				? getSyncDestinationFields()
				: getWorkspaceFieldPromise()
			)
				.fail(function (error) {
					IP.message.error.raise("Could not load destination workspace fields");
				});

		var destination = JSON.parse(model.destination);
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('rdometa', destination.artifactTypeID) }).then(function (result) {
			self.hasParent(result.hasParent);
		});

		this.destinationCaseArtifactID = destination.CaseArtifactId;

		self.findField = function (array, field) {
			const fields = $.grep(array, function (value, _index) { return value.fieldIdentifier === field.fieldIdentifier || value.name == field.displayName; });
			const fieldFound = fields.length > 0;
			return {
				exist: fieldFound,
				type: fieldFound ? fields[0].type : null,
				name: fieldFound ? fields[0].name : null
			};
		};

		self.updateFieldFromMapping = function (mappedField, fields) {
			var field = self.findField(fields, mappedField);
			if (field.exist) {
				if (field.isIdentifier) {
					mappedField.displayName = field.name + objectIdentifierFieldSuffix;
				}
				else {
					mappedField.displayName = field.name + " [" + field.type + "]";
				}
				mappedField.name = field.name;
				mappedField.type = field.type;
				return mappedField;
			}
			return null;
		};

		var mappedSourcePromise;
		if (destination.DoNotUseFieldsMapCache) {
			mappedSourcePromise = [];
		} else {
			if (typeof (model.map) === "undefined" || model.map === null) {
				mappedSourcePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('FieldMap', artifactId) });
			} else {
				mappedSourcePromise = jQuery.parseJSON(model.map);
			}
		}

		var promises = [destinationPromise, sourceFieldPromise, mappedSourcePromise];

		var mapTypes = {
			identifier: 'Identifier',
			parent: 'FolderPathInformation',
			native: 'NativeFilePath'
		};

		var mapHelper = (function () {
			function find(fields, fieldMapping, key, func) {
				return $.grep(fields,
					function (item) {
						var remove = false;
						$.each(fieldMapping,
							function () {
								if (this[key].fieldIdentifier === item.fieldIdentifier &&
									this["fieldMapType"] !== mapTypes.parent &&
									this["fieldMapType"] !== mapTypes.native) {
									remove = true;
									return false;
								}
							});
						return func(remove);
					});
			}

			function getNotMapped(fields, fieldMapping, key) {
				return find(fields, fieldMapping, key, function (r) { return !r });
			}

			function getMapped(sourceFields, destinationFields, fieldMapping, sourceKey, destinationKey) {
				var sourceMapped = [];
				var destinationMapped = [];

				var sourceWithoutPair = [];
				var destinationWithoutPair = [];

				$.each(fieldMapping, function (_index, mapping) {
					var sourceField = self.updateFieldFromMapping(mapping[sourceKey], sourceFields);
					var destinationField = self.updateFieldFromMapping(mapping[destinationKey], destinationFields);
					if (!!sourceField && !!destinationField) {
						sourceMapped.push(sourceField);
						destinationMapped.push(destinationField);
					} else {
						if (!destinationField) {
							sourceWithoutPair.push(sourceField);
						} else {
							destinationWithoutPair.push(destinationField);
						}
					}
				});

				return [sourceMapped, destinationMapped, sourceWithoutPair, destinationWithoutPair];
			}

			return {
				getNotMapped: getNotMapped,
				getMapped: getMapped
			};
		})();

		function confirmRemovingNotMappedFields(notMappedSourceFields,
			notMappedDestinationFields,
			successCallback,
			cancelCallback) {

			var tableDiv = $('<div/>').css({ "overflow-y": "auto", "max-height": "400px" });

			function addColumn(description, elements) {
				var columnDiv = $('<div/>').css({ "float": "left", "width": "50%" });
				$('<p/>').html(description).appendTo(columnDiv);

				var list = $('<ul/>').appendTo(columnDiv);

				$.each(elements,
					function () {
						$('<li/>').html(this).appendTo(list);
					});

				$(columnDiv).appendTo(tableDiv);
			}

			if (notMappedSourceFields.length) {
				addColumn("Source:", notMappedSourceFields.map(x => x.name));
			}

			if (notMappedDestinationFields.length) {
				addColumn("Destination:", notMappedDestinationFields.map(x => x.name));
			}

			var dialogContent = $('<div/>')
				.html('<p>The below fields were skipped from mapping.</p>');

			tableDiv.appendTo(dialogContent);

			$('<div/>')
				.html('<p>Would you like to keep them in mapping anyway?</p>').appendTo(dialogContent)

			return window.Dragon.dialogs.showConfirmWithCancelHandler({
				message: dialogContent.html(),
				title: "Integration Point Mapping",
				width: 450,
				okText: "Yes, keep them",
				cancelText: "No, skip them",
				showCancel: true,
				messageAsHtml: true,
				closeOnEscape: false,
				success: function (calls) {
					calls.close();
					successCallback();
				},
				cancel: function (calls) {
					calls.close();
					cancelCallback();
				}
			});
		}

		this.applyMapping = function (mapping) {
			var mapped = mapHelper.getMapped(self.sourceFields, self.destinationFields, mapping, 'sourceField', 'destinationField');
			var sourceMapped = mapped[0];
			var destinationMapped = mapped[1];
			var destinationNotMapped = mapHelper.getNotMapped(self.destinationFields, mapping, 'destinationField');
			var sourceNotMapped = mapHelper.getNotMapped(self.sourceFields, mapping, 'sourceField');

			var sourceWithoutPair = mapped[2];
			var destinationWithoutPair = mapped[3];

			return new Promise(function (resolve, reject) {
				if (!sourceWithoutPair.length && !destinationWithoutPair.length) {
					resolve();
				} else {
					confirmRemovingNotMappedFields(sourceWithoutPair,
						destinationWithoutPair,
						() => {
							sourceMapped = sourceMapped.concat(sourceWithoutPair);
							destinationMapped = destinationMapped.concat(destinationWithoutPair);
							resolve();
						},
						() => {
							sourceNotMapped = sourceNotMapped.concat(sourceWithoutPair);
							destinationNotMapped = destinationNotMapped.concat(destinationWithoutPair);
							resolve();
						});
				}
			}).then(() => {
				self.workspaceFields(mapFields(destinationNotMapped));
				self.mappedWorkspace(mapFields(destinationMapped));
				self.sourceField(mapFields(sourceNotMapped));
				self.sourceMapped(mapFields(sourceMapped));

				if (destinationModel.WorkspaceHasChanged) {
					IP.message.notifyWithTimeout("We restored the fields mapping as destination workspace has changed", 5000);

					// mark change as handled
					destinationModel.WorkspaceHasChanged = false;
				}

				self.populateExtractedText();
			});
		};

		root.data.deferred().all(promises).then(
			function (result) {
				var destinationFields = result[0] || [],
					sourceFields = result[1] || [],
					mapping = result[2] || [];

				self.nativeFilePathOption(sourceFields);
				self.FolderPathImportProvider(sourceFields);

				self.destinationFields = destinationFields;
				self.sourceFields = sourceFields;

				// Setting the cached value for Non-Relativity Providers
				self.FolderPathSourceField(model.FolderPathSourceField);

				var types = mapFields(sourceFields);
				self.overlay(destinationFields);
				$.each(self.overlay(), function () {
					if (this.isIdentifier) {
						self.rdoIdentifier(this.name);
						self.selectedUniqueId(this.name);
					}
				});

				$.each(self.overlay(), function () {
					if (model.identifer == this.name) {
						self.selectedUniqueId(this.name);
					}
				});

				self.parentField(types);
				if (typeof (model.parentIdentifier) !== "undefined") {
					$.each(self.parentField(), function () {
						if (this.name === model.parentIdentifier) {
							self.selectedIdentifier(this.name);
							return false;
						}
					});
				} else {
					for (var i = 0; i < mapping.length; i++) {
						var a = mapping[i];
						if (a.fieldMapType === mapTypes.parent && self.hasParent) {
							self.selectedIdentifier(a.sourceField.displayName); break;
						}
					}
				}

				$.each(mapping, function () {
					if (this.fieldMapType == mapTypes.native && artifactTypeId == 10) {
						self.importNativeFile("true");
						self.importNativeFileCopyMode(self.importNativeFileCopyMode() === "CopyFiles" ||
							self.importNativeFileCopyMode() === "SetFileLinks" ? self.importNativeFileCopyMode() : "CopyFiles");
						self.nativeFilePathValue(this.sourceField.displayName);
						return false;
					}
				});

				if (typeof (model.nativeFilePathValue) !== "undefined") {
					$.each(sourceFields, function () {
						if (this.displayName === model.nativeFilePathValue)
							self.nativeFilePathValue(this.displayName);
					});
				}
				for (var i = 0; i < mapping.length; i++) {
					if (mapping[i].fieldMapType == mapTypes.identifier) {
						var identifierFromMapping =
							self.overlay().find(x => (x.name == mapping[i].destinationField.displayName) ||
								(x.fieldIdentifier == mapping[i].destinationField.fieldIdentifier))
						if (identifierFromMapping) {
							self.selectedUniqueId(identifierFromMapping.name);
						}
					}
				}

				mapping = $.map(mapping, function (value) {
					// Drop auxiliary mapping entries that don't have destination field specified (such as FolderPathInformation or NativeFilePath) as they shouldn't be displayed in the UI
					if (value.destinationField.fieldIdentifier === undefined && (value.fieldMapType === mapTypes.parent || value.fieldMapType === mapTypes.native)) {
						return null;
					}
					return value;
				});


				self.applyMapping(mapping);

				self.LongTextColumnThatContainsPathToFullText(model.LongTextColumnThatContainsPathToFullText);
				self.ExtractedTextFileEncoding(model.ExtractedTextFileEncoding || "utf-16");

				if (self.IsRelativityProvider() && (destinationModel.ProductionImport || self.IsProductionExport())) {
					self.ImageImport('true');
					root.utils.UI.disable("#copyImages", true);
				}

			}).fail(function (result) {
				IP.message.error.raise(result);
			});
		
		/********** Submit Validation**********/
		this.submit = function () {
			this.showErrors(true);
		};

		/********** WorkspaceFields control  **********/


		this.addSelectFields = function () {
			IP.workspaceFieldsControls.add(this.workspaceFields, this.workspaceFieldSelected, this.mappedWorkspace);
			self.populateExtractedText();
		}
		this.addToWorkspaceField = function () {
			IP.workspaceFieldsControls.add(this.mappedWorkspace, this.selectedMappedWorkspace, this.workspaceFields);
			self.populateExtractedText();
		}
		this.addAllWorkspaceFields = function () {
			IP.workspaceFieldsControls.addAll(this.workspaceFields, this.workspaceFieldSelected, this.mappedWorkspace);
			self.populateExtractedText();
		}
		this.addAlltoWorkspaceField = function () {
			IP.workspaceFieldsControls.addAll(this.mappedWorkspace, this.selectedMappedWorkspace, this.workspaceFields);
			self.populateExtractedText();
		}

		/********** Source Attribute control  **********/
		this.addToMappedSource = function () { IP.workspaceFieldsControls.add(this.sourceField, this.selectedSourceField, this.sourceMapped); };
		this.addToSourceField = function () { IP.workspaceFieldsControls.add(this.sourceMapped, this.selectedMappedSource, this.sourceField); };
		this.addSourceToMapped = function () { IP.workspaceFieldsControls.addAll(this.sourceField, this.selectedSourceField, this.sourceMapped); };
		this.addAlltoSourceField = function () { IP.workspaceFieldsControls.addAll(this.sourceMapped, this.selectedSourceField, this.sourceField); };
		this.moveMappedWorkspaceUp = function () { IP.workspaceFieldsControls.up(this.mappedWorkspace, this.selectedMappedWorkspace); };
		this.moveMappedWorkspaceDown = function () { IP.workspaceFieldsControls.down(this.mappedWorkspace, this.selectedMappedWorkspace); };
		this.moveMappedSourceUp = function () { IP.workspaceFieldsControls.up(this.sourceMapped, this.selectedMappedSource); };
		this.moveMappedSourceDown = function () { IP.workspaceFieldsControls.down(this.sourceMapped, this.selectedMappedSource); };
		this.moveMappedWorkspaceTop = function () {
			IP.workspaceFieldsControls.moveTop(this.mappedWorkspace, this.selectedMappedWorkspace());
		};
		this.moveMappedWorkspaceBottom = function () {
			IP.workspaceFieldsControls.moveBottom(this.mappedWorkspace, this.selectedMappedWorkspace());
		};
		this.moveMappedSourceTop = function () {
			IP.workspaceFieldsControls.moveTop(this.sourceMapped, this.selectedMappedSource());
		};
		this.moveMappedSourceBottom = function () {
			IP.workspaceFieldsControls.moveBottom(this.sourceMapped, this.selectedMappedSource());
		};

		/********** AutoMap Controls  **********/
		this.autoFieldMap = function () {
			self.autoFieldMapWithCustomOptions();
		};

		this.autoMapFieldsFromSavedSearch = function () {
			self.autoMapFieldsFromSavedSearchWithCustomOptions();
		};

		this.autoFieldMapWithCustomOptions = function (matchOnlyIdentifierFields) {
			//Remove current mappings first
			const showErrors = self.showErrors();
			self.showErrors(false);
			self.addAlltoSourceField();
			self.addAlltoWorkspaceField();

			var fieldForAutomap = function (field) {
				return field.classificationLevel == 0;
			};

			root.data.ajax({
				type: 'POST', url: root.utils.generateWebAPIURL('/FieldMappings/AutomapFields', model.destinationProviderGuid),
				data: JSON.stringify({
					SourceFields: this.sourceFields.filter(fieldForAutomap),
					DestinationFields: this.destinationFields.filter(fieldForAutomap),
					MatchOnlyIdentifiers: !!matchOnlyIdentifierFields
				})
			}).then(function (mapping) {
				self.applyMapping(mapping);
				self.showErrors(showErrors);
				IP.mappingType = mappingType.Automap;
				return mapping;
			}, function () {
				self.showErrors(showErrors);
			});
		};

		this.autoMapFieldsFromSavedSearchWithCustomOptions = function () {
			//Remove current mappings first
			const showErrors = self.showErrors();
			self.showErrors(false);
			self.addAlltoSourceField();
			self.addAlltoWorkspaceField();

			var fieldForAutomap = function (field) {
				return field.classificationLevel == 0;
			};

			const sourceConfig = JSON.parse(model.sourceConfiguration);
			const savedSearchArtifactID = sourceConfig.SavedSearchArtifactId;

			root.data.ajax({
				type: 'POST', url: root.utils.generateWebAPIURL('/FieldMappings/AutomapFieldsFromSavedSearch', savedSearchArtifactID, model.destinationProviderGuid),
				data: JSON.stringify({
					SourceFields: this.sourceFields.filter(fieldForAutomap),
					DestinationFields: this.destinationFields.filter(fieldForAutomap)
				})
			}).then(function (mapping) {
				self.applyMapping(mapping);
				self.showErrors(showErrors);
				IP.mappingType = mappingType.SavedSearch;
				return mapping;
			}, function () {
				self.showErrors(showErrors);
			});
		};


		this.autoMapFieldsFromView = function () {
			//Remove current mappings first
			const showErrors = self.showErrors();
			self.showErrors(false);
			self.addAlltoSourceField();
			self.addAlltoWorkspaceField();

			var fieldForAutomap = function (field) {
				return field.classificationLevel == 0;
			};

			const sourceConfig = JSON.parse(model.sourceConfiguration);
			const viewArtifactID = sourceConfig.SourceViewId;

			root.data.ajax({
				type: 'POST', url: root.utils.generateWebAPIURL('/FieldMappings/AutomapFieldsFromView', viewArtifactID, model.destinationProviderGuid),
				data: JSON.stringify({
					SourceFields: this.sourceFields.filter(fieldForAutomap),
					DestinationFields: this.destinationFields.filter(fieldForAutomap)
				})
			})
			.then(function (mapping) {
				self.applyMapping(mapping);
				self.showErrors(showErrors);
				IP.mappingType = mappingType.View;
				return mapping;
			})
			.fail(function (error) {
				console.log(error);
				self.showErrors(showErrors);
			});
		};


		/********** Tooltips  **********/
		var settingsTooltipViewModel = new TooltipViewModel(TooltipDefs.RelativityProviderSettingsDetails, TooltipDefs.RelativityProviderSettingsDetailsTitle);

		Picker.create("Tooltip", "tooltipSettingsId", "TooltipView", settingsTooltipViewModel);

		this.openRelativityProviderSettingsTooltip = function (data, event) {
			settingsTooltipViewModel.open(event);
		};
		
	};// end of the viewmodel



	var Step = function (settings) {
		function setCache(model, key) {
			//we only want to cache the fields this page is in charge of
			stepCache[key] = {
				map: model.map,
				parentIdentifier: model.parentIdentifier,
				identifer: model.identifer,
				EntityManagerFieldContainsLink: model.EntityManagerFieldContainsLink,
				importNativeFile: model.importNativeFile,
				importNativeFileCopyMode: model.importNativeFileCopyMode,
				nativeFilePathValue: model.nativeFilePathValue,
				UseFolderPathInformation: model.UseFolderPathInformation,
				UseDynamicFolderPath: model.UseDynamicFolderPath,
				SelectedOverwrite: model.SelectedOverwrite,
				FieldOverlayBehavior: model.FieldOverlayBehavior,
				FolderPathSourceField: model.FolderPathSourceField,
				LongTextColumnThatContainsPathToFullText: model.LongTextColumnThatContainsPathToFullText,
				ExtractedTextFieldContainsFilePath: model.ExtractedTextFieldContainsFilePath,
				ExtractedTextFileEncoding: model.ExtractedTextFileEncoding
			};
		}

		var stepCache = {};
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.hasBeenLoaded = false;
		this.returnModel = {};
		this.bus = IP.frameMessaging();
		this.key = "";
		this.loadModel = function (model) {
			this.key = JSON.parse(model.destination).artifactTypeID;
			if (typeof (stepCache[this.key]) === "undefined") {

				setCache(model, this.key);
			}
			this.returnModel = $.extend(true, {}, model);

			var c = stepCache[this.key];
			for (var k in c) {
				if (c.hasOwnProperty(k)) {
					this.returnModel[k] = c[k];
				}
			}
			this.model = new viewModel(this.returnModel);
			this.model.errors = ko.validation.group(this.model, { deep: true });

			self.model.showMapSavedSearchButton(self.model.IsSavedSearchExport());
		};

		var relativityImportType;
		IP.frameMessaging().subscribe('importType', function (importType) {
			relativityImportType = importType;
		});

		this.getTemplate = function () {
			// If import provider and non-document type, we want to skip the field mapping step
			if (this.returnModel.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636" &&
				relativityImportType !== 0) {
				return;
			};
			self.settings.url =
				IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3');
			self.settings.templateID = "step3";

			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {

				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
				IP.affects.hover();
				self.model.onDOMLoaded();
			});
		};

		this.bus.subscribe("saveState", function (state) {
		});

		var _createEntry = function (field) {
			return {
				displayName: field.name,
				isIdentifier: field.isIdentifier,
				fieldIdentifier: field.identifer,
				isRequired: field.isRequired,
				type: field.type
			}
		};

		var _addNativePathFieldToMapping = function (map, nativePathField) {

			var nativePathMapping = undefined;
			for (var i = 0; i < map.length; i++) {
				if (map[i].sourceField.fieldIdentifier === nativePathField.identifer) {
					nativePathMapping = map[i];
					break;
				}
			}

			if (nativePathMapping === undefined) {
				map.push({
					sourceField: _createEntry(nativePathField),
					destinationField: {},
					fieldMapType: "NativeFilePath"
				});
			} else {
				nativePathMapping.fieldMapType = "NativeFilePath";
			}
		}

		this.back = function () {
			var d = root.data.deferred().defer();
			this.returnModel.importNativeFile = this.model.importNativeFile();
			this.returnModel.importNativeFileCopyMode = this.model.importNativeFileCopyMode();
			this.returnModel.nativeFilePathValue = this.model.nativeFilePathValue();
			this.returnModel.identifer = this.model.selectedUniqueId();
			this.returnModel.parentIdentifier = this.model.selectedIdentifier();
			this.returnModel.SelectedOverwrite = this.model.SelectedOverwrite();
			this.returnModel.UseFolderPathInformation = this.model.UseFolderPathInformation();
			this.returnModel.UseDynamicFolderPath = this.model.UseDynamicFolderPath();
			this.returnModel.FolderPathSourceField = this.model.FolderPathSourceField();
			this.returnModel.MoveExistingDocuments = this.model.MoveExistingDocuments();
			this.returnModel.LongTextColumnThatContainsPathToFullText = this.model.LongTextColumnThatContainsPathToFullText();
			this.returnModel.ExtractedTextFieldContainsFilePath = this.model.ExtractedTextFieldContainsFilePath();
			this.returnModel.ExtractedTextFileEncoding = this.model.ExtractedTextFileEncoding();
			this.returnModel.ImageImport = this.model.ImageImport();
			this.returnModel.ImagePrecedence = this.model.ImagePrecedence();
			this.returnModel.IncludeOriginalImages = this.model.IncludeOriginalImages();
			this.returnModel.ProductionPrecedence = this.model.ProductionPrecedence();


			var map = [];
			var emptyField = { name: '', identifer: '' };
			var maxMapFieldLength = Math.max(this.model.mappedWorkspace().length, this.model.sourceMapped().length);//make sure we grab the left overs
			for (var i = 0; i < maxMapFieldLength; i++) {
				var workspace = this.model.mappedWorkspace()[i] || emptyField;
				var source = this.model.sourceMapped()[i] || emptyField;
				map.push({
					sourceField: _createEntry(source),
					destinationField: _createEntry(workspace),
					fieldMapType: "None"
				});
			}

			this.returnModel.map = JSON.stringify(map);
			this.returnModel.EntityManagerFieldContainsLink = this.model.EntityManagerFieldContainsLink();
			setCache(this.returnModel, self.key);
			d.resolve(this.returnModel);
			return d.promise;
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			this.model.submit();
			var modelErrors = this.model.errors();
			if (modelErrors.length === 0) {
				var mapping = ko.toJS(self.model);
				var map = [];
				var allSourceField = mapping.sourceField.concat(mapping.sourceMapped);
				for (var i = 0; i < mapping.sourceMapped.length; i++) {
					var source = mapping.sourceMapped[i];
					var destination = mapping.mappedWorkspace[i];

					if (mapping.selectedUniqueId === destination.name) {
						map.push({
							sourceField: _createEntry(source),
							destinationField: _createEntry(destination),
							fieldMapType: "Identifier"
						});
					} else {
						map.push({
							sourceField: _createEntry(source),
							destinationField: _createEntry(destination),
							fieldMapType: "None"
						});
					}
				}
				if (mapping.hasParent) {
					var allSource = mapping.sourceField.concat(mapping.sourceMapped);
					for (var i = 0; i < allSource.length; i++) {
						if (mapping.selectedIdentifier === allSource[i].name) {
							map.push({
								sourceField: _createEntry(allSource[i]),
								destinationField: {},
								fieldMapType: "Parent"
							});
						}
					}
				}

				var _destination = JSON.parse(this.returnModel.destination);

				// specific to document type
				if (this.model.isDocument) {
					// pushing native file setting
					if (this.model.importNativeFile() == "true") {
						var nativePathField = "";
						for (var i = 0; i < allSourceField.length; i++) {
							if (allSourceField[i].name === this.model.nativeFilePathValue()) {
								nativePathField = allSourceField[i];

								//If we are the import provider, we need to take the identifier and put it in the source config
								//This will allow the provider to convert any relative paths to absolute.
								if (this.returnModel.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636") {
									var importConfig = JSON.parse(this.returnModel.sourceConfiguration);
									$.extend(importConfig, { NativeFilePathFieldIdentifier: nativePathField.identifer });
									this.returnModel.sourceConfiguration = JSON.stringify(importConfig);
								}
								break;
							}
						}
						if (nativePathField !== "") {
							_addNativePathFieldToMapping(map, nativePathField);

						}
					}

					AddFolderPathInfoToMapping(map);

					_destination.ImportOverwriteMode = ko.toJS(this.model.SelectedOverwrite).replace('/', '').replace(' ', '');
					_destination.importNativeFile = this.model.importNativeFile();
					_destination.importNativeFileCopyMode = this.model.importNativeFileCopyMode();

					// pushing create folder setting
					_destination.UseFolderPathInformation = this.model.UseFolderPathInformation();
					_destination.UseDynamicFolderPath = this.model.UseDynamicFolderPath();
					_destination.FolderPathSourceField = this.model.FolderPathSourceField();
					_destination.ImageImport = this.model.ImageImport();
					_destination.ImagePrecedence = this.model.ImagePrecedence();
					_destination.ProductionPrecedence = this.model.ProductionPrecedence();
					_destination.IncludeOriginalImages = this.model.IncludeOriginalImages();
					_destination.IdentifierField = this.model.IdentifierField();
					_destination.MoveExistingDocuments = this.model.MoveExistingDocuments();

					// pushing extracted text location setting
					_destination.ExtractedTextFieldContainsFilePath = this.model.ExtractedTextFieldContainsFilePath();
					_destination.ExtractedTextFileEncoding = this.model.ExtractedTextFileEncoding();
					_destination.LongTextColumnThatContainsPathToFullText = this.model.LongTextColumnThatContainsPathToFullText();

				}

				this.bus.subscribe('saveComplete', function (data) {
				});
				this.bus.subscribe('saveError', function (error) {
					d.reject(error);
				});

				//If we are the import provider, we need to take the identifier and put it in the source config
				//This will allow the provider to convert any relative paths to absolute.
				if (this.returnModel.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636") {
					if (_destination.ExtractedTextFieldContainsFilePath === 'true') {
						var importConfig = JSON.parse(this.returnModel.sourceConfiguration);
						//get the field identifier for the source field that contians the extracted text path
						for (var i = 0; i < map.length; i++) {
							if (map[i].destinationField.displayName === _destination.LongTextColumnThatContainsPathToFullText) {
								$.extend(importConfig, { ExtractedTextPathFieldIdentifier: map[i].sourceField.fieldIdentifier });
								break;
							}
						}
						this.returnModel.sourceConfiguration = JSON.stringify(importConfig);
					}
				}

				this.returnModel.map = JSON.stringify(map);
				this.returnModel.identifer = this.model.selectedUniqueId();
				this.returnModel.parentIdentifier = this.model.selectedIdentifier();
				this.returnModel.SelectedOverwrite = this.model.SelectedOverwrite();
				_destination.EntityManagerFieldContainsLink = this.model.EntityManagerFieldContainsLink();
				_destination.FieldOverlayBehavior = this.model.FieldOverlayBehavior();
				this.returnModel.destination = JSON.stringify(_destination);
				this.returnModel.SecuredConfiguration = this.model.SecuredConfiguration;
				this.returnModel.CreateSavedSearchForTagging = this.model.CreateSavedSearchForTagging;

				if (this.model.IsRelativityProvider()) {

					this.returnModel.destinationWorkspaceChanged = JSON.parse(this.returnModel.destination).WorkspaceHasChanged;

					var validateMappedFields = root.data.ajax({
						type: 'POST',
						url: root.utils.generateWebAPIURL('FieldMappings/Validate', _destination.CaseArtifactId, this.returnModel.destinationProviderGuid, this.returnModel.artifactTypeID, _destination.DestinationArtifactTypeId),
						data: JSON.stringify(map)
					})
						.fail(function (error) {
							IP.message.error.raise("Could not validate mapped fields");
						});

					const proceedConfirmation = function(validationResult) {
						if (validationResult.invalidMappedFields.length > 0 ||
							!validationResult.isObjectIdentifierMapValid) {
							var proceedCallback = function() {
								this.returnModel.mappingHasWarnings = true;
								d.resolve(this.returnModel);
							}.bind(this);

							var clearAndProceedCallback = function() {
								var filteredOutInvalidFields = map.filter(
									x => validationResult.invalidMappedFields.map(f => f.fieldMap).findIndex(
										i => StepMapFieldsValidator.isFieldMapEqual(i, x)) ==
										-1);
								this.returnModel.proceedAndClearClicked = true;
								this.returnModel.map = JSON.stringify(filteredOutInvalidFields);
								d.resolve(this.returnModel);
							}.bind(this);

							StepMapFieldsValidator.showProceedConfirmationPopup(validationResult.invalidMappedFields,
								validationResult.isObjectIdentifierMapValid,
								proceedCallback,
								clearAndProceedCallback);
						} else {
							d.resolve(this.returnModel);
						}
					};

					validateMappedFields.then(proceedConfirmation.bind(this));
				} else {
					d.resolve(this.returnModel);
				}
			} else {
				this.model.errors.showAllMessages();
				d.reject();
			}
			return d.promise;
		};
	};

	var AddFolderPathInfoToMapping = function (map) {
		if (step.model.UseFolderPathInformation() == "true") {
			var folderPathField = "";
			var folderPathFields = step.model.FolderPathFields();
			for (var i = 0; i < folderPathFields.length; i++) {
				if (folderPathFields[i].fieldIdentifier === step.model.FolderPathSourceField()) {
					folderPathField = folderPathFields[i];
					break;
				}
			}

			var sourceFields = step.model.FolderPathImportProvider();
			for (var k = 0; k < sourceFields.length; k++) {
				if (sourceFields[k].fieldIdentifier === step.model.FolderPathSourceField()) {
					folderPathField = sourceFields[k];
					break;
				}
			}

			// update fieldMapType if folderPath is in mapping field
			var containsFolderPathInMapping = false;
			for (var index = 0; index < map.length; index++) {
				if (map[index].sourceField.fieldIdentifier === folderPathField.fieldIdentifier) {
					map[index].fieldMapType = "FolderPathInformation";
					containsFolderPathInMapping = true;
					break;
				}
			}

			// create folder path entry if it is not in the field
			if (containsFolderPathInMapping === false) {
				var entry =
				{
					displayName: folderPathField.actualName,
					isIdentifier: "false",
					fieldIdentifier: folderPathField.fieldIdentifier,
					isRequired: "false"
				}
				map.push({
					sourceField: entry,
					destinationField: {},
					fieldMapType: "FolderPathInformation"
				});
			}
		}
	}

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3'),
		templateID: 'step3'
	});


	IP.messaging.subscribe('back', function () {

	});
	root.points.steps.push(step);

	//Added to make field mapping logic available to Import provider settings page
	window.top.getCurrentIpFieldMapping = function () {
		step.back();
		var currentMapping = {};
		currentMapping = JSON.parse(step.returnModel.map);
		AddFolderPathInfoToMapping(currentMapping);

		return JSON.stringify(currentMapping);
	};

	window.top.getExtractedTextInfo = function () {
		var extractedTextInfo = {};
		if (step.returnModel.ExtractedTextFieldContainsFilePath == 'true') {
			extractedTextInfo.LongTextColumnThatContainsPathToFullText = step.returnModel.LongTextColumnThatContainsPathToFullText;
			extractedTextInfo.ExtractedTextFileEncoding = step.returnModel.ExtractedTextFileEncoding;
		}

		return extractedTextInfo;
	};

	window.top.getMappedChoiceFieldsPromise = function () {
		return IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('FolderPath', 'GetChoiceFields') }).then(function (result) {
			var choiceFields = [];
			var totalChoiceFields = {};
			totalChoiceFields = result;
			var currentMapping = JSON.parse(step.returnModel.map);
			for (var i = 0; i < totalChoiceFields.length; i++) {
				for (var j = 0; j < currentMapping.length; j++) {
					if (totalChoiceFields[i].displayName === currentMapping[j].destinationField.displayName) {
						choiceFields.push(totalChoiceFields[i].displayName);
						break;
					}
				}
			}
			return choiceFields;
		});
	};

})(IP, ko);