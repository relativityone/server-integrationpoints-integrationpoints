using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class RelativityIntegrationPointsRepositoryAdminAccess : IRelativityIntegrationPointsRepository
	{
		private readonly IRSAPIService _rsapiService;
		private readonly IIntegrationPointByProvidersQueryBuilder _integrationPointByProvidersQueryBuilder;
		private readonly ISourceProviderArtifactIdByGuidQueryBuilder _sourceProviderArtifactIdByGuidQueryBuilder;
		private readonly IDestinationProviderArtifactIdByGuidQueryBuilder _destinationProviderArtifactIdByGuidQueryBuilder;

		public RelativityIntegrationPointsRepositoryAdminAccess(IRSAPIService rsapiService,
			IIntegrationPointByProvidersQueryBuilder integrationPointByProvidersQueryBuilder,
			ISourceProviderArtifactIdByGuidQueryBuilder sourceProviderArtifactIdByGuidQueryBuilder,
			IDestinationProviderArtifactIdByGuidQueryBuilder destinationProviderArtifactIdByGuidQueryBuilder)
		{
			_rsapiService = rsapiService;
			_integrationPointByProvidersQueryBuilder = integrationPointByProvidersQueryBuilder;
			_sourceProviderArtifactIdByGuidQueryBuilder = sourceProviderArtifactIdByGuidQueryBuilder;
			_destinationProviderArtifactIdByGuidQueryBuilder = destinationProviderArtifactIdByGuidQueryBuilder;
		}

		public List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints()
		{
			var sourceProviderIds = RetrieveRelativitySourceProviderIds();
			var destinationProviderIds = RetrieveRelativityDestinationProviderIds();

			return RetrieveIntegrationPoints(sourceProviderIds, destinationProviderIds);
		}

		private IList<int> RetrieveRelativitySourceProviderIds()
		{
			var sourceProviderQuery = _sourceProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);
			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{SourceProviderFields.Identifier}' == '{Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID}'"
			};
			return GetArtifactIds(_rsapiService.RelativityObjectManager.Query<SourceProvider>(request));
		}

		private IList<int> RetrieveRelativityDestinationProviderIds()
		{
			var destinationProviderQuery = _destinationProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID);

			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{DestinationProviderFields.Identifier}' == '{Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID}'"
			};
			return GetArtifactIds(_rsapiService.RelativityObjectManager.Query<DestinationProvider>(request));
		}

		private List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints(IList<int> sourceProvider, IList<int> destinationProvider)
		{
			QueryRequest integrationPointsQuery = _integrationPointByProvidersQueryBuilder.CreateQuery(sourceProvider[0], destinationProvider[0]);

			return _rsapiService.RelativityObjectManager.Query<IntegrationPoint>(integrationPointsQuery).Select(Core.Models.IntegrationPointModel.FromIntegrationPoint).ToList();
		}

		private List<int> GetArtifactIds<T>(List<T> results) where T : BaseRdo
		{
			return results.Select(x => x.ArtifactId).ToList();
		}
	}
}