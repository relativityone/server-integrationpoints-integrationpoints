﻿using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IJobHistoryService
	{
		/// <summary>
		/// Creates a job history object for an integration point if the given batch instance guid does not already exist;
		/// otherwise, we retrieve the existing job history object.
		/// </summary>
		/// <param name="integrationPoint">The integration point object.</param>
		/// <param name="batchInstance">The batch instance guid.</param>
		/// <param name="startTimeUtc">The job start time in UTC format.</param>
		/// <returns>The job history object of the integration point.</returns>
		Data.JobHistory CreateRdo(Data.IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc);

		/// <summary>
		/// Retrieves a list of job history objects given the job history artifact ids.
		/// </summary>
		/// <param name="jobHistoryArtifactIds">A list of job history artifact ids.</param>
		/// <returns>A list of job history objects.</returns>
		IList<Data.JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds);
	}
}