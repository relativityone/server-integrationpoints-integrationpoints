using System.Threading.Tasks;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Logging
{
    internal interface IItemLevelErrorLogAggregator
    {
        void AddItemLevelError(ItemLevelError itemLevelError, int artifactId);
        Task LogAllItemLevelErrorsAsync();
    }
}