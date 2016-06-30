using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobHistoryErrorManager
	{
		/// <summary>
		/// Scratch table repository for updating a job-level Job History Error
		/// </summary>
		IScratchTableRepository JobHistoryErrorJobError { get; }

		/// <summary>
		/// Scratch table repository for updating item-level Job History Errors that are included in the new job (only used for Retry jobs)
		/// </summary>
		IScratchTableRepository JobHistoryErrorItemErrorsIncluded { get; }

		/// <summary>
		/// Scratch table repository for updating item-level Job History Errors that are excluded the new job
		/// </summary>
		IScratchTableRepository JobHistoryErrorItemErrorsExcluded { get; }

		/// <summary>
		/// Prepares the temp tables and determines the Update Status Type for updating errors at start and complete
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="jobType">Job Type of the currently running job</param>
		/// <returns>An UpdateStatusType that houses the job type and error types to make error status changes with</returns>
		JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, Relativity.Client.Choice jobType);

		/// <summary>
		/// Creates a saved search to temporarily be used for retry error jobs.
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="originalSavedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(Job job, int originalSavedSearchArtifactId);

		/// <summary>
		/// Prepares the temp tables and determines the Update Status Type for updating errors at start and complete for Item Level Errors
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="savedSearchIdForItemLevelErrors">Saved search artifact id for item level errors.</param>
		void CreateErrorListTempTablesForItemLevelErrors(Job job, int savedSearchIdForItemLevelErrors);
	}
}
