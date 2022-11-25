using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class SourceWorkspaceDataReaderTests
    {
        private Mock<IRelativityExportBatcher> _exportBatcher;
        private Mock<ISynchronizationConfiguration> _configuration;
        private Mock<IFieldManager> _fieldManager;
        private Mock<IItemStatusMonitor> _itemStatusMonitor;
        private FieldInfoDto _identifierField;

        [SetUp]
        public void SetUp()
        {
            _exportBatcher = new Mock<IRelativityExportBatcher>();
            Mock<IRelativityExportBatcherFactory> exportBatcherFactory = new Mock<IRelativityExportBatcherFactory>();
            exportBatcherFactory.Setup(x => x.CreateRelativityExportBatcher(It.IsAny<Batch>()))
                .Returns(_exportBatcher.Object);

            _identifierField = FieldInfoDto.DocumentField("IdentifierField", "IdentifierField", true);
            _identifierField.DocumentFieldIndex = 0;
            _fieldManager = new Mock<IFieldManager>();
            _fieldManager.Setup(m => m.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_identifierField);
            _itemStatusMonitor = new Mock<IItemStatusMonitor>();

            _configuration = new Mock<ISynchronizationConfiguration>();
            _configuration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(0);
            _configuration.SetupGet(x => x.DestinationWorkspaceTagArtifactId).Returns(0);
            _configuration.SetupGet(x => x.ExportRunId).Returns(Guid.Empty);
            _configuration.SetupGet(x => x.JobHistoryArtifactId).Returns(0);
            _configuration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(0);
        }

        [Test]
        public void Read_ShouldGetNextItemsForBatch_WhenCurrentIsEmpty()
        {
            // Arrange
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            instance.Read();

            // Assert
            _exportBatcher.Verify(x => x.GetNextItemsFromBatchAsync());
        }

        [Test]
        public void Read_ShouldReturnTrue_WhenCurrentBatchIsEmptyAndNextExists()
        {
            // Arrange
            const int batchSize = 1;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            bool result = instance.Read();

            // Assert
            result.Should().Be(true);
        }

        [Test]
        public void Read_ShouldReturnTrue_WhenCurrentBatchHasMoreData()
        {
            // Arrange
            const int batchSize = 2;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            instance.Read();
            bool result = instance.Read();

            // Assert
            result.Should().Be(true);
        }

        [Test]
        public void Read_ShouldNotGetNextBatch_WhenCurrentBatchHasMoreData()
        {
            // Arrange
            const int batchSize = 2;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            instance.Read();
            instance.Read();

            // Assert
            _exportBatcher.Verify(x => x.GetNextItemsFromBatchAsync(), Times.Exactly(1));
        }

        [Test]
        public void Dispose_ShouldDisposePreviousBlock_WhenNewOneIsCreated()
        {
            // Arrange
            const int batchSize = 2;
            const int numberOfBatches = 5;

            RelativityObjectSlim[][] batches = Enumerable.Range(1, numberOfBatches)
                .Select(i => GenerateBatch(batchSize))
                .ToArray();
            ExportBatcherReturnsBatches(batches);

            IList<Mock<IDataReader>> mocks = new List<Mock<IDataReader>>();
            bool previousReaderWasDisposed = true;
            var dataReaderBuilder = new Mock<IBatchDataReaderBuilder>();
            dataReaderBuilder
                .Setup(x => x.BuildAsync(It.IsAny<int>(), It.IsAny<RelativityObjectSlim[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => 
            {
                previousReaderWasDisposed.Should().BeTrue();
                previousReaderWasDisposed = false;
                var dataReaderMock = new Mock<IDataReader>();
                dataReaderMock.Setup(x => x.Dispose()).Callback(() => previousReaderWasDisposed = true);
                mocks.Add(dataReaderMock);
                return new SimpleBatchDataReader(dataReaderMock.Object);
            });

            SourceWorkspaceDataReader instance = BuildInstanceUnderTest(dataReaderBuilder.Object);

            // Act
            Enumerable.Range(0, numberOfBatches).ForEach(i => instance.Read());
            instance.Dispose();

            // Assert
            mocks.Count.Should().Be(numberOfBatches);
            mocks.ForEach(m => m.Verify(x => x.Dispose(), Times.AtLeastOnce));
        }

        [Test]
        public void Read_ShouldReturnFalse_WhenCurrentBatchIsEmptyAndNextBatchIsNull()
        {
            // Arrange
            const int batchSize = 1;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), null);
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            instance.Read();
            bool result = instance.Read();

            // Assert
            result.Should().Be(false);
        }

        [Test]
        public void Read_ShouldDisposeReader_WhenDrainStoppingAndBatchDataReaderFinishedWork()
        {
            // Arrange
            const int batchSize = 2;
            const int numberOfBatches = 5;

            RelativityObjectSlim[][] batches = Enumerable.Range(1, numberOfBatches)
                .Select(i => GenerateBatch(batchSize))
                .ToArray();
            ExportBatcherReturnsBatches(batches);

            Mock<IBatchDataReader> batchDataReader = new Mock<IBatchDataReader>();
            batchDataReader.SetupGet(x => x.CanCancel).Returns(true);
            Mock<IBatchDataReaderBuilder> batchDataReaderBuilder = new Mock<IBatchDataReaderBuilder>();
            batchDataReaderBuilder.Setup(x => x.BuildAsync(It.IsAny<int>(), It.IsAny<RelativityObjectSlim[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(batchDataReader.Object);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            SourceWorkspaceDataReader sut = BuildInstanceUnderTest(batchDataReaderBuilder.Object, tokenSource.Token);

            // Act
            sut.Read();
            tokenSource.Cancel();
            bool read = sut.Read();

            // Assert
            read.Should().BeFalse();
            batchDataReader.Verify(x => x.Dispose());
        }

        [Test]
        public void Read_ShouldReturnFalse_WhenCancelled()
        {
            // Arrange
            const int batchSize = 2;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            IAPILog log = new EmptyLogger();

            var dataReader = new SourceWorkspaceDataReader(new SimpleBatchDataReaderBuilder(_identifierField), 
                _configuration.Object,
                _exportBatcher.Object,
                _fieldManager.Object,
                new ItemLevelErrorLogAggregator(log),
                _itemStatusMonitor.Object,
                new EmptyLogger(),
                tokenSource.Token);

            // Act
            bool firstCall = dataReader.Read();
            tokenSource.Cancel();
            bool secondCall = dataReader.Read();

            // Assert
            firstCall.Should().BeTrue();
            secondCall.Should().BeFalse();
        }

        [Test]
        public void Read_ShouldReturnFalse_WhenCurrentBatchIsEmptyAndNextBatchIsEmpty()
        {
            // Arrange
            const int batchSize = 1;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            instance.Read();
            bool result = instance.Read();

            // Assert
            result.Should().Be(false);
        }

        [Test]
        public void Read_ShouldWorkAcrossMultipleBatches()
        {
            // Arrange
            const int batchSize = 5;
            const int numBatches = 3;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), GenerateBatch(batchSize), GenerateBatch(batchSize), EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            bool allObjectReads = Enumerable.Range(0, batchSize * numBatches).All(_ => instance.Read());
            bool finalEmptyRead = instance.Read();

            // Assert
            allObjectReads.Should().Be(true);
            finalEmptyRead.Should().Be(false);
        }

        [Test]
        public void Dispose_ShouldNotThrow_WhenMultipleDisposeCalls()
        {
            // Arrange
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            Action action = () =>
            {
                instance.Dispose();
                instance.Dispose();
            };

            // Assert
            action.Should().NotThrow();
        }

        [Test]
        public void Read_ShouldAccessUnderlyingDataReader()
        {
#pragma warning disable RG2009 // need to use a lot of literal indices here, and making them consts is absurd

            // Arrange
            RelativityObjectSlim[] batch =
            {
                CreateObjectWithValues("foo", 1, true),
                CreateObjectWithValues("baz", 0, true),
                CreateObjectWithValues("ban", 2, false)
            };
            ExportBatcherReturnsBatches(batch, EmptyBatch());
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            List<List<object>> results = new List<List<object>>();
            while (instance.Read())
            {
                results.Add(new List<object> { instance[0], instance[1], instance[2] });
            }

            // Assert
            const int expectedCount = 3;
            results.Count.Should().Be(expectedCount);

            results[0][0].Should().Be("foo");
            results[0][1].Should().Be(1);
            results[0][2].Should().Be(true);
            results[1][0].Should().Be("baz");
            results[1][1].Should().Be(0);
            results[1][2].Should().Be(true);
            results[2][0].Should().Be("ban");
            results[2][1].Should().Be(2);
            results[2][2].Should().Be(false);

#pragma warning restore RG2009 // need to use a lot of literal indices here, and making them consts is absurd
        }

        [Test]
        public void Read_ShouldThrowProperException_WhenExportBatcherThrows()
        {
            // Arrange
            _exportBatcher.Setup(x => x.GetNextItemsFromBatchAsync())
                .Throws(new ServiceException("Foo"));
            SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

            // Act
            SourceDataReaderException thrownException = Assert.Throws<SourceDataReaderException>(() => instance.Read());

            // Assert
            thrownException.InnerException.Should().BeOfType<ServiceException>();
        }

        [Test]
        public void Read_ShouldThrowProperException_WhenTableBuilderThrows()
        {
            // Arrange
            const int batchSize = 1;
            ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

            Mock<IBatchDataReaderBuilder> builder = new Mock<IBatchDataReaderBuilder>();
            builder.Setup(x => x.BuildAsync(It.IsAny<int>(), It.IsAny<RelativityObjectSlim[]>(), CancellationToken.None))
                .Throws(new ServiceException());

            SourceWorkspaceDataReader instance = BuildInstanceUnderTest(builder.Object);

            // Act
            SourceDataReaderException thrownException = Assert.Throws<SourceDataReaderException>(() => instance.Read());

            // Assert
            thrownException.InnerException.Should().BeOfType<ServiceException>();
        }

        // We use these builder methods instead of a dedicated instance variable b/c the data reader
        // implements IDisposable, so we would have to make the entire test fixture IDisposable if we
        // kept around a reference.

        private SourceWorkspaceDataReader BuildInstanceUnderTest()
        {
            return BuildInstanceUnderTest(new SimpleBatchDataReaderBuilder(_identifierField));
        }

        private SourceWorkspaceDataReader BuildInstanceUnderTest(IBatchDataReaderBuilder dataTableBuilder)
        {
            return BuildInstanceUnderTest(dataTableBuilder, CancellationToken.None);
        }

        private SourceWorkspaceDataReader BuildInstanceUnderTest(IBatchDataReaderBuilder dataTableBuilder, CancellationToken token)
        {
            IAPILog log = new EmptyLogger();

            return new SourceWorkspaceDataReader(
                dataTableBuilder,
                _configuration.Object,
                _exportBatcher.Object,
                _fieldManager.Object,
                new ItemLevelErrorLogAggregator(log),
                _itemStatusMonitor.Object,
                log,
                token);
        }

        private void ExportBatcherReturnsBatches(params RelativityObjectSlim[][] batches)
        {
            ISetupSequentialResult<Task<RelativityObjectSlim[]>> setupAssertion = _exportBatcher.SetupSequence(x => x.GetNextItemsFromBatchAsync());
            foreach (RelativityObjectSlim[] batch in batches)
            {
                setupAssertion.ReturnsAsync(batch);
            }
        }

        private static RelativityObjectSlim[] GenerateBatch(int size, int numValues = 1)
        {
            return Enumerable.Range(0, size)
                .Select(_ => GenerateObject(numValues))
                .ToArray();
        }

        private static RelativityObjectSlim[] EmptyBatch() => Array.Empty<RelativityObjectSlim>();

        private static RelativityObjectSlim GenerateObject(int numValues)
        {
            var obj = new RelativityObjectSlim
            {
                ArtifactID = Guid.NewGuid().GetHashCode(),
                Values = Enumerable.Range(0, numValues).Select(_ => new object()).ToList()
            };
            return obj;
        }

        private static RelativityObjectSlim CreateObjectWithValues(params object[] values)
        {
            return new RelativityObjectSlim { Values = values.ToList() };
        }
    }
}
