using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.Relativity.Client.DTOs;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class RelativityIntegrationPointsRepository : IRelativityIntegrationPointsRepository
	{
		private readonly ILibraryFactory _libraryFactory;
		private readonly IIntegrationPointByProvidersQueryBuilder _integrationPointByProvidersQueryBuilder;
		private readonly ISourceProviderArtifactIdByGuidQueryBuilder _sourceProviderArtifactIdByGuidQueryBuilder;
		private readonly IDestinationProviderArtifactIdByGuidQueryBuilder _destinationProviderArtifactIdByGuidQueryBuilder;

		public RelativityIntegrationPointsRepository(ILibraryFactory libraryFactory, IIntegrationPointByProvidersQueryBuilder integrationPointByProvidersQueryBuilder,
			ISourceProviderArtifactIdByGuidQueryBuilder sourceProviderArtifactIdByGuidQueryBuilder,
			IDestinationProviderArtifactIdByGuidQueryBuilder destinationProviderArtifactIdByGuidQueryBuilder)
		{
			_libraryFactory = libraryFactory;
			_integrationPointByProvidersQueryBuilder = integrationPointByProvidersQueryBuilder;
			_sourceProviderArtifactIdByGuidQueryBuilder = sourceProviderArtifactIdByGuidQueryBuilder;
			_destinationProviderArtifactIdByGuidQueryBuilder = destinationProviderArtifactIdByGuidQueryBuilder;
		}

		public List<IntegrationPoint> RetrieveRelativityIntegrationPoints(int workspaceId)
		{
			var sourceProvider = RetrieveRelativitySourceProvider(workspaceId);
			var destinationProvider = RetrieveRelativityDestinationProvider(workspaceId);

			var integrationPoints = RetrieveIntegrationPoints(workspaceId, sourceProvider, destinationProvider);
			return integrationPoints;
		}

		private List<IntegrationPoint> RetrieveIntegrationPoints(int workspaceId, List<SourceProvider> sourceProvider, List<DestinationProvider> destinationProvider)
		{
			Query<RDO> integrationPointsQuery = _integrationPointByProvidersQueryBuilder.CreateQuery(sourceProvider[0].ArtifactId, destinationProvider[0].ArtifactId);
			return _libraryFactory.Create<IntegrationPoint>(workspaceId).Query(integrationPointsQuery);
		}

		private List<DestinationProvider> RetrieveRelativityDestinationProvider(int workspaceId)
		{
			var destinationProviderQuery = _destinationProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID);
			return _libraryFactory.Create<DestinationProvider>(workspaceId).Query(destinationProviderQuery);
		}

		private List<SourceProvider> RetrieveRelativitySourceProvider(int workspaceId)
		{
			var sourceProviderQuery = _sourceProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);
			return _libraryFactory.Create<SourceProvider>(workspaceId).Query(sourceProviderQuery);
		}
	}
}