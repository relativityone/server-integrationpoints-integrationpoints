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
	public sealed class SourceWorkspaceNameValidatorTests
	{
		private SourceWorkspaceNameValidator _sut;

		private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUserMock;
		private Mock<IWorkspaceNameValidator> _workspaceNameValidatorMock;
		private Mock<IValidationConfiguration> _configurationMock;
		
		private const string _WORKSPACE_NAME = "The workspace";
		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForUserMock = new Mock<ISourceServiceFactoryForUser>();
			_workspaceNameValidatorMock = new Mock<IWorkspaceNameValidator>();

			var workspaceNameQuery = new Mock<IWorkspaceNameQuery>();
			workspaceNameQuery.Setup(wnq => wnq.GetWorkspaceNameAsync(_serviceFactoryForUserMock.Object, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).ReturnsAsync(_WORKSPACE_NAME);

			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new SourceWorkspaceNameValidator(_serviceFactoryForUserMock.Object, workspaceNameQuery.Object, _workspaceNameValidatorMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldHandleValidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = true;
			_workspaceNameValidatorMock.Setup(v => v.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Returns(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ItShouldHandleInvalidDestinationFolderName()
		{
			const bool workspaceNameValidationResult = false;
			_workspaceNameValidatorMock.Setup(v => v.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Returns(workspaceNameValidationResult);

			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}

		[Test]
		public void ItShouldThrowExceptionDuringValidation()
		{
			_workspaceNameValidatorMock.Setup(x => x.Validate(_WORKSPACE_NAME, _WORKSPACE_ARTIFACT_ID, CancellationToken.None)).Throws<InvalidOperationException>();

			Func<Task<ValidationResult>> func = async () => await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			func.Should().Throw<InvalidOperationException>();
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]
		[TestCase(typeof(SyncImageRunPipeline), true)]
		[TestCase(typeof(SyncImageRetryPipeline), true)]
		[TestCase(typeof(SyncNonDocumentRunPipeline), true)]
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