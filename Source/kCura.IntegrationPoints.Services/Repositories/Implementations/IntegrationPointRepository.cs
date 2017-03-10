using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Extensions;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointRepository : IntegrationPointBaseRepository, IIntegrationPointRepository
	{
		private readonly IChoiceQuery _choiceQuery;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IIntegrationPointProfileService _integrationPointProfileService;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IUserInfo _userInfo;

		public IntegrationPointRepository(IIntegrationPointService integrationPointService, IObjectTypeRepository objectTypeRepository, IUserInfo userInfo,
			IChoiceQuery choiceQuery, IBackwardCompatibility backwardCompatibility, IIntegrationPointProfileService integrationPointProfileService) : base(backwardCompatibility)
		{
			_choiceQuery = choiceQuery;
			_integrationPointProfileService = integrationPointProfileService;
			_integrationPointService = integrationPointService;
			_objectTypeRepository = objectTypeRepository;
			_userInfo = userInfo;
		}

		public IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request)
		{
			request.IntegrationPoint.ArtifactId = 0;
			var artifactId = SaveIntegrationPoint(request);
			return GetIntegrationPoint(artifactId);
		}

		public IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request)
		{
			var artifactId = SaveIntegrationPoint(request);
			return GetIntegrationPoint(artifactId);
		}

		public override int Save(IntegrationPointModel model, string overwriteFieldsName)
		{
			var integrationPointModel = model.ToCoreModel(overwriteFieldsName);
			return _integrationPointService.SaveIntegration(integrationPointModel);
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
		
		public IList<IntegrationPointModel> GetEligibleToPromoteIntegrationPoints()
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointService.GetAllRDOs();
			return integrationPoints.Where(x => x.PromoteEligible.GetValueOrDefault(false)).Select(x => x.ToIntegrationPointModel()).ToList();
		}

		public int GetIntegrationPointArtifactTypeId()
		{
			return _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
		}

		public override IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
		{
			var choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			return choices.Select(Mapper.Map<OverwriteFieldsModel>).ToList();
		}

		public IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactId, string integrationPointName)
		{
			var profile = _integrationPointProfileService.GetRdo(profileArtifactId);
			var integrationPointModel = Core.Models.IntegrationPointModel.FromIntegrationPointProfile(profile, integrationPointName);

			var artifactId = _integrationPointService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactId);
		}
	}
}