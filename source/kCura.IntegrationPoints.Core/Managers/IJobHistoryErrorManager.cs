using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobHistoryErrorManager
	{
		IScratchTableRepository JobHistoryErrorJobStart { get; }

		IScratchTableRepository JobHistoryErrorJobComplete { get; }

		IScratchTableRepository JobHistoryErrorItemStart { get; }

		IScratchTableRepository JobHistoryErrorItemComplete { get; }


		/// <summary>
		/// Prepares the temp tables and determines the Update Status Type for updating errors at start and complete
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="jobType">Job Type of the currently running job</param>
		/// <param name="uniqueJobId">Job Id and Job Guid combined to be a suffix for the temp tables</param>
		/// <returns>An UpdateStatusType that houses the job type and error types to make error status changes with</returns>
		JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, Relativity.Client.Choice jobType);

		/// <summary>
		/// Creates a saved search to temporarily be used for retry error jobs.
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="originalSavedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(Job job, int originalSavedSearchArtifactId);
	}
}
