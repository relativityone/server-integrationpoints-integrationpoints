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
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json.Linq;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using static Relativity.IntegrationPoints.Tests.Integration.Const;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("9D111D3B-2E16-4652-82C9-6F8B7A8F2AD5")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncManagerTests : TestsBase
	{
		private JobTest PrepareJob(string xmlPath, SourceProviderTest provider, out JobHistoryTest jobHistory)
		{
			FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider, identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

			integrationPoint.SourceProvider = provider.ArtifactId;
			integrationPoint.SourceConfiguration = xmlPath;

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
			jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);
			return job;
		}
		
		private SyncManager PrepareSut()
		{
			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>().Named(Provider._MY_FIRST_PROVIDER));
			SyncManager sut = Container.Resolve<SyncManager>();
			return sut;
		}

		[IdentifiedTest("F0F133E0-0101-4E21-93C5-A6365FD720B3")]
		public void Execute_GetUnbatchedIDs_ShouldAbortThread_WhenDrainStopTimeoutExceeded()
		{
			// Arrange
			Guid customProviderId = Guid.NewGuid();

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateCustomProvider(nameof(CustomProvider), customProviderId);

			Context.InstanceSettings.DrainStopTimeout = TimeSpan.FromSeconds(1);
			const int numberOfRecords = 1500;
			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, provider, out JobHistoryTest jobHistory);

			Container.Register(Component.For<IDataSourceProvider>().ImplementedBy<CustomProvider>().Named(customProviderId.ToString()));
			SyncManager sut = Container.Resolve<SyncManager>();
			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			// Act
			Thread thread = new Thread(() => sut.Execute(job.AsJob()));
			thread.Start();
			agent.ToBeRemoved = true;
			Thread.Sleep(TimeSpan.FromSeconds(1));
			thread.Join();
			
			// Assert
			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
		}

		[IdentifiedTest("D7134532-7560-4F63-B695-384FCD464F11")]
		public void SyncManager_ShouldSplitJobIntoBatches()
		{
			// Arrange
			const int numberOfRecords = 1500;
			const int expectedNumberOfSyncWorkersJobs = 2;
			string xmlPath = PrepareRecords(numberOfRecords);

			SourceProviderTest provider = SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			JobTest job = PrepareJob(xmlPath, provider, out JobHistoryTest jobHistory);
			SyncManager sut = PrepareSut();

			// Act
			sut.Execute(job.AsJob());

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

		private class CustomProvider : IDataSourceProvider
		{
			public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
			{
				throw new NotImplementedException();
			}

			public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
			{
				throw new NotImplementedException();
			}

			public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));
				return null;
			}

		}
	}
}