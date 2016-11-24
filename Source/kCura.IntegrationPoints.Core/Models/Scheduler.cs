using System;
using System.Collections.Generic;
using System.Linq;
using kCura.ScheduleQueue.Core.Helpers;
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

		public Scheduler(bool enableScheduler, string scheduleRule)
		{
			EnableScheduler = enableScheduler;

			var rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(scheduleRule);
			if (rule != null)
			{

				if (rule.EndDate.HasValue)
				{
					EndDate = rule.EndDate.Value.ToString("MM/dd/yyyy");
				}
				if (rule.StartDate.HasValue)
				{
					StartDate = rule.StartDate.Value.ToString("MM/dd/yyyy");
				}
				if (rule.OccuranceInMonth.HasValue)
				{
					//we are the more complex month selector

				}
				switch (rule.Interval)
				{
					case ScheduleInterval.Daily:
						SendOn = string.Empty;
						break;
					case ScheduleInterval.Weekly:
						SendOn =
							JsonConvert.SerializeObject(new Weekly
							{
								SelectedDays = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday) == DaysOfWeek.Day ? new List<string> { DaysOfWeek.Day.ToString().ToLower() } : DaysOfWeekConverter.FromDaysOfWeek(rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday)).Select(x => x.ToString()).ToList()

							}, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
						break;
					case ScheduleInterval.Monthly:
						var type = rule.OccuranceInMonth.HasValue ? MonthlyType.Month : MonthlyType.Days;
						SendOn = JsonConvert.SerializeObject(new Monthly
						{
							MonthChoice = type,
							SelectedDay = rule.DayOfMonth.GetValueOrDefault(1),
							SelectedDayOfTheMonth = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday),
							SelectedType = rule.OccuranceInMonth
						}, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

						break;
				}

				Reoccur = rule.Reoccur.GetValueOrDefault(0);
				SelectedFrequency = rule.Interval.ToString();
				if (rule.LocalTimeOfDay.HasValue)
				{
					var date = DateTime.Today;
					var ticks = new DateTime(rule.LocalTimeOfDay.Value.Ticks);
					date = date.AddHours(ticks.Hour);
					date = date.AddMinutes(ticks.Minute);
					var time = date.ToUniversalTime();
					ScheduledTime = time.Hour + ":" + time.Minute;
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
}