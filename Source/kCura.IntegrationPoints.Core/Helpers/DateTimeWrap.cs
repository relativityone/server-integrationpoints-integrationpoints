using System;
using System.Globalization;
using SystemInterface;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public class DateTimeWrap : IDateTime
	{
		private readonly DateTime _dateTime;

		public DateTimeWrap(DateTime dateTime)
		{
			_dateTime = dateTime;
		}

		public DateTime DateTimeInstance => _dateTime;

		#region Not Implemented

		public IDateTime Date => throw new NotImplementedException();

		public int Day => throw new NotImplementedException();

		public DayOfWeek DayOfWeek => throw new NotImplementedException();

		public int DayOfYear => throw new NotImplementedException();

		public int Hour => throw new NotImplementedException();

		public DateTimeKind Kind => throw new NotImplementedException();

		public int Millisecond => throw new NotImplementedException();

		public int Minute => throw new NotImplementedException();

		public int Month => throw new NotImplementedException();

		public IDateTime Now => throw new NotImplementedException();

		public int Second => throw new NotImplementedException();

		public long Ticks => throw new NotImplementedException();

		public TimeSpan TimeOfDay => throw new NotImplementedException();

		public IDateTime Today => throw new NotImplementedException();

		public IDateTime UtcNow => throw new NotImplementedException();

		public int Year => throw new NotImplementedException();

		public IDateTime Add(TimeSpan value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddDays(double value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddHours(double value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddMilliseconds(double value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddMinutes(double value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddMonths(int months)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddSeconds(double value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddTicks(long value)
		{
			throw new NotImplementedException();
		}

		public IDateTime AddYears(int value)
		{
			throw new NotImplementedException();
		}

		public int Compare(IDateTime t1, IDateTime t2)
		{
			throw new NotImplementedException();
		}

		public int CompareTo(IDateTime value)
		{
			throw new NotImplementedException();
		}

		public int CompareTo(object value)
		{
			throw new NotImplementedException();
		}

		public int DaysInMonth(int year, int month)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IDateTime value)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IDateTime t1, IDateTime t2)
		{
			throw new NotImplementedException();
		}

		public IDateTime FromBinary(long dateData)
		{
			throw new NotImplementedException();
		}

		public IDateTime FromFileTime(long fileTime)
		{
			throw new NotImplementedException();
		}

		public IDateTime FromFileTimeUtc(long fileTime)
		{
			throw new NotImplementedException();
		}

		public IDateTime FromOADate(double d)
		{
			throw new NotImplementedException();
		}

		public string[] GetDateTimeFormats()
		{
			throw new NotImplementedException();
		}

		public string[] GetDateTimeFormats(char format)
		{
			throw new NotImplementedException();
		}

		public string[] GetDateTimeFormats(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string[] GetDateTimeFormats(char format, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public TypeCode GetTypeCode()
		{
			throw new NotImplementedException();
		}

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public void Initialize(DateTime dateTime)
		{
			throw new NotImplementedException();
		}

		public void Initialize(long ticks)
		{
			throw new NotImplementedException();
		}

		public void Initialize(long ticks, DateTimeKind kind)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, Calendar calendar)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, Calendar calendar)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, int millisecond)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
		{
			throw new NotImplementedException();
		}

		public void Initialize(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, DateTimeKind kind)
		{
			throw new NotImplementedException();
		}

		public bool IsDaylightSavingTime()
		{
			throw new NotImplementedException();
		}

		public bool IsLeapYear(int year)
		{
			throw new NotImplementedException();
		}

		public IDateTime Parse(string s)
		{
			throw new NotImplementedException();
		}

		public IDateTime Parse(string s, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public IDateTime Parse(string s, IFormatProvider provider, DateTimeStyles styles)
		{
			throw new NotImplementedException();
		}

		public IDateTime ParseExact(string s, string format, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public IDateTime ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style)
		{
			throw new NotImplementedException();
		}

		public IDateTime ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style)
		{
			throw new NotImplementedException();
		}

		public IDateTime SpecifyKind(IDateTime value, DateTimeKind kind)
		{
			throw new NotImplementedException();
		}

		public TimeSpan Subtract(IDateTime value)
		{
			throw new NotImplementedException();
		}

		public IDateTime Subtract(TimeSpan value)
		{
			throw new NotImplementedException();
		}

		public long ToBinary()
		{
			throw new NotImplementedException();
		}

		public long ToFileTime()
		{
			throw new NotImplementedException();
		}

		public long ToFileTimeUtc()
		{
			throw new NotImplementedException();
		}

		public IDateTime ToLocalTime()
		{
			throw new NotImplementedException();
		}

		public string ToLongDateString()
		{
			throw new NotImplementedException();
		}

		public string ToLongTimeString()
		{
			throw new NotImplementedException();
		}

		public double ToOADate()
		{
			throw new NotImplementedException();
		}

		public string ToShortDateString()
		{
			throw new NotImplementedException();
		}

		public string ToShortTimeString()
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(string format)
		{
			throw new NotImplementedException();
		}

		public string ToString(string format, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public IDateTime ToUniversalTime()
		{
			throw new NotImplementedException();
		}

		public bool TryParse(string s, out IDateTime result)
		{
			throw new NotImplementedException();
		}

		public bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out IDateTime result)
		{
			throw new NotImplementedException();
		}

		public bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out IDateTime result)
		{
			throw new NotImplementedException();
		}

		public bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out IDateTime result)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
