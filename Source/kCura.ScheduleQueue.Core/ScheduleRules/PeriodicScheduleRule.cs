﻿using System;
using System.Runtime.Serialization;
using System.Text;

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

		[DataMember(EmitDefaultValue = true)]
		public int UtcDateOffSet { get; set; }

		[DataMember]
		public int? DayOfMonth { get; set; }
		[DataMember]
		public bool? SetLastDayOfMonth { get; set; }
		[DataMember]
		public DaysOfWeek? DaysToRun { get; set; }
		[DataMember]
		public int? Reoccur { get; set; }
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
			DateTime? endDate = null, int utcDateTimeOffSet = 0, DaysOfWeek? daysToRun = null,
			int? dayOfMonth = null, bool? setLastDayOfMonth = null,
			int? reoccur = null, OccuranceInMonth? occuranceInMonth = null
			)
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
			UtcDateOffSet = utcDateTimeOffSet;
			OccuranceInMonth = occuranceInMonth;
		}

		public override DateTime? GetNextUTCRunDateTime(DateTime? lastRunTime = null, TaskStatusEnum? lastTaskStatus = null)
		{
			return GetNextRunTimeByInterval(Interval, StartDate.GetValueOrDefault(DateTime.UtcNow),
				localTimeOfDayTicks.GetValueOrDefault(DateTime.UtcNow.TimeOfDay.Ticks),
				DaysToRun, DayOfMonth, SetLastDayOfMonth, EndDate, Reoccur, OccuranceInMonth);
		}

		public override string Description
		{
			get
			{
				var returnValue = new StringBuilder();

				if (StartDate.HasValue) returnValue.Append(string.Format("Recurring job. Scheduled as: starting on {0}", StartDate.Value.ToString("d")));
				if (EndDate.HasValue) returnValue.Append(string.Format(", ending on {0}", EndDate.Value.ToString("d")));
				switch (Interval)
				{
					case ScheduleInterval.Daily:
						returnValue.Append(string.Format(", run this job every day"));
						break;
					case ScheduleInterval.Weekly:
						returnValue.Append(string.Format(", run this job every {0}", Reoccur.HasValue && Reoccur.Value > 1 ? string.Format("{0} week(s)", Reoccur.Value) : "week"));
						if (DaysToRun.HasValue) returnValue.Append(string.Format(" on {0}", DaysOfWeekToString(DaysToRun.Value)));
						break;
					case ScheduleInterval.Monthly:
						returnValue.Append(string.Format(", run this job every {0}", Reoccur.HasValue && Reoccur.Value > 1 ? string.Format("{0} month(s)", Reoccur.Value) : "month"));
						if (DayOfMonth.HasValue) returnValue.Append(string.Format(" on {0} day", DayOfMonth.Value));
						else if (OccuranceInMonth.HasValue) returnValue.Append(string.Format(" the {0} {1} of the month", OccuranceInMonth.Value.ToString(), DaysOfWeekToString(DaysToRun.Value)));
						break;
					case ScheduleInterval.None:
						returnValue.Append(", run this job once");
						break;
					default:
						throw new NotImplementedException(
							"Scheduling rule does not exist on this object, this only supports Daily, Weekly, Monthly");
				}
				if (localTimeOfDayTicks.HasValue) returnValue.Append(string.Format(" at {0} local server time.", DateTime.Now.Date.AddTicks(localTimeOfDayTicks.Value).ToString("t")));
				return returnValue.ToString();
			}
		}
	}
}
