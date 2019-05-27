using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobFactory : IImportJobFactory
	{
		private readonly IImportAPI _importApi;
		private readonly ISourceWorkspaceDataReader _dataReader;
		private readonly IBatchProgressHandlerFactory _batchProgressHandlerFactory;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;

		public ImportJobFactory(IImportAPI importApi, ISourceWorkspaceDataReader dataReader, IBatchProgressHandlerFactory batchProgressHandlerFactory, 
			IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_importApi = importApi;
			_dataReader = dataReader;
			_batchProgressHandlerFactory = batchProgressHandlerFactory;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
		}

		public async Task<IImportJob> CreateImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch)
		{
			ImportBulkArtifactJob importBulkArtifactJob = await CreateImportBulkArtifactJobAsync(configuration, batch.StartingIndex).ConfigureAwait(false);
			var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importBulkArtifactJob, _dataReader.ItemStatusMonitor);

			_batchProgressHandlerFactory.CreateBatchProgressHandler(batch, importBulkArtifactJob);

			return new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryTagArtifactId, _logger);
		}

		private async Task<ImportBulkArtifactJob> CreateImportBulkArtifactJobAsync(ISynchronizationConfiguration configuration, int startingIndex)
		{
			ImportBulkArtifactJob importJob = await Task.Run(() => _importApi.NewNativeDocumentImportJob()).ConfigureAwait(false);
			importJob.SourceData.SourceData = _dataReader;

			// Default values
			importJob.Settings.ArtifactTypeId = configuration.ImportSettings.ArtifactTypeId;
			importJob.Settings.AuditLevel = configuration.ImportSettings.AuditLevel;
			importJob.Settings.MaximumErrorCount = configuration.ImportSettings.MaximumErrorCount;
			importJob.Settings.MultiValueDelimiter = configuration.ImportSettings.MultiValueDelimiter;
			importJob.Settings.NestedValueDelimiter = configuration.ImportSettings.NestedValueDelimiter;

			// Base values
			importJob.Settings.CaseArtifactId = configuration.ImportSettings.CaseArtifactId;

			importJob.Settings.Billable = configuration.ImportSettings.Billable;
			importJob.Settings.CopyFilesToDocumentRepository = configuration.ImportSettings.CopyFilesToDocumentRepository;

			importJob.Settings.DisableExtractedTextEncodingCheck = configuration.ImportSettings.DisableExtractedTextEncodingCheck;
			importJob.Settings.DisableUserSecurityCheck = configuration.ImportSettings.DisableUserSecurityCheck;
			importJob.Settings.ExtractedTextFieldContainsFilePath = configuration.ImportSettings.ExtractedTextFieldContainsFilePath;
			importJob.Settings.IdentityFieldId = configuration.ImportSettings.IdentityFieldId;
			importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportSettings.ImportNativeFileCopyMode;
			importJob.Settings.ObjectFieldIdListContainsArtifactId = configuration.ImportSettings.ObjectFieldIdListContainsArtifactId;
			importJob.Settings.OverlayBehavior = (OverlayBehavior)configuration.ImportSettings.FieldOverlayBehavior;
			importJob.Settings.OverwriteMode = (OverwriteModeEnum)configuration.ImportSettings.ImportOverwriteMode;
			importJob.Settings.ParentObjectIdSourceFieldName = configuration.ImportSettings.ParentObjectIdSourceFieldName;
			importJob.Settings.SendEmailOnLoadCompletion = configuration.ImportSettings.SendEmailOnLoadCompletion;
			importJob.Settings.StartRecordNumber = startingIndex;

			// Configured values
			importJob.Settings.BulkLoadFileFieldDelimiter = configuration.ImportSettings.BulkLoadFileFieldDelimiter;
			importJob.Settings.DestinationFolderArtifactID = configuration.ImportSettings.DestinationFolderArtifactId;
			importJob.Settings.DisableControlNumberCompatibilityMode = configuration.ImportSettings.DisableControlNumberCompatibilityMode;
			importJob.Settings.DisableExtractedTextFileLocationValidation = configuration.ImportSettings.DisableExtractedTextFileLocationValidation;
			importJob.Settings.DisableNativeLocationValidation = configuration.ImportSettings.DisableNativeLocationValidation;
			importJob.Settings.DisableNativeValidation = configuration.ImportSettings.DisableNativeValidation;
			importJob.Settings.FileNameColumn = configuration.ImportSettings.FileNameColumn;
			importJob.Settings.FileSizeColumn = configuration.ImportSettings.FileSizeColumn;
			importJob.Settings.FileSizeMapped = configuration.ImportSettings.FileSizeMapped;
			importJob.Settings.FolderPathSourceFieldName = configuration.ImportSettings.FolderPathSourceFieldName;
			importJob.Settings.LoadImportedFullTextFromServer = configuration.ImportSettings.LoadImportedFullTextFromServer;
			importJob.Settings.MoveDocumentsInAppendOverlayMode = configuration.ImportSettings.MoveDocumentsInAnyOverlayMode;
			importJob.Settings.NativeFilePathSourceFieldName = configuration.ImportSettings.NativeFilePathSourceFieldName;
			importJob.Settings.OIFileIdColumnName = configuration.ImportSettings.OiFileIdColumnName;
			importJob.Settings.OIFileIdMapped = configuration.ImportSettings.OiFileIdMapped;
			importJob.Settings.OIFileTypeColumnName = configuration.ImportSettings.OiFileTypeColumnName;
			importJob.Settings.SupportedByViewerColumn = configuration.ImportSettings.SupportedByViewerColumn;

			// Extended configurations
			if (importJob.Settings.ExtractedTextFieldContainsFilePath)
			{
				importJob.Settings.ExtractedTextEncoding = GetTextEncoding(configuration.ImportSettings.ExtractedTextFileEncoding);
				importJob.Settings.LongTextColumnThatContainsPathToFullText = configuration.ImportSettings.LongTextColumnThatContainsPathToFullText;
			}

			importJob.Settings.SelectedIdentifierFieldName = await GetSelectedIdentifierFieldNameAsync(
				configuration.ImportSettings.CaseArtifactId, configuration.ImportSettings.ArtifactTypeId, configuration.ImportSettings.IdentityFieldId).ConfigureAwait(false);

			return importJob;
		}

		private static Encoding GetTextEncoding(string textEncoding)
		{
			Encoding encoding = Encoding.GetEncoding(textEncoding);
			return encoding;
		}

		private async Task<string> GetSelectedIdentifierFieldNameAsync(int workspaceArtifactId, int artifactTypeId, int identityFieldArtifactId)
		{
			IEnumerable<Field> workspaceFields = await Task.Run(() => _importApi.GetWorkspaceFields(workspaceArtifactId, artifactTypeId)).ConfigureAwait(false);
			Field identityField = workspaceFields.First(x => x.ArtifactID == identityFieldArtifactId);
			return identityField.Name;
		}
	}
}