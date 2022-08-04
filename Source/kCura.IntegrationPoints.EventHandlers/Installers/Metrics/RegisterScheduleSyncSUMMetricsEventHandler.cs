using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;
using kCura.IntegrationPoints.EventHandlers.Commands.Metrics;
using System;
using System.Runtime.InteropServices;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Metrics
{
    [Description("Register Schedule Sync metrics")]
    [RunOnce(true)]
    [Guid("87E4A98B-1EA8-4776-A12C-9AA7F1E29589")]
    public class RegisterScheduleSyncSUMMetricsEventHandler : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(RegisterScheduleJobSumMetricsCommand);

        protected override string SuccessMessage => "Schedule Sync Metrics has been successfully installed";

        protected override string GetFailureMessage(Exception ex)
        {
            const string errorMsg = "Schedule Sync Metrics installation failed";

            Logger.LogError(ex, errorMsg);

            return errorMsg;
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
