using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobHistoryErrorManager
	{
		/// <summary>
		/// Scratch table repository for updating job-level Job History Errors at the start of a job
		/// </summary>
		IScratchTableRepository JobHistoryErrorJobStart { get; }

		/// <summary>
		/// Scratch table repository for updating job-level Job History Errors at the end of a job
		/// </summary>
		IScratchTableRepository JobHistoryErrorJobComplete { get; set; }

		/// <summary>
		/// Scratch table repository for updating item-level Job History Errors that are included in the retry at the start of a job
		/// </summary>
		IScratchTableRepository JobHistoryErrorItemStart { get; }

		/// <summary>
		/// Scratch table repository for updating item-level Job History Errors at the end of a job
		/// </summary>
		IScratchTableRepository JobHistoryErrorItemComplete { get; set; }

		/// <summary>
		/// Scratch table repository for updating item-level Job History Errors that are excluded from the retry at the start of a job
		/// </summary>
		IScratchTableRepository JobHistoryErrorItemStartExcluded { get; }

		/// <summary>
		/// Prepares the temp tables and determines the Update Status Type for updating errors at start and complete
		/// </summary>
		/// <param name="job">Job object representing the currently running job</param>
		/// <param name="jobType">Job Type of the currently running job</param>
		/// <returns>An UpdateStatusType that houses the job type and error types to make error status changes with</returns>
		JobHistoryErrorDTO.UpdateStatusType StageForUpdatingErrors(Job job, ChoiceRef jobType);

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
