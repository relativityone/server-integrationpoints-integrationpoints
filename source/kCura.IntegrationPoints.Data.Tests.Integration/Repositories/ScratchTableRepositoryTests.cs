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
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class ScratchTableRepositoryTests : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private ICaseServiceContext _caseServiceContext;
		private IDocumentRepository _documentRepository;
		private IFieldRepository _fieldRepository;
		private IResourceDbProvider _resourceDbProvider;
		private ScratchTableRepository _currentScratchTableRepository;
		private const string _DOC_IDENTIFIER = "SCRATCH_";

		public ScratchTableRepositoryTests() : base("Scratch table", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldRepository = _repositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId);
			_resourceDbProvider = new ResourceDbProvider();
		}

		public override void TestTeardown()
		{
			_currentScratchTableRepository.Dispose();
		}

		[TestCase(2001)]
		[TestCase(999)]
		[TestCase(1000)]
		[TestCase(0)]
		public void CreateScratchTableAndVerifyEntries(int numberOfDocuments)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numberOfDocuments));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			var scratchTableRepository = new ScratchTableRepository(Helper, _documentRepository, _fieldRepository, _resourceDbProvider, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);
			string tableName = $"{tablePrefix}_{tableSuffix}";
			DataTable tempTable = GetTempTable(tableName);

			//ASSERT
			VerifyTempTableCountAndEntries(tempTable, tableName, documentIds);
		}

		[TestCase(5, 1)]
		[TestCase(1500, 1500)]
		public void CreateScratchTableAndDeleteErroredDocuments(int numDocs, int numDocsWithErrors)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numDocs));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			var scratchTableRepository = new ScratchTableRepository(Helper, _documentRepository, _fieldRepository, _resourceDbProvider, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);

			string tableName = $"{tablePrefix}_{tableSuffix}";
			int docArtifactIdToRemove = controlNumbersByDocumentIds.Keys.ElementAt(2);

			if (numDocsWithErrors == 1)
			{
				string docIdentifierToRemove = controlNumbersByDocumentIds[docArtifactIdToRemove];
				scratchTableRepository.RemoveErrorDocuments(new List<string> { docIdentifierToRemove});
			}
			else //all docs have errors
			{
				List<string> docIdentifiers = controlNumbersByDocumentIds.Values.ToList();
				scratchTableRepository.RemoveErrorDocuments(docIdentifiers);
			}

			//ASSERT
			VerifyErroredDocumentRemoval(tableName, docArtifactIdToRemove, documentIds.Count - numDocsWithErrors);
		}

		public void CreateScratchTableAndErroredDocumentDoesntExist()
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			var scratchTableRepository = new ScratchTableRepository(Helper, _documentRepository, _fieldRepository, _resourceDbProvider, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);

			//ASSERT
			try
			{
				scratchTableRepository.RemoveErrorDocuments(new List<string> { "Doesn't Exist" });
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message == "Unable to retrieve Document Artifact ID. Object Query failed.");
			}
		}

		public void CreateScratchTableAndInsertDuplicateEntries()
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			var scratchTableRepository = new ScratchTableRepository(Helper, _documentRepository, _fieldRepository, _resourceDbProvider, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
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
				Assert.IsTrue(ex.Message.Contains("Cannot insert duplicate key in object"));
			}
		}

		public void DeletionOfScratchTable()
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, 5));
			string tablePrefix = "RKO";
			string tableSuffix = Guid.NewGuid().ToString();
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIds = controlNumbersByDocumentIds.Keys.ToList();

			var scratchTableRepository = new ScratchTableRepository(Helper, _documentRepository, _fieldRepository, _resourceDbProvider, tablePrefix, tableSuffix, SourceWorkspaceArtifactId);
			_currentScratchTableRepository = scratchTableRepository;
			string tableName = $"{tablePrefix}_{tableSuffix}";

			//ACT
			scratchTableRepository.AddArtifactIdsIntoTempTable(documentIds);
			scratchTableRepository.Dispose();

			//ASSERT
			VerifyTableDisposal(tableName);
		}

		private DataTable GetTempTable(string tempTableName)
		{
			string query = $"SELECT [ArtifactID] FROM {_resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactId)}.[{ tempTableName }]";
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
				actualJobHistoryArtifactIds.Add(Convert.ToInt32((object) dataRow["ArtifactID"]));
			}

			List<int> discrepancies = expectedDocIds.Except(actualJobHistoryArtifactIds).ToList();

			if (discrepancies.Count > 0)
			{
				throw new Exception($"Error: { tableName } is missing expected Document ArtifactIds. ArtifactIds missing: {string.Join(",", expectedDocIds)}");
			}
		}

		private void VerifyErroredDocumentRemoval(string tableName, int erroredDocumentArtifactId, int expectedNewCount)
		{
			string targetDatabaseFormat = _resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactId);

			if (expectedNewCount != 0)
			{
				string getErroredDocumentQuery =
				$"SELECT COUNT(*) FROM {targetDatabaseFormat}.[{tableName}] WHERE [ArtifactID] = {erroredDocumentArtifactId}";

				bool entryExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(getErroredDocumentQuery);
				if (entryExists)
				{
					throw new Exception(
						$"Error: {tableName} still contains Document ArtifactID {erroredDocumentArtifactId}, it should have been removed.");
				}
			}

			string scratchTableCountQuery = $"SELECT COUNT(*) FROM {targetDatabaseFormat}.[{tableName}]";
			int entryCount = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<int>(scratchTableCountQuery);

			if (entryCount != expectedNewCount)
			{
				throw new Exception($"Error: {tableName} has an incorrect count. Expected: {expectedNewCount}. Actual: {entryCount}");
			}
		}

		private void VerifyTableDisposal(string tableName)
		{
			string query = $@"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

			bool tableExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(query);

			if (tableExists)
			{
				throw new Exception($"Error: {tableName} still exists and was not properly disposed.");
			}
		}
	}
}