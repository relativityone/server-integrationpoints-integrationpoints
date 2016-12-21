using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	[WebService("Integration Point Profile Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IIntegrationPointProfileManager : IKeplerService, IDisposable
	{
		Task<IntegrationPointModel> CreateIntegrationPointProfileAsync(CreateIntegrationPointRequest request);

		Task<IntegrationPointModel> UpdateIntegrationPointProfileAsync(CreateIntegrationPointRequest request);

		Task<IntegrationPointModel> GetIntegrationPointProfileAsync(int workspaceArtifactId, int integrationPointProfileArtifactId);

		Task<IList<IntegrationPointModel>> GetAllIntegrationPointProfilesAsync(int workspaceArtifactId);

		/// <summary>
		/// Retrieve Overwrite Fields choices
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <returns>A list of all available choices for Overwrite Fields field</returns>
		Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId);
	}
}