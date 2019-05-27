using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;
using Enumerable = System.Linq.Enumerable;

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
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numberOfDocuments).ToList();

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			DataTable tempTable = GetTempTable(_tableName);

			//ASSERT
			VerifyTempTableCountAndEntries(tempTable, _tableName, documentIDs);
		}

		[Test]
		public void GetTempTableShouldThrowIfNoDocumentsInScratchTable()
		{
			//ARRANGE
			Action action = () => GetTempTable(_tableName);
			string expectedErrorMessage = $"An error occurred trying to query Temp Table:{ _tableName }. Exception: SQL Statement Failed";
			//ACT && ASSERT
			action.ShouldThrow<Exception>().Which.Message.Should().Be(expectedErrorMessage);
		}

		[TestCase(5, 1)]
		[TestCase(10, 4)]
		[TestCase(3, 3)]
		public void CreateScratchTableAndDeleteErroredDocuments(int numDocs, int numDocsWithErrors)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numDocs));
			Dictionary<int, string> controlNumbersByDocumentIDs = GetDocumentIDToControlNumberMapping();
			List<int> documentIDs = controlNumbersByDocumentIDs.Keys.ToList();
			int expectedNumberOfDocuments = documentIDs.Count - numDocsWithErrors;

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			IEnumerable<KeyValuePair<int, string>> controlNumbersByDocumentIDsToRemove = controlNumbersByDocumentIDs.Take(numDocsWithErrors);
			IList<string> documentsControlNumbersToRemove = controlNumbersByDocumentIDsToRemove.Select(x => x.Value).ToList();
			IList<int> documentsIDsToRemove = controlNumbersByDocumentIDsToRemove.Select(x => x.Key).ToList();

			//ACT
			_sut.RemoveErrorDocuments(documentsControlNumbersToRemove);

			//ASSERT
			AssertErroredDocumentRemoval(_tableName, documentsIDsToRemove, expectedNumberOfDocuments);
		}

		[Test]
		public void CreateScratchTableAndErroredDocumentDoesntExist()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();
			var nonExistingDocumentsIdentifiers = new List<string> {"Non Existing Identifier"};

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ASSERT
			try
			{
				_sut.RemoveErrorDocuments(nonExistingDocumentsIdentifiers);
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
			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			_sut.Dispose();

			//ASSERT
			VerifyTableDisposal(_tableName);
		}

		[Test]
		public void ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTable()
		{
			//ARRANGE
			const int numDocs = 10;
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset: 0, size: numDocs).ToList();

			//ASSERT
			result.Should().Equal(documentIDs);
		}

		[TestCase(100, 30)]
		[TestCase(100, 101)]
		public void ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTableWithOffset(int numDocs, int offset)
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();
			documentIDs.Sort();
			List<int> documentsAfterOffseting = documentIDs.Skip(offset).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset, numDocs).ToList();

			//ASSERT
			result.Should().Equal(documentsAfterOffseting);
		}

		[Test]
		public void ReadDocumentIDs_ShouldRetrieveNotOrderedData()
		{
			//ARRANGE
			const int numDocs = 200;
			const int offset = 0;
			var randomNumberGenerator = new Random();
			List<int> documentIDs = Enumerable.Range(0, numDocs).OrderBy(x => randomNumberGenerator.Next()).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			documentIDs.Sort();
			//ACT
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset, numDocs).ToList();

			//ASSERT
			result.Should().Equal(documentIDs);
		}

		[TestCase(100, 30)]
		[TestCase(2000000, 3000, Category = TestCategories.STRESS_TEST)]
		public void ReadDocumentIDs_ShouldRetrieveAllDocumentsInBatches(int numDocs, int batchSize)
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			int numberOfBatchIterations = (int) Math.Ceiling((double)numDocs / batchSize);

			//ACT
			List<int> result = Enumerable.Range(0, numberOfBatchIterations)
				.SelectMany(batchNumber => _sut.ReadDocumentIDs(batchSize * batchNumber, batchSize))
				.ToList();

			//ASSERT
			result.Should().Equal(documentIDs);
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

		private Dictionary<int, string> GetDocumentIDToControlNumberMapping()
		{
			string query = "SELECT [ArtifactID], [ControlNumber] FROM [Document]";

			DataTable table = _caseServiceContext.SqlContext.ExecuteSqlStatementAsDataTable(query);

			Dictionary<int, string> controlNumberByDocumentID =
				table.AsEnumerable().ToDictionary(row => row.Field<int>("ArtifactID"),
					row => row.Field<string>("ControlNumber"));

			return controlNumberByDocumentID;
		}

		private void VerifyTempTableCountAndEntries(DataTable tempTable, string tableName, List<int> expectedDocIDs)
		{
			if (tempTable.Rows.Count != expectedDocIDs.Count)
			{
				throw new Exception($"Error: Expected { expectedDocIDs.Count } Document ArtifactIds. { tableName } contains { tempTable.Rows.Count } ArtifactIds.");
			}

			List<int> actualJobHistoryArtifactIDs = new List<int>();
			foreach (DataRow dataRow in tempTable.Rows)
			{
				actualJobHistoryArtifactIDs.Add(Convert.ToInt32((object)dataRow["ArtifactID"]));
			}

			List<int> discrepancies = expectedDocIDs.Except(actualJobHistoryArtifactIDs).ToList();

			if (discrepancies.Count > 0)
			{
				throw new Exception($"Error: { tableName } is missing expected Document ArtifactIds. ArtifactIds missing: {string.Join(",", expectedDocIDs)}");
			}
		}

		private void AssertErroredDocumentRemoval(string tableName, IEnumerable<int> erroredDocumentArtifactIDs, int expectedNewCount)
		{
			string targetDatabaseFormat = _resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactId);

			if (expectedNewCount != 0)
			{
				string erroredDocumentsListAsString = $"({string.Join(",", erroredDocumentArtifactIDs)})";
				string getErroredDocumentQuery =
				$"SELECT COUNT(*) FROM {targetDatabaseFormat}.[{tableName}] WHERE [ArtifactID] in {erroredDocumentsListAsString}";

				bool entryExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(getErroredDocumentQuery);
				if (entryExists)
				{
					throw new Exception(
						$"Error: {tableName} still contains Document ArtifactID {erroredDocumentsListAsString}, it should have been removed.");
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