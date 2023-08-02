using Relativity.Import.V1.Models.Settings;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.RdoFlow
{
    internal class RdoImportConfiguration
    {
        public RdoImportConfiguration(ImportRdoSettings rdoSettings, AdvancedImportSettings advancedSettings)
        {
            RdoSettings = rdoSettings;
            AdvancedSettings = advancedSettings;
        }

        public ImportRdoSettings RdoSettings { get; }

        public AdvancedImportSettings AdvancedSettings { get; }
    }
}
