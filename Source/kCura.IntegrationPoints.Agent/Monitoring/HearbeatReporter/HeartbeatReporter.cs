﻿using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using System;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter
{
    public class HeartbeatReporter : IHeartbeatReporter
    {
        private readonly IAPILog _log;
        private readonly IQueueQueryManager _queueManager;
        private readonly IMonitoringConfig _config;
        private readonly IDateTime _dateTime;

        public HeartbeatReporter(IQueueQueryManager queueManager, IMonitoringConfig config, IDateTime dateTime, IAPILog log)
        {
            _queueManager = queueManager;
            _config = config;
            _dateTime = dateTime;
            _log = log;
        }

        public IDisposable ActivateHeartbeat(long jobId)
        {
            return new Timer(state => Execute(jobId), null, TimeSpan.Zero, _config.HeartbeatInterval);
        }

        private void Execute(long jobId)
        {
            try
            {
                DateTime nowUtc = _dateTime.UtcNow;
                int affectedJobs = _queueManager.Heartbeat(jobId, nowUtc)
                    .Execute();

                HandleHeartbeatError(jobId, affectedJobs);

                _log.LogInformation("Job {jobId} heartbeat was updated with {dateTime}", jobId, nowUtc);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to Heartbeat JobId: {jobId}", jobId);
            }
        }

        private void HandleHeartbeatError(long jobId, int affectedJobs)
        {
            if (affectedJobs == 0)
            {
                throw new InvalidOperationException($"Job {jobId} was not updated, because it doesn't exist.");
            }
            else if (affectedJobs > 1)
            {
                throw new InvalidOperationException($"Too many jobs ({affectedJobs}) exist in ScheduleAgentQueue with JobId {jobId}.");
            }
        }
    }
}
