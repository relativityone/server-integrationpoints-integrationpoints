using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class JobHistoryErrorsBatchingTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private IBatchStatus _batchStatus;
		private const int _ADMIN_USER_ID = 9;
		private const string jobStartTempTablePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		private const string jobCompleteTempTablePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";
		private const string itemStartIncludedTempTablePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart";
		private const string itemStartExcludedTempTablePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart_Excluded";
		private const string itemCompleteIncludedTempTablePrefix = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";

		public JobHistoryErrorsBatchingTests() : base("JobHistoryErrorsSource", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			ResolveServices();
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsMatchingBatchSize()
		{
			string docPrefix = "EqualBatchDoc";
			string expDocPrefix = "EqualBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(1, 2000, docPrefix, expDocPrefix);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsUnderBatchSize()
		{
			string docPrefix = "LessThanBatchDoc";
			string expDocPrefix = "LessThanBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(3000, 1998, docPrefix, expDocPrefix);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectItemLevelJobHistoryErrorsUpdatedWithErrorsOverBatchSize()
		{
			string docPrefix = "MoreThanBatchDoc";
			string expDocPrefix = "MoreThanBatchExp";
			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(5000, 2002, docPrefix, expDocPrefix);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutAJobHistory()
		{
			//Arrange
			bool errorReceived = false;

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			try
			{
				_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
				Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("The integration point cannot be retried as there are no errors to be retried."))
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
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutJobHistoryErrors()
		{
			//Arrange
			bool errorReceived = false;
			string docPrefix = "ErrorScenarioDoc";

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			ModifySavedSearch(docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID);

			//Act
			try
			{
				_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
				Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("The integration point cannot be retried as there are no errors to be retried."))
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
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectJobLevelJobHistoryErrorUpdatedForJobLevelErrorWhenBatching()
		{
			//Arrange
			string docPrefix = "JobLevelImport";
			IJobStopManager stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			IHelper helper = NSubstitute.Substitute.For<IHelper>();
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			ModifySavedSearch(docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap()
			};

			//Create an Integration Point and assign a Job History
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID, 1);
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";
			_jobHistoryErrorManager = new JobHistoryErrorManager(_repositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create job level error
			List<int> expectedJobHistoryErrorArtifactIds = CreateJobLevelJobHistoryError(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew);

			//Act
			_jobHistoryErrorManager.StageForUpdatingErrors(job, JobTypeChoices.JobHistoryRetryErrors);

			string startTempTableName = $"{ jobStartTempTablePrefix }_{ tempTableSuffix }";
			string completeTempTableName = $"{ jobCompleteTempTablePrefix }_{ tempTableSuffix }";
			DataTable startTempTable = GetTempTable(startTempTableName);
			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, new JobHistoryErrorDTO.UpdateStatusType());

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorArtifactIds, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorArtifactIds, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Retried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorArtifactIds);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorArtifactIds);
		}

		[Test]
		[Ignore("Test doesn't work and needs fix")]
		public void ExpectJobandItemLevelJobHistoryErrorsUpdatedWhenBatching()
		{
			//Arrange
			string docPrefix = "DocForItemAndJob";
			string expiredDocPrefix = "ExpForItemAndJob";
			DataTable importTable = GetImportTable(8000, 1000, docPrefix, expiredDocPrefix);
			IJobStopManager stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			IHelper helper = NSubstitute.Substitute.For<IHelper>();

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTable);
			ModifySavedSearch(docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap()
			};

			//Create an Integration Point and assign a Job History
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID, 1);
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";

			_jobHistoryErrorManager = new JobHistoryErrorManager(_repositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create item level error
			ICollection<int> expectedJobHistoryErrorExpired = CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);
			ICollection<int> expectedJobHistoryErrorsForRetry = CreateJobLevelJobHistoryError(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew);

			//Act
			_jobHistoryErrorManager.StageForUpdatingErrors(job, JobTypeChoices.JobHistoryRetryErrors);

			string startTempTableName = $"{ jobStartTempTablePrefix }_{ tempTableSuffix }";
			string completeTempTableName = $"{ jobCompleteTempTablePrefix }_{ tempTableSuffix }";
			string otherTempTableName = $"{ itemStartIncludedTempTablePrefix }_{ tempTableSuffix }";

			DataTable startTempTable = GetTempTable(startTempTableName);
			DataTable otherTempTable = GetTempTable(otherTempTableName);

			JobHistoryErrorDTO.UpdateStatusType updateStatusType = new JobHistoryErrorDTO.UpdateStatusType
			{
				ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.JobAndItem
			};

			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, updateStatusType);

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorExpired, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Expired);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Retried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(otherTempTable, otherTempTableName, expectedJobHistoryErrorExpired);
		}

		[Test]
		public void ExpectTempSavedSearchCreatedAndDeleted()
		{
			//Arrange
			string docPrefix = "SavedSearchDoc";
			string expiredDocPrefix = "TempSavedSearchExp";
			DataTable importTable = GetImportTable(1, 1000, docPrefix, expiredDocPrefix);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, importTable);
			ModifySavedSearch(docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap()
			};

			//Create an Integration Point and assign a Job History
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create item level error
			CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);

			//Act
			ModifySavedSearch(docPrefix, true);
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
			ModifySavedSearch(documentPrefix, true);
			IJobStopManager stopJobManager = NSubstitute.Substitute.For<IJobStopManager>();
			IHelper helper = NSubstitute.Substitute.For<IHelper>();

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap()
			};

			//Create an Integration Point and assign a Job History
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			JobHistory jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);

			//Create Job and temp table suffix
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, _ADMIN_USER_ID, 1);
			string tempTableSuffix = $"{ job.JobId }_{ batchInstance }";

			_jobHistoryErrorManager = new JobHistoryErrorManager(_repositoryFactory, helper, SourceWorkspaceArtifactId, tempTableSuffix);

			//Create item level error
			CreateItemLevelJobHistoryErrors(jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, importTable);

			IDictionary<int, string> expectedNonExpiredJobHistoryArtifacts = _jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(jobHistory.ArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values.Item);
			List<int> expectedJobHistoryErrorsForRetry = GetExpectedInprogressAndRetriedErrors(expectedNonExpiredJobHistoryArtifacts);
			List<int> expectedJobHistoryErrorExpired = GetExpectedExpiredErrors(expectedNonExpiredJobHistoryArtifacts);

			//Act
			ModifySavedSearch(documentPrefix, true);
			_jobHistoryErrorManager.CreateErrorListTempTablesForItemLevelErrors(job, SavedSearchArtifactId);

			string startTempTableName = $"{ itemStartIncludedTempTablePrefix }_{ tempTableSuffix }";
			string completeTempTableName = $"{ itemCompleteIncludedTempTablePrefix }_{ tempTableSuffix }";
			string otherTempTableName = $"{ itemStartExcludedTempTablePrefix }_{ tempTableSuffix }";

			DataTable startTempTable = GetTempTable(startTempTableName);
			DataTable otherTempTable = GetTempTable(otherTempTableName);

			JobHistoryErrorDTO.UpdateStatusType updateStatusType = new JobHistoryErrorDTO.UpdateStatusType
			{
				ErrorTypes = JobHistoryErrorDTO.UpdateStatusType.ErrorTypesChoices.ItemOnly
			};
			_batchStatus = new JobHistoryErrorBatchUpdateManager(_jobHistoryErrorManager, _repositoryFactory, new OnBehalfOfUserClaimsPrincipalFactory(), stopJobManager, SourceWorkspaceArtifactId, _ADMIN_USER_ID, updateStatusType);

			_batchStatus.OnJobStart(job);
			DataTable completedTempTable = GetTempTable(completeTempTableName);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorExpired, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Expired);

			_batchStatus.OnJobComplete(job);
			CompareJobHistoryErrorStatuses(expectedJobHistoryErrorsForRetry, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Retried);

			//Assert
			VerifyTempTableCountAndEntries(startTempTable, startTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(completedTempTable, completeTempTableName, expectedJobHistoryErrorsForRetry);
			VerifyTempTableCountAndEntries(otherTempTable, otherTempTableName, expectedJobHistoryErrorExpired);
		}

		private DataTable GetImportTable(int startingDocNumber, int numberOfDocuments, string documentPrefix, string expiredDocumentPrefix)
		{
			DataTable table = new DataTable();
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
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
		}

		private List<int> CreateItemLevelJobHistoryErrors(int jobHistoryArtifactId, Relativity.Client.DTOs.Choice errorStatus, DataTable importedDocuments)
		{
			List<JobHistoryError> jobHistoryErrors = new List<JobHistoryError>();

			foreach (DataRow dataRow in importedDocuments.Rows)
			{
				JobHistoryError jobHistoryError = new JobHistoryError
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
			List<JobHistoryError> jobHistoryErrors = new List<JobHistoryError>();
			JobHistoryError jobHistoryError = new JobHistoryError
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

		private void ModifySavedSearch(string documentPrefix, bool excludeExpDocs)
		{
			IFieldRepository sourceFieldRepository = _repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			int controlNumberFieldArtifactId = sourceFieldRepository.RetrieveTheIdentifierField((int)ArtifactType.Document).ArtifactId;

			FieldRef fieldRef = new FieldRef(controlNumberFieldArtifactId)
			{
				Name = "Control Number",
				Guids = new List<Guid>() { new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438") }
			};

			CriteriaCollection searchCriteria = new CriteriaCollection();

			if (excludeExpDocs)
			{
				Criteria criteria = new Criteria()
				{
					BooleanOperator = BooleanOperatorEnum.None,
					Condition = new CriteriaCondition(fieldRef, CriteriaConditionEnum.IsLike, documentPrefix),
				};

				searchCriteria.Conditions.Add(criteria);
			}
			SavedSearch.UpdateSavedSearchCriteria(SourceWorkspaceArtifactId, SavedSearchArtifactId, searchCriteria);
		}

		private DataTable GetTempTable(string tempTableName)
		{
			string query = $"SELECT [ArtifactID] FROM [Resource].[{ tempTableName }]";
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

		private void CompareJobHistoryErrorStatuses(ICollection<int> jobHistoryErrorArtifactIds, JobHistoryErrorDTO.Choices.ErrorStatus.Values expectedErrorStatus)
		{
			IList<JobHistoryErrorDTO> jobHistoryErrors = _jobHistoryErrorRepository.Read(jobHistoryErrorArtifactIds);
			foreach (JobHistoryErrorDTO jobHistoryError in jobHistoryErrors)
			{
				if (jobHistoryError.ErrorStatus != expectedErrorStatus)
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

			List<int> actualJobHistoryArtifactIds = new List<int>();
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
			List<int> inProgressAndRetriedErrors = new List<int>();
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
			List<int> expiredErrors = new List<int>();
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
			IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointArtifactId);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, batchInstance, JobTypeChoices.JobHistoryRun, DateTime.Now);
			jobHistory.EndTimeUTC = DateTime.Now;
			jobHistory.JobStatus = JobStatusChoices.JobHistoryCompletedWithErrors;
			_jobHistoryService.UpdateRdo(jobHistory);
			return jobHistory;
		}

		private void VerifyTempSavedSearchDeletion(int integrationPointArtifactId, int jobHistoryArtifactId)
		{
			string tempSavedSearchName = $"{Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME} - {integrationPointArtifactId} - {jobHistoryArtifactId}";
			global::Relativity.Services.Query savedSearchQuery = new global::Relativity.Services.Query();
			savedSearchQuery.Condition = $"'Name' EqualTo '{tempSavedSearchName}'";

			using (IKeywordSearchManager proxy = Kepler.CreateProxy<IKeywordSearchManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
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