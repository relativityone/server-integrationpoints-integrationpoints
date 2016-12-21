using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Extensions;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointProfileRepository : IntegrationPointBaseRepository, IIntegrationPointProfileRepository
	{
		private readonly IIntegrationPointProfileService _integrationPointProfileService;
		private readonly IChoiceQuery _choiceQuery;

		public IntegrationPointProfileRepository(IBackwardCompatibility backwardCompatibility, IIntegrationPointProfileService integrationPointProfileService,
			IChoiceQuery choiceQuery)
			: base(backwardCompatibility)
		{
			_integrationPointProfileService = integrationPointProfileService;
			_choiceQuery = choiceQuery;
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
			IntegrationPointProfile integrationPointProfile = _integrationPointProfileService.GetRdo(integrationPointProfileArtifactId);
			return integrationPointProfile.ToIntegrationPointModel();
		}

		public IList<IntegrationPointModel> GetAllIntegrationPointProfiles()
		{
			IList<IntegrationPointProfile> integrationPoints = _integrationPointProfileService.GetAllRDOs();
			return integrationPoints.Select(x => x.ToIntegrationPointModel()).ToList();
		}

		public override IList<OverwriteFieldsModel> GetOverwriteFieldChoices()
		{
			var choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields));
			return choices.Select(x => x.ToModel()).ToList();
		}
	}
}