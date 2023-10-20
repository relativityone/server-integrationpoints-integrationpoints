using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using kCura.ScheduleQueue.Core.Helpers;

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

        public override string Description
        {
            get
            {
                var returnValue = new StringBuilder();

                if (StartDate.HasValue)
                {
                    returnValue.Append(string.Format("Recurring job. Scheduled as: starting on {0}", StartDate.Value.ToString("d")));
                }

                if (EndDate.HasValue)
                {
                    returnValue.Append(string.Format(", ending on {0}", EndDate.Value.ToString("d")));
                }
                switch (Interval)
                {
                    case ScheduleInterval.Daily:
                        returnValue.Append(string.Format(", run this job every day"));
                        break;

                    case ScheduleInterval.Weekly:
                        returnValue.Append(string.Format(", run this job every {0}", Reoccur.HasValue && Reoccur.Value > 1 ? string.Format("{0} week(s)", Reoccur.Value) : "week"));
                        if (DaysToRun.HasValue)
                        {
                            returnValue.Append(string.Format(" on {0}", DaysOfWeekToString(DaysToRun.Value)));
                        }
                        break;

                    case ScheduleInterval.Monthly:
                        returnValue.Append(string.Format(", run this job every {0}", Reoccur.HasValue && Reoccur.Value > 1 ? string.Format("{0} month(s)", Reoccur.Value) : "month"));
                        if (DayOfMonth.HasValue)
                        {
                            returnValue.Append(string.Format(" on {0} day", DayOfMonth.Value));
                        }
                        else if (OccuranceInMonth.HasValue)
                        {
                            returnValue.Append(string.Format(" the {0} {1} of the month", OccuranceInMonth.Value.ToString(), DaysOfWeekToString(DaysToRun.Value)));
                        }
                        break;

                    case ScheduleInterval.None:
                        returnValue.Append(", run this job once");
                        break;

                    default:
                        throw new NotImplementedException(
                            "Scheduling rule does not exist on this object, this only supports Daily, Weekly, Monthly");
                }

                if (localTimeOfDayTicks.HasValue)
                {
                    returnValue.Append(string.Format(" at {0} local server time.", DateTime.Now.Date.AddTicks(localTimeOfDayTicks.Value).ToString("t")));
                }
                return returnValue.ToString();
            }
        }

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

        // !!! HANDLE EXCEPTIONS SOMEHOW IN HERE OR IN CALLERS BECAUSE IT COULD LEAD TO CREATION OF MULTIPLE SUBJOBS IE. FOR EXPORT LOAD FILE

        public override DateTime? GetFirstUtcRunDateTime()
        {
            ValidateSchedule();

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
                return dateTime.AddDays(7 - daysOfWeekDifference);
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
            ValidateSchedule();
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

            DateTime lastNextRunDateTimeInTimeZone = lastNextUtcRunDateTime.Add(timeZone.BaseUtcOffset);
            DateTime nextRunDateTimeInTimeZone = AddDateTime(lastNextRunDateTimeInTimeZone);

            DateTime lastRunDateTimeWithDstShift = CalculateDateTimeWithDstShift(timeZone, lastNextUtcRunDateTime, nextRunDateTimeInTimeZone);

            return AddDateTime(lastRunDateTimeWithDstShift);
        }

        private static DateTime CalculateDateTimeWithDstShift(TimeZoneInfo timeZone, DateTime dateTimeUtc, DateTime startDateTimeInTimeZone)
        {
            TimeZoneInfo.AdjustmentRule[] adjustments = timeZone.GetAdjustmentRules();

            if (adjustments.Length == 0)
            {
                return dateTimeUtc;
            }

            int year = dateTimeUtc.Year;
            TimeZoneInfo.AdjustmentRule adjustment = adjustments.FirstOrDefault(adj => adj.DateStart.Year <= year && adj.DateEnd.Year >= year);

            if (adjustment == null)
            {
                return dateTimeUtc;
            }

            bool isDstTimeZoneSplitsInYear = adjustment.DaylightTransitionStart.Month > adjustment.DaylightTransitionEnd.Month;

            TimeZoneInfo.TransitionTime transitionTimeStart = isDstTimeZoneSplitsInYear
                    ? adjustment.DaylightTransitionStart
                    : adjustment.DaylightTransitionEnd;

            TimeZoneInfo.TransitionTime transitionTimeEnd = isDstTimeZoneSplitsInYear
                    ? adjustment.DaylightTransitionEnd
                    : adjustment.DaylightTransitionStart;

            DateTime dstAdjustmentStart = GetAdjustmentDateTime(transitionTimeStart, year);
            DateTime dstAdjustmentEnd = GetAdjustmentDateTime(transitionTimeEnd, year);

            bool isInDstTime = isDstTimeZoneSplitsInYear
                ? dstAdjustmentStart <= startDateTimeInTimeZone || dstAdjustmentEnd > startDateTimeInTimeZone
                : dstAdjustmentStart >= startDateTimeInTimeZone && dstAdjustmentEnd <= startDateTimeInTimeZone;

            if (isInDstTime)
            {
                return dateTimeUtc.Subtract(adjustment.DaylightDelta);
            }

            return dateTimeUtc;
        }

        private static DateTime GetAdjustmentDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
        {
            if (transitionTime.IsFixedDateRule)
            {
                return new DateTime(year, transitionTime.Month, transitionTime.Day);
            }

            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            int startOfWeek = (transitionTime.Week * 7) - 6;
            int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(year, transitionTime.Month, 1));

            int transitionDay;
            int changeDayOfWeek = (int)transitionTime.DayOfWeek;

            if (firstDayOfWeek <= changeDayOfWeek)
            {
                transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
            }
            else
            {
                transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);
            }

            if (transitionDay > cal.GetDaysInMonth(year, transitionTime.Month))
            {
                transitionDay -= 7;
            }

            return new DateTime(
                year,
                transitionTime.Month,
                transitionDay,
                transitionTime.TimeOfDay.Hour,
                transitionTime.TimeOfDay.Minute,
                transitionTime.TimeOfDay.Second);
        }

        private DateTime AddDateTime(DateTime dateTime)
        {
            switch (Interval)
            {
                case ScheduleInterval.Immediate:
                    return dateTime.AddMinutes(3);

                case ScheduleInterval.Hourly:
                    return dateTime.AddHours(1);

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

        private void ValidateSchedule()
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
                throw new ArgumentNullException("Reoccur not specified for scheduler.");
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
                throw new ArgumentNullException("Reoccur not specified for scheduler.");
            }
        }
    }
}
