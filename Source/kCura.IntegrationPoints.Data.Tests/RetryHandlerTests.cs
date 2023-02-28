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
    [TestFixture, Category("Unit")]
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
        public async Task ExecuteWithRetriesAsync_TaskWithResult_ShouldReturnValueWhenNumberOfRetriesGreaterOrEqualThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            string result = await sut.ExecuteWithRetriesAsync(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [TestCase(1, (ushort)0)]
        [TestCase(2, (ushort)1)]
        [TestCase(10, (ushort)3)]
        public void ExecuteWithRetriesAsync_TaskWithResult_ShouldThrowsExceptionWhenNumberOfRetriesIsLessThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task<string>> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetriesAsync_TaskWithResult_ShouldThrowsExactlyTheSameExceptionAsync()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 0;
            var expectedException = new Exception(Guid.NewGuid().ToString());

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, expectedException, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task<string>> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrowExactly<Exception>()
                .Where(ex => ex == expectedException);
        }

        [Test]
        public async Task ExecuteWithRetriesAsync_TaskWithResult_ShouldWorkWithNullLogger()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            var sut = new RetryHandler(null, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);

            // act
            string result = await sut.ExecuteWithRetriesAsync(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [Test]
        public async Task ExecuteWithRetriesAsync_TaskWithResult_ShouldReturnResultWhenOperationsEventuallySucceed()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            string result = await sut.ExecuteWithRetriesAsync(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [Test]
        public void ExecuteWithRetriesAsync_TaskWithResult_ShouldRethrowExceptionWhenOperationConstantlyFails()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 5;
            const int numberOfRetries = 3;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task ExecuteWithRetriesAsync_TaskWithResult_ItShouldLogCallerNameWhenRetrying()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 1;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            await sut.ExecuteWithRetriesAsync(operationToExecute).ConfigureAwait(false);

            // assert
            string expectedName = nameof(ExecuteWithRetriesAsync_TaskWithResult_ItShouldLogCallerNameWhenRetrying);
            _logger.Verify(
                z => z.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.Is<object[]>(parameters => parameters.Any(x => expectedName == x as string))
                )
            );
        }

        [TestCase(0, (ushort)0)]
        [TestCase(0, (ushort)1)]
        [TestCase(1, (ushort)1)]
        [TestCase(0, (ushort)3)]
        [TestCase(2, (ushort)3)]
        public void ExecuteWithRetriesAsync_Task_ShouldNotThrowWhenNumberOfRetriesGreaterOrEqualThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> retryAction = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [TestCase(1, (ushort)0)]
        [TestCase(2, (ushort)1)]
        [TestCase(10, (ushort)3)]
        public void ExecuteWithRetriesAsync_Task_ShouldThrowsExceptionWhenNumberOfRetriesIsLessThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(
                _EXPECTED_RESULT,
                numberOfFailsBeforeSuccess
            );
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetriesAsync_Task_ShouldThrowsExactlyTheSameExceptionAsync()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 0;
            var expectedException = new Exception(Guid.NewGuid().ToString());

            Func<Task> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, expectedException, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrowExactly<Exception>()
                .Where(ex => ex == expectedException);
        }

        [Test]
        public void ExecuteWithRetriesAsync_Task_ShouldWorkWithNullLogger()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            var sut = new RetryHandler(null, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);

            // act
            Func<Task> retryAction = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [Test]
        public void ExecuteWithRetriesAsync_Task_ShouldNotThrowWhenOperationsEventuallySucceed()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> retryAction = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [Test]
        public void ExecuteWithRetriesAsync_Task_ShouldRethrowExceptionWhenOperationConstantlyFails()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 5;
            const int numberOfRetries = 3;

            Func<Task> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> functionToExecute = () => sut.ExecuteWithRetriesAsync(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task ExecuteWithRetriesAsync_Task_ItShouldLogCallerNameWhenRetrying()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 1;

            Func<Task> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            await sut.ExecuteWithRetriesAsync(operationToExecute).ConfigureAwait(false);

            // assert
            string expectedName = nameof(ExecuteWithRetriesAsync_Task_ItShouldLogCallerNameWhenRetrying);
            _logger.Verify(
                z => z.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.Is<object[]>(parameters => parameters.Any(x => expectedName == x as string))
                )
            );
        }

        [TestCase(0, (ushort)0)]
        [TestCase(0, (ushort)1)]
        [TestCase(1, (ushort)1)]
        [TestCase(0, (ushort)3)]
        [TestCase(2, (ushort)3)]
        public async Task ExecuteWithRetries_TaskWithResult_ShouldReturnValueWhenNumberOfRetriesGreaterOrEqualThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            string result = await sut.ExecuteWithRetries(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [TestCase(1, (ushort)0)]
        [TestCase(2, (ushort)1)]
        [TestCase(10, (ushort)3)]
        public void ExecuteWithRetries_TaskWithResult_ShouldThrowsExceptionWhenNumberOfRetriesIsLessThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task<string>> functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetries_TaskWithResult_ShouldThrowsExactlyTheSameExceptionAsync()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 0;
            var expectedException = new Exception(Guid.NewGuid().ToString());

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, expectedException, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task<string>> functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrowExactly<Exception>()
                .Where(ex => ex == expectedException);
        }

        [Test]
        public async Task ExecuteWithRetries_TaskWithResult_ShouldWorkWithNullLogger()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            var sut = new RetryHandler(null, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);

            // act
            string result = await sut.ExecuteWithRetries(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [Test]
        public async Task ExecuteWithRetries_TaskWithResult_ShouldReturnResultWhenOperationsEventuallySucceed()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            string result = await sut.ExecuteWithRetries(operationToExecute).ConfigureAwait(false);

            // assert
            result.Should().Be(_EXPECTED_RESULT);
        }

        [Test]
        public void ExecuteWithRetries_TaskWithResult_ShouldRethrowExceptionWhenOperationConstantlyFails()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 5;
            const int numberOfRetries = 3;

            Func<Task<string>> operationToExecute = CreateAsyncOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Func<Task> functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetries_TaskWithResult_ItShouldLogCallerNameWhenRetrying()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 1;

            Func<string> operationToExecute = CreateOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            sut.ExecuteWithRetries(operationToExecute);

            // assert
            string expectedName = nameof(ExecuteWithRetries_TaskWithResult_ItShouldLogCallerNameWhenRetrying);
            _logger.Verify(
                z => z.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.Is<object[]>(parameters => parameters.Any(x => expectedName == x as string))
                )
            );
        }

        [TestCase(0, (ushort)0)]
        [TestCase(0, (ushort)1)]
        [TestCase(1, (ushort)1)]
        [TestCase(0, (ushort)3)]
        [TestCase(2, (ushort)3)]
        public void ExecuteWithRetries_Task_ShouldNotThrowWhenNumberOfRetriesGreaterOrEqualThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<string> operationToExecute = CreateOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Action retryAction = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [TestCase(1, (ushort)0)]
        [TestCase(2, (ushort)1)]
        [TestCase(10, (ushort)3)]
        public void ExecuteWithRetries_Task_ShouldThrowsExceptionWhenNumberOfRetriesIsLessThanNumberOfFailuresAsync(
            int numberOfFailsBeforeSuccess, ushort numberOfRetries)
        {
            // arrange
            Func<string> operationToExecute = CreateOperationMock<string, InvalidOperationException>(
                _EXPECTED_RESULT,
                numberOfFailsBeforeSuccess
            );
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Action functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetries_Task_ShouldThrowsExactlyTheSameExceptionAsync()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 0;
            var expectedException = new Exception(Guid.NewGuid().ToString());

            Func<string> operationToExecute = CreateOperationMock(_EXPECTED_RESULT, expectedException, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Action functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrowExactly<Exception>()
                .Where(ex => ex == expectedException);
        }

        [Test]
        public void ExecuteWithRetries_Task_ShouldWorkWithNullLogger()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<string> operationToExecute = CreateOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            var sut = new RetryHandler(null, numberOfRetries, _WAIT_TIME_BETWEEN_RETRIES);

            // act
            Action retryAction = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [Test]
        public void ExecuteWithRetries_Task_ShouldNotThrowWhenOperationsEventuallySucceed()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 5;

            Func<string> operationToExecute = CreateOperationMock(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Action retryAction = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            retryAction.ShouldNotThrow();
        }

        [Test]
        public void ExecuteWithRetries_Task_ShouldRethrowExceptionWhenOperationConstantlyFails()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 5;
            const int numberOfRetries = 3;

            Func<string> operationToExecute = CreateOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            Action functionToExecute = () => sut.ExecuteWithRetries(operationToExecute);

            // assert
            functionToExecute.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ExecuteWithRetries_Task_ItShouldLogCallerNameWhenRetrying()
        {
            // arrange
            const int numberOfFailsBeforeSuccess = 1;
            const int numberOfRetries = 1;

            Func<string> operationToExecute = CreateOperationMock<string, InvalidOperationException>(_EXPECTED_RESULT, numberOfFailsBeforeSuccess);
            RetryHandler sut = CreateRetryHandler(numberOfRetries);

            // act
            sut.ExecuteWithRetries(operationToExecute);

            // assert
            string expectedName = nameof(ExecuteWithRetries_Task_ItShouldLogCallerNameWhenRetrying);
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

        private Func<TResult> CreateOperationMock<TResult>(TResult successResult, int numberOfFailsBeforeSuccess = 0)
            where TResult : class
        {
            return CreateOperationMock<TResult, Exception>(successResult, numberOfFailsBeforeSuccess);
        }

        private Func<Task<TResult>> CreateAsyncOperationMock<TResult, TException>(TResult successResult, TException exception, int numberOfFailsBeforeSuccess = 0)
            where TResult : class
            where TException : Exception
        {
            return CreateOperationMock(Task.FromResult(successResult), exception, numberOfFailsBeforeSuccess);
        }

        private Func<TResult> CreateOperationMock<TResult, TException>(TResult successResult, int numberOfFailsBeforeSuccess = 0)
            where TResult : class
            where TException : Exception, new()
        {
            var exceptionToThrow = new TException();
            return CreateOperationMock(successResult, exceptionToThrow, numberOfFailsBeforeSuccess);
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
