using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Extensions
{
    public static class ModelExtensions
    {
        public static IntegrationPointWebModel ToWebModel(this IntegrationPointDto dto, ICamelCaseSerializer serializer)
        {
            return new IntegrationPointWebModel
            {
                ArtifactID = dto.ArtifactId,
                Name = dto.Name,
                SelectedOverwrite = dto.SelectedOverwrite,
                SourceConfiguration = dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                Destination = dto.DestinationConfiguration,
                Map = serializer.SerializeCamelCase(dto.FieldMappings),
                Scheduler = dto.Scheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                NotificationEmails = dto.EmailNotificationRecipients,
                SecuredConfiguration = dto.SecuredConfiguration,
                NextRun = dto.NextRun,
                PromoteEligible = dto.PromoteEligible,
                LastRun = dto.LastRun,
                HasErrors = dto.HasErrors,
                JobHistory = dto.JobHistory,
            };
        }

        public static IntegrationPointDto ToDto(this IntegrationPointWebModel webModel, ICamelCaseSerializer serializer)
        {
            return new IntegrationPointDto
            {
                ArtifactId = webModel.ArtifactID,
                Name = webModel.Name,
                SelectedOverwrite = webModel.SelectedOverwrite,
                SourceConfiguration = webModel.SourceConfiguration,
                SourceProvider = webModel.SourceProvider,
                Type = webModel.Type,
                DestinationConfiguration = webModel.Destination,
                FieldMappings = serializer.Deserialize<List<FieldMap>>(webModel.Map),
                Scheduler = webModel.Scheduler,
                DestinationProvider = webModel.DestinationProvider,
                LogErrors = webModel.LogErrors,
                EmailNotificationRecipients = webModel.NotificationEmails,
                SecuredConfiguration = webModel.SecuredConfiguration,
                NextRun = webModel.NextRun,
                PromoteEligible = webModel.PromoteEligible,
                LastRun = webModel.LastRun,
                HasErrors = webModel.HasErrors,
                JobHistory = webModel.JobHistory,
            };
        }

        public static IntegrationPointProfileWebModel ToWebModel(this IntegrationPointProfileDto dto, ICamelCaseSerializer serializer)
        {
            return new IntegrationPointProfileWebModel
            {
                ArtifactID = dto.ArtifactId,
                Name = dto.Name,
                SelectedOverwrite = dto.SelectedOverwrite,
                SourceConfiguration = dto.SourceConfiguration,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                Destination = dto.DestinationConfiguration,
                Map = serializer.SerializeCamelCase(dto.FieldMappings),
                Scheduler = dto.Scheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                NotificationEmails = dto.EmailNotificationRecipients,
                SecuredConfiguration = dto.SecuredConfiguration,
                NextRun = dto.NextRun,
                PromoteEligible = dto.PromoteEligible,
            };
        }

        public static IntegrationPointProfileWebModel ToWebModel(this IntegrationPointProfileSlimDto dto)
        {
            return new IntegrationPointProfileWebModel
            {
                ArtifactID = dto.ArtifactId,
                Name = dto.Name,
                SelectedOverwrite = dto.SelectedOverwrite,
                SourceProvider = dto.SourceProvider,
                Type = dto.Type,
                Scheduler = dto.Scheduler,
                DestinationProvider = dto.DestinationProvider,
                LogErrors = dto.LogErrors,
                NotificationEmails = dto.EmailNotificationRecipients,
                SecuredConfiguration = dto.SecuredConfiguration,
                NextRun = dto.NextRun,
                PromoteEligible = dto.PromoteEligible,
            };
        }

        public static IntegrationPointProfileDto ToDto(this IntegrationPointProfileWebModel webModel, ICamelCaseSerializer serializer)
        {
            return new IntegrationPointProfileDto
            {
                ArtifactId = webModel.ArtifactID,
                Name = webModel.Name,
                SelectedOverwrite = webModel.SelectedOverwrite,
                SourceConfiguration = webModel.SourceConfiguration,
                SourceProvider = webModel.SourceProvider,
                Type = webModel.Type,
                DestinationConfiguration = webModel.Destination,
                FieldMappings = serializer.Deserialize<List<FieldMap>>(webModel.Map),
                Scheduler = webModel.Scheduler,
                DestinationProvider = webModel.DestinationProvider,
                LogErrors = webModel.LogErrors,
                EmailNotificationRecipients = webModel.NotificationEmails,
                SecuredConfiguration = webModel.SecuredConfiguration,
                NextRun = webModel.NextRun,
                PromoteEligible = webModel.PromoteEligible,
            };
        }
    }
}
