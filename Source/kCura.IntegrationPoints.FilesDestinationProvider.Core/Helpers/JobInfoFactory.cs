

using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class JobInfoFactory : IJobInfoFactory
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ISerializer _serializer;

        public JobInfoFactory(IJobHistoryService jobHistoryService, ISerializer serializer)
        {
            _jobHistoryService = jobHistoryService;
            _serializer = serializer;
        }

        public IJobInfo Create(Job job)
        {
            return new JobInfo(_jobHistoryService, job, _serializer);
        }
    }
}
