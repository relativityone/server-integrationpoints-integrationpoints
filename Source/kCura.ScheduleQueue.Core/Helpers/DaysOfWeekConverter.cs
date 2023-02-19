using System;
using System.Collections.Generic;
using System.Linq;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.ScheduleQueue.Core.Helpers
{
    public static class DaysOfWeekConverter
    {
        public static DaysOfWeek FromDayOfWeek(List<DayOfWeek> days)
        {
            var map = ScheduleRuleBase.DaysOfWeekMap.ToDictionary(x => x.Value, x => x.Key);
            return days.Aggregate(DaysOfWeek.None, (current, dayOfWeek) => current | map[dayOfWeek]);
        }

        public static List<DayOfWeek> FromDaysOfWeek(DaysOfWeek days)
        {
            var map = ScheduleRuleBase.DaysOfWeekMap;
            if (days == DaysOfWeek.None)
            {
                return new List<DayOfWeek>();
            }
            var values = (DaysOfWeek[])Enum.GetValues(typeof(DaysOfWeek));
            return (from value in values where (days & value) == value && map.ContainsKey(value) select map[value]).ToList();
        }
    }
}
