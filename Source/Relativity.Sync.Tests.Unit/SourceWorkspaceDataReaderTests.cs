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
	internal sealed class SourceWorkspaceDataReaderTests : IDisposable
	{
		private Mock<IRelativityExportBatcher> _exportBatcher;
		private SourceWorkspaceDataReader _instance;
		private Mock<ISynchronizationConfiguration> _configuration;
		private Mock<IFieldManager> _fieldManager;
		private Mock<IItemStatusMonitor> _itemStatusMonitor;
		private FieldInfoDto _identifierField;

		[SetUp]
		public void SetUp()
		{
			_exportBatcher = new Mock<IRelativityExportBatcher>();
			_exportBatcher.Setup(x => x.Start(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(Guid.NewGuid());

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

			_instance = new SourceWorkspaceDataReader(new SimpleSourceWorkspaceDataTableBuilder(_identifierField),
				_configuration.Object,
				_exportBatcher.Object,
				_fieldManager.Object,
				_itemStatusMonitor.Object,
				Mock.Of<ISyncLog>());
		}

		[TearDown]
		public void TearDown()
		{
			_instance?.Dispose();
			_instance = null;
		}

		public void Dispose()
		{
			TearDown();
		}

		[Test]
		public void ItShouldPassArgumentsToExportBatcher()
		{
			// Arrange
			const int sourceWorkspaceId = 123;
			const int syncConfigurationId = 456;
			Guid runId = Guid.NewGuid();

			_configuration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(sourceWorkspaceId);
			_configuration.SetupGet(x => x.SyncConfigurationArtifactId).Returns(syncConfigurationId);
			_configuration.SetupGet(x => x.ExportRunId).Returns(runId);

			_instance = new SourceWorkspaceDataReader(new SimpleSourceWorkspaceDataTableBuilder(_identifierField),
				_configuration.Object,
				_exportBatcher.Object,
				_fieldManager.Object,
				_itemStatusMonitor.Object,
				Mock.Of<ISyncLog>());

			// Act
			_instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.Start(runId, sourceWorkspaceId, syncConfigurationId), Times.AtLeastOnce);
		}

		[Test]
		public void ItShouldGetNextBatchWhenCurrentIsEmpty()
		{
			// Act
			_instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.GetNextAsync(It.IsAny<Guid>()));
		}

		[Test]
		public void ItShouldStartBatchingBeforeGettingNextBatch()
		{
			// Arrange
			Guid token = Guid.NewGuid();
			_exportBatcher.Setup(x => x.Start(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns(token);

			// Act
			_instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.GetNextAsync(It.Is<Guid>(t => t == token)));
		}

		[Test]
		public void ItShouldReturnTrueWhenCurrentBatchIsEmptyAndNextExists()
		{
			// Arrange
			const int batchSize = 1;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

			// Act
			bool result = _instance.Read();

			// Assert
			result.Should().Be(true);
		}

		[Test]
		public void ItShouldReturnTrueWhenCurrentBatchHasMoreData()
		{
			// Arrange
			const int batchSize = 2;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

			// Act
			_instance.Read();
			bool result = _instance.Read();

			// Assert
			result.Should().Be(true);
		}

		[Test]
		public void ItShouldNotGetNextBatchWhenCurrentBatchHasMoreData()
		{
			// Arrange
			const int batchSize = 2;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

			// Act
			_instance.Read();
			_instance.Read();

			// Assert
			_exportBatcher.Verify(x => x.GetNextAsync(It.IsAny<Guid>()), Times.Exactly(1));
		}

		[Test]
		public void ItShouldReturnFalseWhenCurrentBatchIsEmptyAndNextBatchIsNull()
		{
			// Arrange
			const int batchSize = 1;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), null);

			// Act
			_instance.Read();
			bool result = _instance.Read();

			// Assert
			result.Should().Be(false);
		}

		[Test]
		public void ItShouldReturnFalseWhenCurrentBatchIsEmptyAndNextBatchIsEmpty()
		{
			// Arrange
			const int batchSize = 1;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

			// Act
			_instance.Read();
			bool result = _instance.Read();

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

			// Act
			bool allObjectReads = Enumerable.Range(0, batchSize * numBatches).All(_ => _instance.Read());
			bool finalEmptyRead = _instance.Read();

			// Assert
			allObjectReads.Should().Be(true);
			finalEmptyRead.Should().Be(false);
		}

		[Test]
		public void ItShouldNotThrowOnMultipleDisposeCalls()
		{
			// Act/Assert
			_instance.Dispose();
			_instance.Dispose();
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
			// Act
			List<List<object>> results = new List<List<object>>();
			while (_instance.Read())
			{
				results.Add(new List<object> {_instance[0], _instance[1], _instance[2]});
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
			_exportBatcher.Setup(x => x.GetNextAsync(It.IsAny<Guid>()))
				.Throws(new ServiceException("Foo"));

			// Act
			SourceDataReaderException thrownException = Assert.Throws<SourceDataReaderException>(() => _instance.Read());

			// Assert
			thrownException.InnerException.Should().BeOfType<ServiceException>();
		}

		[Test]
		public void ItShouldThrowProperExceptionWhenTableBuilderThrows()
		{
			// Arrange
			const int batchSize = 1;
			ExportBatcherReturnsBatches(GenerateBatch(batchSize), EmptyBatch());

			Mock<ISourceWorkspaceDataTableBuilder> builder = new Mock<ISourceWorkspaceDataTableBuilder>();
			builder.Setup(x => x.BuildAsync(It.IsAny<int>(), It.IsAny<RelativityObjectSlim[]>(), CancellationToken.None))
				.Throws(new ServiceException());

			_instance = new SourceWorkspaceDataReader(builder.Object,
				_configuration.Object,
				_exportBatcher.Object,
				_fieldManager.Object,
				_itemStatusMonitor.Object,
				Mock.Of<ISyncLog>());

			// Act
			SourceDataReaderException thrownException = Assert.Throws<SourceDataReaderException>(() => _instance.Read());

			// Assert
			thrownException.InnerException.Should().BeOfType<ServiceException>();
		}

		private void ExportBatcherReturnsBatches(params RelativityObjectSlim[][] batches)
		{
			ISetupSequentialResult<Task<RelativityObjectSlim[]>> setupAssertion = _exportBatcher.SetupSequence(x => x.GetNextAsync(It.IsAny<Guid>()));
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
