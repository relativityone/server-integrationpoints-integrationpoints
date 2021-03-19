using kCura.IntegrationPoints.Agent.Tasks;
using kCura.ScheduleQueue.Core;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("9D111D3B-2E16-4652-82C9-6F8B7A8F2AD5")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncManagerTests : TestsBase
	{
		private SyncManager PrepareSut()
		{
			SyncManager sut = Container.Resolve<SyncManager>();

			return sut;
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(HelperManager.WorkspaceHelper.SourceWorkspace);
			JobTest job = HelperManager.JobHelper.ScheduleIntegrationPointRun(integrationPoint);
			JobHistoryTest jobHistory = HelperManager.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			SyncManager sut = PrepareSut();

			// Act
			sut.Execute(new Job(job.AsDataRow()));
		}
	}
}