using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class KeplerServiceInterceptorTests
	{
		private IStubForInterception _sut;

		private Mock<IStubForInterception> _stubForInterceptionMock;
		private Mock<Func<Task<IStubForInterception>>> _stubForInterceptionFactoryFake;
		private Mock<ISyncMetrics> _syncMetricsMock;
		private Mock<IRandom> _randomFake;

		private readonly TimeSpan _executionTime = TimeSpan.FromMinutes(1);

		private readonly IDictionary<Type, string> _exceptionTypeToMetricNameDictionary = new Dictionary<Type, string>()
		{
			{typeof(HttpRequestException), "KeplerRetries"},
			{typeof(NotAuthorizedException), "AuthTokenRetries"}
		};

		[SetUp]
		public void SetUp()
		{
			_stubForInterceptionMock = new Mock<IStubForInterception>();
			_stubForInterceptionFactoryFake = new Mock<Func<Task<IStubForInterception>>>();
			_stubForInterceptionFactoryFake.Setup(x => x.Invoke()).Returns(Task.FromResult(_stubForInterceptionMock.Object));
			
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_randomFake = new Mock<IRandom>();

			Mock<IStopwatch> stopwatchFake = new Mock<IStopwatch>();
			stopwatchFake.Setup(x => x.Elapsed).Returns(_executionTime);
			Func<IStopwatch> stopwatchFactory = new Func<IStopwatch>(() => stopwatchFake.Object);

			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(_syncMetricsMock.Object, stopwatchFactory, _randomFake.Object, new EmptyLogger());
			_sut = dynamicProxyFactory.WrapKeplerService(_stubForInterceptionMock.Object, _stubForInterceptionFactoryFake.Object);
			const int delayBaseMs = 1;
			SetMillisecondsDelayBetweenHttpRetriesBase(_sut, delayBaseMs);
		}

		[Test]
		public async Task Execute_ShouldReportSuccessfulExecution()
		{
			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Once);
			_syncMetricsMock.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Completed,
					It.Is<Dictionary<string, object>>(dict => !dict.ContainsKey("KeplerException"))), Times.Once);
		}

		[Test]
		public async Task Execute_ShouldReturnValueFromExecution()
		{
			// ARRANGE
			const int expectedValue = 123;

			_stubForInterceptionMock.Setup(x => x.ExecuteAndReturnValueAsync()).ReturnsAsync(expectedValue);

			// ACT
			int actualResult = await _sut.ExecuteAndReturnValueAsync().ConfigureAwait(false);

			// ASSERT
			actualResult.Should().Be(expectedValue);
		}

		[Test]
		public void Execute_ShouldReportFailedExecution()
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetricsMock.Verify(x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Failed, It.IsAny<Dictionary<string, object>>()),
				Times.Once);
		}

		[Test]
		public void Execute_ShouldNotFailWhenMetricsFails()
		{
			// ARRANGE
			_syncMetricsMock.Setup(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>())).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
		}

		[Test]
		public void Execute_ShouldNotReportDisposeMethod()
		{
			// ACT
			_sut.Dispose();

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.Dispose(), Times.Once);
			_syncMetricsMock.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
		}

		[Test]
		public void Execute_ShouldNotRetryDisposeMethod()
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.Dispose()).Throws<Exception>();

			// ACT
			Action action = () => _sut.Dispose();

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterceptionMock.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void Execute_ShouldRetryFailedExecution_WhenHttpExceptionIsThrown()
		{
			// ARRANGE
			const int expectedInvocations = 5;

			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<HttpRequestException>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		public void Execute_ShouldRetryFailedExecution_WhenAuthExceptionIsThrown()
		{
			// ARRANGE
			const int expectedInvocations = 4;

			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<NotAuthorizedException>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void Execute_ShouldStopRetryingAfterSuccess(Type exceptionType)
		{
			// ARRANGE
			const int executionAndRetry = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws((Exception) Activator.CreateInstance(exceptionType)).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(executionAndRetry));
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void Execute_ShouldAddExceptionToMetrics(Type exceptionType)
		{
			// ARRANGE
			Exception exception = (Exception) Activator.CreateInstance(exceptionType);
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws(exception);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetricsMock.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Failed,
					It.Is<Dictionary<string, object>>(dict => dict["KeplerException"] == exception)), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(ExceptionsToRetry))]
		public void Execute_ShouldAddNumberOfRetriesToMetrics(Type exceptionType)
		{
			// ARRANGE
			const int expectedRetries = 2;

			Exception exception = (Exception) Activator.CreateInstance(exceptionType);
			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			string metricName = _exceptionTypeToMetricNameDictionary[exceptionType];
			action.Should().NotThrow();
			_syncMetricsMock.Verify(
				x => x.TimedOperation(GetMetricName(nameof(IStubForInterception.ExecuteAsync)), _executionTime, ExecutionStatus.Completed,
					It.Is<Dictionary<string, object>>(dict => dict[metricName].Equals(expectedRetries))), Times.Once);
		}

		[Test]
		public void Execute_ShouldChangeInvocationTarget()
		{
			// ARRANGE
			Mock<IStubForInterception> badService = new Mock<IStubForInterception>();
			badService.Setup(x => x.ExecuteAsync()).Throws<NotAuthorizedException>();

			Mock<IStubForInterception> newService = new Mock<IStubForInterception>();
			Task<IStubForInterception> ServiceFactory() => Task.FromResult(newService.Object);

			Mock<IStopwatch> stopwatchFake = new Mock<IStopwatch>();
			stopwatchFake.Setup(x => x.Elapsed).Returns(_executionTime);
			Func<IStopwatch> stopwatchFactory = new Func<IStopwatch>(() => stopwatchFake.Object);
			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(_syncMetricsMock.Object, stopwatchFactory, _randomFake.Object, new EmptyLogger());
			IStubForInterception instance = dynamicProxyFactory.WrapKeplerService(badService.Object, ServiceFactory);

			// ACT
			Func<Task> action = () => instance.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
			badService.Verify(x => x.ExecuteAsync(), Times.Once());
			newService.Verify(x => x.ExecuteAsync(), Times.Once());
		}

		[Test]
		public void Execute_ShouldRetryHttpErrorsAndFail_WhenRetried4Times()
		{
			// ARRANGE
			const int expectedInvocations = 5; // 1 invocation + 4 retries
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<HttpRequestException>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<HttpRequestException>();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		public void Execute_ShouldRetryHttpErrorsAndSucceed_WhenRetried1Time()
		{
			// ARRANGE
			const int expectedInvocations = 2; // 1 invocation + 1 retries
			MockSequence seq = new MockSequence();
			_stubForInterceptionMock.InSequence(seq).Setup(x => x.ExecuteAsync()).Throws<HttpRequestException>();
			_stubForInterceptionMock.InSequence(seq).Setup(x => x.ExecuteAsync()).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow<HttpRequestException>();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		public void InvocationObjectHasRequiredField()
		{
			// act
			System.Reflection.FieldInfo field = typeof(AbstractInvocation).GetField("currentInterceptorIndex", BindingFlags.NonPublic | BindingFlags.Instance);

			// assert
			field.Should().NotBeNull();
		}

		private static void SetMillisecondsDelayBetweenHttpRetriesBase(IStubForInterception stub, int delayBaseMs)
		{
			System.Reflection.FieldInfo interceptorsField = stub.GetType().GetAllFields().Single(x => x.Name == "__interceptors");
			IInterceptor[] interceptors = (IInterceptor[])interceptorsField.GetValue(stub);
			IInterceptor interceptor = interceptors.Single();
			System.Reflection.FieldInfo millisecondsBetweenHttpRetriesBaseField = interceptor.GetType().GetAllFields().Single(x => x.Name == "_millisecondsBetweenHttpRetriesBase");
			millisecondsBetweenHttpRetriesBaseField.SetValue(interceptor, delayBaseMs);
		}

		private static IEnumerable<Type> ExceptionsToRetry()
		{
			yield return typeof(HttpRequestException);
			yield return typeof(NotAuthorizedException);
		}

		private static string GetMetricName(string methodName)
		{
			return $"{typeof(IStubForInterception).FullName}.{methodName}";
		}
	}
}