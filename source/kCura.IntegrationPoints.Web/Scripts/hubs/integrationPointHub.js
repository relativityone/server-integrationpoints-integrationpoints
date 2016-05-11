(function ($) {

    $(function () {
        var data = $.connection.IntegrationPointData;
        // Create a function that the hub can call back to update properties.
        data.client.updateIntegrationPointData = function (integrationPoint) {
 
            var lastRun = integrationPoint.LastRun;
            if (lastRun) {
                $("input[fafriendlyname='Last Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(lastRun).toString('M/d/yyyy h:mm tt'));
            }
            var nextRun = integrationPoint.NextRun;
            if (nextRun) {
                $("input[fafriendlyname='Next Scheduled Runtime (UTC)'][type='hidden']").siblings('.dynamicViewFieldValue').text(Date.parse(nextRun).toString('M/d/yyyy h:mm tt'));
            }
        };

        $.connection.hub.start({ transport: 'longPolling' }).done(function () {
            var workspaceId = IP.utils.getParameterByName("AppID");
            var objectId = IP.utils.getParameterByName("ArtifactID");
            data.server.getIntegrationPointUpdate(workspaceId, objectId);
        });

    });

})(jQuery);
