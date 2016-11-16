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
	public class SchedulerValidator : IProviderValidator
	{
		private readonly Scheduler _scheduler;
		private List<string> _errorsList;
		private readonly ISerializer _serializer;

		public SchedulerValidator(Scheduler scheduler, ISerializer serializer)
		{
			_scheduler = scheduler;
			_serializer = serializer;
		}

		public ValidationResult Validate()
		{
			_errorsList = new List<string>();

			ValidateDates();
			ValidateIntervals();

			//TODO Aggregate errors
			if (_errorsList.Count > 0)
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
			
			return new ValidationResult() {IsValid = true};
		}

#region Private methods
		private void ValidateDates()
		{
			DateTime tmpDate;

			if (!DateTime.TryParse(_scheduler.StartDate, out tmpDate))
			{
				_errorsList.Add("StartDate does not contain a valid string representation of a date and time");
			}

			if (!string.IsNullOrEmpty(_scheduler.EndDate) && 
				!DateTime.TryParse(_scheduler.EndDate, out tmpDate))
			{
				_errorsList.Add("EndDate does not contain a valid string representation of a date and time");
			}

			TimeSpan time;
			if (TimeSpan.TryParse(_scheduler.ScheduledTime, out time))
			{
				_errorsList.Add("ScheduledTime is invalid format");
			}
		}

		private void ValidateIntervals()
		{
			List<string> scheduleIntervals = Enum.GetNames(typeof(ScheduleInterval)).ToList();
			if (!scheduleIntervals.Contains(_scheduler.SelectedFrequency))
			{
				_errorsList.Add("Invalid interval: " + _scheduler.SelectedFrequency);
			}

			if (_scheduler.SelectedFrequency == ScheduleInterval.Weekly.ToString())
			{
				ValidateReoccur();
				ValidateWeeklyInterval();
			}
			else if (_scheduler.SelectedFrequency == ScheduleInterval.Monthly.ToString())
			{
				ValidateReoccur();
				ValidateMonthlyInterval();
			}
		}

		private void ValidateWeeklyInterval()
		{
			var weeklySendOn = _serializer.Deserialize<IntegrationPointService.Weekly>(_scheduler.SendOn);
			
			if (weeklySendOn.SelectedDays.Count == 0)
			{
				_errorsList.Add("Any day selected. For Weekly frequency at least one day must be selected.");
				return;
			}

			foreach (string selectedDay in weeklySendOn.SelectedDays)
			{
				ValidateDayOfWeek(selectedDay, "selectedDay");
			}
		}

		private void ValidateMonthlyInterval()
		{
			const int minDayOfMonth = 1, maxDayOfMonth = 31;
			var monthlySendOn = _serializer.Deserialize<IntegrationPointService.Monthly>(_scheduler.SendOn);
			List<string> occurancesInMonth = Enum.GetNames(typeof(OccuranceInMonth)).ToList();

			if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Days)
			{
				if (monthlySendOn.SelectedDay < minDayOfMonth || monthlySendOn.SelectedDay > maxDayOfMonth)
				{
					_errorsList.Add($"DayOfMonth ({monthlySendOn.SelectedDay}) is not in range ({minDayOfMonth}:{maxDayOfMonth})");
				}
			}
			else if (monthlySendOn.MonthChoice == IntegrationPointService.MonthlyType.Month)
			{
				ValidateDayOfWeek(monthlySendOn.SelectedDayOfTheMonth.ToString(), "SelectedDayOfTheMonth");

				if (monthlySendOn.SelectedType == null)
				{
					_errorsList.Add("No occurance selected");
				}
				else if (!occurancesInMonth.Contains(monthlySendOn.SelectedType.ToString()))
				{
					_errorsList.Add("Invalid occurance selected");
				}
			}
			else
			{
				_errorsList.Add("Invalid MonthChoice: " + monthlySendOn.MonthChoice);
			}
		}

		private void ValidateDayOfWeek(string dayOfWeek, string fieldName)
		{
			if (dayOfWeek == null)
			{
				_errorsList.Add("Value for dayOfWeek not selected");
			}
			List<string> daysOfWeek = Enum.GetNames(typeof(DayOfWeek)).ToList();

			if (!daysOfWeek.Contains(dayOfWeek))
			{
				_errorsList.Add($"Invalid {fieldName}: {dayOfWeek}");
			}
		}

		private void ValidateReoccur()
		{
			const int min = 1, max = 999;
			if (_scheduler.Reoccur < min || _scheduler.Reoccur > max)
			{
				_errorsList.Add($"Reoccur value ({_scheduler.Reoccur}) is not in range ({min}:{max})");
			}
		}
#endregion
	}
}
