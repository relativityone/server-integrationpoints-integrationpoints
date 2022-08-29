using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using kCura.IntegrationPoints.Agent.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class MemoryUsageReporter : IMemoryUsageReporter
    {
        private ITimer _timer;

        private readonly IRemovableAgent _agent;
        private readonly IToggleProvider _toggleProvider;
        private readonly ITimerFactory _timerFactory;
        private readonly ISystemHealthReporter _systemHealthReporter;
        private readonly IAPM _apmClient;
        private readonly IRipMetrics _ripMetric;
        private readonly IProcessMemoryHelper _processMemoryHelper;
        private readonly IAppDomainMonitoringEnabler _appDomainMonitoringEnabler;
        private readonly IMonitoringConfig _config;
        private readonly IAPILog _logger;

        private static readonly string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
        private static readonly string _METRIC_NAME = "IntegrationPoints.Performance.System";

        public MemoryUsageReporter(IAPM apmClient, IRipMetrics ripMetric, IProcessMemoryHelper processMemoryHelper,
            IAppDomainMonitoringEnabler appDomainMonitoringEnabler, IMonitoringConfig config, IRemovableAgent agent,
            IToggleProvider toggleProvider, ITimerFactory timerFactory, ISystemHealthReporter systemHealthReporter, IAPILog logger)
        {
            _processMemoryHelper = processMemoryHelper;
            _apmClient = apmClient;
            _ripMetric = ripMetric;
            _appDomainMonitoringEnabler = appDomainMonitoringEnabler;
            _agent = agent;
            _toggleProvider = toggleProvider;
            _timerFactory = timerFactory;
            _systemHealthReporter = systemHealthReporter;
            _config = config;
            _logger = logger;
        }

        public IDisposable ActivateTimer(long jobId, string jobDetails, string jobType)
        {
            if (!_toggleProvider.IsEnabled<EnableMemoryUsageReportingToggle>())
            {
                _logger.LogInformation("EnableMemoryUsageReportingToggle is disabled. JobID {jobId} memory usage metrics won't be send", jobId);
                return Disposable.Empty;
            }

            if (_appDomainMonitoringEnabler.EnableMonitoring())
            {
                TimerCallback timerCallback = state => Execute(jobId, jobDetails, jobType);
                _timer = _timerFactory.Create(timerCallback, null, _config.TimerStartDelay, _config.MemoryUsageInterval, "Memory Usage Timer");

                return _timer;
            }

            return Disposable.Empty;
        }

        private void Execute(long jobId, string workflowId, string jobType)
        {
            if (_agent.ToBeRemoved)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _logger.LogInformation("Memory metrics can't be sent. Agent, AgentInstanceGuid = {AgentInstanceGuid}, is marked as ToBeRemoved.", _agent.AgentInstanceGuid);
                return;
            }

            try
            {
                Dictionary<string, object> customData = new Dictionary<string, object>()
                {
                    { "JobId", jobId },
                    { "JobType", jobType },
                    { "WorkflowId", workflowId}
                };

                _processMemoryHelper.GetApplicationSystemStatistics().ToList().ForEach(x => customData.Add(x.Key, x.Value));
                _systemHealthReporter.GetSystemHealthStatistics().ToList().ForEach(x => customData.Add(x.Key, x.Value));

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
