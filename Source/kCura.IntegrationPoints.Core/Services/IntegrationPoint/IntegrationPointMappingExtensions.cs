using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public static class IntegrationPointMappingExtensions
    {
        public static IntegrationPointDto ToIntegrationPointDto(this IntegrationPointProfileDto profile, string integrationPointName)
        {
            return new IntegrationPointDto
            {
                Name = integrationPointName,
                SelectedOverwrite = profile.SelectedOverwrite,
                SourceProvider = profile.SourceProvider,
                DestinationConfiguration = profile.DestinationConfiguration,
                SourceConfiguration = profile.SourceConfiguration,
                DestinationProvider = profile.DestinationProvider,
                Type = profile.Type,
                Scheduler = Scheduler.Clone(profile.Scheduler),
                EmailNotificationRecipients = profile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = profile.LogErrors,
                NextRun = profile.NextRun,
                FieldMappings = CloneFieldMappings(profile.FieldMappings),
                PromoteEligible = profile.PromoteEligible,
                SecuredConfiguration = profile.SecuredConfiguration
            };
        }

        public static IntegrationPointProfileDto ToProfileDto(this IntegrationPointDto ip, string name)
        {
            return new IntegrationPointProfileDto
            {
                Name = name,
                SelectedOverwrite = ip.SelectedOverwrite,
                SourceProvider = ip.SourceProvider,
                DestinationConfiguration = ip.DestinationConfiguration,
                SourceConfiguration = ip.SourceConfiguration,
                DestinationProvider = ip.DestinationProvider,
                Type = ip.Type,
                Scheduler = Scheduler.Clone(ip.Scheduler),
                EmailNotificationRecipients = ip.EmailNotificationRecipients ?? string.Empty,
                LogErrors = ip.LogErrors,
                NextRun = ip.NextRun,
                FieldMappings = CloneFieldMappings(ip.FieldMappings),
                SecuredConfiguration = ip.SecuredConfiguration,
                PromoteEligible = ip.PromoteEligible
            };
        }

        private static List<FieldMap> CloneFieldMappings(List<FieldMap> source)
        {
            return source != null
                ? new List<FieldMap>(source.Select(x => new FieldMap
                    {
                        SourceField = x.SourceField,
                        DestinationField = x.DestinationField,
                        FieldMapType = x.FieldMapType,
                    }))
                : null;
        }
    }
}
