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

		function formatFolderPathInformation(useFolderPathInfo, useDynamicFolderPath) {
			if (convertToBool(useFolderPathInfo)) {
				IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('FolderPath', 'GetFields') }).then(function (result) {
					var folderPathSourceField = dataContainer.destinationConfiguration.FolderPathSourceField;
					var fields = ko.utils.arrayFilter(result, function (field) { return field.fieldIdentifier === folderPathSourceField; });
					if (fields.length > 0) {
						self.useFolderPathInfo("Read From Field:" + fields[0].actualName);
					}
				});
			} else if (convertToBool(useDynamicFolderPath)) {
				self.useFolderPathInfo("Read From Folder Tree");
			} else {
				self.useFolderPathInfo("No");
			}
		};

		function formatExportType(importNatives, importImages) {
			var exportType = "Workspace;";
			var images = convertToBool(importImages) ? "Images;" : "";
			var natives = convertToBool(importNatives) ? "Natives;" : "";
			return exportType + images + natives;
		};

		function getTextRepresentation(value) {
			if (!value || value.length === 0) {
				return "";
			}

			return ": " + value.map(function (x) {
				return x.displayName;
			}).join("; ");
		};

		this.hasErrors = dataContainer.hasErrors;
		this.logErrors = dataContainer.logErrors;
		this.emailNotification = dataContainer.emailNotification;
		this.name = dataContainer.name;
		this.overwriteMode = dataContainer.overwriteMode;
		this.sourceProviderName = dataContainer.sourceProviderName;
		this.destinationRdoName = dataContainer.destinationRdoName;
		this.targetFolder = dataContainer.sourceConfiguration.TargetFolder;
		this.showTargetFolder = dataContainer.sourceConfiguration.TargetFolder !== undefined;
		this.targetProductionSet = dataContainer.sourceConfiguration.targetProductionSet;
		this.showTargetProductionSet = dataContainer.sourceConfiguration.targetProductionSet !== undefined;
		this.sourceWorkspace = dataContainer.sourceConfiguration.SourceWorkspace;
		this.targetWorkspace = dataContainer.sourceConfiguration.TargetWorkspace;
		if (dataContainer.sourceConfiguration.SourceProductionId >= 0) {
			this.sourceDetails = "Production Set: " + dataContainer.sourceConfiguration.SourceProductionName;
		} else {
			this.sourceDetails = "Saved Search: " + dataContainer.sourceConfiguration.SavedSearch;
		}
		this.sourceRelativityInstance = dataContainer.sourceConfiguration.SourceRelativityInstance;
		this.destinationRelativityInstance = dataContainer.destinationConfiguration.DestinationRelativityInstance;
		this.multiSelectOverlay = dataContainer.destinationConfiguration.FieldOverlayBehavior;
		this.useFolderPathInfo = ko.observable();
		formatFolderPathInformation(dataContainer.destinationConfiguration.UseFolderPathInformation, dataContainer.destinationConfiguration.UseDynamicFolderPath);
		this.moveExistingDocs = formatToYesOrNo(dataContainer.destinationConfiguration.MoveExistingDocuments);
		this.exportType = formatExportType(dataContainer.destinationConfiguration.importNativeFile, dataContainer.destinationConfiguration.ImageImport);
		this.showInstanceInfo = dataContainer.destinationConfiguration.FederatedInstanceArtifactId !== null;
		this.promoteEligible = dataContainer.promoteEligible;

		this.importNativeFile = ko.observable(dataContainer.destinationConfiguration.importNativeFile == 'true');
		this.importImageFile = ko.observable(dataContainer.destinationConfiguration.ImageImport == 'true' && (!dataContainer.destinationConfiguration.ImagePrecedence || dataContainer.destinationConfiguration.ImagePrecedence.length == 0));
		this.copyImages = ko.observable(dataContainer.destinationConfiguration.ImageImport == 'true');
		this.imagePrecedence = ko.observable(getTextRepresentation(dataContainer.destinationConfiguration.ImagePrecedence));
		this.productionPrecedence = ko.observable(dataContainer.destinationConfiguration.ProductionPrecedence === 0 ? "Original" : "Produced");
		this.precedenceSummary = ko.computed(function () {
			return self.productionPrecedence() +  self.imagePrecedence();
		}, self);
		this.copyFilesToRepository = formatToYesOrNo(dataContainer.destinationConfiguration.importNativeFile);
		this.createSavedSearch = formatToYesOrNo(dataContainer.destinationConfiguration.CreateSavedSearchForTagging);
		this.stats = new SavedSearchStatistics(dataContainer.sourceConfiguration, dataContainer.destinationConfiguration);
		
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};