using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Exceptions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
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
		private Mock<IAPILog> _syncLogMock;
		private Mock<IRandom> _randomFake;

		private const int _MAX_NUMBER_OF_HTTP_RETRIES = 4;

		private readonly TimeSpan _executionTime = TimeSpan.FromMinutes(1);

		[SetUp]
		public void SetUp()
		{
			_stubForInterceptionMock = new Mock<IStubForInterception>();
			_stubForInterceptionFactoryFake = new Mock<Func<Task<IStubForInterception>>>();
			_stubForInterceptionFactoryFake.Setup(x => x()).Returns(Task.FromResult(_stubForInterceptionMock.Object));

			_syncLogMock = new Mock<IAPILog>();
			_randomFake = new Mock<IRandom>();

			Mock<IStopwatch> stopwatchFake = new Mock<IStopwatch>();
			stopwatchFake.Setup(x => x.Elapsed).Returns(_executionTime);
			Func<IStopwatch> stopwatchFactory = new Func<IStopwatch>(() => stopwatchFake.Object);
			
			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(stopwatchFactory, _randomFake.Object, _syncLogMock.Object);
			_sut = dynamicProxyFactory.WrapKeplerService(_stubForInterceptionMock.Object, _stubForInterceptionFactoryFake.Object);
			const int delayBaseMs = 0;
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
		}

		[Test]
		public async Task Execute_ShouldReportSuccessMetric_WhenExecutionSucceeds()
		{
			// ACT
			await _sut.ExecuteAsync().ConfigureAwait(false);

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.ExecuteAsync(), Times.Once);
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
		}

        [Test]
		public void Execute_ShouldNotReportMetrics_WhenDisposeIsCalled()
		{
			// ACT
			_sut.Dispose();

			// ASSERT
			_stubForInterceptionMock.Verify(x => x.Dispose(), Times.Once);
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
			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactory(stopwatchFactory, _randomFake.Object, new EmptyLogger());
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
			global::System.Reflection.FieldInfo field = typeof(AbstractInvocation).GetField("currentInterceptorIndex", BindingFlags.NonPublic | BindingFlags.Instance);

			// assert
			field.Should().NotBeNull();
		}

		private static void SetMillisecondsDelayBetweenHttpRetriesBase(IStubForInterception stub, int delayBaseMs)
		{
			global::System.Reflection.FieldInfo interceptorsField = stub.GetType().GetAllFields().Single(x => x.Name == "__interceptors");
			IInterceptor[] interceptors = (IInterceptor[])interceptorsField.GetValue(stub);
			IInterceptor interceptor = interceptors.Single();
			const string fieldName = "_secondsBetweenHttpRetriesBase";
			global::System.Reflection.FieldInfo millisecondsBetweenHttpRetriesBaseField = interceptor.GetType().GetAllFields().SingleOrDefault(x => x.Name == fieldName);
			if (millisecondsBetweenHttpRetriesBaseField == null)
			{
				Assert.Fail($"Cannot find field '{fieldName}' in type '{interceptor.GetType()}'");
			}
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
			yield return new ServiceException("Create Failed");
			yield return new ServiceException("An unexpected server error occurred: Bad Gateway - An invalid response was received from the upstream server");
			yield return new TimeoutException();
			yield return new Exception("Socket exception", new SocketException());
		}
	}
}
