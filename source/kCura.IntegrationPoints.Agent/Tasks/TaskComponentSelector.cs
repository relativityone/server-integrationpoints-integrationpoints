using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class TaskComponentSelector : DefaultTypedFactoryComponentSelector
	{
		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("CreateTask"))
			{
				var job = arguments[0] as Job;
				TaskType taskType = TaskType.None;
				TaskType.TryParse(job.TaskType, true, out taskType);
				switch (taskType)
				{
					case TaskType.SyncManager:
						return typeof(SyncManager);
					case TaskType.SyncWorker:
						return typeof(SyncWorker);
					case TaskType.SyncCustodianManagerWorker:
						return typeof(SyncCustodianManagerWorker);
					default:
						return null;
				}
			}
			return base.GetComponentType(method, arguments);
		}
	}
}
