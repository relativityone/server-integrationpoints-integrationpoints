using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.TypedFactory;
using kCura.IntegrationPoints.Core.Services;
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
						return typeof (SyncManager);
					case TaskType.SyncWorker:
						return typeof (SyncWorker);
					default: 
						return null;
				}
			}
			return base.GetComponentType(method, arguments);
		}
	}
}
