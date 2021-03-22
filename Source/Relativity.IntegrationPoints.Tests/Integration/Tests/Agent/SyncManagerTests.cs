using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
		private JobTest PrepareJob()
		{
			HelperManager.AgentHelper.CreateIntegrationPointAgent();

			WorkspaceTest sourceWorkspace = HelperManager.WorkspaceHelper.SourceWorkspace;
			SourceProviderTest sourceProvider = HelperManager.SourceProviderHelper.CreateSourceProvider(sourceWorkspace);
			DestinationProviderTest destinationProviderTest = HelperManager.DestinationProviderHelper.CreateDestinationProvider(sourceWorkspace);
			IntegrationPointTypeTest integrationPointType = HelperManager.IntegrationPointTypeHelper.CreateIntegrationPointType(sourceWorkspace);

			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateEmptyIntegrationPoint(sourceWorkspace);
			integrationPoint.Type = integrationPointType.ArtifactId;
			integrationPoint.SourceProvider = sourceProvider.ArtifactId;
			integrationPoint.DestinationProvider = destinationProviderTest.ArtifactId;
			
			JobTest job = HelperManager.JobHelper.ScheduleIntegrationPointRun(integrationPoint);
			HelperManager.JobHistoryHelper.CreateJobHistory(job, integrationPoint);
			return job;
		}

		private SyncManager PrepareSut()
		{
			SyncManager sut = Container.Resolve<SyncManager>();
			return sut;
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			JobTest job = PrepareJob();
			SyncManager sut = PrepareSut();

			// Act
			sut.Execute(new Job(job.AsDataRow()));

			// Assert
			List<JobTest> batchTasks = Database.JobsInQueue.Where(x => x.TaskType == "SyncWorker").ToList();
			batchTasks.Count.Should().Be(2);
		}
	}
}