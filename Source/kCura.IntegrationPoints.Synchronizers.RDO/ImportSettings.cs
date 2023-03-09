using System;
using System.Collections.Generic;
using System.Text;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.DataReaderClient;
using Newtonsoft.Json;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class ImportSettings
    {
        public const string FIELDOVERLAYBEHAVIOR_DEFAULT = "Use Field Settings";
        public const string FIELDOVERLAYBEHAVIOR_MERGE = "Merge Values";
        public const string FIELDOVERLAYBEHAVIOR_REPLACE = "Replace Values";
        private int _destinationArtifactTypeId;

        public ImportSettings()
        {
            ImportAuditLevel = ImportAuditLevelEnum.FullAudit;
            MultiValueDelimiter = Constants.MULTI_VALUE_DELIMITER;
            ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
            NestedValueDelimiter = Constants.NESTED_VALUE_DELIMITER;
            ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly;
            ErrorFilePath = string.Empty;
            EnableTagging = true;
            UseSmartOverwrite = false;
        }

        #region "Public Properties"

        [JsonProperty(PropertyName = "artifactTypeID")]
        public int ArtifactTypeId { get; set; }

        /// <summary>
        /// Specifies Artifact Type ID of the destination object type. If the value is not set and equals to 0, then it returns the value of <see cref="ArtifactTypeId"/>
        /// (to preserve backwards compatibility after implementing non-document object transfer in Sync)
        /// </summary>
        public int DestinationArtifactTypeId
        {
            get { return _destinationArtifactTypeId != 0 ? _destinationArtifactTypeId : ArtifactTypeId; }
            set { _destinationArtifactTypeId = value; }
        }

        public string BulkLoadFileFieldDelimiter { get; set; }

        public int CaseArtifactId { get; set; }

        public bool CopyFilesToDocumentRepository { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool EntityManagerFieldContainsLink { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public int DestinationFolderArtifactId { get; set; }

        public bool DisableControlNumberCompatibilityMode { get; set; }

        public bool? DisableExtractedTextEncodingCheck { get; set; }

        public bool DisableExtractedTextFileLocationValidation { get; set; }

        public bool? DisableNativeLocationValidation { get; set; }

        public bool? DisableNativeValidation { get; set; }

        [JsonProperty(PropertyName = "destinationProviderType")]
        public string DestinationProviderType { get; set; }

        public bool DisableUserSecurityCheck { get; set; }

        public string ErrorFilePath { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool ExtractedTextFieldContainsFilePath { get; set; }

        public string ExtractedTextFileEncoding { get; set; }

        /// <summary>
        /// Indicates user preferences of field overlay behavior.
        /// </summary>
        /// <remarks>
        /// possible values : 'Merge Values', 'Replace Values', 'Use Field Settings'
        /// value is assigned in step-mapFields.js
        /// </remarks>
        public string FieldOverlayBehavior { get; set; }

        public string FileNameColumn { get; set; }

        public string FileSizeColumn { get; set; }

        public bool FileSizeMapped { get; set; }

        public string FolderPathSourceFieldName { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool UseDynamicFolderPath { get; set; }

        public int IdentityFieldId { get; set; }

        public ImportAuditLevelEnum ImportAuditLevel { get; set; }

        public Guid CorrelationId { get; set; }

        public long? JobID { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        [JsonProperty(PropertyName = "importNativeFile")]
        public bool ImportNativeFile { get; set; }

        [JsonProperty(PropertyName = "importNativeFileCopyMode")]
        public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; set; }

        public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }

        public string LongTextColumnThatContainsPathToFullText { get; set; }

        public char MultiValueDelimiter { get; set; }

        public string NativeFilePathSourceFieldName { get; set; }

        public char NestedValueDelimiter { get; set; }

        public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }

        public string OIFileIdColumnName { get; set; }

        public bool OIFileIdMapped { get; set; }

        public string OIFileTypeColumnName { get; set; }

        public string SupportedByViewerColumn { get; set; }

        public int OnBehalfOfUserId { get; set; }

        public string ParentObjectIdSourceFieldName { get; set; }

        public string Provider { get; set; }

        public string RelativityPassword { get; set; }

        public string RelativityUsername { get; set; }

        public bool SendEmailOnLoadCompletion { get; set; }

        public string SelectedCaseFileRepoPath { get; set; }

        public int StartRecordNumber { get; set; }

        public string WebServiceURL { get; set; }

        public bool AutoNumberImages { get; set; }

        public bool ProductionImport { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool ImageImport { get; set; }

        public string DestinationIdentifierField { get; set; }

        /// <summary>
        /// In Overlay mode it allows to switch Yes/No if import API should move documents between folders when use folder path information
        /// </summary>
        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool MoveExistingDocuments { get; set; }

        public int ProductionPrecedence { get; set; }

        public bool IncludeOriginalImages { get; set; }

        public List<ProductionDTO> ImagePrecedence { get; set; }

        public int ProductionArtifactId { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool CreateSavedSearchForTagging { get; set; }

        public bool LoadImportedFullTextFromServer { get; set; }

        public bool EnableTagging { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool UseFolderPathInformation { get; set; }

        public int FolderPathSourceField { get; set; }

        public bool UseSmartOverwrite { get; set; }

        #endregion "Public Properties"

        #region "Calculated Properties"

        [JsonIgnore]
        public ImportOverlayBehaviorEnum ImportOverlayBehavior
        {
            get
            {
                if (string.IsNullOrEmpty(FieldOverlayBehavior) || FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_DEFAULT)
                {
                    return ImportOverlayBehaviorEnum.UseRelativityDefaults;
                }
                if (FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_MERGE)
                {
                    return ImportOverlayBehaviorEnum.MergeAll;
                }
                if (FieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_REPLACE)
                {
                    return ImportOverlayBehaviorEnum.ReplaceAll;
                }
                throw new IntegrationPointsException($"Unable to determine Import Overlay Behavior : {FieldOverlayBehavior}");
            }
        }

        [JsonIgnore]
        internal Encoding ExtractedTextEncoding => string.IsNullOrWhiteSpace(ExtractedTextFileEncoding) ? Encoding.Default : Encoding.GetEncoding(ExtractedTextFileEncoding);

        [JsonIgnore]
        internal NativeFileCopyModeEnum NativeFileCopyMode => (NativeFileCopyModeEnum)ImportNativeFileCopyMode;

        [JsonIgnore]
        internal EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior OverlayBehavior => (EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior)ImportOverlayBehavior;

        [JsonIgnore]
        internal EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel AuditLevel => (EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel)ImportAuditLevel;

        [JsonIgnore]
        public bool MoveDocumentsInAnyOverlayMode => ImportOverwriteMode != ImportOverwriteModeEnum.AppendOnly &&
                                                     MoveExistingDocuments && !string.IsNullOrEmpty(FolderPathSourceFieldName);

        [JsonIgnore]
        public bool UseFolderPath => UseFolderPathInformation || UseDynamicFolderPath;

        #endregion "Calculated Properties"

        #region Public Methods
        public bool IsRelativityProvider()
        {
            return Provider != null && string.Equals(Provider, "relativity", StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion
    }
}
