using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class DestinationWorkspaceNameValidatorTests
	{
		private DestinationWorkspaceNameValidator _sut;

		private Mock<IDestinationServiceFactoryForUser> _serviceFactory;
		private Mock<IWorkspaceNameValidator> _workspaceNameValidatorMock;
		private Mock<IValidationConfiguration> _configurationMock;

		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_workspaceNameValidatorMock = new Mock<IWorkspaceNameValidator>();

			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.DestinationWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new DestinationWorkspaceNameValidator(_serviceFactory.Object, _workspaceNameValidatorMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldHandleValidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = true;
			_workspaceNameValidatorMock.Setup(v => v.ValidateWorkspaceNameAsync(_serviceFactory.Object, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ItShouldHandleInvalidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = false;
			_workspaceNameValidatorMock.Setup(v => v.ValidateWorkspaceNameAsync(_serviceFactory.Object, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}
	}
}