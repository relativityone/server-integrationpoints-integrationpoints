using System;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ICancellationManager : IDisposable
	{
		/// <summary>
		/// Gets whether cancellation has been requested for this job.
		/// </summary>
		/// <returns>true if cancellation has been requested for this job; otherwise, false.</returns>
		bool IsCancellationRequested();
	}
}