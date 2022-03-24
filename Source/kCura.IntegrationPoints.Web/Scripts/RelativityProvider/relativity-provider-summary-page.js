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

		function formatExportType(importNatives, importImages, typeOfExport) {
			var exportType = "Workspace; ";
			var images = convertToBool(importImages) ? "Images;" : "";
			var natives = convertToBool(importNatives) ? "Natives;" : "";
			var view = typeOfExport === 4 ? "View;" : "";
			return exportType + images + natives + view;
		};

		function getProductionPrecedenceTextRepresentation() {
			
			if (dataContainer.destinationConfiguration.ProductionPrecedence === 0) {
				// Image precedence is set to Original, so we don't want to show Production Precedence list to the user
				return "";
			}
			
			var productionPrecedence = dataContainer.destinationConfiguration.ImagePrecedence;
			
			if (!productionPrecedence || productionPrecedence.length === 0) {
				return "";
			}

			return ": " + productionPrecedence.map(function (x) {
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
		this.isNonDocumentSyncFlow = dataContainer.sourceConfiguration.SourceViewId !== undefined;
		this.sourceWorkspace = dataContainer.sourceConfiguration.SourceWorkspace;
		this.targetWorkspace = dataContainer.sourceConfiguration.TargetWorkspace;
		if (dataContainer.sourceConfiguration.SourceProductionId) {
			this.sourceDetails = "Production Set: " + dataContainer.sourceConfiguration.SourceProductionName;
		} else if (dataContainer.sourceConfiguration.SourceViewId) {
			this.sourceDetails = "View: " + dataContainer.sourceConfiguration.ViewName;
		} else {
			this.sourceDetails = "Saved Search: " + dataContainer.sourceConfiguration.SavedSearch;
		}
		this.sourceRelativityInstance = dataContainer.sourceConfiguration.SourceRelativityInstance;
		this.multiSelectOverlay = dataContainer.destinationConfiguration.FieldOverlayBehavior;
		this.useFolderPathInfo = ko.observable();
		formatFolderPathInformation(dataContainer.destinationConfiguration.UseFolderPathInformation, dataContainer.destinationConfiguration.UseDynamicFolderPath);
		this.moveExistingDocs = formatToYesOrNo(dataContainer.destinationConfiguration.MoveExistingDocuments);
		this.exportType = formatExportType(dataContainer.destinationConfiguration.importNativeFile, dataContainer.destinationConfiguration.ImageImport, dataContainer.sourceConfiguration.TypeOfExport);
		this.showInstanceInfo = dataContainer.destinationConfiguration.FederatedInstanceArtifactId !== null;

		this.importNativeFile = ko.observable(dataContainer.destinationConfiguration.importNativeFile == 'true');
		this.importImageFile = ko.observable(dataContainer.destinationConfiguration.ImageImport == 'true' &&
			(!dataContainer.destinationConfiguration.ImagePrecedence || dataContainer.destinationConfiguration.ImagePrecedence.length == 0));
		this.copyImages = ko.observable(dataContainer.destinationConfiguration.ImageImport == 'true');
		this.imagePrecedence = ko.observable(getProductionPrecedenceTextRepresentation());
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