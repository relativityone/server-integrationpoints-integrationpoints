var IP = IP || {};

var loadData = function (ko, dataContainer) {

	var Model = function (dataContainer) {
		this.hasErrors = dataContainer.hasErrors;
		this.logErrors = dataContainer.logErrors;
		this.emailNotification = dataContainer.emailNotification;
		this.name = dataContainer.name;
		this.overwriteMode = dataContainer.overwriteMode;
		
		this.sourceProviderName = dataContainer.sourceProviderName;
		this.connectionPath = dataContainer.sourceConfiguration.ConnectionPath;
		this.objectFilterString = dataContainer.sourceConfiguration.Filter;
		this.authenticationMode = dataContainer.sourceConfiguration.ConnectionAuthenticationType;
		this.importNestedItems = dataContainer.sourceConfiguration.ImportNested;
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};