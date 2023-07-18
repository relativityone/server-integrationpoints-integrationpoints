using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;

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
        /// <param name="integrationPoint">integration Point object.</param>
        /// <exception cref="ImportApiResponseException"></exception>
        Task RunImportJobAsync(ImportJobContext importJobContext, IntegrationPointInfo integrationPoint);
    }
}
