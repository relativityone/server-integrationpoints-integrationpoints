$(function (root) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging();

	var viewModel;
	IP.frameMessaging().dFrame.IP.reverseMapFields = true;// set the flag so that the fields can be reversed;
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
		this.disable = IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun();

		this.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId);
		this.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId);

		if (!(this.disable)) {
			this.TargetWorkspaceArtifactId.extend({
				required: true
			});
			this.SavedSearchArtifactId.extend({
				required: true
			});
		}

		this.TargetWorkspaceArtifactId.subscribe(function (value) {
			if (self.TargetWorkspaceArtifactId !== value) {
				self.WorkspaceHasChanged = true;
			}
		});

		if (self.savedSearches.length === 0) {
			(function loadSavedSearches(savedSearchArtifactId, isDisabled) {
				IP.data.ajax({
					type: 'GET',
					url: IP.utils.generateWebAPIURL('SavedSearchFinder'),
					async: true,
					success: function (result) {
						self.savedSearches(result);

						if (isDisabled) {
							var availableSavedSearches = self.savedSearches();
							var isArtifactIdInList = doesArtifactIdExistInObjectList(availableSavedSearches, savedSearchArtifactId);

							if (!isArtifactIdInList) {
								var message = "Unable to access the saved search. Please verify saved search permissions, or create a new integration point if the search no longer exists.";
								IP.frameMessaging().dFrame.IP.message.error.raise(message);
							}
						}
					},
					error: function () {
						self.savedSearches = undefined;
					}
				});
			})(this.SavedSearchArtifactId(), this.disable);
		}

		if (self.workspaces.length === 0) {
			(function loadWorkspaces(targetWorkspaceArtifactId, isDisabled) {
				IP.data.ajax({
					type: 'GET',
					url: IP.utils.generateWebAPIURL('WorkspaceFinder'),
					async: true,
					success: function (result) {
						self.workspaces(result);

						if (isDisabled) {
							var availableWorkspaces = self.workspaces();
							var isArtifactIdInList = doesArtifactIdExistInObjectList(availableWorkspaces, targetWorkspaceArtifactId);

							if (!isArtifactIdInList) {
								var message = "The target workspace no longer exists. Please create a new Integration Point.";
								IP.frameMessaging().dFrame.IP.message.error.raise(message);
							}
						}
					},
					error: function () {
						self.workspaces = undefined;
					}
				});
			})(this.TargetWorkspaceArtifactId(), this.disable);
		}

		this.errors = ko.validation.group(this, { deep: true });
		this.getSelectedOption = function () {
			return {
				"SavedSearchArtifactId": self.SavedSearchArtifactId(),
				"SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
				"TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
			}
		}
	}

	function doesArtifactIdExistInObjectList(list, artifactId) {
		if (artifactId !== undefined) {
			for (var i = 0; i < list.length; i++) {
				if (list[i].value === artifactId) {
					return true;
				}
			}
		}
		return false;
	}
});
