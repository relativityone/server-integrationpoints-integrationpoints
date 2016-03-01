using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Core.Services.RDO;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.IntegrationPoints.DocumentTransferProvider.Tests.Helpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Unit
{
	[TestFixture]
	public class DocumentArtifactIdDataReaderTests
	{
		private IRDORepository _relativityClient;
		private IDataReader _instance;
		private const int SAVED_SEARCH_ID = 123;
		private Query<Document> _expectedQuery;

		[SetUp]
		public void SetUp()
		{
			_relativityClient = NSubstitute.Substitute.For<IRDORepository>();

			_expectedQuery = new Query<Document>
			{
				Condition = new SavedSearchCondition(SAVED_SEARCH_ID),
				Fields = FieldValue.NoFields // we only want the ArtifactId
			};

			_instance = new DocumentArtifactIdDataReader(_relativityClient, SAVED_SEARCH_ID);
		}

		#region Read
		[Test]
		public void Read_FirstRead_RunsSavedSearch_ReturnsTrue()
		{
			// Arrange	
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)}
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = false,
				Results = new List<Result<Document>>()
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = false,
				Results = new List<Result<Document>>()
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Throws(new Exception());

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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool result1 = _instance.Read();
			_instance.Close();
			bool result2 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsFalse(result2, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}
		#endregion

		#region DataReaderTests
		[Test]
		public void ThisNameAccessor_GoldFlow()
		{
			// Arrange
			const int documentArtifactId1 = 123423;
			const int documentArtifactId2 = 123890;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId1)},
					new Result<Document>() {Artifact = new Document(documentArtifactId2)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult1 = _instance.Read();
			object accessorResult1 = _instance[Shared.Constants.ARTIFACT_ID_FIELD_NAME];
			bool readResult2 = _instance.Read();
			object accessorResult2 = _instance[Shared.Constants.ARTIFACT_ID_FIELD_NAME];
			bool readResult3 = _instance.Read();

			// Assert
			Assert.IsTrue(readResult1, "There are records to read, result should be true");
			Assert.IsTrue(readResult2, "There are records to read, result should be true");
			Assert.IsFalse(readResult3, "There are no records to read, result should be false");
			Assert.AreEqual(documentArtifactId1, Convert.ToInt32(accessorResult1));
			Assert.AreEqual(documentArtifactId2, Convert.ToInt32(accessorResult2));
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void ThisNameAccessor_InvalidColumnName_ThrowsIndexOutOfRangeException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};
//
//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			// Act
			bool correctExceptionThrown = false;
			try
			{
				object accessorResult = _instance["WRONG_COLUMN"];
			}
			catch (IndexOutOfRangeException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
		}

		[Test]
		public void ThisIndexAccessor_GoldFlow()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			object accessorResult = _instance[0];

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(documentArtifactId, Convert.ToInt32(accessorResult));
		}

		[Test]
		public void ThisIndexAccessor_InvalidColumnIndex_ThrowsIndexOutOfRangeException()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			// Act
			bool correctExceptionThrown = false;
			try
			{
				object accessorResult = _instance[1100];
			}
			catch (IndexOutOfRangeException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
		}

		[Test]
		public void GetName_ValidIndex_ReturnsName()
		{
			// Act
			string result = _instance.GetName(0);

			// Assert
			Assert.AreEqual(Shared.Constants.ARTIFACT_ID_FIELD_NAME, result, "The result should be correct");
		}

		[Test]
		public void GetName_InvalidIndex_ThrowsIndexOutOfRangeException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				string result = _instance.GetName(1);
			}
			catch (IndexOutOfRangeException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
		}

		[Test]
		public void GetOrdinal_ValidIndex_ReturnsName()
		{
			// Act
			int result = _instance.GetOrdinal(Shared.Constants.ARTIFACT_ID_FIELD_NAME);

			// Assert
			Assert.AreEqual(0, result, "The result should be correct");
		}

		[Test]
		public void GetOrdinal_InvalidIndex_ThrowsIndexOutOfRangeException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				int result = _instance.GetOrdinal("ASDF");
			}
			catch (IndexOutOfRangeException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
		}

		[Test]
		public void GetFieldType_ValidIndex_ReturnsType()
		{
			// Act
			Type result = _instance.GetFieldType(0);

			// Assert
			Assert.AreEqual(typeof(Int32), result, "The result should be correct");
		}

		[Test]
		public void GetFieldType_InvalidIndex_ThrowsIndexOutOfRangeException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				Type result = _instance.GetFieldType(1);
			}
			catch (IndexOutOfRangeException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
		}

		[Test]
		public void GetString_ReturnsString()
		{
			// Act
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			string getResult = _instance.GetString(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(Convert.ToString(documentArtifactId), getResult, "The result should be the documentArtifactId as a string");
		}

		[Test]
		public void GetInt32_ReturnsInt32Value()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			int getResult = _instance.GetInt32(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(documentArtifactId, getResult, "The result should be the documentArtifactId");
		}

		[Test]
		public void IsDBNull_ResultNotNull_ReturnsFalse()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			bool isDbNull = _instance.IsDBNull(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsFalse(isDbNull, "The result should not be DBNull");
		}

		[Test]
		public void FieldCount_ReturnsCorrectCount()
		{
			// Act
			int fieldCount = _instance.FieldCount;

			// Assert
			Assert.AreEqual(1, fieldCount, "There should be 1 field");
		}

		[Test]
		public void Dispose_BeforeRead_DoesNotExcept()
		{
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
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//				x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			_instance.Read();
			bool exceptionThrown = false;
			try
			{
				_instance.Dispose();
			}
			catch
			{
				exceptionThrown = true;
			}

			// Assert
			Assert.IsFalse(exceptionThrown, "No exception should be thrown");
		}

		[Test]
		public void Close_ReaderIsClosed()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient
//				.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//					x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient
//				.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//					x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
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
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient
//				.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//					x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
//				.Returns(resultSet);

			// Act
			_instance.Read();
			_instance.Close();
			_instance.Read();

			// Assert
//			_relativityClient
//				.Received(1)
//				.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//					x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)));
		}


		[Test]
		public void Close_ReadThenClose_CannotAccessDocument()
		{
			// Arrange
			const int documentArtifactId = 123423;
			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(documentArtifactId)},
				}
			};

//			_relativityClient
//				.ExecuteDocumentQuery(Arg.Is<Query<Document>>(
//					x => ArgumentMatcher.DocumentSearchProviderQueriesMatch(_expectedQuery, x)))
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
		public void Depth_ReturnsZero()
		{
			// Act	
			int result = _instance.Depth;

			// Assert
			Assert.AreEqual(0, result, "The Depth should be 0");
		}

		[Test]
		public void NextResult_ReturnsFalse()
		{
			// Act
			bool result = _instance.NextResult();

			// Assert
			Assert.IsFalse(result, "NextResult() should return false");
		}

		[Test]
		public void RecordsAffected_ReturnsNegativeOne()
		{
			// Act
			int result = _instance.RecordsAffected;

			// Assert
			Assert.AreEqual(-1, result, "RecordsAffected should return -1");
		}


		#region NotImplemented Methods

		[Test]
		public void GetValues_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetValues(new object[1]);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetInt64_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetInt64(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetInt16_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetInt16(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetDateTime_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetDateTime(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetData_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetData(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetChars_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetChars(0, 0, new char[0], 0, 0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetChar_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetChar(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetBytes_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetBytes(0, 0, new byte[0], 0, 0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetByte_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetByte(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetBoolean_ThrowsNotImplementedException()
		{
			// Act
			bool notImplementedExceptionThrown = false;
			try
			{
				_instance.GetBoolean(0);
			}
			catch (NotImplementedException)
			{
				notImplementedExceptionThrown = true;
			}
			catch
			{
			}

			Assert.IsTrue(notImplementedExceptionThrown, "Correct exception should have been thrown");
		}

		[Test]
		public void GetSchemaTable_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetSchemaTable();
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}

		[Test]
		public void GetDataTypeName_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetDataTypeName(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}

		[Test]
		public void GetDecimal_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetDecimal(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}

		[Test]
		public void GetDouble_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetDouble(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}

		[Test]
		public void GetFloat_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetFloat(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}


		[Test]
		public void GetGuid_ThrowsNotImplementedException()
		{
			// Act
			bool correctExceptionThrown = false;
			try
			{
				_instance.GetGuid(0);
			}
			catch (NotImplementedException)
			{
				correctExceptionThrown = true;
			}
			catch
			{
			}

			// Assert
			Assert.IsTrue(correctExceptionThrown, "The correct exception type should have been thrown");
		}
		#endregion

		#endregion
	}
}