using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Services.Extensions;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointRepository : IntegrationPointBaseRepository, IIntegrationPointRepository
	{
		private readonly IIntegrationPointRuntimeServiceFactory _serviceFactory;
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IUserInfo _userInfo;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IIntegrationPointService _integrationPointLocalService;
		private readonly IIntegrationPointProfileService _integrationPointProfileService;

		public IntegrationPointRepository(
			IIntegrationPointRuntimeServiceFactory serviceFactory,
			IObjectTypeRepository objectTypeRepository,
			IUserInfo userInfo,
			IChoiceQuery choiceQuery,
			IBackwardCompatibility backwardCompatibility,
			IIntegrationPointService integrationPointLocalService,
			IIntegrationPointProfileService integrationPointProfileService,
			ICaseServiceContext caseServiceContext) : base(backwardCompatibility, caseServiceContext)
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
			int artifactId = SaveIntegrationPoint(request);
			return GetIntegrationPoint(artifactId);
		}

		public IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request)
		{
			int artifactId = SaveIntegrationPoint(request);
			return GetIntegrationPoint(artifactId);
		}

		public override int Save(IntegrationPointModel model, string overwriteFieldsName)
		{
			kCura.IntegrationPoints.Core.Models.IntegrationPointModel integrationPointModel = model.ToCoreModel(overwriteFieldsName);
			IIntegrationPointService integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(integrationPointModel);
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
			var integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(kCura.IntegrationPoints.Core.Models.IntegrationPointModel.FromIntegrationPoint(integrationPoint));
			integrationPointRuntimeService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID);
			return null;
		}

		public object RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, bool switchToAppendOverlayMode)
		{
			IntegrationPoint integrationPoint = _integrationPointLocalService.ReadIntegrationPoint(integrationPointArtifactId);
			var integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(kCura.IntegrationPoints.Core.Models.IntegrationPointModel.FromIntegrationPoint(integrationPoint));
			integrationPointRuntimeService.RetryIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID, switchToAppendOverlayMode);
			return null;
		}

		public IList<IntegrationPointModel> GetAllIntegrationPoints()
		{
			IList<IntegrationPoint> integrationPoints = _integrationPointLocalService.GetAllRDOs();
			return integrationPoints.Select(x => x.ToIntegrationPointModel()).ToList();
		}

		public int GetIntegrationPointArtifactTypeId()
		{
			return _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(ObjectTypeGuids.IntegrationPointGuid);
		}

		public override IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
		{
			List<ChoiceRef> choices = _choiceQuery.GetChoicesOnField(Context.WorkspaceID, IntegrationPointFieldGuids.OverwriteFieldsGuid);
			return choices.Select(Mapper.Map<OverwriteFieldsModel>).ToList();
		}

		public IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactID, string integrationPointName)
		{
			IntegrationPointProfile integrationPointProfile = _integrationPointProfileService.ReadIntegrationPointProfile(profileArtifactID);
			kCura.IntegrationPoints.Core.Models.IntegrationPointModel integrationPointModel = kCura.IntegrationPoints.Core.Models.IntegrationPointModel.FromIntegrationPointProfile(integrationPointProfile, integrationPointName);
			IIntegrationPointService integrationPointRuntimeService = _serviceFactory.CreateIntegrationPointRuntimeService(integrationPointModel);
			int artifactID = integrationPointRuntimeService.SaveIntegration(integrationPointModel);
			return GetIntegrationPoint(artifactID);
		}
	}
}
