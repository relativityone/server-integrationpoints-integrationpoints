using System.Collections;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Core.Helpers;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class NativeJobImport : JobImport<ImportBulkArtifactJob>
	{
		private readonly ImportSettings _importSettings;
		private readonly IExtendedImportAPI _importApi;
		private readonly IImportSettingsBaseBuilder<Settings> _builder;
		private readonly IDataReader _sourceData;
		private readonly IDataTransferContext _context;
		private readonly IAPILog _logger;

		public NativeJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<Settings> builder, IDataTransferContext context, IHelper helper)
		{
			_importSettings = importSettings;
			_importApi = importApi;
			_builder = builder;
			_context = context;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>(); ;
			_sourceData = context.DataReader;
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

		protected internal override ImportBulkArtifactJob CreateJob()
		{
			if (_importSettings.ArtifactTypeId == (int) ArtifactType.Document)
			{
				if (_importSettings.FederatedInstanceArtifactId == null)
					return _importApi.NewNativeDocumentImportJob(_importSettings.OnBehalfOfUserToken);
				return _importApi.NewNativeDocumentImportJob();
			}
			return _importApi.NewObjectImportJob(_importSettings.ArtifactTypeId);
		}

		public override void Execute()
		{
			_builder.PopulateFrom(_importSettings, ImportJob.Settings);

			if (ImportJob.Settings != null)
			{
				string importApiSettings = JsonConvert.SerializeObject(ImportJob.Settings);

				_logger.LogDebug($"Import API settings: {importApiSettings}");
			}

			ImportJob.SourceData.SourceData = _sourceData;

			_logger.LogInformation("Start Import API process");

			ImportJob.Execute();

			_logger.LogInformation("Import API process finished");

			if (! string.IsNullOrEmpty(_importSettings.ErrorFilePath))
			{
				_logger.LogError("Import API process return errors");

				ImportJob.ExportErrorFile(_importSettings.ErrorFilePath);
			}
		}
	}
}
