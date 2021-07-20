using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Services.Choice;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("BA0C4AD6-6236-4235-BEBB-CB1084A978E9")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class SyncWorkerMyFirstProviderTests : TestsBase
	{
		private SyncWorker PrepareSut(Action<FakeJobImport> importAction)
		{
			Container.Register(Component.For<IDataSourceProvider>()
				.ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>()
				.Named(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID));

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction))
				.LifestyleSingleton());

			SyncWorker sut = Container.Resolve<SyncWorker>();
			return sut;
		}

		private JobTest PrepareJob(string xmlPath, out JobHistoryTest jobHistory)
		{
			AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			SourceProviderTest provider =
				SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
					identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

			integrationPoint.SourceProvider = provider.ArtifactId;
			integrationPoint.SourceConfiguration = xmlPath;

			JobTest job =
				FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
			jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value).ToArray();

			taskParameters.BatchParameters = recordsIds;

			job.JobDetails = Serializer.Serialize(taskParameters);
			job.LockedByAgentID = agent.ArtifactId;
			job.RootJobId = JobId.Next;

			InsertBatchToJobTrackerTable(job, jobHistory);

			RegisterJobContext(job);

			return job;
		}

		private void InsertBatchToJobTrackerTable(JobTest job, JobHistoryTest jobHistory)
		{
			string tableName = string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, jobHistory.BatchInstance);

			
			if(!FakeRelativityInstance.JobTrackerResourceTables.ContainsKey(tableName))
			{
				FakeRelativityInstance.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
			}

			FakeRelativityInstance.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = job.JobId });
		}

		/// <summary>
		/// Creates <see cref="numberOfBatches"/> jobs in queue for the same IntegrationPoint and returns one of them
		/// </summary>
		private JobTest PrepareJobs(string xmlPath, int numberOfBatches)
		{
			FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

			SourceProviderTest provider =
				SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

			IntegrationPointTest integrationPoint =
				SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
					identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

			integrationPoint.SourceProvider = provider.ArtifactId;
			integrationPoint.SourceConfiguration = xmlPath;
			JobTest job =
				FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace,
					integrationPoint);

			SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value)
				.ToArray();

			taskParameters.BatchParameters = recordsIds;

			job.JobDetails = Serializer.Serialize(taskParameters);

			for (int i = 1; i < numberOfBatches; i++)
			{
				JobTest newJob =
					FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace,
						integrationPoint);

				newJob.JobDetails = job.JobDetails; // link all jobs together with BatchInstance
			}

			RegisterJobContext(job);

			return job;
		}

		private string PrepareRecords(int numberOfRecords)
		{
			string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
			string tmpPath = Path.GetTempFileName();
			File.WriteAllText(tmpPath, xml);
			return tmpPath;
		}

		[IdentifiedTest("BCF72894-224F-4DB7-985F-0C53C93D153D")]
		public void SyncWorker_ShouldImportData()
		{
			// Arrange
			const int numberOfRecords = 1000;
			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
			SyncWorker sut = PrepareSut((importJob) => { importJob.Complete(); });

			jobHistory.TotalItems = 2000;

			// Act
			sut.Execute(job.AsJob());

			// Assert
			jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
		}

		[IdentifiedTest("A1350299-3F8E-4215-9773-82EB6185079C")]
		public void SyncWorker_ShouldDrainStop_WhenStopRequestedBeforeIAPI()
		{
			// Arrange
			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();
			agent.ToBeRemoved = true;

			const int numberOfRecords = 1000;
			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
			SyncWorker sut = PrepareSut((importJob) => { throw new Exception("IAPI should not be run"); });

			jobHistory.TotalItems = 2000;

			// Act
			sut.Execute(job.AsJob());

			// Assert
			jobHistory.ItemsTransferred.Should().Be(null);
			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.DrainStopped);
		}

		[IdentifiedTest("BCF72894-224F-4DB7-985F-0C53C93D153D")]
		public void SyncWorker_ShouldImportData_NotFullBatch()
		{
			// Arrange
			const int numberOfRecords = 420;
			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
			SyncWorker sut = PrepareSut((importJob) => { importJob.Complete(); });

			jobHistory.TotalItems = 2000;

			// Act
			sut.Execute(job.AsJob());

			// Assert
			jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
		}

		[IdentifiedTest("72118579-91DB-4018-8EF9-A4EB3FC2CD51")]
		public void SyncWorker_ShouldDrainStop()
		{
			// Arrange
			const int numberOfRecords = 100;
			const int drainStopAfterImporting = 50;

			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);


			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			SyncWorker sut = PrepareSut((importJob) =>
			{
				importJob.Complete(maxTransferredItems: drainStopAfterImporting);

				agent.ToBeRemoved = true;
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			List<string> remainingItems = GetRemainingItems(job);

			remainingItems.Count.Should().Be(numberOfRecords - drainStopAfterImporting);
			remainingItems.Should().BeEquivalentTo(Enumerable
				.Range(drainStopAfterImporting, numberOfRecords - drainStopAfterImporting).Select(x => x.ToString()));

			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
			jobHistory.ItemsTransferred.Should().Be(drainStopAfterImporting);
			job.StopState.Should().Be(StopState.DrainStopped);
		}

		[IdentifiedTest("72118579-91DB-4018-8EF9-A4EB3FC2CD51")]
		public void SyncWorker_ShouldNotDrainStop_WhenAllItemsInBatchWereProcessedWithItemLevelErrors()
		{
			// Arrange
			const int numberOfRecords = 100;
			const int numberOfErrors = 100;

			SetupWorkspaceDbContextMock_AsNotLastBatch();

			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
			jobHistory.TotalItems = 1000;

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			SyncWorker sut = PrepareSut((importJob) =>
			{
				importJob.Complete(numberOfItemLevelErrors: numberOfErrors);

				agent.ToBeRemoved = true;
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);
			FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
		}

		[IdentifiedTest("4D867717-3C3D-4763-9E29-63AAAA435885")]
		public void SyncWorker_ShouldNotDrainStopOtherBatches()
		{
			// Arrange
			const int numberOfRecords = 100;
			const int drainStopAfterImporting = 50;
			const int numberOfBatches = 3;

			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJobs(xmlPath, numberOfBatches);
			FakeRelativityInstance.Helpers.JobHelper.ScheduleBasicJob(SourceWorkspace);

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			SyncWorker sut = PrepareSut((importJob) =>
			{
				importJob.Complete(maxTransferredItems: drainStopAfterImporting);

				agent.ToBeRemoved = true;
			});

			// Act
			var syncManagerJob = job.AsJob();
			sut.Execute(syncManagerJob);

			// Assert
			job.StopState.Should().Be(StopState.DrainStopped);
			FakeRelativityInstance.JobsInQueue.Where(x => x.JobId != job.JobId).All(x => x.StopState == StopState.None)
				.Should().BeTrue();
		}

		[IdentifiedTest("6D4ED0EA-DDAA-442D-AEED-0C7C805A3FB4")]
		public void SyncWorker_ShouldResumeDrainStoppedJob()
		{
			// Arrange
			const int numberOfRecords = 100;
			const int numberOfErrors = 10;
			const int initialTransferredItems = 50;
			const int initialErroredItems = 50;

			FakeJobStatisticsQuery statisticsQuery = Container.Resolve<IJobStatisticsQuery>() as FakeJobStatisticsQuery;
			statisticsQuery.AlreadyFailedItems = initialErroredItems;
			statisticsQuery.AlreadyTransferredItems = initialTransferredItems;

			string xmlPath = PrepareRecords(numberOfRecords);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);

			jobHistory.JobStatus = new ChoiceRef(new List<Guid> { JobStatusChoices.JobHistorySuspendedGuid });
			jobHistory.ItemsTransferred = initialTransferredItems;
			jobHistory.ItemsWithErrors = initialErroredItems;

			SyncWorker sut = PrepareSut((importJob) => { importJob.Complete(maxTransferredItems: numberOfRecords + numberOfErrors, numberOfItemLevelErrors: numberOfErrors); });

			// Act
			var syncManagerJob = job.AsJob();
			sut.Execute(syncManagerJob);

			// Assert
			FakeRelativityInstance.JobsInQueue.Single(x => x.JobId == job.JobId).StopState.Should().Be(StopState.None);
			jobHistory.JobStatus.Guids.First().Should().Be(JobStatusChoices.JobHistoryCompletedWithErrorsGuid);
			jobHistory.ItemsTransferred.Should().Be(initialTransferredItems + numberOfRecords);
			jobHistory.ItemsWithErrors.Should().Be(initialErroredItems + numberOfErrors);

			jobHistory.ShouldHaveCorrectItemsTransferredUpdateHistory(initialTransferredItems, initialTransferredItems + numberOfRecords);
			jobHistory.ShouldHaveCorrectItemsWithErrorsUpdateHistory(initialErroredItems, initialErroredItems + numberOfErrors);
		}

		public static IEnumerable<TestCaseData> FinalJobHistoryStatusTestCases()
		{
			yield return new TestCaseData(
				new[] { new JobTest { StopState = StopState.None, LockedByAgentID = 1 }, new JobTest { StopState = StopState.DrainStopped } },
					TOTAL_NUMBER_OF_RECORDS,
					0,
					false,
					JobStatusChoices.JobHistoryProcessingGuid,
					StopState.None
				)
			{
				TestName = "If there are other batches processing, JobHistory should end up in Processing"
			}.WithId("7591EA8C-CF54-482C-B096-C9C4437D3F11");

			yield return new TestCaseData(
				new[] { new JobTest { StopState = StopState.None, LockedByAgentID = 1 }, new JobTest { StopState = StopState.DrainStopped } },
				50,
				0,
				true,
				JobStatusChoices.JobHistoryProcessingGuid,
				StopState.DrainStopped
			)
			{
				TestName = "If there are other batches processing, JobHistory should end up in Processing (DrainStop)"
			}.WithId("1E0AD09D-2DF1-41B4-BC1F-188FB83CCFEA");

			yield return new TestCaseData(
				new[] { new JobTest { StopState = StopState.DrainStopped } },
				TOTAL_NUMBER_OF_RECORDS,
				0,
				false,
				JobStatusChoices.JobHistorySuspendedGuid,
				StopState.None
			)
			{
				TestName = "If other batches are suspended, JobHistory should end up Suspended"
			}.WithId("AB60A230-3B1C-49E1-8F5A-94B7793C24BF");

			yield return new TestCaseData(
				new[] { new JobTest { StopState = StopState.None } },
				TOTAL_NUMBER_OF_RECORDS,
				0,
				false,
				JobStatusChoices.JobHistoryProcessingGuid,
				StopState.None
			)
			{
				TestName = "If other batches are pending, JobHistory should end up Processing"
			}.WithId("FF78BD63-841D-46DD-8762-845DC2110055");

			yield return new TestCaseData(
				new[] { new JobTest { StopState = StopState.None } },
				50,
				0,
				true,
				JobStatusChoices.JobHistorySuspendedGuid,
				StopState.DrainStopped
			)
			{
				TestName = "If other batches are pending, JobHistory should end up Suspended after drain stop"
			}.WithId("326EA29D-E5DB-4A9C-A0CF-8FA71A7EDA3A");
		}

		private const int TOTAL_NUMBER_OF_RECORDS = 100;

		[TestCaseSource(nameof(FinalJobHistoryStatusTestCases))]
		public void SyncWorker_ShouldSetCorrectJobHistoryStatus(JobTest[] otherJobs,
			int transferredItems, int itemLevelErrors, bool drainStopRequested, Guid expectedJobHistoryStatus, StopState expectedStopState)
		{
			// Arrange
			string xmlPath = PrepareRecords(TOTAL_NUMBER_OF_RECORDS);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);
			jobHistory.TotalItems = 5000;

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			PrepareOtherJobs(job, jobHistory, otherJobs);

			SyncWorker sut = PrepareSut((importJob) =>
			{
				importJob.Complete(transferredItems, itemLevelErrors);

				if (drainStopRequested)
				{
					agent.ToBeRemoved = true;
				}
			});

			// Act
			var syncManagerJob = job.AsJob();
			sut.Execute(syncManagerJob);

			// Assert
			FakeRelativityInstance.JobsInQueue.First(x => x.JobId == job.JobId).StopState.Should().Be(expectedStopState);
			jobHistory.JobStatus.Guids.First().Should().Be(expectedJobHistoryStatus);
			jobHistory.ItemsTransferred.Should().Be(transferredItems);
			jobHistory.ItemsWithErrors.Should().Be(itemLevelErrors);
		}

		private void PrepareOtherJobs(JobTest job, JobHistoryTest jobHistory, JobTest[] otherJobs)
		{
			foreach(var otherJob in otherJobs)
			{
				otherJob.RootJobId = job.RootJobId;
				otherJob.WorkspaceID = job.WorkspaceID;

				FakeRelativityInstance.JobsInQueue.Add(otherJob);

				InsertBatchToJobTrackerTable(otherJob, jobHistory);
			}
		}

		[IdentifiedTest("5C618B8A-D8F5-4BD8-B83A-CC9A289093BF")]
		public void SyncWorker_ShouldMarkJobAsFailedOnIAPIException()
		{
			// Arrange
			string xmlPath = PrepareRecords(TOTAL_NUMBER_OF_RECORDS);
			JobTest job = PrepareJob(xmlPath, out JobHistoryTest jobHistory);

			SyncWorker sut = PrepareSut((importJob) => { throw new Exception(); });

			// Act & Assert
			Action act = () => sut.Execute(job.AsJob());

			act.ShouldThrow<Exception>();

			jobHistory.JobStatus.Guids.First().Should().Be(JobStatusChoices.JobHistoryErrorJobFailedGuid);
            FakeRelativityInstance.JobsInQueue.Single().StopState.Should().Be(StopState.None);
		}

		private List<string> GetRemainingItems(JobTest job)
		{
			TaskParameters paramaters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			List<string> remainingItems = (paramaters.BatchParameters as JArray).ToObject<List<string>>();
			return remainingItems;
		}

		private void SetupWorkspaceDbContextMock_AsNotLastBatch()
		{
			Mock<IWorkspaceDBContext> dbContextMock = new Mock<IWorkspaceDBContext>();
			dbContextMock.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(1);
			dbContextMock.Setup(_ =>
					_.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns(0);


			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("");
			dataTable.Rows.Add(new object());
			dbContextMock.Setup(x =>
				x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns(dataTable);

			dbContextMock.Setup(_ => _.ExecuteSqlStatementAsScalar<int>(It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns((string sql, IEnumerable<SqlParameter> sqlParams) =>
				{
					return sqlParams.Any(p => p.ParameterName.Contains("batchIsFinished")) ? 1 : 0;
				});

			Container.Register(Component.For<IWorkspaceDBContext>().Instance(dbContextMock.Object).LifestyleSingleton()
				.IsDefault());
		}
	}
}