﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace kCura.ScheduleQueueAgent.Helpers
{
	public class QueueTableHelper
	{
		public string GetQueueTableName()
		{
			List<Type> agentTypes = System.AppDomain.CurrentDomain.GetAssemblies()
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
			Object[] attributeObjects = null;
			try
			{
				AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
				AppDomain appDomain = System.AppDomain.CurrentDomain;
				List<Type> agentTypes = appDomain.GetAssemblies()
					.SelectMany(assembly => assembly.GetTypes()).ToList();
				agentTypes = agentTypes.Where(type => type.IsSubclassOf(typeof(kCura.ScheduleQueueAgent.ScheduleQueueAgentBase))).ToList();
				attributeObjects = agentTypes.SelectMany(t => t.GetCustomAttributes(true)).ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return GetAgentGuid(attributeObjects);
		}

		static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			Assembly assembly = null;

			string[] assemblyString = args.Name.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (assemblyString.Length > 1)
			{
				Assembly[] loadedAssemblies = Thread.GetDomain().GetAssemblies();
				for (int i = 0; i < loadedAssemblies.Length; i++)
				{
					if (String.Compare(loadedAssemblies[i].GetName().Name, assemblyString.First()) == 0)
					{
						assembly = loadedAssemblies[i];
						break;
					}
				}

				if (assembly == null)
				{
					try { assembly = Assembly.Load(assemblyString.First()); }
					catch
					{ }
				}
			}
			return assembly;
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
