using System;
using System.Runtime.Serialization;
using kCura.Apps.Common.Utils.Serializers;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    [DataContract]
    public class ScheduleRuleTest
    {
        //TODO: VALIDATE WHICH PROPERTIES ARE NEEDED

        [DataMember]
        public int? DayOfMonth { get; set; }

        [DataMember]
        public DaysOfWeek? DaysToRun { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public ScheduleInterval Interval { get; set; }

        [DataMember]
        public OccuranceInMonth? OccurenceInMonth { get; set; }

        [DataMember]
        public int? Reoccur { get; set; } 

        [DataMember]
        public bool? SetLastDayOfMonth { get; set; }

        [DataMember]
        public DateTime? StartDate { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public int? TimeZoneOffsetInMinute { get; set; }

        public static ScheduleRuleTest CreateDailyRule(DateTime startDateTime, DateTime endDateTime,
            TimeZoneInfo timeZone)
        {
            return new ScheduleRuleTest
            {
                EndDate = endDateTime,
                Interval = ScheduleInterval.Daily,
                Reoccur = 1,
                StartDate = startDateTime,
                TimeZoneId = timeZone.Id,
                TimeZoneOffsetInMinute = timeZone.GetUtcOffset(startDateTime).Minutes
            };
        }

        public static ScheduleRuleTest CreateWeeklyRule(DateTime startDateTime, DateTime endDateTime,
            TimeZoneInfo timeZone, DaysOfWeek daysToRun, int everyWeek = 1)
        {
            return new ScheduleRuleTest
            {
                DaysToRun = daysToRun,
                EndDate = endDateTime,
                Interval = ScheduleInterval.Weekly,
                Reoccur = everyWeek,
                StartDate = startDateTime,
                TimeZoneId = timeZone.Id,
                TimeZoneOffsetInMinute = timeZone.GetUtcOffset(startDateTime).Minutes
            };
        }

        public static ScheduleRuleTest CreateMonthlyRule(DateTime startDateTime, DateTime endDateTime,
            TimeZoneInfo timeZone, int dayOfMonth, int everyMonth = 1)
        {
            return new ScheduleRuleTest
            {
                DayOfMonth = dayOfMonth,
                EndDate = endDateTime,
                Interval = ScheduleInterval.Monthly,
                Reoccur = everyMonth,
                StartDate = startDateTime,
                TimeZoneId = timeZone.Id,
                TimeZoneOffsetInMinute = timeZone.GetUtcOffset(startDateTime).Minutes
            };
        }

        public string Serialize()
        {
            var ruleToSerialize = new PeriodicScheduleRule
            {
                LocalTimeOfDay = TimeSpan.Zero,
                Interval = Interval,
                StartDate = StartDate,
                EndDate = EndDate,
                TimeZoneOffsetInMinute = TimeZoneOffsetInMinute,
                TimeZoneId = TimeZoneId,
                DayOfMonth = DayOfMonth,
                SetLastDayOfMonth = SetLastDayOfMonth,
                DaysToRun = DaysToRun,
                Reoccur = Reoccur,
                OccuranceInMonth = OccurenceInMonth
            };

            var serializer = new XMLSerializerFactory();

            return serializer.Serialize(ruleToSerialize);
        }
    }
}
