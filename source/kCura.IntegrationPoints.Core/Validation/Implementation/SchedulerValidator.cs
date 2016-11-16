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
			var scheduler = value as Scheduler;
			var errorsList = new List<string>();

			ValidateDates(scheduler, errorsList);
			ValidateIntervals(scheduler, errorsList);

			//TODO Aggregate errors
			if (errorsList.Count > 0)
			{
				string delimiter = "; ";
				string errorMessage = "Scheduler validation failed: ";
				errorMessage += string.Join(delimiter, errorMessage);
				return new ValidationResult()
				{
					IsValid = false,
					Message = errorMessage
				};
			}

			return new ValidationResult() { IsValid = true };
		}

		private void ValidateDates(Scheduler scheduler, IList<string> errorsList)
		{
			DateTime tmpDate;

			if (!DateTime.TryParse(scheduler.StartDate, out tmpDate))
			{
				errorsList.Add("StartDate does not contain a valid string representation of a date and time");
			}

			if (!string.IsNullOrEmpty(scheduler.EndDate) &&
				!DateTime.TryParse(scheduler.EndDate, out tmpDate))
			{
				errorsList.Add("EndDate does not contain a valid string representation of a date and time");
			}

			TimeSpan time;
			if (TimeSpan.TryParse(scheduler.ScheduledTime, out time))
			{
				errorsList.Add("ScheduledTime is invalid format");
			}
		}

		private void ValidateIntervals(Scheduler scheduler, IList<string> errorsList)
		{
			List<string> scheduleIntervals = Enum.GetNames(typeof(ScheduleInterval)).ToList();
			if (!scheduleIntervals.Contains(scheduler.SelectedFrequency))
			{
				errorsList.Add("Invalid interval: " + scheduler.SelectedFrequency);
			}

			if (scheduler.SelectedFrequency == ScheduleInterval.Weekly.ToString())
			{
				ValidateReoccur(scheduler, errorsList);
				ValidateWeeklyInterval(scheduler, errorsList);
			}
			else if (scheduler.SelectedFrequency == ScheduleInterval.Monthly.ToString())
			{
				ValidateReoccur(scheduler, errorsList);
				ValidateMonthlyInterval(scheduler, errorsList);
			}
		}

		private void ValidateWeeklyInterval(Scheduler scheduler, IList<string> errorsList)
		{
			var weeklySendOn = _serializer.Deserialize<IntegrationPointService.Weekly>(scheduler.SendOn);

			if (weeklySendOn.SelectedDays.Count == 0)
			{
				errorsList.Add("Any day selected. For Weekly frequency at least one day must be selected.");
				return;
			}

			foreach (string selectedDay in weeklySendOn.SelectedDays)
			{
				ValidateDayOfWeek(selectedDay, "selectedDay", errorsList);
			}
		}

		private void ValidateMonthlyInterval(Scheduler scheduler, IList<string> errorsList)
		{
			const int minDayOfMonth = 1, maxDayOfMonth = 31;
			var monthlySendOn = _serializer.Deserialize<IntegrationPointService.Monthly>(scheduler.SendOn);
			List<string> occurancesInMonth = Enum.GetNames(typeof(OccuranceInMonth)).ToList();

			if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Days)
			{
				if (monthlySendOn.SelectedDay < minDayOfMonth || monthlySendOn.SelectedDay > maxDayOfMonth)
				{
					errorsList.Add($"DayOfMonth ({monthlySendOn.SelectedDay}) is not in range ({minDayOfMonth}:{maxDayOfMonth})");
				}
			}
			else if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Month)
			{
				ValidateDayOfWeek(monthlySendOn.SelectedDayOfTheMonth.ToString(), "SelectedDayOfTheMonth", errorsList);

				if (monthlySendOn.SelectedType == null)
				{
					errorsList.Add("No occurance selected");
				}
				else if (!occurancesInMonth.Contains(monthlySendOn.SelectedType.ToString()))
				{
					errorsList.Add("Invalid occurance selected");
				}
			}
			else
			{
				errorsList.Add("Invalid MonthChoice: " + monthlySendOn.MonthChoice);
			}
		}

		private void ValidateDayOfWeek(string dayOfWeek, string fieldName, IList<string> errorsList)
		{
			if (dayOfWeek == null)
			{
				errorsList.Add("Value for dayOfWeek not selected");
			}
			List<string> daysOfWeek = Enum.GetNames(typeof(DayOfWeek)).ToList();

			if (!daysOfWeek.Contains(dayOfWeek))
			{
				errorsList.Add($"Invalid {fieldName}: {dayOfWeek}");
			}
		}

		private void ValidateReoccur(Scheduler scheduler, IList<string> errorsList)
		{
			const int min = 1, max = 999;
			if (scheduler.Reoccur < min || scheduler.Reoccur > max)
			{
				errorsList.Add($"Reoccur value ({scheduler.Reoccur}) is not in range ({min}:{max})");
			}
		}
	}
}