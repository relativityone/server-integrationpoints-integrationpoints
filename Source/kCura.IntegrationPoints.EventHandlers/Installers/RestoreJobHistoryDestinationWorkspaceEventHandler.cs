using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Remove Instance name from DestinationWorkspace and set it in DestinationInstance.")]
	[RunOnce(true)]
	[Guid("B7EE6A17-7B0E-4391-9014-99618F7C72F9")]
	public class RestoreJobHistoryDestinationWorkspaceEventHandler : PostInstallEventHandler
	{
		public override Response Execute()
		{
			var eventHandlerCommandExecutor = new EventHandlerCommandExecutor(Helper.GetLoggerFactory().GetLogger());
			return eventHandlerCommandExecutor.Execute(RestoreJobHistoryDestinationWorkspaceCommandFactory.Create(Helper, Helper.GetActiveCaseID()));
		}
	}
}