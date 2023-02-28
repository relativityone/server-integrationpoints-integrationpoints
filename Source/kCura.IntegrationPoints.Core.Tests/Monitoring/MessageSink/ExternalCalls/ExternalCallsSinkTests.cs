using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Monitoring.MessageSink.ExternalCalls;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using System;
using System.Runtime.InteropServices;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;

namespace kCura.IntegrationPoints.Core.Tests.Monitoring.MessageSink.ExternalCalls
{
    [TestFixture, Category("Unit")]
    public class ExternalCallsSinkTests
    {
        private IAPILog _logger;
        private ExternalCallsSink _sut;
        private IMetricsManager _apmManager;
        private const string _BUCKET_EXTERNALL_CALL = "IntegrationPoints.Performance.ExternalCall";
        private const string _BUCKET_EXTERNALL_CALL_JOB_SUMMARY = "IntegrationPoints.Performance.ExternalCall.Summary";
        private readonly InstrumentationServiceCallContext _firstCallContextForFirstService = new InstrumentationServiceCallContext("st", "sn", "on");
        private readonly InstrumentationServiceCallContext _secondCallContextForFirstService = new InstrumentationServiceCallContext("st", "sn2", "on2");
        private readonly InstrumentationServiceCallContext _firstCallContextForSecondService = new InstrumentationServiceCallContext("st3", "sn", "on");
        private readonly InstrumentationJobContext _firstJobContext = new InstrumentationJobContext(35454, Guid.NewGuid().ToString(), 4342);
        private readonly InstrumentationJobContext _secondJobContext = new InstrumentationJobContext(5435435, Guid.NewGuid().ToString(), 312321);

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _apmManager = Substitute.For<IMetricsManager>();
            IMetricsManagerFactory metricsManagerFactory = Substitute.For<IMetricsManagerFactory>();
            metricsManagerFactory.CreateAPMManager().Returns(_apmManager);
            _sut = new ExternalCallsSink(metricsManagerFactory, _logger);
        }

        [Test]
        public void ItShouldSendMetricWhenSuccessMetricMessageWasSend()
        {
            // arrange
            ExternalCallCompletedMessage callMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);

            // act
            _sut.OnMessage(callMessage);

            // assert
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL,
                callMessage.Duration,
                Arg.Is<IMetricMetadata>(x => ValidateSuccessMetricMessage(x, callMessage)));
        }

        [Test]
        public void ItShouldLogWarningWhenMetricMessageWasSendBeforeJobStartedMessage()
        {
            // arrange
            ExternalCallCompletedMessage callMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);

            // act
            _sut.OnMessage(callMessage);

            // assert
            _logger.Received().LogWarning("Received external call metric with invalid CorrelationId. Current correlationId: {current}, send: {send}", null, callMessage.CorrelationID);
        }

        [Test]
        public void ItShouldLogWarningWhenMetricCorrelationIdDoesNotMatchJobCorrelationId()
        {
            // arrange
            ExternalCallCompletedMessage callMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);
            var jobStarted = new JobStartedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };
            _sut.OnMessage(jobStarted);

            // act
            _sut.OnMessage(callMessage);

            // assert
            _logger.Received().LogWarning("Received external call metric with invalid CorrelationId. Current correlationId: {current}, send: {send}", jobStarted.CorrelationID, callMessage.CorrelationID);
        }

        [Test]
        public void ItShouldLogWarningWhenJobIsStartedBeforeWasCompleted()
        {
            // arrange
            var firstJobStarted = new JobStartedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };
            _sut.OnMessage(firstJobStarted);
            var secondJobStarted = new JobStartedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };

            // act
            _sut.OnMessage(secondJobStarted);

            // assert
            _logger.Received().LogWarning("Job started before previous was completed, job context will be updated. Previous correlationId {previous}, current: {current}",
                firstJobStarted.CorrelationID, secondJobStarted.CorrelationID);
        }

        [Test]
        public void ItShouldLogWarningWhenJobWasCompletedBeforeWasStarted()
        {
            // arrange
            var jobCompleted = new JobCompletedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };

            // act
            _sut.OnMessage(jobCompleted);

            // assert
            _logger.Received().LogWarning("Job completed correlationId does not match context correlationId. Context: {previous}, message: {current}",
                null, jobCompleted.CorrelationID);
        }

        [Test]
        public void ItShouldLogWarningWhenJobWasCompletedButCorrelationIdNotMatch()
        {
            // arrange
            var jobStarted = new JobStartedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };
            _sut.OnMessage(jobStarted);

            var jobCompleted = new JobCompletedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };

            // act
            _sut.OnMessage(jobCompleted);

            // assert
            _logger.Received().LogWarning("Job completed correlationId does not match context correlationId. Context: {previous}, message: {current}",
                jobStarted.CorrelationID, jobCompleted.CorrelationID);
        }

        [Test]
        public void ItShouldSendAggregatedStatisticsWhenJobIsStartedBeforeWasCompleted()
        {
            // arrange

            ExternalCallCompletedMessage callMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);

            var firstJobStarted = new JobStartedMessage
            {
                CorrelationID = callMessage.CorrelationID
            };

            _sut.OnMessage(firstJobStarted);
            _sut.OnMessage(callMessage);

            var secondJobStarted = new JobStartedMessage
            {
                CorrelationID = Guid.NewGuid().ToString()
            };

            // act
            _sut.OnMessage(secondJobStarted);

            // assert
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                callMessage.Duration,
                Arg.Is<IMetricMetadata>(x => ValidateMessageCorrelationId(x, callMessage.CorrelationID)));
        }

        [Test]
        public void ItShouldSendMetricWhenTwoSuccessMetricMessageWasSend()
        {
            // arrange
            ExternalCallCompletedMessage firstCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);
            ExternalCallCompletedMessage secondCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForSecondService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(21);

            // act
            _sut.OnMessage(firstCallMessage);
            _sut.OnMessage(secondCallMessage);

            // assert
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL,
                firstCallMessage.Duration,
                Arg.Is<IMetricMetadata>(x => ValidateSuccessMetricMessage(x, firstCallMessage)));
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL,
                secondCallMessage.Duration,
                Arg.Is<IMetricMetadata>(x => ValidateSuccessMetricMessage(x, secondCallMessage)));
        }

        [Test]
        public void ItShouldSendAggregatedStatisticsWhenJobIsCompleted()
        {
            ExternalCallCompletedMessage callMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(52);

            var firstJobStarted = new JobStartedMessage
            {
                CorrelationID = callMessage.CorrelationID
            };

            _sut.OnMessage(firstJobStarted);
            _sut.OnMessage(callMessage);

            var firstJobCompleted = new JobCompletedMessage()
            {
                CorrelationID = firstJobStarted.CorrelationID
            };

            // act
            _sut.OnMessage(firstJobCompleted);

            // assert
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                callMessage.Duration,
                Arg.Is<IMetricMetadata>(x => ValidateMessageCorrelationId(x, callMessage.CorrelationID)));
        }

        [Test]
        public void ItShouldAggregateStatisticsWhenJobIsCompleted_OnlySuccessMessages()
        {
            const int firstCallDuration = 52;
            const int secondCallDuration = 47;
            const int thirdCallDuration = 53423;

            ExternalCallCompletedMessage firstCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(firstCallDuration);

            ExternalCallCompletedMessage secondCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_secondCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(secondCallDuration);

            ExternalCallCompletedMessage thirdCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForSecondService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(thirdCallDuration);

            var firstJobStarted = new JobStartedMessage
            {
                CorrelationID = _firstJobContext.CorrelationId
            };

            _sut.OnMessage(firstJobStarted);
            _sut.OnMessage(firstCallMessage);
            _sut.OnMessage(secondCallMessage);
            _sut.OnMessage(thirdCallMessage);

            var firstJobCompleted = new JobCompletedMessage()
            {
                CorrelationID = firstJobStarted.CorrelationID
            };

            // act
            _sut.OnMessage(firstJobCompleted);

            // assert
            int firstServiceExpectedDuration = firstCallDuration + secondCallDuration;
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                firstServiceExpectedDuration,
                Arg.Is<IMetricMetadata>(x => ValidateSummaryMessage(x, _firstCallContextForFirstService.ServiceType, 2, firstServiceExpectedDuration)));
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                thirdCallDuration,
                Arg.Is<IMetricMetadata>(x => ValidateSummaryMessage(x, _firstCallContextForSecondService.ServiceType, 1, thirdCallDuration)));
        }

        [Test]
        public void ItShouldAggregateStatisticsWhenJobIsCompleted_SuccessAndFailMessages()
        {
            const int firstCallDuration = 52;
            const int secondCallDuration = 47;

            ExternalCallCompletedMessage firstCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(firstCallDuration);

            ExternalCallCompletedMessage secondCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_secondCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForFailure(secondCallDuration, "someReason");

            var firstJobStarted = new JobStartedMessage
            {
                CorrelationID = _firstJobContext.CorrelationId
            };

            _sut.OnMessage(firstJobStarted);
            _sut.OnMessage(firstCallMessage);
            _sut.OnMessage(secondCallMessage);

            var firstJobCompleted = new JobCompletedMessage()
            {
                CorrelationID = firstJobStarted.CorrelationID
            };

            // act
            _sut.OnMessage(firstJobCompleted);

            // assert
            int firstServiceExpectedDuration = firstCallDuration + secondCallDuration;
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                firstServiceExpectedDuration,
                Arg.Is<IMetricMetadata>(x => ValidateSummaryMessage(x, _firstCallContextForFirstService.ServiceType, 2, firstServiceExpectedDuration, 1, secondCallDuration)));
        }

        [Test]
        public void ItShouldNotAggregateMetricsWithNotMatchingCorrelationId()
        {
            const int firstCallDuration = 52;
            const int secondCallDuration = 87;
            const int thirdCallDuration = 332;

            ExternalCallCompletedMessage firstCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(firstCallDuration);

            ExternalCallCompletedMessage secondCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_secondJobContext)
                .SetPropertiesForSuccess(secondCallDuration);

            ExternalCallCompletedMessage thirdCallMessage = new ExternalCallCompletedMessage()
                .SetCallContext(_firstCallContextForFirstService)
                .SetJobContext(_firstJobContext)
                .SetPropertiesForSuccess(thirdCallDuration);

            var jobStarted = new JobStartedMessage
            {
                CorrelationID = _firstJobContext.CorrelationId
            };

            _sut.OnMessage(jobStarted);
            _sut.OnMessage(firstCallMessage);
            _sut.OnMessage(secondCallMessage);
            _sut.OnMessage(thirdCallMessage);

            var jobCompleted = new JobCompletedMessage()
            {
                CorrelationID = _firstJobContext.CorrelationId
            };

            // act
            _sut.OnMessage(jobCompleted);

            // assert
            int expectedDuration = firstCallDuration + thirdCallDuration;
            _apmManager.Received().LogCount(
                _BUCKET_EXTERNALL_CALL_JOB_SUMMARY,
                expectedDuration,
                Arg.Is<IMetricMetadata>(x=>ValidateSummaryMessage(x, _firstCallContextForFirstService.ServiceType, 2, expectedDuration)));
        }

        [Test]
        public void ItShouldNotThrowExceptionForNullJobStartedMessages()
        {
            // act
            _sut.OnMessage((JobStartedMessage)null);
        }

        [Test]
        public void ItShouldNotThrowExceptionForNullJobCompletedMessages()
        {
            // act
            _sut.OnMessage((JobCompletedMessage)null);
        }

        [Test]
        public void ItShouldNotThrowExceptionForNullJobFailedMessages()
        {
            // act
            _sut.OnMessage((JobFailedMessage)null);
        }

        [Test]
        public void ItShouldNotThrowExceptionForNullMetricMessages()
        {
            // act
            _sut.OnMessage((ExternalCallCompletedMessage)null);
        }

        private bool ValidateSuccessMetricMessage(IMetricMetadata sendMetadata, ExternalCallCompletedMessage receivedMessage)
        {
            Assert.AreEqual(receivedMessage.Duration, sendMetadata.CustomData[nameof(receivedMessage.Duration)]);
            Assert.IsFalse((bool)sendMetadata.CustomData[nameof(receivedMessage.HasFailed)]);
            Assert.IsEmpty((string)sendMetadata.CustomData[nameof(receivedMessage.FailureReason)]);
            Assert.AreEqual(receivedMessage.ServiceType, sendMetadata.CustomData[nameof(receivedMessage.ServiceType)]);
            Assert.AreEqual(receivedMessage.ServiceName, sendMetadata.CustomData[nameof(receivedMessage.ServiceName)]);
            Assert.AreEqual(receivedMessage.OperationName, sendMetadata.CustomData[nameof(receivedMessage.OperationName)]);

            return true;
        }

        private bool ValidateMessageCorrelationId(IMetricMetadata sendMetadata, string correlationId)
        {
            Assert.AreEqual(correlationId, sendMetadata.CorrelationID);

            return true;
        }

        private bool ValidateSummaryMessage(IMetricMetadata sendMetadata, string serviceType, long totalCount, long totalDuration)
        {
            Assert.AreEqual(serviceType, sendMetadata.CustomData[nameof(ExternalCallsJobSummaryMessage.ServiceType)]);
            Assert.AreEqual(totalCount, sendMetadata.CustomData[nameof(ExternalCallsJobSummaryMessage.TotalCount)]);
            Assert.AreEqual(totalDuration, sendMetadata.CustomData[nameof(ExternalCallsJobSummaryMessage.TotalDuration)]);

            return true;
        }

        private bool ValidateSummaryMessage(IMetricMetadata sendMetadata, string serviceType, long totalCount, long totalDuration, long failedCount, long failedDuration)
        {
            ValidateSummaryMessage(sendMetadata, serviceType, totalCount, totalDuration);

            Assert.AreEqual(failedCount, sendMetadata.CustomData[nameof(ExternalCallsJobSummaryMessage.FailedCount)]);
            Assert.AreEqual(failedDuration, sendMetadata.CustomData[nameof(ExternalCallsJobSummaryMessage.FailedDuration)]);

            return true;
        }
    }
}
