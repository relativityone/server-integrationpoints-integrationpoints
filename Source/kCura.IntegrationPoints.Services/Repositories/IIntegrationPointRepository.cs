using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IIntegrationPointRepository
	{
		IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request);
		IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request);
		IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId);
		object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId);
		IList<IntegrationPointModel> GetAllIntegrationPoints();
		IList<IntegrationPointModel> GetEligibleToPromoteIntegrationPoints();
		int GetIntegrationPointArtifactTypeId();
		IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
		IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactId, string integrationPointName);
	}
}