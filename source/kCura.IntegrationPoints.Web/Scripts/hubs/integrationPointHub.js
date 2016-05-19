(function ($) {
	$(function () {
		var data = $.connection.IntegrationPointData;
		// Create a function that the hub can call back to update properties.
		data.client.updateIntegrationPointData = function (integrationPoint, buttonStates, onClickEvents, sourceProviderIsRelativity) {
			var lastRun = integrationPoint.LastRun;
			if (lastRun) {
				$("input[fafriendlyname='Last Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(lastRun).toString('M/d/yyyy h:mm tt'));
			}
			var nextRun = integrationPoint.NextRun;
			if (nextRun) {
				$("input[fafriendlyname='Next Scheduled Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(nextRun).toString('M/d/yyyy h:mm tt'));
			}

			if (sourceProviderIsRelativity) {
				var consoleContainer = $(".ConsoleControl");
				if (buttonStates.RunNowButtonEnabled) {
					var runNowOnClick = onClickEvents.RunNowOnClickEvent;
					$(consoleContainer.find(":contains('Run Now')")).removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", runNowOnClick).removeAttr('disabled');
				} else {
					$(consoleContainer.find(":contains('Run Now')")).removeClass("consoleButtonEnabled").addClass("consoleButtonDisabled").removeAttr('onClick');
				}
				if (buttonStates.RetryErrorsButtonEnabled) {
					var retryErrorsClick = onClickEvents.RetryErrorsOnClickEvent;
					$(consoleContainer.find(":contains('Retry Errors')")).removeClass("consoleButtonDisabled").addClass("consoleButtonEnabled").attr("onClick", retryErrorsClick).removeAttr('disabled');
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
		};

		$.connection.hub.start({ transport: 'longPolling' }).done(function () {
			var workspaceId = IP.utils.getParameterByName("AppID");
			var objectId = IP.utils.getParameterByName("ArtifactID");
			data.server.getIntegrationPointUpdate(workspaceId, objectId);
		});
	});
})(jQuery);