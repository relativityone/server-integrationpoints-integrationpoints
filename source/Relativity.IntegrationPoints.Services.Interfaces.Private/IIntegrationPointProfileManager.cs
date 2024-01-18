using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    [WebService("Integration Point Profile Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IIntegrationPointProfileManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Create integration point profile from the given model
        /// </summary>
        /// <param name="request">Integration point profile model to create integration point object.</param>
        /// <returns>Integration point model</returns>
        Task<IntegrationPointModel> CreateIntegrationPointProfileAsync(CreateIntegrationPointRequest request);

        /// <summary>
        /// Update integration point profile from the given model
        /// </summary>
        /// <param name="request">Integration point profile model to update integration point's object.</param>
        /// <returns>Integration point model</returns>
        Task<IntegrationPointModel> UpdateIntegrationPointProfileAsync(CreateIntegrationPointRequest request);

        /// <summary>
        /// Retrieve integration point profile object
        /// </summary>
        /// <param name="workspaceArtifactId">The workspace's id where the requested integration point profile object lives</param>
        /// <param name="integrationPointProfileArtifactId">The integration point profile's artifact id</param>
        /// <returns>Integration point model</returns>
        Task<IntegrationPointModel> GetIntegrationPointProfileAsync(int workspaceArtifactId, int integrationPointProfileArtifactId);

        /// <summary>
        /// Get all available integration point profile objects in the workspace.
        /// </summary>
        /// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
        /// <returns>A list of integration point profile objects</returns>
        Task<IList<IntegrationPointModel>> GetAllIntegrationPointProfilesAsync(int workspaceArtifactId);

        /// <summary>
        /// Retrieve Overwrite Fields choices
        /// </summary>
        /// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
        /// <returns>A list of all available choices for Overwrite Fields field</returns>
        Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId);

        /// <summary>
        /// Create integration point profile based on existing integration point
        /// </summary>
        /// <param name="workspaceArtifactId">An artifact id of the workspace.</param>
        /// <param name="integrationPointArtifactId">Artifact ID of integration point which will be used to create profile</param>
        /// <param name="profileName">Integration point profile's name</param>
        /// <returns></returns>
        Task<IntegrationPointModel> CreateIntegrationPointProfileFromIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId, string profileName);
    }
}
