using System;

namespace kCura.IntegrationPoints.Core.Models
{
    public abstract class IntegrationPointSlimDtoBase
    {
        public int ArtifactId { get; set; }
        public string Name { get; set; }
        public string SelectedOverwrite { get; set; }
        public int SourceProvider { get; set; }
        public int DestinationProvider { get; set; }
        public int Type { get; set; }
        public Scheduler Scheduler { get; set; }
        public bool LogErrors { get; set; }
        public string EmailNotificationRecipients { get; set; }
        public DateTime? NextRun { get; set; }
        public string SecuredConfiguration { get; set; }
        public bool PromoteEligible { get; set; }
    }
}
