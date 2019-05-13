using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobFactory : IImportJobFactory
	{
		private readonly IImportAPI _importApi;
		private readonly IDataReader _dataReader;
		private readonly IBatchProgressHandlerFactory _batchProgressHandlerFactory;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJobFactory(IImportAPI importApi, IDataReader dataReader, IBatchProgressHandlerFactory batchProgressHandlerFactory, 
			IJobHistoryErrorRepository jobHistoryErrorRepository, ISemaphoreSlim semaphoreSlim, ISyncLog logger)
		{
			_importApi = importApi;
			_dataReader = dataReader;
			_batchProgressHandlerFactory = batchProgressHandlerFactory;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_semaphoreSlim = semaphoreSlim;
			_logger = logger;
		}

		public IImportJob CreateImportJob(ISynchronizationConfiguration configuration, IBatch batch)
		{
			ImportBulkArtifactJob importBulkArtifactJob = CreateImportJob(configuration, batch.StartingIndex);
			ImportBulkArtifactJobWrapper importBulkArtifactJobWrapper = new ImportBulkArtifactJobWrapper(importBulkArtifactJob);

			_batchProgressHandlerFactory.CreateBatchProgressHandler(batch, importBulkArtifactJob);

			return new ImportJob(importBulkArtifactJobWrapper, _semaphoreSlim, _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryTagArtifactId, _logger);
		}

		// TODO !!!
		private ImportBulkArtifactJob CreateImportJob(ISynchronizationConfiguration configuration, int startingIndex)
		{
			ImportBulkArtifactJob importJob = _importApi.NewNativeDocumentImportJob();
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
			importJob.Settings.DestinationFolderArtifactID = GetDestinationFolderArtifactId(
				configuration.ImportSettings.CaseArtifactId, configuration.ImportSettings.ArtifactTypeId, configuration.ImportSettings.DestinationFolderArtifactId);
			importJob.Settings.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(
				configuration.ImportSettings.CaseArtifactId, configuration.ImportSettings.ArtifactTypeId, configuration.ImportSettings.IdentityFieldId);

			return importJob;
		}

		private static Encoding GetTextEncoding(string textEncoding)
		{
			Encoding encoding = Encoding.GetEncoding(textEncoding);
			return encoding;
		}

		private int GetDestinationFolderArtifactId(int workspaceArtifactId, int artifactTypeId, int destinationFolderArtifactId)
		{
			IEnumerable<Workspace> workspaces = _importApi.Workspaces();
			Workspace currentWorkspace = workspaces.First(x => x.ArtifactID == workspaceArtifactId);
			int folderArtifactId = destinationFolderArtifactId;
			if (currentWorkspace != null && folderArtifactId == 0)
			{
				folderArtifactId = artifactTypeId == (int)ArtifactType.Document ? currentWorkspace.RootFolderID : currentWorkspace.RootArtifactID;
			}
			return folderArtifactId;
		}

		private string GetSelectedIdentifierFieldName(int workspaceArtifactId, int artifactTypeId, int identityFieldArtifactId)
		{
			IEnumerable<Field> workspaceFields = _importApi.GetWorkspaceFields(workspaceArtifactId, artifactTypeId);
			Field identityField = workspaceFields.First(x => x.ArtifactID == identityFieldArtifactId);
			return identityField.Name;
		}
	}
}