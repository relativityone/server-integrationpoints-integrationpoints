using System.Threading;
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
        private Mock<IRelativityExportBatcherFactory> _exportBatcherFactoryFake;
        private SourceWorkspaceDataReaderFactory _instance;

        [SetUp]
        public void SetUp()
        {
            _exportBatcherFactoryFake = new Mock<IRelativityExportBatcherFactory>();
            Mock<IFieldManager> fieldManager = new Mock<IFieldManager>();
            Mock<ISynchronizationConfiguration> configuration = new Mock<ISynchronizationConfiguration>();
            Mock<IExportDataSanitizer> dataSanitizer = new Mock<IExportDataSanitizer>();

            _instance = new SourceWorkspaceDataReaderFactory(_exportBatcherFactoryFake.Object, fieldManager.Object, configuration.Object,
                dataSanitizer.Object, new EmptyLogger());
        }

        [Test]
        public void CreateNativeSourceWorkspaceDataReader_ShouldCreateNativeSourceWorkspaceDataReader()
        {
            Mock<IRelativityExportBatcher> batcher = new Mock<IRelativityExportBatcher>();
            _exportBatcherFactoryFake.Setup(x => x.CreateRelativityExportBatcher(It.IsAny<IBatch>())).Returns(batcher.Object);

            // act
            ISourceWorkspaceDataReader dataReader = _instance.CreateNativeSourceWorkspaceDataReader(Mock.Of<IBatch>(), CancellationToken.None);

            // assert
            dataReader.Should().NotBeNull();
        }

        [Test]
        public void CreateImageSourceWorkspaceDataReader_ShouldCreateImageSourceWorkspaceDataReader()
        {
            Mock<IRelativityExportBatcher> batcher = new Mock<IRelativityExportBatcher>();
            _exportBatcherFactoryFake.Setup(x => x.CreateRelativityExportBatcher(It.IsAny<IBatch>())).Returns(batcher.Object);

            // act
            ISourceWorkspaceDataReader dataReader = _instance.CreateImageSourceWorkspaceDataReader(Mock.Of<IBatch>(), CancellationToken.None);

            // assert
            dataReader.Should().NotBeNull();
        }
    }
}
