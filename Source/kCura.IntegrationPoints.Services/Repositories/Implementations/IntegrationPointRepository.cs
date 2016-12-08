using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IUserInfo _userInfo;
		private readonly IChoiceQuery _choiceQuery;

		public IntegrationPointRepository(IIntegrationPointService integrationPointService, IObjectTypeRepository objectTypeRepository, IUserInfo userInfo,
			IChoiceQuery choiceQuery)
		{
			_integrationPointService = integrationPointService;
			_objectTypeRepository = objectTypeRepository;
			_userInfo = userInfo;
			_choiceQuery = choiceQuery;
		}

		public IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request)
		{
			var integrationPointModel = request.ToModel(_choiceQuery.GetChoicesOnField(new Guid(IntegrationPointFieldGuids.OverwriteFields)));
			var artifactId = _integrationPointService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactId);
		}

		public IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request)
		{
			var integrationPointModel = request.ToModel(_choiceQuery.GetChoicesOnField(new Guid(IntegrationPointFieldGuids.OverwriteFields)));
			var artifactId = _integrationPointService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactId);
		}

		public IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId)
		{
			IntegrationPoint integrationPoint = _integrationPointService.GetRdo(integrationPointArtifactId);
			return integrationPoint.ToIntegrationPointModel();
		}

		public object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId)
		{
			_integrationPointService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID);
			return null;
		}

		public IList<IntegrationPointModel> GetAllIntegrationPoints()
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointService.GetAllRDOs();
			return integrationPoints.Select(x => x.ToIntegrationPointModel()).ToList();
		}

		public int GetIntegrationPointArtifactTypeId()
		{
			return _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}
	}
}