using System;
using System.Collections.Generic;
using System.Text;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.DataReaderClient;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class ImportSettings
    {
        public const string FIELDOVERLAYBEHAVIOR_DEFAULT = "Use Field Settings";
        public const string FIELDOVERLAYBEHAVIOR_MERGE = "Merge Values";
        public const string FIELDOVERLAYBEHAVIOR_REPLACE = "Replace Values";

        public ImportSettings(DestinationConfiguration destinationConfiguration)
        {
            DestinationConfiguration = destinationConfiguration;
            MultiValueDelimiter = Constants.MULTI_VALUE_DELIMITER;
            NestedValueDelimiter = Constants.NESTED_VALUE_DELIMITER;
        }

        public DestinationConfiguration DestinationConfiguration { get; }

        public bool? DisableNativeLocationValidation { get; set; }

        public bool? DisableNativeValidation { get; set; }

        public string ErrorFilePath { get; set; }

        public string FileNameColumn { get; set; }

        public string FileSizeColumn { get; set; }

        public bool FileSizeMapped { get; set; }

        public string FolderPathSourceFieldName { get; set; }

        // Obsolete: Used only in ImportJobFactory. To be removed after rewriting CustomProviders with IAPI v2
        public Guid CorrelationId { get; set; }

        // Obsolete: Used only in ImportJobFactory. To be removed after rewriting CustomProviders with IAPI v2
        public long? JobID { get; set; }

        public char MultiValueDelimiter { get; set; }

        public string NativeFilePathSourceFieldName { get; set; }

        public char NestedValueDelimiter { get; set; }

        public IList<int> ObjectFieldIdListContainsArtifactId { get; set; }

        public bool OIFileIdMapped { get; set; }

        public string OIFileTypeColumnName { get; set; }

        public string OIFileIdColumnName { get; set; }

        public string SupportedByViewerColumn { get; set; }

        public string ParentObjectIdSourceFieldName { get; set; }

        public int StartRecordNumber { get; set; }

        public string DestinationIdentifierField { get; set; }

        public bool LoadImportedFullTextFromServer { get; set; }

        public EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior ImportOverlayBehavior
        {
            get
            {
                string fieldOverlayBehavior = DestinationConfiguration.FieldOverlayBehavior;
                if (string.IsNullOrEmpty(fieldOverlayBehavior) || fieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_DEFAULT)
                {
                    return EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.UseRelativityDefaults;
                }
                if (fieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_MERGE)
                {
                    return EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.MergeAll;
                }
                if (fieldOverlayBehavior == FIELDOVERLAYBEHAVIOR_REPLACE)
                {
                    return EDDS.WebAPI.BulkImportManagerBase.OverlayBehavior.ReplaceAll;
                }
                throw new IntegrationPointsException($"Unable to determine Import Overlay Behavior : {fieldOverlayBehavior}");
            }
        }

        internal Encoding ExtractedTextEncoding => string.IsNullOrWhiteSpace(DestinationConfiguration.ExtractedTextFileEncoding)
            ? Encoding.Default
            : Encoding.GetEncoding(DestinationConfiguration.ExtractedTextFileEncoding);

        internal NativeFileCopyModeEnum NativeFileCopyMode => (NativeFileCopyModeEnum)DestinationConfiguration.ImportNativeFileCopyMode;

        internal bool MoveDocumentsInAnyOverlayMode => DestinationConfiguration.ImportOverwriteMode != ImportOverwriteModeEnum.AppendOnly &&
                                                     DestinationConfiguration.MoveExistingDocuments && !string.IsNullOrEmpty(FolderPathSourceFieldName);

        // Obsolete: Used only in ImportJobFactory. To be removed after rewriting CustomProviders with IAPI v2
        public bool IsRelativityProvider()
        {
            return DestinationConfiguration.Provider != null && string.Equals(DestinationConfiguration.Provider, "relativity", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
