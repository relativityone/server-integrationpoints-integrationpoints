$(function (root) {
	//Create a new communication object that talks to the host page.
	var message = IP.frameMessaging(); // handle into the global Integration point framework
	var savedSearchService = new SavedSearchService();

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

		$(element).parents('.field-value').eq(0).append(errorContainer);

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
		async: true,
		validator: function (value, params, callback) {
			var okCallback = function(result) {
				callback(!!result);
			};
			var errorCallback = function() {
				callback({ isValid: false, message: 'Unable to validate if the saved search is accessible. Please try again.' });
			};
			savedSearchService.RetrieveSavedSearch(value, okCallback, errorCallback);
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
		var stepModel = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model;
		var destinationJson = stepModel.destination;
		var destination = JSON.parse(destinationJson);
		destination.FederatedInstanceArtifactId = viewModel.FederatedInstanceArtifactId();
		destination.CreateSavedSearchForTagging = viewModel.CreateSavedSearchForTagging();
		destination.CaseArtifactId = viewModel.TargetWorkspaceArtifactId();
		destination.DestinationFolderArtifactId = viewModel.FolderArtifactId();
		destination.ProductionImport = viewModel.ProductionImport();
		destination.ProductionArtifactId = viewModel.ProductionArtifactId();
		destination.Provider = "relativity";
		destination.DoNotUseFieldsMapCache = viewModel.WorkspaceHasChanged;

		destinationJson = JSON.stringify(destination);
		stepModel.destination = destinationJson;

		stepModel.SecuredConfiguration = viewModel.SecuredConfiguration();
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
		self.SavedSearchService = new SavedSearchService();

		self.federatedInstances = ko.observableArray(state.federatedInstances);
		self.workspaces = ko.observableArray(state.workspaces);
		self.FederatedInstanceArtifactId = ko.observable(state.FederatedInstanceArtifactId);
		self.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId);
		self.DestinationFolder = ko.observable(state.DestinationFolder);
		self.FolderArtifactId = ko.observable(state.FolderArtifactId);
		self.TargetFolder = ko.observable();
		self.SecuredConfiguration = ko.observable(state.SecuredConfiguration);
		self.ProductionImport = ko.observable(state.ProductionImport || false);//Import into production in destination workspace
		self.SourceProductionSets = ko.observableArray();
		self.EnableLocationRadio = ko.observable(state.EnableLocationRadio || false);
		self.LocationFolderChecked = ko.observable(state.LocationFolderChecked || "true");
		self.DestinationProductionSets = ko.observableArray();
		self.ProductionArtifactId = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.LocationFolderChecked() === "false";
				}
			}
		});
		self.ProductionArtifactId.subscribe(function (value) {
			self.ProductionImport(!!value);
			if (value) {
				self.FolderArtifactId(undefined);
				self.locationSelector.toggle(false);
			}
		});
		self.CreateSavedSearchForTagging = ko.observable(JSON.parse(IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination).CreateSavedSearchForTagging || "false");
		self.TypeOfExport = ko.observable();//todo:self.TypeOfExport = ko.observable(initTypeOfExport);
		self.IsSavedSearchSelected = function () {
			return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.SavedSearch;
		};
		self.IsProductionSelected = function () {
			return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Production;
		};
		
		self.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId).extend({
			required: {
				onlyIf: function () {
					return self.IsSavedSearchSelected();
				}
			}
		});

		self.SourceProductionId = ko.observable().extend({
			required: {
				onlyIf: function () {
					return self.IsProductionSelected();
				}
			}
		});
		self.SourceOptions = [
			{ value: 3, key: "Saved Search" },
			{ value: 2, key: "Production" }
		];
		self.TypeOfExport.subscribe(function (value) {
			if (value === ExportEnums.SourceOptionsEnum.Production) {

				var productionSetsPromise = IP.data.ajax({
					type: "get",
					url: IP.utils.generateWebAPIURL("Production/GetProductionsForExport"),
					data: {
						sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top)
					}
				}).fail(function (error) {
					IP.message.error.raise("No production sets were returned from the source provider.");
				});

				IP.data.deferred().all(productionSetsPromise).then(function (result) {

					self.SourceProductionSets(result);
					self.SourceProductionId(state.SourceProductionId);
				});
			}
		});
		self.TypeOfExport(state.TypeOfExport);


		self.ShowRelativityInstance = ko.observable(false);
		self.ShowAuthentiactionButton = ko.observable(false);
		self.AuthenticationFailed = ko.observable(false);

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

		if (self.federatedInstances.length === 0) {
			IP.data.ajax({
				type: 'GET',
				url: IP.utils.generateWebAPIURL('InstanceFinder'),
				async: true,
				success: function (result) {
					//TODO hack for now - remove this after enabling I2I in profiles
					if (parent.IP.data.params['apiControllerName'] == 'IntegrationPointProfilesAPI') {
						result = result.filter(function (value) { return value.artifactId == null; });
					}
					self.federatedInstances(result);

					if (state.FederatedInstanceArtifactId != undefined) {
						self.FederatedInstanceArtifactId(state.FederatedInstanceArtifactId);
					} else {
						self.FederatedInstanceArtifactId(self.federatedInstances()[0].artifactId);
					}
					self.updateWorkspaces();
					self.ShowAuthentiactionButton(self.FederatedInstanceArtifactId() != null);
					self.FederatedInstanceArtifactId.subscribe(function (value) {
						var isRemoteInstance = value != null;
						self.AuthenticationFailed(false);
						self.SecuredConfiguration(null);
						if (state.TargetWorkspaceArtifactId != null) {
							state.TargetWorkspaceArtifactId = null;
							state.FolderArtifactId = null;
						}
						if (isRemoteInstance) {
							self.openAuthenticateModal();
						} else {
							self.updateWorkspaces();
						}
						self.ShowAuthentiactionButton(isRemoteInstance);
					});
					self.ShowRelativityInstance(result.length >= 2);
				},
				error: function () {
					IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve Relativity instances. Please contact your system administrator.");
					self.federatedInstances([]);
				}
			});
		}

		self.getFolderAndSubFolders = function (destinationWorkspaceId, folderArtifactId) {
			var reloadTree = function (params, onSuccess, onFail) {
				IP.data.ajax({
					type: "POST",
					url: IP.utils.generateWebAPIURL("SearchFolder/GetStructure",
						destinationWorkspaceId,
						params.id != "#" ? params.id : "0",
						self.FederatedInstanceArtifactId() ? self.FederatedInstanceArtifactId() : 0),
					data: self.SecuredConfiguration()
				}).then(function (result) {
					onSuccess(result);
					if (!!folderArtifactId) {
						self.FolderArtifactId(folderArtifactId);
						self.TargetFolder(self.getFolderPath(destinationWorkspaceId, folderArtifactId));
						self.TargetFolder.isModified(false);
					}
					self.foldersStructure = result;
				}).fail(function (error) {
					onFail(error);
					IP.frameMessaging().dFrame.IP.message.error.raise(error);
				});
			}
			self.locationSelector.reloadWithRootWithData(reloadTree);
		};

		self.LocationFolderChecked.subscribe(function (value) {
			if (value === "true") {
				self.ProductionArtifactId(null);
				self.getFolderAndSubFolders(self.TargetWorkspaceArtifactId());
				self.locationSelector.toggle(self.TargetWorkspaceArtifactId.isValid());
			} else {
				self.TargetFolder("");
				self.TargetFolder.isModified(false);
				self.locationSelector.toggle(false);

				self.getDestinationProductionSets(self.TargetWorkspaceArtifactId());
				self.ProductionArtifactId.isModified(false);
			}
		});

		self.getFolderPath = function (destinationWorkspaceId, folderArtifactId) {
			IP.data.ajax({
				contentType: "application/json",
				dataType: "json",
				headers: { "X-CSRF-Header": "-" },
				type: "POST",
				url: IP.utils.generateWebAPIURL("SearchFolder/GetFullPathList",
					destinationWorkspaceId,
					folderArtifactId,
					self.FederatedInstanceArtifactId() ? self.FederatedInstanceArtifactId() : 0),
				async: true,
				data: self.SecuredConfiguration()
			})
				.then(function (result) {
					if (result[0]) {
						self.TargetFolder(result[0].fullPath);
					}
				});
		}

		self.onDOMLoaded = function () {
			self.locationSelector = new LocationJSTreeSelector();

			self.locationSelector.init(self.TargetFolder(), [], {
				onNodeSelectedEventHandler: function (node) {
					self.FolderArtifactId(node.id);
					self.getFolderPath(self.TargetWorkspaceArtifactId(), self.FolderArtifactId());
				}
			});

			if (self.FolderArtifactId() && self.TargetWorkspaceArtifactId()) {
				self.getFolderAndSubFolders(self.TargetWorkspaceArtifactId(), self.FolderArtifactId());
			}

			self.locationSelector.toggle(self.TargetWorkspaceArtifactId.isValid());
		};

		self.SavedSearchUrl = IP.utils.generateWebAPIURL('SavedSearchFinder', IP.utils.getParameterByName("AppID", window.top));

		self.updateWorkspaces = function () {
			IP.data.ajax({
				type: 'POST',
				url: IP.utils.generateWebAPIURL('WorkspaceFinder', self.FederatedInstanceArtifactId()),
				data: self.SecuredConfiguration(),
				async: true,
				success: function (result) {
					ko.utils.arrayForEach(result, function (item) {
						item.displayName = IP.utils.decode(item.displayName);
					});
					self.workspaces(result);
					self.TargetWorkspaceArtifactId(state.TargetWorkspaceArtifactId);
					self.getDestinationProductionSets(self.TargetWorkspaceArtifactId());
					self.TargetWorkspaceArtifactId.subscribe(function (value) {
						if (value) {
							self.EnableLocationRadio(true);
							self.TargetFolder("");
							self.TargetFolder.isModified(false);
						} else {
							self.EnableLocationRadio(false);
							state.ProductionArtifactId = null;
							self.ProductionArtifactId(null);
						}
						if (self.TargetWorkspaceArtifactId() !== value) {

							self.getDestinationProductionSets(self.TargetWorkspaceArtifactId());
							self.TargetWorkspaceArtifactId.isModified(false);
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

		self.RetrieveSavedSearchTree = function(nodeId, callback) {
			var selectedSavedSearchId = self.SavedSearchArtifactId();
			self.SavedSearchService.RetrieveSavedSearchTree(nodeId, selectedSavedSearchId, callback);
		};

		var savedSearchPickerViewModel = new SavedSearchPickerViewModel(function (value) {
			self.SavedSearchArtifactId(value.id);
		}, self.RetrieveSavedSearchTree);

		Picker.create("Fileshare", "savedSearchPicker", "SavedSearchPicker", savedSearchPickerViewModel);


		self.OpenSavedSearchPicker = function () {
			savedSearchPickerViewModel.open(self.SavedSearchArtifactId());
		};

		self.updateSecuredConfiguration = function (clientId, clientSecret) {
			self.SecuredConfiguration(IP.utils.generateCredentialsData(self.FederatedInstanceArtifactId(), clientId, clientSecret));
			self.updateWorkspaces();
		}

		self.getDestinationProductionSets = function (targetWorkspaceId) {
			if (targetWorkspaceId) {
				var productionSetsPromise = IP.data.ajax({
					type: "POST",
					url: IP.utils.generateWebAPIURL("Production/GetProductionsForImport", targetWorkspaceId, self.FederatedInstanceArtifactId()),
					data: self.SecuredConfiguration()
				}).fail(function (error) {
					IP.message.error.raise("No production sets were returned for target workspace.");
				});


				IP.data.deferred().all(productionSetsPromise).then(function (result) {
					self.DestinationProductionSets(result);
					self.ProductionArtifactId(state.ProductionArtifactId);
				});
			}
		};

		var authenticateModalViewModel = new AuthenticateViewModel(
			function (clientId, clientSecret) {
				self.updateSecuredConfiguration(clientId, clientSecret);
			},
			function () {
				self.AuthenticationFailed(true);
				self.workspaces([]);
				self.TargetWorkspaceArtifactId(null);
				self.FolderArtifactId(null);
				self.TargetFolder(null);
				self.TargetFolder.isModified(false);
				self.locationSelector.reload([]);
			}
		);

		Picker.create("Modals", "authenticate-modal", "AuthenticationModalView", authenticateModalViewModel);

		self.openAuthenticateModal = function () {
			self.AuthenticationFailed(false);
			authenticateModalViewModel.open(self.SecuredConfiguration());
		};

		var creatingProductionSetModalViewModel = new CreatingProductionSetViewModel(
			function (newProductionArtifactId) {
				self.getDestinationProductionSets(self.TargetWorkspaceArtifactId());
				state.ProductionArtifactId = newProductionArtifactId;
				self.ProductionArtifactId(newProductionArtifactId);
			}
		);

		Picker.create("Modals", "creating-production-set-modal", "CreatingProductionSetModalView", creatingProductionSetModalViewModel);

		var createProductionSetModalViewModel = new CreateProductionSetViewModel(
			function (newProductionSetName, positionLeft) {
				creatingProductionSetModalViewModel.open(newProductionSetName, self.TargetWorkspaceArtifactId(), self.SecuredConfiguration(), self.FederatedInstanceArtifactId(), positionLeft);
			}
		);

		Picker.create("Modals", "create-production-set-modal", "CreateProductionSetModalView", createProductionSetModalViewModel);

		self.openCreateProductionSetModal = function () {
			createProductionSetModalViewModel.open();
		}

		this.TargetFolder.extend({
			required: {
				onlyIf: function () {
					return self.LocationFolderChecked() === "true";
				}
			}
		});
		this.TargetWorkspaceArtifactId.extend({
			required: true
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

		self.TargetWorkspaceArtifactId.subscribe(function (value) {
			if (value) {
				self.getFolderAndSubFolders(value);
				self.EnableLocationRadio(true);
				self.LocationFolderChecked((state.ProductionArtifactId != undefined && state.ProductionArtifactId > 0) ? 'false' : 'true');
			} else {
				self.EnableLocationRadio(false);
				self.ProductionArtifactId(null);
				self.ProductionArtifactId.isModified(false);
			}
			if (!self.TargetWorkspaceArtifactId.isValid()) {
				self.TargetFolder("");
				self.TargetFolder.isModified(false);
			}
			self.locationSelector.toggle(self.TargetWorkspaceArtifactId.isValid());
		});

		this.SavedSearchArtifactId.extend({
			checkSavedSearch: {
				onlyIf: function () {
					return (self.IsSavedSearchSelected());
				}
			}
		});

		this.errors = ko.validation.group(this, { deep: true });
		this.getSelectedOption = function () {
			return {
				"FederatedInstanceArtifactId": self.FederatedInstanceArtifactId(),
				"SavedSearchArtifactId": self.SavedSearchArtifactId(),
				"TypeOfExport": self.TypeOfExport(),
				"ProductionImport": self.ProductionImport(),
				"ProductionArtifactId": self.ProductionArtifactId(),
				"SourceProductionId": self.SourceProductionId(),
				"SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
				"TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
				"FolderArtifactId": self.FolderArtifactId()
			}
		}

		/********** Tooltips  **********/
		var destinationTooltipViewModel = new TooltipViewModel(TooltipDefs.RelativityProviderDestinationDetails, TooltipDefs.RelativityProviderDestinationDetailsTitle);

		Picker.create("Tooltip", "tooltipDestinationId", "TooltipView", destinationTooltipViewModel);

		this.openRelativityProviderDetailsTooltip = function (data, event) {
			destinationTooltipViewModel.open(event);
		};

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
