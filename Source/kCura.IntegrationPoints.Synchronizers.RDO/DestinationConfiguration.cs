using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    /// <summary>
    /// This class represents a destination configuration data structure readable for our frontend.
    /// It also reflects the object stored in SQL - IntegrationPoint.DestinationConfiguration field.
    /// </summary>
    public class DestinationConfiguration
    {
        public DestinationConfiguration()
        {
            ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles;
            ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly;
            TaggingOption = TaggingOptionEnum.Enabled;
        }

        [JsonProperty(PropertyName = "artifactTypeID")]
        public int ArtifactTypeId { get; set; }

        /// <summary>
        /// Specifies Artifact Type ID of the destination object type.
        /// </summary>
        public int DestinationArtifactTypeId { get; set; }

        /// <summary>
        /// The destination workspace id.
        /// </summary>
        public int CaseArtifactId { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool CopyFilesToDocumentRepository { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool EntityManagerFieldContainsLink { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public int DestinationFolderArtifactId { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool ExtractedTextFieldContainsFilePath { get; set; }

        public string ExtractedTextFileEncoding { get; set; }

        public string FieldOverlayBehavior { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool UseDynamicFolderPath { get; set; }

        public int IdentityFieldId { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        [JsonProperty(PropertyName = "importNativeFile")]
        public bool ImportNativeFile { get; set; }

        [JsonProperty(PropertyName = "importNativeFileCopyMode")]
        public ImportNativeFileCopyModeEnum ImportNativeFileCopyMode { get; set; }

        public ImportOverwriteModeEnum ImportOverwriteMode { get; set; }

        public string LongTextColumnThatContainsPathToFullText { get; set; }

        public string Provider { get; set; }

        [JsonProperty(PropertyName = "destinationProviderType")]
        public string DestinationProviderType { get; set; }

        public string SelectedCaseFileRepoPath { get; set; }

        public bool AutoNumberImages { get; set; }

        public bool ProductionImport { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool ImageImport { get; set; }

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

        [JsonProperty(PropertyName = "TaggingOption")]
        public TaggingOptionEnum TaggingOption { get; set; }

        [JsonConverter(typeof(JsonQuotesConverter))]
        public bool UseFolderPathInformation { get; set; }

        public int FolderPathSourceField { get; set; }

        public bool UseSmartOverwrite { get; set; }

        // TODO: we need to make migration in order to get rid of this workaround
        public int GetDestinationArtifactTypeId()
        {
            return DestinationArtifactTypeId != 0
                ? DestinationArtifactTypeId
                : ArtifactTypeId;
        }
    }
}
