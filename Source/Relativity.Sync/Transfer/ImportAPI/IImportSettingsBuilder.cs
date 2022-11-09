using System.Threading;
using System.Threading.Tasks;
using Relativity.Import.V1.Models.Settings;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal interface IImportSettingsBuilder
    {
        Task<(ImportDocumentSettings importSettings, AdvancedImportSettings advancedSettings)> BuildAsync(
            IConfigureDocumentSynchronizationConfiguration configuration,
            CancellationToken token);
    }
}
