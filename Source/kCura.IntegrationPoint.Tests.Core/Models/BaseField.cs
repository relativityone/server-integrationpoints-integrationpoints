using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class BaseField
    {
        [JsonProperty(PropertyName = "Artifact ID")]
        public int ArtifactId;
    }
}
