using System;
using System.Collections.Generic;
using System.Globalization;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointServiceForRunTests : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private ISavedSearchQueryRepository _savedSearchRepository;

		public IntegrationPointServiceForRunTests() : base("IntegrationPointService Source", "IntegrationPointService Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobService = Container.Resolve<IJobService>();
			_savedSearchRepository = _repositoryFactory.GetSavedSearchQueryRepository(SourceWorkspaceArtifactID);
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, Import.GetImportTable("IPTestDocument", 3));
		}

		[IdentifiedTest("69e67a17-8b23-41a8-b120-9a4441171d16")]
		[Order(1)]
		[SmokeTest]
		public void CreateAndRunIntegrationPoint_GoldFlow()
		{
			//Arrange
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateAndRunIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);
			IntegrationPointModel integrationPointPostJob = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactID);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointPostJob.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(false, integrationPointPostJob.HasErrors);
			Assert.IsNotNull(integrationPointPostJob.LastRun);
			Assert.AreEqual(3, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRun.Name, jobHistory.JobType.Name);
		}

		[IdentifiedTest("8a1efb36-117e-4c96-814d-537209d04314")]
		[Order(2)]
		[NotWorkingOnTrident]
		public void RetryIntegrationPoint_GoldFlow()
		{
			//Arrange
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"RetryIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			//Update Integration Point's SelectedOverWrite to "Overlay Only"
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);
			integrationPointPostRun.SelectedOverwrite = "Overlay Only";
			integrationPointPostRun.Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly);
			CreateOrUpdateIntegrationPoint(integrationPointPostRun);

			//Retry Errors
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactID, integrationPointPostRun.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointPostRun.ArtifactID);
			IntegrationPointModel integrationPointPostRetry = _integrationPointService.ReadIntegrationPointModel(integrationPointPostRun.ArtifactID);

			IJobHistoryRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactID);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryErrorRepository.GetLastJobHistoryArtifactId(integrationPointPostRetry.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(true, integrationPointPostRun.HasErrors, "The first integration point run should have errors");
			Assert.AreEqual(false, integrationPointPostRetry.HasErrors, "The integration point post retry should not have errors");
			Assert.AreEqual(3, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRetryErrors.Name, jobHistory.JobType.Name);
		}

		[IdentifiedTest("7d479f10-b1fc-4a52-a081-18332ed91fc0")]
		[Order(3)]
		[NotWorkingOnTrident]
		public void CreateAndRunIntegrationPoint_ScheduledIntegrationPoint_GoldFlow()
		{
			//Arrange

			DateTime utcNow = DateTime.UtcNow;
			const int schedulerRunTimeDelayMinutes = 2;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateAndRunIntegrationPoint_ScheduledIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = utcNow.AddMinutes(schedulerRunTimeDelayMinutes).ToString("HH:mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointPreJobExecution = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			Status.WaitForScheduledJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointPreJobExecution.ArtifactID, timeoutInSeconds: 600);
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPointModel(integrationPointPreJobExecution.ArtifactID);

			//Assert
			Assert.AreEqual(null, integrationPointPreJobExecution.LastRun);
			Assert.AreEqual(false, integrationPointPreJobExecution.HasErrors);
			Assert.AreEqual(false, integrationPointPostRun.HasErrors);
			Assert.IsNotNull(integrationPointPostRun.LastRun);
			Assert.IsNotNull(integrationPointPostRun.NextRun);

			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointPreJobExecution.ArtifactID);
		}

		[IdentifiedTest("0b87716a-5712-41a1-ab79-c98b4a07461a")]
		[Order(4)]
		public void RunJobWithFailingValidation_ExpectError_SaveJobHistory()
		{
			// Arrange 

			int TemporarySavedSearchId = CreateTemporarySavedSearch();
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfigWithCustomParameters(TargetWorkspaceArtifactID, TemporarySavedSearchId, SourceWorkspaceArtifactID, SourceConfiguration.ExportType.SavedSearch),
				LogErrors = true,
				Name = $"RunJobWithFailingValidation_ExpectError_SaveJobHistory {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId,
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				}
			};

			IntegrationPointModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			DeleteSavedSearch(SourceWorkspaceArtifactID, TemporarySavedSearchId);

			// Act
			Assert.Throws<IntegrationPointValidationException>(() => _integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPointModel.ArtifactID, _ADMIN_USER_ID));

			// Assert
			Data.IntegrationPoint ip = _integrationPointService.ReadIntegrationPoint(integrationPointModel.ArtifactID);
			var jobHistory = _jobHistoryService.GetJobHistory(ip.JobHistory);
			Assert.NotNull(ip.JobHistory);
			Assert.AreEqual(JobStatusChoices.JobHistoryValidationFailed.Name, jobHistory[0].JobStatus.Name);
		}

		#region "Helpers"

		private void DeleteSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			SavedSearch.Delete(workspaceArtifactId, savedSearchId);
			while (_savedSearchRepository.RetrieveSavedSearch(savedSearchId) != null)
			{
			}
		}

		private int CreateTemporarySavedSearch()
		{
			return SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, "NewSavedSearch");
		}

		private void CleanScheduleAgentQueueFromAllRipJobs(int integrationPointArtifactId)
		{
			IList<Job> jobs = _jobService.GetJobs(integrationPointArtifactId);
			foreach (Job job in jobs)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}

		#endregion
	}
}