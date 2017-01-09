using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImageJobImport : JobImport<ImageImportBulkArtifactJob>
	{
		private readonly ImportSettings _importSettings;
		private readonly IExtendedImportAPI _importApi;
		private readonly IImportSettingsBaseBuilder<ImageSettings> _builder;
		private readonly IDataReader _sourceData;

		public ImageJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<ImageSettings> builder, IDataReader sourceData)
		{
			_importSettings = importSettings;
			_importApi = importApi;
			_builder = builder;
			_sourceData = sourceData;
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
			//TODO: decide whether to call new production image job based on settings
			return _importApi.NewImageImportJob();
		}

		public override void Execute()
		{
			_builder.PopulateFrom(_importSettings, ImportJob.Settings);
			ImportJob.SourceData.SourceData = ImageDataTableHelper.GetDataTable(_sourceData);
			ImportJob.Execute();
		}
	}
}
