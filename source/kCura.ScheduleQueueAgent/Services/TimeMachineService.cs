using System;
using System.Collections;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Utils.Serializers;
using kCura.ScheduleQueueAgent.Helpers;
using kCura.ScheduleQueueAgent.ScheduleRules;
using kCura.ScheduleQueueAgent.TimeMachine;

namespace kCura.ScheduleQueueAgent.Services
{
	public class TimeMachineService : ITimeService
	{
		private int _workspaceID;
		public TimeMachineService(int workspaceID)
		{
			_workspaceID = workspaceID;
		}
		
		DateTime ITimeService.UtcNow
		{
			get
			{
				DateTime returnValue = DateTime.UtcNow;
				if (AgentTimeMachineProvider.Current.Enabled)
				{
					if (AgentTimeMachineProvider.Current.WorkspaceID > 0)
					{
						if (AgentTimeMachineProvider.Current.WorkspaceID.Equals(_workspaceID))
						{
							returnValue = AgentTimeMachineProvider.Current.UtcNow;
						}
					}
					else
					{
						returnValue = AgentTimeMachineProvider.Current.UtcNow;
					}
				}
				return returnValue;
			}
		}
	}
}