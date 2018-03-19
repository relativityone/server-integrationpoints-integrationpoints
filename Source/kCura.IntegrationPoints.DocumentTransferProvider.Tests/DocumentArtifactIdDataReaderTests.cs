using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests
{
	[TestFixture]
	public class DocumentArtifactIdDataReaderTests : TestBase
	{
		private const string _ARTIFACT_ID_FIELD_NAME = "ArtifactId";
		private ISavedSearchRepository _savedSearchRepository;
		private IDataReader _instance;
		private const int SAVED_SEARCH_ID = 123;
		private Query<kCura.Relativity.Client.DTOs.Document> _expectedQuery;

		[SetUp]
		public override void SetUp()
		{
			_savedSearchRepository = NSubstitute.Substitute.For<ISavedSearchRepository>();

			_expectedQuery = new Query<kCura.Relativity.Client.DTOs.Document>
			{
				Condition = new SavedSearchCondition(SAVED_SEARCH_ID),
				Fields = FieldValue.NoFields // we only want the ArtifactId
			};

			_instance = new DocumentArtifactIdDataReader(_savedSearchRepository);
		}

		#region Read

		[Test]
		public void Read_FirstRead_RunsSavedSearch_ReturnsTrue()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1, 10, "Document", new ArtifactFieldDTO[0])
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsTrue(result, "There are records to read, result should be true");
			Assert.IsFalse(_instance.IsClosed, "The reader should be open");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_NoResults_ReturnsFalse()
		{
			// Arrange
			var documents = new ArtifactDTO[] { };

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(true);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(2).AllDocumentsRetrieved();
		}

		[Test]
		[Ignore("Working on fix")]
		public void Read_FirstRead_RunsSavedSearch_RequestFailsWithException()
		{
			// Arrange
			_savedSearchRepository.RetrieveNextDocuments().Throws(new Exception());
			_savedSearchRepository.AllDocumentsRetrieved().Returns(true);

			// Act & Assert
			Assert.Throws<IntegrationPointsException>(() => _instance.Read());
		}

		[Test]
		public void Read_ReadAllResults_GoldFlow()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
				new ArtifactDTO(10,10, "Document", new ArtifactFieldDTO[0])
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(true);

			// Act
			bool result1 = _instance.Read();
			bool result2 = _instance.Read();
			bool result3 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsTrue(result2, "There are records to read, result should be true");
			Assert.IsFalse(result3, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
		}

		[Test]
		public void Read_ReadSomeResultsThenClose_GoldFlow()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
				new ArtifactDTO(10,10, "Document", new ArtifactFieldDTO[0])
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(true);

			// Act
			bool result1 = _instance.Read();
			_instance.Close();
			bool result2 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsFalse(result2, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
		}

		#endregion Read

		#region DataReaderTests

		[Test]
		public void ThisNameAccessor_GoldFlow()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
				new ArtifactDTO(10,10, "Document", new ArtifactFieldDTO[0])
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);
			_savedSearchRepository.AllDocumentsRetrieved().Returns(true);

			// Act
			bool readResult1 = _instance.Read();
			object accessorResult1 = _instance[_ARTIFACT_ID_FIELD_NAME];
			bool readResult2 = _instance.Read();
			object accessorResult2 = _instance[_ARTIFACT_ID_FIELD_NAME];
			bool readResult3 = _instance.Read();

			// Assert
			Assert.IsTrue(readResult1, "There are records to read, result should be true");
			Assert.IsTrue(readResult2, "There are records to read, result should be true");
			Assert.IsFalse(readResult3, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
		}

		[Test]
		public void ThisNameAccessor_InvalidColumnName_ThrowsIndexOutOfRangeException()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
				new ArtifactDTO(10,10, "Document", new ArtifactFieldDTO[0])
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool readResult = _instance.Read();
			// Act
			bool correctExceptionThrown = false;
			string exceptionMessage = String.Empty;
			try
			{
				object accessorResult = _instance["WRONG_COLUMN"];
			}
			catch (IndexOutOfRangeException e)
			{
				correctExceptionThrown = true;
				exceptionMessage = e.Message;
			}
			catch
			{
				// To catch any other types of exceptions before failing the tests
			}

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsTrue(correctExceptionThrown, "An IndexOutOfRangeException should have been thrown");
			Assert.AreEqual("'WRONG_COLUMN' is not a valid column", exceptionMessage, "The exception message should be as expected");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void ThisIndexAccessor_GoldFlow()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool readResult = _instance.Read();
			object accessorResult = _instance[0];

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(documents[0].ArtifactId, Convert.ToInt32(accessorResult));
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void ThisIndexAccessor_InvalidColumnIndex_ThrowsIndexOutOfRangeException()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

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
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void GetName_ValidIndex_ReturnsName()
		{
			// Act
			string result = _instance.GetName(0);

			// Assert
			Assert.AreEqual(_ARTIFACT_ID_FIELD_NAME, result, "The result should be correct");
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
			int result = _instance.GetOrdinal(_ARTIFACT_ID_FIELD_NAME);

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
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool readResult = _instance.Read();
			string getResult = _instance.GetString(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(Convert.ToString(documents[0].ArtifactId), getResult, "The result should be the documentArtifactId as a string");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void GetInt32_ReturnsInt32Value()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool readResult = _instance.Read();
			int getResult = _instance.GetInt32(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(documents[0].ArtifactId, getResult, "The result should be the documentArtifactId");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void IsDBNull_ResultNotNull_ReturnsFalse()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			bool readResult = _instance.Read();
			bool isDbNull = _instance.IsDBNull(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsFalse(isDbNull, "The result should not be DBNull");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
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
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

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
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void Close_ReaderIsClosed()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			_instance.Read();
			_instance.Close();
			bool isClosed = _instance.IsClosed;

			// Assert
			Assert.IsTrue(isClosed, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void Close_ReadThenCloseThenRead_ReaderIsClosed()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};
			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			_instance.Read();
			_instance.Close();
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "The reader should be closed");
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void Close_ReadThenCloseThenRead_QueryIsNotRerun()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

			// Act
			_instance.Read();
			_instance.Close();
			_instance.Read();

			// Assert
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
		}

		[Test]
		public void Close_ReadThenClose_CannotAccessDocument()
		{
			// Arrange
			var documents = new ArtifactDTO[]
			{
				new ArtifactDTO(1,10, "Document", new ArtifactFieldDTO[0]),
			};

			_savedSearchRepository.RetrieveNextDocuments().Returns(documents);

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
			_savedSearchRepository.Received(1).RetrieveNextDocuments();
			_savedSearchRepository.Received(0).AllDocumentsRetrieved();
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

		#endregion DataReaderTests
	}
}