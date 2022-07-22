using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PreValidation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.PreValidation
{
    [TestFixture]
    public class DestinationWorkspaceExistenceValidatorTests
    {
        private Mock<IWorkspaceManager> _workspaceManagerFake;

        private Mock<IPreValidationConfiguration> _configurationFake;

        private DestinationWorkspaceExistenceValidator _sut;

        private const int _WORKSPACE_ARTIFACT_ID = 100000;

        [SetUp]
        public void SetUp()
        {
            _configurationFake = new Mock<IPreValidationConfiguration>();
            _configurationFake.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

            _workspaceManagerFake = new Mock<IWorkspaceManager>();

            var logger = new EmptyLogger();

            var serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
            serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>())
                .ReturnsAsync(_workspaceManagerFake.Object);

            _sut = new DestinationWorkspaceExistenceValidator(serviceFactoryForAdminMock.Object, logger);
        }

        [Test]
        public async Task ValidateAsync_ShouldHandle_WhenWorkspaceExists()
        {
            // Arrange
            SetupWorkspaceExists(true);

            // Act
            var validationResult = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            validationResult.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task ValidateAsync_ShouldBeInvalid_WhenWorkspaceDoesNotExist()
        {
            // Arrange
            SetupWorkspaceExists(false);

            // Act
            var validationResult = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.Should().Contain(x => x.ShortMessage.Contains(_WORKSPACE_ARTIFACT_ID.ToString()));
        }

        [Test]
        public async Task ValidateAsync_ShouldBeInvalid_WhenExceptionWasThrown()
        {
            // Arrange
            _workspaceManagerFake
                .Setup(x => x.WorkspaceExists(It.Is<WorkspaceRef>(w => w.ArtifactID == _WORKSPACE_ARTIFACT_ID)))
                .Throws<Exception>();

            // Act
            var validationResult = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.Should().Contain(x => x.ShortMessage.Contains(_WORKSPACE_ARTIFACT_ID.ToString()));
        }

        private void SetupWorkspaceExists(bool isExists)
        {
            _workspaceManagerFake
                .Setup(x => x.WorkspaceExists(It.Is<WorkspaceRef>(w => w.ArtifactID == _WORKSPACE_ARTIFACT_ID)))
                .ReturnsAsync(isExists);
        }
    }
}
