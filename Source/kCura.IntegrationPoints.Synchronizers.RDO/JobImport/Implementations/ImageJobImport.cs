using System.Collections;
using System.Data;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImageJobImport : JobImport<ImageImportBulkArtifactJob>
	{
		private readonly IImportSettingsBaseBuilder<ImageSettings> _builder;
		private readonly IDataReader _sourceData;
		protected readonly ImportSettings ImportSettings;
		protected readonly IExtendedImportAPI ImportApi;
		public IDataTransferContext Context { get; set; }

		public ImageJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataTransferContext context)
		{
			Context = context;
			ImportSettings = importSettings;
			ImportApi = importApi;
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

		protected internal override ImageImportBulkArtifactJob CreateJob()
		{
			return ImportApi.NewImageImportJob();
		}

		public override void Execute()
		{
			_builder.PopulateFrom(ImportSettings, ImportJob.Settings);
			ImportJob.SourceData.SourceData = ImageDataTableHelper.GetDataTable(_sourceData);
			Context.UpdateTransferStatus();
			ImportJob.Execute();

			if (! string.IsNullOrEmpty(ImportSettings.ErrorFilePath))
			{
				ImportJob.ExportErrorFile(ImportSettings.ErrorFilePath);
			}
		}
	}
}
