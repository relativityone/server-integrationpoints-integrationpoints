using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IIntegrationPointByProvidersQueryBuilder
	{
		QueryRequest CreateQuery(int sourceProviderArtifactId, int destinationProviderArtifactId);
	}
}