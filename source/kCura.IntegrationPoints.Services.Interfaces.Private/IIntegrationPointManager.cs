using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	[WebService("Integration Point Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IIntegrationPointManager : IKeplerService, IDisposable
	{
		/// <summary>
		/// Get ArtifactType Id of the integration point object in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <returns>The ArtifactType id of integration point</returns>
		Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId);
		
		/// <summary>
		/// Create integration point from the given model
		/// </summary>
		/// <param name="request">Integration point model to create integration point object.</param>
		/// <returns>Integration point model</returns>
		Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request);

		/// <summary>
		/// Update integration point from the given model
		/// </summary>
		/// <param name="request">Integration point model to update integration point's object.</param>
		/// <returns>Integration point model</returns>
		Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request);

		/// <summary>
		/// Retrieve integration point object
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace's id where the requested integration point object lives</param>
		/// <param name="integrationPointArtifactId">The integration point's artifact id</param>
		/// <returns>Integration point model</returns>
		Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId);

		/// <summary>
		/// Execute integration point
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace's id where the requested integration point object lives</param>
		/// <param name="integrationPointArtifactId">the integration point's artifact id</param>
		Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId);

		/// <summary>
		/// Get all available integration point objects in the workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <returns>A list of integration point objects</returns>
		Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId);
	}
}