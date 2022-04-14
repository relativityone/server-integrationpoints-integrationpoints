using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
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
		/// Get Artifact Id of the source provider object in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <param name="sourceProviderGuidIdentifier">The source provider guid identifier that we want the artifact id of</param>
		/// <returns>The Artifact id of the source provider specified</returns>
		[Obsolete("Use ProviderManager instead")]
		Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier);

		/// <summary>
		/// Get Artifact Id of the destination provider object in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
		/// <param name="destinationProviderGuidIdentifier">The destination provider guid identifier that we want the artifact id of</param>
		/// <returns>The Artifact id of the destination provider specified</returns>
		[Obsolete("Use ProviderManager instead")]
		Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier);

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
		Task<object> RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId);

		/// <summary>
		/// Retry integration point
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace's id where the requested integration point object lives</param>
		/// <param name="integrationPointArtifactId">the integration point's artifact id</param>
		/// <param name="switchToAppendOverlayMode">if set to <see langword="true"/> the job will be retried with Append/Overlay mode</param>
		Task<object> RetryIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId, bool switchToAppendOverlayMode = false);

		/// <summary>
		/// Get all available integration point objects in the workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <returns>A list of integration point objects</returns>
		Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId);

		/// <summary>
		/// Get all eligible to promote integration point objects in the workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <returns>A list of integration point objects which are eligible to promote</returns>
		[Obsolete("Method will be deprecated in next releases. ECA & Investigation Application has been sunset.")]
		Task<IList<IntegrationPointModel>> GetEligibleToPromoteIntegrationPointsAsync(int workspaceArtifactId);

		/// <summary>
		/// Retrieve Overwrite Fields choices
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <returns>A list of all available choices for Overwrite Fields field</returns>
		Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId);

		/// <summary>
		/// Create integration point based on existing integration point profile
		/// </summary>
		/// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
		/// <param name="profileArtifactId">Artifact ID of profile which will be used to create integration point</param>
		/// <param name="integrationPointName">Integration point's name</param>
		/// <returns></returns>
		Task<IntegrationPointModel> CreateIntegrationPointFromProfileAsync(int workspaceArtifactId, int profileArtifactId, string integrationPointName);
	}
}