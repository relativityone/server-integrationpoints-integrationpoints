using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal interface IImportSettingsBuilder
    {
        Task<ImportSettings> BuildAsync(IConfigureDocumentSynchronizationConfiguration configuration, CancellationToken token);
    }
}
