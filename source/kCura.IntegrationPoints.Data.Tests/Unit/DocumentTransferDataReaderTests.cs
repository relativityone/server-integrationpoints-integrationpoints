using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Data.Managers;
using kCura.IntegrationPoints.Data.Tests.Unit.Helpers;
using kCura.IntegrationPoints.DocumentTransferProvider.DataReaders;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class DocumentTransferDataReaderTests
	{
		private IDataReader _instance;
		private IDocumentManager _documentManager;

		private const int _DOCUMENT_ARTIFACTID = 123423;
		private readonly int[] _documentIds = new[] { _DOCUMENT_ARTIFACTID };
		private const string _FIELD_NAME = "DispName";
		private const int _FIELD_IDENTIFIER = 123;
		private const string _CONTROL_NUMBER = "WEB000123";

		private static readonly ArtifactDTO _templateArtifactDto = new ArtifactDTO(
			_DOCUMENT_ARTIFACTID,
			10,
			new List<ArtifactFieldDTO>()
			{
				new ArtifactFieldDTO()
				{
					ArtifactId = _FIELD_IDENTIFIER,
					FieldType = "Fixed Length",
					Name = "Control Number",
					Value = _CONTROL_NUMBER
				}
			});

		private readonly ArtifactDTO[] _templateArtifactDtos = new ArtifactDTO[]
			{
				_templateArtifactDto
			};

		private readonly FieldEntry[] _templateFieldEntries = new FieldEntry[]
		{
			new FieldEntry() {DisplayName = _FIELD_NAME, FieldIdentifier = _FIELD_IDENTIFIER.ToString()}
		};

		[SetUp]
		public void SetUp()
		{
			_documentManager = NSubstitute.Substitute.For<IDocumentManager>();
		}

		#region Read

		[Test]
		public void Read_FirstRead_RetrievesDocuments_ReturnsTrue()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsTrue(result, "There are records to read, result should be true");
			Assert.IsFalse(_instance.IsClosed, "The reader should be open");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_NoResults_ReturnsFalse()
		{
			// Arrange
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[0]);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_RequestFailsWithException_ReturnsFalse()
		{
			// Arrange
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Throws(new Exception());

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

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
			int[] documentIds = { 123, 345 };
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns(new ArtifactDTO[]
				{
					new ArtifactDTO(documentIds[0], 10, new ArtifactFieldDTO[0]),
					new ArtifactDTO(documentIds[1], 10, new ArtifactFieldDTO[0]),
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result1 = _instance.Read();
			bool result2 = _instance.Read();
			bool result3 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsTrue(result2, "There are records to read, result should be true");
			Assert.IsFalse(result3, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Read_ReadSomeResultsThenClose_GoldFlow()
		{
			// Arrange
			int[] documentIds = { 123, 345 };
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns(new ArtifactDTO[]
				{
					new ArtifactDTO(documentIds[0], 10, new ArtifactFieldDTO[0]),
					new ArtifactDTO(documentIds[1], 10, new ArtifactFieldDTO[0]),
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result1 = _instance.Read();
			_instance.Close();
			bool result2 = _instance.Read();

			// Assert
			Assert.IsTrue(result1, "There are records to read, result should be true");
			Assert.IsFalse(result2, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Read_NoFields_DoesNotFail()
		{
			// Arrange
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
					.Returns(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new List<FieldEntry>(),
				new int[0]);

			// Act
			bool result = _instance.Read();

			// Assert
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())));
		}

		[Test]
		public void Read_NoDocumentIds_DoesNotFail()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				new int[0],
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result = _instance.Read();

			// Assert
			_documentManager.Received(0).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())));
		}

		[Test]
		public void Read_NoDocumentIdsNoFields_DoesNotFail()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				new int[0],
				new List<FieldEntry>(),
				new int[0]);

			// Act
			bool result = _instance.Read();

			// Assert
			_documentManager.Received(0).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())));
		}

		#endregion Read

		#region IDataReader methods

		[Test]
		public void IsDBNull_ResultNotNull_ReturnsFalse()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool readResult = _instance.Read();
			bool isDbNull = _instance.IsDBNull(0);

			// Assert
			Assert.IsTrue(readResult, "There are records to read, result should be true");
			Assert.IsFalse(isDbNull, "The result should not be DBNull");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetFieldType_ReturnsString()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			Type result = _instance.GetFieldType(0);

			// Assert
			Assert.AreEqual(result, typeof(string), "The types should match");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetFieldTypeName_ReturnsString()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			string result = _instance.GetDataTypeName(0);

			// Assert
			Assert.AreEqual(result, typeof(string).FullName, "The types should match");
		}

		[Test]
		public void NextResult_ReturnsFalse()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			bool result = _instance.NextResult();

			// Assert
			Assert.IsFalse(result, "NextResult() should return false");
		}

		[Test]
		public void Depth_ReturnsZero()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			int result = _instance.Depth;

			// Assert
			Assert.AreEqual(0, result, "Depth should return 0");
		}

		[Test]
		public void RecordsAffected_ReturnsNegativeOne()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			int result = _instance.RecordsAffected;

			// Assert
			Assert.AreEqual(-1, result, "RecordsAffected should alwayds return -1");
		}

		[Test]
		public void GetName_FieldExists_LookUpSucceeds()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual(_FIELD_IDENTIFIER.ToString(), fieldName, "The field lookup should succeed");
		}

		[Test]
		public void GetName_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[]
				{
					new FieldEntry()
					{
						DisplayName = _FIELD_NAME +  Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
						FieldIdentifier = _FIELD_IDENTIFIER.ToString()
					}
				},
				new int[0]);

			// Act
			string fieldName = _instance.GetName(0);

			// Assert
			Assert.AreEqual(_FIELD_IDENTIFIER.ToString(), fieldName, "The field lookup should succeed");
		}

		[Test]
		public void GetOrdinal_FieldExists_LookUpSucceeds()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			int ordinal = _instance.GetOrdinal(_FIELD_IDENTIFIER.ToString());

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void GetOrdinal_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[]
				{
					new FieldEntry()
					{
						DisplayName = _FIELD_NAME + Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
						FieldIdentifier = _FIELD_IDENTIFIER.ToString()
					}
				},
				new int[0]);

			// Act
			int ordinal = _instance.GetOrdinal(_FIELD_IDENTIFIER.ToString());

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void ThisAccessor_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[]
				{
					new FieldEntry()
					{
						DisplayName = _FIELD_NAME + Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
						FieldIdentifier = _FIELD_IDENTIFIER.ToString()
					}
				},
				new int[0]);

			// Act
			_instance.Read();
			object result = _instance[_FIELD_IDENTIFIER.ToString()];

			// Assert
			Assert.AreEqual(_CONTROL_NUMBER, result.ToString(), "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void ThisAccessor_FieldExists_LookUpSucceeds()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			object result = _instance[_FIELD_IDENTIFIER.ToString()];

			// Assert
			Assert.AreEqual(_CONTROL_NUMBER, result.ToString(), "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void FieldCount_NoLongTextFields_ReturnsCorrectCount()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			int fieldCount = _instance.FieldCount;

			// Assert
			Assert.AreEqual(2, fieldCount, "There should be 2 fields");
		}

		[Test]
		public void FieldCount_WithLongTextFields_ReturnsCorrectCount()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[] { 1232 });

			// Act
			int fieldCount = _instance.FieldCount;

			// Assert
			Assert.AreEqual(2, fieldCount, "There should be 2 fields");
		}

		[Test]
		public void Dispose_BeforeRead_DoesNotExcept()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

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
			_documentManager.Received(0).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Dispose_WhileReaderIsOpen_DoesNotExcept()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

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
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Close_ReaderIsClosed()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			_instance.Close();
			bool isClosed = _instance.IsClosed;

			// Assert
			Assert.IsTrue(isClosed, "The reader should be closed");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Close_ReadThenCloseThenRead_ReaderIsClosed()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			_instance.Close();
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "The reader should be closed");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Close_ReadThenCloseThenRead_QueryIsNotRerun()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			_instance.Close();
			_instance.Read();

			// Assert
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void Close_ReadThenClose_CannotAccessDocument()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

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
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetSchemaTable_OneField_ReturnsCorrectSchema()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			var expectedResult = new DataTable() { Columns = { new DataColumn(_FIELD_IDENTIFIER.ToString()), new DataColumn(Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD) } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_MultipleFields_ReturnsCorrectSchema()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[]
				{
					new FieldEntry() {FieldIdentifier = "123", DisplayName = "abc"},
					new FieldEntry() {FieldIdentifier = "456", DisplayName = "def"},
				},
				new int[0]);

			var expectedResult = new DataTable()
			{
				Columns = { new DataColumn("123"), new DataColumn("456"), new DataColumn(Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD) }
			};

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_NoFields_ReturnsCorrectSchema()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[0],
				new int[0]);

			var expectedResult = new DataTable() { Columns = { new DataColumn(Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD) } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		[Test]
		public void GetSchemaTable_NoDocumentsNoFields_ReturnsCorrectSchema()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_documentManager,
				new int[0],
				new FieldEntry[0],
				new int[0]);

			var expectedResult = new DataTable() { Columns = { new DataColumn(Contracts.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD) } };

			// Act
			DataTable result = _instance.GetSchemaTable();

			// Arrange
			Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
		}

		#endregion IDataReader methods

		#region Gets

		[Test]
		public void GetString_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				_templateFieldEntries,
				new int[0]);

			// Act
			_instance.Read();
			string result = _instance.GetString(0);

			// Arrange
			Assert.AreEqual(_CONTROL_NUMBER, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetInt64_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			Int64 value = Int64.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			Int64 result = _instance.GetInt64(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetInt16_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			Int16 value = Int16.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			Int16 result = _instance.GetInt16(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetInt32_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			Int32 value = Int32.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			Int32 result = _instance.GetInt32(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test()]
		public void GetGuid_GoldFlow()
		{
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			Guid value = Guid.NewGuid();
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			Guid result = _instance.GetGuid(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetFloat_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			float value = float.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			float result = _instance.GetFloat(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetDouble_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			double value = double.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			double result = _instance.GetDouble(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetDecimal_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			decimal value = decimal.MaxValue;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			decimal result = _instance.GetDecimal(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetDateTime_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			DateTime value = DateTime.Now;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			DateTime result = _instance.GetDateTime(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetChar_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			char value = 'a';
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			char result = _instance.GetChar(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetByte_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			byte value = 1;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			byte result = _instance.GetByte(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetBoolean_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => !x.Any())))
				.Returns<ArtifactDTO>(_templateArtifactDto);

			bool value = false;
			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(new ArtifactDTO[]
				{
					new ArtifactDTO(
						1234,
						10,
						new List<ArtifactFieldDTO>()
						{
							new ArtifactFieldDTO()
							{
								ArtifactId = 123,
								Name = "Some Number",
								Value = value
							}
						})
				});

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				new FieldEntry[] { new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, },
				new int[0]);

			// Act
			_instance.Read();
			bool result = _instance.GetBoolean(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetValue_LongTextField_GoldFlow()
		{
			// Arrange
			const string longTextFieldValue = "woop, der it is!";
			const int longTextFieldIdentifier = 8392;
			const string longTextFieldName = "LongTextFieldNameGoesHere";
			var longTextField = new ArtifactFieldDTO()
			{
				ArtifactId = longTextFieldIdentifier,
				FieldType = "Long Text",
				Name = longTextFieldName,
				Value = longTextFieldValue
			};

			var artifactDtoOnlyLongTextField = new ArtifactDTO(
				_DOCUMENT_ARTIFACTID,
				10,
				new List<ArtifactFieldDTO>()
				{
					longTextField
				});

			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<int[]>(x => x[0] == longTextFieldIdentifier)))
				.Returns<ArtifactDTO>(artifactDtoOnlyLongTextField);

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			List<FieldEntry> fieldEntries = _templateFieldEntries.ToList();
			fieldEntries.Add(new FieldEntry() { FieldIdentifier = longTextFieldIdentifier.ToString(), DisplayName = longTextFieldName });

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				fieldEntries,
				new[] { longTextFieldIdentifier });

			// Act
			_instance.Read();
			object result = _instance.GetValue(1);

			// Arrange
			Assert.AreEqual(longTextFieldValue, result as string, "The result should be correct");

			_documentManager.Received(1).RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<int[]>(x => x[0] == longTextFieldIdentifier)));
			_documentManager.Received(1).RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))));
		}

		[Test]
		public void GetValue_DocumentReadFailed()
		{
			// Arrange
			const string longTextFieldValue = "woop, der it is!";
			const int longTextFieldIdentifier = 8392;
			const string longTextFieldName = "LongTextFieldNameGoesHere";
			var longTextField = new ArtifactFieldDTO()
			{
				ArtifactId = longTextFieldIdentifier,
				FieldType = "Long Text",
				Name = longTextFieldName,
				Value = longTextFieldValue
			};

			var artifactDto = new ArtifactDTO(_DOCUMENT_ARTIFACTID, 10, new List<ArtifactFieldDTO>() { });

			ArtifactDTO[] artifactDtos = { artifactDto };

			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Contains(longTextField.ArtifactId))))
				.ReturnsNull();

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			var fieldEntries = _templateFieldEntries.ToList();
			fieldEntries.Add(new FieldEntry() { FieldIdentifier = longTextFieldIdentifier.ToString(), DisplayName = longTextFieldName });

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				fieldEntries.ToArray(),
				new[] { longTextFieldIdentifier });

			// Act
			Assert.Throws<ProviderReadDataException>(() =>
			{
				_instance.Read();
				object result = _instance.GetValue(1);
			});
		}

		[Test]
		public void GetValue_LongTextField_DocumentReadThrowsException()
		{
			// Arrange
			const string longTextFieldValue = "woop, der it is!";
			const int longTextFieldIdentifier = 8392;
			const string longTextFieldName = "LongTextFieldNameGoesHere";
			var longTextField = new ArtifactFieldDTO()
			{
				ArtifactId = longTextFieldIdentifier,
				FieldType = "Long Text",
				Name = longTextFieldName,
				Value = longTextFieldValue
			};

			var artifactDto = new ArtifactDTO(_DOCUMENT_ARTIFACTID, 10, new List<ArtifactFieldDTO>() { });

			ArtifactDTO[] artifactDtos = { artifactDto };

			// for retrieving long text field values (per doc)
			_documentManager.RetrieveDocumentAsync(
				Arg.Is(_DOCUMENT_ARTIFACTID),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Contains(longTextField.ArtifactId))))
				.Throws(new Exception());

			// for retrieving all the documents
			_documentManager.RetrieveDocumentsAsync(
				Arg.Is(_documentIds),
				Arg.Is(Arg.Is<HashSet<int>>(x => x.Count() == 1 && x.Contains(_FIELD_IDENTIFIER))))
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			var fieldEntries = _templateFieldEntries.ToList();
			fieldEntries.Add(new FieldEntry() { FieldIdentifier = longTextFieldIdentifier.ToString(), DisplayName = longTextFieldName });

			_instance = new DocumentTransferDataReader(
				_documentManager,
				_documentIds,
				fieldEntries.ToArray(),
				new[] { longTextFieldIdentifier });

			// Act
			Assert.Throws<ProviderReadDataException>(() =>
			{
				_instance.Read();
				object result = _instance.GetValue(1);
			});
		}

		#endregion Gets
	}
}