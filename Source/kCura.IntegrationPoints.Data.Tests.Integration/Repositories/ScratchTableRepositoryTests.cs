﻿using System;
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
using Enumerable = System.Linq.Enumerable;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
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
			_documentRepository = _repositoryFactory.GetDocumentRepository(SourceWorkspaceArtifactID);
			_fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);
			_resourceDbProvider = new ResourceDbProvider(Helper);
		}

		public override void TestTeardown()
		{
			_sut.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
			string tableSuffix = Guid.NewGuid().ToString();
			_tableName = $"{_TABLE_PREFIX}_{tableSuffix}";
			_sut = new ScratchTableRepository(
				new WorkspaceDBContext(
					Helper.GetDBContext(SourceWorkspaceArtifactID)
				),
				_documentRepository,
				_fieldQueryRepository,
				_resourceDbProvider,
				_TABLE_PREFIX,
				tableSuffix,
				SourceWorkspaceArtifactID);
		}

		[IdentifiedTestCase("ba093aef-6cbb-4211-a1fb-5407a31126e6", 2001)]
		[IdentifiedTestCase("604a3327-f219-4cfe-8d7e-86cb84140d22", 999)]
		[IdentifiedTestCase("49110dcb-d94c-4453-ab90-118354531903", 1000)]
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

		[IdentifiedTest("0ec702be-45f1-4237-9f98-1ad2a835fa7b")]
		public void GetTempTableShouldThrowIfNoDocumentsInScratchTable()
		{
			//ARRANGE
			Action action = () => GetTempTable(_tableName);
			string expectedErrorMessage = $"An error occurred trying to query Temp Table:{ _tableName }. Exception: SQL Statement Failed";
			//ACT && ASSERT
			action.Should().Throw<Exception>().Which.Message.Should().Be(expectedErrorMessage);
		}

		[IdentifiedTestCase("fb180ba6-65ef-4d9f-b8aa-f5e11812a2e3", 5, 1)]
		[IdentifiedTestCase("9C725CBD-4E5C-45E2-97FF-85C12EAC8CD0", 10, 4)]
		[IdentifiedTestCase("54bb84de-fe82-4ea6-b432-c48d38431f23", 3, 3)]
		public void CreateScratchTableAndDeleteErroredDocuments(int numDocs, int numDocsWithErrors)
		{
			//ARRANGE
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, GetImportTable(1, numDocs));
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

		[IdentifiedTest("d219e78d-a35d-4c54-9de0-659718987fda")]
		public void CreateScratchTableAndErroredDocumentDoesntExist()
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, _DEFAULT_NUMBER_OF_DOCS_TO_CREATE).ToList();
			var nonExistingDocumentsIdentifiers = new List<string> { "Non Existing Identifier" };

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

		[IdentifiedTest("ceaa43a6-fdf5-4756-90a7-1d7c5825a96c")]
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

		[IdentifiedTest("e9736947-a005-477f-8681-f972ac2d41d5")]
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

		[IdentifiedTest("2804213b-32a7-404f-8866-89d11c01afc7")]
		public void ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTable()
		{
			//ARRANGE
			const int numDocs = 10;
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadArtifactIDs(offset: 0, size: numDocs);

			//ASSERT
			result.Should().Equal(documentIDs);
		}

		[IdentifiedTestCase("bbe320e5-4673-466b-9f82-3064a0d5f5c0", 100, 30)]
		[IdentifiedTestCase("ac9fd15a-6250-44a8-85dc-130e7574bd01", 100, 101)]
		public void ReadDocumentIDs_ShouldRetrieveDocumentIDsFromScratchTableWithOffset(int numDocs, int offset)
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();
			documentIDs.Sort();
			List<int> documentsAfterOffseting = documentIDs.Skip(offset).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);

			//ACT
			IEnumerable<int> result = _sut.ReadArtifactIDs(offset, numDocs);

			//ASSERT
			result.Should().Equal(documentsAfterOffseting);
		}

		[IdentifiedTest("74ac35ca-c5e6-4092-92a5-a58bed7d0370")]
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
			IEnumerable<int> result = _sut.ReadArtifactIDs(offset, numDocs);

			//ASSERT
			result.Should().Equal(documentIDs);
		}

		[IdentifiedTestCase("e4d1f249-df11-4b8e-a8eb-586a14e4c03f", 100, 30)]
		public void ReadDocumentIDs_ShouldRetrieveAllDocumentsInBatches(int numDocs, int batchSize)
		{
			//ARRANGE
			List<int> documentIDs = Enumerable.Range(0, numDocs).ToList();

			_sut.AddArtifactIdsIntoTempTable(documentIDs);
			int numberOfBatchIterations = (int)Math.Ceiling((double)numDocs / batchSize);

			//ACT
			List<int> result = Enumerable.Range(0, numberOfBatchIterations)
				.SelectMany(batchNumber => _sut.ReadArtifactIDs(batchSize * batchNumber, batchSize))
				.ToList();

			//ASSERT
			result.Should().Equal(documentIDs);
		}

		private DataTable GetTempTable(string tempTableName)
		{
			string query = $"SELECT [ArtifactID] FROM {_resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactID)}.[{ tempTableName }]";
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
			string targetDatabaseFormat = _resourceDbProvider.GetResourceDbPrepend(SourceWorkspaceArtifactID);

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