using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.ScheduleQueue.Core
{
    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException() :base(){}

        public AgentNotFoundException(string message) : base(message)
        {
        }
    }
}
