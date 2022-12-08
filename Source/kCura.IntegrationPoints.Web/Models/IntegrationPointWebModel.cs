using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Web.Models
{
    /// <summary>
    /// This class reflects integration point data structure readable by frontend.
    /// It was created to limit the scope of changes in one pull request.
    /// Once we refactor IntegrationPointDto class to hold deserialized SourceConfiguration and DestinationConfiguration
    /// instead of strings, frontend side should be refactored to receive/send objects for all 3 Long Text fields
    /// and then this class would be forgotten.
    /// </summary>
    public class IntegrationPointWebModel : IntegrationPointWebModelBase
    {
        public DateTime? LastRun { get; set; }

        public bool? HasErrors { get; set; }

        public List<int> JobHistory { get; set; }
    }
}
