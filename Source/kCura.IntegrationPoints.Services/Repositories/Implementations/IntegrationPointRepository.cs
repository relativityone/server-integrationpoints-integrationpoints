using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Extensions;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointRepository : IntegrationPointBaseRepository, IIntegrationPointRepository
	{
		private readonly IIntegrationPointRuntimeServiceFactory _serviceFactory;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IUserInfo _userInfo;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IIntegrationPointService _integrationPointLocalService;
		private readonly IIntegrationPointProfileService _integrationPointProfileService;

		public IntegrationPointRepository(IIntegrationPointRuntimeServiceFactory serviceFactory, IObjectTypeRepository objectTypeRepository, IUserInfo userInfo, IChoiceQuery choiceQuery, 
			IBackwardCompatibility backwardCompatibility, IIntegrationPointService integrationPointLocalService, IIntegrationPointProfileService integrationPointProfileService) : base(backwardCompatibility)
		{
			_serviceFactory = serviceFactory;
			_objectTypeRepository = objectTypeRepository;
			_userInfo = userInfo;
			_choiceQuery = choiceQuery;
			_integrationPointLocalService = integrationPointLocalService;
			_integrationPointProfileService = integrationPointProfileService;
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
			var integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(integrationPointModel);
			return integrationPointRuntimeService.SaveIntegration(integrationPointModel);
		}

		public IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId)
		{
			IntegrationPoint integrationPoint = _integrationPointLocalService.ReadIntegrationPoint(integrationPointArtifactId);
			return integrationPoint.ToIntegrationPointModel();
		}

		public object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IntegrationPoint integrationPoint = _integrationPointLocalService.ReadIntegrationPoint(integrationPointArtifactId);
			var integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(Core.Models.IntegrationPointModel.FromIntegrationPoint(integrationPoint));
			integrationPointRuntimeService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID);
			return null;
		}

		public IList<IntegrationPointModel> GetAllIntegrationPoints()
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointLocalService.GetAllRDOs();
			return integrationPoints.Select(x => x.ToIntegrationPointModel()).ToList();
		}
		
		public IList<IntegrationPointModel> GetEligibleToPromoteIntegrationPoints()
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointLocalService.GetAllRDOs();
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
			var profile = _integrationPointProfileService.ReadIntegrationPointProfile(profileArtifactId);
			var integrationPointModel = Core.Models.IntegrationPointModel.FromIntegrationPointProfile(profile, integrationPointName);
			var integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(integrationPointModel);
			var artifactId = integrationPointRuntimeService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactId);
		}
	}
}