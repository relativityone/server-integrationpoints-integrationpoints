using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class SchedulerValidator : IValidator
	{
		private readonly ISerializer _serializer;

		public string Key => Constants.IntegrationPoints.Validation.SCHEDULE;

		public SchedulerValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var result = new ValidationResult();

			var scheduler = value as Scheduler;

			result.Add(ValidateDates(scheduler));
			result.Add(ValidateIntervals(scheduler));

			return result;
		}

		private ValidationResult ValidateDates(Scheduler scheduler)
		{
			var result = new ValidationResult();

			DateTime tmpDate;
			if (!DateTime.TryParse(scheduler.StartDate, out tmpDate))
			{
				result.Add("StartDate does not contain a valid string representation of a date and time");
			}

			if (!string.IsNullOrEmpty(scheduler.EndDate) && !DateTime.TryParse(scheduler.EndDate, out tmpDate))
			{
				result.Add("EndDate does not contain a valid string representation of a date and time");
			}

			TimeSpan time;
			if (TimeSpan.TryParse(scheduler.ScheduledTime, out time))
			{
				result.Add("ScheduledTime is invalid format");
			}

			return result;
		}

		private ValidationResult ValidateIntervals(Scheduler scheduler)
		{
			var result = new ValidationResult();

			if (!Enum.GetNames(typeof(ScheduleInterval)).Contains(scheduler.SelectedFrequency))
			{
				result.Add($"Invalid interval: {scheduler.SelectedFrequency}");
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

			var weeklySendOn = _serializer.Deserialize<IntegrationPointService.Weekly>(scheduler.SendOn);
			if (weeklySendOn.SelectedDays.Count == 0)
			{
				result.Add("Any day selected. For Weekly frequency at least one day must be selected.");
			}
			else
			{
				foreach (string selectedDay in weeklySendOn.SelectedDays)
				{
					result.Add(ValidateDayOfWeek(selectedDay, "selectedDay"));
				}
			}

			return result;
		}

		private ValidationResult ValidateMonthlyInterval(Scheduler scheduler)
		{
			var result = new ValidationResult();

			const int minDayOfMonth = 1, maxDayOfMonth = 31;
			var monthlySendOn = _serializer.Deserialize<IntegrationPointService.Monthly>(scheduler.SendOn);
			List<string> occurancesInMonth = Enum.GetNames(typeof(OccuranceInMonth)).ToList();

			if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Days)
			{
				if (monthlySendOn.SelectedDay < minDayOfMonth || monthlySendOn.SelectedDay > maxDayOfMonth)
				{
					result.Add($"DayOfMonth ({monthlySendOn.SelectedDay}) is not in range ({minDayOfMonth}:{maxDayOfMonth})");
				}
			}
			else if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Month)
			{
				result.Add(ValidateDayOfWeek(monthlySendOn.SelectedDayOfTheMonth.ToString(), "SelectedDayOfTheMonth"));

				if (monthlySendOn.SelectedType == null)
				{
					result.Add("No occurance selected");
				}
				else if (!occurancesInMonth.Contains(monthlySendOn.SelectedType.ToString()))
				{
					result.Add("Invalid occurance selected");
				}
			}
			else
			{
				result.Add($"Invalid MonthChoice: {monthlySendOn.MonthChoice}");
			}

			return result;
		}

		private ValidationResult ValidateDayOfWeek(string dayOfWeek, string fieldName)
		{
			var result = new ValidationResult();

			if (dayOfWeek == null)
			{
				result.Add("Value for dayOfWeek not selected");
			}

			if (!Enum.GetNames(typeof(DayOfWeek)).Contains(dayOfWeek))
			{
				result.Add($"Invalid {fieldName}: {dayOfWeek}");
			}

			return result;
		}

		private ValidationResult ValidateReoccur(Scheduler scheduler)
		{
			var result = new ValidationResult();

			const int min = 1, max = 999;
			if (scheduler.Reoccur < min || scheduler.Reoccur > max)
			{
				result.Add($"Reoccur value ({scheduler.Reoccur}) is not in range ({min}:{max})");
			}

			return result;
		}
	}
}