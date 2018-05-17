var IP = IP || {};
(function (root) {
	root.importNow = function (artifactId, appid) {
		var overlayOnlyMessage = "Only documents and their metadata with the same identifier will be overwritten.";
		var appendOverlayMesssage = "All existing documents and their metadata in the target workspace that have the same identifier will be overwritten.";
		var appendMessage = "Documents will be placed in the workspace parent folder.";
		var appendWithFolderPathMessage = "You may be creating folders in the destination workspace using the ";
		var expiredErrorsMessage = " Any existing errors will be marked as expired.";
		var proceedWarningMessage = " Would you still like to proceed?";
		var overwriteOption = $("[fafriendlyname=\"Overwrite Fields\"]").closest("tr").find(".dynamicViewFieldValue").text();

		var selectedMessage = "";
		if (overwriteOption === "Append Only") {
			if (!!IP.fieldName && IP.fieldName.length !== 0) {
				selectedMessage = appendWithFolderPathMessage + IP.fieldName + " field.";
			} else {
				selectedMessage = appendMessage;
			}
		} else if (overwriteOption === "Overlay Only") {
			selectedMessage = overlayOnlyMessage;
		} else if (overwriteOption === "Append/Overlay") {
			selectedMessage = appendOverlayMesssage;
		}
		if (IP.isFileshareProvider) {
			selectedMessage = "Documents will be exported to your selected destination. If you chose to schedule your job, export will begin at the selected time.";
		}
		var consoleContainer = $(".ConsoleControl");
		var hasErrors = $(consoleContainer.find(":contains('Retry Errors')")).hasClass("consoleButtonEnabled");
		if (hasErrors) {
			selectedMessage += expiredErrorsMessage;
		}
		selectedMessage += proceedWarningMessage;

		if (!!root.errorMessage && root.errorMessage.length !== 0) {
			IP.message.error.raise(root.errorMessage, $(".cardContainer"));
			return;
		}
		window.Dragon.dialogs.showConfirm({
			message: selectedMessage,
			title: 'Run',
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close(); var ajax = IP.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('Job'),
					data: JSON.stringify({
						"appId": appid,
						"artifactId": artifactId
					})
				});
				ajax.fail(function (value) {
					IP.message.error.raise("Failed to submit integration job. " + value.responseText, $(".cardContainer"));
				});
				ajax.done(function () {
					IP.message.info.raise("Job started", $(".cardContainer"));
				});
			}
		});
	};

	root.stopJob = function (artifactId, appId) {
		var confirmationMessage = "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.";

		window.Dragon.dialogs.showConfirm({
			message: confirmationMessage,
			title: "Stop Transfer",
			okText: 'Stop Transfer',
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: "POST",
					url: root.utils.generateWebAPIURL('Job/Stop'),
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
						okText: 'Ok',
						showCancel: false,
						width: 450,
					});
				});
			}
		});
	};

	root.retryJob = function (artifactId, appId) {
		var overwriteOption = $("[fafriendlyname=\"Overwrite Fields\"]").closest("tr").find(".dynamicViewFieldValue").text();
		var selectedMessage = "";
		if (overwriteOption === "Overlay Only") {
			selectedMessage = "The retry job will run in Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
		} else {
			selectedMessage = "The retry job will run in Append/Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
		}

		if (!!root.errorMessage && root.errorMessage.length !== 0) {
			IP.message.error.raise(root.errorMessage, $(".cardContainer"));
			return;
		}
		window.Dragon.dialogs.showConfirm({
			message: selectedMessage,
			title: "Retry Errors",
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: "POST",
					url: root.utils.generateWebAPIURL('Job/Retry'),
					async: true,
					data: JSON.stringify({
						"appId": appId,
						"artifactId": artifactId
					})
				});
				ajax.fail(function (value) {
					window.Dragon.dialogs.showConfirm({
						message: "Failed to submit the retry job. " + value.responseText,
						title: "Unable to Retry Errors",
						okText: 'Ok',
						showCancel: false,
						width: 450
					});
				});
				ajax.done(function () {
					IP.message.info.raise("Retry job started", $(".cardContainer"));
				});
			}
		});
	};

	root.saveAsProfile = function (integrationPointId, workspaceId, ipName) {
		var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(function (value) {
			IP.data.ajax({
				url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/SaveAsProfile', integrationPointId, value),
				type: 'POST',
				success: function() {
					IP.message.notify("Profile has been saved", $("#customRDOWithConsoleWrapper"));
				},
				fail:function (error) {
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

$(window).unload(function () {
	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
			obj.push({ key: 'Scheduled Time', value: IP.timeUtil.format24HourToMilitaryTime(result.scheduledTime, "h:mm A") + (timeZone ? "; " + timeZone.DisplayName : "") });
});