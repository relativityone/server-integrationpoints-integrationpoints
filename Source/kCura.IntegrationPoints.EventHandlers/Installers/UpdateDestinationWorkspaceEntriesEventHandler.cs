using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Updates Destination Workspace entires with Federated Instance name.")]
	[RunOnce(true)]
	[Guid("C91E34A1-856B-490F-B15D-575CA9021BD2")]
	public class UpdateDestinationWorkspaceEntriesEventHandler : PostInstallEventHandler
	{
		public override Response Execute()
		{
			var eventHandlerCommandExecutor = new EventHandlerCommandExecutor(Helper.GetLoggerFactory().GetLogger());
			return eventHandlerCommandExecutor.Execute(UpdateDestinationWorkspaceEntriesFactory.Create(Helper, Helper.GetActiveCaseID()));
		}
	}
}