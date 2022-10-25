using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using kCura.ScheduleQueue.Core.Helpers;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    [DataContract]
    public class PeriodicScheduleRule : ScheduleRuleBase
    {
        //* Due to Daylight Saving Time (DST) we have to store TimeToRun in local format
        //* For example: if we want to run job always at 12:00pm local time, when converting and storing from CST to UTC in January it will be 6:00pm(UTC). Converting back to local in January it will be 12:00pm(CST) (UTC-6) local, but in July it would be 1:00pm(CST) (UTC-5) due to DST.
        [DataMember]
        private long? localTimeOfDayTicks { get; set; }

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
        public int? FailedScheduledJobsCount { get; set; }

        [DataMember]
        public OccuranceInMonth? OccuranceInMonth { get; set; }

        ///<summary>
        ///LocalTimeOfDay must have Local to Server Time
        ///</summary>
        public TimeSpan? LocalTimeOfDay
        {
            get { return localTimeOfDayTicks.HasValue ? new DateTime(localTimeOfDayTicks.Value, DateTimeKind.Local).TimeOfDay : (TimeSpan?)null; }
            set { localTimeOfDayTicks = value.Value.Ticks; }
        }

        public PeriodicScheduleRule()
            : base()
        {
        }

        public PeriodicScheduleRule(
            ScheduleInterval interval, DateTime startDate, TimeSpan localTimeOfDay,
            DateTime? endDate = null, int? timeZoneOffset = null, DaysOfWeek? daysToRun = null,
            int? dayOfMonth = null, bool? setLastDayOfMonth = null,
            int? reoccur = null, int? failedScheduledJobsCount = 0, OccuranceInMonth? occuranceInMonth = null, string timeZoneId = null)
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

        public override DateTime? GetNextUTCRunDateTime()
        {
            EndDateHelperBase endDateHelper;

            TimeZoneInfo clientTimeZoneInfo = TimeZoneId != null ? 
                TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id == TimeZoneId)
                : TimeZoneInfo.Local;            

            DateTime startDate = StartDate ?? StartDate.GetValueOrDefault(TimeService.UtcNow);            

            //current client local date/time is required for correct calculation of DST change                     
            DateTime clientTimeLocal = TimeService.UtcNow.Date.AddMinutes(LocalTimeOfDay.GetValueOrDefault().TotalMinutes);
            TimeSpan clientUtcOffset = clientTimeZoneInfo.GetUtcOffset(clientTimeLocal);
            DateTime clientTimeUtc = DateTime.SpecifyKind(clientTimeLocal.AddMinutes(-clientUtcOffset.TotalMinutes), DateTimeKind.Utc);            

            //Old sheduler does not have TimeZoneOffSet value so use the local time to adjust the next runtime
            if (TimeZoneOffsetInMinute == null)
            {
                endDateHelper = new LocalEndDate(TimeService);
                endDateHelper.EndDate = EndDate;
                endDateHelper.StartDate = StartDate ?? StartDate.GetValueOrDefault(TimeService.UtcNow);
                endDateHelper.TimeOfDayTick = localTimeOfDayTicks ?? localTimeOfDayTicks.GetValueOrDefault(TimeService.UtcNow.TimeOfDay.Ticks);

                return GetNextRunTimeByInterval(Interval, endDateHelper,
                    DaysToRun, DayOfMonth, SetLastDayOfMonth, Reoccur, OccuranceInMonth);
            }

            DaysOfWeek? daysToRunUtc = AdjustDaysShiftBetweenLocalAndUtc(clientTimeLocal, clientTimeUtc);
            int? dayOfMonth = AdjustDayOfMonthsShiftBetweenLocalAndUtc(clientTimeLocal, clientTimeUtc);

            endDateHelper = new UtcEndDate(TimeService);
            endDateHelper.EndDate = EndDate?.Date.AddMinutes(LocalTimeOfDay.GetValueOrDefault(TimeService.UtcNow.TimeOfDay).TotalMinutes)
                    .AddMinutes(-clientUtcOffset.TotalMinutes);
            endDateHelper.StartDate = clientTimeUtc.Date > endDateHelper.Time.Date ? clientTimeUtc.Date : endDateHelper.Time.Date;
            endDateHelper.TimeOfDayTick = clientTimeUtc.Ticks % TimeSpan.FromDays(1).Ticks;

            DateTime? nextRunTimeUtc = GetNextRunTimeByInterval(Interval, endDateHelper,
                daysToRunUtc, dayOfMonth, SetLastDayOfMonth, Reoccur, OccuranceInMonth);
           
            return AdjustToDaylightSavingOrStandardTime(nextRunTimeUtc, clientTimeZoneInfo,
            clientUtcOffset);
        }

        /// <summary>
        /// Adjust nextRunTime to Daylight Saving Time / Standard Time
        /// </summary>
        /// <returns>Adjusted nextRunTime to DST or Standard</returns>
        private DateTime? AdjustToDaylightSavingOrStandardTime(DateTime? nextRunTimeUtc, TimeZoneInfo clientTimeZoneInfo, 
            TimeSpan clientUtcOffset)
        {
            if (nextRunTimeUtc == null || LocalTimeOfDay == null) { return nextRunTimeUtc; }

            TimeSpan nextRunTimeUtcOffSet = clientTimeZoneInfo.GetUtcOffset((DateTime) nextRunTimeUtc);
            nextRunTimeUtc = nextRunTimeUtc.Value.AddMinutes(clientUtcOffset.TotalMinutes - nextRunTimeUtcOffSet.TotalMinutes);
            return nextRunTimeUtc;
        }


        private int? AdjustDayOfMonthsShiftBetweenLocalAndUtc(DateTime clientTime, DateTime clientTimeUtc)
        {
            if (StartDate == null || LocalTimeOfDay == null || DayOfMonth == null)
            {
                return DayOfMonth;
            }

            int? dayOfMonth = DayOfMonth;

            if (clientTime.DayOfWeek == clientTimeUtc.AddDays(-1).DayOfWeek)
            {
                dayOfMonth = GetNextDayOfMonth(DayOfMonth);
            }
            if (clientTime.DayOfWeek == clientTimeUtc.AddDays(1).DayOfWeek)
            {
                dayOfMonth = GetPreviousDayOfMonth(DayOfMonth);
            }

            return dayOfMonth;
        }

        private static int? GetNextDayOfMonth(int? dayOfMonth)
        {
            const int firstDaysInMonth = 1;
            const int lastDaysInMonth = 31;
            int? nextDay = dayOfMonth + 1;

            return nextDay > lastDaysInMonth ? firstDaysInMonth : nextDay;
        }

        private static int? GetPreviousDayOfMonth(int? dayOfMonth)
        {
            const int firstDaysInMonth = 1;
            const int lastDaysInMonth = 31;
            int? nextDay = dayOfMonth - 1;

            return nextDay < firstDaysInMonth ? lastDaysInMonth : nextDay;
        }

        /// <summary>
        /// Shift days of week if are not corresponding between local and UTC
        /// </summary>
        /// <returns></returns>
        private DaysOfWeek? AdjustDaysShiftBetweenLocalAndUtc(DateTime clientTime, DateTime clientTimeUtc)
        {
            if (StartDate == null || LocalTimeOfDay == null || DaysToRun == null || DaysToRun == DaysOfWeek.Day || DaysToRun == DaysOfWeek.All)
            {
                return DaysToRun;
            }

            List<DayOfWeek> selectedDays = DaysOfWeekConverter.FromDaysOfWeek(DaysToRun.GetValueOrDefault());
            List<DayOfWeek> adjustedDays = new List<DayOfWeek>();

            foreach (DayOfWeek dayToRun in selectedDays)
            {
                adjustedDays.Add(ShiftDayBetweenLocalAndUtc(dayToRun, clientTime, clientTimeUtc));
            }

            return DaysOfWeekConverter.FromDayOfWeek(adjustedDays);
        }

        private DayOfWeek ShiftDayBetweenLocalAndUtc(DayOfWeek dayToRun, DateTime clientTime, DateTime clientTimeUtc)
        {
            if (clientTime.DayOfWeek  == clientTimeUtc.AddDays(-1).DayOfWeek)
            {
                return GetNextWeekday(dayToRun);
            }
            if (clientTime.DayOfWeek == clientTimeUtc.AddDays(1).DayOfWeek)
            {
                return GetPreviousWeekday(dayToRun);
            }

            return dayToRun;
        }

        private static DayOfWeek GetNextWeekday(DayOfWeek day)
        {
            DateTime result = DateTime.Now;
            while (result.DayOfWeek != day)
            {
                result = result.AddDays(1);
            }
            return result.AddDays(1).DayOfWeek;
        }

        private static DayOfWeek GetPreviousWeekday(DayOfWeek day)
        {
            DateTime result = DateTime.Now;
            while (result.DayOfWeek != day)
            {
                result = result.AddDays(1);
            }
            return result.AddDays(-1).DayOfWeek;
        }

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
    }
}