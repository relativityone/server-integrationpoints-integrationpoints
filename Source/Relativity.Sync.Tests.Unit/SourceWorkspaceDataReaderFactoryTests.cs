using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderFactoryTests
	{
		private Mock<IRelativityExportBatcherFactory> _exportBatcherFactory;
		private Mock<IFieldManager> _fieldManager;
		private Mock<ISyncLog> _logger;
		private Mock<ISynchronizationConfiguration> _configuration;
		private Mock<IBatchDataReaderBuilder> _readerBuilder;
		private Mock<IItemStatusMonitor> _itemStatusMonitor;

		private SourceWorkspaceDataReaderFactory _instance;

		[SetUp]
		public void SetUp()
		{
			_exportBatcherFactory = new Mock<IRelativityExportBatcherFactory>();
			_fieldManager = new Mock<IFieldManager>();
			_logger = new Mock<ISyncLog>();
			_configuration = new Mock<ISynchronizationConfiguration>();
			_readerBuilder = new Mock<IBatchDataReaderBuilder>();
			_itemStatusMonitor = new Mock<IItemStatusMonitor>();

			_instance = new SourceWorkspaceDataReaderFactory(_exportBatcherFactory.Object, _fieldManager.Object, _configuration.Object,
				_readerBuilder.Object, _itemStatusMonitor.Object, _logger.Object);
		}

		[Test]
		public void ItShouldCreateSourceWorkspaceDataReader()
		{
			Mock<IRelativityExportBatcher> batcher = new Mock<IRelativityExportBatcher>();
			_exportBatcherFactory.Setup(x => x.CreateRelativityExportBatcher(It.IsAny<IBatch>())).Returns(batcher.Object);

			// act
			ISourceWorkspaceDataReader dataReader = _instance.CreateSourceWorkspaceDataReader(Mock.Of<IBatch>());

			// assert
			dataReader.Should().NotBeNull();
		}
	}
}