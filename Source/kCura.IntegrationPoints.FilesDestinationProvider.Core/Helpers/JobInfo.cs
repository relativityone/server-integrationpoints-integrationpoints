using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

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
            JobHistory jobHistoryRdo = _jobHistoryService.GetRdoWithoutDocuments(Guid.Parse(_job.CorrelationID));
            return jobHistoryRdo;
        }
    }
}
