using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class DestinationWorkspaceNameValidatorTests
	{
		private DestinationWorkspaceNameValidator _sut;

		private Mock<IWorkspaceNameValidator> _workspaceNameValidatorMock;
		private Mock<IValidationConfiguration> _configurationMock;
		private Mock<IWorkspaceNameQuery> _workspaceNameQuery;
		
		private const string _WORKSPACE_NAME = "The workspace";
		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			var serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_workspaceNameValidatorMock = new Mock<IWorkspaceNameValidator>();

			_workspaceNameQuery = new Mock<IWorkspaceNameQuery>();
			_workspaceNameQuery.Setup(wnq => wnq.GetWorkspaceNameAsync(serviceFactory.Object, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(_WORKSPACE_NAME);
			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.DestinationWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new DestinationWorkspaceNameValidator(serviceFactory.Object, _workspaceNameQuery.Object, _workspaceNameValidatorMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidDestinationFolderName()
		{
			// Arrange
			const bool workspaceNameValidationResult = true;
			_workspaceNameValidatorMock.Setup(v => v.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Returns(workspaceNameValidationResult);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
			_workspaceNameQuery.Verify();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidDestinationFolderName()
		{
			// Arrange
			const bool workspaceNameValidationResult = false;
			_workspaceNameValidatorMock.Setup(v => v.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Returns(workspaceNameValidationResult);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}

		[Test]
		public void ValidateAsync_ShouldThrowExceptionDuringValidation()
		{
			// Arrange
			_workspaceNameValidatorMock.Setup(x => x.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Throws<InvalidOperationException>();

			// Act
			Func<Task<ValidationResult>> result = async () => await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Throw<InvalidOperationException>();
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]
		[TestCase(typeof(SyncImageRunPipeline), true)]
		[TestCase(typeof(SyncImageRetryPipeline), true)]
		[EnsureAllPipelineTestCase(0)]
		public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
		{
			// Arrange
			ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

			// Act
			bool actualResult = _sut.ShouldValidate(pipelineObject);

			// Assert
			actualResult.Should().Be(expectedResult,
				$"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
		}
	}
}