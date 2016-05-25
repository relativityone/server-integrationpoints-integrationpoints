using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using NUnit.Framework;
using kCura.Relativity.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data.Tests
{
	using global::Relativity.Data;
	using global::Relativity.Services.Field;
	using global::Relativity.Services.Search;

	public class JobHistoryErrorsBatchingTests : WorkspaceDependentTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private ICaseServiceContext _caseServiceContext;
		private IJobHistoryRepository _jobHistoryRepository;
		private IRepositoryFactory _repositoryFactory;
		private IQueueRepository _queueRepository;

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
		public void ExpectTempTablesWithExactBatchSize()
		{
			//Arrange
			const int startingControlNumber = 1;
			const int numberOfDocuments = 2000;

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(startingControlNumber, numberOfDocuments));

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = false,
				Name = "JobHistoryErrors" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);


			int jobHistoryArtifactId = _jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointCreated.ArtifactID);
			JobHistory jobHistory = _caseServiceContext.RsapiService.JobHistoryLibrary.Read(jobHistoryArtifactId);

			ModifySavedSearchToExcludeDocs();

			//Act
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);
			kCura.IntegrationPoint.Tests.Core.Injection.EnableInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval, kCura.IntegrationPoint.Tests.Core.Injection.InjectionBehavior.InfiniteLoop, string.Empty, string.Empty);

			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);

			kCura.IntegrationPoint.Tests.Core.Injection.WaitUntilInjectionPointIsReached(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate, DateTime.Now);

			Status.WaitForIntegrationPointJobToComplete(_queueRepository, SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);

			//Assert
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsStatusUpdate);
			kCura.IntegrationPoint.Tests.Core.Injection.RemoveInjectionPoint(InjectionPoints.BeforeJobHistoryErrorsTempTableRemoval);

		}

		private DataTable GetImportTable(int startingDocNumber, int numberOfDocuments)
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
					string controlNumber = $"Exp{index}";
					table.Rows.Add(controlNumber);
				}
				else
				{
					string controlNumber = $"Doc{index}";
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

		private void ModifySavedSearchToExcludeDocs()
		{
			CriteriaCollection searchCriteria = new CriteriaCollection();
			Criteria criteria = new Criteria()
			{
				BooleanOperator = BooleanOperatorEnum.None,
				Condition = new CriteriaCondition(new FieldRef("Control Number"), CriteriaConditionEnum.IsLike, "Exp"),
			};

			searchCriteria.Conditions.Add(criteria);
			SavedSearch.UpdateSavedSearchCriteria(SourceWorkspaceArtifactId, SavedSearchArtifactId, searchCriteria);
		}
	}
}
