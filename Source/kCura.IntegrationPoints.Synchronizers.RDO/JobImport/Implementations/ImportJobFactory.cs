﻿using System;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
	public class ImportJobFactory : IImportJobFactory
	{
		private readonly IMessageService _messageService;

		public ImportJobFactory(IMessageService messageService)
		{
			_messageService = messageService;
		}

		public IJobImport Create(IExtendedImportAPI importApi, ImportSettings settings, IDataTransferContext context, IHelper helper)
		{
			IJobImport rv;
			switch (GetJobContextType(settings))
			{
				case JobContextType.RelativityToRelativityImagesProduction:
					IImportSettingsBaseBuilder<ImageSettings> imageProductionRelativityToRelativityImportSettingsBuilder = new ImageRelativityToRelativityImportSettingsBuilder(importApi);
					rv = new ProductionImageJobImport(settings, importApi, imageProductionRelativityToRelativityImportSettingsBuilder, context, helper);
					break;
				case JobContextType.RelativityToRelativityImages:
					IImportSettingsBaseBuilder<ImageSettings> imageRelativityToRelativityImportSettingsBuilder = new ImageRelativityToRelativityImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageRelativityToRelativityImportSettingsBuilder, context, helper);
					break;
				case JobContextType.ImportImagesFromLoadFile:
					IImportSettingsBaseBuilder<ImageSettings> imageImportSettingsBuilder = new ImageImportSettingsBuilder(importApi);
					rv = new ImageJobImport(settings, importApi, imageImportSettingsBuilder, context, helper);
					break;
				case JobContextType.Native:
					IImportSettingsBaseBuilder<Settings> nativeImportSettingsBuilder = new NativeImportSettingsBuilder(importApi);
					rv = new NativeJobImport(settings, importApi, nativeImportSettingsBuilder, context, helper);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			rv.RegisterEventHandlers();
			rv.OnComplete += report => OnJobComplete(report, settings.CaseArtifactId, settings.CorrelationId, settings.Provider, settings.JobID);
			return rv;
		}

		private void OnJobComplete(JobReport jobReport, int workspaceId, Guid correlationId, string provider, long? jobID)
		{
			if (!IsValidProviderName(provider)) // filter out jobs without provider name (for example, Tagger that is being run within push job)
			{
				return;
			}

			long jobSizeInBytes = jobReport.FileBytes + jobReport.MetadataBytes;
			TimeSpan jobDuration = jobReport.EndTime - jobReport.StartTime;
			double bytesPerSecond = jobDuration.TotalSeconds > 0 ? jobSizeInBytes / jobDuration.TotalSeconds : 0;

			_messageService.Send(new ImportJobStatisticsMessage()
			{
				Provider = provider,
				JobID = jobID?.ToString() ?? "",
				FileBytes = jobReport.FileBytes,
				MetaBytes = jobReport.MetadataBytes,
				JobSizeInBytes = jobSizeInBytes,
				CorellationID = correlationId.ToString(),
				WorkspaceID = workspaceId,
				UnitOfMeasure = "Bytes(s)"
			});

			_messageService.Send(new ImportJobThroughputBytesMessage()
			{
				Provider = provider,
				BytesPerSecond = bytesPerSecond
			});
		}

		private bool IsValidProviderName(string provider)
		{
			return !string.IsNullOrWhiteSpace(provider);
		}

		internal enum JobContextType
		{
			RelativityToRelativityImages,
			RelativityToRelativityImagesProduction,
			ImportImagesFromLoadFile,
			Native
		}

		internal static JobContextType GetJobContextType(ImportSettings settings)
		{
			const string relativity = "relativity";
			if (relativity == settings.Provider && settings.ProductionImport && settings.ImageImport)
			{
				return JobContextType.RelativityToRelativityImagesProduction;
			}
			else if (relativity == settings.Provider && settings.ImageImport)
			{
				return JobContextType.RelativityToRelativityImages;
			}
			else if (settings.ImageImport)
			{
				return JobContextType.ImportImagesFromLoadFile;
			}
			else
			{
				return JobContextType.Native;
			}
		}
	}
}
