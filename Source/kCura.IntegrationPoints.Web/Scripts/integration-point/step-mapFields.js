﻿var IP = IP || {};

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
		var containsIdentifier = false;
		var rdoIdentifierMapped = false;
		$.each(value, function (index, item) {
			if (item.isIdentifier === true) {
				rdoIdentifierMapped = true;
			}
			if (item.name == params[1]()) {
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
				if (params[1]() !== params[0]()) {
					missingField = "The object identifier, " + params[1]() + ", and the unique identifier, " + params[0]();
				} else {
					missingField = "The object identifier, " + params[1]();
				}
			}
			if (!rdoIdentifierMapped && containsIdentifier) {
				missingField = "The object identifier, " + params[1]();
			}
			if (rdoIdentifierMapped && !containsIdentifier) {
				missingField = "The unique identifier, " + params[0]();
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

	function mapField(entry) {
		var fieldDisplayName = entry.displayName;
		if (!entry.isIdentifier && entry.type) {
			fieldDisplayName = entry.displayName + " [" + entry.type + "]";
		}

		return {
			name: entry.displayName,
			displayName: fieldDisplayName,
			type: entry.type,
			identifer: entry.fieldIdentifier,
			isIdentifier: entry.isIdentifier,
			isRequired: entry.isRequired
		};
	}

	var mapFields = function (result) {
		return $.map(result, function (entry) {
			return mapField(entry);
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

		this.selectedUniqueId = ko.observable().extend({ required: true });
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

		this.overlay = ko.observableArray([]);
		this.nativeFilePathOption = ko.observableArray([]);
		this.hasParent = ko.observable(false);
		this.parentField = ko.observableArray([]);

	    //We want to dispable options CopyFile, SetLinks and None for Copy Images job
		this.ImportNativeFileCopyModeEnabled = ko.observable(model.ImageImport === "true" ? "false" : "true");

		this.importNativeFile = ko.observable(model.importNativeFile || "false");

	    var setDefaultImportNativeFileCopyMode = function(importNativeFile, importNativeFileCopyMode) {
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


		//use this to bind which elements show up depending on if the user is accessing Relativity Provider or not
		this.IsRelativityProvider = ko.observable(IP.reverseMapFields);

        var copyNativeFileText = "Copy Native Files:";
		var copyFileToRepositoryText = "Copy Files to Repository:";
		this.copyNativeLabel = ko.observable(copyNativeFileText);
		this.ImageImport = ko.observable(model.ImageImport || "false");
		this.IsProductionExport = function () {
		    if (this.IsRelativityProvider()) {
		        var sourceModel = JSON.parse(model.sourceConfiguration);
		        if (sourceModel.TypeOfExport && sourceModel.TypeOfExport === ExportEnums.SourceOptionsEnum.Production) {
		            return true;
		        }
		    }
		    return false;
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
		this.ImageImportVisible = ko.observable("false");
		root.data.ajax({
			type: 'get',
			url: root.utils.generateWebAPIURL('ToggleAPI', 'kCura.IntegrationPoints.Web.Toggles.UI.ShowImageImportToggle'),
			success: function (result) {
				self.ImageImportVisible(result && self.IsRelativityProvider());
			}
		});


		this.ImageImport.subscribe(function (value) {
			setCopyFilesLabel(value);
			if (value === "true") {
				root.utils.UI.disable("#fieldMappings", true);
				self.UseFolderPathInformation("false");
				self.UseDynamicFolderPath("false");
				self.MoveExistingDocuments("false");
				self.FolderPathSourceField(null);
				self.autoFieldMapWithCustomOptions(function (identfier) {
					var name = identfier.name.replace(" [Object Identifier]", "");
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
					return self.showErrors() && self.hasParent();
				},
				message: 'The Parent Attribute is required.',
			}
		});
		this.showManager = ko.observable(false);
		this.cacheMapped = ko.observableArray([]);
		root.data.ajax({
			type: 'GET', url: root.utils.generateWebAPIURL('Entity/' + artifactTypeId)
		}).then(function (result) {
			self.showManager(result);
		});

		var workspaceFieldPromise = root.data.ajax({
			type: 'POST', url: root.utils.generateWebAPIURL('WorkspaceField'), data: JSON.stringify({
				settings: model.destination,
				credentials: self.SecuredConfiguration
			})
		}).then(function (result) {
			return result;
		});

		var sourceFieldPromise = root.data.ajax({
			type: 'Post', url: root.utils.generateWebAPIURL('SourceFields'), data: JSON.stringify({
				'options': model.sourceConfiguration,
				'type': model.source.selectedType,
				'credentials': self.SecuredConfiguration
			})
		}).fail(function (error) {
			IP.message.error.raise("No attributes were returned from the source provider.");
		});

		var destination = JSON.parse(model.destination);
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('rdometa', destination.artifactTypeID) }).then(function (result) {
			self.hasParent(result.hasParent);
		});

		this.destinationCaseArtifactID = destination.CaseArtifactId;

		self.findField = function(array, field) {
			const fields = $.grep(array, function (value, _index) { return value.fieldIdentifier === field.fieldIdentifier; });
			const fieldFound = fields.length > 0;
			return {
				exist: fieldFound,
				type: fieldFound ? fields[0].type : null,
				actualName: fieldFound ? fields[0].actualName : null,
				displayName: fieldFound ? fields[0].displayName : null
			};
		};

		self.updateFieldFromMapping = function(mappedField, fields) {
			var field = self.findField(fields, mappedField);
			if (field.exist) {
				mappedField.type = field.type;
				mappedField.actualName = field.actualName;
				mappedField.displayName = field.displayName;
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

		var promises = [workspaceFieldPromise, sourceFieldPromise, mappedSourcePromise];

		var mapTypes = {
			identifier: 'Identifier',
			parent: 'FolderPathInformation',
			native: 'NativeFilePath'
		};
		var mapHelper = (function () {
			function find(fields, fieldMapping, key, func) {
				return $.grep(fields,
					function(item) {
						var remove = false;
						$.each(fieldMapping,
							function() {
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
				$.each(fieldMapping, function (_index, mapping) {
					var sourceField = self.updateFieldFromMapping(mapping[sourceKey], sourceFields)
					if (!!sourceField) {
						sourceMapped.push(sourceField);
					}
					var destinationField = self.updateFieldFromMapping(mapping[destinationKey], destinationFields);
					if (!!destinationField) {
						destinationMapped.push(destinationField);
					}
				});
				return [destinationMapped, sourceMapped];
			}

			return {
				getNotMapped: getNotMapped,
				getMapped: getMapped
			};
		})();

		root.data.deferred().all(promises).then(
			function (result) {
				var destinationFields = result[0],
					sourceFields = result[1] || [],
					mapping = result[2];

				self.nativeFilePathOption(sourceFields);
				self.FolderPathImportProvider(sourceFields);

				// Setting the cached value for Non-Relativity Providers
				self.FolderPathSourceField(model.FolderPathSourceField);

				var types = mapFields(sourceFields);
				self.overlay(destinationFields);
				$.each(self.overlay(), function () {
					if (this.isIdentifier) {
						self.rdoIdentifier(this.displayName);
						self.selectedUniqueId(this.displayName);
					}
				});

				$.each(self.overlay(), function () {
					if (model.identifer == this.displayName) {
						self.selectedUniqueId(this.displayName);
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
						self.selectedUniqueId(mapping[i].destinationField.displayName);
					}
				}

				mapping = $.map(mapping, function (value) {
					// Drop auxiliary mapping entries that don't have destination field specified (such as FolderPathInformation or NativeFilePath) as they shouldn't be displayed in the UI
					if (value.destinationField.fieldIdentifier === undefined && (value.fieldMapType === mapTypes.parent || value.fieldMapType === mapTypes.native)) {
						return null;
					}
					return value;
				});


				var mapped = mapHelper.getMapped(sourceFields, destinationFields, mapping, 'sourceField', 'destinationField');
				var destinationMapped = mapped[0];
				var sourceMapped = mapped[1];
				var destinationNotMapped = mapHelper.getNotMapped(destinationFields, mapping, 'destinationField');
				var sourceNotMapped = mapHelper.getNotMapped(sourceFields, mapping, 'sourceField');

				self.workspaceFields(mapFields(destinationNotMapped));
				self.mappedWorkspace(mapFields(destinationMapped));
				self.sourceField(mapFields(sourceNotMapped));
				self.sourceMapped(mapFields(sourceMapped));
				self.populateExtractedText();
				self.LongTextColumnThatContainsPathToFullText(model.LongTextColumnThatContainsPathToFullText);
				self.ExtractedTextFileEncoding(model.ExtractedTextFileEncoding || "utf-16");

				if (self.IsRelativityProvider() && (destinationModel.ProductionImport || self.IsProductionExport())) {
					self.ImageImport('true');
					root.utils.UI.disable("#copyImages", true);
				}

			}).fail(function (result) {
			IP.message.error.raise(result);
		});

		this.GetCatalogFieldMappings = function () {
			self.CatalogField = {};
			var destinationWorkspaceID = IP.utils.getParameterByName('AppID', window.top);

			$.ajax({
				url: IP.utils.generateWebAPIURL('FieldCatalog', destinationWorkspaceID),
				type: 'POST',
				success: function (data) {
					self.CatalogField = data;
				},
				error: function (error) {
					console.log(error);
				}
			});
		};

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
		this.GetCatalogFieldMappings();
		this.autoFieldMap = function () {
			self.autoFieldMapWithCustomOptions();
		};
		this.autoFieldMapWithCustomOptions = function (matchOnlyIdentifierFields) {
			//Remove current mappings first
			self.addAlltoSourceField();
			self.addAlltoWorkspaceField();

			var isCatalogFieldMatch = function (wsFieldArtifactId, fieldName) {
				for (var x = 0; x < self.CatalogField.length; x++) {
					if (self.CatalogField[x].fieldArtifactId == wsFieldArtifactId &&
						self.CatalogField[x].friendlyName === fieldName) {
						return true;
					}
				}
				return false;
			};

			var sourceFieldToAdd = ko.observableArray([]);
			var wspaceFieldToAdd = ko.observableArray([]);
            for (var i = 0; i < self.sourceField().length; i++) {
                var currentSourceField = self.sourceField()[i];
                var currentWorkspaceField;
				var fieldAlreadyMatched = false;

				if (matchOnlyIdentifierFields) {
                    if (!currentSourceField.isIdentifier) {
						continue;
					}
                    matchOnlyIdentifierFields(currentSourceField);
				}

                var objectIdentifierFieldSuffix = " [Object Identifier]";

				//check for a match b/w the source and destination fields by identifier flag or name
                for (var j = 0; j < self.workspaceFields().length; j++) {
                    currentWorkspaceField = self.workspaceFields()[j];

					//check to make sure that we ignore any workspace field that's already added
                    if (wspaceFieldToAdd().indexOf(currentWorkspaceField) != -1) {
						continue;
					}

                    if ((currentSourceField.isIdentifier && currentWorkspaceField.isIdentifier) 
						|| 
                        (currentSourceField.name === currentWorkspaceField.name)
                        ||
                        (currentSourceField.name + objectIdentifierFieldSuffix === currentWorkspaceField.name))
					{
                        sourceFieldToAdd.push(currentSourceField);
                        wspaceFieldToAdd.push(currentWorkspaceField);
						fieldAlreadyMatched = true;
						break;
					}
				}

				//if we haven't found a match for the current source field by name, now check the field catalog
				if (!fieldAlreadyMatched) {
                    for (var k = 0; k < self.workspaceFields().length; k++) {
                        currentWorkspaceField = self.workspaceFields()[k];

						//check to make sure that we ignore any workspace field that's already added
                        if (wspaceFieldToAdd().indexOf(currentWorkspaceField) != -1) {
							continue;
						}

						var isCatalogFieldMatchResult;
						if (self.IsRelativityProvider()) {
                            isCatalogFieldMatchResult = isCatalogFieldMatch(currentSourceField.identifer, currentWorkspaceField.name);
						} else {
                            isCatalogFieldMatchResult = isCatalogFieldMatch(currentWorkspaceField.identifer, currentSourceField.name);
						}

						if (isCatalogFieldMatchResult) {
                            sourceFieldToAdd.push(currentSourceField);
                            wspaceFieldToAdd.push(currentWorkspaceField);
							break;
						}
					}
				}
			}

			if (sourceFieldToAdd().length > 0) {
				IP.workspaceFieldsControls.add(self.sourceField, sourceFieldToAdd, self.sourceMapped);
				IP.workspaceFieldsControls.add(self.workspaceFields, wspaceFieldToAdd, self.mappedWorkspace);
			}
			else {
				IP.message.error.raise("Unable to auto map. No matching fields found.");
			}
			self.populateExtractedText();
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
				isRequired: field.isRequired
			}
		};

	    var _addNativePathFieldToMapping = function(map, nativePathField) {

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
			if (this.model.errors().length === 0) {
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
					var mismatchedMappings = StepMapFieldsTypeValidator.validateMappedFieldTypes(mapping);

					if (mismatchedMappings.length > 0) {
						var successCallback = function() {
							d.resolve(this.returnModel);
						}.bind(this);
						StepMapFieldsTypeValidator.showWarningPopup(mismatchedMappings, successCallback);
					} else {
						d.resolve(this.returnModel);
					}
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