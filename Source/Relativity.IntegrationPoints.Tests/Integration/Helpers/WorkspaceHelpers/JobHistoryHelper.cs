using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class JobHistoryHelper : WorkspaceHelperBase
    {
        public JobHistoryHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public JobHistoryTest CreateJobHistory(JobTest job, IntegrationPointTest integrationPoint)
        {
            var jobHistory = new JobHistoryTest()
            {
                BatchInstance = job.JobDetailsHelper.BatchInstance.ToString(),
                IntegrationPoint = new[] {integrationPoint.ArtifactId},
                JobStatus = new ChoiceRef(new List<Guid> {JobStatusChoices.JobHistoryPendingGuid})
            };
            integrationPoint.JobHistory = integrationPoint.JobHistory.Concat(new[] {jobHistory.ArtifactId}).ToArray();
            Workspace.JobHistory.Add(jobHistory);
            return jobHistory;
        }
    }
}