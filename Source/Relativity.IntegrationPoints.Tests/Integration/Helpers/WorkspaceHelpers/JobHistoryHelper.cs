using Relativity.IntegrationPoints.Tests.Integration.Models;

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
				IntegrationPoint = new[] { integrationPoint.ArtifactId },
			};
			Workspace.JobHistory.Add(jobHistory);
			return jobHistory;
		}
	}
}