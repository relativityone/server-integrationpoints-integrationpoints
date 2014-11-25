using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace kCura.ScheduleQueueAgent.Helpers
{
	public class QueueTableHelper
	{
		public string GetQueueTableName()
		{
			string tableName = string.Empty;
			object tableAttribute = null;

			List<Type> agentTypes = AppDomain.CurrentDomain.GetAssemblies()
											 .SelectMany(assembly => assembly.GetTypes())
											 .Where(type => type.IsSubclassOf(typeof(kCura.ScheduleQueueAgent.ScheduleQueueAgentBase))).ToList();
			Object[] attributeObjects = agentTypes.SelectMany(t => t.GetCustomAttributes(true)).ToArray();
			return GetQueueTableName(attributeObjects);
		}

		public string GetQueueTableName(Object[] attributeObjects)
		{
			string tableName = string.Empty;
			object tableAttribute = null;

			if (attributeObjects != null && attributeObjects.Any())
			{
				tableAttribute = attributeObjects.FirstOrDefault(x => x.GetType() == typeof(GuidAttribute));
				if (tableAttribute != null)
				{
					tableName = ((GuidAttribute)tableAttribute).Value;
					if (!string.IsNullOrEmpty(tableName)) tableName = string.Format("ScheduleAgentQueue_{0}", tableName.ToUpper());
				}
			}
			if (string.IsNullOrEmpty(tableName)) throw new Exception("Could not retrieve Queue table name.");
			return tableName;
		}

		public Guid GetAgentGuid()
		{
			string tableName = string.Empty;
			object tableAttribute = null;

			List<Type> agentTypes = AppDomain.CurrentDomain.GetAssemblies()
											 .SelectMany(assembly => assembly.GetTypes())
											 .Where(type => type.IsSubclassOf(typeof(kCura.ScheduleQueueAgent.ScheduleQueueAgentBase))).ToList();
			Object[] attributeObjects = agentTypes.SelectMany(t => t.GetCustomAttributes(true)).ToArray();
			return GetAgentGuid(attributeObjects);
		}

		public Guid GetAgentGuid(Object[] attributeObjects)
		{
			Guid agentGuid = Guid.Empty;
			object tableAttribute = null;

			if (attributeObjects != null && attributeObjects.Any())
			{
				tableAttribute = attributeObjects.FirstOrDefault(x => x.GetType() == typeof(GuidAttribute));
				if (tableAttribute != null)
				{
					string possibleGuid = ((GuidAttribute)tableAttribute).Value;
					if (!string.IsNullOrEmpty(possibleGuid)) agentGuid = Guid.Parse(possibleGuid);
				}
			}
			if (agentGuid == Guid.Empty) throw new Exception("Could not retrieve Agent Guid.");
			return agentGuid;
		}
	}
}
