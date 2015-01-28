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
			IP.message.error.clear();
			return true;
		}
		if (!rdoIdentifierMapped || !containsIdentifier) {
			var missingField = !rdoIdentifierMapped ? params[2]() : params[1]();
			IP.message.error.raise('Error: The object identifier field, ' + missingField + ', must be mapped.');
			return false;
		}
		return true;
	},
	message: "The object identifier field must be mapped."
}
ko.validation.rules['mustContainIdentifer'] = {
	validator: function (value, params) {

		var errorMessage = "";
		if (value.length == 0) {
			IP.message.error.raise('The object identifier field must be mapped.');
			return false;
		}

	},
	message: 'The object identifier field must be mapped.'
}
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
		return { name: entry.displayName, identifer: entry.fieldIdentifier, isIdentifier: entry.isIdentifier };
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
		var artifactTypeId = model.destination.artifactTypeId;
		var artifactId = model.artifactID || 0;
		this.workspaceFields = ko.observableArray([]);
		this.selectedUniqueId = ko.observable().extend({ required: true });
		this.rdoIdentifier = ko.observable();
		this.isAppendOverlay = ko.observable(true);
		this.mappedWorkspace = ko.observableArray([]).extend({
			mustContainIdentifer: {
				onlyIf: function () {
					return self.showErrors() && self.mappedWorkspace().length == 0;
				},
				params: [model.selectedOverwrite, self.selectedUniqueId, self.rdoIdentifier]
			},
			uniqueIdIsMapped: {
				onlyIf: function () {
					return self.showErrors() && self.mappedWorkspace().length > 0;
				},
				params: [model.selectedOverwrite, self.selectedUniqueId, self.rdoIdentifier]
			}
		});

		this.sourceMapped = ko.observableArray([]).extend({
			mustEqualMapped: {
				onlyIf: function () {
					return self.showErrors();
				},
				params: this.mappedWorkspace
			}
		});

		this.sourceField = ko.observableArray([]);
		this.selectedWorkspaceField = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);
		this.overlay = ko.observableArray([]);
		this.hasParent = ko.observable(false);
		this.parentField = ko.observableArray([]);
		this.selectedIdentifier = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.showErrors() && self.hasParent();
				},
				message: 'The Parent Attribute is required.',
			}
		});

		this.cacheMapped = ko.observableArray([]);
		var workspaceFieldPromise = root.data.ajax({
			type: 'POST', url: root.utils.generateWebAPIURL('WorkspaceField'), data: JSON.stringify({
				settings: model.destination
			})
		}).then(function (result) {
			var types = mapFields(result);
			self.overlay(types);

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
			return result;
		});
		var sourceFieldPromise = root.data.ajax({
			type: 'Post', url: root.utils.generateWebAPIURL('SourceFields'), data: JSON.stringify({
				'options': model.sourceConfiguration,
				'type': model.source.selectedType
			})
		});
		var mappedSourcePromise;

		if (typeof (model.map) === "undefined") {
			mappedSourcePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('FieldMap', artifactId) });
		} else {
			mappedSourcePromise = jQuery.parseJSON(model.map);
		}

		var destination = JSON.parse(model.destination);
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('rdometa', destination.artifactTypeID) }).then(function (result) {
			self.hasParent(result.hasParent);

		});
		var promises = [workspaceFieldPromise, sourceFieldPromise, mappedSourcePromise];

		var mapTypes = {
			identifier: 1,
			parent: 2
		};


		var mapHelper = (function () {
			function find(fields, fieldMapping, key, func) {
				return $.grep(fields, function (item) {
					var remove = false;
					$.each(fieldMapping, function () {
						if (this[key].fieldIdentifier === item.fieldIdentifier && this["fieldMapType"] !== mapTypes.parent) {
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
				var orphan = [];
				$.each(fieldMapping, function (item) {
					var source = this[sourceKey];
					var _destination = this[destinationKey];
					var isInSource = _contains(sourceField, source);
					var isInDestination = _contains(destinationFields, _destination);
					if (isInSource && isInDestination) {
						sourceMapped.push(source);
						destinationMapped.push(_destination);
					}
					else if (!isInSource && isInDestination) {
						orphan.push(_destination);
					}
				});
				return [destinationMapped.concat(orphan), sourceMapped];
			}
			return {
				getNotMapped: getNotMapped,
				getMapped: getMapped
			};
		})();
		root.data.deferred().all(promises).then(
			function (result) {
				var destinationFields = result[0],
						sourceFields = result[1],
						mapping = result[2];

				var types = mapFields(sourceFields);

				self.parentField(types);
				if (model.parentIdentifier !== undefined) {
					$.each(self.parentField(), function () {
						if (this.name === model.parentIdentifier) {
							self.selectedIdentifier(this.name);
						}
					});
				} else {
					for (var i = 0; i < mapping.length; i++) {
						var a = mapping[i];
						if (a.fieldMapType === mapTypes.parent && self.hasParent) {
							self.selectedIdentifier(a.sourceField.displayName);
						}
					}
				}
				for (var i = 0; i < mapping.length; i++) {
					if (mapping[i].fieldMapType == mapTypes.identifier) {
						self.selectedUniqueId(mapping[i].destinationField.displayName);
					}
				}
				mapping = $.map(mapping, function (value) {
					return value.fieldMapType !== mapTypes.parent ? value : null;
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
				self.isAppendOverlay(model.selectedOverwrite !== "Append");
			}).fail(function (result) {
				IP.message.error.raise(result);
			});
		/********** Submit Validation**********/
		this.submit = function () {

			this.showErrors(true);
		}
		/********** WorkspaceFields control  **********/


		this.addSelectFields = function () { IP.workspaceFieldsControls.add(this.workspaceFields, this.selectedWorkspaceField, this.mappedWorkspace); }
		this.addToWorkspaceField = function () { IP.workspaceFieldsControls.add(this.mappedWorkspace, this.selectedMappedWorkspace, this.workspaceFields); }
		this.addAllWorkspaceFields = function () { IP.workspaceFieldsControls.addAll(this.workspaceFields, this.selectedWorkspaceField, this.mappedWorkspace); }
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

	};// end of the viewmodel



	var Step = function (settings) {
		function setCache(model, key) {
			//we only want to cache the fields this page is incharge of
			stepCache[key] = {
				map: model.map,
				parentIdentifier: model.parentIdentifier,
				identifer: model.identifer
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
			this.hasBeenLoaded = false;

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
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {

				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
			});
		};

		this.bus.subscribe("saveState", function (state) {
		});

		this.back = function () {
			var d = root.data.deferred().defer();

			this.returnModel.identifer = this.model.selectedUniqueId();
			this.returnModel.parentIdentifier = this.model.selectedIdentifier();
			var map = [];
			var emptyField = { name: '', identifer: '' };
			var maxMapFieldLength = Math.max(this.model.mappedWorkspace().length, this.model.sourceMapped().length);//make sure we grab the left overs
			for (var i = 0; i < maxMapFieldLength; i++) {
				var workspace = this.model.mappedWorkspace()[i] || emptyField;
				var source = this.model.sourceMapped()[i] || emptyField;
				map.push({
					sourceField: {
						displayName: source.name,
						isIdentifier: source.isIdentifier,
						fieldIdentifier: source.identifer
					},
					destinationField: {
						displayName: workspace.name,
						isIdentifier: workspace.isIdentifier,
						fieldIdentifier: workspace.identifer,
					},
					fieldMapType: "None"
				});
			}

			this.returnModel.map = JSON.stringify(map);

			setCache(this.returnModel, self.key);

			d.resolve(this.returnModel);
			return d.promise;
		}


		this.submit = function () {
			var d = root.data.deferred().defer();
			this.model.submit();
			if (this.model.errors().length === 0) {
				var mapping = ko.toJS(self.model);
				var map = [];

				for (var i = 0; i < mapping.sourceMapped.length; i++) {
					var source = mapping.sourceMapped[i];
					var destination = mapping.mappedWorkspace[i];
					if (mapping.selectedUniqueId === destination.name) {
						map.push({
							sourceField: {
								displayName: source.name,
								isIdentifier: source.isIdentifier,
								fieldIdentifier: source.identifer
							},
							destinationField: {
								displayName: destination.name,
								isIdentifier: destination.isIdentifier,
								fieldIdentifier: destination.identifer,
							},
							fieldMapType: "Identifier"
						});
					} else {
						map.push({
							sourceField: {
								displayName: source.name,
								isIdentifier: source.isIdentifier,
								fieldIdentifier: source.identifer
							},
							destinationField: {
								displayName: destination.name,
								isIdentifier: destination.isIdentifier,
								fieldIdentifier: destination.identifer
							},
							fieldMapType: "None"
						});
					}
				}
				if (mapping.hasParent) {
					var allSource = mapping.sourceField.concat(mapping.sourceMapped);
					for (var i = 0; i < allSource.length; i++) {
						if (mapping.selectedIdentifier === allSource[i].name) {
							map.push({
								sourceField: {
									displayName: allSource[i].name,
									isIdentifier: allSource[i].isIdentifier,
									fieldIdentifier: allSource[i].identifer
								},
								destinationField: {

								},
								fieldMapType: "Parent"
							});
						}
					}
				}
				this.bus.subscribe('saveComplete', function (data) {
				});
				this.bus.subscribe('saveError', function (error) {
					d.reject(error);
				});
				this.returnModel.map = JSON.stringify(map);
				this.returnModel.identifer = this.model.selectedUniqueId();
				this.returnModel.parentIdentifier = this.model.selectedIdentifier();

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