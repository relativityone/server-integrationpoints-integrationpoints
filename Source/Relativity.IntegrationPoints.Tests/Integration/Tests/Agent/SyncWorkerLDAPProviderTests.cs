using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.IntegrationPoints.Tests.Integration.Tests.LDAP.TestData;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using kCura.ScheduleQueue.Core.Core;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("55CAA9A3-B9CE-4A69-9CC9-ED931EE9EB81")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncWorkerLDAPProviderTests : TestsBase
	{
		private readonly ManagementTestData _managementTestData = new ManagementTestData();

		[IdentifiedTest("46988B61-878E-4F9F-95BA-3775E13F492E")]
		public void SyncWorker_ShouldImportLDAPData()
		{
			// Arrange
			ScheduleImportEntityFromLdapJob(false);

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) => { importJob.Complete(_managementTestData.Data.Count); })).LifestyleSingleton());

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

			// Act
			sut.Execute();

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedGuid);

			FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
		}

		[IdentifiedTest("F83AF76A-50C5-4F4E-A097-74C5FB57350A")]
		public void SyncWorker_ShouldImportLDAPDataAndSubmitLinkManagersJob_WhenLinkManagersWasEnabled()
		{
			// Arrange
			ScheduleImportEntityFromLdapJob(true);

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(ImportEntity))
				.LifestyleSingleton());

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, true);

			// Act
			sut.Execute();

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryProcessingGuid);

			JobTest linkManagersJob = FakeRelativityInstance.JobsInQueue.Single();
			linkManagersJob.TaskType.Should().Be(TaskType.SyncEntityManagerWorker.ToString());
		}

		[IdentifiedTest("3BDAF07F-FC93-4A74-B60B-A47E404FA85D")]
		public void SyncWorker_ShouldImportLDAPDataAndReSubmitLinkManagersJob_WhenDrainStoppedOnManagerLinks()
		{
			// Arrange
			const int processedItemsBeforeDrainStopped = 3;

			ScheduleImportEntityFromLdapJob(true);

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, false);

			Queue<FakeJobImport> importJobsQueue = new Queue<FakeJobImport>();
			importJobsQueue.Enqueue(new FakeJobImport(ImportEntity));
			importJobsQueue.Enqueue(new FakeJobImport(importJob =>
			{
				importJob.Complete(processedItemsBeforeDrainStopped);
				sut.MarkAgentToBeRemoved();
			}));

			Container.Register(Component.For<IJobImport>().UsingFactoryMethod(k => importJobsQueue.Dequeue()).LifestyleTransient());

			// Act
			sut.Execute();

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistorySuspendedGuid);

			FakeRelativityInstance.JobsInQueue.Single()
				.DeserializeDetails<EntityManagerJobParameters>()
				.EntityManagerMap.Should()
					.HaveCount(_managementTestData.Data.Count - processedItemsBeforeDrainStopped);

			FakeRelativityInstance.EntityManagersResourceTables.Single()
				.Value.Should().OnlyContain(x => x.LockedByJobId == -1);
		}

		[IdentifiedTest("431049DF-B069-4570-AA5A-89FE4F329121")]
		public void SyncWorker_ShouldImportLDAPDataAndLinkManagersJob_WhenAllManagerLinksWereProcessed()
		{
			// Arrange
			ScheduleImportEntityFromLdapJob(true);

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container, false);

			Queue<FakeJobImport> importJobsQueue = new Queue<FakeJobImport>();
			importJobsQueue.Enqueue(new FakeJobImport(ImportEntity));
			importJobsQueue.Enqueue(new FakeJobImport(importJob =>
			{
				importJob.Complete(_managementTestData.Data.Count);
				sut.MarkAgentToBeRemoved();
			}));

			Container.Register(Component.For<IJobImport>().UsingFactoryMethod(k => importJobsQueue.Dequeue()).LifestyleTransient());

			// Act
			sut.Execute();

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedGuid);

			FakeRelativityInstance.JobsInQueue.Should().BeEmpty();
		}

		[IdentifiedTest("81359E78-08A3-4BF2-B0A1-7F8FD62DDFB9")]
		public void SyncWorker_ShouldImportLDAPDataAndLinkManagers_WhenJobWasDrainStoppedAndResumed()
		{
			// Arrange
			const int processedItemsBeforeDrainStopped = 1;
			int totalItems = _managementTestData.Data.Count;
			int processedItemsAfterResume = totalItems - processedItemsBeforeDrainStopped;

			ScheduleImportEntityFromLdapJob(true);

			FakeAgent drainStoppedAgent = FakeAgent.Create(FakeRelativityInstance, Container, false);

			Queue<FakeJobImport> importJobsQueue = new Queue<FakeJobImport>();
			
			// SyncWorker for Entities
			importJobsQueue.Enqueue(new FakeJobImport(ImportEntity));
			
			// SyncEntityManagerWorker (linking managers)
			importJobsQueue.Enqueue(new FakeJobImport(importJob =>
			{
				importJob.Complete(processedItemsBeforeDrainStopped);
				drainStoppedAgent.MarkAgentToBeRemoved();
			}));

			// resume SyncEntityManagerWorker (linking managers)
			FakeJobImport resumedImportJob = new FakeJobImport(importJob => importJob.Complete(processedItemsAfterResume, useDataReader: false));
			importJobsQueue.Enqueue(resumedImportJob);

			Container.Register(Component.For<IJobImport>().UsingFactoryMethod(k => importJobsQueue.Dequeue()).LifestyleTransient());

			// (1) Act & Assert
			drainStoppedAgent.Execute();

			VerifyJobHistoryStatus(JobStatusChoices.JobHistorySuspendedGuid);

			FakeRelativityInstance.JobsInQueue.Single()
				.DeserializeDetails<EntityManagerJobParameters>()
				.EntityManagerMap.Should()
					.HaveCount(processedItemsAfterResume);

			// (2) Act & Assert
			FakeAgent resumedAgent = FakeAgent.Create(FakeRelativityInstance, Container, false);

			resumedAgent.Execute();

			VerifyJobHistoryStatus(JobStatusChoices.JobHistoryCompletedGuid);

			FakeRelativityInstance.JobsInQueue.Should().BeEmpty();

			VerifyFollowingRecordsWereProcessed(resumedImportJob, _managementTestData.EntryIds.Skip(processedItemsBeforeDrainStopped));
		}

		[IdentifiedTest("C0D7BD33-F3B3-4B32-8F12-52E08F361875")]
		public void SyncWorker_ShouldSubmitLinkManagerJob_OnlyForTransferredEntities()
		{
			// Arrange
			JobTest syncWorkerJob = ScheduleImportEntityFromLdapJob(true);
			const int drainStopAfter = 2;

			FakeAgent sut = FakeAgent.Create(FakeRelativityInstance, Container);

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport((importJob) =>
			{
				importJob.Complete(drainStopAfter);
				sut.MarkAgentToBeRemoved();
			})).LifestyleSingleton());

			// Act
			sut.Execute();

			// Assert
			VerifyJobHistoryStatus(JobStatusChoices.JobHistorySuspendedGuid);

			FakeRelativityInstance.JobsInQueue.Count.Should().Be(2);
			syncWorkerJob.StopState.Should().Be(StopState.DrainStopped);
			syncWorkerJob.DeserializeDetails<string[]>().Should().BeEquivalentTo(_managementTestData.EntryIds.Skip(drainStopAfter));

			JobTest linkManagersJob = FakeRelativityInstance.JobsInQueue.Last();
			linkManagersJob.ParentJobId.Should().Be(syncWorkerJob.JobId);
			linkManagersJob.DeserializeDetails<EntityManagerJobParameters>().EntityManagerMap.Should()
				.HaveCount(drainStopAfter).And.ContainKeys(_managementTestData.EntryIds.Take(drainStopAfter));
		}

		private JobTest ScheduleImportEntityFromLdapJob(bool linkEntityManagers)
		{
			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportEntityFromLdapIntegrationPoint(linkEntityManagers);

			Helper.SecretStore.Setup(SourceWorkspace, integrationPoint);

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleSyncWorkerJob(SourceWorkspace, integrationPoint, _managementTestData.EntryIds);

			JobHistoryTest jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			InsertBatchToJobTrackerTable(job, jobHistory);

			return job;
		}

		private void InsertBatchToJobTrackerTable(JobTest job, JobHistoryTest jobHistory)
		{
			string tableName = string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, jobHistory.BatchInstance);


			if (!FakeRelativityInstance.JobTrackerResourceTables.ContainsKey(tableName))
			{
				FakeRelativityInstance.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
			}

			FakeRelativityInstance.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = job.JobId });
		}

		private void VerifyJobHistoryStatus(Guid expectedStatusGuid)
		{
			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();
			jobHistory.JobStatus.Guids.Single().Should().Be(expectedStatusGuid);
		}

		private void VerifyFollowingRecordsWereProcessed(FakeJobImport importJob, IEnumerable<string> expectedProcessedRecords)
		{
			List<string> actualProcessedRecords = new List<string>();

			var reader = importJob.Context.DataReader;
			while (reader.Read())
			{
				actualProcessedRecords.Add(reader.GetString(0));
			}

			actualProcessedRecords.Should().BeEquivalentTo(expectedProcessedRecords);
		}

		private void ImportEntity(FakeJobImport importJob)
		{
			var managementTestData = new ManagementTestData();

			foreach(var data in managementTestData.Data)
			{
				SourceWorkspace.Entities.Add(new EntityTest
				{
					UniqueId = data["uid"].ToString(),
					FirstName = data["givenname"].ToString(),
					LastName = data["sn"].ToString(),
					Manager = data["manager"].ToString()
				});
			}

			importJob.Complete(managementTestData.Data.Count);
		}
	}
}