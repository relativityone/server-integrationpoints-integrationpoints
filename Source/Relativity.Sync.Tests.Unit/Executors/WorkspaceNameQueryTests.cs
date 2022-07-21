using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public sealed class WorkspaceNameQueryTests
    {
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUserMock;

        private WorkspaceNameQuery _sut;
        private Mock<IWorkspaceManager> _workspaceManagerMock;

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryForUserMock = new Mock<ISourceServiceFactoryForUser>();
            _workspaceManagerMock = new Mock<IWorkspaceManager>();

            _serviceFactoryForUserMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>())
                .Returns(Task.FromResult(_workspaceManagerMock.Object));

            _sut = new WorkspaceNameQuery(new EmptyLogger());
        }

        [Test]
        public void ItShouldThrowExceptionWhenQueryFails()
        {
            // Arrange
            _workspaceManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>())).Throws<InvalidOperationException>();
            
            // Act
            Func<Task> action = async () => await _sut.GetWorkspaceNameAsync(_serviceFactoryForUserMock.Object, 1, CancellationToken.None).ConfigureAwait(false);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ItShouldThrowSyncExceptionWhenQueryReturnsNoResults()
        {
            // Arrange
            _workspaceManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>())).Returns(Task.FromResult<WorkspaceResponse>(null));

            // Act
            Func<Task> action = async () => await _sut.GetWorkspaceNameAsync(_serviceFactoryForUserMock.Object, 1, CancellationToken.None).ConfigureAwait(false);

            // Assert
            action.Should().Throw<SyncException>();
        }

        [Test]
        public async Task ItShouldReturnWorkspaceName()
        {
            // Arrange
            string expectedWorkspaceName = "workspace name";

            _workspaceManagerMock.Setup(x => x.ReadAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(new WorkspaceResponse {Name = expectedWorkspaceName}));

            // Act
            string actualWorkspaceName = await _sut.GetWorkspaceNameAsync(_serviceFactoryForUserMock.Object, 1, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expectedWorkspaceName, actualWorkspaceName);

        }
    }
}