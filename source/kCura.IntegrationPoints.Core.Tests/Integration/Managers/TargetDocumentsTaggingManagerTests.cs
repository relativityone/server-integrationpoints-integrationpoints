using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Managers
{
	[TestFixture]
	[Category("Integration Tests")]
	public class TargetDocumentsTaggingManagerTests : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private IDocumentRepository _documentRepository;
		private SourceWorkspaceManager _sourceWorkspaceManager;
		private SourceJobManager _sourceJobManager;
		private ISynchronizerFactory _synchronizerFactory;
		private IJobHistoryService _jobHistoryService;
		private FieldMap[] _fieldMaps;
		private const int _ADMIN_USER_ID = 9;
		private const string _RELATIVITY_SOURCE_CASE = "Relativity Source Case";
		private const string _RELATIVITY_SOURCE_JOB = "Relativity Source Job";

		public TargetDocumentsTaggingManagerTests() : base("TargetDocumentsTaggingManagerSource", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_sourceWorkspaceManager = new SourceWorkspaceManager(_repositoryFactory);
			_sourceJobManager = new SourceJobManager(_repositoryFactory);
			_synchronizerFactory = Container.Resolve<ISynchronizerFactory>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldMaps = GetDefaultFieldMap();
		}

		[Test]
		[TestCase(499, "UnderBatch")]
		[TestCase(500, "EqualBatch")]
		[TestCase(502, "OverBatch")]
		public void TargetWorkspaceDocumentTagging_GoldFlow(int numberOfDocuments, string documentIdentifier)
		{
			//Act
			string expectedRelativitySourceCase = $"TargetDocumentsTaggingManagerSource - {SourceWorkspaceArtifactId}";
			DataTable dataTable = GetImportTable(documentIdentifier, numberOfDocuments);
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, dataTable);
			List<int> documentArtifactIds = GetDocumentArtifactIdsByIdentifier(documentIdentifier);

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
			IntegrationModel integrationModelCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Data.IntegrationPoint integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationModelCreated.ArtifactID);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(integrationPoint, Guid.NewGuid(), DateTime.Now);

			TargetDocumentsTaggingManagerFactory targetDocumentsTaggingManagerFactory = new TargetDocumentsTaggingManagerFactory(_repositoryFactory, _sourceWorkspaceManager, _sourceJobManager, _documentRepository, _synchronizerFactory, _fieldMaps, integrationModelCreated.SourceConfiguration, integrationModelCreated.Destination, jobHistory.ArtifactId, jobHistory.BatchInstance);
			IConsumeScratchTableBatchStatus targetDocumentsTaggingManager = targetDocumentsTaggingManagerFactory.BuildDocumentsTagger();
			targetDocumentsTaggingManager.ScratchTableRepository.AddArtifactIdsIntoTempTable(documentArtifactIds);

			//Act
			Job job = new Job(SourceWorkspaceArtifactId, integrationModelCreated.ArtifactID, _ADMIN_USER_ID, 1);
			targetDocumentsTaggingManager.OnJobStart(job);
			targetDocumentsTaggingManager.OnJobComplete(job);

			//Assert
			VerifyRelativitySourceJobAndSourceCase(documentArtifactIds, jobHistory.Name, expectedRelativitySourceCase);
	}

		private DataTable GetImportTable(string documentPrefix, int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));

			for (int index = 1; index <= numberOfDocuments; index++)
			{
				string controlNumber = $"{documentPrefix}{index}";
				table.Rows.Add(controlNumber);
			}
			return table;
		}

		private List<int> GetDocumentArtifactIdsByIdentifier(string documentIdentifier)
		{
			List<int> documentArtifactIds = new List<int>();
			string query = $"SELECT [ArtifactID] FROM [Document] WHERE ControlNumber like '{documentIdentifier}%'";

			using (DbDataReader dataReader = CaseContext.SqlContext.ExecuteSqlStatementAsDbDataReader(query))
			{
				while (dataReader.Read())
				{
					documentArtifactIds.Add(dataReader.GetInt32(0));
				}
			}
			return documentArtifactIds;
		}

		private void VerifyRelativitySourceJobAndSourceCase(List<int> documentArtifactIds, string expectedSourceJob, string expectedSourceCase)
		{
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);

			int relativitySourceCaseFieldArtifactId = GetDocumentFieldArtifactId(_RELATIVITY_SOURCE_CASE);
			int relativitySourceJobdArtifactId = GetDocumentFieldArtifactId(_RELATIVITY_SOURCE_JOB);

			ArtifactDTO[] documentArtifacts =
				_documentRepository.RetrieveDocumentsAsync(documentArtifactIds,
					new HashSet<int>() {relativitySourceJobdArtifactId, relativitySourceCaseFieldArtifactId})
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

			foreach (ArtifactDTO artifact in documentArtifacts)
			{
				if (artifact.Fields[0].Value == null || !artifact.Fields[0].Value.ToString().Contains(expectedSourceJob))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Job'. Expected value: {expectedSourceJob}. Actual: {artifact.Fields[1].Value}.");
				}

				if (artifact.Fields[1].Value == null || !artifact.Fields[1].Value.ToString().Contains(expectedSourceCase))
				{
					throw new Exception($"Failed to correctly tag Document field 'Relativity Source Case'. Expected value: {expectedSourceCase}. Actual: {artifact.Fields[0].Value}.");
				}
			}
		}

		private int GetDocumentFieldArtifactId(string fieldName)
		{
			const int documentArtifactTypeId = 10;
			string query = $"SELECT [ArtifactID] FROM [Field] WHERE [DisplayName] = '{fieldName}' AND [FieldArtifactTypeID] = {documentArtifactTypeId}";
			int artifactId = CaseContext.SqlContext.ExecuteSqlStatementAsScalar<int>(query);
			return artifactId;
		}
	}
}
