﻿using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
	public interface IIntegrationPointRepository
	{
		IntegrationPointModel CreateIntegrationPoint(CreateIntegrationPointRequest request);
		IntegrationPointModel UpdateIntegrationPoint(UpdateIntegrationPointRequest request);
		IntegrationPointModel GetIntegrationPoint(int integrationPointArtifactId);
		object RunIntegrationPoint(int workspaceArtifactId, int integrationPointArtifactId);
		IList<IntegrationPointModel> GetAllIntegrationPoints();
		int GetIntegrationPointArtifactTypeId();
		IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
		IntegrationPointModel CreateIntegrationPointFromProfile(int profileArtifactId, string integrationPointName);
	}
}