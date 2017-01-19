using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IHelperFactory
	{
		IHelper CreateOAuthClientHelper(IHelper sourceInstanceHelper, int federatedInstanceArtifactId);
	}
}