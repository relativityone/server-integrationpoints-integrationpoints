using System.Collections.Generic;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the model for an integration point.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the list of field mappings.
        /// </summary>
        public List<FieldMap> FieldMappings { get; set; }

        /// <summary>
        /// Gets or sets the source configuration.
        /// </summary>
        public object SourceConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the destination configuration.
        /// </summary>
        public object DestinationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the schedule rule.
        /// </summary>
        public ScheduleModel ScheduleRule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log errors.
        /// </summary>
        public bool LogErrors { get; set; }

        /// <summary>
        /// Gets or sets the email notification recipients.
        /// </summary>
        public string EmailNotificationRecipients { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets the overwrite fields choice Id.
        /// </summary>
        public int OverwriteFieldsChoiceId { get; set; }

        /// <summary>
        /// Gets or sets the secured configuration.
        /// </summary>
        public object SecuredConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to promote eligible items.
        /// </summary>
        public bool PromoteEligible { get; set; }

        /// <summary>
        /// Gets or sets the import file copy mode.
        /// </summary>
        public ImportFileCopyModeEnum? ImportFileCopyMode { get; set; }
    }
}
