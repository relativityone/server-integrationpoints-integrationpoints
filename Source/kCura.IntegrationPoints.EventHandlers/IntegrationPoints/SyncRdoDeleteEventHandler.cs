using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Context;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    [Description("Deletes Relativity Sync RDOs which are no longer needed.")]
    [RunOnce(true)]
    [Guid("8EC8A9F4-55E6-4C16-BE56-2C03A129F3C9")]
    public class SyncRdoDeleteEventHandler : PostInstallEventHandlerBase, IEventHandler
    {
        public IEHContext Context => new EHContext
        {
            Helper = Helper
        };

        public Type CommandType => typeof(SyncRdoDeleteCommand);

        protected override string SuccessMessage => "Successfully deleted Sync RDOs.";

        protected override string GetFailureMessage(Exception ex)
        {
            return $"Failed to delete Sync RDOs from workspace {Helper.GetActiveCaseID()} due to error: {ex}";
        }

        protected override void Run()
        {
            var executor = new EventHandlerExecutor();
            executor.Execute(this);
        }
    }
}
