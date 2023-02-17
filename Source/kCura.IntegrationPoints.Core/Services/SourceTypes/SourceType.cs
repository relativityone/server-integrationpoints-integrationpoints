using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
    public class SourceType
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public string SourceURL { get; set; }

        public int ArtifactID { get; set; }

        public SourceProviderConfiguration Config { get; set; }
    }
}
