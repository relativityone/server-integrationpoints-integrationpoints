using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PreValidation;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.PreValidation
{
    [TestFixture]
    internal sealed class PreValidationExecutorTests
    {
        private Mock<IPreValidator> _validatorMock;
        private PreValidationExecutor _sut;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IPreValidator>();

            _sut = new PreValidationExecutor(new[] { _validatorMock.Object }, new EmptyLogger());
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportSuccessfullyExecutionResult()
        {
            // Act
            ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<IPreValidationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportFailedExecutionResult()
        {
            // Arrange
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<IPreValidationConfiguration>(), CancellationToken.None))
                .ReturnsAsync(ValidationResult.Invalid);

            // Act
            ExecutionResult result = await _sut.ExecuteAsync(Mock.Of<IPreValidationConfiguration>(), CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
        }

        [Test]
        public void ExecuteAsync_ShouldChangeAnyExceptionToValidationException()
        {
            // Arrange
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<IPreValidationConfiguration>(), CancellationToken.None))
                .Throws<InvalidOperationException>();

            // Act
            Func<Task> action = () => _sut.ExecuteAsync(Mock.Of<IPreValidationConfiguration>(), CompositeCancellationToken.None);

            // Assert
            action.Should().Throw<ValidationException>();
        }
    }
}
