using System;
using kCura.IntegrationPoints.Domain.Synchronizer;

namespace kCura.IntegrationPoints.Domain
{
    /// <summary>
    /// Provides a method used to create a synchronizer in an external application domain.
    /// </summary>
    public interface ISynchronizerFactory
    {
        /// <summary>
        /// Creates a new synchronizer based on an identifier and the options. Used inside instance
        /// </summary>
        /// <param name="identifier">A GUID identifying the synchronizer.</param>
        /// <param name="options">The options specific to the current integration point identifier.</param>
        /// <returns>A new instance of the data synchronizer.</returns>
        IDataSynchronizer CreateSynchronizer(Guid identifier, string options);
    }
}
