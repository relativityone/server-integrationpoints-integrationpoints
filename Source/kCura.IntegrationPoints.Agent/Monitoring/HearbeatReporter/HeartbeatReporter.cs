using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter
{
    public class HeartbeatReporter : IHeartbeatReporter
    {
        private DateTime _startDateTime;
        private bool _runningJobTimeExceededCheck;
        private string _correlationId;

        private readonly IQueueQueryManager _queueManager;
        private readonly IMonitoringConfig _config;
        private readonly IDateTime _dateTime;
        private readonly IToggleProvider _toggleProvider;
        private readonly IAPM _apmClient;
        private readonly ITimerFactory _timerFactory;
        private readonly IAPILog _log;

        private static readonly string _METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME = "Relativity.IntegrationPoints.Performance.RunningJobTimeExceeded";

        public HeartbeatReporter(IQueueQueryManager queueManager, IMonitoringConfig config,
            IDateTime dateTime, IToggleProvider toggleProvider, IAPM apmClient, ITimerFactory timerFactory, IAPILog log)
        {
            _queueManager = queueManager;
            _config = config;
            _dateTime = dateTime;
            _log = log;
            _toggleProvider = toggleProvider;
            _apmClient = apmClient;
            _timerFactory = timerFactory;
            _runningJobTimeExceededCheck = true;
        }

        public IDisposable ActivateHeartbeat(long jobId)
        {
            _startDateTime = _dateTime.UtcNow;
            _correlationId = Guid.NewGuid().ToString();

            if (!_toggleProvider.IsEnabled<EnableHeartbeatToggle>())
            {
                _log.LogInformation("EnableHeartbeatToggle is disabled. JobID {jobId} heartbeat won't be updated", jobId);
                return Disposable.Empty;
            }

            return _timerFactory.Create(state => Execute(jobId), null, _config.TimerStartDelay, _config.HeartbeatInterval, "Heartbeat Timer");
        }

        private void Execute(long jobId)
        {
            try
            {
                DateTime nowUtc = _dateTime.UtcNow;
                SendMetricWhenJobRunningTimeThresholdIsExceeded(jobId, nowUtc);
                int affectedJobs = _queueManager.Heartbeat(jobId, nowUtc)
                    .Execute();

                bool isValid = ValidateAffectedJobsCount(jobId, affectedJobs);
                if (!isValid)
                {
                    return;
                }

                _log.LogInformation("Job {jobId} heartbeat was updated with {dateTime}", jobId, nowUtc);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Heartbeat JobId: {jobId}", jobId);
            }
        }

        private bool ValidateAffectedJobsCount(long jobId, int affectedJobs)
        {
            if (affectedJobs == 0)
            {
                _log.LogWarning("Job {jobId} was not updated, because it doesn't exist.", jobId);
                return false;
            }
            else if (affectedJobs > 1)
            {
                _log.LogWarning("Too many jobs ({affectedJobs}) exist in ScheduleAgentQueue with JobId {jobId}.", affectedJobs, jobId);
                return false;
            }

            return true;
        }

        private void SendMetricWhenJobRunningTimeThresholdIsExceeded(long jobId, DateTime utcNow)
        {
            if (_runningJobTimeExceededCheck && (utcNow - _startDateTime) > _config.LongRunningJobsTimeThreshold)
            {
                Dictionary<string, object> runningJobTimeCustomData = new Dictionary<string, object>()
                {
                    { "r1.team.id", "PTCI-2456712" },
                    { "r1.job.id", jobId.ToString() }
                };

                _apmClient.CountOperation(_METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME, correlationID: _correlationId, customData: runningJobTimeCustomData)
                    .Write();

                _runningJobTimeExceededCheck = false;
            }
        }
    }
}
