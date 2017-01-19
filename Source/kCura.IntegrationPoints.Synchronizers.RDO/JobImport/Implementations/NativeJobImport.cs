using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using System.Collections;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class NativeJobImport : JobImport<ImportBulkArtifactJob>
	{
		private readonly ImportSettings _importSettings;
		private readonly IExtendedImportAPI _importApi;
		private readonly IImportSettingsBaseBuilder<Settings> _builder;
		private readonly IDataReader _sourceData;

		public NativeJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<Settings> builder, IDataReader sourceData)
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

		protected override ImportBulkArtifactJob CreateJob()
		{
			if (_importSettings.ArtifactTypeId == (int) ArtifactType.Document)
			{
				if (_importSettings.FederatedInstanceArtifactId == null)
					_importApi.NewNativeDocumentImportJob(_importSettings.OnBehalfOfUserToken);
				else
					_importApi.NewNativeDocumentImportJob();
			}
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
