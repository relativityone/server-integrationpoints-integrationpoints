using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using Relativity.Services.Choice;

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
        Data.JobHistory GetOrCreateScheduledRunHistoryRdo(IntegrationPointDto integrationPointDto, Guid batchInstance, DateTime? startTimeUtc);

        /// <summary>
        /// Creates a job history object for an integration point if the given batch instance guid does not already exist;
        /// otherwise, we retrieve the existing job history object.
        /// </summary>
        /// <param name="integrationPoinDtot">The integration point object.</param>
        /// <param name="batchInstance">The batch instance guid.</param>
        /// <param name="jobType">The job type.</param>
        /// <param name="startTimeUtc">The job start time in UTC zone.</param>
        /// <returns>The job history object of the integration point.</returns>
        Data.JobHistory CreateRdo(IntegrationPointDto integrationPointDto, Guid batchInstance, ChoiceRef jobType, DateTime? startTimeUtc);
        
        /// <summary>
        /// Retrieves job history Rdo without Documents field
        /// </summary>
        /// <param name="batchInstance">Batch instance Guid</param>
        /// <returns></returns>
        Data.JobHistory GetRdoWithoutDocuments(Guid batchInstance);

        /// <summary>
        /// Retrieves job history Rdo without Documents field
        /// </summary>
        /// <param name="artifactId">Artifact ID of the Job History object</param>
        /// <returns></returns>
        Data.JobHistory GetRdoWithoutDocuments(int artifactId);

        /// <summary>
        /// Updates job history Rdo without Documents field
        /// </summary>
        /// <param name="jobHistory">Job history Rdo</param>
        void UpdateRdoWithoutDocuments(Data.JobHistory jobHistory);

        /// <summary>
        /// Deletes an instance of job history given its Artifact ID.
        /// </summary>
        /// <param name="jobHistoryId">The Artifact ID of the job history instance to delete.</param>
        void DeleteRdo(int jobHistoryId);

        IList<Data.JobHistory> GetAll();
    }
}
