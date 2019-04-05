using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	internal sealed class ValidatorWithMetricsTest
	{
		private Mock<IValidator> _internalValidator;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IStopwatch> _stopwatch;

		private ValidatorWithMetrics _validatorWithMetrics;

		[SetUp]
		public void SetUp()
		{
			_internalValidator = new Mock<IValidator>();
			_syncMetrics = new Mock<ISyncMetrics>();
			_stopwatch = new Mock<IStopwatch>();

			_validatorWithMetrics = new ValidatorWithMetrics(_internalValidator.Object, _syncMetrics.Object, _stopwatch.Object);
		}

		[Test]
		public async Task ItShouldNotReportCountOperationWhenValidatorSucceeds()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());

			// act
			await _validatorWithMetrics.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_syncMetrics.Verify(x => x.CountOperation(It.IsAny<string>(), It.IsAny<ExecutionStatus>()), Times.Never);
		}

		[Test]
		public async Task ItShouldReportCountOperationWhenValidatorFails()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult() {IsValid = false});

			// act
			await _validatorWithMetrics.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_syncMetrics.Verify(x => x.CountOperation(It.IsAny<string>(), ExecutionStatus.Failed), Times.Once);
		}

		[Test]
		public async Task ItShouldReportTimedOperationWhenValidatorFails()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult() { IsValid = false });

			// act
			await _validatorWithMetrics.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_syncMetrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), ExecutionStatus.Failed), Times.Once);
		}

		[Test]
		public async Task ItShouldReportTimedOperationWhenValidatorSucceeds()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult());

			// act
			await _validatorWithMetrics.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			_syncMetrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), ExecutionStatus.Completed), Times.Once);
		}

		[Test]
		public void ItShouldReturnFailedResultWhenValidatorThrows()
		{
			_internalValidator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _validatorWithMetrics.ValidateAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().NotThrow<InvalidOperationException>();
		}
	}
}