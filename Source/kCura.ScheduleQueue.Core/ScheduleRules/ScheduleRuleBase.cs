using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    [Serializable]
    public abstract class ScheduleRuleBase : IScheduleRule
    {
        [NonSerialized()]
        private static Dictionary<DaysOfWeek, DayOfWeek> _daysOfWeekMap = null;

        [NonSerialized()]
        private static ISerializer _serializer;

        // Required by Serializable
        protected ScheduleRuleBase()
        {
        }

        [XmlIgnore]
        public static ISerializer Serializer
        {
            set { _serializer = value; }
            get { return _serializer ?? (_serializer = new XMLSerializerFactory()); }
        }

        [XmlIgnore]
        public static Dictionary<DaysOfWeek, DayOfWeek> DaysOfWeekMap
        {
            get
            {
                return _daysOfWeekMap ?? (_daysOfWeekMap = new Dictionary<DaysOfWeek, DayOfWeek>
                {
                    {DaysOfWeek.Monday, DayOfWeek.Monday },
                    {DaysOfWeek.Tuesday, DayOfWeek.Tuesday },
                    {DaysOfWeek.Wednesday, DayOfWeek.Wednesday },
                    {DaysOfWeek.Thursday, DayOfWeek.Thursday },
                    {DaysOfWeek.Friday, DayOfWeek.Friday },
                    {DaysOfWeek.Saturday, DayOfWeek.Saturday },
                    {DaysOfWeek.Sunday, DayOfWeek.Sunday },
                });
            }

            set
            {
                _daysOfWeekMap = value;
            }
        }

        public abstract DateTime? GetNextUtcRunDateTime(DateTime lastNextRunTimeUtc);

        public abstract DateTime? GetFirstUtcRunDateTime();

        public abstract int GetNumberOfContinuouslyFailedScheduledJobs();

        public abstract void IncrementConsecutiveFailedScheduledJobsCount();

        public abstract void ResetConsecutiveFailedScheduledJobsCount();

        public string ToSerializedString()
        {
            return Serializer.Serialize(this);
        }

        public static T Deserialize<T>(string serializedString)
        {
            return Serializer.Deserialize<T>(serializedString);
        }

        public static string DaysOfWeekToString(DaysOfWeek daysOfWeek)
        {
            string weekDays = string.Empty;
            DaysOfWeek nextDayOfWeek = (DaysOfWeek)1;
            for (int i = 0; i < 8; i++)
            {
                if ((daysOfWeek & nextDayOfWeek) == nextDayOfWeek)
                {
                    weekDays += Enum.GetName(typeof(DaysOfWeek), nextDayOfWeek) + ", ";
                }
                nextDayOfWeek = (DaysOfWeek)((byte)nextDayOfWeek << 1);
                if ((nextDayOfWeek & DaysOfWeek.All) == DaysOfWeek.None)
                {
                    break;
                }
            }
            if (!string.IsNullOrEmpty(weekDays))
            {
                weekDays = weekDays.Substring(0, weekDays.Length - 2);
            }

            return weekDays;
        }
    }
}
