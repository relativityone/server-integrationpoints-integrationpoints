using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Logging
{
    public class JobLogInformation
    {
        public Job Job { get; set; }
        public JobLogState State { get; set; }
        public string Details { get; set; }
    }
}
