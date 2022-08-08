using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
    public class ObjectJobImport : JobImport<ImportBulkArtifactJob>
    {
        private readonly ImportSettings _importSettings;
        private readonly IExtendedImportAPI _importApi;
        private readonly IImportSettingsBaseBuilder<Settings> _builder;
        private readonly IDataReader _sourceData;

        public ObjectJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<Settings> builder, IDataReader sourceData)
        {
            _importSettings = importSettings;
            _importApi = importApi;
            _builder = builder;
            _sourceData = sourceData;
        }

        public Settings JobSettings
        {
            get
            {
                return ImportJob.Settings;
            }
        }

        protected override ImportBulkArtifactJob CreateJob()
        {
            return _importApi.NewObjectImportJob(_importSettings.ArtifactTypeId);
        }

        public override void Execute()
        {
            _builder.PopulateFrom(_importSettings, ImportJob.Settings);
            ImportJob.SourceData.SourceData = _sourceData;
            ImportJob.Execute();
        }
    }
}
