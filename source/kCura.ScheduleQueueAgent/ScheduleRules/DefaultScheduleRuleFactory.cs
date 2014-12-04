using System;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.ScheduleQueueAgent.ScheduleRules
{
	public class DefaultScheduleRuleFactory : IScheduleRuleFactory
	{
		public IScheduleRule Deserialize(Job job)
		{
			string scheduleRuleType = job.ScheduleRuleType;
			string serializedString = job.SerializedScheduleRule;
			IScheduleRule rule = null;

			if (string.IsNullOrEmpty(serializedString)) return null;

			ISerializer defaultSerializer = new XMLSerializerFactory();

			//Periodic Schedule Rule
			if (scheduleRuleType.StartsWith("kCura.ScheduleQueueAgent.ScheduleRules.PeriodicScheduleRule,",
				StringComparison.InvariantCultureIgnoreCase))
			{
				rule = Deserialize<kCura.ScheduleQueueAgent.ScheduleRules.PeriodicScheduleRule>(serializedString, defaultSerializer);
				if (rule != null) return rule;
			}

			//Try reflection
			rule = DeserializeWithNoType(job);
			if (rule != null) return rule;

			return null;
		}

		public IScheduleRule DeserializeWithNoType(Job job)
		{
			IScheduleRule rule = null;
			try
			{
				rule = (IScheduleRule)SerializerHelper.DeserializeUsingTypeName(System.AppDomain.CurrentDomain, job.ScheduleRuleType, job.SerializedScheduleRule);
			}
			catch (Exception)
			{
				rule = null;
			}
			return rule;
		}

		public IScheduleRule Deserialize<T>(string serializedString, ISerializer serializer)
		{
			IScheduleRule rule = null;
			try
			{
				rule = (IScheduleRule)serializer.Deserialize<T>(serializedString);
			}
			catch (Exception)
			{
				rule = null;
			}
			return rule;
		}
	}
}
