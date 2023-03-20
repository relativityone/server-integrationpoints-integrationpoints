using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class JobInfo : IJobInfo
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly Job _job;
        private readonly ISerializer _serializer;

        public JobInfo(IJobHistoryService jobHistoryService, Job job, ISerializer serializer)
        {
            _jobHistoryService = jobHistoryService;
            _job = job;
            _serializer = serializer;
        }

        public DateTime GetStartTimeUtc()
        {
            JobHistory jobHistoryRdo = GetJobHistoryRdo();

            return jobHistoryRdo.StartTimeUTC ?? DateTime.UtcNow;
        }

        public string GetName()
        {
            JobHistory jobHistoryRdo = GetJobHistoryRdo();
            return jobHistoryRdo.Name;
        }

        private JobHistory GetJobHistoryRdo()
        {
            var taskParameters = _serializer.Deserialize<TaskParameters>(_job.JobDetails);
            JobHistory jobHistoryRdo = _jobHistoryService.GetRdoWithoutDocuments(taskParameters.BatchInstance);
            return jobHistoryRdo;
        }
    }
}
