using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class IntegrationPointByProvidersQueryBuilder : IIntegrationPointByProvidersQueryBuilder
	{
		public QueryRequest CreateQuery(int sourceProviderArtifactId, int destinationProviderArtifactId)
		{
			return new QueryRequest()
			{
				Condition =
					$"'{IntegrationPointFields.SourceProvider}' == {sourceProviderArtifactId} " +
					$"AND " +
					$"'{IntegrationPointFields.DestinationProvider}' == {destinationProviderArtifactId}"
			};
		}
	}
}