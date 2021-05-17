using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("9D111D3B-2E16-4652-82C9-6F8B7A8F2AD5")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncManagerTests : TestsBase
	{
		private JobTest PrepareJob(string xmlPath)
		{
			FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			SourceProviderTest provider =
				SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider, "Name", xmlPath);

			integrationPoint.SourceProvider = provider.ArtifactId;

			integrationPoint.SourceConfiguration = xmlPath;

			JobTest job =
				FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
			SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);
			return job;
		}

		private SyncManager PrepareSut()
		{
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>().IsDefault());
			SyncManager sut = Container.Resolve<SyncManager>();
			return sut;
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			const int numberOfRecords = 1500;
			const int expectedNumberOfSyncWorkersJobs = 2;
			string xmlPath = PrepareRecords(numberOfRecords); 
			JobTest job = PrepareJob(xmlPath);
			SyncManager sut = PrepareSut();

			// Act
			sut.Execute(new Job(job.AsDataRow()));

			// Assert
			List<JobTest> syncWorkerJobs = FakeRelativityInstance.JobsInQueue.Where(x => x.TaskType == "SyncWorker").ToList();
			syncWorkerJobs.Count.Should().Be(expectedNumberOfSyncWorkersJobs);
			AssertNumberOfRecords(syncWorkerJobs[0], 1000);
			AssertNumberOfRecords(syncWorkerJobs[1], 500);
		}

		private string PrepareRecords(int numberOfRecords)
		{
			string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
			string tmpPath = Path.GetTempFileName();
			File.WriteAllText(tmpPath, xml);
			return tmpPath;
		}

		private void AssertNumberOfRecords(JobTest job, int numberOfRecords)
		{
			JArray records = JArray.FromObject(JObject.Parse(job.JobDetails)["BatchParameters"]);
			records.Count.Should().Be(numberOfRecords);
		}
	}
}