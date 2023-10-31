using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
    public class DefaultScheduleRuleFactory : IScheduleRuleFactory
    {
        public IScheduleRule Deserialize(Job job)
        {
            string scheduleRuleType = job.ScheduleRuleType;
            string serializedString = job.ScheduleRule;
            IScheduleRule rule;

            if (string.IsNullOrEmpty(serializedString))
            {
                return null;
            }

            ISerializer defaultSerializer = new XMLSerializerFactory();

            // Periodic Schedule Rule
            if (!string.IsNullOrEmpty(scheduleRuleType) && scheduleRuleType.StartsWith(Const._PERIODIC_SCHEDULE_RULE_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                rule = Deserialize<PeriodicScheduleRule>(serializedString, defaultSerializer);
                if (rule != null)
                {
                    return rule;
                }
            }

            // Try reflection
            rule = DeserializeWithNoType(job);
            if (rule != null)
            {
                return rule;
            }

            return null;
        }

        private IScheduleRule DeserializeWithNoType(Job job)
        {
            IScheduleRule rule;
            try
            {
                rule = (IScheduleRule)SerializerHelper.DeserializeUsingTypeName(AppDomain.CurrentDomain, job.ScheduleRuleType, job.ScheduleRule);
            }
            catch (Exception)
            {
                rule = null;
            }
            return rule;
        }

        private IScheduleRule Deserialize<T>(string serializedString, ISerializer serializer)
        {
            IScheduleRule rule;
            try
            {
                rule = (IScheduleRule) serializer.Deserialize<T>(serializedString);
            }
            catch (Exception)
            {
                rule = null;
            }
            return rule;
        }
    }
}
