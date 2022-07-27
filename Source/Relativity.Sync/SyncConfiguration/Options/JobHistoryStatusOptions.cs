using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Configuration class for RDO representing JobHistory status
    /// </summary>
    public class JobHistoryStatusOptions
    {
        /// <summary>
        /// Completed status
        /// </summary>
        public Guid CompletedGuid { get; }

        /// <summary>
        /// Completed with errors status
        /// </summary>
        public Guid CompletedWithErrorsGuid { get; }

        /// <summary>
        /// Failed status
        /// </summary>
        public Guid JobFailedGuid { get; }

        /// <summary>
        /// Processing status
        /// </summary>
        public Guid ProcessingGuid { get; }

        /// <summary>
        /// Stopped status
        /// </summary>
        public Guid StoppedGuid { get; }

        /// <summary>
        /// Stopping status
        /// </summary>
        public Guid StoppingGuid { get; }

        /// <summary>
        /// Suspended status
        /// </summary>
        public Guid SuspendedGuid { get; }

        /// <summary>
        /// Suspending status
        /// </summary>
        public Guid SuspendingGuid { get; }

        /// <summary>
        /// Validating status
        /// </summary>
        public Guid ValidatingGuid { get; }

        /// <summary>
        /// Validation failed status
        /// </summary>
        public Guid ValidationFailedGuid { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public JobHistoryStatusOptions(Guid validatingGuid, Guid validationFailedGuid, Guid processingGuid, Guid completedGuid, Guid completedWithErrorsGuid,
            Guid jobFailedGuid, Guid stoppingGuid, Guid stoppedGuid, Guid suspendingGuid, Guid suspendedGuid)
        {
            ValidatingGuid = validatingGuid;
            ValidationFailedGuid = validationFailedGuid;
            ProcessingGuid = processingGuid;
            CompletedGuid = completedGuid;
            CompletedWithErrorsGuid = completedWithErrorsGuid;
            JobFailedGuid = jobFailedGuid;
            StoppingGuid = stoppingGuid;
            StoppedGuid = stoppedGuid;
            SuspendingGuid = suspendingGuid;
            SuspendedGuid = suspendedGuid;
        }
    }
}
