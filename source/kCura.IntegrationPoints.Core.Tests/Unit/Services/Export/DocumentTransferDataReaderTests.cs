﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Tests.Unit.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class DocumentTransferDataReaderTests
	{
		private IExporterService _exportService;
		private IDataReader _instance;
		private ICoreContext _context;
		private IFieldManager _fieldManager;
		private ISourceWorkspaceManager _sourceWorkspaceManager;

		private const int _DOCUMENT_ARTIFACTID = 123423;
		private const string _FIELD_NAME = "DispName";
		private const int _FIELD_IDENTIFIER = 123;
		private const string _CONTROL_NUMBER = "WEB000123";
		private const int _FETCH_ARTIFACTDTOS_BATCH_SIZE = 50;

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

		private readonly FieldMap[] _templateFieldEntries = new FieldMap[]
		{
			new FieldMap()
			{
				DestinationField = null,
				SourceField = new FieldEntry()
				{
					DisplayName = _FIELD_NAME,
					FieldIdentifier = _FIELD_IDENTIFIER.ToString()
				}
			}
		};

		[SetUp]
		public void SetUp()
		{
			_context = Substitute.For<ICoreContext>();
			_exportService = Substitute.For<IExporterService>();
			_fieldManager = Substitute.For<IFieldManager>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
		}

		#region Read

		[Test]
		public void Read_FirstRead_RetrievesDocuments_ReturnsTrue()
		{
			// Arrange
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[0]);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result, "There are no records to read, result should be false");
			Assert.IsTrue(_instance.IsClosed, "The reader should be closed");
		}

		[Test]
		public void Read_FirstRead_RunsSavedSearch_RequestFailsWithException()
		{
			// Arrange
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Throws<Exception>();

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act & Assert
			Assert.Throws<Exception>(() => _instance.Read());
		}

		[Test]
		public void Read_ReadAllResults_GoldFlow()
		{
			// Arrange
			int[] documentIds = { 123, 345 };
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns(new ArtifactDTO[]
				{
					new ArtifactDTO(documentIds[0], 10, new ArtifactFieldDTO[0]),
					new ArtifactDTO(documentIds[1], 10, new ArtifactFieldDTO[0]),
				});

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			int[] documentIds = { 123, 345 };
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns(new ArtifactDTO[]
				{
					new ArtifactDTO(documentIds[0], 10, new ArtifactFieldDTO[0]),
					new ArtifactDTO(documentIds[1], 10, new ArtifactFieldDTO[0]),
				});

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
					.Returns(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[0],
				_context);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void Read_NoDocumentIds_DoesNotFail()
		{
			// Arrange
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns(new ArtifactDTO[0]);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void Read_NoDocumentIdsNoFields_DoesNotFail()
		{
			// Arrange
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns(new ArtifactDTO[0]);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[0],
				_context);

			// Act
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result);
		}

		#endregion Read

		#region IDataReader methods

		[Test]
		public void IsDBNull_ResultNotNull_ReturnsFalse()
		{
			// Arrange
			// for retrieving long text field values (per doc)

			// for retrieving all the
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			_instance.Read();
			Type result = _instance.GetFieldType(0);

			// Assert
			Assert.AreEqual(result, typeof(string), "The types should match");
		}

		[Test]
		public void GetFieldTypeName_ReturnsString()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry()
						{
							DisplayName = _FIELD_NAME +  Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
							FieldIdentifier = _FIELD_IDENTIFIER.ToString()
						}
					}
				},
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry()
						{
							DisplayName = _FIELD_NAME + Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
							FieldIdentifier = _FIELD_IDENTIFIER.ToString()
						}
					}
				},
				_context);

			// Act
			int ordinal = _instance.GetOrdinal(_FIELD_IDENTIFIER.ToString());

			// Assert
			Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
		}

		[Test]
		public void ThisAccessor_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
		{
			// Arrange
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry()
						{
							DisplayName = _FIELD_NAME + Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
							FieldIdentifier = _FIELD_IDENTIFIER.ToString()
						}
					}
				},
				_context);

			// Act
			_instance.Read();
			object result = _instance[_FIELD_IDENTIFIER.ToString()];

			// Assert
			Assert.AreEqual(_CONTROL_NUMBER, result.ToString(), "The result should be correct");
		}

		[Test]
		public void ThisAccessor_FieldExists_LookUpSucceeds()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			_instance.Read();
			object result = _instance[_FIELD_IDENTIFIER.ToString()];

			// Assert
			Assert.AreEqual(_CONTROL_NUMBER, result.ToString(), "The result should be correct");
		}

		[Test]
		public void FieldCount_NoLongTextFields_ReturnsCorrectCount()
		{
			// Arrange
			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			_instance.Read();
			_instance.Close();
			bool result = _instance.Read();

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void Close_ReadThenClose_CannotAccessDocument()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[] 
				{
					new FieldMap()
					{
						SourceField = 	new FieldEntry() {FieldIdentifier = "123", DisplayName = "abc"},
					},
					new FieldMap()
					{
						SourceField = 	new FieldEntry() {FieldIdentifier = "456", DisplayName = "def"}
					}
				},
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[0], 
				_context);

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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[0],
				_context);

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
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
				.Returns<ArtifactDTO[]>(_templateArtifactDtos);

			_instance = new DocumentTransferDataReader(
				_exportService, _fieldManager, _sourceWorkspaceManager,
				_templateFieldEntries,
				_context);

			// Act
			_instance.Read();
			string result = _instance.GetString(0);

			// Arrange
			Assert.AreEqual(_CONTROL_NUMBER, result, "The result should be correct");
		}

		[Test]
		public void GetInt64_GoldFlow()
		{
			// Arrange
			Int64 value = Int64.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" } 
					}
				},
				_context);

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
			Int16 value = Int16.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }
					}
				},
				_context);

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
			Int32 value = Int32.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

			// Act
			_instance.Read();
			Int32 result = _instance.GetInt32(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test()]
		public void GetGuid_GoldFlow()
		{
			Guid value = Guid.NewGuid();
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			float value = float.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			double value = double.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			decimal value = decimal.MaxValue;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

			// Act
			_instance.Read();
			decimal result = _instance.GetDecimal(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}

		[Test]
		public void GetDateTime_GoldFlow()
		{
			// Arrange
			// for retrieving long text field values (per doc)
			DateTime value = DateTime.Now;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			char value = 'a';
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			byte value = 1;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

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
			bool value = false;
			// for retrieving all the documents
			_exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
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
				_exportService, _fieldManager, _sourceWorkspaceManager,
				new FieldMap[]
				{
					new FieldMap()
					{
						SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }, 
					}
				},
				_context);

			// Act
			_instance.Read();
			bool result = _instance.GetBoolean(0);

			// Arrange
			Assert.AreEqual(value, result, "The result should be correct");
		}
		#endregion IDataReader methods
	}
}