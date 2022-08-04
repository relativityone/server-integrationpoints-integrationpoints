namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class Audit
    {
        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public int ArtifactId { get; set; }

        public string ArtifactName { get; set; }

        public string AuditAction { get; set; }

        public string AuditDetails { get; set; }
    }
}