using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	internal sealed class ValidationExecutorTests
	{
		private Mock<IValidator> _validatorMock;
		private ValidationExecutor _sut;
		private Mock<IPipelineSelector> _pipelineSelectorMock;

		[SetUp]
		public void SetUp()
		{
			_validatorMock = new Mock<IValidator>();
			_pipelineSelectorMock = new Mock<IPipelineSelector>();

			_validatorMock.Setup(x => x.ShouldValidate(It.IsAny<ISyncPipeline>())).Returns(true);
			_pipelineSelectorMock.Setup(x => x.GetPipeline()).Returns(new SyncDocumentRunPipeline());

			_sut = new ValidationExecutor(new[] {_validatorMock.Object}, _pipelineSelectorMock.Object, new EmptyLogger());
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportSuccessfullyExecutionResult()
		{
			// Act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReportFailedExecutionResult()
		{
			// Arrange
			_validatorMock.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult(new ValidationMessage("message")));

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Failed);
		}

		[Test]
		public void ExecuteAsync_ShouldThrowExceptionWrappingInnerException()
		{
			// Arrange
			_validatorMock.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).Throws<InvalidOperationException>();

			// Act
			Func<Task> action = () => _sut.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CompositeCancellationToken.None);

			// Assert
			action.Should().Throw<Exception>().Which.InnerException.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public async Task ExecuteAsync_ShouldRespect_IValidator_ShouldValidate()
		{
			// Arrange
			_validatorMock.Setup(x => x.ShouldValidate(It.IsAny<ISyncPipeline>())).Returns(false);

			// Act
			ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			_validatorMock.Verify(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);
		}
	}
}