using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	internal sealed class ValidatorWithMetricsTest
	{
		private Mock<IValidator> _internalValidator;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IStopwatch> _stopwatch;

		private ValidatorWithMetrics _sut;

		[SetUp]
		public void SetUp()
		{
			_internalValidator = new Mock<IValidator>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_stopwatch = new Mock<IStopwatch>();

			_sut = new ValidatorWithMetrics(_internalValidator.Object, _syncMetrics.Object, _stopwatch.Object);
		}

		[Test]
		public async Task ValidateAsync_ShouldNotReportCountOperation_WhenValidatorSucceeds()
		{
			// Arrange
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());

			// Act
			await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifySentMetric(m => m.FailedCounter == null);
		}

		[Test]
		public async Task ValidateAsync_ShouldReportCountOperation_WhenValidatorFails()
		{
			// Arrange
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult() {IsValid = false});

			// Act
			await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifySentMetric(m => m.FailedCounter == Counter.Increment);
		}

		[Test]
		public async Task ValidateAsync_ShouldReportTimedOperation_WhenValidatorFails()
		{
			// Arrange
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult() { IsValid = false });

			// Act
			await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifySentMetric(m => m.Duration != null);
		}

		[Test]
		public async Task ValidateAsync_ShouldReportTimedOperation_WhenValidatorSucceeds()
		{
			// Arrange
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());

			// Act
			await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifySentMetric(m => m.Duration != null);
		}

		[Test]
		public void ValidateAsync_ShouldReturnFailedResult_WhenValidatorThrows()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).Throws<InvalidOperationException>();

			// Act
			Func<Task> action = async () => await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public async Task ValidateAsync_ShouldMeasureExecutionTimeProperly()
		{
			// Arrange
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());
			TimeSpan expected = TimeSpan.FromSeconds(1);
			_stopwatch.SetupGet(x => x.Elapsed).Returns(expected);

			// Act
			await _sut.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifySentMetric(m => m.Duration == expected.TotalMilliseconds);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldValidate_ShouldPassInnerValidatorValue(bool innerValidatorShouldValidate)
		{
			// Arrange
			_internalValidator.Setup(x => x.ShouldValidate(It.IsAny<ISyncPipeline>()))
				.Returns(innerValidatorShouldValidate);

			// Act && Assert
			_sut.ShouldValidate(new Mock<ISyncPipeline>().Object).Should()
				.Be(innerValidatorShouldValidate);
		}

		private void VerifySentMetric(Expression<Func<ValidationMetric, bool>> validationFunc)
		{
			_syncMetrics.Verify(x => x.Send(It.Is(validationFunc)));
		}
	}
}