using kCura.ScheduleQueue.Core;
using System;

namespace kCura.IntegrationPoints.Agent.Context
{
	public interface IJobContextProvider
	{
		IDisposable StartJobContext(Job job);

		Job Job { get; }
	}
}
