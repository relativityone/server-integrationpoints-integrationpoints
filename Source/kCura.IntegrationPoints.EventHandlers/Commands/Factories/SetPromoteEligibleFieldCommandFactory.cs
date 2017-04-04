using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public class SetPromoteEligibleFieldCommandFactory
	{
		public static ICommand Create(IHelper helper, int workspaceArtifactId)
		{
			return new SetPromoteEligibleFieldCommand(helper.GetDBContext(workspaceArtifactId));
		}
	}
}