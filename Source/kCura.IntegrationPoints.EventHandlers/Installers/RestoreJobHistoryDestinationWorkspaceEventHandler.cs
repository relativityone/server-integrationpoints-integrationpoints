using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Remove Instance name from DestinationWorkspace and set it in DestinationInstance.")]
    [RunOnce(true)]
    [Guid("B7EE6A17-7B0E-4391-9014-99618F7C72F9")]
    public class RestoreJobHistoryDestinationWorkspaceEventHandler : PostInstallEventHandlerBase
    {
        protected override string SuccessMessage => "Successfully updated Job History RDOs.";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Failed to update Job History RDOs.";
        }

        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<RestoreJobHistoryDestinationWorkspaceEventHandler>();
        }

        protected override void Run()
        {
            ICommand command = RestoreJobHistoryDestinationWorkspaceCommandFactory.Create(Helper, Helper.GetActiveCaseID());
            command.Execute();
        }
    }
}