var IP = IP || {};

ko.validation.rules.pattern.message = 'Invalid.';

ko.validation.configure({
	registerExtenders: true,
	messagesOnModified: true,
	insertMessages: true,
	parseInputAttributes: true,
	messageTemplate: null
});
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
		return { name: entry.displayName, identifer: entry.fieldIdentifier };
	}

	var mapFields = function (result) {
		return $.map(result, function (entry) {
			return mapField(entry);
		});
	}

	var moveItemFromField = function (from, to) {
		$.each(from, function () {
			to.push(this);
		});
	}


	var viewModel = function (model) {
		var self = this;
		var artifactTypeId = model.destination.artifactTypeId;
		var artifactId = model.artifactID || 0;
		this.workspaceFields = ko.observableArray([]);
		this.mappedWorkspace = ko.observableArray([]);
		this.sourceMapped = ko.observableArray([]);
		this.sourceField = ko.observableArray([]);
		this.selectedWorkspaceField = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);//.extend({ equal: selectedMappedWorkspace });
		this.overlay = ko.observableArray([]);
		this.selectedOverlay = ko.observable();
		this.hasParent = ko.observable(false);
		this.parentIdentifier = ko.observable();

		var workspaceFieldPromise = root.data.ajax({ type: 'POST', url: root.utils.generateWebAPIURL('WorkspaceField'), data: JSON.stringify({ settings: model.destination }) }).then(function (result) {
			var types = mapFields(result);
			self.overlay(types);
			return result;
		});
		var sourceFieldPromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('SourceFields'), data: { 'options': JSON.stringify({ artifactTypeID: artifactTypeId }), 'type': JSON.stringify({ artifactTypeID: artifactTypeId }) } });
		
		var mappedSourcePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('FieldMap', artifactId) });
				
		var destination = JSON.parse(model.destination);
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('rdometa', destination.artifactTypeID) }).then(function (result) {
			self.hasParent(result.hasParent);
		});
		var promises = [workspaceFieldPromise, sourceFieldPromise, mappedSourcePromise];

		var mapHelper = (function () {
			function find(fields, fieldMapping, key, func) {
				return $.grep(fields, function (item) {
					var remove = false;
					$.each(fieldMapping, function () {
						if (this[key].fieldIdentifier === item.fieldIdentifier) {
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

			function getMapped(fields, fieldMapping, key) {
				return find(fields, fieldMapping, key, function (r) { return r });
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

				var destinationMapped = mapHelper.getMapped(destinationFields, mapping, 'destinationField');
				var destinationNotMapped = mapHelper.getNotMapped(destinationFields, mapping, 'destinationField');
				var sourceMapped = mapHelper.getMapped(sourceFields, mapping, 'sourceField');
				var sourceNotMapped = mapHelper.getNotMapped(sourceFields, mapping, 'sourceField');

				self.workspaceFields(mapFields(destinationNotMapped));
				self.mappedWorkspace(mapFields(destinationMapped));
				self.sourceField(mapFields(sourceNotMapped));
				self.sourceMapped(mapFields(sourceMapped));
			});
		
		/********** Submit Validation**********/
		this.submit = function () {

		}
		/********** WorkspaceFields control  **********/

		this.addSelectFields = function () {
			var requested = this.mappedWorkspace;
			moveItemFromField(self.selectedWorkspaceField(), requested);
			this.workspaceFields.removeAll(self.selectedWorkspaceField());
			this.selectedWorkspaceField.splice(0, this.selectedWorkspaceField().length);
		}
		this.addToWorkspaceField = function () {
			var requested = this.workspaceFields;
			moveItemFromField(self.selectedMappedWorkspace(), requested);
			this.mappedWorkspace.removeAll(self.selectedMappedWorkspace());
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}
		this.addAllWorkspaceFields = function () {
			var requested = this.mappedWorkspace;
			moveItemFromField(self.workspaceFields(), requested);
			this.workspaceFields.removeAll();
			this.selectedWorkspaceField.splice(0, this.selectedWorkspaceField().length);
		}

		this.addAlltoWorkspaceField = function () {
			var requested = this.workspaceFields;
			moveItemFromField(self.mappedWorkspace(), requested);
			this.mappedWorkspace.removeAll();
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}

		/********** Source Attribute control  **********/
		this.addToMappedSource = function () {
			var requested = this.sourceMapped;
			moveItemFromField(self.selectedSourceField(), requested);
			this.sourceField.removeAll(self.selectedSourceField());
			this.selectedSourceField.splice(0, this.selectedSourceField().length);
		}

		this.addToSourceField = function () {
			var requested = this.sourceField;
			moveItemFromField(self.selectedMappedSource(), requested);
			this.sourceMapped.removeAll(self.selectedMappedSource());
			this.selectedMappedSource.splice(0, this.selectedMappedSource().length);
		}

		this.addSourceToMapped = function () {
			var requested = this.sourceMapped;
			moveItemFromField(self.sourceField(), requested);
			this.sourceField.removeAll();
			this.selectedSourceField.splice(0, this.selectedSourceField.length);
		}

		this.addAlltoSourceField = function () {
			var requested = this.sourceField;
			moveItemFromField(self.sourceMapped(), requested);
			this.sourceMapped.removeAll();
			this.selectedMappedSource.splice(0, this.selectedMappedSource().length);
		}

		this.moveMappedWorkspaceUp = function () {
			for (var j = 0; j < this.selectedMappedWorkspace().length ; j++) {
				var i = this.mappedWorkspace.indexOf(this.selectedMappedWorkspace()[j]);
				if (i >= 1) {
					var array = this.mappedWorkspace();
					this.mappedWorkspace.splice(i - 1, 2, array[i], array[i - 1]);
				} else {
					break;
				}
			}
		}

		this.moveMappedWorkspaceDown = function () {
			for (var j = this.selectedMappedWorkspace().length - 1; j >= 0 ; j--) {
				var i = this.mappedWorkspace().indexOf(this.selectedMappedWorkspace()[j]);
				var length = this.mappedWorkspace().length - 1;
				if ((i + 1) <= length) {
					var array = this.mappedWorkspace();
					this.mappedWorkspace.splice(i, 2, array[i + 1], array[i]);
				} else {
					break;
				}
			}
		}

		this.moveMappedSourceUp = function () {
			for (var j = 0; j < this.selectedMappedSource().length ; j++) {
				var i = this.sourceMapped.indexOf(this.selectedMappedSource()[j]);
				if (i >= 1) {
					var array = this.sourceMapped();
					this.sourceMapped.splice(i - 1, 2, array[i], array[i - 1]);
				} else {
					break;
				}
			}
		}

		this.moveMappedSourceDown = function () {
			for (var j = this.selectedMappedSource().length - 1; j >= 0 ; j--) {
				var i = this.sourceMapped().indexOf(this.selectedMappedSource()[j]);
				var length = this.sourceMapped().length - 1;

				if ((i + 1) <= length) {
					var array = this.sourceMapped();
					this.sourceMapped.splice(i, 2, array[i + 1], array[i]);
				} else {
					break;
				}
			}
		}

	};// end of the viewmodel


	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.returnModel = {};
		this.loadModel = function (model) {
			this.selectedRdo = model.destination.selectedRdo;
			this.returnModel = model;
			this.model = new viewModel(this.returnModel);
		};

		this.getTemplate = function () {
			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
			});
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			var mapping = ko.toJS(this.model);
			var map = [];
			for (var i = 0; i < mapping.sourceMapped.length; i++) {
				var source = mapping.sourceMapped[i];
				var destination = mapping.mappedWorkspace[i];
				map.push({
					sourceField: {
						displayName: source.name,
						fieldIdentifier: source.identifer
					},
					destinationField: {
						displayName: destination.name,
						fieldIdentifier: destination.identifer
					}
				});
			}

			this.returnModel.map = JSON.stringify(map);
			d.resolve(this.returnModel);
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3'),
		templateID: 'step3'
	});

	root.points.steps.push(step);


})(IP, ko);