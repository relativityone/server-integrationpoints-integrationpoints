var IP = IP || {};

var loadData = function (ko, dataContainer) {

	var Model = function (dataContainer) {
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
		this.savedSearch = dataContainer.sourceConfiguration.SavedSearch;

	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};