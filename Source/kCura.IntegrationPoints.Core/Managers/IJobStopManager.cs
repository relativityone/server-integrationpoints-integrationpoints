using System;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobStopManager : IDisposable
	{
		/// <summary>
		///     Gets an object that can be used to synchronize status check
		/// </summary>
		object SyncRoot { get; }

		/// <summary>
		///     Gets whether stopping has been requested for this job.
		/// </summary>
		/// <returns>true if stopping has been requested for this job; otherwise, false.</returns>
		bool IsStopRequested();

		/// <summary>
		///     Throws an <see cref="OperationCanceledException" /> if the task has been stopped.
		/// </summary>
		void ThrowIfStopRequested();

		/// <summary>
		///     Rises when stopping has been requested for this job.
		/// </summary>
		event EventHandler<EventArgs> StopRequestedEvent;
	}
}