using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Search;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class JobHistoryErrorsBatchingTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private IBatchStatus _batchStatus;
		private const int _ADMIN_USER_ID = 9;
		private const string _JOB_START_TEMP_TABLE_PREFIX = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		private const string _JOB_COMPLETE_TEMP_TABLE_PREFIX = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";
		private const string _ITEM_START_INCLUDED_TEMP_TABLE_PREFIX = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart";
		private const string _ITEM_START_EXCLUDED_TEMP_TABLE_PREFIX = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart_Excluded";
		private const string _ITEM_COMPLETE_INCLUDED_TEMP_TABLE_PREFIX = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
		
		public JobHistoryErrorsBatchingTests() : base("JobHistoryErrorsSource", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			
			ResolveServices();
		}

		[Test]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsMatchingBatchSize()
		{
			var docPrefix = "EqualBatchDoc";
			var expDocPrefix = "EqualBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(1, 100, docPrefix, expDocPrefix);
		}

		[Test]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsUnderBatchSize()
		{
			var docPrefix = "LessThanBatchDoc";
			var expDocPrefix = "LessThanBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(300, 98, docPrefix, expDocPrefix);
		}

		[Test]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsOverBatchSize()
		{
			var docPrefix = "MoreThanBatchDoc";
			var expDocPrefix = "MoreThanBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(500, 102, docPrefix, expDocPrefix);
		}

		[Test]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutAJobHistory()
		{
			//Arrange
			var errorReceived = false;

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			try
			{
				_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
				Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("Unable to retrieve the previous job history"))
				{
					errorReceived = true;
				}
			}

			//Assert
			if (!errorReceived)
			{
				throw new Exception("Error: Did not receive error when submitting a retry job for an integration point without a Job History.");
			}
		}

		[Test]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutJobHistoryErrors()
		{
			//Arrange
			var errorReceived = false;
			var docPrefix = "ErrorScenarioDoc";

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, docPrefix, false);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID);

			//Act
			try
			{
				_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
				Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains(Core.Constants.IntegrationPoints.FAILED_TO_RETRIEVE_JOB_HISTORY))
				{
					errorReceived = true;
				}
			}

			//Assert
			if (!errorReceived)
			{
				throw new Exception("Error: Did not receive error when submitting a retry job for an integration point without a Job History.");
			}
		}

		[Test]
		public void ExpectJobLevelJobHistoryErrorUpdatedForJobLevelErrorWhenBatching()
		{
			//Arrange
			var docPrefix = "JobLevelImport";
			var stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			var helper = NSubstitute.Substitute.For<IHelper>();
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, docPrefix, false);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactId)
				.WithRelatedObjectArtifactId(integrationPointCreated.ArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";
			_jobHistoryErrorManager = new JobHistoryErrorManager(RepositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create job level error
			List<int> expectedJobHistoryErrorArtifactIds = CreateJobLevelJobHistoryError(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew);

			//Act
			_jobHistoryErrorManager.StageForUpdatingErrors(job, JobTypeChoices.JobHistoryRetryErrors);

			string startTempTableName = $"{ _JOB_START_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			string completeTempTableName = $"{ _JOB_COMPLETE_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			DataTable startTempTable = GetTempTable(startTempTableName);
			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, helper, RepositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, new JobHistoryErrorDTO.UpdateStatusType());

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorArtifactIds, ErrorStatusChoices.JobHistoryErrorInProgress);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorArtifactIds, ErrorStatusChoices.JobHistoryErrorRetried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorArtifactIds);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorArtifactIds);
		}

		[Test]
		public void ExpectJobandItemLevelJobHistoryErrorsUpdatedWhenBatching()
		{
			//Arrange
			var docPrefix = "DocForItemAndJob";
			var expiredDocPrefix = "ExpForItemAndJob";
			DataTable importTable = GetImportTable(800, 100, docPrefix, expiredDocPrefix);
			var stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			var helper = NSubstitute.Substitute.For<IHelper>();

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTable);
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, docPrefix, false);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactId)
				.WithRelatedObjectArtifactId(integrationPointCreated.ArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";

			_jobHistoryErrorManager = new JobHistoryErrorManager(RepositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create item level error
			ICollection<int> expectedJobHistoryErrorExpired = CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);
			ICollection<int> expectedJobHistoryErrorsForRetry = CreateJobLevelJobHistoryError(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew);

			//Act
			_jobHistoryErrorManager.StageForUpdatingErrors(job, JobTypeChoices.JobHistoryRetryErrors);

			string startTempTableName = $"{ _JOB_START_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			string completeTempTableName = $"{ _JOB_COMPLETE_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			string otherTempTableName = $"{ _ITEM_START_INCLUDED_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";

			DataTable startTempTable = GetTempTable(startTempTableName);
			DataTable otherTempTable = GetTempTable(otherTempTableName);

			var updateStatusType = new JobHistoryErrorDTO.UpdateStatusType
			{
				ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem
			};

			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, helper, RepositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, updateStatusType);

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, ErrorStatusChoices.JobHistoryErrorInProgress);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorExpired, ErrorStatusChoices.JobHistoryErrorExpired);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, ErrorStatusChoices.JobHistoryErrorRetried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(otherTempTable, otherTempTableName, expectedJobHistoryErrorExpired);
		}

		[Test]
		public void ExpectTempSavedSearchCreatedAndDeleted()
		{
			//Arrange
			var docPrefix = "SavedSearchDoc";
			var expiredDocPrefix = "TempSavedSearchExp";
			DataTable importTable = GetImportTable(1, 100, docPrefix, expiredDocPrefix);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTable);
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, docPrefix, false);

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create item level error
			CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);

			//Act
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, docPrefix, true);
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);

			//Assert
			VerifyTempSavedSearchDeletion(integrationPointCreated.ArtifactID, jobHistory.ArtifactId);
		}

		private void ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(int startingControlNumber, int numberOfDocuments, string documentPrefix, string expiredDocumentPrefix)
		{
			//Arrange
			DataTable importTable = GetImportTable(startingControlNumber, numberOfDocuments, documentPrefix, expiredDocumentPrefix);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTable);
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, documentPrefix, true);
			var stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			var helper = NSubstitute.Substitute.For<IHelper>();

			var integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"JobHistoryErrors{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Create an Integration Point and assign a Job History
			IntegrationPointModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactId)
				.WithRelatedObjectArtifactId(integrationPointCreated.ArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";

			_jobHistoryErrorManager = new JobHistoryErrorManager(RepositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create item level error
			CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);

			IDictionary<int, string> expectedNonExpiredJobHistoryArtifacts = _jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(jobHistory.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			List<int> expectedJobHistoryErrorsForRetry = GetExpectedInprogressAndRetriedErrors(expectedNonExpiredJobHistoryArtifacts);
			List<int> expectedJobHistoryErrorExpired = GetExpectedExpiredErrors(expectedNonExpiredJobHistoryArtifacts);

			//Act
			SavedSearch.ModifySavedSearchByAddingPrefix(RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, documentPrefix, true);
			_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, SavedSearchArtifactId);

			string startTempTableName = $"{ _ITEM_START_INCLUDED_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			string completeTempTableName = $"{ _ITEM_COMPLETE_INCLUDED_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";
			string otherTempTableName = $"{ _ITEM_START_EXCLUDED_TEMP_TABLE_PREFIX }_{ tempTableSuffix }";

			DataTable startTempTable = GetTempTable(startTempTableName);
			DataTable otherTempTable = GetTempTable(otherTempTableName);

			var updateStatusType = new JobHistoryErrorDTO.UpdateStatusType
			{
				ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly
			};
			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, helper, RepositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, updateStatusType);

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, ErrorStatusChoices.JobHistoryErrorInProgress);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorExpired, ErrorStatusChoices.JobHistoryErrorExpired);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, ErrorStatusChoices.JobHistoryErrorRetried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(otherTempTable, otherTempTableName, expectedJobHistoryErrorExpired);
		}

		private DataTable GetImportTable(int startingDocNumber, int numberOfDocuments, string documentPrefix, string expiredDocumentPrefix)
		{
			var table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			int endDocNumber = startingDocNumber + numberOfDocuments - 1;
			int halfDocCount = endDocNumber - (numberOfDocuments / 2);

			for (int index = startingDocNumber; index <= endDocNumber; index++)
			{
				//Tag half the documents with a certain prefix and the other half with another
				if (index > halfDocCount)
				{
					string controlNumber = $"{documentPrefix}{index}";
					table.Rows.Add(controlNumber);
				}
				else
				{
					string controlNumber = $"{expiredDocumentPrefix}{index}";
					table.Rows.Add(controlNumber);
				}
			}
			return table;
		}

		private void ResolveServices()
		{
			RepositoryFactory = Container.Resolve<IRepositoryFactory>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobHistoryErrorRepository = RepositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
		}

		private List<int> CreateItemLevelJobHistoryErrors(int jobHistoryArtifactId, Relativity.Client.DTOs.Choice errorStatus, DataTable importedDocuments)
		{
			var jobHistoryErrors = new List<JobHistoryError>();

			foreach (DataRow dataRow in importedDocuments.Rows)
			{
				var jobHistoryError = new JobHistoryError
				{
					ParentArtifactId = jobHistoryArtifactId,
					JobHistory = jobHistoryArtifactId,
					Name = Guid.NewGuid().ToString(),
					SourceUniqueID = Convert.ToString((object) dataRow["Control Number"]),
					ErrorType = ErrorTypeChoices.JobHistoryErrorItem,
					ErrorStatus = errorStatus,
					Error = "Inserted Error for testing.",
					StackTrace = "Error created from JobHistoryErrorsBatchingTests",
					TimestampUTC = DateTime.Now,
				};

				jobHistoryErrors.Add(jobHistoryError);
			}

			List<int> jobHistoryErrorArtifactIds = CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
			return jobHistoryErrorArtifactIds;
		}

		private List<int> CreateJobLevelJobHistoryError(int jobHistoryArtifactId, Relativity.Client.DTOs.Choice errorStatus)
		{
			var jobHistoryErrors = new List<JobHistoryError>();
			var jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				JobHistory = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				SourceUniqueID = null,
				ErrorType = ErrorTypeChoices.JobHistoryErrorJob,
				ErrorStatus = errorStatus,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from JobHistoryErrorsBatchingTests",
				TimestampUTC = DateTime.Now,
			};

			jobHistoryErrors.Add(jobHistoryError);

			List<int> jobHistoryErrorArtifactIds = CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
			return jobHistoryErrorArtifactIds;
		}

		private DataTable GetTempTable(string tempTableName)
		{
			string query = $"SELECT [ArtifactID] FROM { ClaimsPrincipal.Current.ResourceDBPrepend(SourceWorkspaceArtifactId) }.[{ tempTableName }]";
			try
			{
				DataTable tempTable = CaseContext.SqlContext.ExecuteSqlStatementAsDataTable(query);
				return tempTable;
			}
			catch (Exception ex)
			{
				throw new Exception($"An error occurred trying to query Temp Table:{ tempTableName }. Exception: { ex.Message }");
			}
		}

		private void CompareJobHistoryErrorStatuses(ICollection<int> jobHistoryErrorArtifactIds, Choice expectedErrorStatus)
		{
			IList<JobHistoryError> jobHistoryErrors = _jobHistoryErrorRepository.Read(jobHistoryErrorArtifactIds);
			foreach (JobHistoryError jobHistoryError in jobHistoryErrors)
			{
				if (jobHistoryError.ErrorStatus.Guids.First() != expectedErrorStatus.Guids.First())
				{
					throw new Exception($"Error: JobHistoryError: {jobHistoryError.ArtifactId} has Error Status: { jobHistoryError.ErrorStatus}. Expected Error Status: { expectedErrorStatus }. ");
				}
			}
		}

		private void VerifyTempTableCountAndEntries(DataTable tempTable, string tempTableName, ICollection<int> expectedJobHistoryErrorArtifacts)
		{
			if (tempTable.Rows.Count != expectedJobHistoryErrorArtifacts.Count)
			{
				throw new Exception($"Error: Expected { expectedJobHistoryErrorArtifacts.Count } JobHistoryError ArtifactIds. { tempTable } contains { tempTable.Rows.Count } ArtifactIds.");
			}

			var actualJobHistoryArtifactIds = new List<int>();
			foreach (DataRow dataRow in tempTable.Rows)
			{
				actualJobHistoryArtifactIds.Add(Convert.ToInt32((object) dataRow["ArtifactID"]));
			}

			List<int> discrepancies = expectedJobHistoryErrorArtifacts.Except(actualJobHistoryArtifactIds).ToList();

			if (discrepancies.Count > 0)
			{
				throw new Exception($"Error: { tempTableName } is missing expected JobHistoryError ArtifactIds. ArtifactIds missing: {string.Join(",", expectedJobHistoryErrorArtifacts)}");
			}
		}

		private List<int> GetExpectedInprogressAndRetriedErrors(IDictionary<int, string> errors)
		{
			var inProgressAndRetriedErrors = new List<int>();
			foreach (KeyValuePair<int, string> entry in errors)
			{
				if (entry.Value.Contains("Doc"))
				{
					inProgressAndRetriedErrors.Add(entry.Key);
				}
			}
			return inProgressAndRetriedErrors;
		}

		private List<int> GetExpectedExpiredErrors(IDictionary<int, string> errors)
		{
			var expiredErrors = new List<int>();
			foreach (KeyValuePair<int, string> entry in errors)
			{
				if (entry.Value.Contains("Exp"))
				{
					expiredErrors.Add(entry.Key);
				}
			}
			return expiredErrors;
		}

		private JobHistory CreateJobHistoryOnIntegrationPoint(int integrationPointArtifactId, Guid batchInstance)
		{
			IntegrationPoint integrationPoint = CaseContext.RsapiService.RelativityObjectManager.Read<Data.IntegrationPoint>(integrationPointArtifactId);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);
			jobHistory.EndTimeUTC = DateTime.Now;
			jobHistory.JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors;
			_jobHistoryService.UpdateRdo(jobHistory);
			return jobHistory;
		}

		private void VerifyTempSavedSearchDeletion(int integrationPointArtifactId, int jobHistoryArtifactId)
		{
			string tempSavedSearchName = $"{Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME} - {integrationPointArtifactId} - {jobHistoryArtifactId}";
			var savedSearchQuery = new global::Relativity.Services.Query();
			savedSearchQuery.Condition = $"'Name' EqualTo '{tempSavedSearchName}'";

			using (var proxy = Helper.CreateAdminProxy<IKeywordSearchManager>())
			{
				KeywordSearchQueryResultSet resultSet = proxy.QueryAsync(SourceWorkspaceArtifactId, savedSearchQuery).Result;
				if (resultSet.TotalCount != 0)
				{
					throw new Exception($"Expected temp Saved Search: {tempSavedSearchName} to be deleted after the retry job completed.");
				}
			}
		}
	}
}