using System.Threading.Tasks;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal interface IImportApiItemLevelErrorHandler
    {
        Task HandleItemLevelErrorsAsync(IImportSourceController sourceController, IBatch batch, IDocumentSynchronizationMonitorConfiguration configuration);
    }
}