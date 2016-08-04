(function ($) {
	$(function () {
		var data = $.connection.IntegrationPointData;
		// Create a function that the hub can call back to update properties.
		data.client.updateIntegrationPointData = function (integrationPoint, buttonStates, onClickEvents, sourceProviderIsRelativity) {
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
			if (sourceProviderIsRelativity) {
				if (buttonStates.RunNowButtonEnabled) {
					var runNowOnClick = onClickEvents.RunNowOnClickEvent;
					$(consoleContainer.find(":contains('Stop')")).removeClass("consoleButtonDestructive").removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", runNowOnClick).attr("title", "Run Now").html('Run Now').removeAttr('disabled');
				} else if (buttonStates.StopButtonEnabled) {
					var stopClick = onClickEvents.StopOnClickEvent;
					$(consoleContainer.find(":contains('Run Now')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDestructive").attr("onClick", stopClick).attr("title", "Stop").html('Stop');
				} else {
					$(consoleContainer.find(":contains('Stop')")).removeClass("consoleButtonDestructive").addClass("consoleButtonDisabled").removeAttr('onClick');
				}
				
				if (buttonStates.RetryErrorsButtonEnabled) {
					var retryErrorsClick = onClickEvents.RetryErrorsOnClickEvent;
					$(consoleContainer.find(":contains('Retry Errors')")).removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", retryErrorsClick).attr("title", "Retry Errors").removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('Retry Errors')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDisabled").removeAttr('onClick');
				}
				if (buttonStates.ViewErrorsLinkEnabled) {
					var viewErrorsClick = onClickEvents.ViewErrorsOnClickEvent;
					$(consoleContainer.find(":contains('View Errors')")).removeClass("consoleLinkDisabled").addClass("consoleLinkEnabled").attr("onClick", viewErrorsClick).removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('View Errors')")).removeClass("consoleLinkEnabled").addClass("consoleLinkDisabled").removeAttr('onClick');
				}
			}
			else
			{
				//this section will be deleted when we extend the 'Action button' logic to other providers
				if (buttonStates.StopButtonEnabled) {
					var stopOnClick = onClickEvents.StopOnClickEvent;
					$(consoleContainer.find(":contains('Stop')")).removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", stopOnClick).attr("title", "Stop").removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('Stop')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDisabled").removeAttr('onClick');
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