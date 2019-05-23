using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Utility.Extensions;
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
		public void GetTempTable_ShouldThrowIfNoDocumentsInScratchTable()
		{
			//ARRANGE
			Action action = () => GetTempTable(_tableName);

			//ACT && ASSERT
			action.ShouldThrow<Exception>();
		}

		[TestCase(5, 1)]
		[TestCase(10, 4)]
		[TestCase(3, 3)]
		public void CreateScratchTableAndDeleteErroredDocuments(int numDocs, int numDocsWithErrors)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable(1, numDocs));
			Dictionary<int, string> controlNumbersByDocumentIds = GetDocumentIDToControlNumberMapping();
			List<int> documentIDs = controlNumbersByDocumentIds.Keys.ToList();
			int expectedNumberOfDocuments = documentIDs.Count - numDocsWithErrors;

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			IList<int> documentsToRemoveArtifactIDs = documentIDs.Take(numDocsWithErrors).ToList();
			IList<string> documentsToRemoveControlNumbers =
				controlNumbersByDocumentIds
					.Where(x => x.Key.In(documentsToRemoveArtifactIDs.ToArray()))
					.Select(y => y.Value)
					.ToList();

			//ACT
			_sut.RemoveErrorDocuments(documentsToRemoveControlNumbers);
			

			//ASSERT
			AssertErroredDocumentRemoval(_tableName, documentsToRemoveArtifactIDs, expectedNumberOfDocuments);
		}

		[Test]
		public void CreateScratchTableAndErroredDocumentDoesntExist()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();
			List<string> nonExistingDocumentsIdentifiers = new List<string> {"Non Existing Identifier"};

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

			//ACT
			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			_sut.Dispose();

			//ASSERT
			VerifyTableDisposal(_tableName);
		}

		[TestCase(100)]
		public void ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTable(int numDocs)
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadDocumentIDs(0, numDocs);

			//ASSERT
			result.ShouldBeEquivalentTo(documentIDs);
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
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset, numDocs);

			//ASSERT
			result.ShouldBeEquivalentTo(documentsAfterOffseting);
		}

		[TestCase(200)]
		[TestCase(22222)]
		public void ReadDocumentIDs_ShouldRetrieveNotOrderedData(int numDocs)
		{
			//ARRANGE
			int offset = 0;
			var randomNumberGenerator = new Random();
			List<int> documentIDs = Enumerable.Range(0, numDocs).OrderBy(x => randomNumberGenerator.Next()).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadDocumentIDs(offset, numDocs);

			//ASSERT
			result.ShouldBeEquivalentTo(documentIDs);
		}

		[Test]
		public void ReadDocumentIDs_ShouldRetrieveAllDocumentsInBatches()
		{
			//ARRANGE
			int numDocs = 100;
			int offset = 30;

			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			int numberOfBatchIterations = (int) Math.Ceiling((double)numDocs / offset);
			//ACT
			List<int> result = new List<int>();
			for (int i = 0; i < numberOfBatchIterations; i++)
			{
				IEnumerable<int> batchInts = _sut.ReadDocumentIDs(offset*i , offset);
				result.AddRange(batchInts);
			}

			//ASSERT
			result.ShouldBeEquivalentTo(documentIDs);
		}

		[Test]
		[StressTest]
		public void ReadDocumentIDs_StressTest()
		{
			int numDocs = 200000;
			ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTable(numDocs);
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

		private void AssertErroredDocumentRemoval(string tableName, IEnumerable<int> erroredDocumentArtifactID, int expectedNewCount)
		{
			string targetDatabaseFormat = _resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactId);

			if (expectedNewCount != 0)
			{
				string erroredDocumentsListAsString = "(" + String.Join(",", erroredDocumentArtifactID) + ")";
				string getErroredDocumentQuery =
				$"SELECT COUNT(*) FROM {targetDatabaseFormat}.[{tableName}] WHERE [ArtifactID] in {erroredDocumentsListAsString}";

				bool entryExists = _caseServiceContext.SqlContext.ExecuteSqlStatementAsScalar<bool>(getErroredDocumentQuery);
				if (entryExists)
				{
					throw new Exception(
						$"Error: {tableName} still contains Document ArtifactID {erroredDocumentArtifactID}, it should have been removed.");
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