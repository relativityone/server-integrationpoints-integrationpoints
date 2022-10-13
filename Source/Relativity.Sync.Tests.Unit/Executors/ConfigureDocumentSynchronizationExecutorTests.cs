using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Settings;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal class ConfigureDocumentSynchronizationExecutorTests
    {
        private Mock<IDestinationServiceFactoryForUser> _serviceFactoryMock;
        private Mock<IImportJobController> _importJobControllerMock;
        private Mock<IDocumentConfigurationController> _documentConfigurationControllerMock;
        private Mock<IFieldMappings> _fieldMapingMock;
        private Mock<IFieldManager> _fieldManagerMock;
        private Mock<IConfigureDocumentSynchronizationConfiguration> _executorConfigurationMock;

        private ConfigureDocumentSynchronizationExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();

            _importJobControllerMock = new Mock<IImportJobController>();
            _importJobControllerMock.Setup(x =>
                    x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Response(Guid.Empty, true, null, null));
            _importJobControllerMock.Setup(x =>
                    x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Response(Guid.Empty, true, null, null));

            _documentConfigurationControllerMock = new Mock<IDocumentConfigurationController>();
            _documentConfigurationControllerMock.Setup(x =>
                    x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()))
                .ReturnsAsync(new Response(Guid.Empty, true, null, null));

            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobControllerMock.Object);
            _serviceFactoryMock.Setup(x => x.CreateProxyAsync<IDocumentConfigurationController>())
                .ReturnsAsync(_documentConfigurationControllerMock.Object);

            _fieldMapingMock = new Mock<IFieldMappings>();

            _fieldManagerMock = new Mock<IFieldManager>();
            List<FieldInfoDto> specialFields = new List<FieldInfoDto>
            {
                new FieldInfoDto(SpecialFieldType.NativeFileLocation, "NativeFileLocation", "NativeFileLocation", false, true),
                new FieldInfoDto(SpecialFieldType.NativeFileFilename, "NativeFileFilename", "NativeFileFilename", false, true),
            };
            _fieldManagerMock.Setup(x => x.GetNativeSpecialFields()).Returns(specialFields);
            _fieldManagerMock.Setup(x => x.GetNativeAllFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(specialFields);

            _executorConfigurationMock = new Mock<IConfigureDocumentSynchronizationConfiguration>();

            _sut = new ConfigureDocumentSynchronizationExecutor(
                FakeHelper.CreateSyncJobParameters(),
                _serviceFactoryMock.Object,
                _fieldMapingMock.Object,
                _fieldManagerMock.Object,
                Mock.Of<IAPILog>());
        }

        [Test]
        public async Task ExecuteAsync_ShouldCreateAndConfigureAndBeginJob()
        {
            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importJobControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _documentConfigurationControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()), Times.Once);
            _importJobControllerMock.Verify(x =>
                x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);

            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_WhenJobCreationFails_ShouldFail()
        {
            _importJobControllerMock.Setup(x =>
                    x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Response(Guid.Empty, false, null, null));

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importJobControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _documentConfigurationControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()), Times.Never);
            _importJobControllerMock.Verify(x =>
                x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_WhenDocumentConfigurationFails_ShouldFail()
        {
            _documentConfigurationControllerMock.Setup(x =>
                    x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()))
                .ReturnsAsync(new Response(Guid.Empty, false, null, null));

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importJobControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _documentConfigurationControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()), Times.Once);
            _importJobControllerMock.Verify(x =>
                x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_WhenJobBeginFails_ShouldFail()
        {
            _importJobControllerMock.Setup(x =>
                    x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Response(Guid.Empty, false, null, null));

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            _importJobControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _documentConfigurationControllerMock.Verify(x =>
                x.CreateAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<ImportDocumentSettings>()), Times.Once);
            _importJobControllerMock.Verify(x =>
                x.BeginAsync(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public async Task ExecuteAsync_InCaseOfException_ShouldFail()
        {
            _serviceFactoryMock.Setup(x =>
                x.CreateProxyAsync<IImportJobController>()).ThrowsAsync(new NullReferenceException());

            ExecutionResult result = await _sut
                .ExecuteAsync(_executorConfigurationMock.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            result.Status.Should().Be(ExecutionStatus.Failed);
        }
    }
}
