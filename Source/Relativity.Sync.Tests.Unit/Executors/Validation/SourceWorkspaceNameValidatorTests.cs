using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class SourceWorkspaceNameValidatorTests
	{
		private SourceWorkspaceNameValidator _sut;

		private Mock<IWorkspaceNameValidator> _workspaceNameValidatorMock;
		private Mock<IValidationConfiguration> _configurationMock;

		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_workspaceNameValidatorMock = new Mock<IWorkspaceNameValidator>();

			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new SourceWorkspaceNameValidator(_workspaceNameValidatorMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldHandleValidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = true;
			_workspaceNameValidatorMock.Setup(v => v.ValidateWorkspaceNameAsync(_WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ItShouldHandleInvalidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = false;
			_workspaceNameValidatorMock.Setup(v => v.ValidateWorkspaceNameAsync(_WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}
	}
}