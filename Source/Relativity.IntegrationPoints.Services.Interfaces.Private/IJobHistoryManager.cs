using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Enables access to information regarding integration points
    /// </summary>
    [WebService("Job History Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IJobHistoryManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Gets the job history information for the requested job.
        /// </summary>
        /// <param name="request">The job history request information.</param>
        /// <returns>The job history summary.</returns>
        Task<JobHistorySummaryModel> GetJobHistoryAsync(JobHistoryRequest request);
    }
}
