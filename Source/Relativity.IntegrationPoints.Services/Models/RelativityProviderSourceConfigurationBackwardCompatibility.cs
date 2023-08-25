using kCura.IntegrationPoints.Core.Contracts.Configuration;
using Newtonsoft.Json;

namespace Relativity.IntegrationPoints.Services.Models
{
    internal class RelativityProviderSourceConfigurationBackwardCompatibility : SourceConfiguration
    {
        /// <summary>
        ///     This is not used - DestinationFolderArtifactId
        /// </summary>
        public int FolderArtifactId { get; set;  }

        [JsonProperty(PropertyName = "TaggingOption")]
        public string TaggingOption { get; set; }

        public bool ProductionImport { get; set; }

        public bool UseDynamicFolderPath { get; set; }
    }
}
