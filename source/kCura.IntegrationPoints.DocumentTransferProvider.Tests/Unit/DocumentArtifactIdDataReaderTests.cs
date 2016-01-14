using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Pipes;
using kCura.IntegrationPoints.DocumentTransferProvider.Adaptors;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Tests.Unit
{
	[TestFixture]
	public class DocumentArtifactIdDataReaderTests
	{
		private IRelativityClientAdaptor _relativityClientAdaptor;
		private IDataReader _instance;
		private const int SAVED_SEARCH_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_relativityClientAdaptor = NSubstitute.Substitute.For<IRelativityClientAdaptor>();

			_instance = new DocumentArtifactIdDataReader(_relativityClientAdaptor, SAVED_SEARCH_ID);
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

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool readResult1 = _instance.Read();
			object accessorResult1 = _instance["ArtifactId"];
			bool readResult2 = _instance.Read();
			object accessorResult2 = _instance["ArtifactId"];
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
		public void ThisNameAccessor_InvalidColumnName_ReturnsNull()
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

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			object accessorResult = _instance["WRONG_COLUMN"];

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsNull(accessorResult, "The column doesn't exist, there shouldn't be any results");
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

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			object accessorResult = _instance[0];

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.AreEqual(documentArtifactId, Convert.ToInt32(accessorResult));
		}

		[Test]
		public void ThisIndexAccessor_InvalidColumnIndex_ReturnsNull()
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

			_relativityClientAdaptor.ExecuteDocumentQuery(Arg.Any<Query<Document>>()).Returns(resultSet);

			// Act
			bool readResult = _instance.Read();
			object accessorResult = _instance[1100];

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsNull(accessorResult, "The column doesn't exist, there shouldn't be any results");
		}
		#endregion
	}
}