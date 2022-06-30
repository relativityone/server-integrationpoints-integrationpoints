using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class MemoryUsageReporter : IMemoryUsageReporter
    {
        private DateTime _startDateTime;
        private bool _runningJobTimeExceededCheck;

        private readonly IAPM _apmClient;
        private readonly IAPILog _logger;
        private readonly IRipMetrics _ripMetric;
        private readonly IProcessMemoryHelper _processMemoryHelper;
        private readonly IAppDomainMonitoringEnabler _appDomainMonitoringEnabler;
        private readonly IMonitoringConfig _config;

        private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
        private static string _METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME = "Relativity.IntegrationPoints.Performance.RunningJobTimeExceeded";
        private static string _METRIC_NAME = "IntegrationPoints.Performance.System";


        public MemoryUsageReporter(IAPM apmClient, IAPILog logger,
            IRipMetrics ripMetric,IProcessMemoryHelper processMemoryHelper,
            IAppDomainMonitoringEnabler appDomainMonitoringEnabler, IMonitoringConfig config)
        {
            _processMemoryHelper = processMemoryHelper;
            _apmClient = apmClient;
            _logger = logger;
            _ripMetric = ripMetric;
            _appDomainMonitoringEnabler = appDomainMonitoringEnabler;
            _config = config;
            _runningJobTimeExceededCheck = true;
        }

        public IDisposable ActivateTimer(long jobId, string jobDetails, string jobType)
        {
            _startDateTime = DateTime.Now;
            return _appDomainMonitoringEnabler.EnableMonitoring()
                ? new Timer(state => Execute(jobId, jobDetails, jobType), null, TimeSpan.Zero, _config.MemoryUsageInterval)
                : Disposable.Empty;
        }

        private void Execute(long jobId, string workflowId, string jobType)
        {
            try
            {
                Dictionary<string, object> runningJobTimeCustomData = new Dictionary<string, object>()
                {
                    { "r1.team.id", "PTCI-RIP" },
                    { "JobId", jobId },
                    { "JobType", jobType },
                    { "WorkflowId", workflowId}
                };

                const int runningJobTimeThreshold = 8;
                if (_runningJobTimeExceededCheck && (DateTime.Now - _startDateTime).Hours > runningJobTimeThreshold)
                {
                    _apmClient.CountOperation(_METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME, correlationID: workflowId, customData: runningJobTimeCustomData)
                        .Write();

                    _runningJobTimeExceededCheck = false;
                }

                Dictionary<string, object> customData = new Dictionary<string, object>()
                {
                    { "JobId", jobId },
                    { "JobType", jobType },
                    { "WorkflowId", workflowId}
                };

                _processMemoryHelper.GetApplicationSystemStats().ToList().ForEach(x => customData.Add(x.Key, x.Value));

                _apmClient.CountOperation(_METRIC_NAME, correlationID: workflowId, customData: customData).Write();
                _logger.LogInformation("Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}", _METRIC_LOG_NAME, customData, _ripMetric.GetWorkflowId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Execute while sending APM metric");
            }
        }
    }
}
