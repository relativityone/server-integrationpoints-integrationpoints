using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.IntegrationPoints.DocumentTransferProvider.Tests.Helpers;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Unit
{
	[TestFixture]
	public class DocumentTransferDataReaderTests
	{
		private IRDORepository _rdoRepository;
		private IDataReader _instance;

		const int _DOCUMENT_ARTIFACTID = 123423;
		const string _FIELD_NAME = "DispName";
		const int _FIELD_IDENTIFIER = 123;

		[SetUp]
		public void SetUp()
		{
			_rdoRepository = NSubstitute.Substitute.For<IRDORepository>();
		}

		#region Read
		[Test]
		public void Read_FirstRead_RunsSavedSearch_ReturnsTrue()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			}, new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)}
				}
			};

			//_rdoRepository.RetrieveAsync(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsTrue(result, "There are records to read, result should be true");
			Assert.IsFalse(_instance.IsClosed, "The reader should be open");
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_NoResults_ReturnsFalse()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			}, new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

			////_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_RequestFails_ReturnsFalse()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = false,
				Results = new List<Result<Document>>()
			};

			////_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_RequestFailsWithException_ReturnsFalse()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Throws(new Exception());

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_ReadAllResults_GoldFlow()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool result1 = _instance.Read();
			bool result2 = _instance.Read();
			bool result3 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsTrue(result2, "There are records to read, result should be true");
			Assert.IsFalse(result3, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_ReadSomeResultsThenClose_GoldFlow()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool result1 = _instance.Read();
			_instance.Close();
			bool result2 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsFalse(result2, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_NoFields_DoesNotFail()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new FieldEntry[0],new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			bool result = _instance.Read();

			// Assert
//			_rdoRepository
//				.Received(1)
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>());
		}

		[Test]
		public void Read_NoDocumentIds_DoesNotFail()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new int[0], new[]
			{
				new FieldEntry() {DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			bool result = _instance.Read();

//			// Assert
//			_rdoRepository
//				.Received(1)
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>());
		}

		[Test]
		public void Read_NoDocumentIdsNoFields_DoesNotFail()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new int[0], new FieldEntry[0],new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			bool result = _instance.Read();

			// Assert
//			_rdoRepository
//				.Received(1)
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>());
		}

		#endregion

		#region IDataReader methods
		[Test]
		public void IsDBNull_ResultNotNull_ReturnsFalse()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(123)
								{
									Name = fieldName,
									Value = 999
								}
							}
						}
					},
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			bool isDbNull = _instance.IsDBNull(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsFalse(isDbNull, "The result should not be DBNull");
		}

		[Test]
		public void GetFieldType_ReturnsString()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			}, new QueryDataItemResult[0]);

			// Act
			Type result = _instance.GetFieldType(0);

			// Assert
			Assert.AreEqual(result, typeof(string), "The types should match" );
		}

		[Test]
		public void GetFieldTypeName_ReturnsString()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			// Act
			string result = _instance.GetDataTypeName(0);

			// Assert
			Assert.AreEqual(result, typeof(string).Name, "The types should match");
		}

		[Test]
		public void NextResult_ReturnsFalse()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			// Act
			bool result = _instance.NextResult();

			// Assert
			Assert.IsFalse(result, "NextResult() should return false");
		}

		[Test]
		public void Depth_ReturnsZero()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			// Act
			int result = _instance.Depth;

			// Assert
			Assert.AreEqual(0, result, "Depth should return 0");
		}

		[Test]
		public void RecordsAffected_ReturnsNegativeOne()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			// Act
			int result = _instance.RecordsAffected;

			// Assert
			Assert.AreEqual(-1, result, "RecordsAffected should alwayds return -1");
		}

		[Test]
		public void GetName_FieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual("123", fieldName, "The field loopup should succeed");
		}

		[Test]
		public void GetName_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName" + Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual("123", fieldName, "The field loopup should succeed");
		}

		[Test]
		public void GetOrdinal_FieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			int ordinal = _instance.GetOrdinal("123");

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void GetOrdinal_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName" + Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			int ordinal = _instance.GetOrdinal("123");

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void ThisAccessor_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName" + Shared.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT, FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL001"}}}},
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL002"}}}},
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			_instance.Read();
			object result = _instance["123"];

			// Assert
			Assert.AreEqual("REL001", result.ToString(), "The result should be correct");
		}

		[Test]
		public void ThisAccessor_FieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL001"}}}},
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL002"}}}},
				}
			};

			//_rdoRepository.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			_instance.Read();
			object result = _instance["123"];

			// Assert
			Assert.AreEqual("REL001", result.ToString(), "The result should be correct");
		}

		[Test]
		public void FieldCount_ReturnsCorrectCount()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "1233"}
			} ,new QueryDataItemResult[0]);

			// Act
			int fieldCount = _instance.FieldCount;

			// Assert
			Assert.AreEqual(2, fieldCount, "There should be 2 fields");
		}

		[Test]
		public void Dispose_BeforeRead_DoesNotExcept()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "1233"}
			},new QueryDataItemResult[0]);

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Dispose();
			}
			catch
			{
				exceptionThrown = true;
			}

			Assert.IsFalse(exceptionThrown, "Dispose() should not except");
		}

		[Test]
		public void Dispose_WhileReaderIsOpen_DoesNotExcept()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "1233"}
			},new QueryDataItemResult[0]);

			// Act
			bool exceptionThrown = false;
			try
			{
				_instance.Read();
				_instance.Dispose();
			}
			catch
			{
				exceptionThrown = true;
			}

			Assert.IsFalse(exceptionThrown, "Dispose() should not except");
		}

		[Test]
		public void Close_ReaderIsClosed()
		{
			// Arrange
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			_instance.Close();
			bool isClosed = _instance.IsClosed;

			// Assert
			Assert.IsTrue(isClosed, "The reader should be closed");
		}

		[Test]
		public void Close_ReadThenCloseThenRead_ReaderIsClosed()
		{
			// Arrange
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
			},new QueryDataItemResult[0]); 

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			_instance.Close();
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "The reader should be closed");
		}

		[Test]
		public void Close_ReadThenCloseThenRead_QueryIsNotRerun()
		{
			// Arrange
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
			}, new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			_instance.Close();
			_instance.Read();

			// Assert
//			_rdoRepository
//				.Received(1)
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>());
		}

		[Test]
		public void Close_ReadThenClose_CannotAccessDocument()
		{
			// Arrange
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			_instance.Close();

			bool correctExceptionThrown = false;
			try
			{
				object result = _instance[0];
			}
			catch (NullReferenceException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// in case another exception is thrown
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "Reading after running Close() should nullify the current result");
		}

		[Test]
		public void GetSchemaTable_OneField_ReturnsCorrectSchema()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

			var expectedResult = new DataTable() { Columns = { new DataColumn("123")}};

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_MultipleFields_ReturnsCorrectSchema()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "456"},
			},new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

			var expectedResult = new DataTable() { Columns = { new DataColumn("123"), new DataColumn("456") } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_NoFields_ReturnsCorrectSchema()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new FieldEntry[0],new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

			var expectedResult = new DataTable() { Columns = { } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_NoDocumentsNoFields_ReturnsCorrectSchema()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new int[] { }, new FieldEntry[0],new QueryDataItemResult[0]);

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

			var expectedResult = new DataTable() { Columns = { } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}
		#endregion

		#region Gets

		[Test]
		public void GetString_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			string value = "999";

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			string result = _instance.GetString(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetInt64_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			Int64 value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			Int64 result = _instance.GetInt64(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetInt16_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			Int16 value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			Int16 result = _instance.GetInt16(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetInt32_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			Int32 value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			Int32 result = _instance.GetInt32(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetGuid_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			Guid value = Guid.NewGuid();

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			Guid result = _instance.GetGuid(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetFloat_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			float value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			float result = _instance.GetFloat(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetDouble_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			double value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			double result = _instance.GetDouble(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetDecimal_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			Decimal value = 999;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			Decimal result = _instance.GetDecimal(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetDateTime_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			DateTime value = DateTime.Now;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			DateTime result = _instance.GetDateTime(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetChar_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			char value = 'v';

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			char result = _instance.GetChar(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetByte_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			byte value = 1;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			byte result = _instance.GetByte(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetBoolean_GoldFlow()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool result = _instance.GetBoolean(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetValue_NonLongTextField()
		{
			// Arrange	
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			const int longTextFieldArtifactid = 324;

			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},
			new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			object result = _instance.GetValue(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetValue_LongTextField()
		{
			// Arrange	
			const int longTextFieldIdentifier = 645;
			const string longTextFieldName = "Extracted text";

			string value = "very long text text";

			ResultSet<Document> longTextFieldRead = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(_DOCUMENT_ARTIFACTID)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(longTextFieldIdentifier)
								{
									Name = longTextFieldName,
									Value = value
								}
							}
						}
					},
				}
			};

			object longTextField = LongTextFieldTestTemplate(longTextFieldRead, longTextFieldIdentifier, longTextFieldName);

			// Arrange
			Assert.AreEqual(value, longTextField, "The result should be correct");
		}

		[Test]		
		public void GetValue_DocumentReadReturnNoResult()
		{
			// Arrange	
			const int longTextFieldIdentifier = 645;
			const string longTextFieldName = "Extracted text";

			ResultSet<Document> longTextFieldRead = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(_DOCUMENT_ARTIFACTID)
						{
							Fields = new List<FieldValue>()
						}
					},
				}
			};

			Assert.Throws<ProviderReadDataException>(() =>
			{
				LongTextFieldTestTemplate(longTextFieldRead, longTextFieldIdentifier, longTextFieldName);
			});
		}

		[Test]
		public void GetValue_DocumentReadFailed()
		{
			// Arrange	
			const int longTextFieldIdentifier = 645;
			const string longTextFieldName = "Extracted text";

			ResultSet<Document> longTextFieldRead = new ResultSet<Document>
			{
				Success = false,
				Results = new List<Result<Document>>()
			};

			Assert.Throws<ProviderReadDataException>(() =>
			{
				LongTextFieldTestTemplate(longTextFieldRead, longTextFieldIdentifier, longTextFieldName);
			});
		}

		[Test]
		public void GetValue_DocumentReadNoFieldReturned()
		{
			// Arrange	
			const int longTextFieldIdentifier = 645;
			const string longTextFieldName = "Extracted text";

			ResultSet<Document> longTextFieldRead = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(_DOCUMENT_ARTIFACTID)
						{
							Fields = new List<FieldValue>()
						}
					},
				}
			};

			Assert.Throws<ProviderReadDataException>(() =>
			{
				LongTextFieldTestTemplate(longTextFieldRead, longTextFieldIdentifier, longTextFieldName);
			});
		}

		[Test]
		private void GetValue_DocumentReadThrowException(ResultSet<Document> mockReadResult, int longTextFieldIdentifier, string extractedTextField)
		{
			InitializeReaderToReadLongText(extractedTextField, longTextFieldIdentifier);

//			_rdoRepository
//				.ReadDocument(Arg.Any<Document>())
//				.Throws<Exception>();

			// Act
			_instance.Read();
			Assert.Throws<ProviderReadDataException>( () => { _instance.GetValue(1); });
		}

		private object LongTextFieldTestTemplate(ResultSet<Document> mockReadResult, int longTextFieldIdentifier, string extractedTextField)
		{
			InitializeReaderToReadLongText(extractedTextField, longTextFieldIdentifier);

//			_rdoRepository
//				.ReadDocument(Arg.Any<Document>())
//				.Returns(mockReadResult);
			// Act
			_instance.Read();
			return _instance.GetValue(1);
		}

		private void InitializeReaderToReadLongText(string extractedTextField, int longTextFieldIdentifier)
		{
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { _DOCUMENT_ARTIFACTID }, new[]
			{
				new FieldEntry() { DisplayName = _FIELD_NAME, FieldIdentifier = _FIELD_IDENTIFIER.ToString()},
				new FieldEntry() { DisplayName = extractedTextField, FieldIdentifier = longTextFieldIdentifier.ToString()}
			},
			new QueryDataItemResult[0]);

			ResultSet<Document> initialRead = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(_DOCUMENT_ARTIFACTID)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(_FIELD_IDENTIFIER)
								{
									Name = _FIELD_NAME,
									Value = "REL01"
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(initialRead);
		}

		#endregion

		#region NotImplemented

		[Test]
		public void GetValues_ThrowsNotImplementedException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetValues(new object[0]);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// to catch other exceptions	
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "A NotImplementedException should have been thrown");
		}

		[Test]
		public void GetData_ThrowsNotImplementedException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetData(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// to catch other exceptions	
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "A NotImplementedException should have been thrown");
		}

		[Test]
		public void GetChars_ThrowsNotImplementedException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetChars(0, 0, new char[0], 0, 0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// to catch other exceptions	
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "A NotImplementedException should have been thrown");
		}

		[Test]
		public void GetBytes_ThrowsNotImplementedException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			const string fieldName = "DispName";
			const int fieldIdentifier = 123;
			_instance = new DocumentTranfserDataReader(_rdoRepository, new[] { documentArtifactId }, new[]
			{
				new FieldEntry() { DisplayName = fieldName, FieldIdentifier = fieldIdentifier.ToString()},
			},new QueryDataItemResult[0]);

			bool value = true;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>()
					{
						Artifact = new Document(documentArtifactId)
						{
							Fields = new List<FieldValue>()
							{
								new FieldValue(fieldIdentifier)
								{
									Name = fieldName,
									Value = value
								}
							}
						}
					},
				}
			};

//			_rdoRepository
//				.ExecuteDocumentQuery(Arg.Any<Query<Document>>())
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetBytes(0, 0, new byte[0], 0, 0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// to catch other exceptions	
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "A NotImplementedException should have been thrown");
		}
		#endregion
	}
}