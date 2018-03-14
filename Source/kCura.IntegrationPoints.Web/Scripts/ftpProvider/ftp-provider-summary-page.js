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
		this.host = dataContainer.sourceConfiguration.Host;
		this.port = dataContainer.sourceConfiguration.Port;
		this.protocol = dataContainer.sourceConfiguration.Protocol;
		this.fileNamePrefix = dataContainer.sourceConfiguration.FileNamePrefix;
		this.timezoneOffset = dataContainer.sourceConfiguration.TimezoneOffset;
	};

	var viewModel = new Model(dataContainer);
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};