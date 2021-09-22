using System;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Context
{
	public interface IJobContextProvider
	{
		IDisposable StartJobContext(Job job);

		Job Job { get; }
	}
}
