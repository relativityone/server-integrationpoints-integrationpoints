using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Core.Models;
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
    public class IntegrationPointAccessor : IntegrationPointAccessorBase, IIntegrationPointAccessor
    {
        private readonly IObjectTypeRepository _objectTypeRepository;
        private readonly IUserInfo _userInfo;
        private readonly IChoiceQuery _choiceQuery;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;

        public IntegrationPointAccessor(
            IObjectTypeRepository objectTypeRepository,
            IUserInfo userInfo,
            IChoiceQuery choiceQuery,
            IBackwardCompatibility backwardCompatibility,
            IIntegrationPointService integrationPointService,
            IIntegrationPointProfileService integrationPointProfileService,
            ICaseServiceContext caseServiceContext)
            : base(backwardCompatibility, caseServiceContext)
        {
            _objectTypeRepository = objectTypeRepository;
            _userInfo = userInfo;
            _choiceQuery = choiceQuery;
            _integrationPointService = integrationPointService;
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

        protected override int Save(IntegrationPointModel model, string overwriteFieldsName)
        {
            IntegrationPointDto integrationPointDto = model.ToCoreModel(overwriteFieldsName);
            return _integrationPointService.SaveIntegrationPoint(integrationPointDto);
        }

        public IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId)
        {
            return _integrationPointService.ReadSlim(integrationPointArtifactId).ToIntegrationPointModel();
        }

        public object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId)
        {
            _integrationPointService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID);
            return null;
        }

        public object RetryIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId, bool switchToAppendOverlayMode)
        {
            _integrationPointService.RetryIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, _userInfo.ArtifactID, switchToAppendOverlayMode);
            return null;
        }

        public IList<IntegrationPointModel> GetAllIntegrationPoints()
        {
            IList<IntegrationPointSlimDto> integrationPoints = _integrationPointService.ReadAllSlim();
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
            IntegrationPointProfileDto profile = _integrationPointProfileService.Read(profileArtifactID);
            IntegrationPointDto integrationPointDto = profile.ToIntegrationPointDto(integrationPointName);
            int artifactID = _integrationPointService.SaveIntegrationPoint(integrationPointDto);
            return GetIntegrationPoint(artifactID);
        }
    }
}
