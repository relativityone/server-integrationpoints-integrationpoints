using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class SchedulerValidator : IValidator
    {
        private readonly ISerializer _serializer;

        public string Key => Constants.IntegrationPointProfiles.Validation.SCHEDULE;

        public const int REOCCUR_MIN = 1, REOCCUR_MAX = 999;
        public const int FIRST_DAY_OF_MONTH = 1, LAST_DAY_OF_MONTH = 31;

        public SchedulerValidator(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public ValidationResult Validate(object value)
        {
            var scheduler = value as Scheduler;
            var result = new ValidationResult();
            if (scheduler == null)
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_NOT_INITIALIZED);
                return result;
            }

            result.Add(ValidateDates(scheduler));
            result.Add(ValidateIntervals(scheduler));

            return result;
        }

        private ValidationResult ValidateDates(Scheduler scheduler)
        {
            const string dateTimeFormat = "M/dd/yyyy";
            string[] timeSpanFormats = { @"hh\:mm", @"h\:m" };

            var result = new ValidationResult();
            var startDate = new DateTime();

            if (scheduler.StartDate.IsNullOrEmpty())
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "StartDate");
            }
            else if (!DateTime.TryParseExact(scheduler.StartDate, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_DATE_FORMAT + scheduler.StartDate);
            }

            if (!string.IsNullOrWhiteSpace(scheduler.EndDate))
            {
                DateTime endDate;
                if (!DateTime.TryParseExact(scheduler.EndDate, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_DATE_FORMAT + scheduler.EndDate);
                }
                else if (startDate > endDate)
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_END_DATE_BEFORE_START_DATE);
                }
            }

            TimeSpan time;
            if (scheduler.ScheduledTime.IsNullOrEmpty())
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "ScheduledTime");
            }
            if (!TimeSpan.TryParseExact(scheduler.ScheduledTime, timeSpanFormats, CultureInfo.InvariantCulture, out time))
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_TIME_FORMAT + scheduler.ScheduledTime);
            }

            return result;
        }

        private ValidationResult ValidateIntervals(Scheduler scheduler)
        {
            var result = new ValidationResult();

            if (scheduler.SelectedFrequency.IsNullOrEmpty())
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "SelectedFrequency");
            }
            else if (!Enum.GetNames(typeof(ScheduleInterval)).Contains(scheduler.SelectedFrequency))
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE + "SelectedFrequency");
            }

            if (scheduler.SelectedFrequency == ScheduleInterval.Weekly.ToString())
            {
                result.Add(ValidateReoccur(scheduler));
                result.Add(ValidateWeeklyInterval(scheduler));
            }
            else if (scheduler.SelectedFrequency == ScheduleInterval.Monthly.ToString())
            {
                result.Add(ValidateReoccur(scheduler));
                result.Add(ValidateMonthlyInterval(scheduler));
            }

            return result;
        }

        private ValidationResult ValidateWeeklyInterval(Scheduler scheduler)
        {
            var result = new ValidationResult();

            Weekly weeklySendOn = _serializer.Deserialize<Weekly>(scheduler.SendOn);

            if (weeklySendOn.SelectedDays.Count == 0)
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "SelectedDays");
            }
            else
            {
                foreach (string selectedDay in weeklySendOn.SelectedDays)
                {
                    result.Add(ValidateDayOfWeek(selectedDay));
                }
            }

            return result;
        }

        private ValidationResult ValidateMonthlyInterval(Scheduler scheduler)
        {
            var result = new ValidationResult();

            Monthly monthlySendOn = _serializer.Deserialize<Monthly>(scheduler.SendOn);
            List<string> occurancesInMonth = Enum.GetNames(typeof(OccuranceInMonth)).ToList();

            if (monthlySendOn.MonthChoice == MonthlyType.Days)
            {
                if (monthlySendOn.SelectedDay < FIRST_DAY_OF_MONTH || monthlySendOn.SelectedDay > LAST_DAY_OF_MONTH)
                {
                    result.Add("DayOfMonth" + IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_NOT_IN_RANGE + $"({FIRST_DAY_OF_MONTH}:{LAST_DAY_OF_MONTH})");
                }
            }
            else if (monthlySendOn.MonthChoice == MonthlyType.Month)
            {
                if (monthlySendOn.SelectedDayOfTheMonth.ToString() != DaysOfWeek.Day.ToString())
                {
                    result.Add(ValidateDayOfWeek(monthlySendOn.SelectedDayOfTheMonth.ToString()));
                }

                if (monthlySendOn.SelectedType == null)
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "SelectedType");
                }
                else if (!occurancesInMonth.Contains(monthlySendOn.SelectedType.ToString()))
                {
                    result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE + "OccuranceInMonth");
                }
            }
            else
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE + "MonthChoice");
            }

            return result;
        }

        private ValidationResult ValidateDayOfWeek(string dayOfWeek)
        {
            var result = new ValidationResult();

            if (dayOfWeek.IsNullOrEmpty())
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_REQUIRED_VALUE + "DayOfWeek");
            }

            if (!Enum.GetNames(typeof(DayOfWeek)).Contains(dayOfWeek))
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_INVALID_VALUE + "DayOfWeek");
            }

            return result;
        }

        private ValidationResult ValidateReoccur(Scheduler scheduler)
        {
            var result = new ValidationResult();

            if (scheduler.Reoccur < REOCCUR_MIN || scheduler.Reoccur > REOCCUR_MAX)
            {
                result.Add("Reoccur" + IntegrationPointProviderValidationMessages.ERROR_SCHEDULER_NOT_IN_RANGE + $"({REOCCUR_MIN}:{REOCCUR_MAX})");
            }

            return result;
        }
    }
}