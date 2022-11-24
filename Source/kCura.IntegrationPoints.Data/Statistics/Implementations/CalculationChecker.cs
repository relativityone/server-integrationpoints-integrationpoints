using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class CalculationChecker : ICalculationChecker
    {
        private ConcurrentDictionary<int, bool> _itemsWithCalculationInProgress = new ConcurrentDictionary<int, bool>();

        public bool IsCalculating(int integrationPointId)
        {
            if (_itemsWithCalculationInProgress.ContainsKey(integrationPointId))
            {
                return _itemsWithCalculationInProgress[integrationPointId];
            }

            return false;
        }

        public void MarkAsCalculating(int integrationPointId)
        {
            if (_itemsWithCalculationInProgress.ContainsKey(integrationPointId))
            {
                _itemsWithCalculationInProgress[integrationPointId] = true;
            }
            else
            {
                _itemsWithCalculationInProgress.TryAdd(integrationPointId, true);
            }
        }

        public void MarkCalculationFinished(int integrationPointId)
        {
            bool removed = _itemsWithCalculationInProgress.TryRemove(integrationPointId, out bool value);

            if (!removed && _itemsWithCalculationInProgress.ContainsKey(integrationPointId))
            {
                _itemsWithCalculationInProgress[integrationPointId] = false;
            }
        }
    }
}
