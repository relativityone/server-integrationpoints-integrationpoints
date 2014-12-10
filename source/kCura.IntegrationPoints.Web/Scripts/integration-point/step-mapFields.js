var IP = IP || {};
(function (root, ko) {
	var viewModel = function () {
		var self = this;
		this.workspaceFields = ko.observableArray([]);
		this.mappedWorkspace = ko.observableArray([]);
		this.sourceMapped = ko.observableArray([]);
		this.sourceField = ko.observableArray([]);
		this.selectedWorkspaceField = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);
		this.selectedMappedSource = ko.observableArray([]);

		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('WorkspaceField') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return { name: entry.displayName, identifier: entry.fieldIdentifier };
			});
			self.workspaceFields(types);
		});
		
		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('WorkspaceField') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return {name: entry.displayName, identifier: entry.fieldIdentifier};
			});
			self.sourceField(types);
		});

		/********** WorkspaceFields control  **********/

		this.addSelectFields = function () {
			var requested = this.mappedWorkspace;
			$.each(self.selectedWorkspaceField(), function () {
				requested.push(this);
			});
			this.workspaceFields.removeAll(self.selectedWorkspaceField());
			this.selectedWorkspaceField.splice(0, this.selectedWorkspaceField().length);
		}
		this.addToWorkspaceField = function() {
			var requested = this.workspaceFields;
			$.each(self.selectedMappedWorkspace(), function (n, item) {
				requested.push(item);
			});
			this.mappedWorkspace.removeAll(self.selectedMappedWorkspace());
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}
		this.addAllWorkspaceFields = function (addAll) {
			var requested = this.mappedWorkspace;
			$.each(self.workspaceFields(), function (n, item) {
				requested.push(item);
			});
			this.workspaceFields.removeAll();
			this.selectedWorkspaceField.splice(0, this.selectedWorkspaceField().length);
		}

		this.addAlltoWorkspaceField = function() {
			var requested = this.workspaceFields ;
			$.each(self.mappedWorkspace(), function (n, item) {
				requested.push(item);
			});
			this.mappedWorkspace.removeAll();
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}

		/********** Source Attribute control  **********/
		this.addToMappedSource = function () {
			var requested = this.sourceMapped;
			$.each(self.selectedSourceField(), function (n, item) {
				requested.push(item);
			});
			this.sourceField.removeAll(self.selectedSourceField());
			this.selectedSourceField.splice(0, this.selectedSourceField().length);
		}
		this.addToSourceField = function () {
			var requested = this.sourceField;
			$.each(self.selectedMappedSource(), function (n, item) {
				requested.push(item);
			});
			this.sourceMapped.removeAll(self.selectedMappedSource());
			this.selectedMappedSource.splice(0, this.selectedMappedSource().length);
		}

		this.addSourceToMapped = function () {
			var requested = this.sourceMapped;
			$.each(self.sourceField(), function (n, item) {
				requested.push(item);
			});
			this.sourceField.removeAll();
			this.selectedSourceField.splice(0, this.selectedSourceField.length);
		}
		
		this.addAlltoSourceField = function (addAll) {
			var requested = this.sourceField;
			$.each(self.sourceMapped(), function (n, item) {
				requested.push(item);
			});
			this.sourceMapped.removeAll();
			alert(this.selectedMappedSource().length);
			this.selectedMappedSource.splice(0, this.selectedMappedSource().length);
			
		}

		this.moveUp = function () {
			
			var i = this.mappedWorkspace.indexOf(this.selectedMappedWorkspace()[0]);
			if (i >= 1) {
				var array = this.mappedWorkspace();
				this.mappedWorkspace.splice(i - 1, 2, array[i], array[i - 1]);
				this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
			}	
		}
		
		this.moveDown = function () {
			var i = this.mappedWorkspace.indexOf(this.selectedMappedWorkspace()[0]);
			if (i >= this.mappedWorkspace.length) {
				var array = this.mappedWorkspace();
				this.mappedWorkspace.splice(i, 2, array[i + 1], array[i]);
				this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
			}
			
		}
		

	};// end of the viewmodel



	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = new viewModel();
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