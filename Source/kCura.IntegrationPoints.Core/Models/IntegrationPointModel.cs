using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Models
{
	public class IntegrationPointModel : IntegrationPointModelBase
	{
		public DateTime? LastRun { get; set; }
		public bool? HasErrors { get; set; }

		public IntegrationPointModel()
		{
			HasErrors = false;
		}

		public IntegrationPoint ToRdo(IEnumerable<Choice> choices, PeriodicScheduleRule rule)
		{
			var choice = choices.FirstOrDefault(x => x.Name.Equals(SelectedOverwrite));
			if (choice == null)
			{
				throw new Exception("Cannot find choice by the name " + SelectedOverwrite);
			}
			var point = new IntegrationPoint
			{
				ArtifactId = ArtifactID,
				Name = Name,
				OverwriteFields = new Choice(choice.ArtifactID) {Name = choice.Name},
				SourceConfiguration = SourceConfiguration,
				SourceProvider = SourceProvider,
				Type = Type,
				DestinationConfiguration = Destination,
				FieldMappings = Map,
				EnableScheduler = Scheduler.EnableScheduler,
				DestinationProvider = DestinationProvider,
				LogErrors = LogErrors,
				HasErrors = HasErrors,
				EmailNotificationRecipients =
					string.Join("; ", (NotificationEmails ?? string.Empty).Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList()),
				LastRuntimeUTC = LastRun,
				Credentials = string.Empty
			};

			if (point.EnableScheduler.GetValueOrDefault(false))
			{
				point.ScheduleRule = rule.ToSerializedString();
				point.NextScheduledRuntimeUTC = rule.GetNextUTCRunDateTime();
			}
			else
			{
				point.ScheduleRule = string.Empty;
				point.NextScheduledRuntimeUTC = null;
			}

			return point;
		}

		public static IntegrationPointModel FromIntegrationPoint(IntegrationPoint ip)
		{
			return new IntegrationPointModel
			{
				ArtifactID = ip.ArtifactId,
				Name = ip.Name,
				SelectedOverwrite = ip.OverwriteFields == null ? string.Empty : ip.OverwriteFields.Name,
				SourceProvider = ip.SourceProvider.GetValueOrDefault(0),
				Destination = ip.DestinationConfiguration,
				SourceConfiguration = ip.SourceConfiguration,
				DestinationProvider = ip.DestinationProvider.GetValueOrDefault(0),
				Type = ip.Type.GetValueOrDefault(0),
				Scheduler = new Scheduler(ip.EnableScheduler.GetValueOrDefault(false), ip.ScheduleRule),
				NotificationEmails = ip.EmailNotificationRecipients ?? string.Empty,
				LogErrors = ip.LogErrors.GetValueOrDefault(false),
				HasErrors = ip.HasErrors.GetValueOrDefault(false),
				LastRun = ip.LastRuntimeUTC,
				NextRun = ip.NextScheduledRuntimeUTC,
				Map = ip.FieldMappings
			};
		}
	}
}
