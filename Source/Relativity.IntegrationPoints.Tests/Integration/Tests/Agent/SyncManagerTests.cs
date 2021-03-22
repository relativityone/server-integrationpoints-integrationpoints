using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
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
			
			IntegrationPointTest integrationPoint = HelperManager.IntegrationPointHelper.CreateIntegrationPointWithFakeProviders(SourceWorkspace);
			
			JobTest job = HelperManager.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
			HelperManager.JobHistoryHelper.CreateJobHistory(job, integrationPoint);
			return job;
		}

		private SyncManager PrepareSut(int numberOfRecords)
		{
			Container.Register(Component.For<IDataReader>().UsingFactoryMethod(c => new FakeDataReader(numberOfRecords)));
			SyncManager sut = Container.Resolve<SyncManager>();
			return sut;
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			const int numberOfRecords = 1500;
			const int expectedNumberOfSyncWorkersJobs = 2;
			JobTest job = PrepareJob();
			SyncManager sut = PrepareSut(numberOfRecords);

			// Act
			sut.Execute(new Job(job.AsDataRow()));

			// Assert
			List<JobTest> syncWorkerJobs = Database.JobsInQueue.Where(x => x.TaskType == "SyncWorker").ToList();
			syncWorkerJobs.Count.Should().Be(expectedNumberOfSyncWorkersJobs);
			AssertNumberOfRecords(syncWorkerJobs[0], 1000);
			AssertNumberOfRecords(syncWorkerJobs[1], 500);
		}

		private void AssertNumberOfRecords(JobTest job, int numberOfRecords)
		{
			JArray records = JArray.FromObject(JObject.Parse(job.JobDetails)["BatchParameters"]);
			records.Count.Should().Be(numberOfRecords);
		}
	}
}