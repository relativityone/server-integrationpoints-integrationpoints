using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Toggle;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Explicit]
	public class ScratchTableRepositoryTests : WorkspaceDependentTemplate
	{
		private IExtendedRelativityToggle _toggle;
		private IRepositoryFactory _repositoryFactory;
		private ICaseServiceContext _caseServiceContext;
		private ICaseServiceContext _mockedCaseServiceContext;
		private IDocumentRepository _documentRepository;
		private IFieldRepository _fieldRepository;
		private ScratchTableRepository _currentScratchTableRepository;
		private const string _DOC_IDENTIFIER = "SCRATCH_";

		public ScratchTableRepositoryTests() : base("Scratch table", null)
		{
		}

		[TestFixtureSetUp]
		[Explicit]
		public override void SetUp()
		{
			base.SetUp();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldRepository = _repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);

			_toggle = Substitute.For<IExtendedRelativityToggle>();
		}

		[TestCase(true, 2001)]
		[TestCase(false, 999)]
		[TestCase(true, 1000)]
		[TestCase(false, 1000)]
		[TestCase(true, 0)]
		public void CreateScratchTableAndVerifyEntries(bool useEDDSResource, int numberOfDocuments)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numberOfDocuments));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			_toggle.IsAOAGFeatureEnabled().Returns(!useEDDSResource);

			var scratchTableRepository = new ScratchTableRepository(Helper, _toggle, _documentRepository, _fieldRepository, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);
			string tableName = useEDDSResource? $"{tablePrefix}_{tableSuffix}" : $"EDDSResource_{tablePrefix}_{tableSuffix}";
			DataTable tempTable = GetTempTable(tableName, useEDDSResource);

			//ASSERT
			VerifyTempTableCountAndEntries(tempTable, tableName, documentIds);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CreateScratchTableAndDeleteErroredDocuments(bool useEDDSResource)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			_toggle.IsAOAGFeatureEnabled().Returns(!useEDDSResource);

			var scratchTableRepository = new ScratchTableRepository(Helper, _toggle, _documentRepository, _fieldRepository, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);

			string tableName = useEDDSResource ? $"{tablePrefix}_{tableSuffix}" : $"EDDSResource_{tablePrefix}_{tableSuffix}";
			int docArtifactIdToRemove = controlNumbersByDocumentIds.Keys.ElementAt(2);
			string docIdentifierToRemove = controlNumbersByDocumentIds[docArtifactIdToRemove];

			scratchTableRepository.RemoveErrorDocument(docIdentifierToRemove);

			//ASSERT
			VerifyErroredDocumentRemoval(tableName, docArtifactIdToRemove, documentIds.Count - 1, useEDDSResource);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CreateScratchTableAndErroredDocumentDoesntExist(bool useEDDSResource)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			_toggle.IsAOAGFeatureEnabled().Returns(!useEDDSResource);

			var scratchTableRepository = new ScratchTableRepository(Helper, _toggle, _documentRepository, _fieldRepository, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);

			//ASSERT
			try
			{
				scratchTableRepository.RemoveErrorDocument("Doesn't Exist");
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message == "Unable to retrieve Document Artifact ID. Object Query failed.");
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CreateScratchTableAndInsertDuplicateEntries(bool useEDDSResource)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			_toggle.IsAOAGFeatureEnabled().Returns(!useEDDSResource);

			var scratchTableRepository = new ScratchTableRepository(Helper, _toggle, _documentRepository, _fieldRepository, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);

			//ASSERT
			try
			{
				scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.InnerException.Message.Contains("Cannot insert duplicate key in object"));
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void DeletionOfScratchTable(bool useEDDSResource)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			_toggle.IsAOAGFeatureEnabled().Returns(!useEDDSResource);

			var scratchTableRepository = new ScratchTableRepository(Helper, _toggle, _documentRepository, _fieldRepository, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;
			string tableName = useEDDSResource ? $"{tablePrefix}_{tableSuffix}" : $"EDDSResource_{tablePrefix}_{tableSuffix}";

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);
			scratchTableRepository.Dispose();
			
			//ASSERT
			VerifyTableDisposal(tableName, useEDDSResource);
		}

		private DataTable GetTempTable(string tempTableName, bool isScratchTableOnEDDSResource)
		{
			string targetDatabaseFormat = isScratchTableOnEDDSResource ? "[EDDSResource].." : "[Resource].";
			string query = $"SELECT [ArtifactID] FROM {targetDatabaseFormat}[{ tempTableName }]";
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

		private DataTable GetImportTable(int startingDocNumber, int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			int endDocNumber = startingDocNumber + numberOfDocuments - 1;

			for (int index = startingDocNumber; index <= endDocNumber; index++)
			{
				string controlNumber = $"{_DOC_IDENTIFIER}{index}";
				table.Rows.Add(controlNumber);
			}
			return table;
		}

		private Dictionary<int, string> GetDocumentIdToControlNumberMapping()
		{
			string query = "SELECT [ArtifactID], [ControlNumber] FROM [Document]";

			DataTable table = _caseServiceContext.SqlContext.ExecuteSqlStatementAsDataTable(query);

			Dictionary<int, string> controlNumberByDocumentId =
				table.AsEnumerable().ToDictionary(row => row.Field<int>("ArtifactID"),
					row => row.Field<string>("ControlNumber"));

			return controlNumberByDocumentId;
		}

		private void VerifyTempTableCountAndEntries(DataTable tempTable, string tableName, List<int> expectedDocIds)
		{
			if (tempTable.Rows.Count != expectedDocIds.Count)
			{
				throw new Exception($"Error: Expected { expectedDocIds.Count } Document ArtifactIds. { tableName } contains { tempTable.Rows.Count } ArtifactIds.");
			}

			List<int> actualJobHistoryArtifactIds = new List<int>();
			foreach (DataRow dataRow in tempTable.Rows)
			{
				actualJobHistoryArtifactIds.Add(Convert.ToInt32(dataRow["ArtifactID"]));
			}

			List<int> discrepancies = expectedDocIds.Except(actualJobHistoryArtifactIds).ToList();

			if (discrepancies.Count > 0)
			{
				throw new Exception($"Error: { tableName } is missing expected Document ArtifactIds. ArtifactIds missing: {string.Join(",", expectedDocIds)}");
			}
		}

		private void VerifyErroredDocumentRemoval(string tableName, int erroredDocumentArtifactId, int newCount, bool isScratchTableOnEDDSResource)
		{
			string targetDatabaseFormat = isScratchTableOnEDDSResource ? "[EDDSResource].." : "[Resource].";
			string getErroredDocumentQuery =
				$"SELECT COUNT(*) FROM {targetDatabaseFormat}[{tableName}] WHERE [ArtifactID] = {erroredDocumentArtifactId}";

			bool entryExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(getErroredDocumentQuery);
			if (entryExists)
			{
				throw new Exception(
					$"Error: {tableName} still contains Document ArtifactID {erroredDocumentArtifactId}, it should have been removed.");
			}

			string scratchTableCountQuery = $"SELECT COUNT(*) FROM {targetDatabaseFormat}[{tableName}]";
			int entryCount = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<int>(scratchTableCountQuery);

			if (entryCount != newCount)
			{
				throw new Exception($"Error: {tableName} has an incorrect count. Expected: {newCount}. Actual: {entryCount}");
			}
		}


		private void VerifyTableDisposal(string tableName, bool isScratchTableOnEDDSResource)
		{
			string targetDatabaseFormat = isScratchTableOnEDDSResource ? "[EDDSResource]." : String.Empty;
			string query = $@"SELECT COUNT(*) FROM {targetDatabaseFormat}INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

			bool tableExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(query);

			if (tableExists)
			{
				throw new Exception($"Error: {tableName} still exists and was not properly disposed.");
			}
		}

		[TearDown]
		public void TestTearDown()
		{
			_currentScratchTableRepository.Dispose();
		}
	}
}