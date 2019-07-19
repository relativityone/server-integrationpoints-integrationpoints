﻿using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderFactoryTests
	{
		private Mock<IRelativityExportBatcherFactory> _exportBatcherFactory;

		private SourceWorkspaceDataReaderFactory _instance;

		[SetUp]
		public void SetUp()
		{
			_exportBatcherFactory = new Mock<IRelativityExportBatcherFactory>();
			Mock<IFieldManager> fieldManager = new Mock<IFieldManager>();
			Mock<ISynchronizationConfiguration> configuration = new Mock<ISynchronizationConfiguration>();
			Mock<IBatchDataReaderBuilder> readerBuilder = new Mock<IBatchDataReaderBuilder>();
			Mock<IItemStatusMonitor> itemStatusMonitor = new Mock<IItemStatusMonitor>();

			_instance = new SourceWorkspaceDataReaderFactory(_exportBatcherFactory.Object, fieldManager.Object, configuration.Object,
				readerBuilder.Object, itemStatusMonitor.Object, new EmptyLogger());
		}

		[Test]
		public void ItShouldCreateSourceWorkspaceDataReader()
		{
			Mock<IRelativityExportBatcher> batcher = new Mock<IRelativityExportBatcher>();
			_exportBatcherFactory.Setup(x => x.CreateRelativityExportBatcher(It.IsAny<IBatch>())).Returns(batcher.Object);

			// act
			ISourceWorkspaceDataReader dataReader = _instance.CreateSourceWorkspaceDataReader(Mock.Of<IBatch>(), CancellationToken.None);

			// assert
			dataReader.Should().NotBeNull();
		}
	}
}