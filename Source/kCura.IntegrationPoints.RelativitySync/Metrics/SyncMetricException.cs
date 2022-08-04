using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    [Serializable]
    public class SyncMetricException : Exception
    {
        public SyncMetricException()
        {
        }

        public SyncMetricException(string message)
            : base(message)
        {
        }

        public SyncMetricException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
