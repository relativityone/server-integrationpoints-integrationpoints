using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
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
        private Mock<IRelativityStorageService> _storageService;
        private LoadFileBuilder _sut;
        private FakeStorageStream _fakeStorageSteam;

        private int _numberOfRecords = 5;

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
                .ReturnsAsync(new string[_numberOfRecords]);

            _sut = new LoadFileBuilder(_storageService.Object, Mock.Of<IAPILog>());
        }

        [Test]
        public async Task CreateDataFileAsync_ShouldBuildDataFile()
        {
            // Arrange
            List<IndexedFieldMap> fieldMap = new List<IndexedFieldMap>
            {
                new IndexedFieldMap(
                    new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = "Source Field"
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = "Source Field"
                        }
                    }, 0),
                new IndexedFieldMap(
                    new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = EntityFieldNames.FirstName
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = EntityFieldNames.FirstName
                        }
                    }, 1),
                new IndexedFieldMap(
                    new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = EntityFieldNames.LastName
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = EntityFieldNames.LastName
                        }
                    }, 2)
            };

            IntegrationPointDto integrationPointDto = new IntegrationPointDto
            {
                FieldMappings = fieldMap.Select(x => x.FieldMap).ToList(),
                //DestinationConfiguration = new DestinationConfiguration
                //{
                //    ArtifactTypeId = ObjectTypeIds.Entity
                //}
            };

            CustomProviderBatch batch = new CustomProviderBatch
            {
                BatchID = 5
            };

            string importDirectory = "/import/";

            IDataSourceProvider provider = new FakeSourceProvider(_numberOfRecords);

            // Act
            DataSourceSettings settings = await _sut.CreateDataFileAsync(
                batch,
                provider,
                new IntegrationPointInfo(integrationPointDto),
                importDirectory);

            // Assert
            settings.Path.Should().Be($"{importDirectory}000000{batch.BatchID}.data");

            string[] allLines = _fakeStorageSteam.LastWrittenData.Trim().Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in allLines)
            {
                string[] columns = line.Split(LoadFileOptions._DEFAULT_COLUMN_DELIMITER_ASCII);
                columns.Length.ShouldBeEquivalentTo(fieldMap.Count + 1);
                columns[fieldMap.Count].ShouldBeEquivalentTo($"Value of {EntityFieldNames.LastName}, Value of {EntityFieldNames.FirstName}");
            }

            allLines.Length.ShouldBeEquivalentTo(_numberOfRecords);
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
