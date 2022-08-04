using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Models
{
    public class IntegrationPointProfileModel : IntegrationPointModelBase
    {
        public static IntegrationPointProfileModel FromIntegrationPoint(IntegrationPoint ip, string name)
        {
            return new IntegrationPointProfileModel
            {
                Name = name,
                SelectedOverwrite = ip.OverwriteFields == null ? string.Empty : ip.OverwriteFields.Name,
                SourceProvider = ip.SourceProvider.GetValueOrDefault(0),
                Destination = ip.DestinationConfiguration,
                SourceConfiguration = ip.SourceConfiguration,
                DestinationProvider = ip.DestinationProvider.GetValueOrDefault(0),
                Type = ip.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(ip.EnableScheduler.GetValueOrDefault(false), ip.ScheduleRule),
                NotificationEmails = ip.EmailNotificationRecipients ?? string.Empty,
                LogErrors = ip.LogErrors.GetValueOrDefault(false),
                NextRun = ip.NextScheduledRuntimeUTC,
                Map = ip.FieldMappings,
                SecuredConfiguration = ip.SecuredConfiguration
            };
        }

        public static IntegrationPointProfileModel FromIntegrationPointProfile(IntegrationPointProfile profile)
        {
            return new IntegrationPointProfileModel
            {
                ArtifactID = profile.ArtifactId,
                Name = profile.Name,
                SelectedOverwrite = profile.OverwriteFields == null ? string.Empty : profile.OverwriteFields.Name,
                SourceProvider = profile.SourceProvider.GetValueOrDefault(0),
                Destination = profile.DestinationConfiguration,
                SourceConfiguration = profile.SourceConfiguration,
                DestinationProvider = profile.DestinationProvider.GetValueOrDefault(0),
                Type = profile.Type.GetValueOrDefault(0),
                Scheduler = new Scheduler(profile.EnableScheduler.GetValueOrDefault(false), profile.ScheduleRule),
                NotificationEmails = profile.EmailNotificationRecipients ?? string.Empty,
                LogErrors = profile.LogErrors.GetValueOrDefault(false),
                Map = profile.FieldMappings
            };
        }

        public static IntegrationPointProfileModel FromIntegrationPointProfileSimpleModel(IntegrationPointProfile profile)
        {
            return new IntegrationPointProfileModel
            {
                ArtifactID = profile.ArtifactId,
                Name = profile.Name,
                SourceProvider = profile.SourceProvider.GetValueOrDefault(0),
                DestinationProvider = profile.DestinationProvider.GetValueOrDefault(0),
                Type = profile.Type.GetValueOrDefault(0)
            };
        }

        public IntegrationPointProfile ToRdo(IEnumerable<ChoiceRef> choices, PeriodicScheduleRule rule)
        {
            ChoiceRef choice = choices.FirstOrDefault(x => x.Name.Equals(SelectedOverwrite));
            if (choice == null)
            {
                throw new Exception("Cannot find choice by the name " + SelectedOverwrite);
            }
            var profile = new IntegrationPointProfile
            {
                ArtifactId = ArtifactID,
                Name = Name,
                OverwriteFields = new ChoiceRef(choice.ArtifactID) {Name = choice.Name},
                SourceConfiguration = SourceConfiguration,
                SourceProvider = SourceProvider,
                Type = Type,
                DestinationConfiguration = Destination,
                FieldMappings = Map,
                EnableScheduler = Scheduler.EnableScheduler,
                DestinationProvider = DestinationProvider,
                LogErrors = LogErrors,
                EmailNotificationRecipients = string.Join("; ",
                    (NotificationEmails ?? string.Empty).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList())
            };

            if (profile.EnableScheduler.GetValueOrDefault(false))
            {
                profile.ScheduleRule = rule.ToSerializedString();
                profile.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
            }
            else
            {
                profile.ScheduleRule = string.Empty;
                profile.NextScheduledRuntimeUTC = null;
            }

            return profile;
        }
    }
}