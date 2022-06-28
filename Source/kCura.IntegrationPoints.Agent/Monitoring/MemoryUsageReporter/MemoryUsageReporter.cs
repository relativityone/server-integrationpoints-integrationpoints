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
        private IAPM _apmClient;
        private IAPILog _logger;
        private IRipMetrics _ripMetric;
        private IProcessMemoryHelper _processMemoryHelper;
        private IAppDomainMonitoringEnabler _appDomainMonitoringEnabler;
        private readonly IRemovableAgent _agent;

        private static string _METRIC_LOG_NAME = "Relativity.IntegrationPoints.Performance.System";
        private static string _METRIC_NAME = "IntegrationPoints.Performance.System";


        public MemoryUsageReporter(IAPM apmClient, IAPILog logger, IRipMetrics ripMetric, IProcessMemoryHelper processMemoryHelper, 
            IAppDomainMonitoringEnabler appDomainMonitoringEnabler, IRemovableAgent agent)
        {
            _processMemoryHelper = processMemoryHelper;
            _apmClient = apmClient;
            _logger = logger;
            _ripMetric = ripMetric;
            _appDomainMonitoringEnabler = appDomainMonitoringEnabler;
            _agent = agent;
        }

        public IDisposable ActivateTimer(int timeIntervalMilliseconds, long jobId, string jobDetails, string jobType)
        {
            return _appDomainMonitoringEnabler.EnableMonitoring()
                ? new Timer(state => Execute(jobId, jobDetails, jobType), null, 0, timeIntervalMilliseconds)
                : Disposable.Empty;
        }

        private void Execute(long jobId, string workflowId, string jobType)
        {
            if (_agent.ToBeRemoved)
            {
                Agent agent = _agent as Agent;
                _logger.LogInformation("Memory metrics can't be sent. Agent, agentId = {agentId}, is marked as ToBeRemoved.", agent.AgentID);

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
