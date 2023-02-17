using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Common.Tests.Monitoring.Instrumentation
{
    [TestFixture, Category("Unit")]
    public class ExternalServiceInstrumentationTests
    {
        private ExternalServiceInstrumentation _sut;
        private IAPILog _logger;
        private IMessageService _messageService;
        private const int _WORKSPACE_ID = 531412;
        private readonly InstrumentationJobContext _jobContext = new InstrumentationJobContext
        (
            correlationId: Guid.NewGuid().ToString(),
            jobId: 4384343,
            workspaceId: _WORKSPACE_ID
        );
        private readonly InstrumentationServiceCallContext _callContext = new InstrumentationServiceCallContext
        (
            serviceType: Guid.NewGuid().ToString(),
            serviceName: Guid.NewGuid().ToString(),
            operationName: Guid.NewGuid().ToString()
        );

        [SetUp]
        public void SetUp()
        {
            _messageService = Substitute.For<IMessageService>();
            _logger = Substitute.For<IAPILog>();
            _logger.ForContext<ExternalServiceInstrumentation>().Returns(_logger);

            _sut = new ExternalServiceInstrumentation(_jobContext, _callContext, _messageService, _logger);
        }

        [Test]
        public void ItShouldNotSendMetricWhenJobWasStarted()
        {
            // act
            _sut.Started();

            // assert
            _messageService.DidNotReceiveWithAnyArgs().Send(Arg.Any<IMessage>());
        }

        [Test]
        public void ItShouldSendMetricWhenJobWasCompleted()
        {
            // arrange
            _sut.Started();

            // act
            _sut.Completed();

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateSuccessMessage(x)));
        }

        [Test]
        public void ItShouldSendMetricWhenJobFailedWithReasonProvided()
        {
            // arrange
            const string failReason = "Bad request";
            _sut.Started();

            // act
            _sut.Failed(failReason);

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateFailureMessage(x, failReason)));
        }

        [Test]
        public void ItShouldLogErrorWhenJobFailedWithReasonProvided()
        {
            // arrange
            const string failReason = "Bad request";
            _sut.Started();

            // act
            _sut.Failed(failReason);

            // assert
            _logger.Received().LogError(Arg.Any<string>(), _callContext, failReason);
        }

        [Test]
        public void ItShouldSendMetricWhenJobFailedWithExceptionProvided()
        {
            // arrange
            Exception ex = new InvalidOperationException();
            string expectedFailReason = nameof(InvalidOperationException);
            _sut.Started();

            // act
            _sut.Failed(ex);

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateFailureMessage(x, expectedFailReason)));
        }

        [Test]
        public void ItShouldLogErrorWhenJobFailedWithExceptionProvided()
        {
            // arrange
            Exception ex = new InvalidOperationException();
            _sut.Started();

            // act
            _sut.Failed(ex);

            // assert
            _logger.Received().LogError(ex, Arg.Any<string>(), _callContext);
        }

        [Test]
        public void ItShouldNotSendAnyMessageAfterCompletedIfItWasNotStarted()
        {
            // act
            _sut.Completed();

            // assert
            _messageService.DidNotReceiveWithAnyArgs().Send(Arg.Any<ExternalCallCompletedMessage>());
        }

        [Test]
        public void ItShouldNotSendAnyMetricsAfterFailedIfItWasNotStarted()
        {
            // act
            _sut.Failed("reason");

            // assert
            _messageService.DidNotReceiveWithAnyArgs().Send(Arg.Any<ExternalCallCompletedMessage>());
        }

        [Test]
        public void ItShouldLogErrorAfterFailedEvenIfItWasNotStarted()
        {
            // act
            const string failureReason = "reason";
            _sut.Failed(failureReason);

            // assert
            _logger.Received().LogError(Arg.Any<string>(), _callContext, failureReason);
        }

        [Test]
        public void ItShouldSendOneMessageIfWasCompletedTwice()
        {
            // arrange
            _sut.Started();

            // act
            _sut.Completed();
            _sut.Completed();

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateSuccessMessage(x)));
        }

        [Test]
        public void ItShouldSendOneMessageIfWasCompletedAndFailed()
        {
            // arrange
            _sut.Started();

            // act
            _sut.Completed();
            _sut.Failed("some reason");

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateSuccessMessage(x)));
            _messageService.ReceivedWithAnyArgs(1).Send(Arg.Any<IMessage>());
        }

        [Test]
        public void ItShouldSendOneMessageIfWasFailedAndCompleted()
        {
            // arrange
            const string failureReason = "404";
            _sut.Started();

            // act
            _sut.Failed(failureReason);
            _sut.Completed();

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateFailureMessage(x, failureReason)));
            _messageService.ReceivedWithAnyArgs(1).Send(Arg.Any<IMessage>());
        }

        [Test]
        public void ItShouldThrowInvalidOperationExceptionIfWasStartedTwiceWithoutCompletion()
        {
            // arrange
            _sut.Started();

            // act & assert
            Assert.Throws(typeof(InvalidOperationException), () => _sut.Started());
        }

        [Test]
        public void ItShouldBePossibleToStartAgainWhenFirstMeasurementWasCompleted()
        {
            // arrange
            _sut.Started();
            _sut.Completed();

            // act & assert
            _sut.Started();
        }

        [Test]
        public void ItShouldSendBothMessagesWhenStartedAndCompletedTwice()
        {
            // arrange
            const string failureReason = "404";

            // act
            _sut.Started();
            _sut.Completed();
            _sut.Started();
            _sut.Failed(failureReason);

            // assert
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateFailureMessage(x, failureReason)));
            _messageService.Received(1).Send(Arg.Is<ExternalCallCompletedMessage>(x => ValidateSuccessMessage(x)));
        }

        private bool ValidateSuccessMessage(ExternalCallCompletedMessage message)
        {
            bool isValid = ValidateMessage(message);
            isValid &= !message.HasFailed;
            isValid &= string.IsNullOrEmpty(message.FailureReason);
            return isValid;
        }

        private bool ValidateFailureMessage(ExternalCallCompletedMessage message, string reason)
        {
            bool isValid = ValidateMessage(message);
            isValid &= message.HasFailed;
            isValid &= message.FailureReason == reason;
            return isValid;
        }

        private bool ValidateMessage(ExternalCallCompletedMessage message)
        {
            bool isValid = true;
            isValid &= message.CorrelationID == _jobContext.CorrelationId;
            isValid &= message.JobID == _jobContext.JobId.ToString();
            isValid &= message.WorkspaceID == _jobContext.WorkspaceId;

            isValid &= message.ServiceType == _callContext.ServiceType;
            isValid &= message.ServiceName == _callContext.ServiceName;
            isValid &= message.OperationName == _callContext.OperationName;

            return isValid;
        }
    }
}
