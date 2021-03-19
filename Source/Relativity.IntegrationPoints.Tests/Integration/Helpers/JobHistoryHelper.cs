using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class JobHistoryHelper : HelperBase
	{
		public JobHistoryHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock)
			: base(helperManager, database, proxyMock)
		{
		}

		public JobHistoryTest CreateJobHistory(JobTest job, IntegrationPointTest integrationPoint)
		{
			var jobHistory = new JobHistoryTest()
			{
				BatchInstance = job.JobDetailsHelper.BatchInstance.ToString(),
				IntegrationPoint = new[] { integrationPoint.ArtifactId },
				WorkspaceId = integrationPoint.WorkspaceId
			};
			Database.JobHistory.Add(jobHistory);
			return jobHistory;
		}
	}
}