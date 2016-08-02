using System;
using System.Threading;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobStopManager : IDisposable
	{
		/// <summary>
		/// Gets whether stopping has been requested for this job.
		/// </summary>
		/// <returns>true if stopping has been requested for this job; otherwise, false.</returns>
		bool IsStoppingRequested();

		/// <summary>
		/// Gets an object that can be used to synchronize status check
		/// </summary>
		Object SyncRoot { get; }
	}
}