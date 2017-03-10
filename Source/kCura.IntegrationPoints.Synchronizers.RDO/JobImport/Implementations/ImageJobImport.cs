using System.Collections;
using System.Data;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImageJobImport : JobImport<ImageImportBulkArtifactJob>
	{
		private readonly ImportSettings _importSettings;
		private readonly IExtendedImportAPI _importApi;
		private readonly IImportSettingsBaseBuilder<ImageSettings> _builder;
		private readonly IDataReader _sourceData;
		public IDataTransferContext Context { get; set; }

		public ImageJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataTransferContext context)
		{
			Context = context;
			_importSettings = importSettings;
			_importApi = importApi;
			_builder = builder;
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

		protected override ImageImportBulkArtifactJob CreateJob()
		{
			return _importApi.NewImageImportJob();
		}

		public override void Execute()
		{
			_builder.PopulateFrom(_importSettings, ImportJob.Settings);
			ImportJob.SourceData.SourceData = ImageDataTableHelper.GetDataTable(_sourceData);
			Context.UpdateTransferStatus();
			ImportJob.Execute();

			if (! string.IsNullOrEmpty(_importSettings.ErrorFilePath))
			{
				ImportJob.ExportErrorFile(_importSettings.ErrorFilePath);
			}
		}
	}
}
