using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Exceptions;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class KeplerServiceInterceptorTests
	{
		private IStubForInterception _sut;

		private Mock<IStubForInterception> _stubForInterceptionMock;
		private Mock<Func<Task<IStubForInterception>>> _stubForInterceptionFactoryFake;
		private Mock<ISyncMetrics> _syncMetricsMock;
		private Mock<ISyncLog> _syncLogMock;
		private Mock<IRandom> _randomFake;

		private const int _MAX_NUMBER_OF_HTTP_RETRIES = 4;

		private readonly string _durationMetricName = $"Relativity.Sync.KeplerServiceInterceptor.{nameof(IStubForInterception)}.Duration";
		private readonly string _successMetricName = $"Relativity.Sync.KeplerServiceInterceptor.{nameof(IStubForInterception)}.Success";
		private readonly string _failedMetricName = $"Relativity.Sync.KeplerServiceInterceptor.{nameof(IStubForInterception)}.Failed";
		private readonly string _authRefreshMetricName = $"Relativity.Sync.KeplerServiceInterceptor.{nameof(IStubForInterception)}.AuthRefresh";

		private readonly TimeSpan _executionTime = TimeSpan.FromMinutes(1);

		[SetUp]
		public void SetUp()
		{
			_stubForInterceptionMock = new Mock<IStubForInterception>();
			_stubForInterceptionFactoryFake = new Mock<Func<Task<IStubForInterception>>>();
			_stubForInterceptionFactoryFake.Setup(x => x.Invoke()).Returns(Task.FromResult(_stubForInterceptionMock.Object));
			
			_syncMetricsMock = new Mock<ISyncMetrics>();
			_syncLogMock = new Mock<ISyncLog>();
			_randomFake = new Mock<IRandom>();

			Mock<IStopwatch> stopwatchFake = new Mock<IStopwatch>();
			stopwatchFake.Setup(x => x.Elapsed).Returns(_executionTime);
			Func<IStopwatch> stopwatchFactory = new Func<IStopwatch>(() => stopwatchFake.Object);

			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(_syncMetricsMock.Object, stopwatchFactory, _randomFake.Object, _syncLogMock.Object);
			_sut = dynamicProxyFactory.WrapKeplerService(_stubForInterceptionMock.Object, _stubForInterceptionFactoryFake.Object);
			const int delayBaseMs = 1;
			SetMillisecondsDelayBetweenHttpRetriesBase(_sut, delayBaseMs);
		}

		[Test]
		public async Task Execute_ShouldReturnValue_WhenExecutionSucceeds()
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
		public async Task Execute_ShouldReportDurationMetricWithCompletedExecutionStatus_WhenExecutionSucceeds()
		{
			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Once);
			_syncMetricsMock.Verify(x => x.TimedOperation(_durationMetricName, _executionTime, ExecutionStatus.Completed), Times.Once);
		}

		[Test]
		public void Execute_ShouldReportDurationMetricWithFailedExecutionStatus_WhenExecutionFails()
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetricsMock.Verify(x => x.TimedOperation(_durationMetricName, _executionTime, ExecutionStatus.Failed), Times.Once);
		}

		[Test]
		public async Task Execute_ShouldReportSuccessMetric_WhenExecutionSucceeds()
		{
			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Once);
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(_successMetricName, 0), Times.Once);
		}

		[Test]
		public void Execute_ShouldReportFailedMetric_WhenExecutionFails()
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(_failedMetricName, 0), Times.Once);
		}

		[Test]
		public void Execute_ShouldNotFail_WhenMetricsFails()
		{
			// ARRANGE
			_syncMetricsMock.Setup(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>())).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
		}

		[Test]
		public void Execute_ShouldNotReportMetrics_WhenDisposeIsCalled()
		{
			// ACT
			_sut.Dispose();

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.Dispose(), Times.Once);
			_syncMetricsMock.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
		}

		[Test]
		public void Execute_ShouldNotRetry_WhenDisposeIsCalled()
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
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldRetryFailedExecution_WhenConnectionExceptionIsThrown(Exception exception)
		{
			// ARRANGE
			const int expectedInvocations = 5;

			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws(exception);

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
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldStopRetrying_WhenExecutionSucceeds(Exception exception)
		{
			// ARRANGE
			const int executionAndRetry = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(executionAndRetry));
		}

		[Test]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldThrowSyncMaxKeplerRetriesException_WhenExecutionFailsAfterRetriesDueToConnectionException(Exception exception)
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws(exception);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<SyncMaxKeplerRetriesException>();
		}

		[Test]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldReportFailedMetricWithNumberOfRetries_WhenExecutionFailsAfterRetriesDueToConnectionException(Exception exception)
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws(exception);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<SyncMaxKeplerRetriesException>();
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(_failedMetricName, _MAX_NUMBER_OF_HTTP_RETRIES), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		public async Task Execute_ShouldReportAuthRefreshMetricWithNumberOfRefreshes_WhenAuthRefreshed(Exception exception)
		{
			// ARRANGE
			const int expectedRefreshes = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(_authRefreshMetricName, expectedRefreshes), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public async Task Execute_ShouldReportSuccessMetricWithNumberOfRetries_WhenRetried(Exception exception)
		{
			// ARRANGE
			const int expectedRetries = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncMetricsMock.Verify(x => x.LogPointInTimeLong(_successMetricName, expectedRetries), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public async Task Execute_ShouldLogWarning_WhenRetried(Exception exception)
		{
			// ARRANGE
			const int expectedRetries = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(m => m.LogWarning(exception, It.IsNotNull<string>(), It.IsAny<object[]>()), Times.Exactly(expectedRetries));
		}

		[Test]
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public async Task Execute_ShouldPutKeplerInWarning_WhenRetried(Exception exception)
		{
			// ARRANGE
			const int expectedRetries = 2;

			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(m => m.LogWarning(exception, 
				It.Is<string>(messageTemplate => messageTemplate.Contains("{IKepler}")), 
				It.Is<object[]>(propertyValues => propertyValues.Contains(nameof(IStubForInterception))))
				, Times.Exactly(expectedRetries));
		}

		[Test]
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public async Task Execute_ShouldLogInformation_WhenExecutionSucceedsAfterRetries(Exception exception)
		{
			// ARRANGE
			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(m => m.LogInformation(It.IsNotNull<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(AuthTokenExceptionToRetry))]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public async Task Execute_ShouldPutKeplerInInformation_WhenExecutionSucceedsAfterRetries(Exception exception)
		{
			// ARRANGE
			_stubForInterceptionMock.SetupSequence(x => x.ExecuteAsync()).Throws(exception).Throws(exception).Returns(Task.CompletedTask);

			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(m => m.LogInformation(
				It.Is<string>(messageTemplate => messageTemplate.Contains("{IKepler}")),
				It.Is<object[]>(propertyValues => propertyValues.Contains(nameof(IStubForInterception))))
				, Times.Once);
		}

		[Test]
		public async Task Execute_ShouldNotLogInformation_WhenExecutionSucceedsWithoutRetries()
		{
			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(m => m.LogInformation(It.IsNotNull<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void Execute_ShouldNotLogInformation_WhenExecutionFails()
		{
			// ARRANGE
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_syncLogMock.Verify(m => m.LogInformation(It.IsNotNull<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void Execute_ShouldChangeInvocationTarget_WhenNotAuthorizedExceptionIsThrown()
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
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldRetryConnectionErrorsAndFail_WhenRetried4Times(Exception exception)
		{
			// ARRANGE
			const int expectedInvocations = 5; // 1 invocation + 4 retries
			_stubForInterceptionMock.Setup(x => x.ExecuteAsync()).Throws(exception);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().Throw<Exception>();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		[TestCaseSource(nameof(ConnectionExceptionsToRetry))]
		public void Execute_ShouldRetryConnectionErrorsAndSucceed_WhenRetried1Time(Exception exception)
		{
			// ARRANGE
			const int expectedInvocations = 2; // 1 invocation + 1 retries
			MockSequence seq = new MockSequence();
			_stubForInterceptionMock.InSequence(seq).Setup(x => x.ExecuteAsync()).Throws(exception);
			_stubForInterceptionMock.InSequence(seq).Setup(x => x.ExecuteAsync()).Returns(Task.CompletedTask);

			// ACT
			Func<Task> action = () => _sut.ExecuteAsync();

			// ASSERT
			action.Should().NotThrow();
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Exactly(expectedInvocations));
		}

		[Test]
		public void InvocationObject_ShouldHaveRequiredField()
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

		private static IEnumerable<Exception> AuthTokenExceptionToRetry()
		{
			yield return new NotAuthorizedException();
		}

		private static IEnumerable<Exception> ConnectionExceptionsToRetry()
		{
			yield return new ServiceNotFoundException();
			yield return new TemporarilyUnavailableException();
			yield return new ServiceException("Failed to determine route");
			yield return new TimeoutException();
		}

		private static string GetMetricName(string methodName)
		{
			return $"{typeof(IStubForInterception).FullName}.{methodName}";
		}
	}
}