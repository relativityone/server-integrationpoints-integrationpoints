using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services.Extensions
{
    public static class IntegrationPointExtensions
    {
        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPointDto data)
        {
            return GetIntegrationPointModel(data);
        }

        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPointProfileDto data)
        {
            return GetIntegrationPointModel(data);
        }

        private static IntegrationPointModel GetIntegrationPointModel(IntegrationPointDtoBase data)
        {
            return new IntegrationPointModel
            {
                ArtifactId = data.ArtifactId,
                Name = data.Name,
                SourceProvider = data.SourceProvider,
                DestinationProvider = data.DestinationProvider,
                DestinationConfiguration = data.DestinationConfiguration,
                Type = data.Type,
                EmailNotificationRecipients = data.EmailNotificationRecipients,
                FieldMappings = data.FieldMappings.Select(x => new FieldMap
                {
                    DestinationField = new FieldEntry
                    {
                        DisplayName = x.DestinationField.DisplayName,
                        IsIdentifier = x.DestinationField.IsIdentifier,
                        IsRequired = x.DestinationField.IsRequired,
                        Type = x.DestinationField.Type,
                        FieldType = (FieldType)x.DestinationField.FieldType,
                        FieldIdentifier = x.DestinationField.FieldIdentifier
                    },
                    SourceField = new FieldEntry
                    {
                        DisplayName = x.SourceField.DisplayName,
                        IsIdentifier = x.SourceField.IsIdentifier,
                        IsRequired = x.SourceField.IsRequired,
                        Type = x.SourceField.Type,
                        FieldType = (FieldType)x.SourceField.FieldType,
                        FieldIdentifier = x.SourceField.FieldIdentifier
                    },
                    FieldMapType = (FieldMapType)x.FieldMapType
                }).ToList(),
                PromoteEligible = data.PromoteEligible,
                SourceConfiguration = data.SourceConfiguration,
                ImportFileCopyMode = (ImportFileCopyModeEnum?)data.DestinationConfiguration.ImportNativeFileCopyMode,
                SecuredConfiguration = data.SecuredConfiguration,
                LogErrors = data.LogErrors,
                ScheduleRule = new ScheduleModel
                {
                    EnableScheduler = data.Scheduler.EnableScheduler,
                    EndDate = data.Scheduler.EndDate,
                    TimeZoneOffsetInMinute = data.Scheduler.TimeZoneOffsetInMinute,
                    Reoccur = data.Scheduler.Reoccur,
                    ScheduledTime = data.Scheduler.ScheduledTime,
                    SelectedFrequency = data.Scheduler.SelectedFrequency,
                    StartDate = data.Scheduler.StartDate,
                    SendOn = data.Scheduler.SendOn,
                    TimeZoneId = data.Scheduler.TimeZoneId,
                }
            };
        }
    }
}
