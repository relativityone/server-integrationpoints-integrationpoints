$(function () {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	var viewModel;

	//An event raised when the user has clicked the Next or Save button.
	message.subscribe('submit', function () {
		//Execute save logic that persists the state.

		if (viewModel.errors().length === 0) {
			//Communicate to the host page that it to continue.
			this.publish('saveComplete', viewModel.getselectedOption());
		} else {
			viewModel.errors.showAllMessages();
		}
	});

	//An event raised when a user clicks the Back button.
	message.subscribe('back', function () {
		//Execute save logic that persists the state.
		this.publish('saveState', getModel());
	});

	//An event raised when the host page has loaded the current settings page.
	message.subscribe('load', function (m) {
		var _bind = function (m) {
			viewModel = new Model(m);
			ko.applyBindings(viewModel, document.getElementById('relativityProviderConfiguration'));
		}
		_bind({});
	});

	var Model = function (root, m) {
		var state = $.extend({}, {}, m);
		var self = this;

		this.workspaces = ko.observableArray();
		this.savedSearches = ko.observableArray();

		self.savedSearches = this.savedSearches;
		self.workspaces = this.workspaces;

		// load savedsearches
		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('SavedSearchFinder') }).then(function (result) {
			self.savedSearches(result);
		});

		// load workspaces
		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('WorkspaceFinder') }).then(function (result) {
			self.workspaces(result);
		});

		this.selectedWorkspace = ko.observable(state.selectedWorkspace).extend({
			required: true
		});

		this.selectedSavedSearch = ko.observable(state.selectedSavedSearch).extend({
			required: true
		});

		self.selectedSavedSearch = this.selectedSavedSearch;
		self.selectedWorkspace = this.selectedWorkspace;
		
		this.errors = ko.validation.group(this, { deep: true });
		this.getselectedOption = function() {
			return {
				"WorkspaceArtifactId": self.selectedWorkspace,
				"SavedSearch": self.selectedSavedSearch
			}
		}
	}
});
