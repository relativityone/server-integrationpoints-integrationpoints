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
    public class IntegrationPointProfileRepository : IntegrationPointBaseRepository, IIntegrationPointProfileRepository
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IIntegrationPointProfileService _integrationPointProfileService;
        private readonly IChoiceQuery _choiceQuery;

        public IntegrationPointProfileRepository(IBackwardCompatibility backwardCompatibility, IIntegrationPointProfileService integrationPointProfileService,
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
            return _integrationPointProfileService.SaveIntegration(integrationPointProfileModel);
        }

        public IntegrationPointModel GetIntegrationPointProfile(int integrationPointProfileArtifactId)
        {
            IntegrationPointProfile integrationPointProfile = _integrationPointProfileService.ReadIntegrationPointProfile(integrationPointProfileArtifactId);
            return integrationPointProfile.ToIntegrationPointModel();
        }

        public IList<IntegrationPointModel> GetAllIntegrationPointProfiles()
        {
            IList<IntegrationPointProfile> integrationPoints = _integrationPointProfileService.GetAllRDOs();
            return integrationPoints.Select(x => x.ToIntegrationPointModel()).ToList();
        }

        public override IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
        {
            List<ChoiceRef> choices = _choiceQuery.GetChoicesOnField(Context.WorkspaceID, IntegrationPointProfileFieldGuids.OverwriteFieldsGuid);
            return choices.Select(Mapper.Map<OverwriteFieldsModel>).ToList();
        }

        public IntegrationPointModel CreateIntegrationPointProfileFromIntegrationPoint(int integrationPointArtifactId, string profileName)
        {
            IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(integrationPointArtifactId);
            IntegrationPointProfileModel integrationPointProfileModel = IntegrationPointProfileModel.FromIntegrationPoint(integrationPoint, profileName);

            int artifactId = _integrationPointProfileService.SaveIntegration(integrationPointProfileModel);
            return GetIntegrationPointProfile(artifactId);
        }
    }
}