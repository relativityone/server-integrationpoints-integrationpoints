$(function (root) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	var viewModel;

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.
		this.publish("saveState", JSON.stringify(ko.toJS(viewModel)));

		if (viewModel.errors().length === 0) {
			//Communicate to the host page that it to continue.
			this.publish('saveComplete', JSON.stringify(viewModel.getSelectedOption()));
		} else {
			viewModel.errors.showAllMessages();
		}

		// Modify destination object to contain target workspaceId
		var destinationJson = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination;
		var destination = JSON.parse(destinationJson);
		destination.CaseArtifactId = viewModel.TargetWorkspaceArtifactId();
		destination.Provider = "relativity";
		destination.DoNotUseFieldsMapCache = viewModel.WorkspaceHasChanged;
		destinationJson = JSON.stringify(destination);
		IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination = destinationJson;
	});

	//An event raised when a user clicks the Back button.
	message.subscribe('back', function () {
		//Execute save logic that persists the state.
		this.publish('saveState', JSON.stringify(ko.toJS(viewModel)));
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (m) {
		var _bind = function (m) {
			viewModel = new Model(m);
			ko.applyBindings(viewModel, document.getElementById('relativityProviderConfiguration'));
		}

		// expect model to be serialized to string
		if (typeof m === "string") {
			try {
				m = JSON.parse(m);
			} catch (e) {
				m = undefined;
			}
			_bind(m);
		} else {
			_bind({});
		}
	});

	var Model = function (m) {

		var state = $.extend({}, {}, m);
		var self = this;

		this.workspaces = ko.observableArray(state.workspaces);
		this.savedSearches = ko.observableArray(state.savedSearches);

		this.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId).extend({
			required: true
		});


		this.TargetWorkspaceArtifactId.subscribe(function (value) {
			if (self.TargetWorkspaceArtifactId !== value) {
				self.WorkspaceHasChanged = true;
			}
		});


		this.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId).extend({
			required: true
		});

		if (self.savedSearches.length === 0) {
			// load saved searches
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('SavedSearchFinder') }).then(function (result) {
				self.savedSearches(result);

			});
		}
			
		if (self.workspaces.length === 0) {
			// load workspaces
			IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('WorkspaceFinder') }).then(function (result) {
				self.workspaces(result);
			});
		}
		
		this.errors = ko.validation.group(this, { deep: true });
		this.getSelectedOption = function() {
			return {
				"SavedSearchArtifactId": self.SavedSearchArtifactId(),
				"SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
				"TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
			}
		}
	}
});
