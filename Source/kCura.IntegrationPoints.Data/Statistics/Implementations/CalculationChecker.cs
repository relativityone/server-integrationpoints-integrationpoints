using System.Collections.Concurrent;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class CalculationChecker : ICalculationChecker
    {
        private static ConcurrentDictionary<int, bool> _itemsWithCalculationInProgress = new ConcurrentDictionary<int, bool>();

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
            bool removed = _itemsWithCalculationInProgress.TryRemove(integrationPointId, out _);

            if (!removed && _itemsWithCalculationInProgress.ContainsKey(integrationPointId))
            {
                _itemsWithCalculationInProgress[integrationPointId] = false;
            }
        }
    }
}
