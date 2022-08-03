namespace kCura.IntegrationPoints.Domain.Models
{
    public class FederatedInstanceDto
    {
        public int? ArtifactId { get; set; } 
        public string Name { get; set; }
        public string InstanceUrl { get; set; }
        public string KeplerUrl { get; set; }
        public string WebApiUrl { get; set; }
    }
}