using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Extensions;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class SystemHealthReporter : ISystemHealthReporter
    {
        private readonly IEnumerable<IHealthStatisticReporter> _reporters;

        public SystemHealthReporter(IEnumerable<IHealthStatisticReporter> reporters)
        {
            _reporters = reporters;
        }


        public async Task<Dictionary<string, object>> GetSystemHealthStatisticsAsync()
        {
            Dictionary<string, object> systemHealthStatistics = new Dictionary<string, object>();
            List<Task<Dictionary<string, object>>> taskList = new List<Task<Dictionary<string, object>>>();
            foreach (var reporter in _reporters)
            {
                taskList.Add(Task.Run(() => reporter.GetStatisticAsync()));
            }

            Dictionary<string, object>[] allReportedStatistics = await Task.WhenAll(taskList).ConfigureAwait(false);
            foreach (var stat in allReportedStatistics)
            {
                systemHealthStatistics.AddDictionary(stat);
            }

            return systemHealthStatistics;
        }



    }
}
