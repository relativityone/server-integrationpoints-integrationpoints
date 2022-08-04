using System;

namespace kCura.IntegrationPoints.Domain.Managers
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
        /// Indicates whether the job should be drain stopped (i.e. the Agent is marked to be removed and the current job supports drain stop)
        /// </summary>
        bool ShouldDrainStop { get; }

        /// <summary>
        ///     Throws an <see cref="OperationCanceledException" /> if the task has been stopped.
        /// </summary>
        void ThrowIfStopRequested();

        /// <summary>
        ///     Stop checking if DrainStop was invoked.
        /// </summary>
        void StopCheckingDrainStop();

        /// <summary>
        ///     Rises when stopping has been requested for this job.
        /// </summary>
        event EventHandler<EventArgs> StopRequestedEvent;

        /// <summary>
        ///     Cleans up Job Drain Stop by setting StopState to None
        /// </summary>
        void CleanUpJobDrainStop();
    }
}