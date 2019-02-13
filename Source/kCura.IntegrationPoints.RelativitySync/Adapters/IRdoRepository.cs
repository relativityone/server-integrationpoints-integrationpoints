using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal interface IRdoRepository
	{
		T Get<T>(int artifactId) where T : BaseRdo, new();
	}
}
