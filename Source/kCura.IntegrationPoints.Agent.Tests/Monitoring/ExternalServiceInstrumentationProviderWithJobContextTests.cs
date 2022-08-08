using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Config;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class ExternalServiceInstrumentationProviderWithJobContextTests
    {
        private IDisposable _disposableJobContext;
        private ExternalServiceInstrumentationProviderWithJobContext _sut;
        private IConfig _config;
        private IAPILog _logger;
        private IMessageService _messageService;

        private const string _SERVICE_TYPE = "TestService";
        private const string _SERVICE_NAME = "ProviderTests";
        private const string _OPERATION_NAME = "Unit";

        private const long _JOB_ID = 45325324531;
        private const int _WORKSPACE_ID = 343;
        private readonly Guid _BATCH_INSTANCE = Guid.NewGuid();

        private readonly IJobContextProvider _jobContextProvider = new JobContextProvider();

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _logger.ForContext<ExternalServiceInstrumentationProviderWithJobContext>().Returns(_logger);
            _logger.ForContext<ExternalServiceLogsInstrumentation>().Returns(_logger);
            _logger.ForContext<ExternalServiceInstrumentation>().Returns(_logger);
            _config = Substitute.For<IConfig>();

            ISerializer serializer = new JSONSerializer();
            _messageService = Substitute.For<IMessageService>();

            _sut = new ExternalServiceInstrumentationProviderWithJobContext(_jobContextProvider, _messageService, _logger, serializer, _config);
        }

        [Test]
        public void ItShouldReturnLogOnlyImplementationWhenInstanceSetingsIsSetToFalse()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(false);

            // act
            IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // assert
            Assert.IsInstanceOf<ExternalServiceLogsInstrumentation>(instrumentation);
        }

        [Test]
        public void ItShouldReturnLogOnlyImplementationWhenExceptionIsThrownFromIConfig()
        {
            // arrange
            Exception expectedException = new Exception();
            _config.MeasureDurationOfExternalCalls.Throws(expectedException);

            // act
            IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // assert
            Assert.IsInstanceOf<ExternalServiceLogsInstrumentation>(instrumentation);
        }

        [Test]
        public void ItShouldLogWarningWhenExceptionIsThrownFromIConfig()
        {
            // arrange
            var expectedException = new Exception();
            _config.MeasureDurationOfExternalCalls.Throws(expectedException);

            // act
            _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // assert
            _logger.Received().LogWarning(expectedException, Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldPassProperContextToLogsOnlyInstrumentation()
        {
            // arrange
            Exception expectedException = new Exception();
            _config.MeasureDurationOfExternalCalls.Returns(false);
            IExternalServiceInstrumentation instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // act
            IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
            startedInstrumentation.Failed(expectedException);

            // assert
            _logger.Received().LogError(expectedException, Arg.Any<string>(), Arg.Is<InstrumentationServiceCallContext>(x => ValidateCallContext(x)));
        }

        [Test]
        public void ItShouldReturnMetricsInstrumentationWhenEnabledInConfiguration()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);

            // act
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // assert
            Assert.IsInstanceOf<ExternalServiceInstrumentation>(instrumentation);
        }

        [Test]
        public void ItShouldReturnMetricsInstrumentationWhenJobContextIsDisposed()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);
            DisposeCurrentJobContext();

            // act
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // assert
            Assert.IsInstanceOf<ExternalServiceInstrumentation>(instrumentation);
        }

        [Test]
        public void ItShouldPassCorrectCallContextToMetricsInstrumentation()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);
            SetJobContext(GetJob());
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // act
            IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
            startedInstrumentation.Completed();

            // assert
            _messageService.Received().Send(Arg.Is<IMessage>(x => ValidateCallContextInMetrics(x)));
        }

        [Test]
        public void ItShouldPassCorrectJobContextToMetricsInstrumentation()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);
            SetJobContext(GetJob());
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // act
            IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
            startedInstrumentation.Completed();

            // assert
            _messageService.Received().Send(Arg.Is<IMessage>(x => ValidateDefaultJobContextIsSet(x)));
        }

        [Test]
        public void ItShouldUseEmptyJobContextWhenJobContextIsDisposed()
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);
            DisposeCurrentJobContext();
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // act
            IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
            startedInstrumentation.Completed();

            // assert
            _messageService.Received().Send(Arg.Is<IMessage>(x=>ValidateEmptyJobContextIsSet(x)));
        }

        [Test]
        public void ItShouldUseEmptyCorrelationIdJobContextWhenJobDetailsAreMissing()
        {
            ItShouldUseEmptyCorrelationIdJobContextWhenBatchInstanceIsNotSetInJobDetails(GetJobWithoutJobDetails());
        }

        [Test]
        public void ItShouldUseEmptyCorrelationIdJobContextWhenJobDetailsAreInvalid()
        {
            ItShouldUseEmptyCorrelationIdJobContextWhenBatchInstanceIsNotSetInJobDetails(GetJobWithInvalidJobDetails());
        }

        private void ItShouldUseEmptyCorrelationIdJobContextWhenBatchInstanceIsNotSetInJobDetails(Job job)
        {
            // arrange
            _config.MeasureDurationOfExternalCalls.Returns(true);
            SetJobContext(job);
            IExternalServiceInstrumentation
                instrumentation = _sut.Create(_SERVICE_TYPE, _SERVICE_NAME, _OPERATION_NAME);

            // act
            IExternalServiceInstrumentationStarted startedInstrumentation = instrumentation.Started();
            startedInstrumentation.Completed();

            // assert
            _messageService.Received().Send(Arg.Is<IMessage>(x => ValidateEmptyCorrelationIdIsSet(x)));
        }

        private bool ValidateCallContext(InstrumentationServiceCallContext context)
        {
            bool isValid = true;
            isValid &= context.ServiceType == _SERVICE_TYPE;
            isValid &= context.ServiceName == _SERVICE_NAME;
            isValid &= context.OperationName == _OPERATION_NAME;

            return isValid;
        }

        private bool ValidateDefaultJobContextIsSet(IMessage message)
        {
            return ValidateJobContext(message, _JOB_ID, _WORKSPACE_ID, _BATCH_INSTANCE.ToString());
        }

        private bool ValidateEmptyJobContextIsSet(IMessage message)
        {
            return ValidateJobContext(message, 0, 0, string.Empty);
        }

        private bool ValidateEmptyCorrelationIdIsSet(IMessage message)
        {
            return ValidateJobContext(message, _JOB_ID, _WORKSPACE_ID, string.Empty);
        }

        private bool ValidateJobContext(IMessage message, long jobId, int workspaceId, string correlationId)
        {
            var castedMessage = message as JobMessageBase;
            if (castedMessage == null)
            {
                return false;
            }
            
            bool isValid = true;
            isValid &= castedMessage.CorrelationID == correlationId;
            isValid &= castedMessage.WorkspaceID == workspaceId;
            isValid &= (string)castedMessage.CustomData["JobID"] == jobId.ToString();
            return isValid;
        }

        private bool ValidateCallContextInMetrics(IMessage message)
        {
            var castedMessage = message as JobMessageBase;
            if (castedMessage == null)
            {
                return false;
            }

            bool isValid = true;
            isValid &= (string)castedMessage.CustomData["ServiceType"] == _SERVICE_TYPE;
            isValid &= (string)castedMessage.CustomData["ServiceName"] == _SERVICE_NAME;
            isValid &= (string) castedMessage.CustomData["OperationName"] == _OPERATION_NAME;
            return isValid;
        }

        private Job GetJob()
        {
            var parameters = new TaskParameters
            {
                BatchInstance = _BATCH_INSTANCE
            };
            var jobBuilder = new JobBuilder();
            return jobBuilder
                .WithJobId(_JOB_ID)
                .WithWorkspaceId(_WORKSPACE_ID)
                .WithJobDetails(parameters)
                .Build();
        }

        private Job GetJobWithoutJobDetails()
        {
            var jobBuilder = new JobBuilder();
            return jobBuilder
                .WithJobId(_JOB_ID)
                .WithWorkspaceId(_WORKSPACE_ID)
                .WithJobDetails(string.Empty)
                .Build();
        }

        private Job GetJobWithInvalidJobDetails()
        {
            var jobBuilder = new JobBuilder();
            return jobBuilder
                .WithJobId(_JOB_ID)
                .WithWorkspaceId(_WORKSPACE_ID)
                .WithJobDetails("<invalid>JobDetails</invalid>")
                .Build();
        }

        private void SetJobContext(Job job)
        {
            DisposeCurrentJobContext();
            _disposableJobContext = _jobContextProvider.StartJobContext(job);
        }

        private void DisposeCurrentJobContext()
        {
            _disposableJobContext?.Dispose();
            _disposableJobContext = null;
        }
    }
}
