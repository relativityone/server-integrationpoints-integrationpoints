using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.DataReaderClient;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class ImportSettingsForLogging
    {
        private readonly ImportSettings _settings;

        public ImportSettingsForLogging(ImportSettings settings)
        {
            _settings = settings;
        }

        public int ArtifactTypeId => _settings.ArtifactTypeId;

        public string BulkLoadFileFieldDelimiter => _settings.BulkLoadFileFieldDelimiter;

        public int CaseArtifactId => _settings.CaseArtifactId;

        public int? FederatedInstanceArtifactId => _settings.FederatedInstanceArtifactId;

        public string FederatedInstanceCredentials => RemoveIfNotEmpty(_settings.FederatedInstanceCredentials);

        public bool CopyFilesToDocumentRepository => _settings.CopyFilesToDocumentRepository;

        public bool EntityManagerFieldContainsLink => _settings.EntityManagerFieldContainsLink;

        public int DestinationFolderArtifactId => _settings.DestinationFolderArtifactId;

        public bool DisableControlNumberCompatibilityMode => _settings.DisableControlNumberCompatibilityMode;

        public bool? DisableExtractedTextEncodingCheck => _settings.DisableExtractedTextEncodingCheck;

        public bool DisableExtractedTextFileLocationValidation => _settings.DisableExtractedTextFileLocationValidation;

        public bool? DisableNativeLocationValidation => _settings.DisableNativeLocationValidation;

        public bool? DisableNativeValidation => _settings.DisableNativeValidation;

        public string DestinationProviderType => _settings.DestinationProviderType;

        public bool DisableUserSecurityCheck => _settings.DisableUserSecurityCheck;

        public string ErrorFilePath => RemoveIfNotEmpty(_settings.ErrorFilePath);

        public Encoding ExtractedTextEncoding => _settings.ExtractedTextEncoding;

        public bool ExtractedTextFieldContainsFilePath => _settings.ExtractedTextFieldContainsFilePath;

        public string ExtractedTextFileEncoding => _settings.ExtractedTextFileEncoding;

        public string FieldOverlayBehavior => _settings.FieldOverlayBehavior;

        public string FileNameColumn => RemoveIfNotEmpty(_settings.FileNameColumn);

        public string FileSizeColumn => RemoveIfNotEmpty(_settings.FileSizeColumn);

        public bool FileSizeMapped => _settings.FileSizeMapped;

        public string FolderPathSourceFieldName => RemoveIfNotEmpty(_settings.FolderPathSourceFieldName);

        public bool UseDynamicFolderPath => _settings.UseDynamicFolderPath;

        public int IdentityFieldId => _settings.IdentityFieldId;

        public ImportAuditLevelEnum ImportAuditLevel => _settings.ImportAuditLevel;

        public Guid CorrelationId => _settings.CorrelationId;

        public long? JobID => _settings.JobID;

        public bool ImportNativeFile => _settings.ImportNativeFile;

        public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode => _settings.ImportNativeFileCopyMode;

        public ImportOverlayBehaviorEnum ImportOverlayBehavior => _settings.ImportOverlayBehavior;

        public ImportOverwriteModeEnum ImportOverwriteMode => _settings.ImportOverwriteMode;

        public string LongTextColumnThatContainsPathToFullText => _settings.LongTextColumnThatContainsPathToFullText;

        public int MaximumErrorCount => _settings.MaximumErrorCount;

        public char MultiValueDelimiter => _settings.MultiValueDelimiter;

        public string NativeFilePathSourceFieldName => RemoveIfNotEmpty(_settings.NativeFilePathSourceFieldName);

        public char NestedValueDelimiter => _settings.NestedValueDelimiter;

        public IList<int> ObjectFieldIdListContainsArtifactId => _settings.ObjectFieldIdListContainsArtifactId;

        public string OIFileIdColumnName => _settings.OIFileIdColumnName;

        public bool OIFileIdMapped => _settings.OIFileIdMapped;

        public string OIFileTypeColumnName => _settings.OIFileTypeColumnName;

        public string SupportedByViewerColumn => RemoveIfNotEmpty(_settings.SupportedByViewerColumn);

        public int OnBehalfOfUserId => _settings.OnBehalfOfUserId;

        public string ParentObjectIdSourceFieldName => _settings.ParentObjectIdSourceFieldName;

        public string Provider => _settings.Provider;

        public string RelativityPassword => RemoveIfNotEmpty(_settings.RelativityPassword);

        public string RelativityUsername => RemoveIfNotEmpty(_settings.RelativityUsername);

        public bool SendEmailOnLoadCompletion => _settings.SendEmailOnLoadCompletion;

        public string SelectedCaseFileRepoPath => RemoveIfNotEmpty(_settings.SelectedCaseFileRepoPath);

        public int StartRecordNumber => _settings.StartRecordNumber;

        public string WebServiceURL => _settings.WebServiceURL;

        public bool AutoNumberImages => _settings.AutoNumberImages;

        public bool ProductionImport => _settings.ProductionImport;

        public bool ImageImport => _settings.ImageImport;

        public string IdentifierField => _settings.IdentifierField;

        public string DestinationIdentifierField => RemoveIfNotEmpty(_settings.DestinationIdentifierField);

        public bool MoveExistingDocuments => _settings.MoveExistingDocuments;

        public string ProductionPrecedence => RemoveIfNotEmpty(_settings.ProductionPrecedence);

        public bool IncludeOriginalImages => _settings.IncludeOriginalImages;

        public IEnumerable<ProductionDTO> ImagePrecedence => _settings.ImagePrecedence.Select(prod => new ProductionDTO
        {
            ArtifactID = prod.ArtifactID,
            DisplayName = RemoveIfNotEmpty(prod.DisplayName)
        });

        public int ProductionArtifactId => _settings.ProductionArtifactId;

        public bool CreateSavedSearchForTagging => _settings.CreateSavedSearchForTagging;

        public bool LoadImportedFullTextFromServer => _settings.LoadImportedFullTextFromServer;

        internal NativeFileCopyModeEnum NativeFileCopyMode => _settings.NativeFileCopyMode;

        internal OverlayBehavior OverlayBehavior => _settings.OverlayBehavior;

        internal ImportAuditLevel AuditLevel => _settings.AuditLevel;

        public bool MoveDocumentsInAnyOverlayMode => _settings.MoveDocumentsInAnyOverlayMode;

        private string RemoveIfNotEmpty(string toSanitize)
        {
            const string sensitiveDataRemoved = "[Sensitive data has been removed]";

            return string.IsNullOrEmpty(toSanitize) ? string.Empty : sensitiveDataRemoved;
        }
    }
}
