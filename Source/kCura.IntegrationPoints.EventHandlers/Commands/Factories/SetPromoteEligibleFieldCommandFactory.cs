using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public class SetPromoteEligibleFieldCommandFactory
	{
		public static ICommand Create(IHelper helper, int workspaceArtifactId)
		{
			IRSAPIService rsapiService = new RSAPIService(helper, workspaceArtifactId);
			return new SetPromoteEligibleFieldCommand(rsapiService);
		}
	}
}