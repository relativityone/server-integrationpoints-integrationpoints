using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Config;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    public class ExternalServiceInstrumentationProviderWithJobContext : IExternalServiceInstrumentationProvider
    {
        private InstrumentationJobContext _currentJobContext;
        private const bool _DEFAULT_MEASURE_DURATION_OF_EXTERNAL_CALLS = false;
        private readonly IAPILog _logger;
        private readonly IMessageService _messageService;
        private readonly IJobContextProvider _jobContextProvider;
        private readonly ISerializer _serializer;
        private readonly IConfig _config;

        public ExternalServiceInstrumentationProviderWithJobContext(IJobContextProvider jobContextProvider, IMessageService messageService, IAPILog logger, ISerializer serializer, IConfig config)
        {
            _jobContextProvider = jobContextProvider;
            _messageService = messageService;
            _logger = logger.ForContext<ExternalServiceInstrumentationProviderWithJobContext>();
            _serializer = serializer;
            _config = config;
        }

        public IExternalServiceInstrumentation Create(string serviceType, string serviceName, string operationName)
        {
            var callContext = new InstrumentationServiceCallContext(serviceType, serviceName, operationName);
            if (IsMeasuringOfExternalCallEnabled())
            {
                InstrumentationJobContext jobContext = GetJobContext();
                return new ExternalServiceInstrumentation(jobContext, callContext, _messageService, _logger);
            }
            return new ExternalServiceLogsInstrumentation(callContext, _logger);
        }

        public IExternalServiceSimpleInstrumentation CreateSimple(string serviceType, string serviceName, string operationName)
        {
            return new ExternalServiceSimpleInstrumentation(Create(serviceType, serviceName, operationName));
        }

        private bool IsMeasuringOfExternalCallEnabled()
        {
            bool result = _DEFAULT_MEASURE_DURATION_OF_EXTERNAL_CALLS;
            try
            {
                result = _config.MeasureDurationOfExternalCalls;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occured while reading setting: {nameOfSetting} from configuration. Default value will be used: {value}",
                    nameof(_config.MeasureDurationOfExternalCalls), _DEFAULT_MEASURE_DURATION_OF_EXTERNAL_CALLS);
            }
            return result;
        }

        private InstrumentationJobContext GetJobContext()
        {
            Job job = GetExecutingJob();
            if (job == null)
            {
                _currentJobContext = InstrumentationJobContext.EmptyContext;
            }
            else if (_currentJobContext?.JobId != job.JobId)
            {
                string correlationId = GetCorrelationId(job);
                _currentJobContext = new InstrumentationJobContext(job.JobId, correlationId, job.WorkspaceID);
            }

            return _currentJobContext;
        }

        private Job GetExecutingJob()
        {
            Job result = null;
            try
            {
                result = _jobContextProvider.Job;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error while retrieving job from context.");
            }
            return result;
        }

        private string GetCorrelationId(Job job)
        {
            string result = string.Empty;
            try
            {
                TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
                result = taskParameters.BatchInstance.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occured while retrieving batch instance for job: {jobId}", job.JobId);
            }
            return result;
        }
    }
}
