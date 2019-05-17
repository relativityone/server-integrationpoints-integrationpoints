using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class ScratchTableRepositoryTests : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private ICaseServiceContext _caseServiceContext;
		private IDocumentRepository _documentRepository;
		private IFieldQueryRepository _fieldQueryRepository;
		private IResourceDbProvider _resourceDbProvider;
		private ScratchTableRepository _sut;
		private string _tableName;
		private const string _DOC_IDENTIFIER = "SCRATCH_";
		private const string _TABLE_PREFIX = "RKO";
		private const int _DEFAULT_NUMBER_OF_DOCS_TO_CREATE = 100;

		public ScratchTableRepositoryTests() : base("Scratch table", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactId);
			_fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactId);
			_resourceDbProvider = new ResourceDbProvider();
		}

		public override void TestTeardown()
		{
			_sut.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
			string tableSuffix =  Guid.NewGuid().ToString();
			_tableName =  $"{_TABLE_PREFIX}_{tableSuffix}";
			_sut = new ScratchTableRepository(
				new WorkspaceDBContext(
					Helper.GetDBContext(SourceWorkspaceArtifactId)
				), 
				_documentRepository, 
				_fieldQueryRepository, 
				_resourceDbProvider,
				_TABLE_PREFIX, 
				tableSuffix, 
				SourceWorkspaceArtifactId);
		}

		[TestCase(2001)]
		[TestCase(999)]
		[TestCase(1000)]
		public void CreateScratchTableAndVerifyEntries(int numberOfDocuments)
		{
			List<int> documentIDs = Enumerable.Range(0, numberOfDocuments).ToList(); //controlNumbersByDocumentIds.Keys.ToList();

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			DataTable tempTable = GetTempTable(_tableName);

			//ASSERT
			VerifyTempTableCountAndEntries(tempTable, _tableName, documentIDs);
		}

		[Test]
		public void GetTempTable_ShouldThrowIfNoDocumentsInScratchTable()
		{
			Assert.Throws<Exception>(() => GetTempTable(_tableName));
		}

		[TestCase(5, 1)]
		[TestCase(1500, 1500)]
		public void CreateScratchTableAndDeleteErroredDocuments(int numDocs, int numDocsWithErrors)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numDocs));
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIdToControlNumberMapping();
			List<int> documentIDs = controlNumbersByDocumentIds.Keys.ToList();

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			int docArtifactIdToRemove = controlNumbersByDocumentIds.Keys.ElementAt(2);

			if (numDocsWithErrors == 1)
			{
				string docIdentifierToRemove = controlNumbersByDocumentIds[docArtifactIdToRemove];
				_sut.RemoveErrorDocuments(new List<string> { docIdentifierToRemove });
			}
			else //all docs have errors
			{
				List<string> docIdentifiers = controlNumbersByDocumentIds.Values.ToList();
				_sut.RemoveErrorDocuments(docIdentifiers);
			}

			//ASSERT
			VerifyErroredDocumentRemoval(_tableName, docArtifactIdToRemove, documentIDs.Count - numDocsWithErrors);
		}

		[Test]
		public void CreateScratchTableAndErroredDocumentDoesntExist()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList(); 

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ASSERT
			try
			{
				_sut.RemoveErrorDocuments(new List<string> { "Doesn't Exist" });
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message == "Unable to retrieve Document Artifact ID. Object Query failed.");
			}
		}

		[Test]
		public void CreateScratchTableAndInsertDuplicateEntries()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ASSERT
			try
			{
				_sut.AddArtifactIdsIntoTempTable(documentIDs);
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message.Contains("Cannot insert duplicate key in object"));
			}
		}

		[Test]
		public void DeletionOfScratchTable()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			_sut.Dispose();

			//ASSERT
			VerifyTableDisposal(_tableName);
		}

		[Test]
		public void GetDocumentIdsFromTable_ShouldRetrieveDocumentIDsFromScratchTable()
		{
			//arrange
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//act
			IEnumerable<int> result = _sut.ReadDocumentIDs(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE);

			//assert
			result.ShouldBeEquivalentTo(documentIDs);
		}

		[Test]
		public void GetDocumentIdsFromTable_ShouldRetrieveDocumentIDsFromScratchTableWithOffset()
		{
			//arrange
			int numDocs = 100;
			int offset = 30;
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();
			documentIDs.Sort();
			List<int> documentsAfterOffseting = documentIDs.Skip(offset).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//act
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset, numDocs);

			//assert
			result.ShouldBeEquivalentTo(documentsAfterOffseting);
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
				actualJobHistoryArtifactIds.Add(Convert.ToInt32((object)dataRow["ArtifactID"]));
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