using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kCura.Agent.ScheduleQueueAgent.Helpers
{
	public class QueueTableHelper
	{
		public string GetQueueTableName()
		{
			string tableName = string.Empty;
			object tableAttribute = null;

			List<Type> agentTypes = AppDomain.CurrentDomain.GetAssemblies()
											 .SelectMany(assembly => assembly.GetTypes())
											 .Where(type => type.IsSubclassOf(typeof(kCura.Agent.AgentBase))).ToList();
			Object[] attributeObjects = agentTypes.SelectMany(t => t.GetCustomAttributes(true)).ToArray();
			return GetQueueTableName(attributeObjects);
		}

		public string GetQueueTableName(Object[] attributeObjects)
		{
			string tableName = string.Empty;
			object tableAttribute = null;

			if (attributeObjects != null && attributeObjects.Any())
			{
				tableAttribute = attributeObjects.FirstOrDefault(x => x.GetType() == typeof(CustomAttributes.QueueTableAttribute));
				if (tableAttribute != null)
				{
					tableName = ((CustomAttributes.QueueTableAttribute)tableAttribute).Name;
				}

				if (string.IsNullOrEmpty(tableName))
				{
					tableAttribute = attributeObjects.FirstOrDefault(x => x.GetType() == typeof(GuidAttribute));
					if (tableAttribute != null)
					{
						tableName = ((GuidAttribute)tableAttribute).Value;
						if (!string.IsNullOrEmpty(tableName)) tableName = string.Format("AgentQueue_{0}", tableName);
					}
				}
			}
			if (string.IsNullOrEmpty(tableName)) tableName = "ScheduleAgentQueue";
			return tableName;
		}
	}
}
