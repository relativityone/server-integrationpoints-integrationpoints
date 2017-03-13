using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class RelativityIntegrationPointsRepositoryAdminAccess : IRelativityIntegrationPointsRepository
	{
		private readonly IHelper _helper;
		private readonly IIntegrationPointByProvidersQueryBuilder _integrationPointByProvidersQueryBuilder;
		private readonly ISourceProviderArtifactIdByGuidQueryBuilder _sourceProviderArtifactIdByGuidQueryBuilder;
		private readonly IDestinationProviderArtifactIdByGuidQueryBuilder _destinationProviderArtifactIdByGuidQueryBuilder;

		public RelativityIntegrationPointsRepositoryAdminAccess(IHelper helper,
			IIntegrationPointByProvidersQueryBuilder integrationPointByProvidersQueryBuilder,
			ISourceProviderArtifactIdByGuidQueryBuilder sourceProviderArtifactIdByGuidQueryBuilder,
			IDestinationProviderArtifactIdByGuidQueryBuilder destinationProviderArtifactIdByGuidQueryBuilder)
		{
			_helper = helper;
			_integrationPointByProvidersQueryBuilder = integrationPointByProvidersQueryBuilder;
			_sourceProviderArtifactIdByGuidQueryBuilder = sourceProviderArtifactIdByGuidQueryBuilder;
			_destinationProviderArtifactIdByGuidQueryBuilder = destinationProviderArtifactIdByGuidQueryBuilder;
		}

		public List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints(int workspaceId)
		{
			var sourceProviderIds = RetrieveRelativitySourceProviderIds(workspaceId);
			var destinationProviderIds = RetrieveRelativityDestinationProviderIds(workspaceId);

			return RetrieveIntegrationPoints(workspaceId, sourceProviderIds, destinationProviderIds);
		}

		private IList<int> RetrieveRelativitySourceProviderIds(int workspaceId)
		{
			var sourceProviderQuery = _sourceProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);
			return QueryArtifactIds(workspaceId, sourceProviderQuery);
		}

		private IList<int> RetrieveRelativityDestinationProviderIds(int workspaceId)
		{
			var destinationProviderQuery = _destinationProviderArtifactIdByGuidQueryBuilder.Create(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID);
			return QueryArtifactIds(workspaceId, destinationProviderQuery);
		}

		private List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints(int workspaceId, IList<int> sourceProvider, IList<int> destinationProvider)
		{
			Query<RDO> integrationPointsQuery = _integrationPointByProvidersQueryBuilder.CreateQuery(sourceProvider[0], destinationProvider[0]);
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				rsapiClient.APIOptions.WorkspaceID = workspaceId;
				var result = rsapiClient.Repositories.RDO.Query(integrationPointsQuery);
				return result.Results.Select(x => new Core.Models.IntegrationPointModel
				{
					SecuredConfiguration = x.Artifact[new Guid(IntegrationPointFieldGuids.SecuredConfiguration)].ToString(),
					ArtifactID = x.Artifact.ArtifactID
				}).ToList();
			}
		}

		private List<int> QueryArtifactIds(int workspaceId, Query<RDO> query)
		{
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				rsapiClient.APIOptions.WorkspaceID = workspaceId;
				var result = rsapiClient.Repositories.RDO.Query(query);
				return result.Results.Select(x => x.Artifact.ArtifactID).ToList();
			}
		}
	}
}