using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using kCura.ScheduleQueue.Core.Helpers;
using Relativity.Services.TimeZone;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    [DataContract]
    public class PeriodicScheduleRule : ScheduleRuleBase
    {
        [DataMember]
        public ScheduleInterval Interval { get; set; }

        [DataMember]
        public DateTime? StartDate { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public int? TimeZoneOffsetInMinute { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public int? DayOfMonth { get; set; }

        [DataMember]
        public bool? SetLastDayOfMonth { get; set; }

        [DataMember]
        public DaysOfWeek? DaysToRun { get; set; }

        [DataMember]
        public int? Reoccur { get; set; }

        [DataMember]
        public int FailedScheduledJobsCount { get; set; }

        [DataMember]
        public OccuranceInMonth? OccuranceInMonth { get; set; }

        /// <summary>
        /// LocalTimeOfDay must have Local to Server Time
        /// </summary>
        public TimeSpan? LocalTimeOfDay
        {
            get { return localTimeOfDayTicks.HasValue ? new DateTime(localTimeOfDayTicks.Value, DateTimeKind.Local).TimeOfDay : (TimeSpan?)null; }
            set { localTimeOfDayTicks = value.Value.Ticks; }
        }

        // * Due to Daylight Saving Time (DST) we have to store TimeToRun in local format
        // * For example: if we want to run job always at 12:00pm local time, when converting and storing from CST to UTC in January it will be 6:00pm(UTC). Converting back to local in January it will be 12:00pm(CST) (UTC-6) local, but in July it would be 1:00pm(CST) (UTC-5) due to DST.
        [DataMember]
        private long? localTimeOfDayTicks { get; set; }

        public PeriodicScheduleRule()
        {
        }

        public PeriodicScheduleRule(
            ScheduleInterval interval,
            DateTime startDate,
            TimeSpan localTimeOfDay,
            DateTime? endDate = null,
            int? timeZoneOffset = null,
            DaysOfWeek? daysToRun = null,
            int? dayOfMonth = null,
            bool? setLastDayOfMonth = null,
            int? reoccur = null,
            int failedScheduledJobsCount = 0,
            OccuranceInMonth? occuranceInMonth = null,
            string timeZoneId = null)
            : this()
        {
            Interval = interval;
            StartDate = startDate;
            LocalTimeOfDay = localTimeOfDay;
            EndDate = endDate;
            DaysToRun = daysToRun;
            DayOfMonth = dayOfMonth;
            SetLastDayOfMonth = setLastDayOfMonth;
            Reoccur = reoccur;
            FailedScheduledJobsCount = failedScheduledJobsCount;
            TimeZoneOffsetInMinute = timeZoneOffset;
            OccuranceInMonth = occuranceInMonth;
            TimeZoneId = timeZoneId;
        }

        public override int GetNumberOfContinuouslyFailedScheduledJobs()
        {
            return FailedScheduledJobsCount;
        }

        public override void IncrementConsecutiveFailedScheduledJobsCount()
        {
            ++FailedScheduledJobsCount;
        }

        public override void ResetConsecutiveFailedScheduledJobsCount()
        {
            FailedScheduledJobsCount = 0;
        }

        public override DateTime? GetFirstUtcRunDateTime()
        {
            ValidateGetFirstUtcRunDateTime();

            DateTime startDateTimeInTimeZone = StartDate.GetValueOrDefault().AddMinutes(LocalTimeOfDay.GetValueOrDefault().TotalMinutes);
            if (Interval == ScheduleInterval.Weekly)
            {
                startDateTimeInTimeZone = CalculateFirstDateTimeForWeeklyWorkflow(startDateTimeInTimeZone);
            }

            if (Interval == ScheduleInterval.Monthly)
            {
                startDateTimeInTimeZone = CalculateFirstDateTimeForMonthlyWorkflow(startDateTimeInTimeZone);
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            DateTime startDateTimeUtc = startDateTimeInTimeZone.Subtract(timeZone.BaseUtcOffset);

            return CalculateDateTimeWithDstShift(timeZone, startDateTimeUtc, startDateTimeInTimeZone);
        }

        public override DateTime? GetNextUtcRunDateTime(DateTime lastNextUtcRunDateTime)
        {
            DateTime nextUtcRunDateTime = CalculateNextUtcRunDateTime(lastNextUtcRunDateTime);

            if (EndDate.HasValue && EndDate < new DateTime(nextUtcRunDateTime.Year, nextUtcRunDateTime.Month, nextUtcRunDateTime.Day))
            {
                return null;
            }

            return nextUtcRunDateTime;
        }

        private DateTime CalculateFirstDateTimeForWeeklyWorkflow(DateTime dateTime)
        {
            ValidateWeeklyWorkflow();
            int dayOfWeek = DaysOfWeekConverter.DayOfWeekToIndex(dateTime.DayOfWeek);
            List<int> selectedDaysOfWeek = DaysOfWeekConverter.FromDaysOfWeek(DaysToRun.GetValueOrDefault()).Select(x => DaysOfWeekConverter.DayOfWeekToIndex(x)).ToList();

            if (dayOfWeek >= selectedDaysOfWeek.Max())
            {
                int daysOfWeekDifference = dayOfWeek - selectedDaysOfWeek.Min();
                return dateTime.AddDays((Reoccur.GetValueOrDefault() * 7) - daysOfWeekDifference);
            }

            int nextDatDayOfWeek = selectedDaysOfWeek.First(x => x > dayOfWeek);
            int todayAndNextDayOfWeekDifference = nextDatDayOfWeek - dayOfWeek;

            return dateTime.AddDays(todayAndNextDayOfWeekDifference);
        }

        private DateTime CalculateFirstDateTimeForMonthlyWorkflow(DateTime dateTime)
        {
            ValidateMonthlyWorkflow();
            int lastDayOfMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            int dayOfMonth = DayOfMonth.GetValueOrDefault() >= lastDayOfMonth ? lastDayOfMonth : DayOfMonth.GetValueOrDefault();
            DateTime scheduledDateTime = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dayOfMonth,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);

            if (dateTime > scheduledDateTime)
            {
                return scheduledDateTime.AddMonths(Reoccur.GetValueOrDefault());
            }

            return scheduledDateTime;
        }

        private DateTime CalculateNextUtcRunDateTime(DateTime lastNextUtcRunDateTime)
        {
            if (string.IsNullOrEmpty(TimeZoneId))
            {
                throw new ArgumentNullException("Time Zone should be set to schedule a job.");
            }

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            DateTime lastNextRunDateTimeInTimeZone = lastNextUtcRunDateTime.Add(timeZone.BaseUtcOffset);

            if (timeZone.IsDaylightSavingTime(lastNextRunDateTimeInTimeZone))
            {
                TimeZoneInfo.AdjustmentRule adjustment = GetAdjustmentRule(lastNextRunDateTimeInTimeZone, timeZone.GetAdjustmentRules());
                lastNextRunDateTimeInTimeZone = lastNextRunDateTimeInTimeZone.Add(adjustment.DaylightDelta);
                lastNextUtcRunDateTime = lastNextUtcRunDateTime.Add(adjustment.DaylightDelta);
            }

            DateTime nextRunDateTimeInTimeZone = AddDateTime(lastNextRunDateTimeInTimeZone);
            DateTime lastRunDateTimeWithDstShift = CalculateDateTimeWithDstShift(timeZone, lastNextUtcRunDateTime, nextRunDateTimeInTimeZone);

            return AddDateTime(lastRunDateTimeWithDstShift);
        }

        private static DateTime CalculateDateTimeWithDstShift(TimeZoneInfo timeZone, DateTime dateTimeUtc, DateTime dateTimeInTimeZone)
        {
            TimeZoneInfo.AdjustmentRule[] adjustments = timeZone.GetAdjustmentRules();
            if (adjustments.Length == 0)
            {
                return dateTimeUtc;
            }

            TimeZoneInfo.AdjustmentRule adjustment = GetAdjustmentRule(dateTimeInTimeZone, adjustments);
            if (adjustment == null)
            {
                return dateTimeUtc;
            }

            bool isInDstTime = timeZone.IsDaylightSavingTime(dateTimeInTimeZone);
            if (isInDstTime)
            {
                return dateTimeUtc.Subtract(adjustment.DaylightDelta);
            }

            return dateTimeUtc;
        }

        private static TimeZoneInfo.AdjustmentRule GetAdjustmentRule(DateTime startDateTimeInTimeZone, TimeZoneInfo.AdjustmentRule[] adjustments)
        {
            int year = startDateTimeInTimeZone.Year;
            TimeZoneInfo.AdjustmentRule adjustment =
                adjustments.FirstOrDefault(adj => adj.DateStart.Year <= year && adj.DateEnd.Year >= year);
            return adjustment;
        }

        private DateTime AddDateTime(DateTime dateTime)
        {
            switch (Interval)
            {
                case ScheduleInterval.Daily:
                    return dateTime.AddDays(1);

                case ScheduleInterval.Weekly:
                    return CalculateNextDateTimeForWeeklyWorkflow(dateTime);

                case ScheduleInterval.Monthly:
                    ValidateMonthlyWorkflow();
                    return dateTime.AddMonths(Reoccur.GetValueOrDefault());
                default:
                    return dateTime;
            }
        }

        private DateTime CalculateNextDateTimeForWeeklyWorkflow(DateTime dateTime)
        {
            ValidateWeeklyWorkflow();
            int dayOfWeek = DaysOfWeekConverter.DayOfWeekToIndex(dateTime.DayOfWeek);
            List<int> selectedDaysOfWeek = DaysOfWeekConverter.FromDaysOfWeek(DaysToRun.GetValueOrDefault()).Select(x => DaysOfWeekConverter.DayOfWeekToIndex(x)).ToList();

            if (dayOfWeek >= selectedDaysOfWeek.Max())
            {
                int daysOfWeekDifference = dayOfWeek - selectedDaysOfWeek.Min();
                return dateTime.AddDays((Reoccur.GetValueOrDefault() * 7) - daysOfWeekDifference);
            }

            int nextDatDayOfWeek = selectedDaysOfWeek.First(x => x > dayOfWeek);
            int todayAndNextDayOfWeekDifference = nextDatDayOfWeek - dayOfWeek;

            return dateTime.AddDays(todayAndNextDayOfWeekDifference);
        }

        private void ValidateGetFirstUtcRunDateTime()
        {
            if (!StartDate.HasValue)
            {
                throw new ArgumentNullException("Start Date should be set to schedule a job.");
            }

            if (!LocalTimeOfDay.HasValue)
            {
                throw new ArgumentNullException("Local Time of day should be set to schedule a job.");
            }

            if (string.IsNullOrEmpty(TimeZoneId))
            {
                throw new ArgumentNullException("Time Zone should be set to schedule a job.");
            }
        }

        private void ValidateWeeklyWorkflow()
        {
            if (!DaysToRun.HasValue)
            {
                throw new ArgumentNullException("Days of a week not specified for scheduler.");
            }

            if (!Reoccur.HasValue)
            {
                Reoccur = 1;
            }
        }

        private void ValidateMonthlyWorkflow()
        {
            if (!DayOfMonth.HasValue)
            {
                throw new ArgumentNullException("Days of a month not specified for scheduler.");
            }

            if (!Reoccur.HasValue)
            {
                Reoccur = 1;
            }
        }
    }
}
