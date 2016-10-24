using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace kCura.IntegrationPoints.Core.Models
{
	public class Scheduler
	{
		public Scheduler()
		{
		}

		public Scheduler(IntegrationPoint ip)
		{
			this.EnableScheduler = ip.EnableScheduler.GetValueOrDefault(false);

			var rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(ip.ScheduleRule);
			if (rule != null)
			{

				if (rule.EndDate.HasValue)
				{
					this.EndDate = rule.EndDate.Value.ToString("MM/dd/yyyy");
				}
				if (rule.StartDate.HasValue)
				{
					this.StartDate = rule.StartDate.Value.ToString("MM/dd/yyyy");
				}
				if (rule.OccuranceInMonth.HasValue)
				{
					//we are the more complex month selector

				}
				switch (rule.Interval)
				{
					case ScheduleInterval.Daily:
						this.SendOn = string.Empty;
						break;
					case ScheduleInterval.Weekly:
						this.SendOn =
							JsonConvert.SerializeObject(new IntegrationPointService.Weekly()
							{
								SelectedDays = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday) == DaysOfWeek.Day ? new List<string> { DaysOfWeek.Day.ToString().ToLower() } : IntegrationPointService.FromDaysOfWeek(rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday)).Select(x => x.ToString()).ToList()

							}, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
						break;
					case ScheduleInterval.Monthly:
						var type = rule.OccuranceInMonth.HasValue ? IntegrationPointService.MonthlyType.Month : IntegrationPointService.MonthlyType.Days;
						this.SendOn = JsonConvert.SerializeObject(new IntegrationPointService.Monthly()
						{
							MonthChoice = type,
							SelectedDay = rule.DayOfMonth.GetValueOrDefault(1),
							SelectedDayOfTheMonth = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday),
							SelectedType = rule.OccuranceInMonth
						}, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

						break;
				}

				this.Reoccur = rule.Reoccur.GetValueOrDefault(0);
				SelectedFrequency = rule.Interval.ToString();
				if (rule.LocalTimeOfDay.HasValue)
				{
					var date = DateTime.Today;
					var ticks = new DateTime(rule.LocalTimeOfDay.Value.Ticks);
					date = date.AddHours(ticks.Hour);
					date = date.AddMinutes(ticks.Minute);
					var time = date.ToUniversalTime();
					this.ScheduledTime = time.Hour + ":" + time.Minute;
				}
			}
		}

		public bool EnableScheduler { get; set; }
		public string EndDate { get; set; }
		public int TimeZoneOffsetInMinute { get; set; }
		public string StartDate { get; set; }
		public string SelectedFrequency { get; set; }
		public int Reoccur { get; set; }
		public string ScheduledTime { get; set; }
		public string SendOn { get; set; }
	}



	public class IntegrationModel
	{
		public int ArtifactID { get; set; }
		public string Name { get; set; }
		public string SelectedOverwrite { get; set; }
		public int SourceProvider { get; set; }
		public int DestinationProvider { get; set; }
		public string Destination { get; set; }
		public Scheduler Scheduler { get; set; }
		public DateTime? NextRun { get; set; }
		public DateTime? LastRun { get; set; }
		public string SourceConfiguration { get; set; }
		public string Map { get; set; }
		public bool LogErrors { get; set; }
		public bool? HasErrors { get; set; }
		public string NotificationEmails { get; set; }

		public IntegrationModel()
		{
			this.SourceConfiguration = string.Empty;
			this.LogErrors = true;
			this.HasErrors = false;
		}

		public IntegrationPoint ToRdo(IEnumerable<Relativity.Client.DTOs.Choice> choices)
		{
			var point = new IntegrationPoint();
			point.ArtifactId = this.ArtifactID;
			point.Name = this.Name;
			var choice = choices.FirstOrDefault(x => x.Name.Equals(this.SelectedOverwrite));
			if (choice == null)
			{
				throw new Exception("Cannot find choice by the name " + this.SelectedOverwrite);
			}
			point.OverwriteFields = new Relativity.Client.DTOs.Choice(choice.ArtifactID) {Name = choice.Name};
			point.SourceConfiguration = this.SourceConfiguration;
			point.SourceProvider = null;
			point.SourceProvider = this.SourceProvider;
			point.DestinationConfiguration = this.Destination;
			point.FieldMappings = this.Map;
			point.EnableScheduler = this.Scheduler.EnableScheduler;
			point.DestinationProvider = this.DestinationProvider;
			point.LogErrors = this.LogErrors;
			point.HasErrors = this.HasErrors;
			point.EmailNotificationRecipients = string.Join("; ", (this.NotificationEmails ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList());
			point.LastRuntimeUTC = LastRun;

			return point;
		}

		public IntegrationModel(IntegrationPoint ip)
		{
			this.ArtifactID = ip.ArtifactId;
			Name = ip.Name;
			SelectedOverwrite = string.Empty;
			if (ip.OverwriteFields != null)
			{
				SelectedOverwrite = ip.OverwriteFields.Name;
			}

			this.SourceProvider = ip.SourceProvider.GetValueOrDefault(0);
			//if (ip.OverwriteFields != null)
			//{
			//	SelectedOverwrite = ip.OverwriteFields.Name;
			//}
			//StartDate = ip.StartDate;
			//EndDate = ip.EndDate;
			this.Destination = ip.DestinationConfiguration;
			this.SourceConfiguration = ip.SourceConfiguration;
			this.DestinationProvider = ip.DestinationProvider.GetValueOrDefault(0);
			Scheduler = new Scheduler(ip);
			this.NotificationEmails = ip.EmailNotificationRecipients ?? string.Empty;
			this.LogErrors = ip.LogErrors.GetValueOrDefault(false);
			this.HasErrors = ip.HasErrors.GetValueOrDefault(false);
			this.LastRun = ip.LastRuntimeUTC;
			this.NextRun = ip.NextScheduledRuntimeUTC;
			this.Map = ip.FieldMappings;
		}
	}
}
