using kCura.Relativity.DataReaderClient;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
    public interface IImportSettingsBaseBuilder<T> where T : ImportSettingsBase
    {
        void PopulateFrom(ImportSettings importSettings, T target);
    }
}
