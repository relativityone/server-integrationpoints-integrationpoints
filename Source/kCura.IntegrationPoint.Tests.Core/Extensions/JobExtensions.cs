using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class JobExtensions
    {
        public static Job CopyJobWithStopState(this Job job, StopState state)
        {
            return new JobBuilder().WithJob(job).WithStopState(state).Build();
        }

        public static Job CopyJobWithJobId(this Job job, long jobId)
        {
            return new JobBuilder().WithJob(job).WithJobId(jobId).Build();
        }

        public static Job CreateJob()
        {
            return new JobBuilder().Build();
        }
    }
}