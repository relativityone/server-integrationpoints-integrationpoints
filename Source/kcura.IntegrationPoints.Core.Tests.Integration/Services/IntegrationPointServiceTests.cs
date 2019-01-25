using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	public class IntegrationPointServiceTests : RelativityProviderTemplate
	{
		private const string _SOURCECONFIG = "Source Config";
		private const string _NAME = "Name";
		private const string _FIELDMAP = "Map";
		private const int _ADMIN_USER_ID = 9;
		private const string _REALTIVITY_SERVICE_ACCOUNT_FULL_NAME = "Service Account, Relativity";
		private const string _INTEGRATION_POINT_PROVIDER_VALIDATION_EXCEPTION_MESSAGE = "Integration Points provider validation failed, please review result property for the details.";
		private DestinationProvider _destinationProvider;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private ISavedSearchQueryRepository _savedSearchRepository;

		public IntegrationPointServiceTests() : base("IntegrationPointService Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			QueryRequest request = new QueryRequest()
			{
				Fields = new DestinationProvider().ToFieldList(),
			};
			_destinationProvider = CaseContext.RsapiService.RelativityObjectManager.Query<DestinationProvider>(request).First();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobService = Container.Resolve<IJobService>();
			_savedSearchRepository = _repositoryFactory.GetSavedSearchQueryRepository(SourceWorkspaceArtifactId);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, Import.GetImportTable("IPTestDocument", 3));
		}

		#region UpdateProperties

		[Test]
		public void SaveIntegration_UpdateNothing()
		{
			const string name = "Resaved Rip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);
			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			ValidateModel(originalModel, newModel, new string[0]);
		}

		[Test]
		public void SaveIntegration_UpdateName_OnRanIp_ErrorCase()
		{
			const string name = "Update Name - OnRanIp";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Name = "newName";

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[Test]
		public void SaveIntegration_UpdateMap_OnRanIp()
		{
			const string name = "Update Map - OnRanIp";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Map = CreateSampleFieldsMapWithLongTextField();

			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);
			ValidateModel(originalModel, newModel, new string[] { _FIELDMAP });

			Audit audit = this.GetLastAuditsForIntegrationPoint(defaultModel.Name, 1).First();
			Assert.AreEqual(SharedVariables.UserFullName, audit.UserFullName, "The user should be correct.");
			Assert.AreEqual("Update", audit.AuditAction, "The audit action should be correct.");
		}

		[Test]
		public void SaveIntegration_UpdateConfig_OnNewRip()
		{
			//Arrange
			const string name = "Update Source Config - SavedSearch - OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			int newSavedSearch = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, name);
			defaultModel.SourceConfiguration = CreateSourceConfig(newSavedSearch, SourceWorkspaceArtifactId, SourceConfiguration.ExportType.SavedSearch);

			//Act & Assert
			Assert.DoesNotThrow(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[Test]
		public void SaveIntegration_UpdateName_OnNewRip()
		{
			//Arrange
			const string name = "Update Name - OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Name = name + " 2";

			//Act & Assert
			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Name cannot be changed once the Integration Point has been run");
		}

		[Test]
		public void SaveIntegration_UpdateMap_OnNewRip()
		{
			//Arrange
			const string name = "Update Map - OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Map = CreateSampleFieldsMapWithLongTextField();

			//Act
			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			//Assert
			ValidateModel(originalModel, newModel, new[] { _FIELDMAP });

			Audit audit = this.GetLastAuditsForIntegrationPoint(defaultModel.Name, 1).First();
			Assert.AreEqual(SharedVariables.UserFullName, audit.UserFullName, "The user should be correct.");
			Assert.AreEqual("Update", audit.AuditAction, "The audit action should be correct.");
		}

		[Test]
		public void SaveIntegration_IntegrationPointWithNoSchedulerAndUpdateWithScheduler()
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, Import.GetImportTable("RunNow", 3));

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Act
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			integrationPoint.Scheduler = new Scheduler()
			{
				EnableScheduler = true,
				StartDate = DateTime.UtcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
				EndDate = DateTime.UtcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
				ScheduledTime = DateTime.UtcNow.ToString("HH") + ":" + DateTime.UtcNow.AddMinutes(1).ToString("mm"),
				Reoccur = 0,
				SelectedFrequency = ScheduleInterval.None.ToString(),
				TimeZoneId = TimeZoneInfo.Utc.Id
			};
			IntegrationPointModel modifiedIntegrationPoint = CreateOrUpdateIntegrationPoint(integrationPoint);
			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPoint.ArtifactID);

			//Assert
			Audit postRunAudit = this.GetLastAuditsForIntegrationPoint(modifiedIntegrationPoint.Name, 1).First();

			Assert.AreEqual("Update", postRunAudit.AuditAction, "The audit action should be Update");
			Assert.AreEqual(SharedVariables.UserFullName, postRunAudit.UserFullName, "The user should be correct");

			AssertThatAuditDetailsChanged(postRunAudit, new HashSet<string>() { "Next Scheduled Runtime (UTC)", "Has Errors" });
		}

		#endregion UpdateProperties

		[SmokeTest]
		[TestInQuarantine("Unstable - to be fixed -> REL-280316")]
		public void CreateAndRunIntegrationPoint_GoldFlow()
		{
			//Arrange
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{ DateTime.Now:yy - MM - dd HH - mm - ss}",
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
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
			IntegrationPointModel integrationPointPostJob = _integrationPointService.ReadIntegrationPoint(integrationPoint.ArtifactID);
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactId);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointPostJob.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(false, integrationPointPostJob.HasErrors);
			Assert.IsNotNull(integrationPointPostJob.LastRun);
			Assert.AreEqual(3, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRun.Name, jobHistory.JobType.Name);

			IList<Audit> postRunAudits = this.GetLastAuditsForIntegrationPoint(integrationModel.Name, 3);
			Assert.AreEqual(3, postRunAudits.Count, "There should be 4 audits");
			Assert.IsTrue(postRunAudits.All(x => x.AuditAction == "Update"));
			Assert.IsTrue(postRunAudits.All(x => x.UserFullName == _REALTIVITY_SERVICE_ACCOUNT_FULL_NAME), "The user full name should match");

			AssertThatAuditDetailsChanged(postRunAudits.First(), new HashSet<string>() { "Last Runtime (UTC)" });
		}

		[Test]
		[TestInQuarantine(@"Test to be fixed - doesn't work on Jenkins.
				Is fine when run locally. More info in REL-270155")]
		public void RetryIntegrationPoint_GoldFlow()
		{
			//Arrange

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
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
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
			IList<Audit> postRunAudits = this.GetLastAuditsForIntegrationPoint(integrationModel.Name, 4);

			//Update Integration Point's SelectedOverWrite to "Overlay Only"
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPoint(integrationPoint.ArtifactID);
			integrationPointPostRun.SelectedOverwrite = "Overlay Only";
			integrationPointPostRun.Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly);
			CreateOrUpdateIntegrationPoint(integrationPointPostRun);

			//Retry Errors
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointPostRun.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointPostRun.ArtifactID);
			IntegrationPointModel integrationPointPostRetry = _integrationPointService.ReadIntegrationPoint(integrationPointPostRun.ArtifactID);

			IJobHistoryRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactId);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryErrorRepository.GetLastJobHistoryArtifactId(integrationPointPostRetry.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(true, integrationPointPostRun.HasErrors, "The first integration point run should have errors");
			Assert.AreEqual(false, integrationPointPostRetry.HasErrors, "The integration point post retry should not have errors");
			Assert.AreEqual(3, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRetryErrors.Name, jobHistory.JobType.Name);

			Assert.AreEqual(4, postRunAudits.Count, "There should be 4 audits");
			Assert.IsTrue(postRunAudits.All(x => x.AuditAction == "Update"));
			Assert.IsTrue(postRunAudits.All(x => x.UserFullName == _REALTIVITY_SERVICE_ACCOUNT_FULL_NAME), "The user full name should match");
			AssertThatAuditDetailsChanged(postRunAudits.First(), new HashSet<string>() { "Last Runtime (UTC)", "Has Errors" });

			IList<Audit> postRetryAudits = this.GetLastAuditsForIntegrationPoint(integrationModel.Name, 4);
			Assert.AreEqual(4, postRetryAudits.Count, "There should be 4 audits");
			Assert.IsTrue(postRetryAudits.All(x => x.AuditAction == "Update"));
			Assert.IsTrue(postRetryAudits.All(x => x.UserFullName == _REALTIVITY_SERVICE_ACCOUNT_FULL_NAME), "The user full name should match");
			AssertThatAuditDetailsChanged(postRetryAudits.First(), new HashSet<string>() { "Last Runtime (UTC)", "Has Errors" });
		}

		[TestInQuarantine]
		public void CreateAndRunIntegrationPoint_ScheduledIntegrationPoint_GoldFlow()
		{
			//Arrange

			DateTime utcNow = DateTime.UtcNow;
			const int schedulerRunTimeDelayMinutes = 2;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(schedulerRunTimeDelayMinutes).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointPreJobExecution = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			Status.WaitForScheduledJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointPreJobExecution.ArtifactID, timeoutInSeconds: 600);
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPoint(integrationPointPreJobExecution.ArtifactID);

			//Assert
			Assert.AreEqual(null, integrationPointPreJobExecution.LastRun);
			Assert.AreEqual(false, integrationPointPreJobExecution.HasErrors);
			Assert.AreEqual(false, integrationPointPostRun.HasErrors);
			Assert.IsNotNull(integrationPointPostRun.LastRun);
			Assert.IsNotNull(integrationPointPostRun.NextRun);

			Audit postRunAudit = this.GetLastAuditsForIntegrationPoint(integrationPointPostRun.Name, 1).First();

			Assert.AreEqual("Update", postRunAudit.AuditAction, "The audit action should be Update");
			Assert.AreEqual(_REALTIVITY_SERVICE_ACCOUNT_FULL_NAME, postRunAudit.UserFullName, "The user should be correct");

			AssertThatAuditDetailsChanged(postRunAudit, new HashSet<string>() { "Next Scheduled Runtime (UTC)", "Last Runtime (UTC)" });

			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointPreJobExecution.ArtifactID);
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		[TestCase("02/31/3000")]
		[TestCase("01-31-3000")]
		[TestCase("abcdefg")]
		[TestCase("12345")]
		[TestCase("-01/31/3000")]
		public void CreateScheduledIntegrationPoint_WithInvalidStartDate_ExpectError(string startDate)
		{
			//Arrange
			DateTime utcNow = DateTime.UtcNow;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = startDate,
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy"),
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(1).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Act & Assert
			Assert.Throws<IntegrationPointValidationException>(() => CreateOrUpdateIntegrationPoint(integrationModel),
				_INTEGRATION_POINT_PROVIDER_VALIDATION_EXCEPTION_MESSAGE);
		}

		[Test]
		[TestCase("")]
		[TestCase(null)]
		[TestCase("15/31/3000")]
		[TestCase("31-01-3000")]
		[TestCase("abcdefg")]
		[TestCase("12345")]
		[TestCase("-01/31/3000")]
		public void CreateScheduledIntegrationPoint_WithInvalidEndDate_ExpectError(string endDate)
		{
			//Arrange
			DateTime utcNow = DateTime.UtcNow;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = endDate,
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(1).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Act
			IntegrationPointModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointModel.ArtifactID);

			//Assert 
			Assert.IsNull(integrationPointModel.Scheduler.EndDate);
		}

		[Test]
		public void RunJobWithFailingValidation_ExpectError_SaveJobHistory()
		{
			// Arrange 

			int TemporarySavedSearchId = CreateTemporarySavedSearch();
			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfigWithCustomParameters(TargetWorkspaceArtifactId, TemporarySavedSearchId, SourceWorkspaceArtifactId, SourceConfiguration.ExportType.SavedSearch),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId,
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				}
			};

			IntegrationPointModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			DeleteSavedSearch(SourceWorkspaceArtifactId, TemporarySavedSearchId);

			// Act
			Assert.Throws<IntegrationPointValidationException>(() => _integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointModel.ArtifactID, _ADMIN_USER_ID));

			// Assert
			Data.IntegrationPoint ip = _integrationPointService.GetRdo(integrationPointModel.ArtifactID);
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
			return SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, "NewSavedSearch");
		}

		private void CleanScheduleAgentQueueFromAllRipJobs(int integrationPointArtifactId)
		{
			IList<Job> jobs = _jobService.GetJobs(integrationPointArtifactId);
			foreach (Job job in jobs)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}

		private void AssertThatAuditDetailsChanged(Audit audit, HashSet<string> fieldNames)
		{
			IDictionary<string, Tuple<string, string>> auditDetailsFieldValueDictionary = this.GetAuditDetailsFieldValues(audit, fieldNames);

			foreach (string key in auditDetailsFieldValueDictionary.Keys)
			{
				Tuple<string, string> auditDetailsFieldValueTuple = auditDetailsFieldValueDictionary[key];
				Assert.IsNotNull(auditDetailsFieldValueTuple, "The audit should contain the field value changes");
				Assert.AreNotEqual(auditDetailsFieldValueTuple.Item1, auditDetailsFieldValueTuple.Item2, "The field's values should have changed");
			}
		}

		private void ValidateModel(IntegrationPointModel expectedModel, IntegrationPointModel actual, string[] updatedProperties)
		{
			Action<object, object> assertion = DetermineAssertion(updatedProperties, _SOURCECONFIG);
			assertion(expectedModel.SourceConfiguration, actual.SourceConfiguration);

			assertion = DetermineAssertion(updatedProperties, _NAME);
			assertion(expectedModel.Name, actual.Name);

			assertion = DetermineAssertion(updatedProperties, _FIELDMAP);
			assertion(expectedModel.Map, actual.Map);

			Assert.AreEqual(expectedModel.HasErrors, actual.HasErrors);
			Assert.AreEqual(expectedModel.DestinationProvider, actual.DestinationProvider);
			Assert.NotNull(expectedModel.LastRun);
			Assert.NotNull(actual.LastRun);
			Assert.AreEqual(expectedModel.LastRun.Value.Date, actual.LastRun.Value.Date);
			Assert.AreEqual(expectedModel.LastRun.Value.Hour, actual.LastRun.Value.Hour);
			Assert.AreEqual(expectedModel.LastRun.Value.Minute, actual.LastRun.Value.Minute);
		}

		private Action<object, object> DetermineAssertion(string[] updatedProperties, string property)
		{
			Action<object, object> assertion;
			if (updatedProperties.Contains(property))
			{
				assertion = Assert.AreNotEqual;
			}
			else
			{
				assertion = Assert.AreEqual;
			}
			return assertion;
		}

		private string CreateSourceConfig(int savedSearchId, int targetWorkspaceId, SourceConfiguration.ExportType typeOfExport)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SavedSearchArtifactId = savedSearchId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				TypeOfExport = typeOfExport
			};
			return Container.Resolve<ISerializer>().Serialize(sourceConfiguration);
		}

		private IntegrationPointModel CreateIntegrationPointThatHasNotRun(string name)
		{
			return new IntegrationPointModel()
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = _destinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfig(SavedSearchArtifactId, TargetWorkspaceArtifactId, SourceConfiguration.ExportType.SavedSearch),
				LogErrors = true,
				Name = $"${name} - {DateTime.Today:yy-MM-dd HH-mm-ss}",
				Map = CreateDefaultFieldMap(),
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private IntegrationPointModel CreateIntegrationPointThatIsAlreadyRunModel(string name)
		{
			IntegrationPointModel model = CreateIntegrationPointThatHasNotRun(name);
			model.LastRun = DateTime.UtcNow;
			return model;
		}

		private FieldMap[] GetSampleFields()
		{
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);
			IFieldQueryRepository destinationFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(TargetWorkspaceArtifactId);

			ArtifactDTO identifierSourceField = sourceFieldQueryRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);
			ArtifactDTO identifierDestinationField = destinationFieldQueryRepository.RetrieveTheIdentifierField((int)ArtifactType.Document);

			ArtifactFieldDTO[] sourceFields = sourceFieldQueryRepository.RetrieveLongTextFieldsAsync((int)ArtifactType.Document).Result;
			ArtifactFieldDTO[] destinationFields = destinationFieldQueryRepository.RetrieveLongTextFieldsAsync((int)ArtifactType.Document).Result;

			var map = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = identifierSourceField.ArtifactId.ToString(),
						DisplayName = identifierSourceField.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = identifierDestinationField.ArtifactId.ToString(),
						DisplayName = identifierDestinationField.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true
					},
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = sourceFields.First().ArtifactId.ToString(),
						DisplayName = sourceFields.First().Name,
						IsIdentifier = false
					},
					FieldMapType = FieldMapTypeEnum.None,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = destinationFields.First().ArtifactId.ToString(),
						DisplayName = destinationFields.First().Name,
						IsIdentifier = false
					}
				}
			};
			return map;
		}

		private string CreateSampleFieldsMapWithLongTextField()
		{
			FieldMap[] map = GetSampleFields();
			return Container.Resolve<ISerializer>().Serialize(map);
		}
		#endregion
	}
}