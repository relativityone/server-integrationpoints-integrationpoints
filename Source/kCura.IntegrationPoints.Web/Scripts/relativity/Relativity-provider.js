$(function (root) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging(); // handle into the global Integration point framework
	ko.validation.configure({
		registerExtenders: true,
		messagesOnModified: true,
		insertMessages: true,
		parseInputAttributes: true,
		messageTemplate: null
	});
	ko.validation.insertValidationMessage = function (element) {
		var errorContainer = document.createElement('div');
		var iconSpan = document.createElement('span');
		iconSpan.className = 'icon-error legal-hold field-validation-error';

		errorContainer.appendChild(iconSpan);

		$(element).parents('.editFieldValue').eq(0).append(errorContainer);

		return iconSpan;
	};

	// custom validation rules 
	ko.validation.rules['checkWorkspace'] = {
		validator: function (value, params) {
			var isArtifactIdInList = doesArtifactIdExistInObjectList(params.workspaces(), value);
			if (!isArtifactIdInList) {
				return false;
			}
			return true;
		},
		message: 'The target workspace is no longer accessible. Please verify your settings or create a new Integration Point.'
	};
	ko.validation.rules['checkSavedSearch'] = {
		validator: function (value, params) {
			var isArtifactIdInList = doesArtifactIdExistInObjectList(params.savedSearches(), value);
			if (!isArtifactIdInList) {
				return false;
			}
			return true;
		},
		message: 'The saved search is no longer accessible. Please verify your settings or create a new Integration Point.'
	};
	ko.validation.registerExtenders();

	var viewModel;

	message.dFrame.IP.reverseMapFields = true; // set the flag so that the fields can be reversed;
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
		destination.DestinationFolderArtifactId = viewModel.FolderArtifactId();
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
			viewModel.onDOMLoaded();
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

		self.workspaces = ko.observableArray(state.workspaces);
		self.savedSearches = ko.observableArray(state.savedSearches);
		self.disable = IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun();
		self.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId);
		self.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId);
		self.DestinationFolder = ko.observable(state.DestinationFolder);
		self.FolderArtifactId = ko.observable(state.FolderArtifactId);
		self.TargetFolder = ko.observable(state.TargetFolder);

		self.TargetWorkspaceArtifactId.subscribe(function (value) {
			if (value !== undefined) {
				self.getFolderAndSubFolders(value);
			}
		});

		self.getFolderAndSubFolders = function (destinationWorkspaceId) {
			IP.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("SearchFolder/GetFolders", destinationWorkspaceId)
			}).then(function (result) {
				if (!!self.TargetFolder() && self.TargetFolder().indexOf(result.text) === -1) {
					self.FolderArtifactId("");
					self.TargetFolder("");
				}
				self.foldersStructure = result;
				self.locationSelector.reload(result);
			}).fail(function (error) {
				IP.frameMessaging().dFrame.IP.message.error.raise(error);
			});
		};

		self.getFolderFullName = function (currentFolder, folderId) {
			if (currentFolder.id === folderId) {
				return currentFolder.text;
			} else {
				for (var i = 0; i < currentFolder.children.length; i++) {
					var childFolderPath = self.getFolderFullName(currentFolder.children[i], folderId);
					if (childFolderPath !== "") {
						return currentFolder.text + "/" + childFolderPath;
					}
				}
			}
			return "";
		};

		self.onDOMLoaded = function () {
			self.locationSelector = new LocationJSTreeSelector();
			self.locationSelector.init(self.TargetFolder(), [], {
				onNodeSelectedEventHandler: function (node) {
					self.FolderArtifactId(node.id);
					self.TargetFolder(self.getFolderFullName(self.foldersStructure, self.FolderArtifactId()));
				}
			});
			self.locationSelector.toggle(!self.disable);
		};

		// load the data first before preceding this could cause problems below when we try to do validation on fields
		if (self.savedSearches.length === 0) {
			IP.data.ajax({
				type: 'GET',
				url: IP.utils.generateWebAPIURL('SavedSearchFinder'),
				async: true,
				success: function (result) {
					self.savedSearches(result);
					self.SavedSearchArtifactId(state.SavedSearchArtifactId);
				},
				error: function () {
					IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve the saved searches. Please contact your system administrator.");
					self.savedSearches([]);
				}
			});
		}

		if (self.workspaces.length === 0) {
			IP.data.ajax({
				type: 'GET',
				url: IP.utils.generateWebAPIURL('WorkspaceFinder'),
				async: true,
				success: function (result) {
					ko.utils.arrayForEach(result, function (item) {
						item.displayName = IP.utils.decode(item.displayName);
					});
					self.workspaces(result);
					self.TargetWorkspaceArtifactId(state.TargetWorkspaceArtifactId);
					self.TargetWorkspaceArtifactId.subscribe(function (value) {
						if (self.TargetWorkspaceArtifactId !== value) {
							self.WorkspaceHasChanged = true;
						}
					});
				},
				error: function () {
					IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve the workspace information. Please contact your system administrator.");
					self.workspaces([]);
				}
			});
		}

		this.TargetWorkspaceArtifactId.extend({
			required: {
				onlyIf: function () {
					return !self.disable;
				}
			}
		}).extend({
			validation: {
				validator: function (value) {
					var workspaces = self.workspaces();
					if (typeof (workspaces) !== "undefined") {
						for (var i = 0; i < workspaces.length; i++) {
							if (workspaces[i].displayName.indexOf(';') != -1 && value == workspaces[i].value) {
								return false;
							}
						}
					}
					return true;
				},
				message: "Destination workspace name contains an invalid character. Please remove before continuing."
			}
		}).extend({
			validation: {
				validator: function (value) {
					var workspaces = self.workspaces();
					if (typeof (workspaces) !== "undefined") {
						var sourceId = IP.utils.getParameterByName('AppID', window.top);
						for (var i = 0; i < workspaces.length; i++) {
							if (workspaces[i].displayName.indexOf(';') != -1 && workspaces[i].value == sourceId) {
								return false;
							}
						}
					}
					return true;
				},
				message: "Source workspace name contains an invalid character. Please remove before continuing."
			}
		}).extend({
			checkWorkspace: {
				onlyIf: function () {
					return (typeof self.workspaces()) !== "undefined";
				},
				params: { workspaces: self.workspaces }
			}
		});

		this.SavedSearchArtifactId.extend({
			required: {
				onlyIf: function () {
					return !self.disable;
				}
			}
		}).extend({
			checkSavedSearch: {
				onlyIf: function () {
					return (typeof self.savedSearches()) !== "undefined";
				},
				params: { savedSearches: self.savedSearches }
			}
		});

		this.errors = ko.validation.group(this, { deep: true });
		this.getSelectedOption = function () {
			return {
				"SavedSearchArtifactId": self.SavedSearchArtifactId(),
				"SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
				"TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
				"FolderArtifactId": self.FolderArtifactId(),
				TargetFolder: self.TargetFolder()
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
