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

		const RUN_ERR_PREFIX = "Failed to submit integration job.";
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
					try {
						const validationResultDto = JSON.parse(value.responseText);
						IP.message.errorFormatted.raise(validationResultDto.errors, $(".cardContainer"), RUN_ERR_PREFIX);
					} catch (e) {
						IP.message.error.raise(RUN_ERR_PREFIX + " " + value.responseText, $(".cardContainer"));
					}
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
					const prefix = "Failed to stop the job.";
					const errTitle = "Unable to Stop the Transfer";
					IP.message.errorDialog.show(errTitle, prefix, value.responseText);
				});
			}
		});
	};

	root.retryJob = function (artifactId, appId, switchToAppendOverlayMode) {
		var overwriteOption = $("[fafriendlyname=\"Overwrite Fields\"]").closest("tr").find(".dynamicViewFieldValue").text();
		var selectedMessage = "";
		switchToAppendOverlayMode = false;

		console.log(overwriteOption);

		if (overwriteOption === "Append Only") 
		{
			selectedMessage = "Test msg - choose Append/Overlay or leave Append mode"; 
			console.log(selectedMessage);
			window.Dragon.dialogs.showYesNoCancel({
				message: selectedMessage,
				title: "Retry Errors",
				showCancel: true,
				yesText: "Append/Overlay",
				noText: overwriteOption,
				cancelText: "Cancel",
				width: 450,
				yesHandle: function (calls) {
					switchToAppendOverlayMode = true;
					calls.close();					
					var ajax = IP.data.ajax({
						type: "POST",
						url: root.utils.generateWebAPIURL('Job/Retry'),
						async: true,
						data: JSON.stringify({
							"appId": appId,
							"artifactId": artifactId,
							"switchToAppendOverlayMode": switchToAppendOverlayMode
						})
					});
					ajax.fail(function (value) {
						const prefix = "Failed to submit the retry job.";
						const errTitle = "Unable to Retry Errors";
						IP.message.errorDialog.show(errTitle, prefix, value.responseText);
					});
					ajax.done(function () {
						IP.message.info.raise("Retry job started", $(".cardContainer"));
					});
				},
				noHandle: function (calls) {
					calls.close();					
					var ajax = IP.data.ajax({
						type: "POST",
						url: root.utils.generateWebAPIURL('Job/Retry'),
						async: true,
						data: JSON.stringify({
							"appId": appId,
							"artifactId": artifactId,
							"switchToAppendOverlayMode": switchToAppendOverlayMode
						})
					});
					ajax.fail(function (value) {
						const prefix = "Failed to submit the retry job.";
						const errTitle = "Unable to Retry Errors";
						IP.message.errorDialog.show(errTitle, prefix, value.responseText);
					});
					ajax.done(function () {
						IP.message.info.raise("Retry job started", $(".cardContainer"));
					});
				}
			});
		}
		else 
		{
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
						const prefix = "Failed to submit the retry job.";
						const errTitle = "Unable to Retry Errors";
						IP.message.errorDialog.show(errTitle, prefix, value.responseText);
					});
					ajax.done(function () {
						IP.message.info.raise("Retry job started", $(".cardContainer"));
					});
				}
			});
        }


		
	};

	//root.retryJob = function (artifactId, appId) {
	//	var overwriteOption = $("[fafriendlyname=\"Overwrite Fields\"]").closest("tr").find(".dynamicViewFieldValue").text();
	//	var selectedMessage = "";
	//	if (overwriteOption === "Overlay Only") {
	//		selectedMessage = "The retry job will run in Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
	//	} else {
	//		selectedMessage = "The retry job will run in Append/Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
	//	}

	//	if (!!root.errorMessage && root.errorMessage.length !== 0) {
	//		IP.message.error.raise(root.errorMessage, $(".cardContainer"));
	//		return;
	//	}
	//	window.Dragon.dialogs.showConfirm({
	//		message: selectedMessage,
	//		title: "Retry Errors",
	//		showCancel: true,
	//		width: 450,
	//		success: function (calls) {
	//			calls.close();
	//			var ajax = IP.data.ajax({
	//				type: "POST",
	//				url: root.utils.generateWebAPIURL('Job/Retry'),
	//				async: true,
	//				data: JSON.stringify({
	//					"appId": appId,
	//					"artifactId": artifactId
	//				})
	//			});
	//			ajax.fail(function (value) {
	//				const prefix = "Failed to submit the retry job.";
	//				const errTitle = "Unable to Retry Errors";
	//				IP.message.errorDialog.show(errTitle, prefix, value.responseText);
	//			});
	//			ajax.done(function () {
	//				IP.message.info.raise("Retry job started", $(".cardContainer"));
	//			});
	//		}
	//	});
	//};

	root.saveAsProfile = function (integrationPointId, workspaceId, ipName) {
		const SAVE_AS_PROFILE_ERR_PREFIX = "Failed to save integration point as profile.";

		var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(function (value) {
			var ajax = IP.data.ajax({
                url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/SaveAsProfile'),
                type: 'POST',
                data: JSON.stringify({
                    "integrationPointArtifactId": integrationPointId,
                    "profileName": value
                }),
				success: function () {
					IP.message.info.raise("Profile has been saved", $("#customRDOWithConsoleWrapper"));
				}
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

$(window).on("unload", function () {
	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
	obj.push({ key: 'Scheduled Time', value: IP.timeUtil.format24HourToMilitaryTime(result.scheduledTime, "h:mm A") + (timeZone ? "; " + timeZone.DisplayName : "") });
});