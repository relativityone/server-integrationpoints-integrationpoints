using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Repository responsible for Source Provider object
    /// </summary>
    public interface ISourceProviderRepository : IRepository<SourceProvider>
    {
        /// <summary>
        /// Gets the Source Provider artifact id given a guid identifier
        /// </summary>
        /// <param name="sourceProviderGuidIdentifier">Guid identifier of Source Provider type</param>
        /// <returns>Artifact id of the Source Provider</returns>
        int GetArtifactIdFromSourceProviderTypeGuidIdentifier(string sourceProviderGuidIdentifier);

        Task<List<SourceProvider>> GetSourceProviderRdoByApplicationIdentifierAsync(Guid appGuid);
    }
}
