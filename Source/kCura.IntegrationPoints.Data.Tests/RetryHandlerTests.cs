using FluentAssertions;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Tests
{
	[TestFixture]
	public class RetryHandlerTests
	{
		private Mock<IAPILog> _logger;

		private const int _WAIT_TIME_BETWEEN_RETRIES = 0;
		private const string _EXPECTED_RESULT = "Result";

		[SetUp]
		public void SetUp()
		{
			var loggerMock = new Mock<IAPILog>();
			loggerMock
				.Setup(x => x.ForContext<RetryHandler>())
				.Returns(loggerMock.Object);
			_logger = loggerMock;
		}

		[TestCase(0, (ushort)0)]
		[TestCase(0, (ushort)1)]
		[TestCase(1, (ushort)1)]
		[TestCase(0, (ushort)3)]
		[TestCase(2, (ushort)3)]
		public async Task ShouldReturnValueWhenNumberOfRetriesGreaterOrEqualThanNumberOfFailuresAsync(
			int numberOfFailsBeforeSuccess, ushort numberOfRetries)
		{
			// arrange
			Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			string result = await sut.ExecuteWithRetriesAsync(operationToExecute);

			// assert
			result.Should().Be(_EXPECTED_RESULT);
		}

		[TestCase(1, (ushort)0)]
		[TestCase(2, (ushort)1)]
		[TestCase(10, (ushort)3)]
		public void ShouldThrowsExceptionWhenNumberOfRetriesIsLessThanNumberOfFailuresAsync(
			int numberOfFailsBeforeSuccess, ushort numberOfRetries)
		{
			// arrange
			Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			Func<Task<string>> functionToExecute = async () => await sut.ExecuteWithRetriesAsync(operationToExecute);

			// assert
			functionToExecute.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void ShouldThrowsExactlyTheSameExceptionAsync()
		{
			// arrange
			const int numberOfFailsBeforeSuccess = 1;
			const int numberOfRetries = 0;
			var expectedException = new Exception(Guid.NewGuid().ToString());

			Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, expectedException, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			Func<Task<string>> functionToExecute = async () => await sut.ExecuteWithRetriesAsync(operationToExecute);

			// assert
			functionToExecute.ShouldThrowExactly<Exception>()
				.Where(ex => ex == expectedException);
		}

		[Test]
		public async Task ShouldWorkWithNullLogger()
		{
			// arrange
			const int numberOfFailsBeforeSuccess = 1;
			const int numberOfRetries = 5;

			Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			var sut = new RetryHandler(null, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);

			// act
			string result = await sut.ExecuteWithRetriesAsync(operationToExecute);

			// assert
			result.Should().Be(_EXPECTED_RESULT);
		}

		[Test]
		public void ShouldReturnResultWhenOperationsEventuallySucceed()
		{
			// arrange
			const int numberOfFailsBeforeSuccess = 1;
			const int numberOfRetries = 5;

			Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			string result = sut.ExecuteWithRetries(operationToExecute);

			// assert
			result.Should().Be(_EXPECTED_RESULT);
		}

		[Test]
		public void ShouldRethrowExceptionWhenOperationConstantlyFails()
		{
			// arrange
			const int numberOfFailsBeforeSuccess = 5;
			const int numberOfRetries = 3;

			Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			Action functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

			// assert
			functionToExecute.ShouldThrow<InvalidOperationException>();
		}

		[Test]
		public void ItShouldLogCallerNameWhenRetrying()
		{
			// arrange
			const int numberOfFailsBeforeSuccess = 1;
			const int numberOfRetries = 1;

			Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
			RetryHandler sut = CreateRetryHandler(numberOfRetries);

			// act
			sut.ExecuteWithRetries(operationToExecute);

			// assert
			string expectedName = nameof(ItShouldLogCallerNameWhenRetrying);
			_logger.Verify(
				z => z.LogWarning(
					It.IsAny<Exception>(),
					It.IsAny<string>(),
					It.Is<object[]>(parameters => parameters.Any(x => expectedName == x as string))
				)
			);
		}

		private RetryHandler CreateRetryHandler(ushort numberOfRetries)
		{
			return new RetryHandler(_logger.Object, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);
		}

		private Func<Task<TResult>> CreateAsyncOperationMock<TResult>(TResult successResult, int numberOfFailsBeforeSuccess = 0)
			where TResult : class
		{
			return CreateAsyncOperationMock<TResult, Exception>(successResult, numberOfFailsBeforeSuccess);
		}

		private Func<Task<TResult>> CreateAsyncOperationMock<TResult, TException>(TResult successResult, int numberOfFailsBeforeSuccess = 0)
			where TResult : class
			where TException : Exception, new()
		{
			var exceptionToThrow = new TException();
			return CreateAsyncOperationMock(successResult, exceptionToThrow, numberOfFailsBeforeSuccess);
		}

		private Func<Task<TResult>> CreateAsyncOperationMock<TResult, TException>(TResult successResult, TException exception, int numberOfFailsBeforeSuccess = 0)
			where TResult : class
			where TException : Exception
		{
			return CreateOperationMock(Task.FromResult(successResult), exception, numberOfFailsBeforeSuccess);
		}

		private Func<TResult> CreateOperationMock<TResult, TException>(TResult successResult, TException exception, int numberOfFailsBeforeSuccess = 0)
			where TResult : class
			where TException : Exception
		{
			var operationMock = new Mock<Func<TResult>>();
			ISetupSequentialResult<TResult> z = operationMock.SetupSequence(x => x());
			for (int i = 0; i < numberOfFailsBeforeSuccess; i++)
			{
				z.Throws(exception);
			}
			z.Returns(successResult);

			return operationMock.Object;
		}
	}
}
