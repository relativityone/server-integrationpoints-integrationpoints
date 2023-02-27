using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    /// <summary>
    /// An interface for create, configure and start ImportAPI v2.0 jobs.
    /// </summary>
    internal interface IImportApiRunner
    {
        /// <summary>
        /// Creates, configurates and starts the ImportAPI v2.0 jobs.
        /// </summary>
        /// <param name="importJobContext">The job context containing the job-id and correlation-id.</param>
        /// <param name="destinationConfiguration">The object defining the destination configuration.</param>
        /// <param name="fieldMappings">List of fields mappings to transfer.</param>
        /// <exception cref="ImportApiResponseException"></exception>
        Task RunImportJobAsync(ImportJobContext importJobContext, ImportSettings destinationConfiguration, List<FieldMapWrapper> fieldMappings);
    }
}
