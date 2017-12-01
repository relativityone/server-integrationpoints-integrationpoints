using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Commands;
using kCura.IntegrationPoints.EventHandlers.Commands.Factories;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Updates Promote Eligible field for existing Integration Points and Integration Point Profiles.")]
	[RunOnce(true)]
	[Guid("6D9D621C-D873-47ED-A1D5-528B05184907")]
	public class SetPromoteEligibleFieldEventHandler : PostInstallEventHandlerBase
	{

		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<SetPromoteEligibleFieldEventHandler>();
		}

		protected override string SuccessMessage => "'Promote Eligible' field successfully updated in IntgrationPoint/IntgrationPointProfile tables.";

		protected override string GetFailureMessage(System.Exception ex)
		{
			return "'Promote Eligible' field failed in IntgrationPoint/IntgrationPointProfile tables.";
		}

		protected override void Run()
		{
			ICommand command = SetPromoteEligibleFieldCommandFactory.Create(Helper, Helper.GetActiveCaseID());
			command.Execute();
		}
	}
}