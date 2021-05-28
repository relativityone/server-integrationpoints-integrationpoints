var SavedSearchService = function() {
	var self = this;

	var formatChildrenDirectory = function (parent) {
		$.each(parent.children, function (index, node) {
			if (node.isDirectory) {
				if (node.children === undefined || node.children.length < 1) {
					$.extend(node, { children: true });
				} else {
					formatChildrenDirectory(node);
				}
			}
		});
	};

	self.RetrieveSavedSearchTree = function (nodeId, selectedSavedSearchId, callback) {
		var savedSearchId = null;
		if (nodeId === null) {
			savedSearchId = selectedSavedSearchId;
		}

		IP.data.ajax({
			type: 'GET',
			url: IP.utils.generateWebAPIURL('SavedSearchesTree', IP.utils.getParameterByName("AppID", window.top)),
			dataType: 'json',
			data: {
				savedSearchContainerId: nodeId,
				savedSearchId: savedSearchId
			},
			async: true,
			success: function (result) {
				formatChildrenDirectory(result);
				callback(result);
			},
			error: function () {
				IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve the saved searches. Please contact your system administrator.");
			}
		});
	};

	self.RetrieveSavedSearch = function(savedSearchId, okCallback, errorCallback) {
		IP.data.ajax({
			type: 'GET',
			url: IP.utils.generateWebAPIURL('SavedSearchFinder'),
			async: true,
			data: {
				savedSearchId: savedSearchId
			},
			success: okCallback,
			error: function (err) {
				if (err.status === 404) {
					okCallback(null);
				} else {
					IP.frameMessaging().dFrame.IP.message.error.raise("Unable to retrieve the saved searche. Please contact your system administrator.");
					errorCallback(err);
				}
			}
		});
	}
}