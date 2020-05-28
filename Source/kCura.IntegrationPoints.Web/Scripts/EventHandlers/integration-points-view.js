var IP = IP || {};
(function (root) {
	root.importNow = function (artifactId, appid) {

		window.Dragon.dialogs.showConfirm({
			message: "Are you sure you want to run this job now?",
			title: "Run",
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: "post",
					url: root.utils.generateWebAPIURL("Job"),
					data: JSON.stringify({
						"appId": appid,
						"artifactId": artifactId
					})
				});
				ajax.fail(function (value) {
					IP.message.error.raise("Failed to submit integration job. " + value.responseText, $(".cardContainer"));
				});
				ajax.done(function () {
					IP.message.info.raise("Job started.", $(".cardContainer"));
				});
			}
		});
	};

	root.stopJob = function (artifactId, appId) {
		var confirmationMessage = "Stopping this transfer will not remove any data that was transferred. " +
		    "When re-running this transfer, make sure that your overwrite settings will return expected results.";

		window.Dragon.dialogs.showConfirm({
			message: confirmationMessage,
			title: "Stop Transfer",
			okText: "Stop Transfer",
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: "POST",
					url: root.utils.generateWebAPIURL("Job/Stop"),
					async: true,
					data: JSON.stringify({
						"appId": appId,
						"artifactId": artifactId
					})
				});
				ajax.fail(function (value) {
					window.Dragon.dialogs.showConfirm({
						message: "Failed to stop the job. " + value.responseText,
						title: "Unable to Stop the Transfer",
						okText: "Ok",
						showCancel: false,
						width: 450
					});
				});
			}
		});
	};

	root.saveAsProfile = function (integrationPointId, workspaceId, ipName) {
		const SAVE_AS_PROFILE_ERR_PREFIX = "Failed to save integration point as profile.";

		var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(function (value) {
			var ajax = IP.data.ajax({
                url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/SaveAsProfile', integrationPointId, encodeURIComponent(value)),
				type: 'POST',
				success: function () {
					IP.message.info.raise("Profile has been saved", $("#customRDOWithConsoleWrapper"));
				},
			});
			ajax.fail(function (error) {
				try {
					const validationResultDto = JSON.parse(error.responseText);
					IP.message.errorFormatted.raise(validationResultDto.errors, $(".cardContainer"), SAVE_AS_PROFILE_ERR_PREFIX);
				} catch (e) {
					IP.message.error.raise(SAVE_AS_PROFILE_ERR_PREFIX + " " + error.responseText, $(".cardContainer"));
				}
			});
		});
		var promise = Picker.create("IntegrationPoints", "saveAsProfileModal", "SaveAsProfileModal", saveAsProfileModalViewModel);
		promise.done(function () {
			saveAsProfileModalViewModel.open(ipName);
		});
	};

	root.downloadErrorFile = function (artifactId, appId) {
		var checkUrlBase = IP.utils.generateWebAPIURL('ImportProviderDocument/CheckErrorFile');
		var checkUrl = checkUrlBase + '?artifactId=' + artifactId + '&workspaceId=' + appId;
		IP.data.ajax({
			url: checkUrl,
			type: 'GET',
			success: function () {
				var urlBase = IP.utils.generateWebAPIURL('ImportProviderDocument/DownloadErrorFile');
				window.location = urlBase + '?artifactId=' + artifactId + '&workspaceId=' + appId;
			},
			error: function (error) {
				IP.message.error.raise('The error file could not be found for download.', $("#customRDOWithConsoleWrapper"));
			}
		});
	}

	IP.redirect.set(document.URL);

	$(function () {
		var $editButtons = $(":contains('Edit')").closest('a');
		for (var i = 0; i < $editButtons.length; i++) {
			if ($editButtons[i].text === "Edit") {
				$($editButtons[i]).on("click", function () {
					IP.isEdit = "Edit";
				});
			}
		}
	});

})(IP);

$(function() {
	$(".consoleContainer .consoleButtonDisabled").attr("title", "You do not have permission to run this job.");
});

$(window).on('unload', (function () {

	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
}));