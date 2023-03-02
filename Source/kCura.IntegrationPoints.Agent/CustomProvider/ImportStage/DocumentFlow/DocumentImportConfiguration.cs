using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class DocumentImportConfiguration
    {
        public DocumentImportConfiguration(ImportDocumentSettings documentSettings, AdvancedImportSettings advancedSettings)
        {
            DocumentSettings = documentSettings;
            AdvancedSettings = advancedSettings;
        }

        public ImportDocumentSettings DocumentSettings { get; }

        public AdvancedImportSettings AdvancedSettings { get; }
    }
}
