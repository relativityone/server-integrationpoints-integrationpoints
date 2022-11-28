using System;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Web.Models
{
    /// <summary>
    /// This class reflects integration point data structure readable by frontend.
    /// It was created to limit the scope of changes in one pull request.
    /// Once we refactor IntegrationPointDto class to hold deserialized SourceConfiguration and DestinationConfiguration
    /// instead of strings, frontend side should be refactored to receive/send objects for all 3 Long Text fields
    /// and then this class would be forgotten.
    /// </summary>
    public abstract class IntegrationPointWebModelBase
    {
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
