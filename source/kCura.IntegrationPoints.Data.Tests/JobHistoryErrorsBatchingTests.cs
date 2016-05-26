using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using kCura.Relativity.Client;
using Relativity.Services.Field;
using Relativity.Services.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace kCura.IntegrationPoints.Data.Tests
{
	public class JobHistoryErrorsBatchingTests : WorkspaceDependentTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private ICaseServiceContext _caseServiceContext;
		private IJobHistoryRepository _jobHistoryRepository;
		private IRepositoryFactory _repositoryFactory;
		private IQueueRepository _queueRepository;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;

		public JobHistoryErrorsBatchingTests() : base("JobHistoryErrorsSource", "JobHistoryErrorsDestination")
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			ResolveServices();
		}

		[Test]
		[Explicit]
		public void ExpectJobHistoryErrorsUpdatedWithErrorsMatchingBatchSize()
		{
			string docPrefix = "EqualBatchDoc";
			string expDocPrefix = "EqualBatchExp";

			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(1000, 1, 2000, docPrefix, expDocPrefix);
		}

		[Test]
		[Explicit]
		public void ExpectJobHistoryErrorsUpdatedWithErrorsUnderBatchSize()
		{
			string docPrefix = "LessThanBatchDoc";
			string expDocPrefix = "LessThanBatchExp";

			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(499, 3000, 999, docPrefix, expDocPrefix);
		}

		[Test]
		[Explicit]
		public void ExpectJobHistoryErrorsUpdatedWithErrorsOverBatchSize()
		{
			string docPrefix = "MoreThanBatchDoc";
			string expDocPrefix = "MoreThanBatchExp";

			ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(1001, 5000, 2001, docPrefix, expDocPrefix);
		}

		[Test]
		[Explicit]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutAJobHistory()
		{
			//Arrange
			bool errorReceived = false;

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
				Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
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
		[Explicit]
		public void ExpectErrorWhenRetryingErrorsOnIpWithoutJobHistoryErrors()
		{
			//Arrange
			bool errorReceived = false;
			string docPrefix = "ErrorScenarioDoc";

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			ModifySavedSearch(docPrefix, docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);

			//Act
			try
			{
				_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
				Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
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
		[Explicit]
		public void ExpectJobHistoryErrorUpdatedForJobLevelErrorWhenBatching()
		{
			//Arrange
			string docPrefix = "JobLevelImport";

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 1, docPrefix, docPrefix));
			ModifySavedSearch(docPrefix, docPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 500);

			//Create job level error
			int jobHistoryArtifactId = _jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointCreated.ArtifactID);
			CreateJobHistoryError(jobHistoryArtifactId, ErrorTypeChoices.JobHistoryErrorJob, ErrorStatusChoices.JobHistoryErrorNew, null);

			//Act
			DateTime startTime = DateTime.Now;
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);

			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, startTime);

			//Get error artifactIds along with their associated doc identifier
			List<int> jobLevelErrors = _jobHistoryErrorRepository.RetrieveJobHistoryErrorArtifactIds(jobHistoryArtifactId,ErrorTypeChoices.JobHistoryErrorJob) as List<int>;
			//List<int> expectedInProgressAndRetryErrors = GetExpectedInprogressAndRetriedErrors(itemLevelErrors);

			//Save temp table
			DataTable inProgressTable = GetTempTable("inprogress");

			//Verify table entries
			//VerifyTempTableEntries(inProgressTable, expectedInProgressAndRetryErrors);

			//Check error statuses here after they are updated.
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate);
			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval, startTime);
			CompareJobHistoryErrorStatuses(inProgressTable, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);

		}

		private void ExpectJobHistoryErrorsUpdatedWithBatchingOnRetry(int batchNumber, int startingControlNumber, int numberOfDocuments, string documentPrefix, string expiredDocumentPrefix)
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(startingControlNumber, numberOfDocuments, documentPrefix, expiredDocumentPrefix));
			ModifySavedSearch(documentPrefix, expiredDocumentPrefix, false);

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
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
				Map = CreateDefaultFieldMap()
			};

			//Cause Item Level Errors by RIPing to the same workspace
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 500);

			int jobHistoryArtifactId = _jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointCreated.ArtifactID);
			//JobHistory jobHistory = _caseServiceContext.RsapiService.JobHistoryLibrary.Read(jobHistoryArtifactId);

			//Exclude documents with Exp prefix to generate expired errors for cross referencing
			ModifySavedSearch(documentPrefix, expiredDocumentPrefix, true);

			//Act
			DateTime startTime = DateTime.Now;
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, startTime);

			//Get error artifactIds along with their associated doc identifier
			IDictionary<int, string> itemLevelErrors = _jobHistoryErrorRepository.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(jobHistoryArtifactId, ErrorTypeChoices.JobHistoryErrorItem);
			List<int> expectedInProgressAndRetryErrors = GetExpectedInprogressAndRetriedErrors(itemLevelErrors);
			List<int> expectedExpiredErrors = GetExpectedExpiredErrors(itemLevelErrors);

			//Save temp tables
			DataTable inProgressTable = GetTempTable("inprogress");
			DataTable expiredTable = GetTempTable("expired");

			//Verify table entries
			VerifyTempTableEntries(inProgressTable, expectedInProgressAndRetryErrors);
			VerifyTempTableEntries(expiredTable, expectedExpiredErrors);

			//Check error statuses here after they are updated.
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate);
			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval, startTime);
			CompareJobHistoryErrorStatuses(inProgressTable, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);
			CompareJobHistoryErrorStatuses(expiredTable, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Expired);

			//Read in the finished temp table. Verify later the table count, and the entries (comparing them to the errors retrieved above)
			startTime = DateTime.Now;
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval);
			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, startTime);
			DataTable retried = GetTempTable("retried");

			VerifyTempTableEntries(retried, expectedInProgressAndRetryErrors);
			CompareJobHistoryErrorStatuses(retried, JobHistoryErrorDTO.Choices.ErrorStatus.Values.InProgress);

			//Check finished error statuses in verification step
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate);
			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);
			CompareJobHistoryErrorStatuses(retried, JobHistoryErrorDTO.Choices.ErrorStatus.Values.Retried);


			//Assert
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate);
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval);
			Assert.AreEqual(inProgressTable.Rows.Count, batchNumber);
			Assert.AreEqual(expiredTable.Rows.Count, batchNumber);
			Assert.AreEqual(retried.Rows.Count, batchNumber);
		}

		private DataTable GetImportTable(int startingDocNumber, int numberOfDocuments, string documentPrefix, string expiredDocumentPrefix)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			int endDocNumber = startingDocNumber + numberOfDocuments - 1;
			int halfDocCount = endDocNumber / 2;

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
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_queueRepository = Container.Resolve<IQueueRepository>();
			_jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactId);
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
		}

		private void CreateJobHistoryError(int jobHistoryArtifactId, Choice errorType, Choice errorStatus, string documentIdentifier)
		{
			JobHistoryError jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				JobHistory = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				ErrorType = errorType,
				ErrorStatus = errorStatus,
				SourceUniqueID = documentIdentifier,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from JobHistoryErrorsBatchingTests",
				TimestampUTC = DateTime.Now
			};

			List<JobHistoryError> jobHistoryErrors = new List<JobHistoryError> { jobHistoryError };
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryErrors);
		}

		private void ModifySavedSearch(string documentPrefix, string expDocumentPrefix, bool excludeExpDocs)
		{
			CriteriaCollection searchCriteria = new CriteriaCollection();

			if (excludeExpDocs)
			{
				Criteria criteria = new Criteria()
				{
					BooleanOperator = BooleanOperatorEnum.None,
					Condition = new CriteriaCondition(new FieldRef("Control Number"), CriteriaConditionEnum.IsLike, documentPrefix),
				};

				searchCriteria.Conditions.Add(criteria);
			}
			else
			{
				Criteria criteria = new Criteria()
				{
					BooleanOperator = BooleanOperatorEnum.Or,
					Condition = new CriteriaCondition(new FieldRef("Control Number"), CriteriaConditionEnum.IsLike, documentPrefix),
				};

				Criteria criteria2 = new Criteria()
				{
					BooleanOperator = BooleanOperatorEnum.None,
					Condition = new CriteriaCondition(new FieldRef("Control Number"), CriteriaConditionEnum.IsLike, expDocumentPrefix),
				};

				searchCriteria.Conditions.Add(criteria);
				searchCriteria.Conditions.Add(criteria2);
			}

			SavedSearch.UpdateSavedSearchCriteria(SourceWorkspaceArtifactId, SavedSearchArtifactId, searchCriteria);
		}

		private DataTable GetTempTable(string tempTableName)
		{
			string query = $"SELECT * FROM EDDSResource.eddsdbo.{ tempTableName }";
			try
			{
				DataTable tempTable = _caseServiceContext.SqlContext.ExecuteSqlStatementAsDataTable(query);
				return tempTable;
			}
			catch (Exception ex)
			{
				throw new Exception($"An error occurred trying to query Temp Table:{ tempTableName }. Exception: { ex.Message }");
			}
		}

		private void CompareJobHistoryErrorStatuses(DataTable tempTable, JobHistoryErrorDTO.Choices.ErrorStatus.Values expectedErrorStatus)
		{
			int[] jobHistoryErrorArtifactIds = new int[ tempTable.Rows.Count ];

			int index = 0;
			foreach (DataRow entry in tempTable.Rows)
			{
				int jobHistoryErrorArtifactId = (int) entry["ArtifactId"];
				jobHistoryErrorArtifactIds[index] = jobHistoryErrorArtifactId;
				index++;
			}

			IList<JobHistoryErrorDTO> jobHistoryErrors = _jobHistoryErrorRepository.Read(jobHistoryErrorArtifactIds);
			foreach (JobHistoryErrorDTO jobHistoryError in jobHistoryErrors)
			{
				if (jobHistoryError.ErrorStatus != expectedErrorStatus)
				{
					throw new Exception($"Error: JobHistoryError: {jobHistoryError.ArtifactId} has Error Status: { jobHistoryError.ErrorStatus}. Expected Error Status: { expectedErrorStatus }. ");
				}
			}
		}

		private void VerifyTempTableEntries(DataTable tempTable, List<int> expectedJobHistoryErrorArtifacts)
		{
			if (tempTable.Rows.Count != expectedJobHistoryErrorArtifacts.Count)
			{
				throw new Exception($"Error: Expected JobHistoryError ArtifactIds count does not match the temp table's count.");
            }

			List<int> actualJobHistoryArtifactIds = (List<int>) tempTable.Rows.Cast<DataRow>().Select(row => (int)row["ArtifactId"]);
			List<int> discrepancies = expectedJobHistoryErrorArtifacts.Except(actualJobHistoryArtifactIds).ToList();
			if (discrepancies.Count > 0)
			{
				throw new Exception($"Error: temp tables is missing expected JobHistoryError ArtifactIds. ArtifactIds missing: {string.Join(",", expectedJobHistoryErrorArtifacts)}");
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
	}
}
