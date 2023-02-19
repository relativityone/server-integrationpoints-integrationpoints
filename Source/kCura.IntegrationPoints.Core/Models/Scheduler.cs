using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.ScheduleQueue.Core.Helpers;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Models
{
    public class Scheduler
    {
        public Scheduler()
        {
        }

        public Scheduler(bool enableScheduler, string scheduleRule)
        {
            const string defaultDateFormat = "MM/dd/yyyy";
            EnableScheduler = enableScheduler;

            var rule = ScheduleRuleBase.Deserialize<PeriodicScheduleRule>(scheduleRule);
            if (rule != null)
            {
                if (rule.EndDate.HasValue)
                {
                    EndDate = rule.EndDate.Value.ToString(defaultDateFormat, CultureInfo.InvariantCulture);
                }
                if (rule.StartDate.HasValue)
                {
                    StartDate = rule.StartDate.Value.ToString(defaultDateFormat, CultureInfo.InvariantCulture);
                }
                if (rule.OccuranceInMonth.HasValue)
                {
                    // we are the more complex month selector

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
                                SelectedDays = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday) == DaysOfWeek.Day ?
                                    new List<string> { DaysOfWeek.Day.ToString().ToLowerInvariant() } :
                                    DaysOfWeekConverter.FromDaysOfWeek(rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday)).Select(x => x.ToString()).ToList()

                            }, Formatting.None, JSONHelper.GetDefaultSettings());
                        break;
                    case ScheduleInterval.Monthly:
                        var type = rule.OccuranceInMonth.HasValue ? MonthlyType.Month : MonthlyType.Days;
                        SendOn = JsonConvert.SerializeObject(new Monthly
                        {
                            MonthChoice = type,
                            SelectedDay = rule.DayOfMonth.GetValueOrDefault(1),
                            SelectedDayOfTheMonth = rule.DaysToRun.GetValueOrDefault(DaysOfWeek.Monday),
                            SelectedType = rule.OccuranceInMonth
                        }, Formatting.None, JSONHelper.GetDefaultSettings());

                        break;
                }

                Reoccur = rule.Reoccur.GetValueOrDefault(0);
                FailedScheduledJobsCount = rule.FailedScheduledJobsCount;
                SelectedFrequency = rule.Interval.ToString();
                if (rule.LocalTimeOfDay.HasValue)
                {
                    var date = DateTime.Today;
                    var ticks = new DateTime(rule.LocalTimeOfDay.Value.Ticks);
                    date = date.AddHours(ticks.Hour);
                    date = date.AddMinutes(ticks.Minute);
                    ScheduledTime = date.Hour + ":" + date.Minute;
                }
                TimeZoneId = rule.TimeZoneId ?? TimeZoneInfo.Local.Id;  // PN: We assing server time zone when TimeZoneId is empty for compatibility reasons
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

        public string TimeZoneId { get; set; }

        public int FailedScheduledJobsCount { get; set; }

        public static Scheduler Clone(Scheduler template)
        {
            return template != null
                ? new Scheduler
                {
                    EnableScheduler = template.EnableScheduler,
                    EndDate = template.EndDate,
                    TimeZoneOffsetInMinute = template.TimeZoneOffsetInMinute,
                    StartDate = template.StartDate,
                    SelectedFrequency = template.SelectedFrequency,
                    Reoccur = template.Reoccur,
                    ScheduledTime = template.ScheduledTime,
                    SendOn = template.SendOn,
                    TimeZoneId = template.TimeZoneId,
                }
                : null;
        }
    }
}
