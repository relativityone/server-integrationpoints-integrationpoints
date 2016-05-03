using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class TempDocTableHelperTests
	{
		private string tableNameDestWorkspace = "IntegrationPoint_Relativity_DW";
		private string tableNameJobHistory = "IntegrationPoint_Relativity_JH";
		private string _tableSuffix = "12345-6789";
		private int _sourceWorkspaceId = 99999;
		private string _docIdColumn = "ControlNumber";
		private IDBContext _caseContext;
		private IFieldRepository _fieldRepository;
		private IDocumentRepository _documentRepository;
		private Task<ArtifactDTO[]> _successFieldTask;
		private Task<ArtifactDTO> _successDocumentTask;
		private Task<ArtifactDTO> _failedDocumentTask;

		private TempDocTableHelper _instance;
		private IHelper _helper;

		[SetUp]
		public void SetUp()
		{
			_caseContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(_sourceWorkspaceId).Returns(_caseContext);
			_fieldRepository = Substitute.For<IFieldRepository>();
			_documentRepository = Substitute.For<IDocumentRepository>();

			_instance = new TempDocTableHelper(_helper, _tableSuffix, _sourceWorkspaceId, _fieldRepository, _documentRepository, _docIdColumn);

			ArtifactDTO[] fieldArtifacts = CreateArtifactDTOs();
			ArtifactDTO document = new ArtifactDTO(12345, 10, "Document", new ArtifactFieldDTO[] { });

			_successFieldTask = Task<ArtifactDTO[]>.FromResult(fieldArtifacts);
			_successDocumentTask = Task<ArtifactDTO>.FromResult(document);
			_failedDocumentTask = null;
		}

		[Test]
		public void CreateTemporaryDocTable_DestWorkspace_GoldFlow()
		{
			//Arrange
			var artifactIds = new List<int>();
			artifactIds.Add(12345);
			artifactIds.Add(56789);
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", tableNameDestWorkspace + "_" + _tableSuffix, artifactIdList);

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void CreateTemporaryDocTable_JobHistory_GoldFlow()
		{
			//Arrange
			var artifactIds = new List<int>();
			artifactIds.Add(12345);
			artifactIds.Add(56789);
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", tableNameJobHistory + "_" + _tableSuffix, artifactIdList);

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void CreateTemporaryDocTable_EmptyDocumentList()
		{
			//Arrange
			var artifactIds = new List<int>();

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Assert
			_caseContext.DidNotReceive().ExecuteNonQuerySQLStatement(Arg.Any<string>());
		}

		[Test]
		public void DeleteTable_DestinationWorkspace_GoldFlow()
		{
			//Arrange
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", tableNameDestWorkspace + "_" + _tableSuffix);

			//Act
			_instance.DeleteTable(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}

		[Test]
		public void DeleteTable_JobHistory_GoldFlow()
		{
			//Arrange
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", tableNameJobHistory + "_" + _tableSuffix);

			//Act
			_instance.DeleteTable(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}

		[Test]
		public void GetDocumentIdentifierField_GoldFlow()
		{
			//Arrange
			_fieldRepository.RetrieveFieldsAsync(Arg.Any<int>(), Arg.Any<HashSet<string>>()).Returns(_successFieldTask);

			//Act
			string docIdField = _instance.GetDocumentIdentifierField();

			//Assert
			Assert.IsTrue(docIdField == "Control Number X");
			_fieldRepository.Received().RetrieveFieldsAsync(10, Arg.Any<HashSet<string>>());
		}

		[Test]
		public void QueryForDocumentArtifactId_GoldFlow()
		{
			//Arrange
			string docIdentifier = "REL000001";
			_documentRepository.RetrieveDocumentAsync(_docIdColumn, docIdentifier).Returns(_successDocumentTask);

			//Act
			int documentId = _instance.QueryForDocumentArtifactId(docIdentifier);

			//Assert
			Assert.IsTrue(documentId == 12345);
			_documentRepository.Received().RetrieveDocumentAsync(_docIdColumn, docIdentifier);
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void QueryForDocumentArtifactId_ExceptionThrown()
		{
			//Arrange
			string docIdentifier = "REL000001";
			_documentRepository.RetrieveDocumentAsync(_docIdColumn, docIdentifier).Returns(_failedDocumentTask);

			//Act
			_instance.QueryForDocumentArtifactId(docIdentifier);
			_documentRepository.Received().RetrieveDocumentAsync(_docIdColumn, docIdentifier);
		}

		private ArtifactDTO[] CreateArtifactDTOs()
		{
			var artifactFieldName = new ArtifactFieldDTO()
			{
				ArtifactId = 0,
				FieldType = "Text",
				Name = "Name",
				Value = "Control Number X"
			};
			var artifactFieldIdentifier = new ArtifactFieldDTO()
			{
				ArtifactId = 0,
				FieldType = "Text",
				Name = "Is Identifier",
				Value = "1"
			};
			ArtifactFieldDTO[] fieldDTOs = {artifactFieldName, artifactFieldIdentifier};

			var fieldOne = new ArtifactDTO(1, 10, "Document", fieldDTOs);

			var artifactFieldName2 = new ArtifactFieldDTO()
			{
				ArtifactId = 0,
				FieldType = "Text",
				Name = "Name",
				Value = "Not Control Number"
			};
			var artifactFieldIdentifier2 = new ArtifactFieldDTO()
			{
				ArtifactId = 0,
				FieldType = "Text",
				Name = "Is Identifier",
				Value = "0"
			};
			ArtifactFieldDTO[] fieldDTOs2 = {artifactFieldName2, artifactFieldIdentifier2};

			var fieldTwo = new ArtifactDTO(2, 10, "Document", fieldDTOs2);

			return new ArtifactDTO[] {fieldOne, fieldTwo};
		}
	}
}