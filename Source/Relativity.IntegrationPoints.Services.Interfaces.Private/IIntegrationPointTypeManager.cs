using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Manager for Integration Point Types
    /// </summary>
    [WebService("Integration Point Type Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IIntegrationPointTypeManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Retrieve all Integration Point Types
        /// </summary>
        /// <param name="workspaceArtifactId">The Workspace artifact Id of which has installed integration point application</param>
        /// <returns>Integration Point Types</returns>
        Task<IList<IntegrationPointTypeModel>> GetIntegrationPointTypes(int workspaceArtifactId);
    }
}
