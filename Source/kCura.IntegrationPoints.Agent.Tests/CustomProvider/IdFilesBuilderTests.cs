using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    public class IdFilesBuilderTests
    {
        private const int _BATCH_SIZE = 10;

        private Mock<IInstanceSettings> _instanceSettings;
        private Mock<IRelativityStorageService> _storageService;
        private FakeStream _stream;

        [SetUp]
        public void SetUp()
        {
            _instanceSettings = new Mock<IInstanceSettings>();
            _instanceSettings.Setup(x => x.GetCustomProviderBatchSizeAsync()).ReturnsAsync(_BATCH_SIZE);

            _stream = new FakeStream();

            _storageService = new Mock<IRelativityStorageService>();
            _storageService
                .Setup(x => x.CreateFileOrTruncateExistingAsync(It.IsAny<string>()))
                .ReturnsAsync(_stream);
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(9, 1)]
        [TestCase(10, 1)]
        [TestCase(11, 2)]
        [TestCase(19, 2)]
        [TestCase(20, 2)]
        public async Task BuildIdFiles_ShouldCreateBatches(int numberOfRecords, int expectedNumberOfBatches)
        {
            // Arrange
            IDataSourceProvider sourceProvider = PrepareSourceProvider(numberOfRecords);
            IdFilesBuilder sut = PrepareSut();

            // Act
            List<CustomProviderBatch> batches = await sut.BuildIdFilesAsync(sourceProvider, PrepareIntegrationPointDto(), "//fake/path");

            // Assert
            batches.Count.Should().Be(expectedNumberOfBatches);
        }

        private IDataSourceProvider PrepareSourceProvider(int numberOfRecords)
        {
            return new FakeDataSourceProvider(numberOfRecords);
        }

        private IdFilesBuilder PrepareSut()
        {
            return new IdFilesBuilder(_instanceSettings.Object, _storageService.Object, Mock.Of<IAPILog>());
        }

        private IntegrationPointDto PrepareIntegrationPointDto()
        {
            return new IntegrationPointDto()
            {
                FieldMappings = new List<FieldMap>()
                {
                    new FieldMap()
                    {
                        SourceField = new FieldEntry(),
                        DestinationField = new FieldEntry(),
                        FieldMapType = FieldMapTypeEnum.Identifier
                    }
                }
            };
        }

        private class FakeDataSourceProvider : IDataSourceProvider
        {
            private readonly int _numberOfRecords;

            public FakeDataSourceProvider(int numberOfRecords)
            {
                _numberOfRecords = numberOfRecords;
            }

            public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
            {
                throw new System.NotImplementedException();
            }

            public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
            {
                throw new System.NotImplementedException();
            }

            public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
            {
                return new FakeDataReader(_numberOfRecords);
            }
        }

        private class FakeDataReader : IDataReader
        {
            private int _current = 0;

            private readonly int _numberOfRecords;

            public FakeDataReader(int numberOfRecords)
            {
                _numberOfRecords = numberOfRecords;
            }

            public bool Read()
            {
                if (_current == _numberOfRecords)
                {
                    return false;
                }

                _current++;
                return true;
            }

            public string GetString(int i)
            {
                return _current.ToString();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
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

            public object this[string name] => throw new NotImplementedException();

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

            public int Depth { get; }
            public bool IsClosed { get; }
            public int RecordsAffected { get; }
        }

        private class FakeStream : StorageStream
        {
            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return 0;
            }

            public override void SetLength(long value)
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            public override bool CanWrite => true;

            public override bool CanRead { get; }
            public override bool CanSeek { get; }
            public override long Length { get; }
            public override long Position { get; set; }
            public override string StoragePath { get; }
            public override StorageInterface StorageInterface { get; }
        }
    }
}