using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using Job = kCura.IntegrationPoints.Data.Job;

namespace kCura.IntegrationPoints.Agent.Sync
{
    internal class ScheduledSyncTask : IScheduledSyncTask
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISerializer _serializer;
        private readonly IDateTime _dateTime;

        public JobHistory JobHistory { get; private set; }

        public ScheduledSyncTask(IJobHistoryService jobHistoryService, IIntegrationPointService integrationPointService, ISerializer serializer, IDateTime dateTime)
        {
            _jobHistoryService = jobHistoryService;
            _integrationPointService = integrationPointService;
            _serializer = serializer;
            _dateTime = dateTime;
        }

        public void Execute(Job job)
        {
            PreExecute(job);

            throw new NotImplementedException("This code path should not be reached. Contact Customer Support for help.");
        }

        private void PreExecute(Job job)
        {
            TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);

            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);

            JobHistory = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(integrationPoint, taskParameters.BatchInstance, _dateTime.UtcNow);
        }
    }
}
