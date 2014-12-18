using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
	[Serializable]
	public abstract class ScheduleRuleBase : IScheduleRule
	{
		//Required by Serializable
		protected ScheduleRuleBase()
		{
		}

		[NonSerialized()]
		private static ISerializer _serializer;
		[XmlIgnore]
		public static ISerializer Serializer
		{
			set { _serializer = value; }
			get
			{
				if (_serializer == null)
					_serializer = new XMLSerializerFactory();

				return _serializer;
			}
		}

		[NonSerialized()]
		private ITimeService _timeService = null;
		[XmlIgnore]
		public ITimeService TimeService
		{
			set { _timeService = value; }
			get
			{
				if (_timeService == null)
					_timeService = new DefaultTimeService();

				return _timeService;
			}
		}

		[NonSerialized()]
		private static Dictionary<DaysOfWeek, DayOfWeek> _daysOfWeekMap = null;
		[XmlIgnore]
		public static Dictionary<DaysOfWeek, DayOfWeek> DaysOfWeekMap
		{
			set { _daysOfWeekMap = value; }
			get
			{
				if (_daysOfWeekMap == null)
				{
					_daysOfWeekMap = new Dictionary<DaysOfWeek, DayOfWeek>()
					{
						{DaysOfWeek.Monday,DayOfWeek.Monday},
						{DaysOfWeek.Tuesday,DayOfWeek.Tuesday},
						{DaysOfWeek.Wednesday,DayOfWeek.Wednesday},
						{DaysOfWeek.Thursday,DayOfWeek.Thursday},
						{DaysOfWeek.Friday,DayOfWeek.Friday},
						{DaysOfWeek.Saturday,DayOfWeek.Saturday},
						{DaysOfWeek.Sunday,DayOfWeek.Sunday},
					};
				}
				return _daysOfWeekMap;
			}
		}

		public abstract DateTime? GetNextUTCRunDateTime(DateTime? LastRunTime = null, TaskStatusEnum? lastTaskStatus = null);
		public abstract string Description { get; }
		public string ToSerializedString()
		{
			return Serializer.Serialize(this);
		}

		public static T Deserialize<T>(string serializedString)
		{
			return Serializer.Deserialize<T>(serializedString);
		}

		///<summary>
		///Date/Time is returned in UTC
		///</summary>
		public DateTime? GetNextRunTimeByInterval(ScheduleInterval Interval, DateTime StartDate, long localTimeOfDayTicks, DaysOfWeek? DaysToRun, int? DayOfMonth, bool? SetLastDayOfMonth, DateTime? EndDate, int? reoccur, OccuranceInMonth? occuranceInMonth)
		{
			//* Due to Daylight Saving Time (DST) all calculations are done with local date/time to insure final time corresponds to Scheduled time
			//* However, returning value in UTC, since Method Agent framework operates in UTC.

			DateTime localNow = TimeService.UtcNow.ToLocalTime();
			DateTime nextRunTimeDate = StartDate.Date > localNow.Date ? StartDate.Date : localNow.Date;
			nextRunTimeDate = nextRunTimeDate.AddTicks(localTimeOfDayTicks);
			switch (Interval)
			{
				case ScheduleInterval.Immediate:
					if (nextRunTimeDate < localNow)
					{ nextRunTimeDate = localNow; }
					nextRunTimeDate = nextRunTimeDate.AddMinutes(3);
					break;
				case ScheduleInterval.Hourly:
					if (nextRunTimeDate < localNow)
					{
						nextRunTimeDate = new DateTime(localNow.Year, localNow.Month, localNow.Day, localNow.Hour, nextRunTimeDate.Minute, nextRunTimeDate.Second);
						if (nextRunTimeDate < localNow) nextRunTimeDate = nextRunTimeDate.AddHours(1);
					}
					break;
				case ScheduleInterval.Daily:
					if (nextRunTimeDate < localNow)
					{ nextRunTimeDate = nextRunTimeDate.AddDays(1); }
					break;
				case ScheduleInterval.Weekly:
					if (nextRunTimeDate < localNow)
					{ nextRunTimeDate = nextRunTimeDate.AddDays(1); }
					nextRunTimeDate = GetNextScheduledWeekDay(DaysToRun.Value, nextRunTimeDate, localNow, reoccur);
					break;
				case ScheduleInterval.Monthly:
					CheckForOverride(ref SetLastDayOfMonth, ref DayOfMonth, DaysToRun, occuranceInMonth);

					if ((SetLastDayOfMonth.HasValue && SetLastDayOfMonth.Value) || DayOfMonth.HasValue)
					{
						nextRunTimeDate = GetNextScheduledMonthDayByDay(nextRunTimeDate, StartDate, localTimeOfDayTicks, SetLastDayOfMonth, DayOfMonth, localNow, reoccur);
					}
					else
					{
						nextRunTimeDate = GetNextScheduledMonthDayByWeek(nextRunTimeDate, StartDate, localTimeOfDayTicks, localNow, DaysToRun, reoccur, occuranceInMonth);
					}
					break;
			}
			if (EndDate.HasValue && EndDate.Value.AddDays(1).Date <= nextRunTimeDate.Date)
				return null;
			else
				return nextRunTimeDate.ToUniversalTime();
		}

		public DateTime GetNextScheduledWeekDay(DaysOfWeek scheduleDayOfWeek, DateTime nextRunTimeDate, DateTime localNow, int? reoccur)
		{
			bool endOfTheWeekReached = false;
			DaysOfWeek localNowDayOfWeek = DaysOfWeekMap.Where(x => x.Value == localNow.DayOfWeek).ToList()[0].Key;
			DaysOfWeek nextDayOfWeek = DaysOfWeekMap.Where(x => x.Value == nextRunTimeDate.DayOfWeek).ToList()[0].Key;
			if (localNowDayOfWeek.CompareTo(nextDayOfWeek) == 1) endOfTheWeekReached = true;
			int i;
			for (i = 0; i < 8; i++)
			{
				if ((scheduleDayOfWeek & nextDayOfWeek) == nextDayOfWeek) break;
				nextDayOfWeek = (DaysOfWeek)((byte)nextDayOfWeek << 1);
				if ((nextDayOfWeek & DaysOfWeek.All) == DaysOfWeek.None)
				{
					nextDayOfWeek = (DaysOfWeek)1;
					endOfTheWeekReached = true;
				}
			}
			if (endOfTheWeekReached && reoccur.HasValue && reoccur.Value > 1)
			{
				i = i + ((reoccur.Value - 1) * 7);
			}
			return nextRunTimeDate.AddDays(i);
		}

		public DateTime GetNextScheduledMonthDayByDay(DateTime nextRunTimeDate, DateTime StartDate, long localTimeOfDayTicks, bool? SetLastDayOfMonth, int? DayOfMonth, DateTime localNow, int? reoccur)
		{
			int year = nextRunTimeDate.Year;
			int month = nextRunTimeDate.Month;
			int numberOfDaysInTheMonth = DateTime.DaysInMonth(year, month);
			int dayOfMonth = localNow.Day;

			if (SetLastDayOfMonth.HasValue && SetLastDayOfMonth.Value)
			{
				dayOfMonth = numberOfDaysInTheMonth;
			}
			else if (DayOfMonth.HasValue)
			{
				dayOfMonth = numberOfDaysInTheMonth < DayOfMonth.Value ? numberOfDaysInTheMonth : DayOfMonth.Value;
			}
			nextRunTimeDate = new DateTime(year, month, dayOfMonth, nextRunTimeDate.Hour, nextRunTimeDate.Minute, nextRunTimeDate.Second);
			if (nextRunTimeDate < localNow || nextRunTimeDate < StartDate.AddTicks(localTimeOfDayTicks))
			{
				int monthReoccurance = 1;
				if (reoccur.HasValue) monthReoccurance = reoccur.Value;
				nextRunTimeDate = nextRunTimeDate.AddMonths(monthReoccurance);
				year = nextRunTimeDate.Year;
				month = nextRunTimeDate.Month;
				numberOfDaysInTheMonth = DateTime.DaysInMonth(year, month);
				if (SetLastDayOfMonth.HasValue && SetLastDayOfMonth.Value)
				{ dayOfMonth = numberOfDaysInTheMonth; }
				else
				{ dayOfMonth = numberOfDaysInTheMonth < DayOfMonth.Value ? numberOfDaysInTheMonth : DayOfMonth.Value; }
				nextRunTimeDate = new DateTime(year, month, dayOfMonth, nextRunTimeDate.Hour, nextRunTimeDate.Minute, nextRunTimeDate.Second);
			}
			return nextRunTimeDate;
		}

		///<summary>
		///This function finds day occurance in the particular month.
		///For example, find Second occurance of Monday in October 2014:
		/// (year=2014, month=10, occurance=2, DayOfWeek.Monday)
		/// will return October, 13 2014
		///</summary>
		public DateTime SearchMonthForForwardOccuranceOfDay(int year, int month, ForwardValidOccurance occurance, DayOfWeek dayOfWeekToFind)
		{
			DateTime dt = new DateTime(year, month, 1);
			int daysInMonth = DateTime.DaysInMonth(year, month);
			int dayOccurance = 0;
			for (int i = 1; i <= daysInMonth; i++)
			{
				dt = new DateTime(year, month, i);
				if (dt.DayOfWeek == dayOfWeekToFind)
				{
					dayOccurance++;
					if (dayOccurance == (int)occurance)
					{
						//found the next run date/time
						break;
					}
					else
					{
						//skip to the right occurance right the way
						dt = dt.AddDays((((int)occurance - dayOccurance) * 7) - 1);
					}
				}
			}

			return dt;
		}

		///<summary>
		///This function finds last occurance of a day in the particular month.
		///For example, find last occurance of Monday in October 2014:
		/// (year=2014, month=10, DayOfWeek.Monday)
		/// will return October, 27 2014
		///</summary>
		public DateTime SearchMonthForLastOccuranceOfDay(int year, int month, DayOfWeek dayOfWeekToFind)
		{
			DateTime dt = new DateTime(year, month, 1);
			int daysInMonth = DateTime.DaysInMonth(year, month);
			for (int i = daysInMonth; i > 1; i--)
			{
				dt = new DateTime(year, month, i);
				if (dt.DayOfWeek == dayOfWeekToFind)
				{
					//found the next run date/time
					break;
				}
			}

			return dt;
		}

		private void CheckForOverride(ref bool? SetLastDayOfMonth, ref int? DayOfMonth, DaysOfWeek? DaysToRun, OccuranceInMonth? occuranceInMonth)
		{
			if (DaysToRun.HasValue && ((DaysToRun & DaysOfWeek.Day) == DaysOfWeek.Day) && occuranceInMonth.HasValue)
			{
				switch (occuranceInMonth.Value)
				{
					case OccuranceInMonth.First:
						DayOfMonth = 1;
						break;
					case OccuranceInMonth.Second:
						DayOfMonth = 2;
						break;
					case OccuranceInMonth.Third:
						DayOfMonth = 3;
						break;
					case OccuranceInMonth.Fourth:
						DayOfMonth = 4;
						break;
					case OccuranceInMonth.Last:
						SetLastDayOfMonth = true;
						break;
				}
			}
		}

		public DateTime GetNextScheduledMonthDayByWeek(DateTime nextRunTimeDate, DateTime StartDate, long localTimeOfDayTicks, DateTime localNow, DaysOfWeek? DaysToRun, int? reoccur, OccuranceInMonth? occuranceInMonth)
		{
			int reoccurance = 1;
			if (reoccur.HasValue && reoccur.Value > 1) reoccurance = reoccur.Value;
			bool continueSearch = false;
			DayOfWeek dayOfWeekToRun = DaysOfWeekMap[DaysToRun.Value];

			do
			{
				continueSearch = false;
				if (occuranceInMonth.Value == OccuranceInMonth.Last)
				{
					DateTime dt = SearchMonthForLastOccuranceOfDay(nextRunTimeDate.Year, nextRunTimeDate.Month, dayOfWeekToRun);
					nextRunTimeDate = new DateTime(dt.Year, dt.Month, dt.Day, nextRunTimeDate.Hour, nextRunTimeDate.Minute, nextRunTimeDate.Second);
				}
				else
				{
					DateTime dt = SearchMonthForForwardOccuranceOfDay(nextRunTimeDate.Year, nextRunTimeDate.Month, (ForwardValidOccurance)(int)occuranceInMonth, dayOfWeekToRun);
					nextRunTimeDate = new DateTime(dt.Year, dt.Month, dt.Day, nextRunTimeDate.Hour, nextRunTimeDate.Minute, nextRunTimeDate.Second);
				}
				if (nextRunTimeDate < localNow || nextRunTimeDate.Date < StartDate.Date)
				{
					//need to move to future month
					nextRunTimeDate = nextRunTimeDate.AddMonths(reoccurance);
					continueSearch = true;
				}
			} while (continueSearch);

			return nextRunTimeDate;
		}

		public static string DaysOfWeekToString(DaysOfWeek daysOfWeek)
		{
			string weekDays = string.Empty;
			DaysOfWeek nextDayOfWeek = (DaysOfWeek)1;
			for (int i = 0; i < 8; i++)
			{
				if ((daysOfWeek & nextDayOfWeek) == nextDayOfWeek) weekDays += Enum.GetName(typeof(DaysOfWeek), nextDayOfWeek) + ", ";
				nextDayOfWeek = (DaysOfWeek)((byte)nextDayOfWeek << 1);
				if ((nextDayOfWeek & DaysOfWeek.All) == DaysOfWeek.None) break;
			}
			if (!string.IsNullOrEmpty(weekDays)) weekDays = weekDays.Substring(0, weekDays.Length - 2);
			return weekDays;
		}
	}
}
