using System;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.Choice;
using Constants = kCura.IntegrationPoints.Core.Constants;
using DateTime = System.DateTime;
using PreMassDeleteEventHandler = kCura.IntegrationPoints.EventHandlers.IntegrationPoint.PreMassDeleteEventHandler;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	public class DeleteHistoryServiceTest : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private int _applicationArtifactId;

		public DeleteHistoryServiceTest() : base("Eventhandler Tests", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
			_applicationArtifactId = GetArtifactIdByGuid();
		}

		[Test]
		public void PreMassDeleteEventHandler_DeleteJobHistoryErrors_Success()
		{
			//Arrange
			const int expectedErrorCount = 0;

			PreMassDeleteEventHandler preMassDeleteEventHandler = new PreMassDeleteEventHandler
			{
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId),
				Application = new Application(_applicationArtifactId, null, null)
			};

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);

			JobHistory jobHistoryRunNow = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRunNow, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryRunNow.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryRunNow.ArtifactId, ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory jobHistoryRetry = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRetryErrors, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryRetry.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryRetry.ArtifactId, ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory jobHistoryScheduled = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryScheduledRun, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistoryScheduled.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);
			CreateJobLevelJobHistoryError(jobHistoryScheduled.ArtifactId, ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob);

			JobHistory runNowJobhistoryError2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRunNow, DateTime.Now);
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
			PreMassDeleteEventHandler preMassDeleteEventHandler = new PreMassDeleteEventHandler
			{
				Helper = new EHHelper(Helper, SourceWorkspaceArtifactId),
				Application = new Application(_applicationArtifactId, null, null)
			};

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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

			IntegrationModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationModel integrationPointModel2 = CreateOrUpdateIntegrationPoint(integrationModel);
			IntegrationModel integrationPointModel3 = CreateOrUpdateIntegrationPoint(integrationModel);
			Data.IntegrationPoint integrationPointRdo = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);
			Data.IntegrationPoint integrationPointRdo2 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel2.ArtifactID);
			Data.IntegrationPoint integrationPointRdo3 = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel3.ArtifactID);

			JobHistory jobHistory1 = _jobHistoryService.CreateRdo(integrationPointRdo, Guid.NewGuid(), JobTypeChoices.JobHistoryRunNow, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory1.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory2 = _jobHistoryService.CreateRdo(integrationPointRdo2, Guid.NewGuid(), JobTypeChoices.JobHistoryRunNow, DateTime.Now);
			CreateJobLevelJobHistoryError(jobHistory2.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			JobHistory jobHistory3 = _jobHistoryService.CreateRdo(integrationPointRdo3, Guid.NewGuid(), JobTypeChoices.JobHistoryRunNow, DateTime.Now);
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

		private int GetArtifactIdByGuid()
		{
			Guid applicationGuid = new Guid(Constants.IntegrationPoints.APPLICATION_GUID_STRING);
			return DBContextExtensions.GetArtifactIDByGuid(CaseContext.SqlContext, applicationGuid);
		}

		private List<int> CreateJobLevelJobHistoryError(int jobHistoryArtifactId, Choice errorStatus, Choice errorType)
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

			List<int> jobHistoryErrorArtifactIds = CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
			return jobHistoryErrorArtifactIds;
		}
	}
}