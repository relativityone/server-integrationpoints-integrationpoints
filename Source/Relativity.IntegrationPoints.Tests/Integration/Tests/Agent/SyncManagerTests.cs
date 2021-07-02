using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using static Relativity.IntegrationPoints.Tests.Integration.Const;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncManagerTests : TestsBase
	{
		[IdentifiedTest("F0F133E0-0101-4E21-93C5-A6365FD720B3")]
		public void Execute_ShouldAbortGetUnbatchedIDs_WhenDrainStopTimeoutExceeded()
		{
			// Arrange
			Guid customProviderId = Guid.NewGuid();

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateCustomProvider(nameof(FakeCustomProvider), customProviderId);

			FakeCustomProvider customProviderImpl = new FakeCustomProvider()
			{
				GetBatchableIdsFunc = () =>
				{
					Thread.Sleep(TimeSpan.FromSeconds(10));
					return null;
				}
			};

			Context.InstanceSettings.DrainStopTimeout = TimeSpan.FromSeconds(1);

			JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory);

			SyncManager sut = PrepareSutWithCustomProvider(customProviderImpl, customProviderId);

			// Act
			RunActionWithDrainStop(() => sut.Execute(job.AsJob()));

			// Assert
			VerifyJobHistoryStatus(jobHistory, JobStatusChoices.JobHistorySuspendedGuid);
		}

		[IdentifiedTest("73B8F442-F0DE-437A-BF8A-493FF0FCC5AB")]
		public void Execute_ShouldComplete_WhenGetBatchableIdsFitsInTime()
		{
			// Arrange
			const int numberOfRecords = 1500;
			string xmlPath = PrepareRecords(numberOfRecords);

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);
			SyncManager sut = PrepareSut();

			// Act
			RunActionWithDrainStop(() => sut.Execute(job.AsJob()));

			// Assert
			VerifyCreatedSyncWorkerJobs(new int[] { 1000, 500 });
		}

		[IdentifiedTest("09663B11-23D1-4114-8F4D-097DE47098BB")]
		public void Execute_ShouldFail_WhenGetBatchableIdsThrowException()
		{
			// Arrange
			Guid customProviderId = Guid.NewGuid();

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateCustomProvider(nameof(FakeCustomProvider), customProviderId);

			FakeCustomProvider customProviderImpl = new FakeCustomProvider()
			{
				GetBatchableIdsFunc = () =>
				{
					throw new InvalidOperationException();
				}
			};

			JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory);

			SyncManager sut = PrepareSutWithCustomProvider(customProviderImpl, customProviderId);

			// Act
			sut.Execute(job.AsJob());

			// Assert
			VerifyJobHistoryStatus(jobHistory, JobStatusChoices.JobHistoryErrorJobFailedGuid);
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			const int numberOfRecords = 1500;
			string xmlPath = PrepareRecords(numberOfRecords);

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			JobTest job = PrepareJob(provider, out JobHistoryTest jobHistory, xmlPath);

			SyncManager sut = PrepareSut();

			// Act
			sut.Execute(job.AsJob());

			// Assert
			VerifyCreatedSyncWorkerJobs(new int[] { 1000, 500 });
		}

		private JobTest PrepareJob(SourceProviderTest provider, out JobHistoryTest jobHistory, string xmlPath = null)
		{
			FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider, identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

			integrationPoint.SourceProvider = provider.ArtifactId;
			integrationPoint.SourceConfiguration = xmlPath;

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
			jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			RegisterJobContext(job);

			return job;
		}

		private SyncManager PrepareSutWithCustomProvider(FakeCustomProvider providerImpl, Guid providerId)
		{
			Container.Register(Component.For<IDataSourceProvider>().UsingFactoryMethod(() => providerImpl)
				.Named(providerId.ToString()));

			return PrepareSut();
		}

		private SyncManager PrepareSut()
		{
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>().Named(Provider._MY_FIRST_PROVIDER));
			SyncManager sut = Container.Resolve<SyncManager>();
			return sut;
		}

		private void RunActionWithDrainStop(Action action)
		{
			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			Thread thread = new Thread(() => action());
			thread.Start();
			agent.ToBeRemoved = true;
			Thread.Sleep(TimeSpan.FromSeconds(1));
			thread.Join();
		}

		private string PrepareRecords(int numberOfRecords)
		{
			string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
			string tmpPath = Path.GetTempFileName();
			File.WriteAllText(tmpPath, xml);
			return tmpPath;
		}

		private void VerifyJobHistoryStatus(JobHistoryTest actual, Guid expectedStatus)
		{
			actual.JobStatus.Guids.Single().Should().Be(expectedStatus);
		}

		private void VerifyCreatedSyncWorkerJobs(int[] documentsInSyncWorkerJobs)
		{
			List<JobTest> syncWorkerJobs = FakeRelativityInstance.JobsInQueue.Where(
				x => x.TaskType == TaskType.SyncWorker.ToString()).ToList();
			syncWorkerJobs.Count.Should().Be(documentsInSyncWorkerJobs.Length);
			for (int i = 0; i < documentsInSyncWorkerJobs.Length; ++i)
			{
				AssertNumberOfRecords(syncWorkerJobs[i], documentsInSyncWorkerJobs[i]);
			}
		}

		private void AssertNumberOfRecords(JobTest job, int numberOfRecords)
		{
			JArray records = JArray.FromObject(JObject.Parse(job.JobDetails)["BatchParameters"]);
			records.Count.Should().Be(numberOfRecords);
		}
	}
}