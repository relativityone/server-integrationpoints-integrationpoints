using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Updates Promote Eligible field for existing Integration Points and Integration Point Profiles.")]
	[RunOnce(true)]
	[Guid("6D9D621C-D873-47ED-A1D5-528B05184907")]
	public class SetPromoteEligibleFieldEventHandler : PostInstallEventHandler
	{
		public override Response Execute()
		{
			var eventHandlerCommandExecutor = new EventHandlerCommandExecutor(Helper.GetLoggerFactory().GetLogger());
			return eventHandlerCommandExecutor.Execute(SetPromoteEligibleFieldCommandFactory.Create(Helper, Helper.GetActiveCaseID()));
		}
	}
}