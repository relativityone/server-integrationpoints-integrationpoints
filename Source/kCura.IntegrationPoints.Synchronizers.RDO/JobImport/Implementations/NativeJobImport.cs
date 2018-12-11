﻿using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.Client;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using System.Collections;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
	public class NativeJobImport : JobImport<ImportBulkArtifactJob>
	{
		private readonly IImportSettingsBaseBuilder<Settings> _builder;
		private readonly IDataReader _sourceData;
		private readonly IAPILog _logger;

		public NativeJobImport(ImportSettings importSettings, IExtendedImportAPI importApi, IImportSettingsBaseBuilder<Settings> builder, IDataTransferContext context, IHelper helper) :
			base(importSettings, importApi, helper.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>())
		{
			_builder = builder;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<NativeJobImport>();
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
			int artifactTypeId = ImportSettings.ArtifactTypeId;
			int? federatedInstanceArtifactId = ImportSettings.FederatedInstanceArtifactId;
			_logger.LogInformation("Creating Import Job. ArtifactTypeId: {artifactTypeId}, FederatedInstanceArtifactId: {federatedInstanceArtifactId}", artifactTypeId, federatedInstanceArtifactId);

			if (artifactTypeId == (int)ArtifactType.Document)
			{
				if (federatedInstanceArtifactId == null)
				{
					return ImportApi.NewNativeDocumentImportJob();
				}
				return ImportApi.NewNativeDocumentImportJob();
			}
			return ImportApi.NewObjectImportJob(artifactTypeId);
		}

		public override void Execute()
		{
			_logger.LogDebug("Start preparing Native Import API process");
			PrepareImportJob();

			_logger.LogInformation("Start Native Import API process");
			ImportJob.Execute();
			_logger.LogInformation("Native Import API process finished");

			ExportErrorFile();
		}

		private void PrepareImportJob()
		{
			_builder.PopulateFrom(ImportSettings, ImportJob.Settings);
			LogJobSettings();

			ImportJob.SourceData.SourceData = _sourceData;
		}

		private void LogJobSettings()
		{
			if (ImportJob.Settings != null)
			{
				string importApiSettings = JsonConvert.SerializeObject(ImportJob.Settings);
				_logger.LogInformation("Import API native import settings: {importApiSettings}", importApiSettings);
			}
		}
	}
}
