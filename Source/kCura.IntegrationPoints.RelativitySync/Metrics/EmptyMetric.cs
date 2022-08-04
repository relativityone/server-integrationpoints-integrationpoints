using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    class EmptyMetric : IMetric
    {
        public Task SendAsync()
        {
            return Task.CompletedTask;
        }
    }
}
