using System;

namespace kCura.ScheduleQueue.Core.Core
{
	[Flags]
	public enum StopState
	{
		None = 0,
		Stopping = 1,
		Unstoppable = 1 << 1
	}
}