using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IHelperFactory
	{
		IHelper CreateTargetHelper(IHelper sourceInstanceHelper, int? federatedInstanceArtifactId, string credentials);
	}
}