using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Extensions;
using kCura.IntegrationPoints.Services.Helpers;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IUserInfo _userInfo;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IBackwardCompatibility _backwardCompatibility;

		public IntegrationPointRepository(IIntegrationPointService integrationPointService, IObjectTypeRepository objectTypeRepository, IUserInfo userInfo,
			IChoiceQuery choiceQuery, IBackwardCompatibility backwardCompatibility)
		{
			_integrationPointService = integrationPointService;
			_objectTypeRepository = objectTypeRepository;
			_userInfo = userInfo;
			_choiceQuery = choiceQuery;
			_backwardCompatibility = backwardCompatibility;
		}

		public IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request)
		{
			request.IntegrationPoint.ArtifactId = 0;
			return SaveIntegrationPoint(request);
		}

		public IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request)
		{
			return SaveIntegrationPoint(request);
		}

		private IntegrationPointModel SaveIntegrationPoint(CreateIntegrationPointRequest request)
		{
			var overwriteFieldsName = GetOverwriteFieldsName(request.IntegrationPoint.OverwriteFieldsChoiceId);
			_backwardCompatibility.FixIncompatibilities(request.IntegrationPoint, overwriteFieldsName);
			var integrationPointModel = request.IntegrationPoint.ToCoreModel(overwriteFieldsName);
			var artifactId = _integrationPointService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactId);
		}

		private string GetOverwriteFieldsName(int overwriteFieldsId)
		{
			//TODO remove this hack when IntegrationPointModel will start using ChoiceId instead of ChoiceName
			return _choiceQuery.GetChoicesOnField(new Guid(IntegrationPointFieldGuids.OverwriteFields)).First(x => x.ArtifactID == overwriteFieldsId).Name;
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