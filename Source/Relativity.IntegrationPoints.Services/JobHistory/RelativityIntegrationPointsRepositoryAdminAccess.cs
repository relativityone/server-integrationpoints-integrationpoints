﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
	public class RelativityIntegrationPointsRepositoryAdminAccess : IRelativityIntegrationPointsRepository
	{
		private readonly IRSAPIService _rsapiService;
		private readonly IIntegrationPointRepository _integrationPointRepository;

		public RelativityIntegrationPointsRepositoryAdminAccess(
			IRSAPIService rsapiService,
			IIntegrationPointRepository integrationPointRepository)
		{
			_rsapiService = rsapiService;
			_integrationPointRepository = integrationPointRepository;
		}

		public List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> RetrieveIntegrationPoints()
		{
			var sourceProviderIds = RetrieveRelativitySourceProviderIds();
			var destinationProviderIds = RetrieveRelativityDestinationProviderIds();

			return RetrieveIntegrationPoints(sourceProviderIds, destinationProviderIds);
		}

		private IList<int> RetrieveRelativitySourceProviderIds()
		{
			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{SourceProviderFields.Identifier}' == '{Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID}'"
			};
			return GetArtifactIds(_rsapiService.RelativityObjectManager.Query<SourceProvider>(request));
		}

		private IList<int> RetrieveRelativityDestinationProviderIds()
		{
			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{DestinationProviderFields.Identifier}' == '{Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID}'"
			};
			return GetArtifactIds(_rsapiService.RelativityObjectManager.Query<DestinationProvider>(request));
		}

		private List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> RetrieveIntegrationPoints(IList<int> sourceProvider, IList<int> destinationProvider)
		{
			return _integrationPointRepository
				.GetAllBySourceAndDestinationProviderIDs(sourceProvider[0], destinationProvider[0])
				.Select(kCura.IntegrationPoints.Core.Models.IntegrationPointModel.FromIntegrationPoint)
				.ToList();
		}

		private List<int> GetArtifactIds<T>(List<T> results) where T : BaseRdo
		{
			return results.Select(x => x.ArtifactId).ToList();
		}
	}
}