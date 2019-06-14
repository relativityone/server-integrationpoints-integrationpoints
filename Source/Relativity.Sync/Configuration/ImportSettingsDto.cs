using System;
using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	/// <summary>
	/// Holds configuration properties for ImportAPI.
	/// </summary>
	public class ImportSettingsDto
	{
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		// Default values
		/// <summary>
		/// Artifact type ID of the object to transfer.
		/// </summary>
		public int ArtifactTypeId => (int)ArtifactType.Document;

		/// <summary>
		/// Audit level. Defaults to <value>ImportAuditLevel.FullAudit</value>
		/// </summary>
		public kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel => kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;

		/// <summary>
		/// Maximum error count.
		/// </summary>
		public int MaximumErrorCount => int.MaxValue - 1;   // Must be this because of how ImportAPI validation works

		/// <summary>
		/// Delimiter character for multi values.
		/// </summary>
		public char MultiValueDelimiter { get; set; } = (char)_ASCII_RECORD_SEPARATOR;

		/// <summary>
		/// Delimiter character for nested values.
		/// </summary>
		public char NestedValueDelimiter { get; set; } = (char)_ASCII_GROUP_SEPARATOR;

		// Base values
		/// <summary>
		/// Destination workspace artifact ID.
		/// </summary>
		public int CaseArtifactId { get; set; }

		/// <summary>
		/// Billable flag. Default value is same as CopyFilesToDocumentRepository.
		/// </summary>
		public bool Billable => CopyFilesToDocumentRepository;

		/// <summary>
		/// Determines whether to copy files to document repository.
		/// </summary>
		public bool CopyFilesToDocumentRepository { get; set; }

		/// <summary>
		/// Whether to disable extracted text encoding check.
		/// </summary>
		public bool? DisableExtractedTextEncodingCheck => null;

		/// <summary>
		/// Whether to disable user security check. Defaults to false.
		/// </summary>
		public bool DisableUserSecurityCheck => false;

		/// <summary>
		/// Error file path.
		/// </summary>
		public string ErrorFilePath { get; set; }

		/// <summary>
		/// Determines whether extracted text field contains file path.
		/// </summary>
		public bool ExtractedTextFieldContainsFilePath { get; set; }

		/// <summary>
		/// The field overlay behavior.
		/// </summary>
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }

		/// <summary>
		/// Artifact ID of the identity field.
		/// </summary>
		public int IdentityFieldId { get; set; }

		/// <summary>
		/// Import natives mode.
		/// </summary>
		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }

		/// <summary>
		/// Import overwrite mode.
		/// </summary>
		public ImportOverwriteMode ImportOverwriteMode { get; set; }

		/// <summary>
		/// List of object field IDs.
		/// </summary>
		public IList<int> ObjectFieldIdListContainsArtifactId { get; } = new List<int>();

		/// <summary>
		/// Field name of the parent object ID.
		/// </summary>
		public string ParentObjectIdSourceFieldName { get; set; }

		/// <summary>
		/// Whether to send email on load completion. Defaults to false.
		/// </summary>
		public bool SendEmailOnLoadCompletion => false;

		// Configured values

		/// <summary>
		/// Bulk load file fields delimiter.
		/// </summary>
		public string BulkLoadFileFieldDelimiter => string.Empty;

		/// <summary>
		/// Whether to disable control number compatibility mode. Defaults to false.
		/// </summary>
		public bool DisableControlNumberCompatibilityMode => false;

		/// <summary>
		/// Whether to disable extracted text file location validation. Defaults to false.
		/// </summary>
		public bool DisableExtractedTextFileLocationValidation => false;

		/// <summary>
		/// Whether to disable native location validation.
		/// </summary>
		public bool? DisableNativeLocationValidation { get; set; }

		/// <summary>
		/// Whether to disable native files validation.
		/// </summary>
		public bool? DisableNativeValidation { get; set; }

		/// <summary>
		/// Specifies column name for file name.
		/// </summary>
		public string FileNameColumn { get; set; }

		/// <summary>
		/// Specifies column name for file size.
		/// </summary>
		public string FileSizeColumn { get; set; }

		/// <summary>
		/// Determines if file size has been mapped.
		/// </summary>
		public bool FileSizeMapped { get; set; }

		/// <summary>
		/// Specifies field name for folder path source.
		/// </summary>
		public string FolderPathSourceFieldName { get; set; }

		/// <summary>
		/// Whether to load imported full text from server.
		/// </summary>
		public bool LoadImportedFullTextFromServer { get; set; }

		/// <summary>
		/// Field name for native file path in the source.
		/// </summary>
		public string NativeFilePathSourceFieldName { get; set; }

		/// <summary>
		/// Column name of outside-in file ID.
		/// </summary>
		public string OiFileIdColumnName => string.Empty;

		/// <summary>
		/// Determines if outside-in file ID has been mapped.
		/// </summary>
		public bool OiFileIdMapped { get; set; }

		/// <summary>
		/// Column name of the file type.
		/// </summary>
		public string OiFileTypeColumnName { get; set; }

		/// <summary>
		/// Specifies the column name for supported by viewer field.
		/// </summary>
		public string SupportedByViewerColumn { get; set; }

		// Extended configurations
		/// <summary>
		/// Artifact ID of the destination folder.
		/// </summary>
		public int DestinationFolderArtifactId { get; set; }

		/// <summary>
		/// Specifies file encoding for extracted texts.
		/// </summary>
		public string ExtractedTextFileEncoding { get; set; }

		/// <summary>
		/// Column name that contains path to full text file. Defaults to empty string.
		/// </summary>
		public string LongTextColumnThatContainsPathToFullText => string.Empty;

		/// <summary>
		/// In Overlay Mode it allows to switch yes/no if ImportAPI should move documents between folders when use folder path information
		/// </summary>
		public bool MoveExistingDocuments { get; set; }

		/// <summary>
		/// Determines whether to move documents in any overlay mode.
		/// </summary>
		public bool MoveDocumentsInAnyOverlayMode => ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
			MoveExistingDocuments && !string.IsNullOrEmpty(FolderPathSourceFieldName);

		/// <summary>
		/// Relativity Web Service URL.
		/// </summary>
		public Uri RelativityWebServiceUrl { get; set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ImportSettingsDto()
		{
			ErrorFilePath = string.Empty;
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