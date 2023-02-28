using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Filters;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Filters
{
    [TestFixture, Category("Unit")]
    public class ExceptionFilterTests
    {
        private Mock<Func<ITextSanitizer>> _textSanitizerFactoryMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<Func<IAPILog>> _loggerFactoryMock;
        private const string _CONTACT_ADMIN_SUFFIX = " Please check Error tab for more details";
        private readonly LogApiExceptionFilterAttribute _attribute = new LogApiExceptionFilterAttribute
        {
            IsUserMessage = true,
            Message = "User message"
        };

        private readonly CancellationToken _cancellationToken = new CancellationToken();

        [SetUp]
        public void SetUp()
        {
            var textSanitizerMock = new Mock<ITextSanitizer>();
            textSanitizerMock
                .Setup(x => x.Sanitize(It.IsAny<string>()))
                .Returns<string>(input => new SanitizationResult(input, false));

            _loggerMock = new Mock<IAPILog>();
            _loggerMock
                .Setup(x => x.ForContext<ExceptionFilter>())
                .Returns(_loggerMock.Object);
            _textSanitizerFactoryMock = new Mock<Func<ITextSanitizer>>();
            _textSanitizerFactoryMock
                .Setup(x => x())
                .Returns(textSanitizerMock.Object);
            _loggerFactoryMock = new Mock<Func<IAPILog>>();
            _loggerFactoryMock
                .Setup(x => x())
                .Returns(_loggerMock.Object);
        }

        [Test]
        public void Constructor_ShouldThrowNullArgumentExceptionWhenAttributeIsNull()
        {
            // act
            Action constructor = () => new ExceptionFilter(
                attribute: null,
                textSanitizerFactory: _textSanitizerFactoryMock.Object,
                loggerFactory: _loggerFactoryMock.Object);

            // assert
            constructor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowNullArgumentExceptionWhenTextSanitizerFactoryIsNull()
        {
            // act
            Action constructor = () => new ExceptionFilter(
                _attribute,
                textSanitizerFactory: null,
                loggerFactory: _loggerFactoryMock.Object);

            // assert
            constructor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldThrowNullArgumentExceptionWhenLoggerFactoryIsNull()
        {
            // act
            Action constructor = () => new ExceptionFilter(
                _attribute,
                _textSanitizerFactoryMock.Object,
                loggerFactory: null);

            // assert
            constructor.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ShouldNotInstantiateSanitizerAndLogger()
        {
            // act
            new ExceptionFilter(
                _attribute,
                _textSanitizerFactoryMock.Object,
                _loggerFactoryMock.Object);

            // assert
            _textSanitizerFactoryMock.Verify(x => x(), Times.Never);
            _loggerFactoryMock.Verify(x => x(), Times.Never);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task ExecuteExceptionFilterAsync_ShouldCreateNewSanitizerAndLoggerInstancesForEachCall(int numberOfCalls)
        {
            // arrange
            var sut = new ExceptionFilter(
                _attribute,
                _textSanitizerFactoryMock.Object,
                _loggerFactoryMock.Object);

            var exception = new IntegrationPointsException();
            HttpActionExecutedContext actionExecutedContext = CreateDummyActionExecutedContext(exception);

            // act
            for (int i = 0; i < numberOfCalls; i++)
            {
                await sut.ExecuteExceptionFilterAsync(actionExecutedContext, _cancellationToken).ConfigureAwait(false);
            }

            // assert
            _loggerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
            _textSanitizerFactoryMock.Verify(x => x(), Times.Exactly(numberOfCalls));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ExecuteExceptionFilterAsync_ShouldUseIntegrationPointExceptionUserMessageWhenDefined(bool isUserMessage)
        {
            // arrange
            var attribute = new LogApiExceptionFilterAttribute
            {
                IsUserMessage = isUserMessage,
                Message = "This message won't be added to response"
            };
            var sut = new ExceptionFilter(
                attribute,
                _textSanitizerFactoryMock.Object,
                _loggerFactoryMock.Object);

            string errorMessage = "Error occured.";
            var exception = new IntegrationPointsException
            {
                UserMessage = errorMessage
            };
            HttpActionExecutedContext actionExecutedContext = CreateDummyActionExecutedContext(exception);

            // act
            await sut.ExecuteExceptionFilterAsync(actionExecutedContext, _cancellationToken).ConfigureAwait(false);

            // assert
            string expectedMessage = isUserMessage
                ? errorMessage + _CONTACT_ADMIN_SUFFIX
                : errorMessage;

            string content = await actionExecutedContext.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Should().Be(expectedMessage);
            _loggerMock.Verify(x => x.LogError(exception, expectedMessage));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ExecuteExceptionFilterAsync_ShouldUseMessageFromAttributeForNoIntegrationPointException(bool isUserMessage)
        {
            // arrange
            string errorMessage = "Error occured.";
            var attribute = new LogApiExceptionFilterAttribute
            {
                IsUserMessage = isUserMessage,
                Message = errorMessage
            };
            var sut = new ExceptionFilter(
                attribute,
                _textSanitizerFactoryMock.Object,
                _loggerFactoryMock.Object);

            var exception = new ArgumentNullException();
            HttpActionExecutedContext actionExecutedContext = CreateDummyActionExecutedContext(exception);

            // act
            await sut.ExecuteExceptionFilterAsync(actionExecutedContext, _cancellationToken).ConfigureAwait(false);

            // assert
            string expectedMessage = isUserMessage
                ? errorMessage + _CONTACT_ADMIN_SUFFIX
                : errorMessage;

            string content = await actionExecutedContext.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Should().Be(expectedMessage);
            _loggerMock.Verify(x => x.LogError(exception, expectedMessage));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ExecuteExceptionFilterAsync_ShouldReturnUnexpectedErrorWhenNoIntegrationPointsExceptionAndNoMessageInAttribute(bool isUserMessage)
        {
            // arrange
            var attribute = new LogApiExceptionFilterAttribute
            {
                IsUserMessage = isUserMessage,
                Message = ""
            };
            var sut = new ExceptionFilter(
                attribute,
                _textSanitizerFactoryMock.Object,
                _loggerFactoryMock.Object);

            var exception = new ArgumentNullException();
            HttpActionExecutedContext actionExecutedContext = CreateDummyActionExecutedContext(exception);

            // act
            await sut.ExecuteExceptionFilterAsync(actionExecutedContext, _cancellationToken).ConfigureAwait(false);

            // assert
            string errorMessage = "UnexpectedErrorOccurred";
            string expectedMessage = isUserMessage
                ? errorMessage + _CONTACT_ADMIN_SUFFIX
                : errorMessage;

            string content = await actionExecutedContext.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Should().Be(expectedMessage);
            _loggerMock.Verify(x => x.LogError(exception, expectedMessage));
        }

        private HttpActionExecutedContext CreateDummyActionExecutedContext(Exception exception)
        {
            var request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());

            var actionExecutedContext = new HttpActionExecutedContext
            {
                ActionContext = new HttpActionContext
                {
                    ControllerContext = new HttpControllerContext
                    {
                        Request = request
                    }
                },
                Response = new HttpResponseMessage(),
                Exception = exception
            };

            return actionExecutedContext;
        }
    }
}
