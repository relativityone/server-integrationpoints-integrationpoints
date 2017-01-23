using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IJobHistoryService
	{
		/// <summary>
		/// Creates a job history object for an integration point of a schedule run type if the given batch instance guid does not already exist;
		/// otherwise, we retrieve the existing job history object.
		/// </summary>
		/// <param name="integrationPoint">The integration point object.</param>
		/// <param name="batchInstance">The batch instance guid.</param>
		/// <param name="startTimeUtc">The job start time in UTC format.</param>
		/// <returns>The job history object of the integration point.</returns>
		Data.JobHistory GetOrCreateScheduledRunHistoryRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc);

		/// <summary>
		/// Creates a job history object for an integration point if the given batch instance guid does not already exist;
		/// otherwise, we retrieve the existing job history object.
		/// </summary>
		/// <param name="integrationPoint">The integration point object.</param>
		/// <param name="batchInstance">The batch instance guid.</param>
		/// <param name="jobType">The job type.</param>
		/// <param name="startTimeUtc">The job start time in UTC zone.</param>
		/// <returns>The job history object of the integration point.</returns>
		Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, Relativity.Client.DTOs.Choice jobType, DateTime? startTimeUtc);

		/// <summary>
		/// Retrieves a list of job history objects given the job history artifact ids.
		/// </summary>
		/// <param name="jobHistoryArtifactIds">A list of job history artifact ids.</param>
		/// <returns>A list of job history objects.</returns>
		IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds);

		/// <summary>
		/// Retrieves job history Rdo
		/// </summary>
		/// <param name="batchInstance">Batch instance Guid</param>
		/// <returns></returns>
		Data.JobHistory GetRdo(Guid batchInstance);

		/// <summary>
		/// Updates job history Rdo
		/// </summary>
		/// <param name="jobHistory">Job history Rdo</param>
		void UpdateRdo(Data.JobHistory jobHistory);

		/// <summary>
		/// Deletes an instance of job history given its Artifact ID.
		/// </summary>
		/// <param name="jobHistoryId">The Artifact ID of the job history instance to delete.</param>
		void DeleteRdo(int jobHistoryId);
	}
}