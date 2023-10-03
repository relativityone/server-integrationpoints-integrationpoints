using System;
using System.Linq;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Choice;
using static kCura.IntegrationPoints.Core.Constants;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class JobHistoryHelper : WorkspaceHelperBase
    {
        public JobHistoryHelper(WorkspaceFake workspace) : base(workspace)
        {
        }

        public JobHistoryFake CreateJobHistory(JobTest job, IntegrationPointFake integrationPoint)
        {
            JobHistoryFake jobHistory = new JobHistoryFake()
            {
                BatchInstance = job.JobDetailsHelper.BatchInstance.ToString(),
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors,
            };
            integrationPoint.JobHistory = integrationPoint.JobHistory.Concat(new[] { jobHistory.ArtifactId }).ToArray();
            Workspace.JobHistory.Add(jobHistory);
            return jobHistory;
        }

        public void CreateCustomJobHistory(
            IntegrationPointFake integrationPoint,
            string destinationName,
            DateTime endDate,
            ChoiceRef status,
            int itemsTransferred = 0,
            int totalItems = 0,
            string overwrite = OverwriteModeNames.AppendOnlyModeName)
        {
            JobHistoryFake jobHistory = new JobHistoryFake
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                ItemsTransferred = itemsTransferred,
                TotalItems = totalItems,
                EndTimeUTC = endDate,
                JobStatus = status,
                Overwrite = overwrite,
                DestinationWorkspace = destinationName,
                DestinationInstance = destinationName
            };
            integrationPoint.JobHistory = integrationPoint.JobHistory.Concat(new[] { jobHistory.ArtifactId }).ToArray();
            Workspace.JobHistory.Add(jobHistory);
        }
    }
}
