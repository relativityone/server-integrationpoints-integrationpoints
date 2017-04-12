var IP = IP || {};

var SavedSearchStatistics = function (workspaceId, savedSearchId, importNatives, importImages) {
	var self = this;

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
			} else if (size == -1) {
				result += " (Error occured)";
			} else {
				result += " (Calculating size...)";
			}
		} else if (total == -1) {
			result = "Error occured";
		} else {
			result = "Calculating...";
		}
		updateDelegate(result);
	};

	function getDocsTotal(workspaceId, savedSearchId) {
		$.ajax(jQuery.extend(self.defaultSettings(), {
			data: JSON.stringify({ WorkspaceArtifactId: workspaceId, savedSearchId: savedSearchId }),
			url: ("/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Statistics%20Manager/GetDocumentsTotalForSavedSearchAsync"),
			success: self.documents,
			error: function (err) {
				console.log(err);
				self.documents("Error occured");
			}
		}));
	};
	function getNativesTotal(workspaceId, savedSearchId) {
		$.ajax(jQuery.extend(self.defaultSettings(), {
			data: JSON.stringify({ WorkspaceArtifactId: workspaceId, savedSearchId: savedSearchId }),
			url: ("/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Statistics%20Manager/GetNativesTotalForSavedSearchAsync"),
			success: self.nativesTotal,
			error: function (err) {
				console.log(err);
				self.nativesTotal(-1);
			}
		}));
	};
	function getNativesSize(workspaceId, savedSearchId) {
		$.ajax(jQuery.extend(self.defaultSettings(), {
			data: JSON.stringify({ WorkspaceArtifactId: workspaceId, savedSearchId: savedSearchId }),
			url: ("/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Statistics%20Manager/GetNativesFileSizeForSavedSearchAsync"),
			success: self.nativesSize,
			error: function (err) {
				console.log(err);
				self.nativesSize(-1);
			}
		}));
	};
	function getImagesTotal(workspaceId, savedSearchId) {
		$.ajax(jQuery.extend(self.defaultSettings(), {
			data: JSON.stringify({ WorkspaceArtifactId: workspaceId, savedSearchId: savedSearchId }),
			url: ("/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Statistics%20Manager/GetImagesTotalForSavedSearchAsync"),
			success: self.imagesTotal,
			error: function (err) {
				console.log(err);
				self.imagesTotal(-1);
			}
		}));
	};
	function getImagesSize(workspaceId, savedSearchId) {
		$.ajax(jQuery.extend(self.defaultSettings(), {
			data: JSON.stringify({ WorkspaceArtifactId: workspaceId, savedSearchId: savedSearchId }),
			url: ("/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Statistics%20Manager/GetImagesFileSizeForSavedSearchAsync"),
			success: self.imagesSize,
			error: function (err) {
				console.log(err);
				self.imagesSize(-1);
			}
		}));
	};

	function formatBytes(bytes) {
		if (bytes == 0) return '0 Bytes';
		var k = 1024;
		var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
		var i = Math.floor(Math.log(bytes) / Math.log(k));
		return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
	}

	getDocsTotal(workspaceId, savedSearchId);
	if (importNatives && !importImages) {
		getNativesTotal(workspaceId, savedSearchId);
		getNativesSize(workspaceId, savedSearchId);
	}
	if (importImages) {
		getImagesTotal(workspaceId, savedSearchId);
		if (importNatives) {
			getImagesSize(workspaceId, savedSearchId);
		} else {
			self.imagesSize(0);
		}
	}
};