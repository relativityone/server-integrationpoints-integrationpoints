﻿using System.Threading;
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
	internal sealed class ValidationExecutorTests
	{
		private Mock<IValidator> _validator;
		private ValidationExecutor _instance;

		[SetUp]
		public void SetUp()
		{
			_validator = new Mock<IValidator>();
			_instance = new ValidationExecutor(new []{_validator.Object}, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldReportSuccessfullExecutionResult()
		{
			// act
			ExecutionResult result = await _instance.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ItShouldReportFailedExecutionResult()
		{
			_validator.Setup(x => x.ValidateAsync(It.IsAny<IValidationConfiguration>(), CancellationToken.None)).ReturnsAsync(new ValidationResult() {IsValid = false});

			// act
			ExecutionResult result = await _instance.ExecuteAsync(Mock.Of<IValidationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Failed);
		}
	}
}