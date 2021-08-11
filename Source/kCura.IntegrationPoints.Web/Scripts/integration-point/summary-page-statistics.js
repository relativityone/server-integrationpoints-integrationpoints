var IP = IP || {};

var SavedSearchStatistics = function (sourceConfiguration, destinationConfiguration) {
	var self = this;

	this.sourceConfiguration = sourceConfiguration;
	this.destinationConfiguration = destinationConfiguration;

	this.importNatives = destinationConfiguration.importNativeFile === 'true';
	this.importImages = destinationConfiguration.ImageImport === 'true' && (!destinationConfiguration.ImagePrecedence || destinationConfiguration.ImagePrecedence.length === 0);
	this.workspaceId = sourceConfiguration.SourceWorkspaceArtifactId;
	this.savedSearchId = sourceConfiguration.SavedSearchArtifactId;
	this.sourceProductionId = sourceConfiguration.SourceProductionId;

	this.defaultSettings = function () {
		return {
			cache: false,
			contentType: "application/json",
			dataType: "json",
			headers: { "X-CSRF-Header": "-" },
			type: "POST"
		};
	};

	this.documents = ko.observable("Calculating...");
	this.natives = ko.observable("Calculating...");
	this.images = ko.observable("Calculating...");

	this.nativesTotal = ko.observable();
	this.nativesSize = ko.observable();

	this.imagesTotal = ko.observable();
	this.imagesSize = ko.observable();

	this.nativesTotal.subscribe(function () {
		self.updateUI(self.nativesTotal(), self.nativesSize(), self.natives);
	});
	this.nativesSize.subscribe(function () {
		self.updateUI(self.nativesTotal(), self.nativesSize(), self.natives);
	});

	this.imagesTotal.subscribe(function () {
		self.updateUI(self.imagesTotal(), self.imagesSize(), self.images);
	});
	this.imagesSize.subscribe(function () {
		self.updateUI(self.imagesTotal(), self.imagesSize(), self.images);
	});

	this.updateUI = function (total, size, updateDelegate) {
		var result = "";
		if (total > -1) {
			result += total;

			if (size > -1) {
				result += " (" + formatBytes(size) + ")";
			} else if (size === -1) {
				result += " (Error occured)";
			} else {
				result += " (Calculating size...)";
			}
		} else if (total === -1) {
			result = "Error occured";
		} else {
			result = "Calculating...";
		}
		updateDelegate(result);
	};

	function getNativesStatistics(workspaceId, savedSearchId) {
		IP.data.ajax(jQuery.extend(self.defaultSettings(),
			{
				type: 'POST',
				url: IP.utils.generateWebURL('SummaryPage/GetNativesStatisticsForSavedSearch'),
				data: JSON.stringify({
					workspaceId: workspaceId,
					savedSearchId: savedSearchId,
					calculateSize: self.destinationConfiguration.importNativeFileCopyMode === 'CopyFiles'
				}),
				success: function (data) {
					self.documents(data.DocumentsCount);
					self.nativesTotal(data.TotalNativesCount);
					self.nativesSize(data.TotalNativesSizeBytes);
				},
				error: function (err) {
					console.error(err);
					self.documents('Error occured');
					self.nativesTotal(-1);
					self.nativesSize(-1);
				}
			}),
			false);
	};

	function getImagesStatisticsForSavedSearch(workspaceId, savedSearchId) {
		console.log('getImagesStatisticsForSavedSearch');
		IP.data.ajax(jQuery.extend(self.defaultSettings(),
			{
				type: 'POST',
				url: IP.utils.generateWebURL('SummaryPage/GetImagesStatisticsForSavedSearch'),
				data: JSON.stringify({
					workspaceId: workspaceId,
					savedSearchId: savedSearchId,
					calculateSize: self.importNatives
				}),
				success: function (data) {
					self.documents(data.DocumentsCount);
					self.imagesTotal(data.TotalImagesCount);
					self.imagesSize(data.TotalImagesSizeBytes);
				},
				error: function (err) {
					console.error(err);
					self.documents('Error occured');
					self.imagesTotal(-1);
					self.imagesSize(-1);
				}
			}),
			false);
	}

	function getImagesStatisticsForProduction(workspaceId, productionId) {
		console.log('getImagesStatisticsForSavedSearch');
		IP.data.ajax(jQuery.extend(self.defaultSettings(),
			{
				type: 'POST',
				url: IP.utils.generateWebURL('SummaryPage/GetImagesStatisticsForProduction'),
				data: JSON.stringify({
					workspaceId: workspaceId,
					productionId: productionId,
					calculateSize: self.importNatives
				}),
				success: function (data) {
					self.documents(data.DocumentsCount);
					self.imagesTotal(data.TotalImagesCount);
					self.imagesSize(data.TotalImagesSizeBytes);
				},
				error: function (err) {
					console.error(err);
					self.documents('Error occured');
					self.imagesTotal(-1);
					self.imagesSize(-1);
				}
			}),
			false);
	}

	function formatBytes(bytes) {
		if (bytes === 0) return '0 Bytes';
		var k = 1024;
		var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
		var i = Math.floor(Math.log(bytes) / Math.log(k));
		return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
	}

	if (self.sourceProductionId) {
		getImagesStatisticsForProduction(self.workspaceId, self.sourceProductionId);
	} else if (self.importNatives && !self.importImages) {
		getNativesStatistics(self.workspaceId, self.savedSearchId);
	} else {
		getImagesStatisticsForSavedSearch(self.workspaceId, self.savedSearchId);
	}
};