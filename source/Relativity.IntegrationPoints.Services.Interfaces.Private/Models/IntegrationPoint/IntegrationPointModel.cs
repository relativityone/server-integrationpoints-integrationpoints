using System.Collections.Generic;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services
{
    public class IntegrationPointModel : BaseModel
    {
        /// <summary>
        /// Artifact Id of the source provider.
        /// </summary>
        public int SourceProvider { get; set; }

        /// <summary>
        /// Artifact Id of the destination provider.
        /// </summary>
        public int DestinationProvider { get; set; }

        public List<FieldMap> FieldMappings { get; set; }

        public object SourceConfiguration { get; set; }

        public object DestinationConfiguration { get; set; }

        public ScheduleModel ScheduleRule { get; set; }

        public bool LogErrors { get; set; }

        public string EmailNotificationRecipients { get; set; }

        public int Type { get; set; }

        public int OverwriteFieldsChoiceId { get; set; }

        public object SecuredConfiguration { get; set; }

        public bool PromoteEligible { get; set; }

        public ImportFileCopyModeEnum? ImportFileCopyMode { get; set; }
    }
}
