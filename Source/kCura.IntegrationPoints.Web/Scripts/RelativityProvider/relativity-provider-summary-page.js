var IP = IP || {};

var loadData = function (ko, dataContainer) {

	

	var Model = function (dataContainer) {
		var self = this;

		function convertToBool(value) {
			return value === "true";
		}

		function formatToYesOrNo(value) {
			return convertToBool(value) ? "Yes" : "No";
		};

		function formatFolderPathInformation(useFolderPathInfo) {
			var value = formatToYesOrNo(useFolderPathInfo);
			if (convertToBool(useFolderPathInfo)) {
				IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('FolderPath', 'GetFields') }).then(function(result) {
					var folderPathSourceField = dataContainer.destinationConfiguration.FolderPathSourceField;
					var fields = ko.utils.arrayFilter(result, function (field) { return field.fieldIdentifier === folderPathSourceField; });
					if (fields.length > 0) {
						self.useFolderPathInfo(value +";"+fields[0].actualName);
					}
				});
			} else {
				self.useFolderPathInfo(value);
			}
		};

		function formatExportType(importNatives, importImages) {
			var exportType = "Workspace;";
			var images = convertToBool(importImages) ? "Images;" : "";
			var natives = convertToBool(importNatives) ? "Natives;" : "";
			return exportType + images + natives;
		};

		this.hasErrors = dataContainer.hasErrors;
		this.logErrors = dataContainer.logErrors;
		this.emailNotification = dataContainer.emailNotification;
		this.name = dataContainer.name;
		this.overwriteMode = dataContainer.overwriteMode;
		this.sourceProviderName = dataContainer.sourceProviderName;
		this.destinationRdoName = dataContainer.destinationRdoName;
		this.targetFolder = dataContainer.sourceConfiguration.TargetFolder;
		this.sourceWorkspace = dataContainer.sourceConfiguration.SourceWorkspace;
		this.targetWorkspace = dataContainer.sourceConfiguration.TargetWorkspace;
		this.savedSearch = "Saved Search: " + dataContainer.sourceConfiguration.SavedSearch;
		this.sourceRelativityInstance = dataContainer.sourceConfiguration.SourceRelativityInstance;
		this.destinationRelativityInstance = dataContainer.destinationConfiguration.DestinationRelativityInstance;
		this.multiSelectOverlay = dataContainer.destinationConfiguration.FieldOverlayBehavior;
		this.useFolderPathInfo = ko.observable();
		formatFolderPathInformation(dataContainer.destinationConfiguration.UseFolderPathInformation);
		this.moveExistingDocs = formatToYesOrNo(dataContainer.destinationConfiguration.MoveExistingDocuments);
		this.exportType = formatExportType(dataContainer.destinationConfiguration.importNativeFile, dataContainer.destinationConfiguration.ImageImport);
		this.showInstanceInfo = dataContainer.destinationConfiguration.FederatedInstanceArtifactId !== null;
		this.promoteEligible = dataContainer.promoteEligible;
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};