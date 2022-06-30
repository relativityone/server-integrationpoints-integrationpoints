using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using kCura.IntegrationPoints.Common.Metrics;
using Relativity.API;
using System.Threading;
using kCura.IntegrationPoints.Common.Agent;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public class MemoryUsageReporter : IMemoryUsageReporter
    {
        private readonly IRemovableAgent _agent;
        private readonly IAPM _apmClient;
        private readonly IAPILog _logger;
        private readonly IRipMetrics _ripMetric;
        private readonly IProcessMemoryHelper _processMemoryHelper;
        private readonly IAppDomainMonitoringEnabler _appDomainMonitoringEnabler;
        private readonly IMonitoringConfig _config;

        private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
        private static string _METRIC_NAME = "IntegrationPoints.Performance.System";

        public MemoryUsageReporter(IAPM apmClient, IAPILog logger,
            IRipMetrics ripMetric,IProcessMemoryHelper processMemoryHelper,
            IAppDomainMonitoringEnabler appDomainMonitoringEnabler, IMonitoringConfig config, IRemovableAgent agent)
        {
            _processMemoryHelper = processMemoryHelper;
            _apmClient = apmClient;
            _logger = logger;
            _ripMetric = ripMetric;
            _appDomainMonitoringEnabler = appDomainMonitoringEnabler;
            _agent = agent;
            _config = config;
        }

        public IDisposable ActivateTimer(long jobId, string jobDetails, string jobType)
        {
            return _appDomainMonitoringEnabler.EnableMonitoring()
                ? new Timer(state => Execute(jobId, jobDetails, jobType), null, TimeSpan.Zero, _config.MemoryUsageInterval)
                : Disposable.Empty;
        }

        private void Execute(long jobId, string workflowId, string jobType)
        {
            if (_agent.ToBeRemoved)
            {
                _logger.LogInformation("Memory metrics can't be sent. Agent, agentGuid = {AgentGuid}, is marked as ToBeRemoved.", _agent.AgentGuid);
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
