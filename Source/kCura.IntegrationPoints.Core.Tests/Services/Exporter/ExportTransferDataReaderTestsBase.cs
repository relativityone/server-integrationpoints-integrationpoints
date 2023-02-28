using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
    [TestFixture, Category("Unit")]
    public abstract class ExportTransferDataReaderTestsBase : TestBase
    {
        protected IExporterService _exportService;
        protected IDataReader _instance;
        protected IScratchTableRepository[] _scratchRepositories;
        protected ISourceWorkspaceManager _sourceWorkspaceManager;
        protected ISourceJobManager _sourceJobManager;
        protected IRelativityObjectManager _relativityObjectManager;
        protected IDocumentRepository _documentRepository;

        protected const int _SPECIAL_FIELD_COUNT = 6;
        protected const int _DOCUMENT_ARTIFACTID = 123423;
        protected const string _FIELD_NAME = "DispName";
        protected const int _FIELD_IDENTIFIER = 123;
        protected const string _CONTROL_NUMBER = "WEB000123";
        protected const int _FETCH_ARTIFACTDTOS_BATCH_SIZE = 200;
        protected const int _SOURCE_WORKSPACE_ARTIFACTID = 93020;
        protected const int _TARGET_WORKSPACE_ARTIFACTID = 930233;
        protected const string _SOURCE_WORKSPACE_NAME = "Source Workspace";

        protected abstract ExportTransferDataReaderBase CreateDataReaderTestInstance();

        protected abstract ExportTransferDataReaderBase CreateDataReaderTestInstanceWithParameters(
            IExporterService relativityExportService,
            FieldMap[] fieldMappings,
            IScratchTableRepository[] scratchTableRepositories);

        protected readonly SourceWorkspaceDTO _sourceWorkspaceDto = new SourceWorkspaceDTO
        {
            ArtifactId = 409303,
            ArtifactTypeId = 4444,
            Name = string.Format("{0} - {1}", _SOURCE_WORKSPACE_NAME, _SOURCE_WORKSPACE_ARTIFACTID),
            SourceCaseName = _SOURCE_WORKSPACE_NAME,
            SourceCaseArtifactId = _SOURCE_WORKSPACE_ARTIFACTID
        };

        protected static readonly ArtifactDTO _templateArtifactDto = new ArtifactDTO(
            _DOCUMENT_ARTIFACTID,
            10, "Document",
            new List<ArtifactFieldDTO>()
            {
                new ArtifactFieldDTO()
                {
                    ArtifactId = _FIELD_IDENTIFIER,
                    FieldType = FieldTypeHelper.FieldType.Varchar,
                    Name = "Control Number",
                    Value = _CONTROL_NUMBER
                }
            });

        protected readonly ArtifactDTO[] _templateArtifactDtos = new ArtifactDTO[]
        {
            _templateArtifactDto
        };

        protected readonly FieldMap[] _templateFieldEntries = new FieldMap[]
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
        public override void SetUp()
        {
            _exportService = Substitute.For<IExporterService>();
            var scratchTable = Substitute.For<IScratchTableRepository>();
            _scratchRepositories = new[] { scratchTable };
            _sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
            _sourceJobManager = Substitute.For<ISourceJobManager>();

            _sourceWorkspaceManager.CreateSourceWorkspaceDto(_TARGET_WORKSPACE_ARTIFACTID, _SOURCE_WORKSPACE_ARTIFACTID, null)
                .Returns(_sourceWorkspaceDto);

        }

        #region Read
        [Test]
        public void Read_FirstRead_RetrievesDocuments_ReturnsTrue()
        {
            // Arrange
            _exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(_templateArtifactDtos);

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

            // Act & Assert
            Assert.Throws<IntegrationPointsException>(() => _instance.Read());
        }

        [Test]
        public void Read_ReadAllResults_GoldFlow()
        {
            // Arrange
            int[] documentIds = { 123, 345 };
            _exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
                .Returns(new ArtifactDTO[]
                {
                    new ArtifactDTO(documentIds[0], 10, "Document", new ArtifactFieldDTO[0]),
                    new ArtifactDTO(documentIds[1], 10, "Document", new ArtifactFieldDTO[0]),
                });

            _instance = CreateDataReaderTestInstance();

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
                    new ArtifactDTO(documentIds[0], 10, "Document", new ArtifactFieldDTO[0]),
                    new ArtifactDTO(documentIds[1], 10, "Document", new ArtifactFieldDTO[0]),
                });

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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
            _instance = CreateDataReaderTestInstance();

            // Act
            bool result = _instance.NextResult();

            // Assert
            Assert.IsFalse(result, "NextResult() should return false");
        }

        [Test]
        public void Depth_ReturnsZero()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            // Act
            int result = _instance.Depth;

            // Assert
            Assert.AreEqual(0, result, "Depth should return 0");
        }

        [Test]
        public void RecordsAffected_ReturnsNegativeOne()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            // Act
            int result = _instance.RecordsAffected;

            // Assert
            Assert.AreEqual(-1, result, "RecordsAffected should alwayds return -1");
        }

        [Test]
        public void GetName_FieldExists_LookUpSucceeds()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            // Act
            string fieldName = _instance.GetName(0);

            // Assert
            Assert.AreEqual(_FIELD_IDENTIFIER.ToString(), fieldName, "The field lookup should succeed");
        }

        [Test]
        public void GetName_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
        {
            // Arrange
            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry()
                        {
                            DisplayName = _FIELD_NAME +  Data.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
                            FieldIdentifier = _FIELD_IDENTIFIER.ToString()
                        }
                    }
                },
                _scratchRepositories);

            // Act
            string fieldName = _instance.GetName(0);

            // Assert
            Assert.AreEqual(_FIELD_IDENTIFIER.ToString(), fieldName, "The field lookup should succeed");
        }

        [Test]
        public void GetOrdinal_FieldExists_LookUpSucceeds()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            // Act
            int ordinal = _instance.GetOrdinal(_FIELD_IDENTIFIER.ToString());

            // Assert
            Assert.AreEqual(0, ordinal, "The ordinal should have been correct");
        }

        [Test]
        public void GetOrdinal_ObjectIdentifierTextInFieldExists_LookUpSucceeds()
        {
            // Arrange
            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry()
                        {
                            DisplayName = _FIELD_NAME + Data.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
                            FieldIdentifier = _FIELD_IDENTIFIER.ToString()
                        }
                    }
                },
                _scratchRepositories);

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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry()
                        {
                            DisplayName = _FIELD_NAME + Data.Constants.OBJECT_IDENTIFIER_APPENDAGE_TEXT,
                            FieldIdentifier = _FIELD_IDENTIFIER.ToString()
                        }
                    }
                },
                _scratchRepositories);

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

            _instance = CreateDataReaderTestInstance();

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
            _instance = CreateDataReaderTestInstance();

            // Act
            int fieldCount = _instance.FieldCount;

            int expectedFieldCount = _SPECIAL_FIELD_COUNT + _templateFieldEntries.Length;

            // Assert
            Assert.AreEqual(expectedFieldCount, fieldCount, $"There should be {expectedFieldCount} fields");
        }

        [Test]
        public void FieldCount_WithLongTextFields_ReturnsCorrectCount()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            // Act
            int fieldCount = _instance.FieldCount;

            int expectedFieldCount = _SPECIAL_FIELD_COUNT + _templateFieldEntries.Length;

            // Assert
            Assert.AreEqual(expectedFieldCount, fieldCount, $"There should be {expectedFieldCount} fields");
        }

        [Test]
        public void Dispose_BeforeRead_DoesNotExcept()
        {
            // Arrange
            // for retrieving long text field values (per doc)
            _exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE)
                .Returns<ArtifactDTO[]>(_templateArtifactDtos);

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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

            _instance = CreateDataReaderTestInstance();

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
            catch (Exception e)
            {
                correctExceptionThrown = e is IntegrationPointsException && e.InnerException is NullReferenceException;
            }

            // Assert
            Assert.IsTrue(correctExceptionThrown, "Reading after running Close() should nullify the current result");
        }

        [Test]
        public void GetSchemaTable_OneField_ReturnsCorrectSchema()
        {
            // Arrange
            _instance = CreateDataReaderTestInstance();

            var expectedResult = new DataTable()
            {
                Columns =
                {
                    new DataColumn(_FIELD_IDENTIFIER.ToString()),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEJOB_FIELD)
                }
            };

            // Act
            DataTable result = _instance.GetSchemaTable();

            // Arrange
            Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
        }

        [Test]
        public void GetSchemaTable_MultipleFields_ReturnsCorrectSchema()
        {
            // Arrange
            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField =   new FieldEntry() {FieldIdentifier = "123", DisplayName = "abc" },
                    },
                    new FieldMap()
                    {
                        SourceField =   new FieldEntry() {FieldIdentifier = "456", DisplayName = "def"}
                    }
                },
                _scratchRepositories);

            var expectedResult = new DataTable()
            {
                Columns =
                {
                    new DataColumn("123"),
                    new DataColumn("456"),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEJOB_FIELD)
                }
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
            _instance = CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[0],
                _scratchRepositories);

            var expectedResult = new DataTable()
            {
                Columns =
                {
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD),

                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEJOB_FIELD)
                }
            };

            // Act
            DataTable result = _instance.GetSchemaTable();

            // Arrange
            Assert.IsTrue(ArgumentMatcher.DataTablesMatch(expectedResult, result), "The schema DataTable should be correct");
        }

        [Test]
        public void GetSchemaTable_NoDocumentsNoFields_ReturnsCorrectSchema()
        {
            // Arrange
            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[0],
                _scratchRepositories);

            var expectedResult = new DataTable()
            {
                Columns =
                {
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_IMAGE_FILE_NAME_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_TYPE_FIELD),
                    new DataColumn(IntegrationPoints.Domain.Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD),
                    // new DataColumn(Contracts.Constants.SPECIAL_SOURCEJOB_FIELD)
                }
            };

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

            _instance = CreateDataReaderTestInstance();

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
                        10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }
                    }
                },
                _scratchRepositories);

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
                        10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" }
                    }
                },
                _scratchRepositories);

            // Act
            _instance.Read();
            Int16 result = _instance.GetInt16(0);

            // Arrange
            Assert.AreEqual(value, result, "The result should be correct");
        }

        [Test]
        public void Getint_GoldFlow()
        {
            // Arrange
            int value = int.MaxValue;
            // for retrieving all the documents
            _exportService.RetrieveData(_FETCH_ARTIFACTDTOS_BATCH_SIZE).Returns<ArtifactDTO[]>(new ArtifactDTO[]
            {
                new ArtifactDTO(
                    1234,
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

            // Act
            _instance.Read();
            int result = _instance.GetInt32(0);

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
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                    10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                        10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                        10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

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
                        10, "Document",
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

            _instance =CreateDataReaderTestInstanceWithParameters(
                _exportService,
                new FieldMap[]
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry() { FieldIdentifier = "123", DisplayName = "Some Number" },
                    }
                },
                _scratchRepositories);

            // Act
            _instance.Read();
            bool result = _instance.GetBoolean(0);

            // Arrange
            Assert.AreEqual(value, result, "The result should be correct");
        }
        #endregion IDataReader methods
    }
}
