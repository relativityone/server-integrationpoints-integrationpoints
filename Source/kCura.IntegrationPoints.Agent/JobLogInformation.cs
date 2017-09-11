using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent
{
    public class JobLogInformation
    {
        public Job Job { get; set; }
        public JobLogState State { get; set; }
        public string Details { get; set; }
    }
}
