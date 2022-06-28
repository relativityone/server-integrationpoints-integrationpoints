using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Toggles;
using System;
using System.Reactive.Disposables;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter
{
    public class HeartbeatReporter : IHeartbeatReporter
    {
        private readonly IAPILog _log;
        private readonly IQueueQueryManager _queueManager;
        private readonly IMonitoringConfig _config;
        private readonly IDateTime _dateTime;
        private readonly IToggleProvider _toggleProvider;

        public HeartbeatReporter(IQueueQueryManager queueManager, IMonitoringConfig config,
            IDateTime dateTime, IAPILog log, IToggleProvider toggleProvider)
        {
            _queueManager = queueManager;
            _config = config;
            _dateTime = dateTime;
            _log = log;
            _toggleProvider = toggleProvider;
        }

        public IDisposable ActivateHeartbeat(long jobId)
        {
            if(!_toggleProvider.IsEnabled<EnableHeartbeatToggle>())
            {
                _log.LogInformation("EnableHeartbeatToggle is disabled. JobID {jobId} heartbeat won't be updated", jobId);
                return Disposable.Empty;
            }

            return new Timer(state => Execute(jobId), null, TimeSpan.Zero, _config.HeartbeatInterval);
        }

        private void Execute(long jobId)
        {
            try
            {
                DateTime nowUtc = _dateTime.UtcNow;
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
    }
}
