using System;

namespace kCura.IntegrationPoints.Core.Models
{
    public class IntegrationPointModelBase
    {
        public IntegrationPointModelBase()
        {
            LogErrors = true;
            SourceConfiguration = string.Empty;
        }

        public int ArtifactID { get; set; }
        public string Name { get; set; }
        public string SelectedOverwrite { get; set; }
        public int SourceProvider { get; set; }
        public int DestinationProvider { get; set; }
        public int Type { get; set; }
        public string Destination { get; set; }
        public Scheduler Scheduler { get; set; }
        public string SourceConfiguration { get; set; }
        public string Map { get; set; }
        public bool LogErrors { get; set; }
        public string NotificationEmails { get; set; }
        public DateTime? NextRun { get; set; }
        public string SecuredConfiguration { get; set; }
        public bool PromoteEligible { get; set; }
    }
}