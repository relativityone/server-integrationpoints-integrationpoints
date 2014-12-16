var IP = IP || {};
(function (root, ko) {


	var mapFields = function(result) {
		return  $.map(result, function (entry) {
			return { name: entry.displayName, identifier: entry.fieldIdentifier };
		});
	}

	var moveItemFromField = function(from, to) {
		$.each(from, function () {
			to.push(this);
		});
	}
	

	var viewModel = function (model) {
		var self = this;
		var artifactTypeId = model.destination.selectedRdo;
		var artifactId = model.artifactID || 0; 
		this.workspaceFields = ko.observableArray([]);
		this.mappedWorkspace = ko.observableArray([]);
		this.sourceMapped = ko.observableArray([]);
		this.sourceField = ko.observableArray([]);
		this.selectedWorkspaceField = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);
		this.overlay = ko.observableArray([]);
		this.selectedOverlay = ko.observable();
		this.hasParent = ko.observable(true);
		
		var workspaceFieldPromise = root.data.ajax({ type: 'Get', url: root.utils.generateWebAPIURL('WorkspaceField/'), data: { 'json': JSON.stringify({ artifactTypeID: artifactTypeId }) } }).then(function (result) {
			var types = mapFields(result.data);
			
			self.workspaceFields(types);
			self.overlay(types);
			var selected = result.selected.fieldIdentifier;
			$.each(self.overlay(), function (index, entry) {
				if (entry.identifier === selected) {
					self.selectedOverlay(self.overlay()[index]);
				}
			});
		});

		var sourceFieldPromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('WorkspaceField/'), data: { 'json': JSON.stringify({ artifactTypeID: artifactTypeId }) } }).then(function (result) {
			var types = mapFields(result.data);
			self.sourceField(types);
		});

		
		var mappedSourcePromise = root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('FieldMap/' + artifactId) }).then(function (result) {
			var source = $.map(result, function (entry) {
				return {
					 name: entry.sourceField.displayName, identifier: entry.sourceField.fieldIdentifier
				};
			});
			var workspace = $.map(result, function(entry) {
				return {
					name: entry.destinationField.displayName, identifier: entry.destinationField.fieldIdentifier 
				};
			});
			self.mappedWorkspace(workspace);
			self.sourceMapped(source);
			});

		Q.all([workspaceFieldPromise, sourceFieldPromise, mappedSourcePromise]).done(
			function () {
				// remove the already source mapped fields 
				$.each(self.sourceField(), function (index, entry) {
					$.each(self.sourceMapped(), function (index2, mapped) {
						if (mapped.name === entry.name) {
							self.sourceField.remove(self.sourceField()[index]);
						}
					});
				});
	
				$.each(self.workspaceFields(), function (index, entry) {
					$.each(self.mappedWorkspace(), function (index2, mapped) {
						if (mapped.name === entry.name) {
							self.workspaceFields.remove(self.workspaceFields()[index]);
						}
					});
				});
				
			});
		
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
		this.addAllWorkspaceFields = function (addAll) {
			var requested = this.mappedWorkspace;
			moveItemFromField(self.workspaceFields(), requested);
			this.workspaceFields.removeAll();
			this.selectedWorkspaceField.splice(0, this.selectedWorkspaceField().length);
		}

		this.addAlltoWorkspaceField = function() {
			var requested = this.workspaceFields ;
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
		
		this.addAlltoSourceField = function (addAll) {
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
			for (var j = this.selectedMappedWorkspace().length-1; j >=0 ; j--) {
				var i = this.mappedWorkspace().indexOf(this.selectedMappedWorkspace()[j]);
				var length = this.mappedWorkspace().length-1;  
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
				if (i  >= 1) {
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
				var itemsSelected = this.selectedMappedSource().length;
				if ( (i + 1) <= length) {
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
		this.loadModel = function (model) {
			this.selectedRdo = model.destination.selectedRdo;
			this.model = new viewModel(model);
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
			d.resolve();
			return d.promise;
		};
	};


	
	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3'),
		templateID: 'step3'
	});
	
	root.points.steps.push(step);
	

})(IP, ko);