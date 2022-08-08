using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [Description("Updates Destination Workspace entires with Federated Instance name.")]
    [RunOnce(true)]
    [Guid("C91E34A1-856B-490F-B15D-575CA9021BD2")]
    public class UpdateDestinationWorkspaceEntriesEventHandler : PostInstallEventHandlerBase
    {
        protected override IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<UpdateDestinationWorkspaceEntriesEventHandler>();
        }

        protected override string SuccessMessage => "Destination Workspace entries successfully updated.";

        protected override string GetFailureMessage(Exception ex)
        {
            return "Failed to update Destination Workspace entries.";
        }

        protected override void Run()
        {
            ICommand command = SetTypeOfExportDefaultValueCommandFactory.Create(Helper, Helper.GetActiveCaseID());
            command.Execute();
        }
    }
}