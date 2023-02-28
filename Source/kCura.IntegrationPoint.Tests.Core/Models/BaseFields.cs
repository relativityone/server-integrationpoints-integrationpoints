using Newtonsoft.Json;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class BaseFields : BaseField
    {
        [JsonProperty(PropertyName = "Artifact Type ID")]
        public int ArtifactTypeId;

        [JsonProperty(PropertyName = "Artifact Type Name")]
        public string ArtifactTypeName;
    }
}
