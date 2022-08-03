using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Telemetry;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Metrics
{
    [Description("Register Integration Points telemetry metrics")]
    [RunOnce(true)]
    [Guid("570BA0F8-4A18-46DD-BAE5-7F5E4A476641")]
    public class RegisterIntegrationPointsMetricsEventHandler: PostInstallEventHandlerBase
    {
        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RegisterIntegrationPointsMetricsEventHandler>();
        }

        protected override string SuccessMessage => "Adding telemetry metrics completed successfully";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Adding Integration Points telemetry metrics failed";
        }

        protected override void Run()
        {
            var telemetryManger = new TelemetryManager(Helper);
            telemetryManger.AddMetricProviders(new TelemetryMetricProvider(Helper));
            telemetryManger.InstallMetrics();
        }
    }
}
