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
		var confirmationMessage = "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.";

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
		var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(function (value) {
			IP.data.ajax({
				url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/SaveAsProfile', integrationPointId, value),
				type: 'POST',
				success: function () {
					IP.message.notify("Profile has been saved", $("#customRDOWithConsoleWrapper"));
				},
				fail: function (error) {
					IP.message.error.raise(error, $("#customRDOWithConsoleWrapper"));
				}
			});
		});
		var promise = Picker.create("IntegrationPoints", "saveAsProfileModal", "SaveAsProfileModal", saveAsProfileModalViewModel);
		promise.done(function () {
			saveAsProfileModalViewModel.open(ipName);
		});
	};

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

$(window).load(function () {
	$(".consoleContainer .consoleButtonDisabled").attr("title", "You do not have permission to run this job.");
});

$(window).unload(function () {

	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
});