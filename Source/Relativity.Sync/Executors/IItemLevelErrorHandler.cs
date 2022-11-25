using System.Threading.Tasks;
using Relativity.Import.V1.Models.Errors;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandler
    {
        void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError);

        Task HandleIAPIItemLevelErrorsAsync(ImportErrors errors);

        Task HandleRemainingErrorsAsync();
    }
}
