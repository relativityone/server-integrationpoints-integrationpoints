using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal class ImportSettingsDto
	{
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		// Default values
		public int ArtifactTypeId => (int)ArtifactType.Document;
		public kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel => kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
		public int MaximumErrorCount => int.MaxValue - 1;   // Must be this because of how ImportAPI validation works
		public char MultiValueDelimiter => (char)_ASCII_RECORD_SEPARATOR;
		public char NestedValueDelimiter => (char)_ASCII_GROUP_SEPARATOR;

		// Base values
		public int CaseArtifactId { get; set; }
		public bool Billable => CopyFilesToDocumentRepository;
		public bool CopyFilesToDocumentRepository { get; set; }
		public bool? DisableExtractedTextEncodingCheck => null;
		public bool DisableUserSecurityCheck => false;
		public string ErrorFilePath { get; set; }
		public bool ExtractedTextFieldContainsFilePath { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public int IdentityFieldId { get; set; }
		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }
		public string ParentObjectIdSourceFieldName { get; set; }
		public bool SendEmailOnLoadCompletion => false;

		// Configured values
		public string BulkLoadFileFieldDelimiter => string.Empty;
		public bool DisableControlNumberCompatibilityMode => false;
		public bool DisableExtractedTextFileLocationValidation => false;
		public bool? DisableNativeLocationValidation { get; set; }
		public bool? DisableNativeValidation { get; set; }
		public string FileNameColumn { get; set; }
		public string FileSizeColumn { get; set; }
		public bool FileSizeMapped { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public bool LoadImportedFullTextFromServer { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public string OiFileIdColumnName => string.Empty;
		public bool OiFileIdMapped { get; set; }
		public string OiFileTypeColumnName { get; set; }
		public string SupportedByViewerColumn { get; set; }

		// Extended configurations
		public int DestinationFolderArtifactId { get; set; }
		public string ExtractedTextFileEncoding { get; set; }
		public string LongTextColumnThatContainsPathToFullText => string.Empty;

		/// <summary>
		/// In Overlay Mode it allows to switch yes/no if ImportAPI should move documents between folders when use folder path information
		/// </summary>
		public bool MoveExistingDocuments { get; set; }
		public bool MoveDocumentsInAnyOverlayMode => ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
			MoveExistingDocuments && !string.IsNullOrEmpty(FolderPathSourceFieldName);

		public ImportSettingsDto()
		{
			CopyFilesToDocumentRepository = false;
			ErrorFilePath = string.Empty;
			ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles;
			ImportOverwriteMode = ImportOverwriteMode.AppendOnly;

			DisableNativeLocationValidation = null;
			DisableNativeValidation = null;
			ExtractedTextFileEncoding = string.Empty;
			FileNameColumn = string.Empty;
			FileSizeColumn = string.Empty;
			FileSizeMapped = false;
			NativeFilePathSourceFieldName = string.Empty;
			OiFileIdMapped = false;
			OiFileTypeColumnName = string.Empty;
			SupportedByViewerColumn = string.Empty;
		}
	}
}