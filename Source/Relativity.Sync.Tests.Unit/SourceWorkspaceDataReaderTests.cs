using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderTests
	{
		private Mock<RelativityExportBatcherFactory> _exportBatcherFactory;
		private Mock<IRelativityExportBatcher> _exportBatcher;
		private Mock<ISynchronizationConfiguration> _configuration;
		private Mock<IFieldManager> _fieldManager;
		private Mock<IItemStatusMonitor> _itemStatusMonitor;
		private FieldInfoDto _identifierField;

		[SetUp]
		public void SetUp()
		{
			_exportBatcher = new Mock<IRelativityExportBatcher>();
			_exportBatcherFactory = new Mock<RelativityExportBatcherFactory>();
			_exportBatcherFactory.Setup(x => x(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(_exportBatcher.Object);

			_identifierField = FieldInfoDto.DocumentField("IdentifierField", true);
			_identifierField.DocumentFieldIndex = 0;
			_fieldManager = new Mock<IFieldManager>();
			_fieldManager.Setup(m => m.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_identifierField);
			_itemStatusMonitor = new Mock<IItemStatusMonitor>();

			_configuration = new Mock<ISynchronizationConfiguration>();
			_configuration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(0);
			_configuration.SetupGet(x => x.DestinationWorkspaceTagArtifactId).Returns(0);
			_configuration.SetupGet(x => x.ExportRunId).Returns(Guid.Empty);
			_configuration.SetupGet(x => x.FieldMappings).Returns(new List<FieldMap>());
			_configuration.SetupGet(x => x.JobHistoryTagArtifactId).Returns(0);
			_configuration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(0);
		}

		[Test]
		public void ItShouldPassArgumentsToExportBatcherFactory()
		{
			// Arrange
			Guid runId = Guid.NewGuid();
			const int sourceWorkspaceId = 123;
			const int syncConfigurationId = 456;

			_configuration.SetupGet(x => x.ExportRunId).Returns(runId);
			_configuration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceId);
			_configuration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(syncConfigurationId);

			// Act
			SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

			// Assert
			_exportBatcherFactory.Verify(x => x(runId, sourceWorkspaceId, syncConfigurationId), Times.Once);
		}

		[Test]
		public void ItShouldGetNextBatchWhenCurrentIsEmpty()
		{
			// Arrange
			SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

			// Act
			instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.GetNextBatchAsync());
		}

		[Test]
		public void ItShouldReturnTrueWhenCurrentBatchIsEmptyAndNextExists()
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
		public void ItShouldReturnTrueWhenCurrentBatchHasMoreData()
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
		public void ItShouldNotGetNextBatchWhenCurrentBatchHasMoreData()
		{
			// Arrange
			const int batchSize = 2;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());
			SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

			// Act
			instance.Read();
			instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.GetNextBatchAsync(), Times.Exactly(1));
		}

		[Test]
		public void ItShouldReturnFalseWhenCurrentBatchIsEmptyAndNextBatchIsNull()
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
		public void ItShouldReturnFalseWhenCurrentBatchIsEmptyAndNextBatchIsEmpty()
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
		public void ItShouldWorkAcrossMultipleBatches()
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
		public void ItShouldNotThrowOnMultipleDisposeCalls()
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
		public void ItShouldAccessUnderlyingDataReader()
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
		public void ItShouldThrowProperExceptionWhenExportBatcherThrows()
		{
			// Arrange
			_exportBatcher.Setup(x => x.GetNextBatchAsync())
				.Throws(new ServiceException("Foo"));
			SourceWorkspaceDataReader instance = BuildInstanceUnderTest();

			// Act
			SourceDataReaderException thrownException = Assert.Throws<SourceDataReaderException>(() => instance.Read());

			// Assert
			thrownException.InnerException.Should().BeOfType<ServiceException>();
		}

		[Test]
		public void ItShouldThrowProperExceptionWhenTableBuilderThrows()
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
			return new SourceWorkspaceDataReader(new SimpleBatchDataReaderBuilder(_identifierField), 
				_configuration.Object,
				_exportBatcherFactory.Object,
				_fieldManager.Object,
				_itemStatusMonitor.Object,
				Mock.Of<ISyncLog>());
		}

		private SourceWorkspaceDataReader BuildInstanceUnderTest(IBatchDataReaderBuilder dataTableBuilder)
		{
			return new SourceWorkspaceDataReader(dataTableBuilder,
				_configuration.Object,
				_exportBatcherFactory.Object,
				_fieldManager.Object,
				_itemStatusMonitor.Object,
				Mock.Of<ISyncLog>());
		}

		private void ExportBatcherReturnsBatches(params RelativityObjectSlim[][] batches)
		{
			ISetupSequentialResult<Task<RelativityObjectSlim[]>> setupAssertion = _exportBatcher.SetupSequence(x => x.GetNextBatchAsync());
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
