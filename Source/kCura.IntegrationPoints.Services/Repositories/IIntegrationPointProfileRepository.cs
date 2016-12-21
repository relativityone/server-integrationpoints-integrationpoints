using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IIntegrationPointProfileRepository
	{
		IntegrationPointModel CreateIntegrationPointProfile(CreateIntegrationPointRequest request);
		IntegrationPointModel UpdateIntegrationPointProfile(CreateIntegrationPointRequest request);
		IntegrationPointModel GetIntegrationPointProfile(int integrationPointProfileArtifactId);
		IList<IntegrationPointModel> GetAllIntegrationPointProfiles();
		IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
	}
}