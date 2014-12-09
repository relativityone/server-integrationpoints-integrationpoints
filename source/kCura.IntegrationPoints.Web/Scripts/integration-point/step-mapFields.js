var IP = IP || {};
(function (root, ko) {



	var Field = function (name, identifier) {
		this.name = name;
		this.identifier = identifier;

	}
	
	var viewModel = function () {
		var self = this;
		this.workspaceFields = ko.observableArray([]);
		this.selectedWorkspaceFields = ko.observableArray([]);
		this.mappedWorkspace = ko.observableArray([]);
		this.selectedMappedWorkspace = ko.observableArray([]);
		this.sourceMapped = ko.observableArray([]);
		this.sourceField = ko.observableArray([]);
		this.selectedSourceMapped = ko.observableArray([]);
		this.selectedSourceField = ko.observableArray([]);


		root.data.ajax({ type: 'get', url: root.utils.generateWebAPIURL('WorkspaceField') }).then(function (result) {
			var types = $.map(result, function (entry) {
				return new Field(entry.displayName, entry.fieldIdentifier);
			});
			self.workspaceFields(types);
		});
		

		/********** WorkspaceFields control  **********/

		this.addSelectFields = function () {
			var requested = this.mappedWorkspace;
			$.each(self.selectedWorkspaceFields(), function (n, item) {
			
				requested.push(item);
			});
			this.workspaceFields.removeAll(self.selectedWorkspaceFields());
			this.selectedWorkspaceFields.splice(0, this.selectedWorkspaceFields().length);
		}

		this.addAllWorkspaceFields = function (addAll) {
			var requested = this.mappedWorkspace;
			$.each(self.workspaceFields(), function (n, item) {
			
				requested.push(item);
			});
			this.workspaceFields.removeAll();
			this.selectedWorkspaceFields.splice(0, this.selectedWorkspaceFields().length);
		}

		this.addToWorkspaceField = function() {
			var requested = this.workspaceFields;
			$.each(self.selectedMappedWorkspace(), function (n, item) {

				requested.push(item);
			});
			this.mappedWorkspace.removeAll(self.selectedMappedWorkspace());
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
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
			var requested = this.mappedWorkspace;
			$.each(self.selectedSourceField(), function (n, item) {
				requested.push(item);
			});
			this.workspaceFields.removeAll(self.selectedWorkspaceFields());
			this.selectedWorkspaceFields.splice(0, this.selectedWorkspaceFields().length);
		}

		this.addAllSourceFields = function (addAll) {
			var requested = this.mappedWorkspace;
			$.each(self.workspaceFields(), function (n, item) {

				requested.push(item);
			});
			this.workspaceFields.removeAll();
			this.selectedWorkspaceFields.splice(0, this.selectedWorkspaceFields().length);
		}

		this.addToSourceField = function () {
			var requested = this.workspaceFields;
			$.each(self.selectedMappedWorkspace(), function (n, item) {

				requested.push(item);
			});
			this.mappedWorkspace.removeAll(self.selectedMappedWorkspace());
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}

		this.addAlltoSourceField = function () {
			var requested = this.workspaceFields;
			$.each(self.mappedWorkspace(), function (n, item) {
				requested.push(item);
			});
			this.mappedWorkspace.removeAll();
			this.selectedMappedWorkspace.splice(0, this.selectedMappedWorkspace().length);
		}

	};// end of the viewmodel



	var step = function (settings) {
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


	
	var step = new step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails3'),
		templateID: 'step3'
	});
	
	root.points.steps.push(step);
	

})(IP, ko);