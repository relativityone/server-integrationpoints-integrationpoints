﻿namespace kCura.ScheduleQueue.Core
{
	public interface ITask
	{
		void Execute(Job job);
	}
}
