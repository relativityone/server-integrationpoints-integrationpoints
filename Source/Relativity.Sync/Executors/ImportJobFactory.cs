using System.Collections.Generic;
using System.Data;
using System.Text;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobFactory : IImportJobFactory
	{
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		private readonly IImportAPI _importApi;
		private readonly IDataReader _dataReader;
		private readonly IBatchProgressHandlerFactory _batchProgressHandlerFactory;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;
		
		private readonly Dictionary<FieldOverlayBehavior, OverlayBehavior> _overlayBehaviorDictionary = new Dictionary<FieldOverlayBehavior, OverlayBehavior>()
		{
			{ FieldOverlayBehavior.MergeValues, OverlayBehavior.MergeAll },
			{ FieldOverlayBehavior.ReplaceValues, OverlayBehavior.ReplaceAll },
			{ FieldOverlayBehavior.UseFieldSettings, OverlayBehavior.UseRelativityDefaults }
		};

		private readonly Dictionary<ImportOverwriteMode, OverwriteModeEnum> _overwriteModeDictionary = new Dictionary<ImportOverwriteMode, OverwriteModeEnum>()
		{
			{ ImportOverwriteMode.AppendOnly, OverwriteModeEnum.Append },
			{ ImportOverwriteMode.AppendOverlay, OverwriteModeEnum.AppendOverlay },
			{ ImportOverwriteMode.OverlayOnly, OverwriteModeEnum.Overlay }
		};

		public ImportJobFactory(IImportAPI importApi, IDataReader dataReader, IBatchProgressHandlerFactory batchProgressHandlerFactory, ISemaphoreSlim semaphoreSlim, ISyncLog logger)
		{
			_importApi = importApi;
			_dataReader = dataReader;
			_batchProgressHandlerFactory = batchProgressHandlerFactory;
			_semaphoreSlim = semaphoreSlim;
			_logger = logger;
		}

		public IImportJob CreateImportJob(ISynchronizationConfiguration configuration, IBatch batch)
		{
			ImportBulkArtifactJob importBulkArtifactJob = CreateImportJob(configuration, batch.StartingIndex);
			ImportBulkArtifactJobWrapper importBulkArtifactJobWrapper = new ImportBulkArtifactJobWrapper(importBulkArtifactJob);
			return new ImportJob(batch, _batchProgressHandlerFactory, importBulkArtifactJobWrapper, _semaphoreSlim, _logger);
		}

		// TODO !!!
		private ImportBulkArtifactJob CreateImportJob(ISynchronizationConfiguration configuration, int startingIndex)
		{
			ImportBulkArtifactJob importJob = _importApi.NewNativeDocumentImportJob();
			importJob.SourceData.SourceData = _dataReader;
			importJob.Settings.StartRecordNumber = startingIndex;
			importJob.Settings.MultiValueDelimiter = (char)_ASCII_RECORD_SEPARATOR;
			importJob.Settings.NestedValueDelimiter = (char)_ASCII_GROUP_SEPARATOR;
			importJob.Settings.MaximumErrorCount = int.MaxValue - 1;
			importJob.Settings.SendEmailOnLoadCompletion = configuration.SendEmails;
			importJob.Settings.OverwriteMode = _overwriteModeDictionary[configuration.ImportOverwriteMode];
			importJob.Settings.OverlayBehavior = _overlayBehaviorDictionary[configuration.FieldOverlayBehavior];
			importJob.Settings.CaseArtifactId = configuration.SourceWorkspaceArtifactId; // ?
			importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles; // ?
			importJob.Settings.ArtifactTypeId = (int)ArtifactType.Document; // ?
			importJob.Settings.FileSizeMapped = true; // ?
			importJob.Settings.DisableNativeValidation = true;
			importJob.Settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;

			importJob.Settings.BulkLoadFileFieldDelimiter = "";
			importJob.Settings.NativeFilePathSourceFieldName = "";
			importJob.Settings.SupportedByViewerColumn = "";
			importJob.Settings.FileNameColumn = "";
			importJob.Settings.FileSizeColumn = "";
			importJob.Settings.FolderPathSourceFieldName = "";
			importJob.Settings.OIFileIdColumnName = "";
			importJob.Settings.OIFileTypeColumnName = "";
			importJob.Settings.OIFileIdMapped = true;
			importJob.Settings.DisableExtractedTextFileLocationValidation = true;

			// TODO
			importJob.Settings.MoveDocumentsInAppendOverlayMode = false;
			importJob.Settings.DisableNativeLocationValidation = true;
			importJob.Settings.DisableControlNumberCompatibilityMode = false;
			importJob.Settings.Billable = true;
			importJob.Settings.ParentObjectIdSourceFieldName = "";
			importJob.Settings.ObjectFieldIdListContainsArtifactId = null;
			importJob.Settings.IdentityFieldId = 0;
			importJob.Settings.ExtractedTextFieldContainsFilePath = false;
			importJob.Settings.DisableUserSecurityCheck = true;
			importJob.Settings.DisableExtractedTextFileLocationValidation = true;
			importJob.Settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
			importJob.Settings.CopyFilesToDocumentRepository = false;

			bool extractedTextFieldContainsFilePath = false;
			if (extractedTextFieldContainsFilePath)
			{
				importJob.Settings.ExtractedTextEncoding = Encoding.Unicode;
				importJob.Settings.LongTextColumnThatContainsPathToFullText = "";
			}

			importJob.Settings.LoadImportedFullTextFromServer = false;
			importJob.Settings.SelectedIdentifierFieldName = "";

			return importJob;
		}
	}
}