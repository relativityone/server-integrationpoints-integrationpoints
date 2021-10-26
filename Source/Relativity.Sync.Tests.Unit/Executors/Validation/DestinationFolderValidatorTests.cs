using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Folder;
using Relativity.Services.Folder;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class DestinationFolderValidatorTests
	{
		private DestinationFolderValidator _sut;

		private FolderStatus _getAccessStatusAsyncResult;

		private Mock<IValidationConfiguration> _configurationMock;
		private Mock<IFolderManager> _folderManagerMock;

		private const int _FOLDER_ARTIFACT_ID = 456;
		private const int _WORKSPACE_ARTIFACT_ID = 123;

		[SetUp]
		public void SetUp()
		{
			_getAccessStatusAsyncResult = new FolderStatus();

			_folderManagerMock = new Mock<IFolderManager>();
			_folderManagerMock.Setup(fm => fm.GetAccessStatusAsync(_WORKSPACE_ARTIFACT_ID, _FOLDER_ARTIFACT_ID)).ReturnsAsync(_getAccessStatusAsyncResult);

			var serviceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
			serviceFactoryMock.Setup(sf => sf.CreateProxyAsync<IFolderManager>()).ReturnsAsync(_folderManagerMock.Object);

			_configurationMock = new Mock<IValidationConfiguration>();
			_configurationMock.Setup(c => c.DestinationFolderArtifactId).Returns(_FOLDER_ARTIFACT_ID);
			_configurationMock.Setup(c => c.DestinationWorkspaceArtifactId).Returns(_WORKSPACE_ARTIFACT_ID);

			_sut = new DestinationFolderValidator(serviceFactoryMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidConfiguration()
		{
			// Arrange
			_getAccessStatusAsyncResult.Exists = true;

			//Act
			ValidationResult result = await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidConfiguration()
		{
			// Arrange
			_getAccessStatusAsyncResult.Exists = false;

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
			_folderManagerMock.Setup(x => x.GetAccessStatusAsync(_WORKSPACE_ARTIFACT_ID, _FOLDER_ARTIFACT_ID)).Throws<InvalidOperationException>();

			// Act
			Func<Task<ValidationResult>> result = async () => await _sut.ValidateAsync(_configurationMock.Object, CancellationToken.None);

			// Assert
			result.Should().Throw<InvalidOperationException>();
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]		
		[TestCase(typeof(SyncImageRunPipeline), true)]
		[TestCase(typeof(SyncImageRetryPipeline), true)]
		[TestCase(typeof(SyncNonDocumentRunPipeline), false)]
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