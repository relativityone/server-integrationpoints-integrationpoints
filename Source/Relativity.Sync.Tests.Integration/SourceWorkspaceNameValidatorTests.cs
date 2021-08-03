using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SourceWorkspaceNameValidatorTests
	{
		private ConfigurationStub _configuration;
		private SourceWorkspaceNameValidator _sut;
		private Mock<IWorkspaceManager> _workspaceManagerMock;
		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_workspaceManagerMock = new Mock<IWorkspaceManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(sf => sf.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(serviceFactory.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<SourceWorkspaceNameValidator>();
			IContainer container = containerBuilder.Build();

			_configuration = new ConfigurationStub();

			_sut = container.Resolve<SourceWorkspaceNameValidator>();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidDestinationWorkspaceName()
		{
			// Arrange
			string validWorkspaceName = "So much valid";

			_workspaceManagerMock.Setup(x => x.ReadAsync(_WORKSPACE_ARTIFACT_ID))
				.ReturnsAsync(new WorkspaceResponse {Name = validWorkspaceName});

			_configuration.SourceWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID;

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidDestinationWorkspaceName()
		{
			// Arrange
			string invalidWorkspaceName = "So ; much ; invalid";

			_workspaceManagerMock.Setup(x => x.ReadAsync(_WORKSPACE_ARTIFACT_ID))
				.ReturnsAsync(new WorkspaceResponse {Name = invalidWorkspaceName});

			_configuration.SourceWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID;

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}
	}
}
