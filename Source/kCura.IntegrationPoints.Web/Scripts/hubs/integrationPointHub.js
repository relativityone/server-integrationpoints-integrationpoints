(function ($) {
	$(function () {
		var data = $.connection.IntegrationPointData;
		// Create a function that the hub can call back to update properties.
		data.client.updateIntegrationPointData = function (integrationPoint, buttonStates, onClickEvents, sourceProviderIsRelativity) {
			var RUN = "Run";
			var STOP = "Stop";
			var RETRY_ERRORS = "Retry Errors";
			var VIEW_ERRORS = "View Errors";
			var DOWNLOAD_ERROR_FILE = "Download Error File";
			var hasErrors = integrationPoint.HasErrors;
			if (hasErrors) {
				$("input[fafriendlyname='Has Errors'][type='hidden']").siblings('.dynamicViewFieldValue').text("Yes");
			} else {
				$("input[fafriendlyname='Has Errors'][type='hidden']").siblings('.dynamicViewFieldValue').text("No");
			}
			var lastRun = integrationPoint.LastRun;
			if (lastRun) {
				$("input[fafriendlyname='Last Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(lastRun).toString('M/d/yyyy h:mm tt'));
			}
			var nextRun = integrationPoint.NextRun;
			if (nextRun) {
				$("input[fafriendlyname='Next Scheduled Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(nextRun).toString('M/d/yyyy h:mm tt'));
			}

			var consoleContainer = $(".ConsoleControl");

			if (buttonStates.RunButtonEnabled) {
				var runOnClick = onClickEvents.RunOnClickEvent;
				$(consoleContainer.find(":contains('" + STOP + "')")).removeClass("consoleButtonDestructive").removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", runOnClick).attr("title", RUN).html(RUN).removeAttr('disabled');
			} else if (buttonStates.StopButtonEnabled) {
				var stopClick = onClickEvents.StopOnClickEvent;
				$(consoleContainer.find(":contains('" + RUN + "')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDestructive").attr("onClick", stopClick).attr("title", STOP).html(STOP);
			} else {
				var stopButton = $(consoleContainer.find(":contains('" + STOP + "')"));
				if (stopButton.length === 0) {
					var runButton = $(consoleContainer.find(":contains('" + RUN + "')"));
					runButton.attr("title", STOP).html(STOP);
				}
				$(consoleContainer.find(":contains('" + STOP + "')")).removeClass("consoleButtonDestructive").addClass("consoleButtonDisabled").removeAttr('onClick');
			}

			if (sourceProviderIsRelativity) {
				if (buttonStates.RetryErrorsButtonEnabled) {
					var retryErrorsClick = onClickEvents.RetryErrorsOnClickEvent;
					$(consoleContainer.find(":contains('" + RETRY_ERRORS + "')")).removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", retryErrorsClick).attr("title", RETRY_ERRORS).removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('" + RETRY_ERRORS + "')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDisabled").removeAttr('onClick');
				}
				if (buttonStates.ViewErrorsLinkEnabled) {
					var viewErrorsClick = onClickEvents.ViewErrorsOnClickEvent;
					$(consoleContainer.find(":contains('" + VIEW_ERRORS + "')")).removeClass("consoleLinkDisabled").addClass("consoleLinkEnabled").attr("onClick", viewErrorsClick).removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('" + VIEW_ERRORS + "')")).removeClass("consoleLinkEnabled").addClass("consoleLinkDisabled").removeAttr('onClick');
				}
			}

			if (buttonStates.DownloadErrorFileLinkVisible) {
				var downloadErrorsClick = onClickEvents.DownloadErrorFileOnClickEvent;
				if (hasErrors) {
					$(consoleContainer.find(":contains('" + DOWNLOAD_ERROR_FILE + "')")).removeClass("consoleLinkDisabled").addClass("consoleLinkEnabled").attr("onClick", downloadErrorsClick).removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('" + DOWNLOAD_ERROR_FILE + "')")).removeClass("consoleLinkEnabled").addClass("consoleLinkDisabled").removeAttr("onClick").attr("disabled");
				}
			}

			$('.associative-list').load(document.URL + ' .associative-list');
		};

		$.connection.hub.start({ transport: 'longPolling' }).done(function () {
			var workspaceId = IP.utils.getParameterByName("AppID");
			var objectId = IP.utils.getParameterByName("ArtifactID");
			data.server.getIntegrationPointUpdate(workspaceId, objectId);
		});
	});
})(jQuery);