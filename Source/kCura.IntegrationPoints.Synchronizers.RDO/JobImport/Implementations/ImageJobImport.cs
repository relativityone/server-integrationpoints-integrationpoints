using System.Collections;
using System.Data;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public class ImageJobImport : JobImport<ImageImportBulkArtifactJob>
    {
        private readonly IAPILog _logger;
        private readonly IImportSettingsBaseBuilder<ImageSettings> _builder;
        private readonly IDataReader _sourceData;

        public IDataTransferContext Context { get; set; }

        public ImageJobImport(ImportSettings importSettings, IImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataTransferContext context, IHelper helper) :
            base(importSettings, importApi, helper.GetLoggerFactory().GetLogger().ForContext<ImageJobImport>())
        {
            Context = context;
            _builder = builder;
            _sourceData = context.DataReader;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ImageJobImport>();
        }

        public override void RegisterEventHandlers()
        {
            ImportJob.OnMessage += OnMessageEventHandler;
            ImportJob.OnError += OnErrorEventHandler;
        }

        private void OnErrorEventHandler(IDictionary row)
        {
            OnError?.Invoke(row);
        }

        private void OnMessageEventHandler(Status status)
        {
            OnMessage?.Invoke(status);
        }

        public override event OnErrorEventHandler OnError;

        public override event OnMessageEventHandler OnMessage;

        protected internal override ImageImportBulkArtifactJob CreateJob()
        {
            return ImportApi.NewImageImportJob();
        }

        public override void Execute()
        {
            _logger.LogInformation("Start preparing Image Import API process");
            PrepareJob();

            _logger.LogInformation("Start Image Import API process");
            ImportJob.Execute();
            _logger.LogInformation("Image Import API process finished");

            ExportErrorFile();
        }

        private void PrepareJob()
        {
            _builder.PopulateFrom(ImportSettings, ImportJob.Settings);
            LogJobSettings();
            _logger.LogInformation("Building data table");
            ImportJob.SourceData.Reader = _sourceData;
            _logger.LogInformation("Building data table finished");
            Context.UpdateTransferStatus();
        }

        private void LogJobSettings()
        {
            if (ImportSettings != null)
            {
                var importSettingsForLogging = new ImportSettingsForLogging(ImportSettings);
                _logger.LogInformation("Import API image import settings: {importApiSettings}", importSettingsForLogging);
            }
        }
    }
}
