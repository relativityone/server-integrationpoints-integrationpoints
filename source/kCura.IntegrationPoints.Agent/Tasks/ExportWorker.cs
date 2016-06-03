using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportWorker : SyncWorker
	{
		private readonly ExportProcessRunner _exportProcessRunner;

		public ExportWorker(ICaseServiceContext caseServiceContext, IHelper helper,
			IDataProviderFactory dataProviderFactory, ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory, IJobHistoryService jobHistoryService,
			JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IJobManager jobManager, IEnumerable<IBatchStatus> statuses,
			JobStatisticsService statisticsService, ExportProcessRunner exportProcessRunner)
			: base(
				caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService, jobHistoryErrorServiceProvider.JobHistoryErrorService, jobManager, statuses, statisticsService)
		{
			_exportProcessRunner = exportProcessRunner;
		}

		protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo,
			string configuration, Job job)
		{
			var providerGuid = new Guid(destinationProviderRdo.Identifier);

			var sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		internal override void ExecuteImport(IEnumerable<FieldMap> fieldMap, string sourceConfiguration,
			string destinationConfiguration, List<string> entryIDs,
			SourceProvider sourceProviderRdo, DestinationProvider destinationProvider, Job job)
		{
			var sourceSettings = JsonConvert.DeserializeObject<ExportUsingSavedSearchSettings>(sourceConfiguration);

			var destinationSettings = JsonConvert.DeserializeObject<ImportSettings>(destinationConfiguration);

			var imageType = default(ExportSettings.ImageFileType);
			Enum.TryParse(sourceSettings.SelectedImageFileType, true, out imageType);
			ExportSettings.DataFileFormat dataFileFormat;
			Enum.TryParse(sourceSettings.SelectedDataFileFormat, true, out dataFileFormat);

			var exportSettings = new ExportSettings
			{
				ExportedObjArtifactId = sourceSettings.SavedSearchArtifactId,
				ExportedObjName = sourceSettings.SavedSearch,
				ExportImages = sourceSettings.ExportImagesChecked,
				ImageType = imageType,
				WorkspaceId = sourceSettings.SourceWorkspaceArtifactId,
				ExportFilesLocation = sourceSettings.Fileshare,
				OverwriteFiles = sourceSettings.OverwriteFiles,
				CopyFileFromRepository = sourceSettings.CopyFileFromRepository,
				SelViewFieldIds = fieldMap.Select(item => int.Parse(item.SourceField.FieldIdentifier)).ToList(),
				ArtifactTypeId = destinationSettings.ArtifactTypeId,
				OutputDataFileFormat = dataFileFormat,
				IncludeNativeFilesPath = sourceSettings.IncludeNativeFilesPath,
				DataFileEncoding = Encoding.GetEncoding(sourceSettings.DataFileEncodingType)
			};

			_exportProcessRunner.StartWith(exportSettings);
		}
	}
}