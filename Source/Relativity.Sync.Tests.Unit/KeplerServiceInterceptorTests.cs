using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class KeplerServiceInterceptorTests
	{
		private IStubForInterception _instance;

		private Mock<IStubForInterception> _stubForInterception;
		private Mock<ISyncMetrics> _syncMetrics;

		private readonly TimeSpan _executionTime = TimeSpan.FromMinutes(1);

		[SetUp]
		public void SetUp()
		{
			_stubForInterception = new Mock<IStubForInterception>();
			_syncMetrics = new Mock<ISyncMetrics>();

			Mock<IStopwatch> stopwatch = new Mock<IStopwatch>();
			stopwatch.Setup(x => x.Elapsed).Returns(_executionTime);

			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(_syncMetrics.Object, stopwatch.Object, new EmptyLogger());
			_instance = dynamicProxyFactory.WrapKeplerService(_stubForInterception.Object);
		}

		[Test]
		public async Task ItShouldReportSuccessfulExecution()
		{
			// ACT
			await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_stubForInterception.Verify(x => x.ExecuteAsync(), Times.Once);
			_syncMetrics.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Completed,
					It.Is<Dictionary<string, object>>(dict => !dict.ContainsKey("KeplerException"))), Times.Once);
		}

		[Test]
		public async Task ItShouldReturnValueFromExecution()
		{
			const int expectedValue = 123;

			_stubForInterception.Setup(x => x.ExecuteAndReturnValueAsync()).ReturnsAsync(expectedValue);

			// ACT
			int actualResult = await _instance.ExecuteAndReturnValueAsync().ConfigureAwait(false);

			// ASSERT
			actualResult.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldReportFailedExecution()
		{
			_stubForInterception.Setup(x => x.ExecuteAsync()).Throws<Exception>();

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetrics.Verify(x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Failed, It.IsAny<Dictionary<string, object>>()),
				Times.Once);
		}

		[Test]
		public void ItShouldNotFailWhenMetricsFails()
		{
			_syncMetrics.Setup(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>())).Throws<Exception>();

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().NotThrow();
		}

		[Test]
		public void ItShouldNotReportDisposeMethod()
		{
			// ACT
			_instance.Dispose();

			// ASSERT
			_stubForInterception.Verify(x => x.Dispose(), Times.Once);
			_syncMetrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
		}

		[Test]
		public void ItShouldNotRetryDisposeMethod()
		{
			_stubForInterception.Setup(x => x.Dispose()).Throws<Exception>();

			// ACT
			Action action = () => _instance.Dispose();

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterception.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void ItShouldRetryFailedExecution(Type exceptionType)
		{
			const int numberOfTries = 3;

			_stubForInterception.Setup(x => x.ExecuteAsync()).Throws((Exception) Activator.CreateInstance(exceptionType));

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterception.Verify(x => x.ExecuteAsync(), Times.Exactly(numberOfTries));
		}

		private static IEnumerable<Type> ExceptionsToRetry()
		{
			yield return typeof(HttpRequestException);
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void ItShouldStopRetryingAfterSuccess(Type exceptionType)
		{
			const int executionAndRetry = 2;

			_stubForInterception.SetupSequence(x => x.ExecuteAsync()).Throws((Exception) Activator.CreateInstance(exceptionType)).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().NotThrow();
			_stubForInterception.Verify(x => x.ExecuteAsync(), Times.Exactly(executionAndRetry));
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void ItShouldAddExceptionToMetrics(Type exceptionType)
		{
			Exception exception = (Exception) Activator.CreateInstance(exceptionType);
			_stubForInterception.Setup(x => x.ExecuteAsync()).Throws(exception);

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetrics.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Failed,
					It.Is<Dictionary<string, object>>(dict => dict["KeplerException"] == exception)), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void ItShouldAddNumberOfRetriesToMetrics(Type exceptionType)
		{
			const int expectedRetries = 2;

			Exception exception = (Exception) Activator.CreateInstance(exceptionType);
			_stubForInterception.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			action.Should().NotThrow();
			_syncMetrics.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Completed,
					It.Is<Dictionary<string, object>>(dict => dict["KeplerRetries"].Equals(expectedRetries))), Times.Once);
		}

		private static string GetMetricName(string methodName)
		{
			return $"{typeof(IStubForInterception).FullName}.{methodName}";
		}
	}
}