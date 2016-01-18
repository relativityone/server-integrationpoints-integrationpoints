using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Unit
{
	[TestFixture]
	public class DocumentTransferDataReaderTests
	{
		private IRelativityClientAdaptor _relativityClientAdaptor;
		private IDataReader _instance;

		[SetUp]
		public void SetUp()
		{
			_relativityClientAdaptor = NSubstitute.Substitute.For<IRelativityClientAdaptor>();
		}

		#region Read
		[Test]
		public void Read_FirstRead_RunsSavedSearch_ReturnsTrue()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			}); ;

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = false,
				Results = new List<Result<Document>>()
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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

		#region IDataReader methods
		[Test]
		public void GetName_FieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual("123", fieldName, "The field loopup should succeed");
		}

		[Test]
		public void GetName_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName [Object Identifier]", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual("123", fieldName, "The field loopup should succeed");
		}

		[Test]
		public void GetOrdinal_FieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			int ordinal = _instance.GetOrdinal("123");

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void GetOrdinal_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName [Object Identifier]", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1)},
					new Result<Document>() {Artifact = new Document(2)}
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			int ordinal = _instance.GetOrdinal("123");

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void ThisAccessor_ObjetIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange	
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName [Object Identifier]", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL001"}}}},
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL002"}}}},
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"}
			});

			ResultSet<Document> resultSet = new ResultSet<Document>
			{
				Success = true,
				Results = new List<Result<Document>>()
				{
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL001"}}}},
					new Result<Document>() {Artifact = new Document(1) {Fields = new List<FieldValue>(){ new FieldValue(){ Name = "DispName", Value = "REL002"}}}},
				}
			};

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

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
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "1233"}
			});

			// Act
			int fieldCount = _instance.FieldCount;

			// Assert
			Assert.AreEqual(2, fieldCount, "There should be 2 fields");
		}

		[Test]
		public void Dispose_BeforeRead_DoesNotExcept()
		{
			// Arrange
			_instance = new DocumentTranfserDataReader(_relativityClientAdaptor, new[] { 1 }, new[]
			{
				new FieldEntry() { DisplayName = "DispName", FieldIdentifier = "123"},
				new FieldEntry() { DisplayName = "DispNameTwo", FieldIdentifier = "1233"}
			});

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
		#endregion
	}
}