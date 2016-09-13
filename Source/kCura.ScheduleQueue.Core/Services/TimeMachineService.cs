using System;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.TimeMachine;

namespace kCura.ScheduleQueue.Core.Services
{
	public class TimeMachineService : ITimeService
	{
		private readonly int _workspaceId;

		public TimeMachineService(int workspaceId)
		{
			_workspaceId = workspaceId;
		}

		public DateTime UtcNow
		{
			get
			{
				DateTime returnValue = DateTime.UtcNow;
				if (AgentTimeMachineProvider.Current.Enabled)
				{
					if (AgentTimeMachineProvider.Current.WorkspaceID > 0)
					{
						if (AgentTimeMachineProvider.Current.WorkspaceID.Equals(_workspaceId))
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

		public DateTime LocalTime => UtcNow.ToLocalTime();
	}
}