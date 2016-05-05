﻿/// <reference path="../jquery-1.8.2.min.js" />
var IP = IP || {};

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
	message: 'All selected items have not been mapped.'
};

ko.validation.rules['identiferMustMappedWithAnotherIdentifier'] = {
	validator: function (value, params) {
		var tagetMap = params();
		var isMappedCorrectly = false;
		$.each(value, function (index, item) {
			if (item.isIdentifier === true) {
				isMappedCorrectly = tagetMap[index].isIdentifier;
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
		return {
			name: entry.displayName,
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
		this.hasBeenLoaded = model.hasBeenLoaded;
		this.showErrors = ko.observable(false);
		var artifactTypeId = JSON.parse(model.destination).artifactTypeID;
		var artifactId = model.artifactID || 0;
		this.workspaceFields = ko.observableArray([]).extend({
			fieldsMustBeMapped: {
				onlyIf: function () {
					return self.showErrors();
				}
			}
		});

		this.AllowUserToMapNativeFileField = ko.observable(model.SourceProviderConfiguration.importSettingVisibility.allowUserToMapNativeFileField || true);

		this.selectedUniqueId = ko.observable().extend({ required: true });
		this.rdoIdentifier = ko.observable();
		this.isAppendOverlay = ko.observable(true);

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
			identiferMustMappedWithAnotherIdentifier: {
				onlyIf: function () {
					return self.showErrors();
				},
				params: this.mappedWorkspace
			}
		});

		if (typeof model.CustodianManagerFieldContainsLink === "undefined") {
			model.CustodianManagerFieldContainsLink = "true";
		}

		this.CustodianManagerFieldContainsLink = ko.observable(model.CustodianManagerFieldContainsLink || "false");
		this.sourceField = ko.observableArray([]);
		this.workspaceFieldSelected = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);
		this.overlay = ko.observableArray([]);
		this.nativeFilePathOption = ko.observableArray([]);
		this.hasParent = ko.observable(false);
		this.parentField = ko.observableArray([]);

		this.importNativeFile = ko.observable(model.importNativeFile || "false");

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
				self.FolderPathSourceField(null);
			} else {
				self.FieldOverlayBehavior('Use Field Settings');
			}
		});

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

		this.ExtractedTextFieldContainsFilePath = ko.observable(model.ExtractedTextFieldContainsFilePath || "false");
		this.ExtractedTextFileEncoding = ko.observable(model.ExtractedTextFileEncoding || "utf-16").extend(
		{
			required : {
				onlyIf: function() {
					return self.ExtractedTextFieldContainsFilePath() === 'true';
				}
			}
		});

		this.ExtractedTextFileEncodingList = ko.observableArray([]);
		if (self.ExtractedTextFileEncodingList.length === 0) {
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
				self.ExtractedTextFileEncodingList(result);
			});
		}

		this.nativeFilePathValue = ko.observableArray([]).extend({
			required: {
				onlyIf: function () {
					return self.AllowUserToMapNativeFileField == true && (self.importNativeFile() == 'true' || self.importNativeFile == true) && self.showErrors();
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
			type: 'POST', url: root.utils.generateWebAPIURL('Custodian/' + artifactTypeId)
		}).then(function (result) {
			self.showManager(result);
		});

		var workspaceFieldPromise = root.data.ajax({
			type: 'POST', url: root.utils.generateWebAPIURL('WorkspaceField'), data: JSON.stringify({
				settings: model.destination
			})
		}).then(function (result) {
			return result;
		});

		var sourceFieldPromise = root.data.ajax({
			type: 'Post', url: root.utils.generateWebAPIURL('SourceFields'), data: JSON.stringify({
				'options': model.sourceConfiguration,
				'type': model.source.selectedType,
			})
		}).fail(function (error) {
			IP.message.error.raise("No attributes were returned from the source provider.");
		});

		var destination = JSON.parse(model.destination);
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('rdometa', destination.artifactTypeID) }).then(function (result) {
			self.hasParent(result.hasParent);
		});

		var mappedSourcePromise;
		if (destination.DoNotUseFieldsMapCache) {
			mappedSourcePromise = [];
		} else {
			if (typeof (model.map) === "undefined") {
				mappedSourcePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('FieldMap', artifactId) });
			} else {
				mappedSourcePromise = jQuery.parseJSON(model.map);
			}
		}

		var promises = [workspaceFieldPromise, sourceFieldPromise, mappedSourcePromise];

		var mapTypes = {
			identifier: 1,
			parent: 2,
			native: 3
		};
		var mapHelper = (function () {
			function find(fields, fieldMapping, key, func) {
				return $.grep(fields, function (item) {
					var remove = false;
					$.each(fieldMapping, function () {
						if (this[key].fieldIdentifier === item.fieldIdentifier && this["fieldMapType"] !== mapTypes.parent && this["fieldMapType"] !== mapTypes.native) {
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
			function getMapped(sourceField, destinationFields, fieldMapping, sourceKey, destinationKey) {
				function _contains(array, field) {
					return $.grep(array, function (value, index) { return value.fieldIdentifier == field.fieldIdentifier; }).length > 0; //I wish underscore was an option
				}
				var sourceMapped = [];
				var destinationMapped = [];
				$.each(fieldMapping, function (item) {
					var source = this[sourceKey];
					var destination = this[destinationKey];
					var isInSource = _contains(sourceField, source);
					var isInDestination = _contains(destinationFields, destination);
					if (isInSource) {
						sourceMapped.push(source);
					}
					if (isInDestination) {
						destinationMapped.push(destination);
					}
				});
				return [destinationMapped, sourceMapped];
			}
			return {
				getNotMapped: getNotMapped,
				getMapped: getMapped,
			};


		})();

		root.data.deferred().all(promises).then(
			function (result) {
				var destinationFields = result[0],
						sourceFields = result[1] || [],
						mapping = result[2];

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
							self.selectedIdentifier(a.sourceField.displayName);break;
						}
					}
				}

				$.each(mapping, function () {
					if (this.fieldMapType == 3 && artifactTypeId == 10) {
						self.importNativeFile("true");
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
					return (value.fieldMapType !== mapTypes.parent && value.fieldMapType !== mapTypes.native) ? value : null;
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
			}).fail(function (result) {
				IP.message.error.raise(result);
			});
		/********** Submit Validation**********/
		this.submit = function () {
			this.showErrors(true);
		};

		/********** WorkspaceFields control  **********/


		this.addSelectFields = function () { IP.workspaceFieldsControls.add(this.workspaceFields, this.workspaceFieldSelected, this.mappedWorkspace); }
		this.addToWorkspaceField = function () { IP.workspaceFieldsControls.add(this.mappedWorkspace, this.selectedMappedWorkspace, this.workspaceFields); }
		this.addAllWorkspaceFields = function () { IP.workspaceFieldsControls.addAll(this.workspaceFields, this.workspaceFieldSelected, this.mappedWorkspace); }
		this.addAlltoWorkspaceField = function () { IP.workspaceFieldsControls.addAll(this.mappedWorkspace, this.selectedMappedWorkspace, this.workspaceFields); }

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

	};// end of the viewmodel



	var Step = function (settings) {
		function setCache(model, key) {
			//we only want to cache the fields this page is in charge of
			stepCache[key] = {
				map: model.map,
				parentIdentifier: model.parentIdentifier,
				identifer: model.identifer,
				CustodianManagerFieldContainsLink: model.CustodianManagerFieldContainsLink,
				importNativeFile: model.importNativeFile,
				nativeFilePathValue: model.nativeFilePathValue,
				UseFolderPathInformation: model.UseFolderPathInformation,
				SelectedOverwrite: model.SelectedOverwrite,
				FieldOverlayBehavior : model.FieldOverlayBehavior,
				FolderPathSourceField: model.FolderPathSourceField,
				ExtractedTextFieldContainsFilePath: model.ExtractedTextFieldContainsFilePath,
				ExtractedTextFileEncoding: model.ExtractedTextFileEncoding
			} || '';
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
		this.getTemplate = function () {
		    if (IP.reverseMapFields) {  
		            self.settings.url=
		        IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3Reversed');
self.settings.templateID = "step4";
		    }else{
				 self.settings.url=
		        IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3');
				self.settings.templateID = "step3";
			}
            
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				
				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
				IP.affects.hover();
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

		this.back = function () {
			var d = root.data.deferred().defer();
			this.returnModel.importNativeFile = this.model.importNativeFile();
			this.returnModel.nativeFilePathValue = this.model.nativeFilePathValue();
			this.returnModel.identifer = this.model.selectedUniqueId();
			this.returnModel.parentIdentifier = this.model.selectedIdentifier();
			this.returnModel.SelectedOverwrite = this.model.SelectedOverwrite();
			this.returnModel.UseFolderPathInformation = this.model.UseFolderPathInformation();
			this.returnModel.FolderPathSourceField = this.model.FolderPathSourceField();
			this.returnModel.ExtractedTextFieldContainsFilePath = this.model.ExtractedTextFieldContainsFilePath();
			this.returnModel.ExtractedTextFileEncoding = this.model.ExtractedTextFileEncoding();

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
			this.returnModel.CustodianManagerFieldContainsLink = this.model.CustodianManagerFieldContainsLink();
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
				var allSourceField = mapping.sourceField.concat(mapping.selectedSourceField);
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
				if (this.model.isDocument){
				    // pushing native file setting
					if (this.model.importNativeFile() == "true") {
						var nativePathField = "";
						for (var i = 0; i < allSourceField.length; i++) {
							if (allSourceField[i].name === this.model.nativeFilePathValue()) {
								nativePathField = allSourceField[i];
							}
						}
						if (nativePathField !== "") {
							map.push({
								sourceField: _createEntry(nativePathField),
								destinationField: {},
								fieldMapType: "NativeFilePath"
							});
						}
					}

					if (this.model.UseFolderPathInformation() == "true") {
						var folderPathField = "";
						var folderPathFields = this.model.FolderPathFields();
						for (var i = 0; i < folderPathFields.length; i++) {
							if (folderPathFields[i].fieldIdentifier === this.model.FolderPathSourceField()) {
								folderPathField = folderPathFields[i];
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

					_destination.ImportOverwriteMode = ko.toJS(this.model.SelectedOverwrite).replace('/', '').replace(' ', '');
					_destination.importNativeFile = this.model.importNativeFile();

					// pushing create folder setting
					_destination.UseFolderPathInformation = this.model.UseFolderPathInformation();
					_destination.FolderPathSourceField = this.model.FolderPathSourceField();

					// pushing extracted text location setting
					_destination.ExtractedTextFieldContainsFilePath = this.model.ExtractedTextFieldContainsFilePath();
					_destination.ExtractedTextFileEncoding = this.model.ExtractedTextFileEncoding();
				}

				this.bus.subscribe('saveComplete', function (data) {
				});
				this.bus.subscribe('saveError', function (error) {
					d.reject(error);
				});

				this.returnModel.map = JSON.stringify(map);
				this.returnModel.identifer = this.model.selectedUniqueId();
				this.returnModel.parentIdentifier = this.model.selectedIdentifier();
				this.returnModel.SelectedOverwrite = this.model.SelectedOverwrite();
				_destination.CustodianManagerFieldContainsLink = this.model.CustodianManagerFieldContainsLink();
				_destination.FieldOverlayBehavior = this.model.FieldOverlayBehavior();
				this.returnModel.destination = JSON.stringify(_destination);
				d.resolve(this.returnModel);
			} else {
				this.model.errors.showAllMessages();
				d.reject();
			}
			return d.promise;
		};
	};
    
        var step = new Step({
            url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3'),
            templateID: 'step3'
        });
    
	
	IP.messaging.subscribe('back', function () {

	});
	root.points.steps.push(step);

})(IP, ko);




