using System;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public enum ScheduleInterval
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    [Flags]
    public enum DaysOfWeek
    {
        None = 0,
        Monday = 1 << 0,
        Tuesday = 1 << 1,
        Wednesday = 1 << 2,
        Thursday = 1 << 3,
        Friday = 1 << 4,
        Saturday = 1 << 5,
        Sunday = 1 << 6,
        All = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
        Day = 1 << 7
    }

    public enum OccuranceInMonth
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Last = 5
    }

    public enum ForwardValidOccurance
    {
        First = OccuranceInMonth.First,
        Second = OccuranceInMonth.Second,
        Third = OccuranceInMonth.Third,
        Fourth = OccuranceInMonth.Fourth
    }

    public enum MonthlyType
    {
        Month = 1,
        Days = 2
    }
}
