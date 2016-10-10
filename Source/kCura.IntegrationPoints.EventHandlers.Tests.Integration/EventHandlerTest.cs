using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	[TestFixture]
	public class EventHandlerTest : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IScratchTableRepository _scratchTableRepository;
		private IJobService _jobService;
		private IIntegrationPointService _integrationPointService;
		private IntegrationModel _integrationModel;

		public EventHandlerTest() : base("Eventhandler Tests", null)
		{
			CreateAgent = false;
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, "EventHandlerTesting", "LikeASir");
			_jobService = Container.Resolve<IJobService>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_integrationModel = CreateIntegrationModel();
		}

		public override void TestTeardown()
		{
			_scratchTableRepository.DeleteTable();
		}

		[Test]
		public void PreMassDeleteEventHandler_DeleteJobHistoryErrors_Success()
		{
			//Arrange
			const int expectedErrorCount = 0;

			IntegrationPoint.PreMassDeleteEventHandler preMassDeleteEventHandler = new IntegrationPoint.PreMassDeleteEventHandler
			{
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId),
				Application = new Application(SourceWorkspaceArtifactId, null, null)
			};

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(_integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);

			JobHistory jobHistoryRunNow = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryRunNow.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryRunNow.ArtifactId, ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory jobHistoryRetry = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRetryErrors, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryRetry.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryRetry.ArtifactId, ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory jobHistoryScheduled = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryScheduledRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryScheduled.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryScheduled.ArtifactId, ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory runNowJobhistoryError2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(runNowJobhistoryError2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			//Act
			List<int> integrationPoints = new List<int>() { integrationPointModel.ArtifactID, integrationPointModel2.ArtifactID };
			preMassDeleteEventHandler.ExecutePreDelete(integrationPoints);

			//Assert
			int runNowItemErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryRunNow.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int runNowJobErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryRunNow.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Count;
			int retryItemErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryRetry.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int retryNowJobErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryRetry.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Count;
			int scheduledItemErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryScheduled.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int scheduledNowJobErrorCount = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryScheduled.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Count;
			int runNowJobHistoryErrorCount2 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(runNowJobhistoryError2.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;

			Assert.AreEqual(expectedErrorCount, runNowItemErrorCount);
			Assert.AreEqual(expectedErrorCount, runNowJobErrorCount);
			Assert.AreEqual(expectedErrorCount, retryItemErrorCount);
			Assert.AreEqual(expectedErrorCount, retryNowJobErrorCount);
			Assert.AreEqual(expectedErrorCount, scheduledItemErrorCount);
			Assert.AreEqual(expectedErrorCount, scheduledNowJobErrorCount);
			Assert.AreEqual(expectedErrorCount, runNowJobHistoryErrorCount2);
		}

		[Test]
		public void PreMassDelete_DeletesSpecificJobHistoryErrors_Success()
		{
			//Arrange
			IntegrationPoint.PreMassDeleteEventHandler preMassDeleteEventHandler = new IntegrationPoint.PreMassDeleteEventHandler
			{
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId),
				Application = new Application(SourceWorkspaceArtifactId, null, null)
			};

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel3 = CreateOrUpdateIntegrationPoint(_integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);
			Data.IntegrationPoint integrationPointRdo3 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel3.ArtifactID);

			JobHistory jobHistory1 = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory1.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory3 = _jobHistoryService.CreateRdo(integrationPointRdo3, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory3.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			//Act
			List<int> integrationPoints = new List<int>() { integrationPointModel.ArtifactID, integrationPointModel2.ArtifactID };
			preMassDeleteEventHandler.ExecutePreDelete(integrationPoints);

			//Assert
			int jobHistoryErrorCount1 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory1.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorCount2 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory2.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorCount3 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory3.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;

			Assert.AreEqual(0, jobHistoryErrorCount1);
			Assert.AreEqual(0, jobHistoryErrorCount2);
			Assert.AreEqual(1, jobHistoryErrorCount3);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void PreCascade_DeletesJobHistoryErrors_Success()
		{
			//Arrange
			IntegrationPoints.PreCascadeDeleteEventHandler preCascadeDeleteEventHandler = new IntegrationPoints.PreCascadeDeleteEventHandler(_repositoryFactory)
			{
				TempTableNameWithParentArtifactsToDelete = _scratchTableRepository.GetTempTableName(),
				Application =  new Application(SourceWorkspaceArtifactId, null, null),
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId)
			};

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel3 = CreateOrUpdateIntegrationPoint(_integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);
			Data.IntegrationPoint integrationPointRdo3 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel3.ArtifactID);

			JobHistory jobHistory1 = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory1.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory3 = _jobHistoryService.CreateRdo(integrationPointRdo3, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory3.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			int[] integrationPointArtifactIds = new int[] { integrationPointModel.ArtifactID, integrationPointModel2.ArtifactID, integrationPointModel3.ArtifactID };
			_scratchTableRepository.AddArtifactIdsIntoTempTable(integrationPointArtifactIds);

			//Act
			Response eventHandlerResponse = preCascadeDeleteEventHandler.Execute();

			//Assert
			int jobHistoryErrorCount1 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory1.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorCount2 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory2.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorCount3 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory3.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;

			Assert.AreEqual(0, jobHistoryErrorCount1);
			Assert.AreEqual(0, jobHistoryErrorCount2);
			Assert.AreEqual(0, jobHistoryErrorCount3);
			Assert.AreEqual(true, eventHandlerResponse.Success);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void PreCascade_DeletesSpecificJobHistoryErrors_Success()
		{
			//Arrange
			IntegrationPoints.PreCascadeDeleteEventHandler preCascadeDeleteEventHandler = new IntegrationPoints.PreCascadeDeleteEventHandler(_repositoryFactory)
			{
				TempTableNameWithParentArtifactsToDelete = _scratchTableRepository.GetTempTableName(),
				Application = new Application(SourceWorkspaceArtifactId, null, null),
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId)
			};

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(_integrationModel);
			IntegrationModel integrationPointModel3 = CreateOrUpdateIntegrationPoint(_integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);
			Data.IntegrationPoint integrationPointRdo3 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel3.ArtifactID);

			JobHistory jobHistory1 = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory1.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory3 = _jobHistoryService.CreateRdo(integrationPointRdo3, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory3.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistory3.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorJob);

			int[] integrationPointArtifactIds = new int[] { integrationPointModel.ArtifactID, integrationPointModel2.ArtifactID };
			_scratchTableRepository.AddArtifactIdsIntoTempTable(integrationPointArtifactIds);

			//Act
			Response eventHandlerResponse = preCascadeDeleteEventHandler.Execute();

			//Assert
			int jobHistoryErrorCount1 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory1.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorCount2 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory2.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorItemCount3 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory3.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item).Count;
			int jobHistoryErrorJobCount3 = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistory3.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Job).Count;


			Assert.AreEqual(0, jobHistoryErrorCount1);
			Assert.AreEqual(0, jobHistoryErrorCount2);
			Assert.AreEqual(1, jobHistoryErrorItemCount3);
			Assert.AreEqual(1, jobHistoryErrorJobCount3);
			Assert.AreEqual(true, eventHandlerResponse.Success);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void PreCascade_DeleteJobsWithAndWithoutHistory_Success()
		{
			//Arrange
			IntegrationPoints.PreCascadeDeleteEventHandler preCascadeDeleteEventHandler = new IntegrationPoints.PreCascadeDeleteEventHandler(_repositoryFactory)
			{
				TempTableNameWithParentArtifactsToDelete = _scratchTableRepository.GetTempTableName(),
				Application = new Application(SourceWorkspaceArtifactId, null, null),
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId)
			};
			
			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(CreateIntegrationModel());
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(CreateIntegrationModel());
			IntegrationModel integrationPointModel3 = CreateOrUpdateIntegrationPoint(CreateIntegrationModel());
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);

			JobHistory jobHistory1 = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			JobHistory jobHistory2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorJob);

			int[] integrationPointArtifactIds = new int[] { integrationPointModel.ArtifactID, integrationPointModel2.ArtifactID, integrationPointModel3.ArtifactID };
			_scratchTableRepository.AddArtifactIdsIntoTempTable(integrationPointArtifactIds);
			
			//Act
			Response eventHandlerResponse = preCascadeDeleteEventHandler.Execute();
			
			//Assert
			Data.IntegrationPoint integrationPoint1AfterRun = _integrationPointService.GetRdo(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPoint2AfterRun = _integrationPointService.GetRdo(integrationPointModel2.ArtifactID);
			Data.IntegrationPoint integrationPoint3AfterRun = _integrationPointService.GetRdo(integrationPointModel3.ArtifactID);

			int jobHistory1ItemCount = integrationPoint1AfterRun.JobHistory.Length;
			int jobHistory2ItemCount = integrationPoint2AfterRun.JobHistory.Length;
			int jobHistory3ItemCount = integrationPoint3AfterRun.JobHistory.Length;

			Assert.AreEqual(0, jobHistory1ItemCount);
			Assert.AreEqual(0, jobHistory2ItemCount);
			Assert.AreEqual(0, jobHistory3ItemCount);
			Assert.AreEqual(true, eventHandlerResponse.Success);
			
		}


		[Test]
		public void Delete_DeletesJobs_Success()
		{
			//Arrange
			IntegrationPoints.DeleteEventHandler deleteEventHandler = new IntegrationPoints.DeleteEventHandler
			{
				Application = new Application(SourceWorkspaceArtifactId, null, null),
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId),
			};

			DateTime utcNow = DateTime.UtcNow;
			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = LdapProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy"),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy"),
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(10).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			Job jobPreDelete = _jobService.GetScheduledJobs(SourceWorkspaceArtifactId, integrationPointModel.ArtifactID, TaskType.SyncManager.ToString());
			
			//Act
			deleteEventHandler.ActiveArtifact = new Artifact(integrationPointModel.ArtifactID, SourceWorkspaceArtifactId, 0, null, false, null);
			Response eventHandlerResponse = deleteEventHandler.Execute();

			//Assert
			Job jobPostDelete = _jobService.GetScheduledJobs(SourceWorkspaceArtifactId, integrationPointModel.ArtifactID, TaskType.SyncManager.ToString());
			Assert.IsNotNull(jobPreDelete);
			Assert.IsNull(jobPostDelete);
			Assert.AreEqual(true, eventHandlerResponse.Success);
		}

		[Test]
		public void PreCascade_ThrowsException_Failure()
		{
			//Arrange
			IntegrationPoints.PreCascadeDeleteEventHandler preCascadeDeleteEventHandler = new IntegrationPoints.PreCascadeDeleteEventHandler(_repositoryFactory)
			{
				Application = new Application(-1, null, null),
				Helper = new EHHelper(Helper, -1),
			};

			//Act
			Response eventHandlerResponse = preCascadeDeleteEventHandler.Execute();

			//Assert
			Assert.AreEqual(false, eventHandlerResponse.Success);
			StringAssert.Contains("An error occurred while executing the Mass Delete operation.", eventHandlerResponse.Exception.Message);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void Delete_ThrowsException_Failure()
		{
			//Arrange
			IntegrationPoints.DeleteEventHandler deleteEventHandler = new IntegrationPoints.DeleteEventHandler()
			{
				Application = new Application(-1, null, null),
				Helper = new EHHelper(Helper, -1),
			};

			//Act 
			Response eventHandlerResponse = deleteEventHandler.Execute();
		
			//Assert
			Assert.AreEqual(false, eventHandlerResponse.Success);
			StringAssert.Contains("Failed to delete corresponding job.", eventHandlerResponse.Message);
		}

		private IntegrationModel CreateIntegrationModel()
		{
			return new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};
		}

		private void CreateJobLevelJobHistoryError(int jobHistoryArtifactId, Relativity.Client.Choice errorStatus, Relativity.Client.Choice errorType)
		{
			List<JobHistoryError> jobHistoryErrors = new List<JobHistoryError>();
			JobHistoryError jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				JobHistory = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				SourceUniqueID = null,
				ErrorType = errorType,
				ErrorStatus = errorStatus,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from EventHandlerTests",
				TimestampUTC = DateTime.Now,
			};

			jobHistoryErrors.Add(jobHistoryError);
			CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
		}
	}
}