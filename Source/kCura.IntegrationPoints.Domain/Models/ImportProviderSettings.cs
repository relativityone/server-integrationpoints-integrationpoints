using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ImportProviderSettings : ImportSettingsBase
    {
        public string ExtractedTextPathFieldIdentifier { get; set; }
        public string NativeFilePathFieldIdentifier { get; set; }
        public int DestinationFolderArtifactId { get; set; }
    }
}
