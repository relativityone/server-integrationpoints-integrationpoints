using Relativity.Import.V1.Models.Settings;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal class ImportSettings
    {
        public ImportSettings(ImportDocumentSettings documentSettings, AdvancedImportSettings advancedSettings)
        {
            DocumentSettings = documentSettings;
            AdvancedSettings = advancedSettings;
        }

        public ImportDocumentSettings DocumentSettings { get; }

        public AdvancedImportSettings AdvancedSettings { get; }
    }
}
