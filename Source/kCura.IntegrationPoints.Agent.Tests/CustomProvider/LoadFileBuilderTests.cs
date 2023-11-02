﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Storage;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class LoadFileBuilderTests
    {
        private const string _IMPORT_DIRECTORY = "/import/";

        private Mock<IRelativityStorageService> _storageService;
        private Mock<IEntityFullNameService> _entityFullNameService;
        private LoadFileBuilder _sut;
        private FakeStorageStream _fakeStorageSteam;
        private CustomProviderBatch _batch;
        private IDataSourceProvider _provider;

        private const int NumberOfRecords = 2;

        [SetUp]
        public void SetUp()
        {
            _storageService = new Mock<IRelativityStorageService>();

            _storageService
                .Setup(x => x.CreateFileOrTruncateExistingAsync(It.IsAny<string>()))
                .Returns((string path) =>
                {
                    _fakeStorageSteam = new FakeStorageStream(path);
                    return Task.FromResult(_fakeStorageSteam as StorageStream);
                });

            _storageService
                .Setup(x => x.ReadAllLinesAsync(It.IsAny<string>()))
                .ReturnsAsync(new string[NumberOfRecords]);

            _entityFullNameService = new Mock<IEntityFullNameService>();

            _batch = new CustomProviderBatch
            {
                BatchID = 5
            };

            _provider = new FakeSourceProvider(NumberOfRecords);

            _sut = new LoadFileBuilder(_storageService.Object, _entityFullNameService.Object, Mock.Of<IAPILog>());
        }

        [Test]
        public async Task CreateDataFileAsync_ShouldBuildDataFile()
        {
            // Arrange
            var fieldName = "Source Field";
            List<IndexedFieldMap> fieldMap = CreateIndexedFieldMaps(new[] { fieldName });
            IntegrationPointDto integrationPointDto = new IntegrationPointDto
            {
                FieldMappings = fieldMap.Select(x => x.FieldMap).ToList(),
                DestinationConfiguration = new Synchronizers.RDO.DestinationConfiguration()
            };

            // Act
            DataSourceSettings settings = await _sut.CreateDataFileAsync(
                _batch,
                _provider,
                new IntegrationPointInfo(integrationPointDto),
                _IMPORT_DIRECTORY);

            // Assert
            Assert(
                settings,
                fieldMap.Count);

            _entityFullNameService
                .Verify(x => x.FormatFullName(It.IsAny<Dictionary<string, IndexedFieldMap>>(), It.IsAny<IDataReader>()), Times.Never);
        }

        [Test]
        public async Task CreateDataFileAsync_ShouldBuildDataFile_WhenImportingEntity_WithoutFullNameMapped()
        {
            // Arrange
            _entityFullNameService
                .Setup(x => x.FormatFullName(It.IsAny<Dictionary<string, IndexedFieldMap>>(), It.IsAny<IDataReader>()))
                .Returns("Bogdan Boner");

            List<IndexedFieldMap> fieldMap = CreateIndexedFieldMaps(new[]
            {
                "UniqueID",
                EntityFieldNames.FirstName,
                EntityFieldNames.LastName
            });

            IntegrationPointDto integrationPointDto = new IntegrationPointDto
            {
                FieldMappings = fieldMap.Select(x => x.FieldMap).ToList(),
                DestinationConfiguration = new Synchronizers.RDO.DestinationConfiguration()
            };

            IndexedFieldMap fullName = new IndexedFieldMap(new FieldMap()
            {
                SourceField = new FieldEntry()
                {
                    FieldIdentifier = EntityFieldNames.FullName,
                    DisplayName = EntityFieldNames.FullName
                },
                DestinationField = new FieldEntry()
                {
                    FieldIdentifier = EntityFieldNames.FullName,
                    DisplayName = EntityFieldNames.FullName
                }
            }, FieldMapType.EntityFullName, fieldMap.Count);

            var integrationPointInfo = new IntegrationPointInfo(integrationPointDto);

            integrationPointInfo.FieldMap.Add(fullName);

            // Act
            DataSourceSettings settings = await _sut.CreateDataFileAsync(
                _batch,
                _provider,
                integrationPointInfo,
                _IMPORT_DIRECTORY);

            // Assert
            Assert(settings, integrationPointInfo.FieldMap.Count);
            _entityFullNameService
                .Verify(x =>
                    x.FormatFullName(It.Is<Dictionary<string, IndexedFieldMap>>(dict => dict.ContainsKey("Full Name")), It.IsAny<IDataReader>()),
                    Times.Exactly(NumberOfRecords));
        }

        private void Assert(DataSourceSettings settings, int expectedColumnsCount)
        {
            settings.Path.Should().Be($"{_IMPORT_DIRECTORY}000000{_batch.BatchID}.data");
            string[] allLines = _fakeStorageSteam.LastWrittenData.Trim().Split('\r').Select(x => x.Trim()).ToArray();
            foreach (string line in allLines)
            {
                string[] columns = line.Split(LoadFileOptions._DEFAULT_COLUMN_DELIMITER_ASCII);
                columns.Length.ShouldBeEquivalentTo(expectedColumnsCount);
            }
            allLines.Length.ShouldBeEquivalentTo(NumberOfRecords);
        }

        private static List<IndexedFieldMap> CreateIndexedFieldMaps(string[] fieldsNames)
        {
            var fieldMap = new List<IndexedFieldMap>();

            for (int i = 0; i < fieldsNames.Length; i++)
            {
                FieldMap field = new FieldMap
                {
                    SourceField = new FieldEntry
                    {
                        FieldIdentifier = fieldsNames[i],
                        DisplayName = fieldsNames[i]
                    },
                    DestinationField = new FieldEntry
                    {
                        FieldIdentifier = fieldsNames[i],
                        DisplayName = fieldsNames[i]
                    }
                };

                fieldMap.Add(new IndexedFieldMap(field, FieldMapType.Normal, i));
            }

            return fieldMap;
        }

        private class FakeSourceProvider : IDataSourceProvider
        {
            private readonly int _numberOfRecords;

            public FakeSourceProvider(int numberOfRecords)
            {
                _numberOfRecords = numberOfRecords;
            }

            public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
            {
                throw new System.NotImplementedException();
            }

            public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
            {
                return new FakeDataSourceReader(_numberOfRecords);
            }

            public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
            {
                throw new System.NotImplementedException();
            }
        }

        private class FakeDataSourceReader : IDataReader
        {
            private readonly int _numberOfRecords;

            private int _currentRecord = 0;

            public FakeDataSourceReader(int numberOfRecords)
            {
                _numberOfRecords = numberOfRecords;
            }

            public void Dispose()
            {
            }

            public string GetName(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public object GetValue(int i)
            {
                throw new NotImplementedException();
            }

            public int GetValues(object[] values)
            {
                throw new NotImplementedException();
            }

            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public int FieldCount { get; }

            public object this[int i] => throw new NotImplementedException();

            public object this[string name] => $"Value of {name}";

            public void Close()
            {
                throw new NotImplementedException();
            }

            public DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            public bool NextResult()
            {
                throw new NotImplementedException();
            }

            public bool Read()
            {
                return _currentRecord++ < _numberOfRecords;
            }

            public int Depth { get; }

            public bool IsClosed { get; }

            public int RecordsAffected { get; }

        }

        private class FakeStorageStream : StorageStream
        {
            public string LastWrittenData { get; set; }

            public FakeStorageStream(string storagePath)
            {
                StoragePath = storagePath;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                LastWrittenData = Encoding.ASCII.GetString(buffer, offset, count);
            }

            public override bool CanRead { get; }

            public override bool CanSeek { get; }

            public override bool CanWrite => true;

            public override long Length { get; }

            public override long Position { get; set; }

            public override string StoragePath { get; }

            public override StorageInterface StorageInterface { get; }
        }
    }
}
