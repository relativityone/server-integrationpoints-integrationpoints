﻿using System;
using kCura.Apps.Common.Utils.Serializers;

namespace kCura.ScheduleQueue.Core.ScheduleRules
{
	public class DefaultScheduleRuleFactory : IScheduleRuleFactory
	{
		public IScheduleRule Deserialize(Job job)
		{
			string scheduleRuleType = job.ScheduleRuleType;
			string serializedString = job.SerializedScheduleRule;
			IScheduleRule rule;

			if (string.IsNullOrEmpty(serializedString))
				return null;

			ISerializer defaultSerializer = new XMLSerializerFactory();

			//Periodic Schedule Rule
			if (scheduleRuleType.StartsWith("kCura.ScheduleQueue.Core.ScheduleRules.PeriodicScheduleRule,",
				StringComparison.InvariantCultureIgnoreCase))
			{
				rule = Deserialize<PeriodicScheduleRule>(serializedString, defaultSerializer);
				if (rule != null)
					return rule;
			}

			//Try reflection
			rule = DeserializeWithNoType(job);
			if (rule != null)
				return rule;

			return null;
		}

		public IScheduleRule DeserializeWithNoType(Job job)
		{
			IScheduleRule rule;
			try
			{
				rule = (IScheduleRule) SerializerHelper.DeserializeUsingTypeName(AppDomain.CurrentDomain, job.ScheduleRuleType, job.SerializedScheduleRule);
			}
			catch (Exception)
			{
				rule = null;
			}
			return rule;
		}

		public IScheduleRule Deserialize<T>(string serializedString, ISerializer serializer)
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