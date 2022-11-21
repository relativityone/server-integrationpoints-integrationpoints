using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services.Extensions;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
    public class IntegrationPointProfileAccessor : IntegrationPointAccessorBase, IIntegrationPointProfileAccessor
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly IChoiceQuery _choiceQuery;

        public IntegrationPointProfileAccessor(IBackwardCompatibility backwardCompatibility, IIntegrationPointProfileService integrationPointProfileService,
            IChoiceQuery choiceQuery, IIntegrationPointService integrationPointService, ICaseServiceContext caseServiceContext)
            : base(backwardCompatibility, caseServiceContext)
        {
            _integrationPointProfileService = integrationPointProfileService;
            _choiceQuery = choiceQuery;
            _integrationPointService = integrationPointService;
        }

        public IntegrationPointModel CreateIntegrationPointProfile(CreateIntegrationPointRequest request)
        {
            request.IntegrationPoint.ArtifactId = 0;
            var artifactId = SaveIntegrationPoint(request);
            return GetIntegrationPointProfile(artifactId);
        }

        public IntegrationPointModel UpdateIntegrationPointProfile(CreateIntegrationPointRequest request)
        {
            var artifactId = SaveIntegrationPoint(request);
            return GetIntegrationPointProfile(artifactId);
        }

        public override int Save(IntegrationPointModel model, string overwriteFieldsName)
        {
            var integrationPointProfileModel = model.ToCoreProfileModel(overwriteFieldsName);
            return _integrationPointProfileService.SaveProfile(integrationPointProfileModel);
        }

        public IntegrationPointModel GetIntegrationPointProfile(int integrationPointProfileArtifactId)
        {
            IntegrationPointProfileDto integrationPointProfile = _integrationPointProfileService.Read(integrationPointProfileArtifactId);
            return integrationPointProfile.ToIntegrationPointModel();
        }

        public IList<IntegrationPointModel> GetAllIntegrationPointProfiles()
        {
            IList<IntegrationPointProfileDto> profiles = _integrationPointProfileService.ReadAll();
            return profiles.Select(x => x.ToIntegrationPointModel()).ToList();
        }

        public override IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
        {
            List<ChoiceRef> choices = _choiceQuery.GetChoicesOnField(Context.WorkspaceID, IntegrationPointProfileFieldGuids.OverwriteFieldsGuid);
            return choices.Select(Mapper.Map<OverwriteFieldsModel>).ToList();
        }

        public IntegrationPointModel CreateIntegrationPointProfileFromIntegrationPoint(int integrationPointArtifactId, string profileName)
        {
            IntegrationPointDto integrationPoint = _integrationPointService.Read(integrationPointArtifactId);
            IntegrationPointProfileDto integrationPointProfileDto = integrationPoint.ToProfileDto(profileName);

            int artifactId = _integrationPointProfileService.SaveProfile(integrationPointProfileDto);
            return GetIntegrationPointProfile(artifactId);
        }
    }
}
